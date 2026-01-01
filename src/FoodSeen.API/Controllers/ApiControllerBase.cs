using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace FoodSeen.API.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// Includes user identity extraction and standardized error responses.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Extracts the current user's ID from the JWT claims.
    /// Returns null if the user is not authenticated or the claim is invalid.
    /// </summary>
    /// <returns>The user's GUID, or null if not authenticated</returns>
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }

    /// <summary>
    /// Gets the current user ID or returns Unauthorized if not authenticated.
    /// Use this when the endpoint requires authentication.
    /// </summary>
    /// <returns>ActionResult with user ID or Unauthorized response</returns>
    protected ActionResult<Guid> RequireUserId()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiError.Unauthorized("User not authenticated"));
        }
        return userId.Value;
    }
}

/// <summary>
/// Standardized error response format for API errors.
/// Provides consistent structure across all endpoints.
/// </summary>
public class ApiError
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiError BadRequest(string message, string? code = null)
    {
        return new ApiError { Message = message, Code = code ?? "BAD_REQUEST" };
    }

    public static ApiError NotFound(string message = "Resource not found")
    {
        return new ApiError { Message = message, Code = "NOT_FOUND" };
    }

    public static ApiError Unauthorized(string message = "Unauthorized")
    {
        return new ApiError { Message = message, Code = "UNAUTHORIZED" };
    }

    public static ApiError Forbidden(string message = "Access denied")
    {
        return new ApiError { Message = message, Code = "FORBIDDEN" };
    }

    public static ApiError ValidationFailed(Dictionary<string, string[]> errors)
    {
        return new ApiError
        {
            Message = "Validation failed",
            Code = "VALIDATION_ERROR",
            Errors = errors
        };
    }
}
