using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LpgLicense.Models;

namespace NewDepot.ViewModels
{
    public class ApplicationViewModel
    {
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public int? ApplicationTypeId { get; set; }
        public string LinkedReference { get; set; }
        public Facility Facility { get; set; }
    }
}