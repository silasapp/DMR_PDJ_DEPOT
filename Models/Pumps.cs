using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Pumps
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TankId { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Version { get; set; }
        public int FacilityId { get; set; }
    }
}
