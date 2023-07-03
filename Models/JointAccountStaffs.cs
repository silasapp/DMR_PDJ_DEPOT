using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class JointAccountStaffs
    {
        public int Id { get; set; }
        public int JointAccountId { get; set; }
        public string Staff { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool? SignedOff { get; set; }
        public bool? OperationsCompleted { get; set; }
        public int? ApplicationId { get; set; }
    }
}
