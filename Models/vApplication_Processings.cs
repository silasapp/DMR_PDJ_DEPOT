using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vApplication_Processings
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int BranchId { get; set; }
        public string ProcessingLocation { get; set; }
        public bool Assigned { get; set; }
        public int ApplicationId { get; set; }
        public bool? Processed { get; set; }
        public int SortOrder { get; set; }
        public DateTime? DateProcessed { get; set; }
        public int? ProcessingRule_Id { get; set; }
        public int? processor { get; set; }
        public int company_id { get; set; }
        public string type { get; set; }
        public DateTime date_added { get; set; }
        public string CategoryName { get; set; }
        public string companyName { get; set; }
        public int year { get; set; }
        public bool? Holding { get; set; }
        public string FacilityName { get; set; }
        public int? FacilityId { get; set; }
        public string PhaseName { get; set; }
        public bool? AllowPush { get; set; }
        public string reference { get; set; }
        public string Role { get; set; }
        public int PhaseId { get; set; }
    }
}
