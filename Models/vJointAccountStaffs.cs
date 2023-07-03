using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vJointAccountStaffs
    {
        public int ApplicationId { get; set; }
        public string Opscon { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool? OperationsCompleted { get; set; }
        public string Staff { get; set; }
        public DateTime? DateStaffAdded { get; set; }
        public int? JAS_Id { get; set; }
        public int? JointAccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? SignedOff { get; set; }
        public bool? Assigned { get; set; }
    }
}
