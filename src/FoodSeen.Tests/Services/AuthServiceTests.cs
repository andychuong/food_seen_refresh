using FluentAssertions;
using FoodSeen.API.Infrastructure.Auth;
using FoodSeen.API.Infrastructure.Data;
using FoodSeen.API.Models.Entities;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Repositories.Interfaces;
using FoodSeen.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace FoodSeen.Tests.Services;

/// <summary>
/// Unit tests for AuthService authentication and authorization logic.
/// Uses a combination of Moq (for user repository) and in-memory database (for refresh tokens)
/// to test registration, login, token refresh, and user retrieval operations.
/// </summary>
public class AuthServiceTests
{
    // Mocked user repository - simulates database operations for User entity
    private readonly Mock<IUserRepository> _mockUserRepository;

    // Mocked JWT token generator - returns predictable tokens for testing
    private readonly Mock<IJwtTokenGenerator> _mockTokenGenerator;

    // In-memory database context for RefreshToken operations (requires actual EF Core behavior)
    private readonly ApplicationDbContext _dbContext;

    // The system under test
    private readonly AuthService _authService;

    // JWT configuration settings used for token generation
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Test fixture setup - creates fresh instances for each test.
    /// Uses in-memory database with unique name to ensure test isolation.
    /// </summary>
    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenGenerator = new Mock<IJwtTokenGenerator>();

        // Create in-memory database with unique name per test to prevent data leakage
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Configure JWT settings for testing (values don't need to be production-valid)
        _jwtSettings = new JwtSettings
        {
            Key = "test-secret-key-that-is-at-least-32-characters",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _authService = new AuthService(
            _mockUserRepository.Object,
            _dbContext,
            _mockTokenGenerator.Object,
            Options.Create(_jwtSettings)
        );

        // Set up predictable token generation for assertions
        _mockTokenGenerator
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("test-access-token");

        _mockTokenGenerator
            .Setup(t => t.GenerateRefreshToken())
            .Returns("test-refresh-token");
    }

    #region Registration Tests

    /// <summary>
    /// Verifies that new users can register successfully with valid credentials.
    /// Should create user, generate tokens, and return success response.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsSuccess()
    {
        // Arrange - New user with unique email and username
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "password123",
            FirstName = "New",
            LastName = "User"
        };

        // Email doesn't exist
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Username doesn't exist
        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        // Return the created user
        _mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert - Registration should succeed with tokens
        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("newuser@example.com");
        result.User.Username.Should().Be("newuser");
    }

    /// <summary>
    /// Verifies that registration fails when email is already in use.
    /// Prevents duplicate accounts with the same email address.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ExistingEmail_ReturnsError()
    {
        // Arrange - Attempt to register with existing email
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Username = "newuser",
            Password = "password123"
        };

        // Email already exists in database
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = "existinguser",
                PasswordHash = "hash"
            });

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert - Should fail with descriptive error
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Email is already registered");
    }

    /// <summary>
    /// Verifies that registration fails when username is already taken.
    /// Ensures unique usernames across the platform.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ExistingUsername_ReturnsError()
    {
        // Arrange - New email but existing username
        var request = new RegisterRequest
        {
            Email = "new@example.com",
            Username = "existinguser",
            Password = "password123"
        };

        // Email is available
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // But username is taken
        _mockUserRepository
            .Setup(r => r.GetByUsernameAsync(request.Username))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = "other@example.com",
                Username = request.Username,
                PasswordHash = "hash"
            });

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Username is already taken");
    }

    #endregion

    #region Login Tests

    /// <summary>
    /// Verifies that users can login with correct email and password.
    /// Should return access and refresh tokens on success.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange - Create user with hashed password
        var password = "password123";
        var passwordHash = PasswordHasher.Hash(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Username = "testuser",
            PasswordHash = passwordHash,
            AuthProvider = AuthProvider.Local  // Important: local auth, not social
        };

        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = password
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert - Login should succeed with tokens
        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.User.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that login fails when email doesn't exist.
    /// Uses generic error message to prevent email enumeration attacks.
    /// </summary>
    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsError()
    {
        // Arrange - Non-existent email
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert - Generic error to prevent email enumeration
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }

    /// <summary>
    /// Verifies that login fails with incorrect password.
    /// Uses same error message as invalid email for security.
    /// </summary>
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsError()
    {
        // Arrange - Correct email, wrong password
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Username = "testuser",
            PasswordHash = PasswordHasher.Hash("correctpassword"),
            AuthProvider = AuthProvider.Local
        };

        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrongpassword"  // Intentionally wrong
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert - Same generic error as invalid email
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");
    }

    /// <summary>
    /// Verifies that users who registered via social auth cannot use password login.
    /// Directs them to use the appropriate OAuth provider.
    /// </summary>
    [Fact]
    public async Task LoginAsync_SocialAuthUser_ReturnsError()
    {
        // Arrange - User registered via Google OAuth
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "social@example.com",
            Username = "socialuser",
            AuthProvider = AuthProvider.Google,
            ProviderId = "google123"
            // Note: No PasswordHash for social auth users
        };

        var request = new LoginRequest
        {
            Email = "social@example.com",
            Password = "anypassword"
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert - Should prompt social login instead
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Please use social login for this account");
    }

    #endregion

    #region User Retrieval Tests

    /// <summary>
    /// Verifies that GetCurrentUser returns user data for valid user IDs.
    /// Used to fetch user profile after JWT validation.
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_ExistingUser_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash"
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert - Should return mapped DTO
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("user@example.com");
        result.Username.Should().Be("testuser");
    }

    /// <summary>
    /// Verifies that GetCurrentUser returns null for non-existent users.
    /// Handles edge case where JWT is valid but user was deleted.
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Token Revocation Tests

    /// <summary>
    /// Verifies that valid refresh tokens can be revoked (logged out).
    /// Sets RevokedAt timestamp to invalidate the token.
    /// </summary>
    [Fact]
    public async Task RevokeTokenAsync_ValidToken_ReturnsTrue()
    {
        // Arrange - Create active refresh token in database
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService.RevokeTokenAsync("valid-refresh-token");

        // Assert - Token should be marked as revoked
        result.Should().BeTrue();

        var token = await _dbContext.RefreshTokens.FindAsync(refreshToken.Id);
        token!.RevokedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that revoking non-existent tokens returns false.
    /// Gracefully handles invalid/expired token revocation attempts.
    /// </summary>
    [Fact]
    public async Task RevokeTokenAsync_InvalidToken_ReturnsFalse()
    {
        // Act - Try to revoke token that doesn't exist
        var result = await _authService.RevokeTokenAsync("nonexistent-token");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
