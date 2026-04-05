namespace TMS.Domain.Models;

/// <summary>Partial update; send at least one coordinate field.</summary>
public sealed class DeliveryPointUpdateRequest
{
    public int? Latitude { get; init; }
    public int? Longitude { get; init; }
}
