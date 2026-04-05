using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using Web.Helpers;

namespace Web.Controllers;

[ApiController]
[Route("api/warehouse")]
[Authorize]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    /// <summary>All warehouses for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouses(CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        var list = await _warehouseRepository
            .GetWarehousesForUserAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);
        return Ok(list);
    }

    /// <summary>Creates a warehouse for the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WarehouseListItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateWarehouse(
        [FromBody] WarehouseCreateRequest? body,
        CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();
        if (body is null)
            return BadRequest(new { error = "Request body is required." });

        var warehouse = new Warehouse
        {
            UserId = user.Id,
            Latitude = body.Latitude,
            Longitude = body.Longitude,
        };

        await _warehouseRepository.AddWarehouseAsync(warehouse, cancellationToken).ConfigureAwait(false);

        var item = new WarehouseListItem
        {
            Id = warehouse.Id,
            Latitude = warehouse.Latitude,
            Longitude = warehouse.Longitude,
        };
        return Created($"{Request.Path}/{item.Id}", item);
    }

    /// <summary>Deletes a warehouse owned by the current user.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteWarehouse(Guid id, CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();

        var deleted = await _warehouseRepository
            .TryDeleteWarehouseForUserAsync(id, user.Id, cancellationToken)
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
    public async Task<IActionResult> UpdateWarehouse(
        Guid id,
        [FromBody] WarehouseUpdateRequest? body,
        CancellationToken cancellationToken)
    {
        if (!AuthClaims.TryGetAuthUser(User, out var user) || user is null)
            return Unauthorized();
        if (body is null)
            return BadRequest(new { error = "Request body is required." });
        if (body.Latitude is null && body.Longitude is null)
            return BadRequest(new { error = "Provide latitude and/or longitude." });

        var updated = await _warehouseRepository
            .TryUpdateWarehouseForUserAsync(id, user.Id, body.Latitude, body.Longitude, cancellationToken)
            .ConfigureAwait(false);
        if (!updated)
            return NotFound();
        return NoContent();
    }
}
