using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public partial class Facilities
    {
        [NotMapped]
        public List<TankModel> ApplicationTanks { get; set; }
        [NotMapped]
        public List<TankModel> Tanks { get; set; }
        [NotMapped]
        public List<Pumps> Pumps { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public int? CategoryId { get; set; }
        public int AddressId { get; set; }
        public int? CoordinateId { get; set; }
        public DateTime Date { get; set; }
        public int NoofDriveIn { get; set; }
        public int NoOfDriveOut { get; set; }
        public int? NoOfPumps { get; set; }
        public int? NoOfTanks { get; set; }
        public int? MaxCapacity { get; set; }
        public int? Elps_Id { get; set; }
        public string LGA { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string IdentificationCode { get; set; }
        public string CategoryCode { get; set; }
        public int? FirstOperationYear { get; set; }
        public string city { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string address_1 { get; set; }
        public bool? DeletedStatus { get; set; }
        public int? DeletedAt { get; set; }
        [ForeignKey("AddressId")]
        public addresses Addresses { get; set; }
    }
}
