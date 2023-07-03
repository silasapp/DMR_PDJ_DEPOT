using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class applications_UT
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public int category_id { get; set; }
        public int PhaseId { get; set; }
        public string type { get; set; }
        public int year { get; set; }
        public decimal? fee_payable { get; set; }
        public decimal? service_charge { get; set; }
        public decimal? TransferCost { get; set; }
        public int? payment_id { get; set; }
        public string status { get; set; }
        public string reference { get; set; }
        public int? current_desk { get; set; }
        public DateTime date_added { get; set; }
        public DateTime? date_modified { get; set; }
        public bool? submitted { get; set; }
        public string current_Permit { get; set; }
        public bool? Migrated { get; set; }
        public int? FacilityId { get; set; }
        public bool? AllowPush { get; set; }
        public Guid? ValGroupId { get; set; }
        public string PaymentDescription { get; set; }
        public bool? AppProcessed { get; set; }
        public bool? SupervisorProcessed { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeleteStatus { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string LastAssignedUser { get; set; }
        public bool? isLegacy { get; set; }
    }
}
