using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class JointApplications
    {
        public int id { get; set; }
        public int JointAccountId { get; set; }
        public int applicationId { get; set; }
        public int FacilityId { get; set; }
        public string Staff { get; set; }
        public string FacilityName { get; set; }
        public string PhaseName { get; set; }
        public string Opscon { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? SignedOff { get; set; }
        public bool? Assigned { get; set; }
        public bool? AllowPAush { get; set; }
        public bool? OperationsCompleted { get; set; }
    }
}
