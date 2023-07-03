using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Notifications
    {
        public int Id { get; set; }
        public string ToStaff { get; set; }
        public string Message { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public bool? Deleted { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
