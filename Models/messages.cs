using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class messages
    {
        public int id { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public int? read { get; set; }
        public int? company_id { get; set; }
        public string sender_id { get; set; }
        public DateTime? date { get; set; }
        public int? AppId { get; set; }
        public int? UserID { get; set; }
        public string UserType { get; set; }
    }
}
