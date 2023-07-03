using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class FieldValues
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string Value { get; set; }
        public Guid? GroupId { get; set; }
        public int? ApplicationId { get; set; }
        public int? FormId { get; set; }
    }
}
