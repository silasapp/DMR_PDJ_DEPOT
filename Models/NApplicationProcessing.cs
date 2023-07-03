using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class NApplicationProcessing
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int SortOrder { get; set; }
        public bool Assigned { get; set; }
        public DateTime? Date_Assigned { get; set; }
        public int? processor { get; set; }
        public int? ProcessingRule_Id { get; set; }
        public bool? Processed { get; set; }
        public string ProcessingLocation { get; set; }
        public DateTime? DateProcessed { get; set; }
        public bool? Holding { get; set; }
        public bool AutoPushed { get; set; }
    }
}
