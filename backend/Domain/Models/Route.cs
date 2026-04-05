namespace TMS.Domain.Models;

public class Route
{
    public Route()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; init; }
}