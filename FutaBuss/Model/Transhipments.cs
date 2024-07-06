using MongoDB.Bson.Serialization.Attributes;

namespace FutaBuss.Model
{
    public class Transhipments
    {
        [BsonElement("pick_up")]
        public List<TranshipmentDetail> PickUp { get; set; } = new List<TranshipmentDetail>();

        [BsonElement("drop_off")]
        public List<TranshipmentDetail> DropOff { get; set; } = new List<TranshipmentDetail> { };
    }
}
