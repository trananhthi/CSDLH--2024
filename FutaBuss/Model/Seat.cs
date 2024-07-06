using MongoDB.Bson.Serialization.Attributes;

namespace FutaBuss.Model
{
    public class Seat
    {
        [BsonElement("id")]
        public string SeatId { get; set; }

        [BsonElement("alias")]
        public string Alias { get; set; }

        [BsonElement("position")]
        public List<int> Position { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }
    }
}
