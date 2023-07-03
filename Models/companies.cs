using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class companies
    {
        public int id { get; set; }
        public string user_id { get; set; }
        public string name { get; set; }
        public string business_type { get; set; }
        public int? registered_address_id { get; set; }
        public int? operational_address_id { get; set; }
        public string affiliate { get; set; }
        public string nationality { get; set; }
        public string contact_firstname { get; set; }
        public string contact_lastname { get; set; }
        public string contact_phone { get; set; }
        public int? year_incorporated { get; set; }
        public string rc_number { get; set; }
        public string tin_number { get; set; }
        public int? no_staff { get; set; }
        public int? no_expatriate { get; set; }
        public string total_asset { get; set; }
        public string yearly_revenue { get; set; }
        public string accident_report { get; set; }
        public string training_program { get; set; }
        public string mission_vision { get; set; }
        public string hse { get; set; }
        public DateTime? Date { get; set; }
        public bool? hseDoc { get; set; }
        public int? accident { get; set; }
        public int? elps_id { get; set; }
        public string DPR_Id { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyEmail { get; set; }
        public string Avarta { get; set; }
        public bool? isFirstTime { get; set; }
        public bool? ActiveStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? DeleteStatus { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public int? LocationId { get; set; }
        public int? RoleId { get; set; }
    }
}
