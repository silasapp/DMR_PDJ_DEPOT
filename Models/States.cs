using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class States
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
