namespace FutaBuss.Model
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TripId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
