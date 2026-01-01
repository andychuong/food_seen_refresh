using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Requests;

namespace FoodSeen.API.Services.Interfaces;

public interface IPostService
{
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<PaginatedResult<PostDto>> GetActivePostsAsync(int page, int pageSize);
    Task<PaginatedResult<PostDto>> GetNearbyPostsAsync(double latitude, double longitude, int radiusKm, int page, int pageSize);
    Task<PaginatedResult<PostDto>> SearchPostsAsync(string query, int page, int pageSize);
    Task<IEnumerable<PostDto>> GetUserPostsAsync(Guid userId);
    Task<IEnumerable<PostDto>> GetPostsByCategoryAsync(Guid categoryId);
    Task<PostDto> CreatePostAsync(Guid userId, CreatePostRequest request);
    Task<PostDto?> UpdatePostAsync(Guid userId, Guid postId, UpdatePostRequest request);
    Task<bool> DeletePostAsync(Guid userId, Guid postId);
}
