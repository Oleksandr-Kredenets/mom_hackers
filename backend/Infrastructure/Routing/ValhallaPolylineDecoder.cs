namespace TMS.Infrastructure.Routing;

/// <summary>Decodes Valhalla / Google encoded polylines (default precision 6).</summary>
internal static class ValhallaPolylineDecoder
{
    public static List<(double Lat, double Lon)> Decode(string encoded, int precision = 6)
    {
        var coordinates = new List<(double Lat, double Lon)>();
        if (string.IsNullOrEmpty(encoded))
            return coordinates;

        var factor = Math.Pow(10, precision);
        var index = 0;
        var lat = 0;
        var lng = 0;
        while (index < encoded.Length)
        {
            lat += ReadChunk(encoded, ref index);
            lng += ReadChunk(encoded, ref index);
            coordinates.Add((lat / factor, lng / factor));
        }

        return coordinates;
    }

    private static int ReadChunk(string encoded, ref int index)
    {
        int result = 0, shift = 0, b;
        do
        {
            b = encoded[index++] - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);

        return (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
    }
}
