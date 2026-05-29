using System.Text.RegularExpressions;
using TimeOn.Domain.Constants;
using TimeOn.Domain.Exceptions;

namespace TimeOn.Domain.ValueObjects;

public sealed partial class Email
{
    private static readonly Regex EmailFormat = EmailRegex();

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email is required.");
        }

        var normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > AuthConstants.MaxEmailLength)
        {
            throw new DomainException($"Email cannot exceed {AuthConstants.MaxEmailLength} characters.");
        }

        if (!EmailFormat.IsMatch(normalized))
        {
            throw new DomainException("Email format is invalid.");
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
