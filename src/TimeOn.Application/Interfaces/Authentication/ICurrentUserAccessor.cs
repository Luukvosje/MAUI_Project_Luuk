namespace TimeOn.Application.Interfaces.Authentication;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
}
