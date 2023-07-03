using System;
using System.Collections.Generic;

namespace NewDepot.ViewModels
{
    public class AppointmentViewModel
    {
        public int ApplicationId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string ScheduleMessage { get; set; }
        public string Action { get; set; }
    }
}