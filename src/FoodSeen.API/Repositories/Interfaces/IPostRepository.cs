using FoodSeen.API.Models.Entities;

namespace FoodSeen.API.Repositories.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetActivePostsAsync(int page, int pageSize);
    Task<IEnumerable<Post>> GetNearbyPostsAsync(double latitude, double longitude, int radiusKm, int page, int pageSize);
    Task<IEnumerable<Post>> SearchPostsAsync(string query, int page, int pageSize);
    Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId);
    Task<IEnumerable<Post>> GetPostsByCategoryIdAsync(Guid categoryId);
    Task<int> GetActivePostsCountAsync();
    Task<int> GetNearbyPostsCountAsync(double latitude, double longitude, int radiusKm);
    Task<int> GetSearchResultsCountAsync(string query);
}
