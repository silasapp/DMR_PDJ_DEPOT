using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ManagerScheduleMeetings
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ScheduleId { get; set; }
    }
}
