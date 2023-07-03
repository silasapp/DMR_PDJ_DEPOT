using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Sanctions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal StatutoryFee { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal SanctionFee { get; set; }
        public bool PriceByVolume { get; set; }
        public bool ProcessingFeeByTank { get; set; }
    }
}
