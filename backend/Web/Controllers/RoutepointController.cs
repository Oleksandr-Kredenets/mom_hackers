using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using Web.Helpers;

namespace Web.Controllers;

[ApiController]
[Route("api/routepoint")]
[Authorize]
public class RoutepointController : ControllerBase
{
    private readonly IDeliveryPointRepository _deliveryPointRepository;

    public RoutepointController(IDeliveryPointRepository deliveryPointRepository)
    {
        _deliveryPointRepository = deliveryPointRepository;
    }

    /// <summary>All delivery points for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DeliveryPointListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDeliveryPoints(CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        var list = await _deliveryPointRepository
            .GetDeliveryPointsForUserAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);
        return Ok(list);
    }

    /// <summary>Creates a delivery point for the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DeliveryPointListItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateDeliveryPoint(
        [FromBody] DeliveryPointCreateRequest? body,
        CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();
        if (body is null)
            return BadRequest(new { error = "Request body is required." });

        var point = new DeliveryPoint
        {
            UserId = user.Id,
            Latitude = body.Latitude,
            Longitude = body.Longitude,
        };

        await _deliveryPointRepository.AddDeliveryPointAsync(point, cancellationToken).ConfigureAwait(false);

        var item = new DeliveryPointListItem
        {
            Id = point.Id,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
        };
        return Created($"{Request.Path}/{item.Id}", item);
    }

    /// <summary>Deletes a delivery point owned by the current user.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteDeliveryPoint(Guid id, CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        var deleted = await _deliveryPointRepository
            .TryDeleteDeliveryPointForUserAsync(id, user.Id, cancellationToken)
            .ConfigureAwait(false);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    /// <summary>Updates coordinates; send at least one field.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateDeliveryPoint(
        Guid id,
        [FromBody] DeliveryPointUpdateRequest? body,
        CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();
        if (body is null)
            return BadRequest(new { error = "Request body is required." });
        if (body.Latitude is null && body.Longitude is null)
            return BadRequest(new { error = "Provide latitude and/or longitude." });

        var updated = await _deliveryPointRepository
            .TryUpdateDeliveryPointForUserAsync(id, user.Id, body.Latitude, body.Longitude, cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
            return NotFound();
        return NoContent();
    }
}
