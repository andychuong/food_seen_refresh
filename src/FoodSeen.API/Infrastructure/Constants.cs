namespace FoodSeen.API.Infrastructure;

/// <summary>
/// Geospatial constants for PostGIS/NetTopologySuite operations.
/// </summary>
public static class GeoConstants
{
    /// <summary>
    /// WGS84 Spatial Reference System Identifier.
    /// This is the standard coordinate system used by GPS devices worldwide.
    /// Coordinates are expressed as (longitude, latitude) in degrees.
    /// </summary>
    public const int WGS84_SRID = 4326;
}

/// <summary>
/// API-wide constants for pagination, validation, and defaults.
/// Centralizes magic numbers to improve maintainability.
/// </summary>
public static class ApiConstants
{
    // Pagination defaults
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MinPageSize = 1;

    // Geospatial defaults
    public const double DefaultRadiusKm = 10.0;
    public const double MaxRadiusKm = 100.0;
    public const double MinRadiusKm = 0.1;

    // Coordinate validation bounds (WGS84)
    public const double MinLatitude = -90.0;
    public const double MaxLatitude = 90.0;
    public const double MinLongitude = -180.0;
    public const double MaxLongitude = 180.0;
}
