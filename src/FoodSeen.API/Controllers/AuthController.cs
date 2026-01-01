using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodSeen.API.Controllers;

/// <summary>
/// Controller for user authentication and authorization.
/// Handles registration, login, token refresh, and logout operations.
/// </summary>
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account with email and password.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Logs out the user by revoking their refresh token.
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var success = await _authService.RevokeTokenAsync(request.RefreshToken);

        if (!success)
            return BadRequest(ApiError.BadRequest("Invalid token", "INVALID_TOKEN"));

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Gets the currently authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdResult = RequireUserId();
        if (userIdResult.Result != null)
            return userIdResult.Result;

        var user = await _authService.GetCurrentUserAsync(userIdResult.Value);
        if (user == null)
            return NotFound(ApiError.NotFound("User not found"));

        return Ok(user);
    }
}
