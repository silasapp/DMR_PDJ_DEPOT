using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class application_documents
    {
        public int Id { get; set; }
        public int application_id { get; set; }
        public int document_id { get; set; }
        public int document_type_id { get; set; }
        public string UniqueId { get; set; }
        public bool? Status { get; set; }
    }
}
