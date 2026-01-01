using System.ComponentModel.DataAnnotations;

namespace FoodSeen.API.Models.Requests;

public class UpdateUserRequest
{
    [MinLength(3)]
    [MaxLength(100)]
    public string? Username { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [Range(-90, 90)]
    public double? DefaultLatitude { get; set; }

    [Range(-180, 180)]
    public double? DefaultLongitude { get; set; }

    [Range(1, 100)]
    public int? DefaultRadiusKm { get; set; }
}
