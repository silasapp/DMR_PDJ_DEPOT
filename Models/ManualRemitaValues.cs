using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ManualRemitaValues
    {
        public int Id { get; set; }
        public string RRR { get; set; }
        public string Company { get; set; }
        public string Beneficiary { get; set; }
        public string FundingBank { get; set; }
        public string PaymentSource { get; set; }
        public double? NetAmount { get; set; }
        public string Status { get; set; }
        public DateTime DateAdded { get; set; }
        public string AddedBy { get; set; }
    }
}
