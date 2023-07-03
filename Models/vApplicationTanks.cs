using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vApplicationTanks
    {
        public int Id { get; set; }
        public string TankName { get; set; }
        public int TankId { get; set; }
        public double Capacity { get; set; }
        public int FacilityId { get; set; }
        public int CompanyId { get; set; }
        public int ApplicationId { get; set; }
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string FriendlyName { get; set; }
        public double Diameter { get; set; }
        public double Height { get; set; }
    }
}
