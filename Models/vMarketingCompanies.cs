using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vMarketingCompanies
    {
        public int Id { get; set; }
        public int SponsorId { get; set; }
        public int CompanyId { get; set; }
        public int FacilityId { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime Date { get; set; }
        public string FacilityName { get; set; }
        public string CompanyName { get; set; }
        public string Reason { get; set; }
    }
}
