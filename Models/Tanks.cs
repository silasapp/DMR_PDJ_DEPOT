using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public partial class Tanks
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public bool HasATG { get; set; }
        public string MaxCapacity { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string Position { get; set; }
        public double Height { get; set; }
        public double Diameter { get; set; }
        public bool Decommissioned { get; set; }
        public string FriendlyName { get; set; }
        public string ModifyType { get; set; }
        public string AppTank { get; set; }
        public string Status { get; set; }
        public string NewTankDetails { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? DeletedStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        public double Capacity => string.IsNullOrEmpty(MaxCapacity) ? 0 : double.Parse(MaxCapacity);
    }
}
