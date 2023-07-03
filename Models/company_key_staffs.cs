using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class company_key_staffs
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string nationality { get; set; }
        public string designation { get; set; }
        public string qualification { get; set; }
        public int years_of_exp { get; set; }
        public string skills { get; set; }
        public string training_certificates { get; set; }
        public int? Elps_Id { get; set; }
    }
}
