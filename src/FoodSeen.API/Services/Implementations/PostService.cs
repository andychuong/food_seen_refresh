using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Entities;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Repositories.Interfaces;
using FoodSeen.API.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace FoodSeen.API.Services.Implementations;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly GeometryFactory _geometryFactory;

    public PostService(IPostRepository postRepository, ICategoryRepository categoryRepository)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        return post != null ? MapToDto(post) : null;
    }

    public async Task<PaginatedResult<PostDto>> GetActivePostsAsync(int page, int pageSize)
    {
        var posts = await _postRepository.GetActivePostsAsync(page, pageSize);
        var totalCount = await _postRepository.GetActivePostsCountAsync();

        return new PaginatedResult<PostDto>
        {
            Items = posts.Select(p => MapToDto(p)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<PostDto>> GetNearbyPostsAsync(double latitude, double longitude, int radiusKm, int page, int pageSize)
    {
        var posts = await _postRepository.GetNearbyPostsAsync(latitude, longitude, radiusKm, page, pageSize);
        var totalCount = await _postRepository.GetNearbyPostsCountAsync(latitude, longitude, radiusKm);

        var userLocation = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        return new PaginatedResult<PostDto>
        {
            Items = posts.Select(p => MapToDto(p, userLocation)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<PostDto>> SearchPostsAsync(string query, int page, int pageSize)
    {
        var posts = await _postRepository.SearchPostsAsync(query, page, pageSize);
        var totalCount = await _postRepository.GetSearchResultsCountAsync(query);

        return new PaginatedResult<PostDto>
        {
            Items = posts.Select(p => MapToDto(p)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<PostDto>> GetUserPostsAsync(Guid userId)
    {
        var posts = await _postRepository.GetPostsByUserIdAsync(userId);
        return posts.Select(p => MapToDto(p)).ToList();
    }

    public async Task<IEnumerable<PostDto>> GetPostsByCategoryAsync(Guid categoryId)
    {
        var posts = await _postRepository.GetPostsByCategoryIdAsync(categoryId);
        return posts.Select(p => MapToDto(p)).ToList();
    }

    public async Task<PostDto> CreatePostAsync(Guid userId, CreatePostRequest request)
    {
        var post = new Post
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            EventDate = request.EventDate,
            EventEndDate = request.EventEndDate
        };

        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            var categories = await _categoryRepository.GetCategoriesByIdsAsync(request.CategoryIds);
            post.Categories = categories.ToList();
        }

        var createdPost = await _postRepository.AddAsync(post);

        // Reload the post with relationships
        var fullPost = await _postRepository.GetByIdAsync(createdPost.Id);
        return MapToDto(fullPost!);
    }

    public async Task<PostDto?> UpdatePostAsync(Guid userId, Guid postId, UpdatePostRequest request)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null || post.UserId != userId)
        {
            return null;
        }

        if (request.Title != null) post.Title = request.Title;
        if (request.Description != null) post.Description = request.Description;
        if (request.Address != null) post.Address = request.Address;
        if (request.Latitude.HasValue) post.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) post.Longitude = request.Longitude.Value;
        if (request.EventDate.HasValue) post.EventDate = request.EventDate.Value;
        if (request.EventEndDate.HasValue) post.EventEndDate = request.EventEndDate;
        if (request.IsActive.HasValue) post.IsActive = request.IsActive.Value;

        if (request.CategoryIds != null)
        {
            var categories = await _categoryRepository.GetCategoriesByIdsAsync(request.CategoryIds);
            post.Categories = categories.ToList();
        }

        await _postRepository.UpdateAsync(post);

        var updatedPost = await _postRepository.GetByIdAsync(postId);
        return MapToDto(updatedPost!);
    }

    public async Task<bool> DeletePostAsync(Guid userId, Guid postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null || post.UserId != userId)
        {
            return false;
        }

        await _postRepository.DeleteAsync(postId);
        return true;
    }

    private static PostDto MapToDto(Post post, Point? userLocation = null)
    {
        var dto = new PostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            AuthorUsername = post.User?.Username ?? "Unknown",
            Title = post.Title,
            Description = post.Description,
            Address = post.Address,
            Latitude = post.Latitude,
            Longitude = post.Longitude,
            EventDate = post.EventDate,
            EventEndDate = post.EventEndDate,
            IsActive = post.IsActive,
            Categories = post.Categories?.Select(c => c.Name).ToList() ?? new List<string>(),
            CreatedAt = post.CreatedAt
        };

        if (userLocation != null && post.Location != null)
        {
            // Distance in meters, convert to km
            dto.DistanceKm = Math.Round(post.Location.Distance(userLocation) / 1000, 2);
        }

        return dto;
    }
}
