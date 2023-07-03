using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class GeoPoliticalZone
    {
        public int GeoId { get; set; }
        public string GeoName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeletedStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
