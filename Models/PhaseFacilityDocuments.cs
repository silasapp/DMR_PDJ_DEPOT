using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class PhaseFacilityDocuments
    {
        public int Id { get; set; }
        public int document_type_id { get; set; }
        public int PhaseId { get; set; }
    }
}
