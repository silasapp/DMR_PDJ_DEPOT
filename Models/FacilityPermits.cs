using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class FacilityPermits
    {
        public string permit_no { get; set; }
        public DateTime date_issued { get; set; }
        public DateTime date_expire { get; set; }
        public string is_renewed { get; set; }
        public int? elps_id { get; set; }
        public int id { get; set; }
        public int CompanyId { get; set; }
        public string type { get; set; }
        public string FacilityName { get; set; }
        public string PhaseName { get; set; }
        public int PhaseId { get; set; }
        public string companyName { get; set; }
        public string CategoryName { get; set; }
        public int application_id { get; set; }
        public int category_id { get; set; }
        public int? FacilityId { get; set; }
    }
}
