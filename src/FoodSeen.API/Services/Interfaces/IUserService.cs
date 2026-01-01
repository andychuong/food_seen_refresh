using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Requests;

namespace FoodSeen.API.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request);
}
