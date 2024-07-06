using MongoDB.Bson.Serialization.Attributes;

namespace FutaBuss.Model
{
    public class OrderInfo
    {
        [BsonElement("license_plate")]
        public string LicensePlate { get; set; }

        [BsonElement("driver_citizen_id")]
        public string DriverCitizenId { get; set; }

        [BsonElement("driver_name")]
        public string DriverName { get; set; }
    }
}
