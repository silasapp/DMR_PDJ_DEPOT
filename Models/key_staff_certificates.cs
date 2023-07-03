using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class key_staff_certificates
    {
        public int id { get; set; }
        public int company_key_staff { get; set; }
        public string name { get; set; }
        public string cert_no { get; set; }
        public int year { get; set; }
        public string issuer { get; set; }
        public int? Elps_Id { get; set; }
    }
}
