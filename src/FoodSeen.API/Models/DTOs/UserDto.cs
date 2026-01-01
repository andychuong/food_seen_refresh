namespace FoodSeen.API.Models.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public double? DefaultLatitude { get; set; }
    public double? DefaultLongitude { get; set; }
    public int DefaultRadiusKm { get; set; }
    public DateTime CreatedAt { get; set; }
}
