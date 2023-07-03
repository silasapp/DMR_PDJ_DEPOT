using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class TrainingPrograms
    {
        public int Id { get; set; }
        public string StaffName { get; set; }
        public string Role { get; set; }
        public string CourseTitle { get; set; }
        public string Duration { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
