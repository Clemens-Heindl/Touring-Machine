namespace TourPlannerAPI.Exceptions;

/// <summary>Raised when domain/business validation fails. Maps to HTTP 400.</summary>
public class ValidationException : TourPlannerException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new[] { message };
    }

    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }
}
