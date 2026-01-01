using FoodSeen.API.Infrastructure;
using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodSeen.API.Controllers;

/// <summary>
/// Controller for managing food event posts.
/// Provides CRUD operations, geospatial queries, and search functionality.
/// </summary>
public class PostsController : ApiControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Gets a paginated list of active food event posts.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PostDto>>> GetPosts(
        [FromQuery] int page = ApiConstants.DefaultPage,
        [FromQuery] int pageSize = ApiConstants.DefaultPageSize)
    {
        page = Math.Max(page, ApiConstants.DefaultPage);
        pageSize = Math.Clamp(pageSize, ApiConstants.MinPageSize, ApiConstants.MaxPageSize);

        var result = await _postService.GetActivePostsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets posts near a geographic location using PostGIS distance queries.
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<PaginatedResult<PostDto>>> GetNearbyPosts(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] int radiusKm = (int)ApiConstants.DefaultRadiusKm,
        [FromQuery] int page = ApiConstants.DefaultPage,
        [FromQuery] int pageSize = ApiConstants.DefaultPageSize)
    {
        // Validate coordinate bounds
        if (latitude < ApiConstants.MinLatitude || latitude > ApiConstants.MaxLatitude)
            return BadRequest(ApiError.BadRequest("Latitude must be between -90 and 90", "INVALID_LATITUDE"));
        if (longitude < ApiConstants.MinLongitude || longitude > ApiConstants.MaxLongitude)
            return BadRequest(ApiError.BadRequest("Longitude must be between -180 and 180", "INVALID_LONGITUDE"));

        // Clamp pagination and radius to valid ranges
        radiusKm = Math.Clamp(radiusKm, (int)ApiConstants.MinRadiusKm, (int)ApiConstants.MaxRadiusKm);
        page = Math.Max(page, ApiConstants.DefaultPage);
        pageSize = Math.Clamp(pageSize, ApiConstants.MinPageSize, ApiConstants.MaxPageSize);

        var result = await _postService.GetNearbyPostsAsync(latitude, longitude, radiusKm, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Searches posts by title and description using case-insensitive matching.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResult<PostDto>>> SearchPosts(
        [FromQuery] string q,
        [FromQuery] int page = ApiConstants.DefaultPage,
        [FromQuery] int pageSize = ApiConstants.DefaultPageSize)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(ApiError.BadRequest("Search query is required", "EMPTY_QUERY"));

        page = Math.Max(page, ApiConstants.DefaultPage);
        pageSize = Math.Clamp(pageSize, ApiConstants.MinPageSize, ApiConstants.MaxPageSize);

        var result = await _postService.SearchPostsAsync(q, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific post by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound(ApiError.NotFound("Post not found"));

        return Ok(post);
    }

    /// <summary>
    /// Gets all posts created by a specific user.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetUserPosts(Guid userId)
    {
        var posts = await _postService.GetUserPostsAsync(userId);
        return Ok(posts);
    }

    /// <summary>
    /// Gets all posts created by the currently authenticated user.
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetMyPosts()
    {
        var userIdResult = RequireUserId();
        if (userIdResult.Result != null)
            return userIdResult.Result;

        var posts = await _postService.GetUserPostsAsync(userIdResult.Value);
        return Ok(posts);
    }

    /// <summary>
    /// Creates a new food event post. Requires authentication.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostRequest request)
    {
        var userIdResult = RequireUserId();
        if (userIdResult.Result != null)
            return userIdResult.Result;

        var post = await _postService.CreatePostAsync(userIdResult.Value, request);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    /// <summary>
    /// Updates an existing post. Only the post owner can update.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PostDto>> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        var userIdResult = RequireUserId();
        if (userIdResult.Result != null)
            return userIdResult.Result;

        var post = await _postService.UpdatePostAsync(userIdResult.Value, id, request);
        if (post == null)
            return NotFound(ApiError.NotFound("Post not found or you don't have permission to update it"));

        return Ok(post);
    }

    /// <summary>
    /// Deletes a post. Only the post owner can delete.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeletePost(Guid id)
    {
        var userIdResult = RequireUserId();
        if (userIdResult.Result != null)
            return userIdResult.Result;

        var success = await _postService.DeletePostAsync(userIdResult.Value, id);
        if (!success)
            return NotFound(ApiError.NotFound("Post not found or you don't have permission to delete it"));

        return NoContent();
    }
}
