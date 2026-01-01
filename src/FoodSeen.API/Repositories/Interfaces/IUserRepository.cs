using FoodSeen.API.Models.Entities;

namespace FoodSeen.API.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByProviderIdAsync(AuthProvider provider, string providerId);
}
