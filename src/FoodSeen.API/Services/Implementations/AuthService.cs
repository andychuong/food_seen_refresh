using FoodSeen.API.Infrastructure.Auth;
using FoodSeen.API.Infrastructure.Data;
using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Entities;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Repositories.Interfaces;
using FoodSeen.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodSeen.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        ApplicationDbContext context,
        IJwtTokenGenerator tokenGenerator,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _context = context;
        _tokenGenerator = tokenGenerator;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResult { Success = false, Error = "Email is already registered" };
        }

        // Check if username already exists
        var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUsername != null)
        {
            return new AuthResult { Success = false, Error = "Username is already taken" };
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            Username = request.Username,
            PasswordHash = PasswordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            AuthProvider = AuthProvider.Local,
            EmailConfirmed = true // For now, skip email verification
        };

        await _userRepository.AddAsync(user);

        // Generate tokens
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResult { Success = false, Error = "Invalid email or password" };
        }

        if (user.AuthProvider != AuthProvider.Local || string.IsNullOrEmpty(user.PasswordHash))
        {
            return new AuthResult { Success = false, Error = "Please use social login for this account" };
        }

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return new AuthResult { Success = false, Error = "Invalid email or password" };
        }

        // Generate tokens
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null)
        {
            return new AuthResult { Success = false, Error = "Invalid refresh token" };
        }

        if (!token.IsActive)
        {
            return new AuthResult { Success = false, Error = "Refresh token has expired or been revoked" };
        }

        // Revoke old token
        token.RevokedAt = DateTime.UtcNow;

        // Create new tokens
        var accessToken = _tokenGenerator.GenerateAccessToken(token.User);
        var newRefreshToken = await CreateRefreshTokenAsync(token.UserId);

        await _context.SaveChangesAsync();

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            User = MapToDto(token.User)
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null || !token.IsActive)
        {
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? MapToDto(user) : null;
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = _tokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private static UserDto MapToDto(User user)
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
