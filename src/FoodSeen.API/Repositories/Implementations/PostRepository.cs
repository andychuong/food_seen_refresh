using FoodSeen.API.Infrastructure.Data;
using FoodSeen.API.Models.Entities;
using FoodSeen.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FoodSeen.API.Repositories.Implementations;

public class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;
    private readonly GeometryFactory _geometryFactory;

    public PostRepository(ApplicationDbContext context)
    {
        _context = context;
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .ToListAsync();
    }

    public async Task<Post> AddAsync(Post entity)
    {
        // Create the PostGIS point from latitude/longitude
        entity.Location = _geometryFactory.CreatePoint(new Coordinate(entity.Longitude, entity.Latitude));

        _context.Posts.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Post entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        // Update the PostGIS point if coordinates changed
        entity.Location = _geometryFactory.CreatePoint(new Coordinate(entity.Longitude, entity.Latitude));

        _context.Posts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Posts.FindAsync(id);
        if (entity != null)
        {
            _context.Posts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Posts.AnyAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetActivePostsAsync(int page, int pageSize)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .Where(p => p.IsActive && p.EventDate >= DateTime.UtcNow)
            .OrderBy(p => p.EventDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetNearbyPostsAsync(double latitude, double longitude, int radiusKm, int page, int pageSize)
    {
        var userLocation = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        var radiusMeters = radiusKm * 1000;

        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .Where(p => p.IsActive && p.EventDate >= DateTime.UtcNow)
            .Where(p => p.Location.Distance(userLocation) <= radiusMeters)
            .OrderBy(p => p.Location.Distance(userLocation))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> SearchPostsAsync(string query, int page, int pageSize)
    {
        var lowerQuery = query.ToLower();

        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .Where(p => p.IsActive && p.EventDate >= DateTime.UtcNow)
            .Where(p => p.Title.ToLower().Contains(lowerQuery) ||
                        p.Description.ToLower().Contains(lowerQuery))
            .OrderBy(p => p.EventDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByCategoryIdAsync(Guid categoryId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Categories)
            .Where(p => p.IsActive && p.EventDate >= DateTime.UtcNow)
            .Where(p => p.Categories.Any(c => c.Id == categoryId))
            .OrderBy(p => p.EventDate)
            .ToListAsync();
    }

    public async Task<int> GetActivePostsCountAsync()
    {
        return await _context.Posts
            .Where(p => p.IsActive && p.EventDate >= DateTime.UtcNow)
            .CountAsync();
    }

    public async Task<int> GetNearbyPostsCountAsync(double latitude, double longitude, int radiusKm)
    {
        var userLocation = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        var radiusMeters = radiusKm * 1000;

        return await _context.Posts
            .Where(p => p.IsActive && p.EventDate >= DateTime.UtcNow)
            .Where(p => p.Location.Distance(userLocation) <= radiusMeters)
            .CountAsync();
    }

    public async Task<int> GetSearchResultsCountAsync(string query)
    {
        var lowerQuery = query.ToLower();

        return await _context.Posts
            .Where(p => p.IsActive && p.EventDate >= DateTime.UtcNow)
            .Where(p => p.Title.ToLower().Contains(lowerQuery) ||
                        p.Description.ToLower().Contains(lowerQuery))
            .CountAsync();
    }
}
