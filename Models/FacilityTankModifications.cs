using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class FacilityTankModifications
    {
        public int Id { get; set; }
        public int TankId { get; set; }
        public int ApplicationId { get; set; }
        public string ModifyType { get; set; }
        public string Type { get; set; }
        public string PrevProduct { get; set; }
    }
}
