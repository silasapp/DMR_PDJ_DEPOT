using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vFormFields
    {
        public int FormId { get; set; }
        public int Id { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public string Validation { get; set; }
        public string DataType { get; set; }
        public string OptionValue { get; set; }
        public string DisplayLabel { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public string FriendlyName { get; set; }
        public string Title { get; set; }
    }
}
