using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace NewDepot.Models
{
    public partial class WorkProccess
    {
        [NotMapped]
        public string PhaseName { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
        [NotMapped]
        public string LocationName { get; set; }

        public int ProccessID { get; set; }
        public int PhaseID { get; set; }
        public int CategoryID { get; set; }
        public int RoleID { get; set; }
        public int Sort { get; set; }
        public int LocationID { get; set; }
        public bool canPush { get; set; }
        public bool canWork { get; set; }
        public bool canInspect { get; set; }
        public bool? canPrint { get; set; }
        public bool canSchdule { get; set; }
        public bool canReport { get; set; }
        public bool canAccept { get; set; }
        public bool canReject { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string ModificationStage { get; set; }
    }
}
