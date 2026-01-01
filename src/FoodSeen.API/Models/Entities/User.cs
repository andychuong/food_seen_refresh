namespace FoodSeen.API.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool EmailConfirmed { get; set; }
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
    public string? ProviderId { get; set; }
    public double? DefaultLatitude { get; set; }
    public double? DefaultLongitude { get; set; }
    public int DefaultRadiusKm { get; set; } = 10;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public enum AuthProvider
{
    Local,
    Google,
    Facebook
}
