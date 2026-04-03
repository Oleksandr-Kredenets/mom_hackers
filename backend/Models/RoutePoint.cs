namespace TMS.Models
{
    public class RoutePoint
    {
        public RoutePoint(Guid routeId, int sequence, string type, double latitude, double longitude)
        {
            Id = Guid.NewGuid();
            RouteId = routeId;
            Sequence = sequence;
            Type = type;
            Latitude = latitude;
            Longitude = longitude;
        }
        public Guid Id { get; }
        public Guid RouteId { get; }
        public int Sequence { get; set; }
        public string Type { get; }
        public double Latitude { get; set;}
        public double Longitude { get; set;}
    }
}