using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ATGs
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public bool Available { get; set; }
        public string Parameters { get; set; }
        public bool Functional { get; set; }
        public string DPROfficial { get; set; }
    }
}
