using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vPermits
    {
        public int id { get; set; }
        public string permit_no { get; set; }
        public string CompanyName { get; set; }
        public int application_id { get; set; }
        public DateTime date_issued { get; set; }
        public DateTime date_expire { get; set; }
        public int company_id { get; set; }
        public decimal? Fees { get; set; }
        public string CategoryName { get; set; }
        public int? elps_id { get; set; }
        public string OrderId { get; set; }
        public int PhaseId { get; set; }
        public string PhaseName { get; set; }
        public int? FacilityId { get; set; }
        public int category_id { get; set; }
        public string is_renewed { get; set; }
        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public bool Printed { get; set; }
        public string StateName { get; set; }
        public string IssueType { get; set; }
    }
}
