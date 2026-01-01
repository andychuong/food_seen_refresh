using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Repositories.Interfaces;
using FoodSeen.API.Services.Interfaces;

namespace FoodSeen.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        if (request.Username != null)
        {
            // Check if username is already taken
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new InvalidOperationException("Username is already taken");
            }
            user.Username = request.Username;
        }

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        if (request.DefaultLatitude.HasValue) user.DefaultLatitude = request.DefaultLatitude;
        if (request.DefaultLongitude.HasValue) user.DefaultLongitude = request.DefaultLongitude;
        if (request.DefaultRadiusKm.HasValue) user.DefaultRadiusKm = request.DefaultRadiusKm.Value;

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    private static UserDto MapToDto(Models.Entities.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            DefaultLatitude = user.DefaultLatitude,
            DefaultLongitude = user.DefaultLongitude,
            DefaultRadiusKm = user.DefaultRadiusKm,
            CreatedAt = user.CreatedAt
        };
    }
}
