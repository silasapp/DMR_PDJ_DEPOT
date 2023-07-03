using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public partial class Categories
    {
        [NotMapped]
        public int ApplicationCount { get; set; }
        [NotMapped]
        public int PermitCount { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string FriendlyName { get; set; }
        public int? Price { get; set; }
        public int? ServiceCharge { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
