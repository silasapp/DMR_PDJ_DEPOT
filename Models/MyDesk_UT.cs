using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class MyDesk_UT
    {
        public int DeskID { get; set; }
        public int ProcessID { get; set; }
        public int RequestID { get; set; }
        public int StaffID { get; set; }
        public int Sort { get; set; }
        public bool HasWork { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? AutoPushed { get; set; }
        public bool HasPushed { get; set; }
        public string Comment { get; set; }
        public int? FromStaffID { get; set; }
        public int? AppId { get; set; }
        public bool? Holding { get; set; }
    }
}
