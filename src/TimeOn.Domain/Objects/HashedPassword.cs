using TimeOn.Domain.Exceptions;

namespace TimeOn.Domain.ValueObjects;

public sealed class HashedPassword
{
    public string Value { get; }

    private HashedPassword(string value) => Value = value;

    public static HashedPassword FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new DomainException("Password hash is required.");
        }

        return new HashedPassword(hash);
    }

    public override string ToString() => "[redacted]";
}
