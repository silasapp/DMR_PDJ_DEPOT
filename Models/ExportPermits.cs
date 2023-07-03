using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ExportPermits
    {
        public int Id { get; set; }
        public string Quarter { get; set; }
        public string Product { get; set; }
        public string Terminal { get; set; }
        public string Classification { get; set; }
        public double Quantity { get; set; }
        public decimal ProductAmount { get; set; }
        public int ApplicationId { get; set; }
        public DateTime Date { get; set; }
        public string CrudeStream { get; set; }
        public int? CompanyId { get; set; }
    }
}
