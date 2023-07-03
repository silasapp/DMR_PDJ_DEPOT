using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class company_nsitfs
    {
        public int id { get; set; }
        public int no_people_covered { get; set; }
        public string policy_no { get; set; }
        public int company_id { get; set; }
        public DateTime date_issued { get; set; }
        public int? FileId { get; set; }
        public int? Elps_Id { get; set; }
    }
}
