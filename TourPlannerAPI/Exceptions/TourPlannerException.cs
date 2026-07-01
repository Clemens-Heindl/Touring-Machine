namespace TourPlannerAPI.Exceptions;

/// <summary>
/// Base type for all domain exceptions raised by the business layer.
/// The layers define their own exceptions so implementation-specific types
/// (EF Core, Npgsql, HTTP) never leak up to the presentation layer.
/// </summary>
public abstract class TourPlannerException : Exception
{
    protected TourPlannerException(string message) : base(message) { }

    protected TourPlannerException(string message, Exception innerException)
        : base(message, innerException) { }
}
