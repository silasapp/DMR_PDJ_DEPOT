using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class application_services
    {
        public int Id { get; set; }
        public int application_id { get; set; }
        public int service_id { get; set; }
    }
}
