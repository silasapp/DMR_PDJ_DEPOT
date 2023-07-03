using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompanyfiles
    {
        public int id { get; set; }
        public int companyId { get; set; }
        public int document_type_id { get; set; }
        public string source { get; set; }
        public DateTime date_modified { get; set; }
        public DateTime date_added { get; set; }
        public string DocumentTypeName { get; set; }
        public string FileName { get; set; }
        public bool? Status { get; set; }
        public bool archived { get; set; }
        public string document_name { get; set; }
        public string UniqueId { get; set; }
        public int? Elps_Id { get; set; }
    }
}
