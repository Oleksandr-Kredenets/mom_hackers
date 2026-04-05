using TMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace TMS.Domain.Models;

public class RoutePoint
{
    // public RoutePoint(Guid routeId, int sequence, RoutePointType type, double latitude, double longitude)
    // {
    //     Id = Guid.NewGuid();
    //     RouteId = routeId;
    //     Sequence = sequence;
    //     Type = type;
    //     Latitude = latitude;
    //     Longitude = longitude;
    // }
    [Key] public Guid Id { get; init; }
    // The id of the route
    public Guid RouteId { get; init; }
    // The index of the point in the route
    public int Sequence { get; set; }
    public RoutePointType Type { get; }
    public double Latitude { get; set;}
    public double Longitude { get; set;}
}