using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Status { get; set; }
        public string Scheduler { get; set; }
        public string ScheduleMessage { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string CompanyComment { get; set; }
        public string NotifyList { get; set; }
        public bool isDone { get; set; }
        public bool IsApproved { get; set; }
        [ForeignKey("ApplicationId")]
        public applications Application { get; set; }
    }
}