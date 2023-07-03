using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ApplicationProccess
    {
        public int ProccessID { get; set; }
        public string Division { get; set; }
        public int RoleID { get; set; }
        public int Sort { get; set; }
        public int LocationID { get; set; }
        public bool canPush { get; set; }
        public bool canWork { get; set; }
        public bool canInspect { get; set; }
        public bool canSchdule { get; set; }
        public bool canReport { get; set; }
        public bool canAccept { get; set; }
        public bool canReject { get; set; }
        public int onAcceptRoleID { get; set; }
        public int onRejectRoleID { get; set; }
        public int? onpushRoleID { get; set; }
        public string Process { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? canRequest { get; set; }
    }
}
