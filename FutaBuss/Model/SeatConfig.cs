using MongoDB.Bson.Serialization.Attributes;

namespace FutaBuss.Model
{
    public class SeatConfig
    {
        [BsonElement("floors")]
        public List<Floor> Floors { get; set; } = new List<Floor>();
    }
}
