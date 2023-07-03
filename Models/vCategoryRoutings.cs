using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCategoryRoutings
    {
        public string RoleName { get; set; }
        public string DepartmentName { get; set; }
        public int Id { get; set; }
        public int category_id { get; set; }
        public string ProcessingLocation { get; set; }
        public int SortOrder { get; set; }
        public bool Deleted { get; set; }
        public int ProcessingRule_id { get; set; }
    }
}
