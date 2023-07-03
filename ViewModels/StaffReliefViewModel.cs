using System.ComponentModel.DataAnnotations;

namespace NewDepot.ViewModels
{
    public class StaffReliefViewModel
    {
        [Required]
        public string OldStaffEmail { get; set; }
        [Required]
        public string NewStaff { get; set; }
        [Required]
        public int AppNumber { get; set; }
    }
}