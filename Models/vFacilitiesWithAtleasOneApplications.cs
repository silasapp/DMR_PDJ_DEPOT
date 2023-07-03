using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vFacilitiesWithAtleasOneApplications
    {
        public int id { get; set; }
        public string name { get; set; }
        public int CompanyId { get; set; }
        public DateTime Date { get; set; }
        public int? elps_id { get; set; }
        public string city { get; set; }
        public string StateName { get; set; }
        public string ContactName { get; set; }
        public string IdentificationCode { get; set; }
        public string CategoryCode { get; set; }
        public int? FirstOperationYear { get; set; }
        public string StreetAddress { get; set; }
    }
}
