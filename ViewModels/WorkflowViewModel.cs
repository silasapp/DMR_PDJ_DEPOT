using System.ComponentModel.DataAnnotations;

namespace NewDepot.ViewModels
{
    public class WorkflowViewModel
    {
        [Display(Name = "Application Type")]
        public int? ApplicationTypeId { get; set; }
        [Display(Name = "Category Name")]
        public int CategoryId { get; set; }
        [Display(Name = "Product Name")]
        public int ProductId { get; set; }
        [Display(Name = "Triggered-by Role")]
        public string TriggeredByRole { get; set; }
        [Display(Name = "Office")]
        public string OfficeLocation { get; set; }
        public string Action { get; set; }
        [Display(Name = "Target Role")]
        public string TargetRole { get; set; }
        [Display(Name = "Application State")]
        public string State { get; set; }
        public string Status { get; set; }
    }
}