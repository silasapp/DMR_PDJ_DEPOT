using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Reports
    {
        public int ReportId { get; set; }
        public int? AppId { get; set; }
        public int? StaffId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string StaffEmail { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string DocSource { get; set; }
        public bool? DeletedStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? JointAccountId { get; set; }
    }
}
