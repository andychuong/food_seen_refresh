using FoodSeen.API.Models.Entities;

namespace FoodSeen.API.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<IEnumerable<Category>> GetCategoriesByIdsAsync(IEnumerable<Guid> ids);
}
