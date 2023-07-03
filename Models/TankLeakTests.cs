using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class TankLeakTests
    {
        public int Id { get; set; }
        public DateTime DateAdded { get; set; }
        public int ApplicationId { get; set; }
        public int FacilityId { get; set; }
        public int TanksTotalCapacity { get; set; }
    }
}
