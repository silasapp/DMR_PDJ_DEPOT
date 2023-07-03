using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class OilTerminals
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Platform { get; set; }
        public string Product { get; set; }
        public string OperationalZone { get; set; }
        public string SupervisedBy { get; set; }
    }
}
