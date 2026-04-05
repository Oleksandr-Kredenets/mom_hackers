using TMS.Domain.Enums;
using TMS.Domain.Models;

namespace TMS.Application.Validation;

public static class RoutePlanPointSequenceValidator
{
    /// <summary>Orders by sequence then validates start/end and middle waypoint rules.</summary>
    public static IReadOnlyList<RoutePlanPoint> OrderAndValidate(IEnumerable<RoutePlanPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        var ordered = points.OrderBy(p => p.Sequence).ToList();
        if (ordered.Count < 2)
            throw new ArgumentException("At least two points are required.", nameof(points));

        ValidateOrdered(ordered);

        var waypoints = ordered.Skip(1).Take(ordered.Count - 2).ToList();
        foreach (var w in waypoints)
        {
            if (w.Type != RoutePointType.Waypoint)
                throw new ArgumentException("All points between Start and End must be Waypoints.");
        }

        return ordered;
    }

    public static void ValidateOrdered(IReadOnlyList<RoutePlanPoint> ordered)
    {
        var starts = ordered.Count(p => p.Type == RoutePointType.Start);
        var ends = ordered.Count(p => p.Type == RoutePointType.End);
        if (starts != 1 || ends != 1)
            throw new ArgumentException("Exactly one Start and one End point are required.");

        if (ordered[0].Type != RoutePointType.Start || ordered[^1].Type != RoutePointType.End)
            throw new ArgumentException("The first point must be Start and the last must be End after ordering by sequence.");
    }
}
