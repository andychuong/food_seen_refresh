using FluentAssertions;
using FoodSeen.API.Models.Entities;
using FoodSeen.API.Models.Requests;
using FoodSeen.API.Repositories.Interfaces;
using FoodSeen.API.Services.Implementations;
using Moq;
using NetTopologySuite.Geometries;

namespace FoodSeen.Tests.Services;

/// <summary>
/// Unit tests for PostService business logic.
/// Uses Moq to isolate the service from its dependencies (repositories).
/// Tests cover CRUD operations, pagination, search, and authorization checks.
/// </summary>
public class PostServiceTests
{
    // Mocked dependencies - these simulate repository behavior without hitting a real database
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;

    // The system under test (SUT)
    private readonly PostService _postService;

    // Factory for creating geospatial Point objects with WGS84 coordinate system (SRID 4326)
    private readonly GeometryFactory _geometryFactory;

    /// <summary>
    /// Test fixture setup - runs before each test method.
    /// Creates fresh mock instances to ensure test isolation.
    /// </summary>
    public PostServiceTests()
    {
        _mockPostRepository = new Mock<IPostRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _postService = new PostService(_mockPostRepository.Object, _mockCategoryRepository.Object);

        // SRID 4326 is the standard WGS84 coordinate system used by GPS
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    /// <summary>
    /// Factory method to create a realistic Post entity for testing.
    /// Includes all required relationships (User, Categories) and geospatial data.
    /// </summary>
    /// <param name="id">Optional post ID (generates random GUID if not provided)</param>
    /// <param name="userId">Optional user ID for ownership testing</param>
    /// <returns>A fully populated Post entity</returns>
    private Post CreateTestPost(Guid? id = null, Guid? userId = null)
    {
        var postId = id ?? Guid.NewGuid();
        var postUserId = userId ?? Guid.NewGuid();

        return new Post
        {
            Id = postId,
            UserId = postUserId,
            Title = "Free Pizza Event",
            Description = "Come get free pizza at the community center",
            Address = "123 Main St, San Francisco, CA",
            Latitude = 37.7749,
            Longitude = -122.4194,
            // Note: Point coordinates are (longitude, latitude) - opposite of common convention
            Location = _geometryFactory.CreatePoint(new Coordinate(-122.4194, 37.7749)),
            EventDate = DateTime.UtcNow.AddDays(1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            User = new User
            {
                Id = postUserId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            },
            Categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Free Food" }
            }
        };
    }

    #region GetPostByIdAsync Tests

    /// <summary>
    /// Verifies that fetching an existing post returns a properly mapped DTO
    /// with all expected fields populated from the entity.
    /// </summary>
    [Fact]
    public async Task GetPostByIdAsync_ExistingPost_ReturnsPostDto()
    {
        // Arrange - Set up repository to return a test post
        var postId = Guid.NewGuid();
        var post = CreateTestPost(postId);

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        // Act - Call the service method
        var result = await _postService.GetPostByIdAsync(postId);

        // Assert - Verify the DTO is correctly mapped from the entity
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId);
        result.Title.Should().Be("Free Pizza Event");
        result.AuthorUsername.Should().Be("testuser");
        result.Categories.Should().Contain("Free Food");
    }

    /// <summary>
    /// Verifies that fetching a non-existent post returns null
    /// rather than throwing an exception (null object pattern).
    /// </summary>
    [Fact]
    public async Task GetPostByIdAsync_NonExistingPost_ReturnsNull()
    {
        // Arrange - Repository returns null for unknown ID
        var postId = Guid.NewGuid();

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _postService.GetPostByIdAsync(postId);

        // Assert - Service should propagate null, not throw
        result.Should().BeNull();
    }

    #endregion

    #region Listing and Search Tests

