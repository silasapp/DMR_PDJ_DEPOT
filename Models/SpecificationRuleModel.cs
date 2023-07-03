using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewDepot.Models;

namespace NewDepot.Models
{
    public class SpecificationRuleModel
    {
        public  List<ProcessingRules> SpecificationRules  { get; set; }

        public List<ProcessingRules> ServiceRules { get; set; }

        public List<ProcessingRules> CategoryRules { get; set; }

       // public List<ProcessLocation> orderList { get; set; }
    }
}