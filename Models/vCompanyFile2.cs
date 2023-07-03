using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompanyFile2
    {
        public int id { get; set; }
        public int CompanyId { get; set; }
        public string FileName { get; set; }
        public string source { get; set; }
        public int document_id { get; set; }
        public int document_type_id { get; set; }
        public string DocumentTypeName { get; set; }
    }
}
