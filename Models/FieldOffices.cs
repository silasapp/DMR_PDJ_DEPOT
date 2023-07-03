using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public partial class FieldOffices
    {
        [Key]
        public int FieldOffice_id { get; set; }
        public string OfficeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? ZoneFieldOffice_id { get; set; }

        [ForeignKey("ZoneFieldOffice_id")]
        public ZonalFieldOffice ZonalFieldOffice { get; set; }

        public int? ELPSID { get; set; }
        public string OfficeAddress { get; set; }
        public string FieldType { get; set; }
        public string StateName { get; set; }
    }
}
