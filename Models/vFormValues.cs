using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vFormValues
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
        public int FieldId { get; set; }
        public int FormId { get; set; }
        public int Id { get; set; }
        public Guid? GroupId { get; set; }
        public string Title { get; set; }
        public string FriendlyName { get; set; }
    }
}
