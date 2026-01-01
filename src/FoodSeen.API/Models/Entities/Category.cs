namespace FoodSeen.API.Models.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
