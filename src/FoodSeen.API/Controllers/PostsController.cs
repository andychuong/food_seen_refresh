using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodSeen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PostDto>>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _postService.GetActivePostsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<PaginatedResult<PostDto>>> GetNearbyPosts(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] int radiusKm = 10,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (latitude < -90 || latitude > 90)
            return BadRequest("Latitude must be between -90 and 90");
        if (longitude < -180 || longitude > 180)
            return BadRequest("Longitude must be between -180 and 180");
        if (radiusKm < 1 || radiusKm > 100) radiusKm = 10;
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _postService.GetNearbyPostsAsync(latitude, longitude, radiusKm, page, pageSize);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResult<PostDto>>> SearchPosts(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query is required");

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _postService.SearchPostsAsync(q, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetUserPosts(Guid userId)
    {
        var posts = await _postService.GetUserPostsAsync(userId);
        return Ok(posts);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetMyPosts()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var posts = await _postService.GetUserPostsAsync(userId.Value);
        return Ok(posts);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var post = await _postService.CreatePostAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PostDto>> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var post = await _postService.UpdatePostAsync(userId.Value, id, request);
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeletePost(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var success = await _postService.DeletePostAsync(userId.Value, id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }
}
