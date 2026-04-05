using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Exceptions;
using TMS.Application.Interfaces;
using TMS.Application.Mappers;
using TMS.Application.Models.GeoJson;
using TMS.Application.Validation;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using Web.Helpers;

namespace Web.Controllers;

[ApiController]
[Route("api/routes")]
[Authorize]
public class RouteController : ControllerBase
{
    private readonly IRoutePlanService _routePlanService;
    private readonly IRouteRepository _routeRepository;

    public RouteController(IRoutePlanService routePlanService, IRouteRepository routeRepository)
    {
        _routePlanService = routePlanService;
        _routeRepository = routeRepository;
    }

    /// <summary>Active routes for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RouteListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveRoutes(CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        var routes = await _routeRepository
            .GetActiveRoutesForUserAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);
        return Ok(routes);
    }

    /// <summary>Single route with stops (must belong to the current user).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RouteDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRouteById(Guid id, CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        var detail = await _routeRepository
            .GetRouteDetailForUserAsync(id, user.Id, cancellationToken)
            .ConfigureAwait(false);
        if (detail is null)
            return NotFound();
        return Ok(detail);
    }

    /// <summary>Deletes a route owned by the current user.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteRoute(Guid id, CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        var deleted = await _routeRepository
            .TryDeleteRouteForUserAsync(id, user.Id, cancellationToken)
            .ConfigureAwait(false);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>Updates <c>isActive</c> and/or replaces all stops. Send at least one field.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateRoute(
        Guid id,
        [FromBody] RouteUpdateRequest? body,
        CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();
        if (body is null)
            return BadRequest(new { error = "Request body is required." });
        if (body.IsActive is null && body.Points is null)
            return BadRequest(new { error = "Provide isActive and/or points." });

        IReadOnlyList<RoutePoint>? replacement = null;
        if (body.Points is not null)
        {
            if (body.Points.Count < 2)
                return BadRequest(new { error = "At least two points are required when replacing points." });
            try
            {
                var ordered = RoutePlanPointSequenceValidator.OrderAndValidate(body.Points);
                replacement = RoutePointMapper.FromOrderedPlanPoints(id, ordered);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        var updated = await _routeRepository
            .TryUpdateRouteForUserAsync(id, user.Id, body.IsActive, replacement, cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    /// <summary>Builds a GeoJSON FeatureCollection; persists each computed vehicle route for the current user.</summary>
    [ProducesResponseType(typeof(GeoJsonFeatureCollection), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> PostRouteCalculate(
        [FromBody] RoutePlanRequest body,
        CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        try
        {
            var geoJson = await _routePlanService
                .BuildGeoJsonAsync(body, user.Id, cancellationToken)
                .ConfigureAwait(false);
            return Ok(geoJson);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ValhallaUnavailableException ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { error = ex.Message, code = "routing_engine_unavailable" });
        }
    }
}
