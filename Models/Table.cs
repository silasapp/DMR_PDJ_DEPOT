using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class FacilityPermits
    {
        public List<Tanks> Tanks { get; set; }
        public List<Pumps> Pumps { get; set; }
        public int Id { get; set; }
        public string PermitNo { get; set; }
        public DateTime DateIssued { get; set; }
        public DateTime DateExpired { get; set; }
        public bool IsRenewed { get; set; }
        public int ElpsID { get; set; }
        public int CompanyID { get; set; }
        public string Type { get; set; }
       // public string FacilityName { get; set; }
        //public string PhaseName { get; set; }
        public string CompanyName { get; set; }
        public int ApplicationID { get; set; }
        public int CategoryID { get; set; }
        public int? FacilityID { get; set; }
    }
}
