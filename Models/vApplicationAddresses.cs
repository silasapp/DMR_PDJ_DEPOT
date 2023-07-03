using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vApplicationAddresses
    {
        public string type { get; set; }
        public int id { get; set; }
        public int CompanyId { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
        public string CompanyName { get; set; }
    }
}
