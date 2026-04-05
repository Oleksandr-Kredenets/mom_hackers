using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Interfaces;
using TMS.Application.Models.GeoJson;
using TMS.Domain.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/route")]
[Authorize]
public class RouteController : ControllerBase
{
    private readonly IRoutePlanService _routePlanService;

    public RouteController(IRoutePlanService routePlanService)
    {
        _routePlanService = routePlanService;
    }

    /// <summary>Builds a GeoJSON FeatureCollection for the ordered route and vehicle markers.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(GeoJsonFeatureCollection), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostRoutePlan(
        [FromBody] RoutePlanRequest body,
        CancellationToken cancellationToken)
    {
        try
        {
            var geoJson = await _routePlanService.BuildGeoJsonAsync(body, cancellationToken).ConfigureAwait(false);
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
    }
}
