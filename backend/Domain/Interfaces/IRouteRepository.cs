namespace TMS.Domain.Interfaces;

public interface IRouteRepository
{
    Task<Models.Route> GetRouteByIdAsync(Guid id);
    Task<IEnumerable<Models.Route>> GetAllRoutesAsync();
    Task AddRouteAsync(Models.Route route, IEnumerable<Models.RoutePoint> points);
    Task DeleteRouteAsync(Guid id);
}