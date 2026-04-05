using TMS.Application.Models.GeoJson;
using TMS.Domain.Models;

namespace TMS.Application.Interfaces;

public interface IRoutePlanService
{
    Task<GeoJsonFeatureCollection> BuildGeoJsonAsync(
        RoutePlanRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}
