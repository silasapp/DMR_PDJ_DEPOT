using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vCompanies
    {
        public int id { get; set; }
        public int? registered_address_id { get; set; }
        public string address_1 { get; set; }
        public string Name { get; set; }
        public string User_Id { get; set; }
        public string Business_Type { get; set; }
        public string Nationality { get; set; }
        public string Contact_FirstName { get; set; }
        public string Contact_LastName { get; set; }
        public string Contact_Phone { get; set; }
        public int? Year_Incorporated { get; set; }
        public string RC_Number { get; set; }
        public string Tin_Number { get; set; }
        public string city { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
    }
}
