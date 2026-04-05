namespace TMS.Application.Models;

/// <param name="Latitude">WGS84 latitude in degrees.</param>
/// <param name="Longitude">WGS84 longitude in degrees.</param>
public readonly record struct GeoCoordinate(double Latitude, double Longitude);
