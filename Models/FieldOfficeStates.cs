using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public partial class FieldOfficeStates
    {
        //[Key]
        public int FieldOfficeStatesId { get; set; }
        public int FieldOffice_id { get; set; }
       
        [ForeignKey("FieldOffice_id")]
        public FieldOffices FieldOffices { get; set; }

        public int StateId { get; set; }
        
        [ForeignKey("StateId")]
        public States_UT states { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
