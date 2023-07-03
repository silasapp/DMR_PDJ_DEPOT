using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ATCs
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int SuitabilityId { get; set; }
        public int ApplicationId { get; set; }
        public int? FacilityId { get; set; }
    }
}