    /// <summary>
    /// Verifies pagination metadata is correctly calculated and returned.
    /// The service should combine repository data with count to build PaginatedResult.
    /// </summary>
    [Fact]
    public async Task GetActivePostsAsync_ReturnsPaginatedResult()
    {
        // Arrange - Set up repository with test data and count
        var posts = new List<Post>
        {
            CreateTestPost(),
            CreateTestPost()
        };

        _mockPostRepository
            .Setup(r => r.GetActivePostsAsync(1, 10))
            .ReturnsAsync(posts);

        _mockPostRepository
            .Setup(r => r.GetActivePostsCountAsync())
            .ReturnsAsync(2);

        // Act
        var result = await _postService.GetActivePostsAsync(1, 10);

        // Assert - Verify pagination wrapper is correctly constructed
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    /// <summary>
    /// Verifies that distance calculation works for geospatial queries.
    /// When user location matches post location, distance should be 0km.
    /// </summary>
    [Fact]
    public async Task GetNearbyPostsAsync_CalculatesDistanceCorrectly()
    {
        // Arrange - User is at same location as the test post (SF downtown)
        var userLat = 37.7749;
        var userLon = -122.4194;
        var post = CreateTestPost();

        _mockPostRepository
            .Setup(r => r.GetNearbyPostsAsync(userLat, userLon, 10, 1, 10))
            .ReturnsAsync(new List<Post> { post });

        _mockPostRepository
            .Setup(r => r.GetNearbyPostsCountAsync(userLat, userLon, 10))
            .ReturnsAsync(1);

        // Act
        var result = await _postService.GetNearbyPostsAsync(userLat, userLon, 10, 1, 10);

        // Assert - Distance should be 0 since user and post are at same coordinates
        result.Items.Should().HaveCount(1);
        result.Items.First().DistanceKm.Should().Be(0);
    }

    /// <summary>
    /// Verifies that search returns posts matching the query string.
    /// Search is case-insensitive and matches against title/description.
    /// </summary>
    [Fact]
    public async Task SearchPostsAsync_ReturnsMatchingPosts()
    {
        // Arrange - Test post title contains "Pizza"
        var posts = new List<Post> { CreateTestPost() };

        _mockPostRepository
            .Setup(r => r.SearchPostsAsync("pizza", 1, 10))
            .ReturnsAsync(posts);

        _mockPostRepository
            .Setup(r => r.GetSearchResultsCountAsync("pizza"))
            .ReturnsAsync(1);

        // Act - Search for "pizza" (lowercase)
        var result = await _postService.SearchPostsAsync("pizza", 1, 10);

        // Assert - Should find the "Free Pizza Event" post
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Contain("Pizza");
    }

    #endregion

    #region Create Post Tests

    /// <summary>
    /// Verifies that a valid create request successfully persists a new post
    /// with proper category associations and returns the created DTO.
    /// </summary>
    [Fact]
    public async Task CreatePostAsync_ValidRequest_CreatesPost()
    {
        // Arrange - Build a valid create request with categories
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new CreatePostRequest
        {
            Title = "New Event",
            Description = "Description here",
            Address = "456 Oak St",
            Latitude = 37.78,
            Longitude = -122.42,
            EventDate = DateTime.UtcNow.AddDays(7),
            CategoryIds = new List<Guid> { categoryId }
        };

        var categories = new List<Category>
        {
            new Category { Id = categoryId, Name = "Samples" }
        };

        var createdPost = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            EventDate = request.EventDate,
            Categories = categories,
            User = new User { Id = userId, Username = "creator", Email = "c@e.com", PasswordHash = "h" }
        };

        // Set up category lookup and post creation
        _mockCategoryRepository
            .Setup(r => r.GetCategoriesByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(categories);

        _mockPostRepository
            .Setup(r => r.AddAsync(It.IsAny<Post>()))
            .ReturnsAsync(createdPost);

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(createdPost.Id))
            .ReturnsAsync(createdPost);

        // Act
        var result = await _postService.CreatePostAsync(userId, request);

        // Assert - Verify post was created with correct data
        result.Should().NotBeNull();
        result.Title.Should().Be("New Event");
        result.Categories.Should().Contain("Samples");
        _mockPostRepository.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
    }

    #endregion

    #region Update Post Tests

    /// <summary>
    /// Verifies that post owners can successfully update their own posts.
    /// The service should allow the update when userId matches post.UserId.
    /// </summary>
    [Fact]
    public async Task UpdatePostAsync_OwnPost_UpdatesSuccessfully()
    {
        // Arrange - Create a post owned by the requesting user
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var existingPost = CreateTestPost(postId, userId);

        var request = new UpdatePostRequest
        {
            Title = "Updated Title"
        };

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(existingPost);

        _mockPostRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _postService.UpdatePostAsync(userId, postId, request);

        // Assert - Update should succeed
        result.Should().NotBeNull();
        _mockPostRepository.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Once);
    }

    /// <summary>
    /// Verifies that users cannot update posts they don't own.
    /// This is a critical authorization check to prevent unauthorized modifications.
    /// </summary>
    [Fact]
    public async Task UpdatePostAsync_NotOwner_ReturnsNull()
    {
        // Arrange - Post is owned by a different user
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();  // This user will attempt the update
        var postId = Guid.NewGuid();
        var existingPost = CreateTestPost(postId, ownerId);

        var request = new UpdatePostRequest
        {
            Title = "Trying to update"
        };

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(existingPost);

        // Act - Attempt to update as non-owner
        var result = await _postService.UpdatePostAsync(otherUserId, postId, request);

        // Assert - Should fail authorization, update never called
        result.Should().BeNull();
        _mockPostRepository.Verify(r => r.UpdateAsync(It.IsAny<Post>()), Times.Never);
    }

    #endregion

    #region Delete Post Tests

    /// <summary>
    /// Verifies that post owners can successfully delete their own posts.
    /// </summary>
    [Fact]
    public async Task DeletePostAsync_OwnPost_DeletesSuccessfully()
    {
        // Arrange - Create a post owned by the requesting user
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var existingPost = CreateTestPost(postId, userId);

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(existingPost);

        _mockPostRepository
            .Setup(r => r.DeleteAsync(postId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _postService.DeletePostAsync(userId, postId);

        // Assert - Delete should succeed
        result.Should().BeTrue();
        _mockPostRepository.Verify(r => r.DeleteAsync(postId), Times.Once);
    }

    /// <summary>
    /// Verifies that users cannot delete posts they don't own.
    /// Unauthorized delete attempts should return false without calling repository.
    /// </summary>
    [Fact]
    public async Task DeletePostAsync_NotOwner_ReturnsFalse()
    {
        // Arrange - Post owned by different user
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var existingPost = CreateTestPost(postId, ownerId);

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(existingPost);

        // Act - Attempt delete as non-owner
        var result = await _postService.DeletePostAsync(otherUserId, postId);

        // Assert - Should fail, delete never called
        result.Should().BeFalse();
        _mockPostRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// Verifies that attempting to delete a non-existent post returns false.
    /// This handles the case where the post was already deleted or never existed.
    /// </summary>
    [Fact]
    public async Task DeletePostAsync_NonExistingPost_ReturnsFalse()
    {
        // Arrange - Repository returns null (post doesn't exist)
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        _mockPostRepository
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _postService.DeletePostAsync(userId, postId);

        // Assert - Should return false gracefully
        result.Should().BeFalse();
    }

    #endregion
}
