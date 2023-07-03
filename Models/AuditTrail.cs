using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class AuditTrail
    {
        public int AuditLogID { get; set; }
        public string AuditAction { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserID { get; set; }
    }
}
