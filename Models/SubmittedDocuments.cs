using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class SubmittedDocuments
    {
        public int SubDocID { get; set; }
        public int? RequestID { get; set; }
        public int AppDocID { get; set; }
        public int AppID { get; set; }
        public int? CompElpsDocID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeletedStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DocSource { get; set; }
        public string DocumentName { get; set; }
        public bool? IsAdditional { get; set; }
    }
}
