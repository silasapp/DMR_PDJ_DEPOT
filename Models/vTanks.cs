using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vTanks
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public bool HasATG { get; set; }
        public string MaxCapacity { get; set; }
        public int? ProductId { get; set; }
        public string Position { get; set; }
        public string ProductName { get; set; }
        public string FriendlyName { get; set; }
        public double Height { get; set; }
        public double Diameter { get; set; }
        public bool Decommissioned { get; set; }
    }
}
