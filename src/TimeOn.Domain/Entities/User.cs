using TimeOn.Domain.Constants;
using TimeOn.Domain.Exceptions;
using TimeOn.Domain.Interfaces;
using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;

public sealed class User: Entity
{
    public Guid UserGuid;

    public string Name { get; private set; } = string.Empty;

    public Email Email { get; private set; } = null!;

    public HashedPassword Password { get; private set; } = null!;

    private User()
    {
    }

    private User(Guid userGuid, string name, Email email, HashedPassword password): base(userGuid)
    {
        Name = name;
        Email = email;
        Password = password;
    }

    public static User Register(
        string name,
        string email,
        string plainPassword,
        IPasswordHasher passwordHasher,
        Guid? userGuid = null)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        ValidateName(name);
        ValidatePlainPassword(plainPassword);

        var emailValue = Email.Create(email);

        if (userGuid == Guid.Empty)
            throw new DomainException("User id is required.");

        var id = userGuid ?? Guid.NewGuid();

        var user = new User(
            id,
            name.Trim(),
            emailValue,
            HashedPassword.FromHash(passwordHasher.Hash(plainPassword)));

        return user;
    }

    public static User Reconstitute(Guid userGuid, string name, string email, string passwordHash)
    {
        if (userGuid == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        ValidateName(name);

        return new User(
            userGuid,
            name.Trim(),
            Email.Create(email),
            HashedPassword.FromHash(passwordHash));
    }

    public Result Authenticate(string plainPassword, IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            return Result.Failure("Password is required.");
        }

        if (!passwordHasher.Verify(plainPassword, Password.Value))
        {
            return Result.Failure("Invalid email or password.");
        }

        return Result.Success();
    }

    public Result ChangePassword(string plainPassword, IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        ValidatePlainPassword(plainPassword);
        Password = HashedPassword.FromHash(passwordHasher.Hash(plainPassword));
        return Result.Success();
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name.Trim();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("User name is required.");
        }

        if (name.Trim().Length > AuthConstants.MaxUserNameLength)
        {
            throw new DomainException($"User name cannot exceed {AuthConstants.MaxUserNameLength} characters.");
        }
    }

    private static void ValidatePlainPassword(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new DomainException("Password is required.");
        }

        if (plainPassword.Length < AuthConstants.MinPasswordLength)
        {
            throw new DomainException(
                $"Password must be at least {AuthConstants.MinPasswordLength} characters.");
        }

        if (plainPassword.Length > AuthConstants.MaxPasswordLength)
        {
            throw new DomainException(
                $"Password cannot exceed {AuthConstants.MaxPasswordLength} characters.");
        }
    }
}
