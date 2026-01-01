using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Repositories.Interfaces;
using FoodSeen.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoodSeen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPostService _postService;

    public CategoriesController(ICategoryRepository categoryRepository, IPostService postService)
    {
        _categoryRepository = categoryRepository;
        _postService = postService;
    }

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

    [HttpGet("{id}/posts")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByCategory(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        var posts = await _postService.GetPostsByCategoryAsync(id);
        return Ok(posts);
    }
}
