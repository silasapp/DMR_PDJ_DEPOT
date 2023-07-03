using System.Collections.Generic;

namespace NewDepot.ViewModels
{
    public class ApprovalViewModel
    {
        public int ApplicationId { get; set; }
        public string Reference { get; set; }
        public string Report { get; set; }
        public string Action { get; set; }
        public string Email { get; set; }
        public List<string> MissingDocs { get; set; }
    }
}