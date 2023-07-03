using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompAddressesU
    {
        public int id { get; set; }
        public int? registered_address_id { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string postal_code { get; set; }
        public int country_id { get; set; }
        public int? elps_id { get; set; }
        public int StateId { get; set; }
        public int? LgaId { get; set; }
        public DateTime? Date { get; set; }
    }
}
