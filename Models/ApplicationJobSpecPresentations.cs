using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class ApplicationJobSpecPresentations
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Job_Specification_Id { get; set; }
        public bool? PresentationApproved { get; set; }
        public bool? InspectionApproved { get; set; }
        public int? DepartmentId { get; set; }
        public bool? PresentationRequired { get; set; }
        public bool? InspectionRequired { get; set; }
    }
}
