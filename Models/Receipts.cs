using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Receipts
    {
        public long id { get; set; }
        public string receiptno { get; set; }
        public int applicationid { get; set; }
        public int invoiceid { get; set; }
        public string companyname { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
        public string applicationreference { get; set; }
        public string RRR { get; set; }
        public DateTime? date_paid { get; set; }
    }
}
