using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCategoryDocuments
    {
        public int id { get; set; }
        public string name { get; set; }
        public int category_id { get; set; }
    }
}
