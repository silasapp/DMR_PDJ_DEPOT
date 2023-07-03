using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ProcessingRules
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }
    }
}
