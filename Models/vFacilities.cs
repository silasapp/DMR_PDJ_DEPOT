using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vFacilities
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public int? CategoryId { get; set; }
        public int AddressId { get; set; }
        public int? CoordinateId { get; set; }
        public DateTime Date { get; set; }
        public int NoofDriveIn { get; set; }
        public int NoOfDriveOut { get; set; }
        public string CompanyName { get; set; }
        public string contact_phone { get; set; }
        public string user_id { get; set; }
        public string address_1 { get; set; }
        public string city { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public int? NoOfPumps { get; set; }
        public int? NoOfTanks { get; set; }
        public double? MaxCapacity { get; set; }
        public int? Elps_Id { get; set; }
        public string Lga { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public int StateId { get; set; }
        public string IdentificationCode { get; set; }
    }
}
