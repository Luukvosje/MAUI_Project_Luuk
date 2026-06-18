using SQLite;
using TimeOn.Domain.Entities;
using TimeOn.Maui.Features.Tracking.Persistence;

namespace TimeOn.Maui.Features.Tracking.Services;

public sealed class SqliteTrackingStore : ITrackingGpsStore
{
    private const string DatabaseFileName = "timeon-tracking-v2.db3";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _database;
    private bool _initialized;

    public Task InitializeAsync() => EnsureInitializedAsync();

    public async Task<ActiveTrackingSession?> GetActiveSessionAsync(Guid userId)
    {
        var db = await EnsureInitializedAsync();
        var userIdText = userId.ToString();

        var rows = await db.QueryAsync<ActiveTrackingSessionDbRow>(
            "SELECT Id, UserId, StartTimeUtc FROM ActiveTrackingSessions WHERE UserId = ? LIMIT 1",
            userIdText);

        var row = rows.FirstOrDefault();
        return row is null
            ? null
            : new ActiveTrackingSession(
                Guid.Parse(row.Id),
                Guid.Parse(row.UserId),
                FromTicks(row.StartTimeUtc));
    }

    public async Task SaveActiveSessionAsync(ActiveTrackingSession session)
    {
        var db = await EnsureInitializedAsync();
        await db.ExecuteAsync(
            """
            INSERT OR REPLACE INTO ActiveTrackingSessions (Id, UserId, StartTimeUtc)
            VALUES (?, ?, ?)
            """,
            session.Id.ToString(),
            session.UserId.ToString(),
            session.StartTimeUtc.Ticks);
    }

    public async Task ClearActiveSessionAsync(Guid userId)
    {
        var db = await EnsureInitializedAsync();
        await db.ExecuteAsync(
            "DELETE FROM ActiveTrackingSessions WHERE UserId = ?",
            userId.ToString());
    }

    public async Task AddPointAsync(Guid workSessionId, GpsPoint point)
    {
        var db = await EnsureInitializedAsync();
        await db.ExecuteAsync(
            """
            INSERT INTO TrackedGpsSamples (WorkSessionId, Latitude, Longitude, RecordedAtUtc)
            VALUES (?, ?, ?, ?)
            """,
            workSessionId.ToString(),
            point.Location.Latitude,
            point.Location.Longitude,
            point.RecordedAtUtc.Ticks);
    }

    public async Task<GpsPoint?> GetLastPointAsync(Guid workSessionId)
    {
        var db = await EnsureInitializedAsync();
        var samples = await db.QueryAsync<TrackedGpsSampleRow>(
            """
            SELECT Latitude, Longitude, RecordedAtUtc
            FROM TrackedGpsSamples
            WHERE WorkSessionId = ?
            ORDER BY RecordedAtUtc DESC
            LIMIT 1
            """,
            workSessionId.ToString());

        var sample = samples.FirstOrDefault();
        return sample is null
            ? null
            : GpsPoint.Create(sample.Latitude, sample.Longitude, FromTicks(sample.RecordedAtUtc));
    }

    public async Task<IReadOnlyList<GpsPoint>> GetPointsAsync(Guid workSessionId)
    {
        var db = await EnsureInitializedAsync();
        var samples = await db.QueryAsync<TrackedGpsSampleRow>(
            """
            SELECT Latitude, Longitude, RecordedAtUtc
            FROM TrackedGpsSamples
            WHERE WorkSessionId = ?
            ORDER BY RecordedAtUtc ASC
            """,
            workSessionId.ToString());

        return samples
            .Select(sample => GpsPoint.Create(
                sample.Latitude,
                sample.Longitude,
                FromTicks(sample.RecordedAtUtc)))
            .ToList();
    }

    public async Task DeletePointsAsync(Guid workSessionId)
    {
        var db = await EnsureInitializedAsync();
        await db.ExecuteAsync(
            "DELETE FROM TrackedGpsSamples WHERE WorkSessionId = ?",
            workSessionId.ToString());
    }

    private async Task<SQLiteAsyncConnection> EnsureInitializedAsync()
    {
        if (_initialized && _database is not null)
        {
            return _database;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_initialized && _database is not null)
            {
                return _database;
            }

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
            var connection = new SQLiteAsyncConnection(
                dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

            await EnsureSchemaAsync(connection);
            _database = connection;
            _initialized = true;
            return _database;
        }
        catch
        {
            _database = null;
            _initialized = false;
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task EnsureSchemaAsync(SQLiteAsyncConnection db)
    {
        await RecreateActiveTrackingSessionsIfInvalidAsync(db);
        await RecreateTrackedGpsSamplesIfInvalidAsync(db);
    }

    private static async Task RecreateActiveTrackingSessionsIfInvalidAsync(SQLiteAsyncConnection db)
    {
        if (await TableExistsAsync(db, "ActiveTrackingSessions") &&
            await ColumnExistsAsync(db, "ActiveTrackingSessions", "UserId") &&
            await ColumnExistsAsync(db, "ActiveTrackingSessions", "StartTimeUtc"))
        {
            return;
        }

        await db.ExecuteAsync("DROP TABLE IF EXISTS ActiveTrackingSessions;");
        await db.ExecuteAsync(
            """
            CREATE TABLE ActiveTrackingSessions (
                Id TEXT NOT NULL PRIMARY KEY,
                UserId TEXT NOT NULL,
                StartTimeUtc INTEGER NOT NULL
            )
            """);
        await db.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS IX_ActiveTrackingSessions_UserId ON ActiveTrackingSessions (UserId)");
    }

    private static async Task RecreateTrackedGpsSamplesIfInvalidAsync(SQLiteAsyncConnection db)
    {
        if (await TableExistsAsync(db, "TrackedGpsSamples") &&
            await ColumnExistsAsync(db, "TrackedGpsSamples", "RecordedAtUtc"))
        {
            return;
        }

        await db.ExecuteAsync("DROP TABLE IF EXISTS TrackedGpsSamples;");
        await db.ExecuteAsync(
            """
            CREATE TABLE TrackedGpsSamples (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                WorkSessionId TEXT NOT NULL,
                Latitude REAL NOT NULL,
                Longitude REAL NOT NULL,
                RecordedAtUtc INTEGER NOT NULL
            )
            """);
        await db.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS IX_TrackedGpsSamples_WorkSessionId ON TrackedGpsSamples (WorkSessionId)");
    }


    private static async Task<bool> TableExistsAsync(SQLiteAsyncConnection db, string tableName)
    {
        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = ?",
            tableName);
        return count > 0;
    }

    private static async Task<bool> ColumnExistsAsync(
        SQLiteAsyncConnection db,
        string tableName,
        string columnName)
    {
        var columns = await db.QueryAsync<TableColumnInfo>(
            "PRAGMA table_info(" + tableName + ")");
        return columns.Any(column =>
            string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase));
    }

    private static DateTime FromTicks(long ticks) =>
        new DateTime(ticks, DateTimeKind.Utc);

    private sealed class ActiveTrackingSessionDbRow
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public long StartTimeUtc { get; set; }
    }

    private sealed class TrackedGpsSampleRow
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long RecordedAtUtc { get; set; }
    }

    private sealed class TableColumnInfo
    {
        public string Name { get; set; } = string.Empty;
    }
}
