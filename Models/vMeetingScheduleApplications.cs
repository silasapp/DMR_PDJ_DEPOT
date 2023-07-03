using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vMeetingScheduleApplications
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Venue { get; set; }
        public int ApplicationId { get; set; }
        public string StaffUserName { get; set; }
        public string Address { get; set; }
        public DateTime Date { get; set; }
        public bool? Accepted { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string UserId { get; set; }
        public int company_id { get; set; }
        public string reference { get; set; }
        public string companyName { get; set; }
        public string AppUserName { get; set; }
        public string CategoryName { get; set; }
        public int year { get; set; }
        public DateTime MeetingDate { get; set; }
        public bool? WaiverRequest { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public bool? ScheduleExpired { get; set; }
    }
}
