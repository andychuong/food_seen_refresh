using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodSeen.API.Infrastructure.Data;
using FoodSeen.API.Models.DTOs;
using FoodSeen.API.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

namespace FoodSeen.Tests.Integration;

/// <summary>
/// Integration tests for the Posts API endpoints.
/// Uses WebApplicationFactory to spin up a test server with in-memory database,
/// testing the full HTTP request/response pipeline including routing, serialization,
/// and database operations.
/// </summary>
public class PostsControllerTests : IDisposable
{
    // HTTP client configured to make requests to the test server
    private readonly HttpClient _client;

    // Factory that creates and manages the test server instance
    private readonly WebApplicationFactory<Program> _factory;

    // Unique database name for test isolation - each test class gets its own DB
    private readonly string _databaseName;

    /// <summary>
    /// Test fixture setup - creates a test server with in-memory database.
    /// Replaces the production PostgreSQL/PostGIS database with EF Core InMemory provider.
    /// </summary>
    public PostsControllerTests()
    {
        // Generate unique DB name to prevent test interference
        _databaseName = "TestDb_" + Guid.NewGuid();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Use "Testing" environment to disable certain production features
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove all database-related service registrations from production config
                // This includes DbContext, DbContextOptions, and Npgsql-specific services
                var descriptorsToRemove = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                         d.ServiceType == typeof(ApplicationDbContext) ||
                         d.ServiceType.FullName?.Contains("EntityFramework") == true ||
                         d.ServiceType.FullName?.Contains("Npgsql") == true
                ).ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Register in-memory database for fast, isolated testing
                // Note: In-memory DB doesn't support all features (transactions, raw SQL)
                var dbName = _databaseName;
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        // Create HTTP client that automatically handles base URL and cookies
        _client = _factory.CreateClient();

