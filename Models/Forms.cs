using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Forms
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string FriendlyName { get; set; }
        public bool? IsPublished { get; set; }
        public bool? Deleted { get; set; }
        public string CreatedByUserId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int? PhaseId { get; set; }
        public string OtherPhases { get; set; }
    }
}
