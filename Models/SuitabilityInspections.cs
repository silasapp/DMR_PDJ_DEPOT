using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class SuitabilityInspections
    {
        public int Id { get; set; }
        public string SizeOfLand { get; set; }
        public bool? ISAlongPipeLine { get; set; }
        public bool? IsUnderHighTension { get; set; }
        public int? StationsWithin2KM { get; set; }
        public string DistanceFromExistingStation { get; set; }
        public bool? IsOnHighWay { get; set; }
        public int CompanyId { get; set; }
        public string EIADPRStaff { get; set; }
        public int FacilityId { get; set; }
        public int ApplicationId { get; set; }
    }
}
