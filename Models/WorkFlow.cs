using System.ComponentModel.DataAnnotations.Schema;

namespace NewDepot.Models
{
    public class WorkFlow
    { 
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int PhaseId { get; set; }
        public int ProductId { get; set; }
        public int? ApplicationTypeId { get; set; }
        public string OfficeLocation { get; set; }
        public string Action { get; set; }
        public string TriggeredByRole { get; set; }
        public string TargetRole { get; set; }
        public string Status { get; set; }
        
        public string State { get; set; }
        public bool IsActive { get; set; }
    }
}