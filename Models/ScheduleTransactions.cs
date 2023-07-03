using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ScheduleTransactions
    {
        public long ID { get; set; }
        public long EmployeeID { get; set; }
        public long ScheduleID { get; set; }
        public string ContributionType { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? EmployerAmount { get; set; }
        public decimal? EmployeeAmount { get; set; }
        public DateTime Date { get; set; }
    }
}