        // Seed the database with test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        SeedTestData(dbContext);
    }

    /// <summary>
    /// Seeds the in-memory database with test data.
    /// Uses unique GUIDs that don't conflict with EF Core seed data from migrations.
    /// Creates a user, category, and post for testing CRUD operations.
    /// </summary>
    private static void SeedTestData(ApplicationDbContext dbContext)
    {
        // Skip seeding if data exists (from EF OnModelCreating HasData)
        if (dbContext.Users.Any())
            return;

        // Factory for creating PostGIS-compatible Point geometries
        // SRID 4326 = WGS84 coordinate system (GPS standard)
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

        // Create test user - uses "a" prefix GUIDs to avoid conflicts
        var user = new User
        {
            Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            Email = "testuser@example.com",
            Username = "testuser",
            PasswordHash = "hash"
        };

        // Create test category
        var category = new Category
        {
            Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
            Name = "Test Category",
            Description = "Test category for integration tests"
        };

        // Create test post with geospatial data (San Francisco coordinates)
        var post = new Post
        {
            Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
            UserId = user.Id,
            Title = "Free Pizza Downtown",
            Description = "Come get free pizza slices!",
            Address = "123 Main St, San Francisco, CA",
            Latitude = 37.7749,
            Longitude = -122.4194,
            // Note: Point constructor takes (longitude, latitude) - opposite of common convention
            Location = geometryFactory.CreatePoint(new Coordinate(-122.4194, 37.7749)),
            EventDate = DateTime.UtcNow.AddDays(1),
            IsActive = true,
            User = user,
            Categories = new List<Category> { category }
        };

        dbContext.Users.Add(user);
        dbContext.Categories.Add(category);
        dbContext.Posts.Add(post);
        dbContext.SaveChanges();
    }

    #region GET /api/posts Tests

    /// <summary>
    /// Verifies that the posts list endpoint returns paginated results.
    /// Tests the basic happy path with default pagination.
    /// </summary>
    [Fact]
    public async Task GetPosts_ReturnsOkWithPaginatedResult()
    {
        // Act - Request posts without pagination parameters
        var response = await _client.GetAsync("/api/posts");

        // Assert - Should return 200 OK with valid paginated response
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PostDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that pagination parameters are correctly parsed and applied.
    /// </summary>
    [Fact]
    public async Task GetPosts_WithPagination_ReturnsCorrectPage()
    {
        // Act - Request specific page with page size
        var response = await _client.GetAsync("/api/posts?page=1&pageSize=10");

        // Assert - Pagination metadata should match request
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PostDto>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    #endregion

    #region GET /api/posts/{id} Tests

    /// <summary>
    /// Verifies that existing posts can be retrieved by ID.
    /// Tests the detail endpoint with seeded test data.
    /// </summary>
    [Fact]
    public async Task GetPostById_ExistingPost_ReturnsOk()
    {
        // Arrange - Use the known test post ID
        var postId = "a3333333-3333-3333-3333-333333333333";

        // Act
        var response = await _client.GetAsync($"/api/posts/{postId}");

        // Assert - Should return full post details
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PostDto>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Free Pizza Downtown");
    }

    /// <summary>
    /// Verifies that requesting a non-existent post returns 404.
    /// Important for proper REST API semantics.
    /// </summary>
    [Fact]
    public async Task GetPostById_NonExistingPost_ReturnsNotFound()
    {
        // Arrange - Generate random GUID that doesn't exist
        var postId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/posts/{postId}");

        // Assert - Should return 404 Not Found
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/posts/nearby Tests

    /// <summary>
    /// Verifies that geospatial nearby search works with valid coordinates.
    /// Tests the PostGIS distance query functionality.
    /// </summary>
    [Fact]
    public async Task GetNearbyPosts_ValidCoordinates_ReturnsOk()
    {
        // Act - Search near San Francisco (same location as test post)
        var response = await _client.GetAsync("/api/posts/nearby?latitude=37.7749&longitude=-122.4194&radiusKm=10");

        // Assert - Should return results within radius
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PostDto>>();
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that invalid latitude values are rejected.
    /// Latitude must be between -90 and 90 degrees.
    /// </summary>
    [Fact]
    public async Task GetNearbyPosts_InvalidLatitude_ReturnsBadRequest()
    {
        // Act - Latitude 100 is out of valid range
        var response = await _client.GetAsync("/api/posts/nearby?latitude=100&longitude=-122.4194&radiusKm=10");

        // Assert - Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that invalid longitude values are rejected.
    /// Longitude must be between -180 and 180 degrees.
    /// </summary>
    [Fact]
    public async Task GetNearbyPosts_InvalidLongitude_ReturnsBadRequest()
    {
        // Act - Longitude -200 is out of valid range
        var response = await _client.GetAsync("/api/posts/nearby?latitude=37.7749&longitude=-200&radiusKm=10");

        // Assert - Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/posts/search Tests

    /// <summary>
    /// Verifies that text search works and returns matching results.
    /// Tests case-insensitive search against title and description.
    /// </summary>
    [Fact]
    public async Task SearchPosts_ValidQuery_ReturnsResults()
    {
        // Act - Search for "pizza" (test post title contains "Pizza")
        var response = await _client.GetAsync("/api/posts/search?q=pizza");

        // Assert - Should return matching posts
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PostDto>>();
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that empty search queries are rejected.
    /// Prevents expensive full-table scans on empty input.
    /// </summary>
    [Fact]
    public async Task SearchPosts_EmptyQuery_ReturnsBadRequest()
    {
        // Act - Empty query string
        var response = await _client.GetAsync("/api/posts/search?q=");

        // Assert - Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/categories Tests

    /// <summary>
    /// Verifies that the categories endpoint returns available categories.
    /// Categories are used for filtering posts.
    /// </summary>
    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert - Should return list of categories
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    #endregion

    #region POST /api/posts Tests (Authorization)

    /// <summary>
    /// Verifies that creating a post requires authentication.
    /// Unauthenticated requests should be rejected with 401.
    /// </summary>
    [Fact]
    public async Task CreatePost_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange - Create request body (no auth token)
        var createRequest = new
        {
            Title = "New Event",
            Description = "Description",
            Address = "123 Test St",
            Latitude = 37.78,
            Longitude = -122.42,
            EventDate = DateTime.UtcNow.AddDays(1)
        };

        // Act - POST without Authorization header
        var response = await _client.PostAsJsonAsync("/api/posts", createRequest);

        // Assert - Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    /// <summary>
    /// Cleanup - disposes the test server and related resources.
    /// </summary>
    public void Dispose()
    {
        _factory.Dispose();
    }
}
