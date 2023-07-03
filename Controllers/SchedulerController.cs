using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NewDepot.Models;
using NewDepot.Helpers;
using System.IO;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
//using NewDepot.Payments;
using System.Data.Sql;
using System.Transactions;
using Rotativa;
using System.Text;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using NewDepot.Controllers;
using Microsoft.AspNetCore.Http;
using LpgLicense.Models;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Controllers.Authentications;
using static NewDepot.Models.Payment;
using Microsoft.AspNetCore.Hosting.Server;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Options;
using Application = NewDepot.Models.applications;
using Microsoft.EntityFrameworkCore;
using RemitaResponse = NewDepot.Models.Payment.RemitaResponse;
using NewDepot.ViewModels;
using ApplicationHistory = NewDepot.Models.application_desk_histories;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace NewDepot.Controllers
{
    public class SchedulerController : Controller
    {
            SubmittedDocuments _appDocRep;
            RestSharpServices _restService = new RestSharpServices();
            public IConfiguration _configuration;
            IHttpContextAccessor _httpContextAccessor;
            private readonly Depot_DBContext _context;

            ElpsResponse elpsResponse = new ElpsResponse();
            ApplicationHelper appHelper;
            Helpers.Authentications auth = new Helpers.Authentications();
            GeneralClass generalClass = new GeneralClass();
            HelpersController _helpersController;

        List<ManagerReminders> _managerRem = new List<ManagerReminders>();
        List<MeetingSchedules> schedules = new List<MeetingSchedules>();

        public SchedulerController(
                Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
            {
                _context = context;
                _httpContextAccessor = httpContextAccessor;
                _configuration = configuration;
                appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
                _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

            }


        //[Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }



        #region Application Processing (waivers, Presentations and Inspections)

        [HttpPost]
        public async Task<ActionResult> ProcessApplication(string em, DateTime? ldate)
        {
            //Log progress
            RunTimes cw = new RunTimes();
            cw.ResponseMessage = "Application Crawler Still running";
            cw.LastRunTime = DateTime.Now;
            _context.SaveChanges();

            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userRole == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }

            //_configuration.GetSection("AmountSetting").GetSection("otherDocId")
            //clientReminderTime
            var clientReminderTime = Convert.ToInt16(2);
          

            #region Initial Reminder
            // Set Benchmark Date to April 1
            var BenchmarkDate = DateTime.Parse("4/1/2016");
            //Operate only on 20 applications per call
            var picks = schedules.Where(a => ((a.Approved == null && a.Accepted == null) || (a.Accepted == null && a.WaiverRequest!=true && a.ScheduleExpired == null) && (a.Date > BenchmarkDate))).Take(20).OrderBy(a => a.Date).ToList();
            foreach (var pick in picks)
            {
                var test =_context.ManagerReminders.Where(a => a.ScheduleId == pick.Id && a.DateAdded < DateTime.Now).FirstOrDefault();
                if (test == null)
                {
                    schedules.Add(pick);
                }
               break;
            }
            #endregion

            #region Final Call
            var reminders = _managerRem.OrderBy(a => a.DateAdded).Take(20);
            List<MeetingSchedules> sches = new List<MeetingSchedules>();
            foreach (var item in reminders)
            {
                if (item.DateAdded.Value.AddHours(24) < DateTime.Now)
                {
                    var x = schedules.Where(a => a.Id == item.ScheduleId).FirstOrDefault();
                    if (x != null)
                        sches.Add(x);
                }
            }
            if (sches.Count() > 0)
                schedules.AddRange(sches);

            Schedule(schedules);
            #endregion

            return Json("Good");
        }

        private void LogError(string message)
        {
            try
            {
                //Log progress
                RunTimes cw = new RunTimes();
                cw.ResponseMessage = message;
                cw.LastRunTime = DateTime.Now;
                _context.SaveChanges();
            }
            catch (Exception)
            {
                //Do nothing
            }
        }

        private bool SendManagerPresentationReminder(MeetingSchedules schedule)
        {
            try
            {
                #region Check if Remincer has been send before
                var rem = _managerRem.Where(a => a.ScheduleId == schedule.Id).FirstOrDefault();
                if (rem != null)
                    return true;
                #endregion
                var ap = _context.applications.Where(a => a.company_id == schedule.ApplicationId).FirstOrDefault();
                var client = _context.companies.Where(a => a.id == ap.company_id).FirstOrDefault();
                var mgr = _context.ManagerScheduleMeetings.Where(m => m.ScheduleId == schedule.Id).FirstOrDefault().UserId;
                var staff = _context.Staff.Where(a => a.StaffEmail.ToLower() == mgr.ToLower()).FirstOrDefault();
               
                var mgrEmail = staff.StaffEmail;

                #region Not yet approved, send Notification mail

                var emailContent = string.Format("Your attention is needed to continue the Application process of an application. <br />" +
                    "An Inspector in your department  has schedule a Presentation for a client on Depot. <br />" +
                    "Details of the Schedule is as follows: <br /><table cellpadding='3' cellspaccing='0' border='0'>" +
                    "<tr><td>Application Reference</td><td>{0}</td></tr>" + "<tr><td>Application Type</td><td>{1}</td></tr>" +
                    "<tr><td>Client Name</td><td>{2}</td></tr>" + "<tr><td>Presentation Scheduled by</td><td>{3}</td></tr>" +
                    "</table><br/><b>Note:</b> This notice is coming <strong>24 hours</strong> after the Inspector has requested for the Presentation schedule approval. " +
                    "You have 24 hour more to act on this application, afterwhich the system " +
                    "will automatically assume you have accepted the <strong>Presentation Schedule</strong> by the Inspector.<br /> Please " +
                    "<a href=\"https://www.Depot.dpr.gov.ng\">Log in</a> to your Depot account to act on it now.",
                    ap.reference, ap.type, client.name, schedule.StaffUserName);

                string body = string.Empty;
                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                string file = up + @"\\Templates\" + "InternalMemo.txt";
                using (var sr = new StreamReader(file))
                {

                    body = sr.ReadToEnd();
                }

                var subject = "Presentation Schedule Reminder";
                var msgNo = "";
                var man = staff.FirstName + " " + staff.LastName;
                var msgBody = string.Format(body, subject, msgNo, man, emailContent);
                // _helpersController.SendEmailMessage(mgrEmail, subject, msgBody);
                var emailMsg3 = _helpersController.SaveMessage(ap.id, staff.StaffID, subject, emailContent, staff.StaffElpsID, "Staff");
                var sendEmail3 = _helpersController.SendEmailMessage2Staff(staff.StaffEmail, staff.FirstName, emailMsg3, null);

                #endregion

                #region SaveReminder t avoid repetition
                ManagerReminders manRem = new ManagerReminders();
                manRem.Id = Guid.NewGuid();
                manRem.ScheduleId = schedule.Id;
                manRem.ReminderSent = true;
                manRem.DateAdded = DateTime.Now;
                _managerRem.Add(manRem);
                _context.SaveChanges();
                #endregion

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void AcceptPresentationSchedule(MeetingSchedules schedule)
        {

            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userRole == "Error")
            {
            
            }

            using (var trans = new TransactionScope())
            {
                try
                {
                    var model = schedule;
                    var history = new ApplicationDeskHistoryModel();
                    var ap = _context.applications.Where(a => a.company_id == schedule.ApplicationId).FirstOrDefault();
                    var client = _context.companies.Where(a => a.id == ap.company_id).FirstOrDefault();
                   
                    #region Notify Client

                    string body = string.Empty;
                    var url = Url.Action("CompanySchedule", "Process", new { Id = model.Id });
                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    string file = up + @"\\Templates\" + "ScheduleMeeting.txt";
                    using (var sr = new StreamReader(file))
                    {

                        body = sr.ReadToEnd();
                    }

                    var subject = "Scheduled for a Presentation by NMDPRA";
                    var address = model.Address;
                    var msgBody = string.Format(body, subject, client.name, model.Venue, model.MeetingDate, model.Message, url, address);

                    var emailMsg = _helpersController.SaveMessage(model.ApplicationId, client.id, subject, msgBody, client.elps_id.ToString(), "Company");
                    var sendEmail = _helpersController.SendEmailMessage(client.CompanyEmail.ToString(), client.name, emailMsg, null);
                   

                    model.Approved = true;
                    model.ApprovedDate = DateTime.Now;
                    if ( _context.SaveChanges() > 0)
                    {
                        _helpersController.SaveHistory(model.ApplicationId, userID, userEmail, GeneralClass.Move, "Manager Approved the Presentation Schedule");

                    }

                    #region Remove Reminder since it has been worked upon
                    var manrem = _managerRem.Where(a => a.ScheduleId == schedule.Id).FirstOrDefault();
                    if (manrem != null)
                    {
                        _managerRem.Remove(manrem);
                        _context.SaveChanges();
                    }
                    #endregion

                    trans.Complete();
                }
                catch (Exception)
                {
                    //Do nothing
                    trans.Dispose();
                }
            }

        }

        private void SetPresentationToExpired(MeetingSchedules schedule)
        {
            using (var trans = new TransactionScope())
            {
                try
                {
                    
                    var scheduleToEdit = schedules.Where(a => a.Id == schedule.Id).FirstOrDefault();
                    scheduleToEdit.ScheduleExpired = true;
                    _context.SaveChanges();


                    //remove holding from Application_Processing
                    var ap = _context.applications.Where(a => a.company_id == scheduleToEdit.ApplicationId).FirstOrDefault();
                    var currentDesk = _context.MyDesk.Where(a => a.StaffID == ap.current_desk && a.AppId== ap.current_desk).FirstOrDefault();

                    if (currentDesk != null && currentDesk.Holding == true)
                    {
                        currentDesk.Holding = false;
                    }
                    _context.SaveChanges();
                    trans.Complete();
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    LogError(ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Work on Schedules with Waiver
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns>int: 0 = Waiver not Found; 1 = Pass; -1 = error occured</returns>
        private int WaiverSchedule(MeetingSchedules schedule)
        {
            var checkpoint = DateTime.Now;

            #region Waiver on schedule Check from Manager
            //Get the waiver in the schedule
            var waiver = _context.Waivers.Where(w => w.entityId == schedule.Id).FirstOrDefault();

            if (waiver == null)
                return 0;
            if (waiver != null && schedule.Approved == null && schedule.Date.AddHours(48) < checkpoint)
            {
                //This is more than 48 hour, So accept for Manager
                if (AcceptPresentationWaiver(schedule, waiver))
                    return 1;
                else
                    return -1;
            }
            else if (waiver != null && schedule.Approved == null && schedule.Date.AddHours(24) < checkpoint)
            {
                //More than 24 hour, resend notification mail
                string body = string.Empty;
                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string file = up + @"\\Templates\" + "InternalMemo.txt";
                using (var sr = new StreamReader(file))
                {

                    body = sr.ReadToEnd();
                }
             
                if (SendPresentationWaiverReminder(schedule, waiver.ID, body))
                    return 1;
                else
                    return -1;
            }
            else
            {
                // Waiver not found/Manager approved/still less than 24hrs; do nothing
                return 1;
            }
            #endregion 
        
        }

        public bool AcceptPresentationWaiver(MeetingSchedules schedule, Waivers waiver, string ip = ":1")
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userRole == "Error")
            {

            }
            using (var trans = new TransactionScope())
            {
                try
                {
                    var waiverToSave = _context.Waivers.Where(w => w.ID == waiver.ID).FirstOrDefault();
                    var ap = _context.applications.Where(a => a.company_id == waiver.ApplicationId).FirstOrDefault();
                    var company = _context.companies.Where(x => x.id == ap.company_id).FirstOrDefault();

                    if (waiverToSave.WaiverFor.ToLower() == "presentation")
                    {
                        var scheduleToSave = schedules.Where(m => m.Id == schedule.Id).FirstOrDefault();
                        scheduleToSave.Approved = true;
                        scheduleToSave.ApprovedDate = DateTime.Now;
                        _context.SaveChanges();

                        waiverToSave.Approved = true;
                        waiverToSave.ManagerReason = "System auto Approval after 48 hours of request by Inspector and no response by Manager";
                        waiverToSave.ManagerResponseDate = DateTime.Now;
                        _context.SaveChanges();
                        //remove holding from Application_Processing
                        var currentDesk = _context.MyDesk.Where(a => a.StaffID == ap.current_desk && a.AppId == ap.current_desk).FirstOrDefault();
                        var currentOffice = _context.Staff.Where(a => a.StaffID == ap.current_desk).FirstOrDefault();

                        if (currentDesk != null)
                        {
                            currentDesk.HasWork = true;
                            currentDesk.UpdatedAt = DateTime.Now;
                            currentDesk.Holding = false;
                        }
                        _context.SaveChanges();

                        string comment = "The System automatically Approve Presentation Waiver Request after 48 hours of Manager not responding to the schedule";
                        _helpersController.SaveHistory(schedule.ApplicationId, userID, userEmail, GeneralClass.Processing, comment);


                        var notification = new Notifications();
                        notification.DateAdded = DateTime.Now;
                        notification.Deleted = false;
                        notification.IsRead = false;
                        notification.Message = "Your Presentation Waiver request for Application with Ref. No. " + ap.reference + " has been APPROVED by the System.";
                        notification.ToStaff = waiver.RequestFrom;
                        _context.Notifications.Add(notification);
                        _context.SaveChanges();

                        #endregion

                        #region Pass to the Next Desk
                        //Get a staff for the next Process
                        List<WorkProccess> process = _helpersController.GetAppProcess( ap.PhaseId, ap.category_id, currentDesk.ProcessID, currentDesk.Sort);
                        if (process.Count <= 0)
                        {
                            string err = "Something went wrong while trying to get work process for your application. Please try again or contact Support.";                            //return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });

                        }

                        var result = _helpersController.Assign(ap.id, currentDesk.DeskID, currentDesk.ProcessID, userEmail,"Waiver for Inspection",null, (int)currentOffice.FieldOfficeID);
                        int AppDropStaffID = _helpersController.ApplicationDropStaff(ap.id, ap.PhaseId, ap.category_id, (int)currentOffice.FieldOfficeID, 0);

                        if (AppDropStaffID <= 0)
                        {
                            string err = "Something went wrong while trying to send your application to a staff for processing. Please try again or contact Support.";
                            //return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });

                        }
                        else
                        {

                            //add to old table
                            var staff = _context.Staff.Where(x => x.StaffID == AppDropStaffID).FirstOrDefault();

                            MyDesk drop = new MyDesk()
                            {
                                ProcessID = process.FirstOrDefault().ProccessID,
                                Sort = process.FirstOrDefault().Sort,
                                AppId = ap.id,
                                StaffID = AppDropStaffID,
                                FromStaffID = 0,
                                HasWork = false,
                                HasPushed = false,
                                CreatedAt = DateTime.Now
                            };

                            // dropping application on staff desk
                            _context.MyDesk.Add(drop);
                            ap.current_desk = AppDropStaffID;
                            int appDrop = _context.SaveChanges();

                            if (appDrop > 0)
                            {

                                string commenthis = "Application landed on Staff Desk";

                                _helpersController.SaveHistory(ap.id, userID, userEmail, GeneralClass.Rejected, commenthis);


                                #region Not yet approved, send Notification mail
                                string body = string.Empty;
                             
                                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                                string file = up + @"\\Templates\" + "InternalMemo.txt";
                                using (var sr = new StreamReader(file))
                                {

                                    body = sr.ReadToEnd();
                                }


                                //var mgrEmail = _vUserBranch.Where(m => m.UserEmail.ToLower() == waiverToSave.AssignedManager.ToLower()).FirstOrDefault();

                                var emailContent = string.Format("This is to inform you that an Application on your desk that required your APPROVAL to continue processing " +
                                    "has been auto APPROVED. The Inspector requeste for a Presentation Waiver on the application. <br />The auto approval is due to the fact that you did not respond to the application for more than 48 hours of the request." +
                                    "Details of the application is as follows: <br /><table cellpadding='3' cellspaccing='0' border='0'>" +
                                    "<tr><td>Waiver Requested by</td><td>{0}</td></tr>" + "<tr><td>Application Reference</td><td>{1}</td></tr>" +
                                    "<tr><td>Waiver Type</td><td>PRESENTATION WAIVER</td></tr>" + "<tr><td>Date Requested</td><td>{2}</td></tr>" +
                                    "</table><br/><br/>", waiverToSave.RequestFrom, ap.reference, waiverToSave.RequestDate);

                                var subject = "Presentation Waiver Auto-Approved";
                                var msgNo = "";
                                var man = staff.FirstName + " " + staff.LastName;
                                var msgBody = string.Format(body, subject, msgNo, man, emailContent);
                                //_helpersController.SendEmail(waiverToSave.AssignedManager, subject, msgBody, waiverToSave.RequestFrom);
                                var emailMsg = _helpersController.SaveMessage(ap.id, userID, subject, msgBody, staff.StaffElpsID.ToString(), waiverToSave.RequestFrom);
                                var sendEmail = _helpersController.SendEmailMessage(waiverToSave.AssignedManager, man, emailMsg, null);

                                #endregion

                                #region Remove Reminder since it has been worked upon
                                var manrem = _managerRem.Where(a => a.ScheduleId == schedule.Id).FirstOrDefault();
                                if (manrem != null)
                                {
                                    _managerRem.Remove(manrem);
                                    _context.SaveChanges();
                                }
                                #endregion
                                trans.Complete();
                                return true;
                            }
                            else if (waiverToSave.WaiverFor.ToLower() == "double")
                            {
                                //Reject Request

                                scheduleToSave.Approved = false;
                                scheduleToSave.ApprovedDate = DateTime.Now;
                                //_context.SaveChanges();

                                waiverToSave.Approved = false;
                                waiverToSave.ManagerReason = "System auto decline double waiver after 48 hours of request by Inspector and no response by Manager";
                                waiverToSave.ManagerResponseDate = DateTime.Now;
                                //_context.SaveChanges();


                                currentDesk.HasWork = false;
                                currentDesk.Holding = false;

                                _context.SaveChanges();

                                string commentHis = "The System automatically Decline Double Waiver Request after 48 hour of Manager not responding to the request";
                                _helpersController.SaveHistory(schedule.ApplicationId, userID, userEmail, GeneralClass.Rejected, commentHis);

                                #region Remove Reminder since it has been worked upon
                                var manrem = _managerRem.Where(a => a.ScheduleId == schedule.Id).FirstOrDefault();
                                if (manrem != null)
                                {
                                    _managerRem.Remove(manrem);
                                    _context.SaveChanges();
                                }
                                #endregion

                                trans.Complete();
                                return true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    trans.Dispose();
                }
            }
            return false;
        }

        public bool SendPresentationWaiverReminder(MeetingSchedules schedule, int waiverId, string body, string ip = ":1")
        {
            try
            {
                #region Check if Remincer has been send before
                var rem = _managerRem.Where(a => a.ScheduleId == schedule.Id).FirstOrDefault();
                if (rem != null)
                    return true;
                #endregion
                var ap = _context.applications.Where(a => a.company_id == schedule.ApplicationId).FirstOrDefault();
                var client = _context.companies.Where(a => a.id == ap.company_id).FirstOrDefault();
                var mgr = _context.ManagerScheduleMeetings.Where(m => m.ScheduleId == schedule.Id).FirstOrDefault().UserId;
                var staff = _context.Staff.Where(a => a.StaffEmail.ToLower() == mgr.ToLower()).FirstOrDefault();

                var mgrEmail = staff.StaffEmail;
                
                #region Not yet approved, send Notification mail
                var waiver = _context.Waivers.Where(w => w.ID== waiverId).FirstOrDefault();
                string CategoryName = _context.Categories.Where(ct => ct.id == ap.category_id).FirstOrDefault().name;

                var emailContent = string.Format("Your attention is needed to continue the Application process of an application. <br /><strong>" +
                    (waiver.WaiverFor.ToLower() == "inspection" ? "An " : "A ") + waiver.WaiverFor.ToUpperInvariant() + " Waiver</strong> " +
                    "is requested by an Inspector in your department for an Depot permit application. <br />" +
                    "Details of the application is as follows: <br /><table cellpadding='3' cellspaccing='0' border='0'>" +
                    "<tr><td>Application Reference</td><td>{0}</td></tr>" + "<tr><td>Application Type</td><td>{1}</td></tr>" +
                    "<tr><td>Category</td><td>{2}</td></tr>" + "<tr><td>Waiver Requested by</td><td>{3}</td></tr>" +
                    "</table><br/><br/><b>Note:</b> This notice is coming <strong>24 hours</strong> after the Inspector has requested for the Presentation to be Waived. ",
                    ap.reference, ap.type, CategoryName, schedule.StaffUserName);
                
                if (waiver.WaiverFor.ToLower() == "presentation")
                {
                    emailContent += "You have 24 hour more to act on this application, afterwhich the system " +
                    "will automatically assume you have <strong>Accepted the PRESENTATION WAIVER request</strong>.<br />";
                }
                else
                {
                    emailContent += "You have 24 hour more to act on this application, afterwhich the system " +
                    "will automatically assume you wish to <strong>Decline the " + waiver.WaiverFor.ToUpper() + " Waiver request</strong>.<br />";
                }
                emailContent += "Please <a href=\"https://www.Depot.dpr.gov.ng\">Log in</a> to your ROMSP account to act on it now.";

                var subject = "Waiver Request Reminder";
                var msgNo = "";
                var man = staff.FirstName + " " + staff.LastName;
                var msgBody = string.Format(body, subject, msgNo, man, emailContent);

                #endregion

                #region SaveReminder to avoid repetition
                ManagerReminders manRem = new ManagerReminders();
                manRem.Id = Guid.NewGuid();
                manRem.ScheduleId = schedule.Id;
                manRem.ReminderSent = true;
                manRem.DateAdded = DateTime.Now;

                _managerRem.Add(manRem);
                _context.SaveChanges();
                #endregion

                //_helpersController.SendEmail(waiverToSave.AssignedManager, subject, msgBody, waiverToSave.RequestFrom);
                var emailMsg = _helpersController.SaveMessage(ap.id, staff.StaffID, subject, msgBody, staff.StaffElpsID.ToString(), waiver.RequestFrom);
                var sendEmail = _helpersController.SendEmailMessage(mgrEmail, man, emailMsg, null);

                return true;
            }
            catch (Exception)
            {
                return false;
                //throw;
            }

        }

        private void Schedule(List<MeetingSchedules> schedules)
        {
            foreach (var schedule in schedules)
            {
                if (schedule.ScheduleExpired == null || !schedule.ScheduleExpired.Value)
                {
                    #region Schedule Not expired
                    if (schedule.WaiverRequest == true)
                    {
                        var responseCode = WaiverSchedule(schedule);
                        string msg = "";
                        switch (responseCode)
                        {
                            case -1:
                                //Error
                                msg = "An Error occured while processing SCHEDULE, scheduleID: " + schedule.Id;
                                LogError(msg);
                                break;
                            case 0:
                                //Waiver not found, render schedule expired and approval to false
                                SetPresentationToExpired(schedule);
                                break;
                            case 1:
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        #region No Waiver on schedule
                        //Schedule(schedule);
                        if (schedule.Approved == null)
                        {
                            //Not worked on atall
                            if (schedule.Date.AddHours(48) < DateTime.Now)
                            {
                                //This is more than 48 hour, So accept for Manager and alert client
                                AcceptPresentationSchedule(schedule);
                            }
                            else if (schedule.Date.AddHours(24) < DateTime.Now)
                            {
                                //More than 24 hour, resend notification mail
                                SendManagerPresentationReminder(schedule);
                            }
                        }
                        else //Worked on already 
                        {
                            if (schedule.Approved.Value == true)    //Manager Accepted
                            {
                                if (schedule.Accepted == null && schedule.ApprovedDate.Value.AddHours(72) < DateTime.Now)
                                {
                                    //72 hours over for client to accept, Render the Schedule Expired
                                    #region Presentation Expiry (If Expired, set Holding to False)
                                    SetPresentationToExpired(schedule);
                                    //dum = 3;
                                    //storeDB.SaveChanges("System", ":1");
                                    #endregion
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }
        //public JsonResult DeleteSchedule(string ScheduleID)
        //{
        //    if (generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName)) == "Error")
        //    {
        //        return Json("Error");
        //    }
        //    else
        //    {
        //        string result = "";

        //        int rID = 0;
        //        var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName));
        //        int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionuserId));
        //        int schid = generalClass.DecryptIDs(ScheduleID.ToString().Trim());

        //        if (schid == 0 && userID == 0)
        //        {
        //            result = "Straddle link error";
        //        }
        //        else
        //        {
        //            var get = _context.Schedules.Where(x => x.ScheduleID == schid && x.DeletedStatus != true);
        //            var getStrads = _context.Straddle.Where(x => x.Strad_Id == get.FirstOrDefault().StradID && x.DeleteStatus == false);
        //            var stage = _context.StraddleStage.Where(x => x.StradStageID == (int)getStrads.FirstOrDefault().CurrentStageID && x.DeleteStatus == false);
        //            var getCompanyA = _context.Party.Where(x => x.partyId == getStrads.FirstOrDefault().Party_Id && x.DeleteStatus == false && x.ActiveStatus == true).FirstOrDefault();
        //            var getCompanyB = _context.Party.Where(x => x.CompanyName == getStrads.FirstOrDefault().PartyBName.Trim() && x.DeleteStatus == false && x.ActiveStatus == true).FirstOrDefault();
        //            int stradID = getStrads.FirstOrDefault().Strad_Id;
        //            if (get.Count() > 0 && getStrads.Count() > 0 && stage.Count() > 0 && getCompanyA != null && getCompanyB != null)
        //            {
        //                var getStaff = (from u in _context.Staff where u.StaffID == userID && u.DeleteStatus != true && u.ActiveStatus != false select u).FirstOrDefault();
        //                string smstaffSubj = get.FirstOrDefault().ScheduleType + " Schedule Cancelled for Straddle with Ref " + getStrads.FirstOrDefault().StradRefNO + ".";
        //                string smstaffCont = getStaff.StaffEmail + " has cancelled a " + get.FirstOrDefault().ScheduleDate + " that was initially proposed for " + get.FirstOrDefault().ScheduleDate + " with " + get.FirstOrDefault().WhichParty + ".";

        //                var oldDate = get.FirstOrDefault().ScheduleDate;
        //                var oldLocation = get.FirstOrDefault().ScheduleLocation;
        //                var oldType = get.FirstOrDefault().ScheduleType;

        //                if (generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName)) == GeneralClass.DESK_OFFICER)
        //                {
        //                    //check if Desk Officer created schedule
        //                    ViewBag.Role = "DESKOFFICER";
        //                }
        //                else if (generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName)) == GeneralClass.MANAGER)
        //                {
        //                    ViewBag.Role = "MANAGER";
        //                }
        //                else
        //                {
        //                    ViewBag.Role = "ADRM";
        //                }
        //                var getDesk = (from u in _context.MyDesk where u.StaffID == userID && u.StradID == stradID && u.hasPushed != true && u.hasWork != true select u);
        //                int StradDropStaffID = _helpersController.StraddleDropStaff(getStrads.FirstOrDefault().Strad_Id, (int)getStrads.FirstOrDefault().CurrentStageID, (getDesk.FirstOrDefault().Sort + 1));

        //                if (StradDropStaffID != 0)
        //                {
        //                    //check if next staff is not on leave
        //                    //var outOfOffice = (from u in _context.OutOfOffice where u.StaffID == StradDropStaffID && u.Status != GeneralClass._FINISHED select u).FirstOrDefault();
        //                    //if (outOfOffice != null)
        //                    //{
        //                    //    StradDropStaffID = outOfOffice.ReliverID;
        //                    //}
        //                    var getStaffInfo = (from u in _context.Staff where u.StaffID == StradDropStaffID && u.DeleteStatus != true && u.ActiveStatus != false select u).FirstOrDefault();
        //                    var StaffemailMsg = _helpersController.SaveStaffMessage(getStrads.FirstOrDefault().Strad_Id, getStaffInfo.StaffEmail, smstaffSubj, smstaffCont, getStaffInfo.StaffElpsID);
        //                    var sendEmail2Staff = _helpersController.SendEmailMessage2Staff(getStaffInfo.StaffEmail, getStaffInfo.FirstName, StaffemailMsg, null);

        //                }
        //                get.FirstOrDefault().DeletedBy = userID;
        //                get.FirstOrDefault().DeletedStatus = true;
        //                get.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
        //                int i = _context.SaveChanges();
        //                result = "Schedule Deleted";

        //                if (get.FirstOrDefault().WhichParty == "Party A" && get.FirstOrDefault().PartyA_Accept != 0)
        //                {
        //                    // Saving Messages to party A
        //                    string subject = "Cancellation of " + get.FirstOrDefault().ScheduleType + "Scheduled for Straddle with Ref : " + getStrads.FirstOrDefault().StradRefNO;
        //                    string content = "The " + get.FirstOrDefault().ScheduleType + "Scheduled For " + get.FirstOrDefault().ScheduleDate + " At " + get.FirstOrDefault().ScheduleLocation + " Have Been Cancelled and Will no Longer Hold on Proposed Date. Kindly find other details below.";
        //                    var emailMsg = _helpersController.SaveMessage(stradID, getCompanyA.partyId, subject, content, getCompanyA.CompanyElpsID.ToString());
        //                    var sendEmail = _helpersController.SendEmailMessage(getCompanyA.Company_Email, getCompanyA.CompanyName, emailMsg, null);
        //                }
        //                else if (get.FirstOrDefault().WhichParty == "Party B" && get.FirstOrDefault().PartyB_Accept != 0)
        //                {
        //                    // Saving Messages to party B
        //                    string subject2 = "Cancellation of " + get.FirstOrDefault().ScheduleType + "Scheduled for Straddle with Ref : " + getStrads.FirstOrDefault().StradRefNO;
        //                    string content2 = "The " + get.FirstOrDefault().ScheduleType + "Scheduled For " + get.FirstOrDefault().ScheduleDate + " At " + get.FirstOrDefault().ScheduleLocation + " Have Been Cancelled and Will no Longer Hold on Proposed Date. Kindly find other details below.";
        //                    var emailMsg2 = _helpersController.SaveMessage(stradID, getCompanyB.partyId, subject2, content2, getCompanyB.CompanyElpsID.ToString());
        //                    var sendEmail2 = _helpersController.SendEmailMessage(getCompanyB.Company_Email, getCompanyB.CompanyName, emailMsg2, null);

        //                }
        //                else
        //                {
        //                    // Saving Messages to party A
        //                    if (get.FirstOrDefault().PartyA_Accept != 0)
        //                    {
        //                        string subject = "Cancellation of " + get.FirstOrDefault().ScheduleType + "Scheduled for Straddle with Ref : " + getStrads.FirstOrDefault().StradRefNO;
        //                        string content = "The " + get.FirstOrDefault().ScheduleType + "Scheduled For " + get.FirstOrDefault().ScheduleDate + " At " + get.FirstOrDefault().ScheduleLocation + " With " + getCompanyB.CompanyName + " Have Been Cancelled and Will no Longer Hold on Proposed Date. Kindly find other details below.";
        //                        var emailMsg = _helpersController.SaveMessage(stradID, getCompanyA.partyId, subject, content, getCompanyA.CompanyElpsID.ToString());
        //                        var sendEmail = _helpersController.SendEmailMessage(getCompanyA.Company_Email, getCompanyA.CompanyName, emailMsg, null);

        //                    }// Saving Messages to party B
        //                    if (get.FirstOrDefault().PartyB_Accept != 0)
        //                    {

        //                        string subject2 = "Cancellation of " + get.FirstOrDefault().ScheduleType + "Scheduled for Straddle with Ref : " + getStrads.FirstOrDefault().StradRefNO;
        //                        string content2 = "The " + get.FirstOrDefault().ScheduleType + "Scheduled For " + get.FirstOrDefault().ScheduleDate + " At " + get.FirstOrDefault().ScheduleLocation + " With " + getCompanyA.CompanyName + " Have Been Cancelled and Will no Longer Hold on Proposed Date. Kindly find other details below.";
        //                        var emailMsg2 = _helpersController.SaveMessage(stradID, getCompanyB.partyId, subject2, content2, getCompanyB.CompanyElpsID.ToString());
        //                        var sendEmail2 = _helpersController.SendEmailMessage(getCompanyB.Company_Email, getCompanyB.CompanyName, emailMsg2, null);
        //                    }
        //                }

        //                if (i > 0)
        //                {
        //                    result = "Schedule Deleted";
        //                    _helpersController.SaveHistory(stradID, Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionuserId))), "Schedule", "A Straddle " + oldType + " Schedule Was Cancelled. ");
        //                }

        //                else
        //                {
        //                    result = "Something went wrong trying to update your schedule";
        //                }

        //            }
        //            else
        //            {
        //                result = "Sorry schedule/straddle/stage/partyA/partyB wasn't found.";
        //            }

        //        }

        //        _helpersController.LogMessages("Schedule Status : " + result + ". Schedule ID : " + schid, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

        //        return Json(result);
        //    }
        //}


    }
    #endregion
    #endregion
}