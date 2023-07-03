using System;
using System.Collections.Generic;

namespace NewDepot.ViewModels
{
    public class ApplicationReportViewModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<string> State { get; set; }
        public List<string> ApplicationType { get; set; }
        public DateTime? Start => Convert.ToDateTime(StartDate).Date;
        public DateTime? End => Convert.ToDateTime(EndDate).Date;
        
    }
}