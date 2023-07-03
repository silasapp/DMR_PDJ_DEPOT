using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class TakeOvers
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int CompanyId { get; set; }
        public int OldCompanyId { get; set; }
        public int ApplicationId { get; set; }
        public DateTime Date { get; set; }
        public string OldCompanyDPRId { get; set; }
    }
}
