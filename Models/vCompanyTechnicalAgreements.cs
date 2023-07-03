using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompanyTechnicalAgreements
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public string name { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
    }
}
