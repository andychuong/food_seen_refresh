using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodSeen.API.Controllers;

/// <summary>
/// Controller for user profile management.
/// Provides endpoints for viewing and updating user information.
/// </summary>
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets a user's public profile by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiError.NotFound("User not found"));

        return Ok(user);
    }

    /// <summary>
    /// Updates a user's profile. Users can only update their own profile.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null || currentUserId != id)
            return StatusCode(403, ApiError.Forbidden("You can only update your own profile"));

        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            if (user == null)
                return NotFound(ApiError.NotFound("User not found"));

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.BadRequest(ex.Message));
        }
    }
}
