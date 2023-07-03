using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class zoneState
    {
        public int id { get; set; }
        public int StateId { get; set; }
        public int ZoneId { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public bool? DeleteStatus { get; set; }
    }
}
