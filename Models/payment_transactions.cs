using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class payment_transactions
    {
        public int id { get; set; }
        public string type { get; set; }
        public string transaction_date { get; set; }
        public string reference_number { get; set; }
        public string online_reference { get; set; }
        public string payment_reference { get; set; }
        public string approved_amount { get; set; }
        public string response_description { get; set; }
        public string response_code { get; set; }
        public string transaction_amount { get; set; }
        public string transaction_currency { get; set; }
        public string customer_name { get; set; }
        public int customer_id { get; set; }
        public string order_id { get; set; }
        public string payment_log_id { get; set; }
        public DateTime query_date { get; set; }
        public string Webpay_Reference { get; set; }
        public string RRR { get; set; }
        public string PaymentSource { get; set; }
    }
}
