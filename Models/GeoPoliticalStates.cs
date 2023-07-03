using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class GeoPoliticalStates
    {
        public int GeoStateId { get; set; }
        public int GeoId { get; set; }
        public int StateId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeletedStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
