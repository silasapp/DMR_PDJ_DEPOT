using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class AuditLogs
    {
        public Guid AuditLogId { get; set; }
        public string UserId { get; set; }
        public DateTime EventDateUTC { get; set; }
        public string EventType { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string ColumnName { get; set; }
        public string OriginalValue { get; set; }
        public string NewValue { get; set; }
        public string IP { get; set; }
    }
}
