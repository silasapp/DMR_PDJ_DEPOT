using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace NewDepot.Models
{
    public partial class ZonalOffice
    {
        [Key]
        public int Zone_id { get; set; }
        public string ZoneName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? ELPSID { get; set; }
    }
}
