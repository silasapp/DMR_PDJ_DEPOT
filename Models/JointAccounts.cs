using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class JointAccounts
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Opscon { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool? OperationsCompleted { get; set; }
        public bool? Assigned { get; set; }
    }
}
