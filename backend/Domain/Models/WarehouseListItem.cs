namespace TMS.Domain.Models;

/// <summary>API projection for a warehouse; extend with new init-only properties as needed.</summary>
public sealed class WarehouseListItem
{
    public required Guid Id { get; init; }
    public int Latitude { get; init; }
    public int Longitude { get; init; }
}
