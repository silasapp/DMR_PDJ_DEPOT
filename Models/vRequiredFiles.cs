using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vRequiredFiles
    {
        public int Id { get; set; }
        public int category_id { get; set; }
        public int document_type_id { get; set; }
        public string DocumentTypeName { get; set; }
        public string CategoryName { get; set; }
    }
}
