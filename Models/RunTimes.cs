using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class RunTimes
    {
        public int Id { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string ResponseMessage { get; set; }
    }
}
