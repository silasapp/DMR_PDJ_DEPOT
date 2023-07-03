using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class pages
    {
        public int id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string meta_keyword { get; set; }
        public string meta_description { get; set; }
        public string content { get; set; }
    }
}
