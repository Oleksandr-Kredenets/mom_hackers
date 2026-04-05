using System.ComponentModel.DataAnnotations;
using TMS.Domain.Enums;

namespace TMS.Domain.Models;

public class RoutePoint
{
    [Key] public Guid Id { get; init; }
    public Guid RouteId { get; init; }
    public int Sequence { get; set; }
    public RoutePointType Type { get; init; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}