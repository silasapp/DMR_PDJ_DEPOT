using NewDepot.Models;
using LpgLicense.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Models
{
    public class CompanyModelAPI
    {
        public Company Company{ get; set; }
        public List<vCompanyMedical> CompanyMedicals { get; set; }
        public List<vCompanyExpatriateQuota> CompanyExpatriateQuotas { get; set; }
        public List<vCompanyNsitf> CompanyNsitfs { get; set; }
        public List<vCompanyProffessional> CompanyProffessionals { get; set; }
        public List<vCompanyTechnicalAgreement> CompanyTechnicalAgreements { get; set; }
    }

    public class StorageModel
    {
        public int CompanyId { get; set; }
        public string StreetAddress { get; set; }
        public int StateId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double StorageCapacity { get; set; }
        public string StorageType { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int CategoryId { get; set; }
        public int LgaId { get; set; }
    }
}