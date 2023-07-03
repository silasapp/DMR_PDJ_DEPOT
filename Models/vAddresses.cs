using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vAddresses
    {
        public int id { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public int country_id { get; set; }
        public int StateId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string postal_code { get; set; }
        public int? elps_id { get; set; }
        public string Lga { get; set; }
    }
}
