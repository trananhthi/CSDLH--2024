namespace FutaBuss.Model
{
    public class Ticket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public string TripId { get; set; }
        public Guid SeatId { get; set; }
        public Guid PickUpLocationId { get; set; }
        public Guid DropOffLocationId { get; set; }
        public Guid PaymentId { get; set; }
    }
}
