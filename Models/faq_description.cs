using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class faq_description
    {
        public int id { get; set; }
        public int? faq_id { get; set; }
        public string locale_code { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }
}
