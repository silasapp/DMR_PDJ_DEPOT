using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Signatories
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Signature { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Type { get; set; }
    }
}
