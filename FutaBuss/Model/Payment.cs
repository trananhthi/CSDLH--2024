using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutaBuss.Model
{
    class Payment
    {
        public Guid Id { get; set; }
        public DateTime PaidAt { get; set; }
        public string Platform { get; set; }
        public string Status { get; set; }
        public string TransactionCode { get; set; }
        
    }
}
