using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Document_Type_Applications
    {
        public int Id { get; set; }
        public int DocumentTypeId { get; set; }
        public int ApplicationId { get; set; }
        public string UniqueId { get; set; }
    }
}
