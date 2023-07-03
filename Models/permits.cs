using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class permits
    {
        public int id { get; set; }
        public string permit_no { get; set; }
        public int application_id { get; set; }
        public int company_id { get; set; }
        public DateTime date_issued { get; set; }
        public DateTime date_expire { get; set; }
        public string categoryName { get; set; }
        public string is_renewed { get; set; }
        public int? elps_id { get; set; }
        public bool Printed { get; set; }
        public string dateString { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
