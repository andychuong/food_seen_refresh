using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Repositories.Interfaces;
using FoodSeen.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoodSeen.API.Controllers;

/// <summary>
/// Controller for managing food event categories.
/// Categories help users filter and discover relevant food events.
/// </summary>
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPostService _postService;

    public CategoriesController(ICategoryRepository categoryRepository, IPostService postService)
    {
        _categoryRepository = categoryRepository;
        _postService = postService;
    }

    /// <summary>
    /// Gets all available categories.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Gets all posts in a specific category.
    /// </summary>
    [HttpGet("{id}/posts")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByCategory(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound(ApiError.NotFound("Category not found"));

        var posts = await _postService.GetPostsByCategoryAsync(id);
        return Ok(posts);
    }
}
