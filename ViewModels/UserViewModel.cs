using Microsoft.AspNetCore.Http;

namespace NewDepot.ViewModels
{
    public class UserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public int LLocation { get; set; }
        public IFormFile Avatar { get; set; }
    }
}