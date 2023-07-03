using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vZones
    {
        public int id { get; set; }
        public int country_id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public short status { get; set; }
        public string CountryName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
    }
}
