using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Phases
    {
        public int id { get; set; }
        public int category_id { get; set; }
        public string name { get; set; }
        public string Description { get; set; }
        public bool? Deleted { get; set; }
        public decimal? Price { get; set; }
        public decimal? ServiceCharge { get; set; }
        public int Stage { get; set; }
        public bool PriceByVolume { get; set; }
        public string ShortName { get; set; }
        public decimal ProcessingFee { get; set; }
        public bool ProcessingFeeByTank { get; set; }
        public decimal? SanctionFee { get; set; }
        public string IssueType { get; set; }
        public string FlowType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
