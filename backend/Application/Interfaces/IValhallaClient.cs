using TMS.Application.Models;

namespace TMS.Application.Interfaces;

public interface IValhallaClient
{
    Task<RoutingMatrices> GetMatricesAsync(
        IReadOnlyList<GeoCoordinate> locations,
        CancellationToken cancellationToken = default);

    /// <summary>Ordered stops; returns merged route as GeoJSON-ready [lon, lat] vertices.</summary>
    Task<IReadOnlyList<double[]>> GetRouteShapeLonLatAsync(
        IReadOnlyList<GeoCoordinate> orderedStops,
        CancellationToken cancellationToken = default);
}
