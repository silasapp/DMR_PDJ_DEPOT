using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Schedules
    {
        public long ID { get; set; }
        public long EmployerID { get; set; }
        public int AccID { get; set; }
        public int? Year { get; set; }
        public string Month { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public string fileUrl { get; set; }
    }
}
