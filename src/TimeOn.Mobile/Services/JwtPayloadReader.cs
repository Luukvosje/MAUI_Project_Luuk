using System.Text;
using System.Text.Json;

namespace TimeOn.Mobile.Services;

internal static class JwtPayloadReader
{
    public static bool IsExpired(string token, TimeSpan clockSkew)
    {
        var expiresAt = GetExpiresAtUtc(token);
        if (expiresAt is null)
        {
            return true;
        }

        return expiresAt.Value <= DateTime.UtcNow.Add(clockSkew);
    }

    private static DateTime? GetExpiresAtUtc(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            var payloadBytes = Convert.FromBase64String(PadBase64(parts[1]));
            using var document = JsonDocument.Parse(payloadBytes);
            if (!document.RootElement.TryGetProperty("exp", out var expElement))
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64()).UtcDateTime;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string PadBase64(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        return padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
    }
}
