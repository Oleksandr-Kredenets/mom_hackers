namespace TMS.Domain.Models;

/// <summary>Body for creating a warehouse. Add optional properties as the domain grows.</summary>
public sealed class WarehouseCreateRequest
{
    public int Latitude { get; init; }
    public int Longitude { get; init; }
}
