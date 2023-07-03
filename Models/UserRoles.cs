using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class UserRoles
    {
        public int Role_id { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
