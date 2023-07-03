using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class invoices
    {
        public long id { get; set; }
        public int application_id { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
        public string payment_code { get; set; }
        public string payment_type { get; set; }
        public string receipt_no { get; set; }
        public DateTime? date_added { get; set; }
        public DateTime? date_paid { get; set; }
        public int? PaymentTransaction_Id { get; set; }
    }
}
