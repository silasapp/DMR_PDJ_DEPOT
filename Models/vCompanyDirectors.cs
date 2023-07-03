using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompanyDirectors
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int address_id { get; set; }
        public string telephone { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public int country_id { get; set; }
        public int StateId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string PostalCode { get; set; }
        public int nationality { get; set; }
        public int? elps_id { get; set; }
    }
}
