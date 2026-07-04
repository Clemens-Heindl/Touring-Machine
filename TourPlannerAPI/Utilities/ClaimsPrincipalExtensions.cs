using System.Security.Claims;

namespace TourPlannerAPI.Utilities;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the authenticated user's id from the NameIdentifier claim.</summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("The token does not contain a valid user id.");
        }

        return userId;
    }
}
