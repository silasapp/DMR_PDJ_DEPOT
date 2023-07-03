using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class company_technical_agreements
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public string name { get; set; }
        public int FileId { get; set; }
        public int? Elps_Id { get; set; }
    }
}
