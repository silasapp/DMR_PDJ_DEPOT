using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class AppPhaseDocuments
    {
        public int DocID { get; set; }
        public int AppPhaseID { get; set; }
        public int AppDocID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
