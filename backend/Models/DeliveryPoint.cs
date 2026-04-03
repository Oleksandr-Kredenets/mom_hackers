namespace TMS.Models
{
    public class DeliveryPoint
    {
        DeliveryPoint()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get ;}
        public int Latitude { get; set; }
        public int Longitude { get; set; }
    }
}