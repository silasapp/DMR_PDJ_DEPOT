using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Fields
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public string Validation { get; set; }
        public string DataType { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public string LastModifiedByUserId { get; set; }
        public DateTime? lastModifiedOnDate { get; set; }
        public string OptionValue { get; set; }
        public string DisplayLabel { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public string hiddenValue { get; set; }
    }
}
