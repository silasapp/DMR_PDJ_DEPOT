using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vApplicationForms
    {
        public int Id { get; set; }
        public string FormId { get; set; }
        public int ApplicationId { get; set; }
        public bool Filled { get; set; }
        public string ExtraReport2 { get; set; }
        public string ExtraReport1 { get; set; }
        public string StaffName { get; set; }
        public Guid? ValGroupId { get; set; }
        public string ManagerReason { get; set; }
        public bool? ManagerAccept { get; set; }
        public int? DepartmentId { get; set; }
        public bool? WaiverApproved { get; set; }
        public bool? WaiverRequest { get; set; }
        public string FormTitle { get; set; }
        public string Reasons { get; set; }
        public bool? Recommend { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime Date { get; set; }
        public bool Confirmed { get; set; }
        public string CompanyName { get; set; }
        public string FacilityName { get; set; }
        public int? FacilityId { get; set; }
        public int companyId { get; set; }
    }
}
