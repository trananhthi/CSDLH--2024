using MongoDB.Bson.Serialization.Attributes;

namespace FutaBuss.Model
{
    [BsonNoId]
    public class TranshipmentDetail
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        [BsonElement("time")]
        public TimeSpan Time { get; set; }

        [BsonElement("detail")]
        public string Detail { get; set; }
    }
}
