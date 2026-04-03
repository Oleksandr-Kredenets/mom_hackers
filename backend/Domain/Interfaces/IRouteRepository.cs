using TMS.Domain.Models;
namespace TMS.Domain.Interfaces;
public interface IRouteRepository
{
    Task<Route> GetRouteByIdAsync(Guid id);
    Task<IEnumerable<Route>> GetAllRoutesAsync();
    Task AddRouteAsync(Route route, IEnumerable<RoutePoint> points);
    Task DeleteRouteAsync(Guid id);
}