using MongoDB.Bson.Serialization.Attributes;

namespace FutaBuss.Model
{
    public class Floor
    {
        [BsonElement("ordinal")]
        public int Ordinal { get; set; }

        [BsonElement("floor_name")]
        public string FloorName { get; set; }

        [BsonElement("num_rows")]
        public int NumRows { get; set; }

        [BsonElement("num_cols")]
        public int NumCols { get; set; }

        [BsonElement("seats")]
        public List<Seat> Seats { get; set; }
    }
}
