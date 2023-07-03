using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vFacilityTankModifications
    {
        public int Id { get; set; }
        public int TankId { get; set; }
        public int ApplicationId { get; set; }
        public string ModifyType { get; set; }
        public int FacilityId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string ProductName { get; set; }
        public double Diameter { get; set; }
        public double Height { get; set; }
        public string MaxCapacity { get; set; }
        public string PrevProduct { get; set; }
    }
}
