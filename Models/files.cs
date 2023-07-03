using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class files
    {
        public int id { get; set; }
        public int model_id { get; set; }
        public int sort_order { get; set; }
        public string size { get; set; }
        public string mime { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string model_name { get; set; }
    }
}
