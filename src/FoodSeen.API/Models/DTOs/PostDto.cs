namespace FoodSeen.API.Models.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? DistanceKm { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public bool IsActive { get; set; }
    public List<string> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
