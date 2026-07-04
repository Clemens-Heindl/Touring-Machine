namespace TourPlannerAPI.Exceptions;

/// <summary>
/// Raised when an authenticated user tries to access a resource they do not
/// own. Maps to HTTP 403. (Tours and logs belong to a single user; nothing is
/// shared between users.)
/// </summary>
public class ForbiddenException : TourPlannerException
{
    public ForbiddenException(string message) : base(message) { }
}
