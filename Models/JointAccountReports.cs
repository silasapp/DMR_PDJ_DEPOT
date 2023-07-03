using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class JointAccountReports
    {
        public int Id { get; set; }
        public int JointAccountId { get; set; }
        public string Report { get; set; }
        public string Reportby { get; set; }
        public DateTime? ReportDate { get; set; }
        public int? ApplicationId { get; set; }
    }
}
