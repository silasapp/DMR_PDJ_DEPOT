using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class MistdoStaff
    {
        public int MistdoId { get; set; }
        public int? CompanyId { get; set; }
        public int? FacilityId { get; set; }
        public string MistdoServerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string CertificateNo { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? DeletedStatus { get; set; }
    }
}
