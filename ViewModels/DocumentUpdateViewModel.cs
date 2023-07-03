using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewDepot.ViewModels
{
    public class DocumentUpdateViewModel
    {
        public int? ApplicationTypeId { get; set; }

        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        [Required]
        public List<string> DocId { get; set; }
    }
}