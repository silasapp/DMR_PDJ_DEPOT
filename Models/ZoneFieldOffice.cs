using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ZoneFieldOffice
    {
        public int ZoneFieldOffice_id { get; set; }
        public int Zone_id { get; set; }
        public int FieldOffice_id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
