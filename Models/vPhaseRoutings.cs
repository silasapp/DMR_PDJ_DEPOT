using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vPhaseRoutings
    {
        public int id { get; set; }
        public int PhaseId { get; set; }
        public string ProcessingLocation { get; set; }
        public int? SortOrder { get; set; }
        public bool Deleted { get; set; }
        public string DepartmentName { get; set; }
        public string RoleName { get; set; }
        public int ProcessingRule_Id { get; set; }
    }
}
