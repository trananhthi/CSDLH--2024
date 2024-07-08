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

        [BsonElement("is_sold")]
        public bool? IsSold { get; set; }

        [BsonElement("row_group")]
        public string RowGroup { get; set; }
    }
}
