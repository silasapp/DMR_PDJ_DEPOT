using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ATCCompanies
    {
        public int Id { get; set; }
        public int ATCId { get; set; }
        public string Name { get; set; }
        public string RCNumber { get; set; }
    }
}
