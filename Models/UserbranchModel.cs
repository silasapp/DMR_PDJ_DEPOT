using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewDepot.Models;

namespace NewDepot.Models
{
    public class UserBranchModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StaffEmail { get; set; }
        public int BranchId { get; set; }
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }
        [Display(Name = "Is Active")]
        public bool Active { get; set; }
        //[NotMapped]
        //public RegisterViewModel User { get; set; }
        [NotMapped]
        public Staff UserBranch { get; set; }
    }

}