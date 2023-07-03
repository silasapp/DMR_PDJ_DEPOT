using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Logins
    {
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string HostName { get; set; }
        public string MacAddress { get; set; }
        public string Local_Ip { get; set; }
        public string Remote_Ip { get; set; }
        public string UserAgent { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string LoginStatus { get; set; }
    }
}
