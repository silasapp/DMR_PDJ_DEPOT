using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewDepot.Models
{
    public class permitsModel
    {
        public string Permit_No { get; set; }
        public string Reference { get; set; }
        public string ModifyType { get; set; }

        public int Application_Id { get; set; }
        public string CompanyName { get; set; }
        public string PhaseName { get; set; }
        public string PhaseShortName { get; set; }
        public string ApprovalType { get; set; }

        public int Company_Id { get; set; }
        public DateTime Date_Issued { get; set; }
        public string dateString { get; set; }

        public DateTime Date_Expire { get; set; }
        public string CheckApprovalType { get; set; }
        public string CategoryName { get; set; }
        public int Id { get; set; }
        public int Category_id { get; set; }
        public string Is_Renewed { get; set; }
        public decimal Fees { get; set; }
        public int Elps_Id { get; set; }
        public string OrderId { get; set; }
        public int PhaseId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string StateName { get; set; }
        public string IssueType { get; set; }
       
        public List<application_services> Services { get; set; }

        public string JS_Combined { get; set; }
        public bool Printed { get; set; }

    }
}
