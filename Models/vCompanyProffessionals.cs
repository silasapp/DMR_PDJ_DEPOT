using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompanyProffessionals
    {
        public int id { get; set; }
        public string proffessional_organisation { get; set; }
        public string cert_no { get; set; }
        public int company_id { get; set; }
        public DateTime? date_issued { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
    }
}
