using TourPlannerAPI.Dtos;

namespace TourPlannerAPI.Services;

/// <summary>Issues signed JWT access tokens for authenticated users.</summary>
public interface IJwtTokenService
{
    string CreateToken(UserDto user);
}
