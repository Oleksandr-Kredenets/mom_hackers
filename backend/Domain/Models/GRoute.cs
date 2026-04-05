namespace TMS.Domain.Models;

public class GRoute
{
    public GRoute()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; init; }
}