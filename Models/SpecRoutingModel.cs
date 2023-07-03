using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Models
{
    public class SpecRoutingModel
    {
        public int JobSpecId { get; set; }
        public int RulesCount { get; set; }
        public int DepartmentId { get; set; }
        public int DepartmentHierarchy { get; set; }
    }

    public class ApplicationRoutingRules
    {

    }
}