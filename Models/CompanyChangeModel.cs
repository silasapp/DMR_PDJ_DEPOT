using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using Microsoft.DotNet.Highcharts.Options;
//using DotNet.Highcharts;
//using Microsoft.DotNet.Highcharts.Enums;
//using DotNet.Highcharts.Helpers;

namespace NewDepot.Models
{
    public class CompanyChangeModel
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string RC_Number { get; set; }
        public string Business_Type { get; set; }
        public string NewEmail { get; set; }
    }
}