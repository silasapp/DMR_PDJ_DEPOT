﻿using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class company_medicals
    {
        public int id { get; set; }
        public string medical_organisation { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public int company_id { get; set; }
        public DateTime date_issued { get; set; }
        public int? FileId { get; set; }
        public int? Elps_Id { get; set; }
    }
}
