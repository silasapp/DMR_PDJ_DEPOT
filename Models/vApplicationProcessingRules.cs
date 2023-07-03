using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vApplicationProcessingRules
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public int? processor { get; set; }
        public DateTime? DateProcessed { get; set; }
        public int? ProcessingRule_Id { get; set; }
        public bool? Processed { get; set; }
        public int ApplicationId { get; set; }
        public bool Assigned { get; set; }
        public string ProcessingLocation { get; set; }
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }
    }
}
