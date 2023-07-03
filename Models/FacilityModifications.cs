using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class FacilityModifications
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int PhaseId { get; set; }
        public int ApplicationId { get; set; }
        public DateTime? Date { get; set; }
        public string Type { get; set; }
        public string PrevProduct { get; set; }
    }
}
