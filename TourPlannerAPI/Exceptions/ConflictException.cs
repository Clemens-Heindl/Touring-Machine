namespace TourPlannerAPI.Exceptions;

/// <summary>Raised on a state conflict, e.g. registering a duplicate email. Maps to HTTP 409.</summary>
public class ConflictException : TourPlannerException
{
    public ConflictException(string message) : base(message) { }
}
