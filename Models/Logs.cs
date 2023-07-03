using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Logs
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Error { get; set; }
        public DateTime Date { get; set; }
    }
}
