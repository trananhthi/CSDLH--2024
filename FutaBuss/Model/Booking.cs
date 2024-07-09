namespace FutaBuss.Model
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TripId { get; set; }
        public Guid PickUpLocationId { get; set; }
        public Guid DropOffLocationId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
