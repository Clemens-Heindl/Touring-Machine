namespace TourPlannerAPI.Exceptions;

/// <summary>Raised when a requested entity does not exist. Maps to HTTP 404.</summary>
public class NotFoundException : TourPlannerException
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resource, object key)
        : base($"{resource} with id '{key}' was not found.") { }
}
