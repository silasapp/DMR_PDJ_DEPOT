using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class MeetingSchedules
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
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime MeetingDate { get; set; }
        public string DeclineReason { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public bool? WaiverRequest { get; set; }
        public string WaiverReason { get; set; }
        public bool? ScheduleExpired { get; set; }
        public bool? Completed { get; set; }
        public string FinalComment { get; set; }
        public int? VenueId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? SchedulerID { get; set; }
    }
}
