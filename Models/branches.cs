using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class branches
    {
        public int id { get; set; }
        public string name { get; set; }
        public string branchCode { get; set; }
        public string location { get; set; }
        public DateTime create_at { get; set; }
        public DateTime lastedit_at { get; set; }
        public int status { get; set; }
        public string Address { get; set; }
        public int? State_id { get; set; }
    }
}
