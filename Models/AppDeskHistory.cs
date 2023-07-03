using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class AppDeskHistory
    {
        public int HistoryID { get; set; }
        public int RequestID { get; set; }
        public int StaffID { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
