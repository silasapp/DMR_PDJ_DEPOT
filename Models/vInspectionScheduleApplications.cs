using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vInspectionScheduleApplications
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string Message { get; set; }
        public string Venue { get; set; }
        public int ApplicationId { get; set; }
        public string StaffUserName { get; set; }
        public string Address { get; set; }
        public DateTime MeetingDate { get; set; }
        public DateTime DateAdded { get; set; }
        public bool? Accepted { get; set; }
        public string DeclineReason { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public bool ScheduleExpired { get; set; }
        public string companyName { get; set; }
        public string reference { get; set; }
    }
}
