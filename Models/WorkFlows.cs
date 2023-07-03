using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class WorkFlows
    {
        public int Id { get; set; }
        public string OfficeLocation { get; set; }
        public string Action { get; set; }
        public string TriggeredByRole { get; set; }
        public string TargetRole { get; set; }
        public string Status { get; set; }
        public string State { get; set; }
        public int? ApplicationTypeId { get; set; }
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public bool? IsActive { get; set; }
    }
}
