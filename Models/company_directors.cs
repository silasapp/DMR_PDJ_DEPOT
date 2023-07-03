using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class company_directors
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int address_id { get; set; }
        public string telephone { get; set; }
        public int nationality { get; set; }
        public int? elps_id { get; set; }
    }
}
