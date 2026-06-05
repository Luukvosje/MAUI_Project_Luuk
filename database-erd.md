# TimeOn Database ERD

Entity-relationship diagram gebaseerd op `src/TimeOn.Infrastructure` (EF Core) en de domeinentiteiten in `src/TimeOn.Domain`.

## Overzicht

De oplossing gebruikt **twee opslaglagen**:


| Opslag                | Provider   | Locatie                           | Doel                                                                                |
| --------------------- | ---------- | --------------------------------- | ----------------------------------------------------------------------------------- |
| `AppDbContext`        | SQL Server | API (`TimeOn.Api`)                | Brondata: gebruikers, klanten, afgeronde werksessies met geclassificeerde segmenten |
| `SqliteTrackingStore` | SQLite     | Mobile (`timeon-tracking-v2.db3`) | Offline ruwe GPS-metingen en actieve sessiestatus tijdens tracking                  |


Ruwe GPS-punten (`GpsPoint`) worden **niet** opgeslagen in SQL Server. Ze worden op het apparaat verzameld, bij afronden naar de API gestuurd, server-side geclassificeerd en alleen bewaard als samenvattingen in `DrivingSegment` / `StationarySegment`.

## API-database (`AppDbContext`)

```mermaid
erDiagram
    Users ||--o{ WorkSessions : "owns (logical UserId)"
    Users ||--o{ Customers : "owns (logical UserId)"
    WorkSessions ||--o{ DrivingSegments : "has many (cascade delete)"
    WorkSessions ||--o{ StationarySegments : "has many (cascade delete)"
    Customers ||--o{ StationarySegments : "optional visit link (logical CustomerId)"

    Users {
        uuid Id PK
        string Name "max 200, required"
        string Email "owned, unique index"
        string PasswordHash "owned, required"
    }

    WorkSessions {
        uuid Id PK
        uuid UserId "required, logical FK to Users"
        datetime StartTimeUtc "required"
        datetime EndTimeUtc "nullable"
        float TotalDistanceKm "precision 12,3"
        string Status "enum as string, max 32 (Active|Stopped|Cancelled)"
    }

    DrivingSegments {
        uuid Id PK
        uuid WorkSessionId FK "required"
        datetime StartUtc "required"
        datetime EndUtc "required"
        float DistanceMeters "required"
    }

    StationarySegments {
        uuid Id PK
        uuid WorkSessionId FK "required"
        uuid CustomerId "nullable, logical FK to Customers"
        datetime StartUtc "required"
        datetime EndUtc "required"
        float CenterLatitude "required"
        float CenterLongitude "required"
        float DistanceFromCustomerMeters "nullable, precision 10,2"
    }

    Customers {
        uuid Id PK
        uuid UserId "nullable, logical FK to Users"
        string Name "max 200, required"
        string Address "max 500, nullable"
        string ContactEmail "max 320, nullable"
        bool IsActive "default true"
        float Location_Latitude "owned Coordinate"
        float Location_Longitude "owned Coordinate"
    }
```



---

## Mobile tracking-database (SQLite)

Beheerd door `SqliteTrackingStore` — geen EF Core.

```mermaid
erDiagram
    ActiveTrackingSessions ||--o{ TrackedGpsSamples : "session id (string, no FK constraint)"

    ActiveTrackingSessions {
        string Id PK "GUID as text"
        string UserId "indexed"
        bigint StartTimeUtc "DateTime ticks"
    }

    TrackedGpsSamples {
        int Id PK "auto increment"
        string WorkSessionId "indexed"
        float Latitude
        float Longitude
        bigint RecordedAtUtc "DateTime ticks"
    }
```



---

## Relaties en constraints


| Van                                | Naar                        | Type | Verwijderregel | Opmerking                                                    |
| ---------------------------------- | --------------------------- | ---- | -------------- | ------------------------------------------------------------ |
| `DrivingSegments.WorkSessionId`    | `WorkSessions.Id`           | 1:N  | Cascade        | `WorkSessionConfiguration`                                   |
| `StationarySegments.WorkSessionId` | `WorkSessions.Id`           | 1:N  | Cascade        | `WorkSessionConfiguration`                                   |
| `WorkSessions.UserId`              | `Users.Id`                  | N:1  | —              | Alleen logisch; geen EF `HasForeignKey`                      |
| `StationarySegments.CustomerId`    | `Customers.Id`              | N:1  | —              | Alleen logisch; bezoek = stilstaand segment met `CustomerId` |
| `Customers.UserId`                 | `Users.Id`                  | N:1  | —              | Alleen logisch                                               |
| `TrackedGpsSamples.WorkSessionId`  | `ActiveTrackingSessions.Id` | N:1  | —              | Alleen koppeling op applicatieniveau                         |


---

## Domein vs. database


| Concept            | Domeintype          | API-tabel                        | Mobile SQLite                             |
| ------------------ | ------------------- | -------------------------------- | ----------------------------------------- |
| Werkdag / rit      | `WorkSession`       | `WorkSessions`                   | `ActiveTrackingSessions` (tijdens actief) |
| Rijperiode         | `DrivingSegment`    | `DrivingSegments`                | —                                         |
| Stop / klantbezoek | `StationarySegment` | `StationarySegments`             | —                                         |
| Ruwe GPS-meting    | `GpsPoint`          | —                                | `TrackedGpsSamples`                       |
| Klant              | `Customer`          | `Customers`                      | —                                         |
| Gebruikersaccount  | `User`              | `Users`                          | JWT + `SecureStorage` (niet in SQLite)    |
| Locatie            | `Coordinate`        | Owned-kolommen op `Customers`    | Lat/long-kolommen op samples              |
| Afstand            | `Distance`          | `DistanceMeters` op rijsegmenten | —                                         |


### Datastroom (tracking → opslag)

```mermaid
flowchart LR
    subgraph Mobile
        GPS[GPS hardware]
        Store[(SQLite TrackedGpsSamples)]
        API[POST /api/worksessions/complete]
        GPS --> Store
        Store --> API
    end

    subgraph API
        Classify[SegmentClassifier]
        WS[WorkSession + segments]
        DB[(SQL Server AppDbContext)]
        API --> Classify
        Classify --> WS
        WS --> DB
    end
```



---

## Bronbestanden

- `src/TimeOn.Infrastructure/Persistence/AppDbContext.cs`
- `src/TimeOn.Infrastructure/Persistence/TimeOnDbContextBase.cs`
- `src/TimeOn.Infrastructure/Persistence/Configurations/WorkSessionConfiguration.cs`
- `src/TimeOn.Infrastructure/Persistence/Configurations/DrivingSegmentConfiguration.cs`
- `src/TimeOn.Infrastructure/Persistence/Configurations/StationarySegmentConfiguration.cs`
- `src/TimeOn.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs`
- `src/TimeOn.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- `src/TimeOn.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `src/TimeOn.Mobile/Features/Tracking/Services/SqliteTrackingStore.cs`
- `src/TimeOn.Domain/Entities/*.cs`

