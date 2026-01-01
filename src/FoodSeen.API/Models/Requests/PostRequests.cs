using System.ComponentModel.DataAnnotations;

namespace FoodSeen.API.Models.Requests;

public class CreatePostRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    public DateTime? EventEndDate { get; set; }

    public List<Guid>? CategoryIds { get; set; }
}

public class UpdatePostRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    public DateTime? EventDate { get; set; }

    public DateTime? EventEndDate { get; set; }

    public bool? IsActive { get; set; }

    public List<Guid>? CategoryIds { get; set; }
}
