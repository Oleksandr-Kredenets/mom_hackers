using System.Net.Http.Json;
using System.Text.Json;
using TMS.Application.Interfaces;
using TMS.Application.Models;

namespace TMS.Infrastructure.Routing;

public sealed class ValhallaClient : IValhallaClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;

    public ValhallaClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<RoutingMatrices> GetMatricesAsync(
        IReadOnlyList<GeoCoordinate> locations,
        CancellationToken cancellationToken = default)
    {
        if (locations.Count == 0)
            throw new ArgumentException("At least one location is required for the matrix.", nameof(locations));

        var locObjects = locations.Select(l => new Dictionary<string, double>
        {
            ["lat"] = l.Latitude,
            ["lon"] = l.Longitude,
        }).ToList();

        var payload = new Dictionary<string, object?>
        {
            ["sources"] = locObjects,
            ["targets"] = locObjects,
            ["costing"] = "auto",
            ["units"] = "kilometers",
            ["verbose"] = false,
        };

        using var response = await _http.PostAsJsonAsync("sources_to_targets", payload, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Valhalla matrix HTTP {(int)response.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.TryGetProperty("error", out var err))
            throw new InvalidOperationException($"Valhalla matrix error: {err}");

        if (!TryGetMatrixArrays(root, out var durationsEl, out var distancesEl))
            throw new InvalidOperationException("Valhalla matrix response missing durations/distances.");

        var n = locations.Count;
        var durationSeconds = new long?[n, n];
        var distanceMeters = new long?[n, n];
        FillMatrix(durationsEl, durationSeconds, convertToSeconds: true);
        FillMatrix(distancesEl, distanceMeters, convertToSeconds: false);
        return new RoutingMatrices { DurationSeconds = durationSeconds, DistanceMeters = distanceMeters };
    }

    private static bool TryGetMatrixArrays(
        JsonElement root,
        out JsonElement durations,
        out JsonElement distances)
    {
        JsonElement bucket = root;
        if (root.TryGetProperty("sources_to_targets", out var st))
            bucket = st;

        if (bucket.TryGetProperty("durations", out durations)
            && bucket.TryGetProperty("distances", out distances))
            return true;

        durations = default;
        distances = default;
        return false;
    }

    private static void FillMatrix(JsonElement matrixEl, long?[,] target, bool convertToSeconds)
    {
        var rows = matrixEl.GetArrayLength();
        for (var i = 0; i < rows; i++)
        {
            var row = matrixEl[i];
            var cols = row.GetArrayLength();
            for (var j = 0; j < cols; j++)
            {
                var cell = row[j];
                if (cell.ValueKind == JsonValueKind.Null)
                {
                    target[i, j] = null;
                    continue;
                }

                if (convertToSeconds)
                    target[i, j] = (long)Math.Round(cell.GetDouble());
                else
                {
                    // Valhalla returns kilometers when units = kilometers
                    var km = cell.GetDouble();
                    target[i, j] = (long)Math.Round(km * 1000.0);
                }
            }
        }
    }

    public async Task<IReadOnlyList<double[]>> GetRouteShapeLonLatAsync(
        IReadOnlyList<GeoCoordinate> orderedStops,
        CancellationToken cancellationToken = default)
    {
        if (orderedStops.Count < 2)
            throw new ArgumentException("At least two stops are required for a route.", nameof(orderedStops));

        var locObjects = orderedStops.Select(l => new Dictionary<string, double>
        {
            ["lat"] = l.Latitude,
            ["lon"] = l.Longitude,
        }).ToList();

        var payload = new Dictionary<string, object?>
        {
            ["locations"] = locObjects,
            ["costing"] = "auto",
            ["units"] = "kilometers",
        };

        using var response = await _http.PostAsJsonAsync("route", payload, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Valhalla route HTTP {(int)response.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.TryGetProperty("error", out var err))
            throw new InvalidOperationException($"Valhalla route error: {err}");

        if (!root.TryGetProperty("trip", out var trip))
            throw new InvalidOperationException("Valhalla route response missing trip.");

        if (!trip.TryGetProperty("legs", out var legs) || legs.GetArrayLength() == 0)
            throw new InvalidOperationException("Valhalla route trip has no legs.");

        var merged = new List<double[]>();
        foreach (var leg in legs.EnumerateArray())
        {
            if (!leg.TryGetProperty("shape", out var shapeEl))
                continue;
            var encoded = shapeEl.GetString();
            if (string.IsNullOrEmpty(encoded))
                continue;

            var decoded = ValhallaPolylineDecoder.Decode(encoded);
            foreach (var (lat, lon) in decoded)
            {
                var pt = new[] { lon, lat };
                if (merged.Count > 0 && merged[^1][0] == pt[0] && merged[^1][1] == pt[1])
                    continue;
                merged.Add(pt);
            }
        }

        if (merged.Count < 2)
            throw new InvalidOperationException("Valhalla returned insufficient shape geometry.");

        return merged;
    }
}
