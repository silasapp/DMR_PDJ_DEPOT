using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Multi_Inspections
    {
        public int Id { get; set; }
        public int? ApplicationId { get; set; }
        public bool? InspectionApproved { get; set; }
        public int? DepartmentId { get; set; }
    }
}
