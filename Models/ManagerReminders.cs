using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ManagerReminders
    {
        public Guid Id { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool ReminderSent { get; set; }
        public int ScheduleId { get; set; }
        public int? InspectionScheduleId { get; set; }
    }
}
