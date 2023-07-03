using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ApplicationRequest
    {
        public int RequestID { get; set; }
        public int CompanyID { get; set; }
        public int DurationID { get; set; }
        public int? DeskID { get; set; }
        public int? CurrentStaffDeskID { get; set; }
        public string RequestRefNo { get; set; }
        public string GeneratedNumber { get; set; }
        public int? RequestSequence { get; set; }
        public bool EmailSent { get; set; }
        public bool hasAcknowledge { get; set; }
        public DateTime? AcknowledgeAt { get; set; }
        public string Status { get; set; }
        public bool? isSubmitted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeletedStatus { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? DateApplied { get; set; }
    }
}
