

namespace FutaBuss.Model
{
    public class Payment
    {
        public Guid Id { get; set; }
        public DateTime PaidAt { get; set; }
        public string Platform { get; set; }
        public string Status { get; set; }
        public string TransactionCode { get; set; }
        
    }
}
