using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class UserBranches
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int BranchId { get; set; }
        public int RoleId { get; set; }
        public int DepartmentId { get; set; }
        public int? DeskCount { get; set; }
        public bool? Active { get; set; }
    }
}
