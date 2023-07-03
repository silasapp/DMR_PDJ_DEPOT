using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vApplicationDocuments
    {
        public int Id { get; set; }
        public int application_id { get; set; }
        public int document_id { get; set; }
        public int document_type_id { get; set; }
        public string documentTypeName { get; set; }
        public string source { get; set; }
        public string FileName { get; set; }
        public bool? Status { get; set; }
        public string document_name { get; set; }
        public string UniqueId { get; set; }
        public int company_id { get; set; }
    }
}
