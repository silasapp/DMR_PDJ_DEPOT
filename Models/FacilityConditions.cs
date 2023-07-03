using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class FacilityConditions
    {
        public int Id { get; set; }
        public int NumberOfAttendants { get; set; }
        public bool AttentdantsHaveUniform { get; set; }
        public bool HasABackOffice { get; set; }
        public string DPRStaff { get; set; }
        public DateTime Date { get; set; }
        public int ApplicationId { get; set; }
        public int FacilityId { get; set; }
    }
}
