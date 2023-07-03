using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompanyAspUsers
    {
        public int id { get; set; }
        public DateTime? Date { get; set; }
        public int? elps_id { get; set; }
        public string AspUserId { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
    }
}
