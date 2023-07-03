using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Models
{
    public class ZoneBranchModel
    {
        public int Id { get; set; }
        public string ZoneName { get; set; }
        public List<BranchesModel> ZoneBranches { get; set; }
        public int Span { get; set; }
    }

    public class BranchesModel
    {
        public string BranchName { get; set; }
        public List<string> MappedStates { get; set; }
    }
}