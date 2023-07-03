using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Waivers
    {
        public int ID { get; set; }
        public int ApplicationId { get; set; }
        public string RequestFrom { get; set; }
        public string RequestReason { get; set; }
        public string AssignedManager { get; set; }
        public bool? Approved { get; set; }
        public string ManagerReason { get; set; }
        public string WaiverFor { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? ManagerResponseDate { get; set; }
        public int? entityId { get; set; }
    }
}
