using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Models
{
    public class DashBoardModel
    {

        public List<messages> Messages { get; set; }
        public int ApplicationToday { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Processing { get; set; }
        public int DocumentRequired { get; set; }

        public int SubmittedNow { get; set; }
        public int PaymentPending { get; set; }

        public int Pending { get; set; }

        public int Declined { get; set; }

        public int TotalApplication { get; set; }
        public int Expiring { get; set; }
        public int Expired { get; set; }

        public int CompanyId { get; set; }

        public int CategoryA { get; set; }
        public int CategoryB { get; set; }
        public int CategoryC { get; set; }
    }


    /// <summary>
    ///     Dash Board Model for the Admin area
    /// </summary>
    public class AdminDashBoardModel
    {
        /// <summary>
        ///     All Application irrespective of their status
        /// </summary>
        public int Processing { get; set; }
        public int Paymentpending { get; set; }
        public int Approved { get; set; }
        public string Date { get; set; }
        /// <summary>
        ///     Total Applications that has been confirmed and active (i.e. not expired)
        /// </summary>
        public int TotalApplications { get; set; }

        /// <summary>
        ///     Total Application that's in processing phase
        /// </summary>
        public int Permits { get; set; }

        /// <summary>
        ///     Total Applications Submitted Today
        /// </summary>
        public int ApplicationToday { get; set; }
    }

    /// <summary>
    ///     Dash Board Model for the Admin area
    /// </summary>
    public class StaffDashBoardModel
    {
        /// <summary>
        ///     All Application That Ive worked on
        /// </summary>
        public int AdTotalApplications { get; set; }
        public int AdApproved { get; set; }
        public int AdRejected { get; set; }
        public int Processing { get; set; }
        public int TotalApplications { get; set; }
        public int TodayApplications { get; set; }
        public int TotalPermits { get; set; }
        /// <summary>
        ///     Total Applications currently on my desk
        /// </summary>
        public int OnMyDesk { get; set; }

        /// <summary>
        ///     Total Application this Staff has Approved
        /// </summary>
        public int Approved { get; set; }

        /// <summary>
        ///     Total Applications this Staff has declined
        /// </summary>
        public int Declined { get; set; }

        public int CategoryA { get; set; }
        public int CategoryB { get; set; }
        public int CategoryC { get; set; }
        public string StaffRole { get; set; }
    }
}