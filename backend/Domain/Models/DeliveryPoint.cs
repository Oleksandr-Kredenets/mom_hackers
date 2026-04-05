namespace TMS.Domain.Models;

public class DeliveryPoint
{
    public DeliveryPoint()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; init; }
    public int Latitude { get; set; }
    public int Longitude { get; set; }
}