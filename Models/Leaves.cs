using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Leaves
    {
        public int Id { get; set; }
        public Guid ApplicantId { get; set; }
        public Guid ManagerId { get; set; }
        public bool? ManagerApproved { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public bool? Ended { get; set; }
        public string Reason { get; set; }
        public bool? Started { get; set; }
        public DateTime? DateLogged { get; set; }
        public string ReasonForDisapproval { get; set; }
    }
}
