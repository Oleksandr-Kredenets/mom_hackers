using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TMS.Application.Models.GeoJson;

public sealed class GeoJsonFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "FeatureCollection";

    [JsonPropertyName("features")]
    public IReadOnlyList<GeoJsonFeature> Features { get; init; } = [];
}

public sealed class GeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "Feature";

    [JsonPropertyName("geometry")]
    public object Geometry { get; init; } = null!;

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonNode>? Properties { get; init; }
}

public sealed class GeoJsonLineStringGeometry
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "LineString";

    /// <summary>Each pair is [longitude, latitude] per RFC 7946.</summary>
    [JsonPropertyName("coordinates")]
    public IReadOnlyList<double[]> Coordinates { get; init; } = [];
}

public sealed class GeoJsonPointGeometry
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "Point";

    /// <summary>[longitude, latitude]</summary>
    [JsonPropertyName("coordinates")]
    public double[] Coordinates { get; init; } = [];
}
