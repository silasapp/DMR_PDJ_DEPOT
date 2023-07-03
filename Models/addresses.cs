using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public partial class addresses
    {
        public int id { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string postal_code { get; set; }
        public int country_id { get; set; }
        public int? elps_id { get; set; }
       
        public int StateId { get; set; }

        [ForeignKey("StateId")]
        public States_UT states { get; set; }

        [ForeignKey("StateId")]
        public FieldOfficeStates FieldOfficeStates { get; set; }

        public int? LgaId { get; set; }
    }
}
