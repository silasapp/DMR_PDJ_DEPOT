using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vProcessingRules
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }
        public string DepartmentName { get; set; }
        public string RoleName { get; set; }
    }
}
