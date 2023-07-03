using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vUserBranches
    {
        public string UserEmail { get; set; }
        public int BranchId { get; set; }
        public int RoleId { get; set; }
        public int DepartmentId { get; set; }
        public int Id { get; set; }
        public int? DeskCount { get; set; }
        public string DepartmentName { get; set; }
        public string BranchName { get; set; }
        public string location { get; set; }
        public string Role { get; set; }
        public bool? Active { get; set; }
        public string UserId { get; set; }
    }
}
