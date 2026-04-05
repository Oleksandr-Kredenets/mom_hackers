using TMS.Domain.Models;

namespace TMS.Application.Mappers;

public static class RoutePointMapper
{
    public static List<RoutePoint> FromOrderedPlanPoints(Guid routeId, IReadOnlyList<RoutePlanPoint> ordered)
    {
        var list = new List<RoutePoint>(ordered.Count);
        for (var i = 0; i < ordered.Count; i++)
        {
            var p = ordered[i];
            list.Add(new RoutePoint
            {
                Id = Guid.NewGuid(),
                RouteId = routeId,
                Sequence = i,
                Type = p.Type,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
            });
        }

        return list;
    }
}
