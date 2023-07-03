using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class TankInspections
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int NumberOfTanks { get; set; }
        public int NumberOfPumps { get; set; }
        public int NumberOfDriveIn { get; set; }
        public int NumberOfDriveOut { get; set; }
        public DateTime Date { get; set; }
        public int FacilityId { get; set; }
    }
}
