namespace TMS.Domain.Models;

public class Route
{
    Route()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; }
}