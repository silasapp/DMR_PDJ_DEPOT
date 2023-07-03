using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class company_documents
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public int document_type_id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string source { get; set; }
        public DateTime date_modified { get; set; }
        public DateTime date_added { get; set; }
        public bool? Status { get; set; }
        public bool archived { get; set; }
        public string document_name { get; set; }
        public string UniqueId { get; set; }
        public int? Elps_Id { get; set; }
    }
}
