using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FutaBuss.Model
{
    public class Trip
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("id")]
        public string TripId { get; set; }

        [BsonElement("departure_date")]
        public DateTime DepartureDate { get; set; }

        [BsonElement("trip_type")]
        public string TripType { get; set; }

        [BsonElement("departure_province_code")]
        public string DepartureProvinceCode { get; set; }

        [BsonElement("destination_province_code")]
        public string DestinationProvinceCode { get; set; }

        [BsonElement("departure_time")]
        public TimeSpan DepartureTime { get; set; }

        [BsonElement("price")]
        public int Price { get; set; }

        [BsonElement("time_zone")]
        public string TimeZone { get; set; }

        [BsonElement("expected_arrival_time")]
        public TimeSpan ExpectedArrivalTime { get; set; }

        [BsonElement("policies")]
        public List<string> Policies { get; set; }

        [BsonElement("other_info")]
        public OrderInfo OtherInfo { get; set; } = new OrderInfo();

        [BsonElement("seat_config")]
        public SeatConfig SeatConfig { get; set; } = new SeatConfig();

        [BsonElement("transhipments")]
        public Transhipments Transhipments { get; set; } = new Transhipments();

        [BsonElement("arrival_date")]
        public DateTime ArrivalDate { get; set; }

    }
}
