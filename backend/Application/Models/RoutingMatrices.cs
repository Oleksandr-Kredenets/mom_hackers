namespace TMS.Application.Models;

/// <summary>Pairwise travel metrics from Valhalla (nullable = no path).</summary>
public sealed class RoutingMatrices
{
    /// <summary>Seconds between nodes; symmetric indexing [i,j].</summary>
    public required long?[,] DurationSeconds { get; init; }

    /// <summary>Meters between nodes.</summary>
    public required long?[,] DistanceMeters { get; init; }
}
