using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class InspectionMeetingAttendees
    {
        public int Id { get; set; }
        public int MeetingScheduleId { get; set; }
        public string StaffEmail { get; set; }
        public DateTime Date { get; set; }
    }
}
