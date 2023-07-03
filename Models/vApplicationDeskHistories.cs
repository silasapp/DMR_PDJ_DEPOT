using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class vApplicationDeskHistories
    {
        public int id { get; set; }
        public int application_id { get; set; }
        public string UserName { get; set; }
        public string comment { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public int year { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string CategoryName { get; set; }
        public string CompanyName { get; set; }
        public string reference { get; set; }
    }
}
