using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public partial class ApplicationExtraPayments
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string RRR { get; set; }
        public string Reference { get; set; }
        public string Comment { get; set; }
        public DateTime? Date { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string DatePaid { get; set; }
    }
}
