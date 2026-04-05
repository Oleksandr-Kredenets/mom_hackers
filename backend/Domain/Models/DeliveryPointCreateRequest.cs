namespace TMS.Domain.Models;

/// <summary>Body for creating a delivery point. Add optional properties as the domain grows.</summary>
public sealed class DeliveryPointCreateRequest
{
    public int Latitude { get; init; }
    public int Longitude { get; init; }
}
