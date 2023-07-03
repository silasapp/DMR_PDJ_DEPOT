using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ExpiredScheduledMeetings
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
    }
}
