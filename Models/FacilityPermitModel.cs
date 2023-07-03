using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Models
{
    public class FacilityAPIModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public int LGAId { get; set; }
        public string FacilityType { get; set; }
    }
    
    public class FacilityPermitModel
    {
        public string Name { get; set; }
        public string PermitNo { get; set; }
        public bool IsApproved { get; set; }
    }


    public class FacilityToVM
    {
        public int oCoyId { get; set; }
        public int lgaId { get; set; }
        public int stateId { get; set; }
        public bool IsLegacy { get; set; }
        public string LicenseNo { get; set; }
        public string facName { get; set; }
        public int facilityId { get; set; }
        public decimal TransferCost { get; set; }
    }
}