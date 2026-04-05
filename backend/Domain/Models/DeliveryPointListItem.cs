namespace TMS.Domain.Models;

/// <summary>API projection for a delivery point; extend with new init-only properties as needed.</summary>
public sealed class DeliveryPointListItem
{
    public required Guid Id { get; init; }
    public int Latitude { get; init; }
    public int Longitude { get; init; }
}
