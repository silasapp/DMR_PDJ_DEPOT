using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vJointApplications
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public int category_id { get; set; }
        public string type { get; set; }
        public int year { get; set; }
        public decimal? fee_payable { get; set; }
        public decimal? service_charge { get; set; }
        public string status { get; set; }
        public DateTime date_added { get; set; }
        public DateTime? date_modified { get; set; }
        public string CategoryName { get; set; }
        public string companyName { get; set; }
        public string reference { get; set; }
        public int? current_desk { get; set; }
        public string user_id { get; set; }
        public bool? submitted { get; set; }
        public int PhaseId { get; set; }
        public string PhaseName { get; set; }
        public string FacilityName { get; set; }
        public int AddressId { get; set; }
        public string address_1 { get; set; }
        public string city { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public int? FacilityId { get; set; }
        public bool? AllowPush { get; set; }
        public string Opscon { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool? OperationsCompleted { get; set; }
        public bool? Assigned { get; set; }
        public bool? SignedOff { get; set; }
    }
}
