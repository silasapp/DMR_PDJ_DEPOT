using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ApplicationDocuments_UT
    {
        public int AppDocID { get; set; }
        public int ElpsDocTypeID { get; set; }
        public string DocName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string docType { get; set; }
        public int? PhaseId { get; set; }
    }
}
