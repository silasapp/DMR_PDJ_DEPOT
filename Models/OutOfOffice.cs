using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class OutOfOffice
    {
        public int OutID { get; set; }
        public int StaffID { get; set; }
        public int ReliverID { get; set; }
        public string Comment { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public string Status { get; set; }
        public bool? DeletedStatus { get; set; }
        public bool? Approved { get; set; }
        public string ApprovedBy { get; set; }
        public string ApproverRole { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApproverComment { get; set; }
    }
}
