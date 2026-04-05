using TMS.Domain.Enums;
namespace TMS.Domain.Models;
using System.ComponentModel.DataAnnotations;

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
    public RoutePoint()
    {
        Id = Guid.NewGuid();
        RouteId = Guid.NewGuid();
    }
    [Key] public Guid Id { get; init; }
    public Guid RouteId { get; init; }
    public int Sequence { get; set; }
    public RoutePointType Type { get; }
    public double Latitude { get; set;}
    public double Longitude { get; set;}
}