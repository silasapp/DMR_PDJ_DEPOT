using System;

namespace NewDepot.Models
{
    public interface IFacilityPermits
    {
        int application_id { get; set; }
        int category_id { get; set; }
        string CategoryName { get; set; }
        int CompanyId { get; set; }
        string companyName { get; set; }
        DateTime date_expire { get; set; }
        DateTime date_issued { get; set; }
        int? elps_id { get; set; }
        int? FacilityId { get; set; }
        string FacilityName { get; set; }
        int id { get; set; }
        string is_renewed { get; set; }
        string permit_no { get; set; }
        int PhaseId { get; set; }
        string PhaseName { get; set; }
        string type { get; set; }
    }
}