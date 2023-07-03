using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vPaymentApprovals
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int CompanyId { get; set; }
        public string PaymentId { get; set; }
        public string Bank { get; set; }
        public string PaymentUrl { get; set; }
        public string UserName { get; set; }
        public double? Amount { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public string CompanyName { get; set; }
        public double Quantity { get; set; }
        public string Quarter { get; set; }
        public string CrudeStream { get; set; }
    }
}
