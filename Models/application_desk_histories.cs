using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class application_desk_histories
    {
        public int id { get; set; }
        public int application_id { get; set; }
        public string UserName { get; set; }
        public string status { get; set; }
        public string comment { get; set; }
        public DateTime date { get; set; }
        public int? StaffID { get; set; }
    }
}
