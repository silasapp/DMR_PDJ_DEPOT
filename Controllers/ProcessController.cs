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
using NewDepot.Payments;

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
using Staff = NewDepot.Models.Staff;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NewDepot.Controllers
{
    public class ProcessController : Controller
    {
        SubmittedDocuments _appDocRep;
        RestSharpServices _restService = new RestSharpServices();
        public IConfiguration _configuration;
        IHttpContextAccessor _httpContextAccessor;
        private readonly Depot_DBContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        ElpsResponse elpsResponse = new ElpsResponse();
        ApplicationHelper appHelper;
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;

        public ProcessController(IHostingEnvironment hostingEnvironment, Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }

        /// Displays all applications on staff desk
        public IActionResult MyDesk()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var myApps = new List<MyApps>();
            var Relievestaff = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRelieveStaff));

            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"].ToString();
                ViewBag.MsgType = TempData["msgType"].ToString();
                TempData.Clear();
            }


            if (userRole.ToUpper() == GeneralClass.ED_STA.ToUpper())
            {

                var ED_Info = (from stf in _context.Staff
                               join rl in _context.UserRoles on stf.RoleID equals rl.Role_id
                               where rl.RoleName == GeneralClass.ED && stf.DeleteStatus != true
                               select stf).FirstOrDefault();
                userID = ED_Info.StaffID;
            }

            if (userRole.ToUpper() == GeneralClass.ACE_STA.ToUpper())
            {

                var ACE_Info = (from stf in _context.Staff
                                join rl in _context.UserRoles on stf.RoleID equals rl.Role_id
                                where rl.RoleName == GeneralClass.AUTHORITY && stf.DeleteStatus != true
                                select stf).FirstOrDefault();
                userID = ACE_Info.StaffID;
            }


            var myDesk = _context.MyDesk.Where(x => /*x.HasWork != true* &&*/ x.StaffID == userID).ToList();
            myApps = (from proc in _context.MyDesk.AsEnumerable()
                      join wp in _context.WorkProccess.AsEnumerable() on proc.ProcessID equals wp.ProccessID
                      join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                      join h in _context.application_desk_histories.AsEnumerable() on app.id equals h.application_id into hist
                      join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                      join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                      join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                      join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                      join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                      join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                      where proc.StaffID == userID && proc.HasWork != true && app.DeleteStatus != true && c.DeleteStatus != true
                      select new MyApps
                      {
                          Assigned = hist.Count() > 2 ? true : false, //check again
                          appID = app.id,
                          Reference = app.reference,
                          CategoryName = cat.name,
                          PhaseName = phs.name,
                          Status = app.status,
                          Date_Added = Convert.ToDateTime(app.date_added),
                          DateSubmitted = app.CreatedAt == null ? Convert.ToDateTime(app.date_added) : Convert.ToDateTime(app.CreatedAt),
                          Submitted = app.submitted,
                          CompanyDetails = c.name + " (" + c.Address + ") ",
                          CompanyName = c.name,
                          Company_Id = c.id,
                          FacilityId = fac.Id,
                          FacilityDetails = fac.Name,
                          FacilityName = fac.Name,
                          processID = proc.ProcessID,
                          currentDeskID = myDesk.Where(x => x.AppId == app.id).FirstOrDefault() != null ? myDesk.Where(x => x.AppId == app.id).FirstOrDefault().DeskID : 0,
                          DateProcessed = proc.CreatedAt,
                          Year = app.year,
                          Type = app.type.ToUpper(),
                          Holding = proc.Holding,
                          Address_1 = ad.address_1,
                          City = ad.city,
                          StateName = sd.StateName,
                          LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                          Activity = wp.Sort == 1 ? "(10%) Approve/Reject" :
                             wp.Sort == 2 ? "(20%) Approve/Reject: Conduct Inspection" :
                             wp.Sort == 3 ? "(30%) Approve/Reject: Conduct Inspection" :
                             wp.Sort == 4 ? "(40%) Approve/Reject: Approve Inspection" :
                             wp.Sort == 5 ? "(50%) Approve/Reject" :
                             wp.Sort == 6 ? "(60%) Approve/Reject" :
                             wp.Sort == 7 ? "(70%) Approve/Reject" :
                             wp.Sort == 8 ? "(80%) Approve/Reject" :
                             wp.Sort == 9 ? "(85%) Approve/Reject" :
                             wp.Sort == 10 ? "(90%) Approve/Reject" :
                             wp.Sort == 11 ? "(95%) Approve/Reject" :
                             wp.Sort == 12 ? "(98%) Final Approval/Reject " : "Approval/License Granted"

                          //AllowPush=proc.AllowPush
                      }).ToList();

            if (userRole.Contains("Opscon") || userRole.Contains("TeamLead") || userRole.Contains("AdOps"))
            {
                if (myApps.Count() < 1)
                {
                    myApps = new List<MyApps>();

                }
                _helpersController.LogMessages($"Yes, User is Opscon with Id of: {userEmail}");
                ViewBag.myDesk = myDesk;
                return View("MyDeskFD", myApps);

            }
            else
            {
                return View(myApps);
            }
        }
        public IActionResult JointApplications()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var myApp = new List<MyApps>();

            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"].ToString();
                ViewBag.MsgType = TempData["msgType"].ToString();
                TempData.Clear();
            }

            var getCStaff = _context.Staff.Where(x => x.StaffID == userID && x.ActiveStatus != false && x.DeleteStatus != true).FirstOrDefault();
            var myFieldOffice = _context.FieldOffices.Where(u => u.FieldOffice_id == getCStaff.FieldOfficeID).FirstOrDefault();
            ViewBag.FieldOffice = myFieldOffice.OfficeName;
            var jointAcc = _context.JointAccounts.Where(a => a.Opscon.ToLower() == userEmail.ToLower() && a.OperationsCompleted != true).ToList();
            ViewBag.MyApplications = jointAcc;

            if (jointAcc.Count() > 0)
            {
                myApp = (from app in _context.applications.AsEnumerable()
                         join h in _context.application_desk_histories.AsEnumerable() on app.id equals h.application_id into hist
                         join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                         join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                         join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                         join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                         join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                         join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                         join acc in _context.JointAccounts.AsEnumerable() on app.id equals acc.ApplicationId
                         where acc.Opscon.ToLower() == userEmail.ToLower()
                         && acc.OperationsCompleted != true && app.DeleteStatus != true && c.DeleteStatus != true
                         select new MyApps
                         {
                             Assigned = hist.Count() > 2 ? true : false, //check again
                             appID = app.id,
                             Reference = app.reference,
                             CategoryName = cat.name,
                             PhaseName = phs.name,
                             Status = app.status,
                             Date_Added = Convert.ToDateTime(app.date_added),
                             DateSubmitted = app.CreatedAt == null ? Convert.ToDateTime(app.date_added) : Convert.ToDateTime(app.CreatedAt),
                             Submitted = app.submitted,
                             CompanyDetails = c.name + " (" + c.Address + ") ",
                             CompanyName = c.name,
                             Company_Id = c.id,
                             FacilityDetails = fac.Name,
                             FacilityName = fac.Name,
                             processID = 1,
                             currentDeskID = 0,
                             DateProcessed = acc.DateAdded,
                             Year = app.year,
                             Type = app.type,
                             Holding = true,
                             Address_1 = ad.address_1,
                             City = ad.city,
                             StateName = sd.StateName,
                             LGA = ad.city,
                             SinglejointAccount = acc
                         }).ToList();


                myApp.FirstOrDefault().jointAccounts = jointAcc;

            }

            return View(myApp);
        }

        public IActionResult HoldingApplications()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var myApp = new List<application_Processings>();


            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"].ToString();
                ViewBag.MsgType = TempData["msgType"].ToString();
                TempData.Clear();
            }

            myApp = _context.application_Processings.Where(a => a.StaffEmail == userEmail && a.Assigned == true && a.Processed == false && a.Holding != null && a.Holding.Value == false).ToList();
            return View(myApp);
        }
        //
        public IActionResult GetSupervisors()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var getCStaff = _context.Staff.Where(x => x.StaffID == userID && x.ActiveStatus != false && x.DeleteStatus != true).FirstOrDefault();

            #region Load Inspectors under Opscon
            if (userRole.Contains("AD") || userRole.Contains("TeamLead") || userRole.Contains("Supervisor"))
            {
                var myFieldOffice = _context.FieldOffices.Where(u => u.FieldOffice_id == getCStaff.FieldOfficeID).FirstOrDefault();
                var staff = _context.Staff.Where(x => x.DeleteStatus != true && x.ActiveStatus != false).ToList();
                var myOfficeStaff = staff.Where(st => st.FieldOfficeID == getCStaff.FieldOfficeID && st.DeleteStatus != true && st.ActiveStatus != false).ToList();
                string rol = userRole.Contains("AD") ? "supervisor" : "inspector"; //AD pushes to supervisor while both teamLead and supervisor pushes to inspector
                int roleID = 0;
                if (rol != null && rol != "Error")
                {
                    roleID = _context.UserRoles.Where(r => r.RoleName == rol).FirstOrDefault().Role_id;
                }

                if (userRole == GeneralClass.ADPDJ)
                {
                    myOfficeStaff = myOfficeStaff.Where(st => st.LocationID == getCStaff.LocationID && st.RoleID == roleID).ToList();
                }
                else
                {
                    // Get all Inspectors in my zone for possible assignment
                    #region Selecting Inspector from My Branch

                    var selected = staff.Where(a => a.DeleteStatus != true && a.ActiveStatus != false && a.FieldOfficeID == myFieldOffice.FieldOffice_id && a.RoleID == roleID && a.FieldOfficeID == myFieldOffice.FieldOffice_id).ToList();
                    // var selected = _vUsrBranchRep.Where(a => a.FieldId == myFieldOffice.FieldId && a.Role.ToLower() == rol && a.DepartmentId == myFieldOffice.FieldOffice_id).ToList();
                    if (selected.Count() > 0)
                    {
                        foreach (var usrB in selected)
                        {
                            if (usrB != null)
                            {
                                var pick = staff.Where(a => a.StaffEmail.Trim().ToLower() == usrB.StaffEmail.Trim().ToLower()).FirstOrDefault();
                                if (pick != null)
                                {
                                    usrB.FirstName = pick.FirstName;
                                    usrB.LastName = pick.LastName;
                                    myOfficeStaff.Add(usrB);
                                }
                            }
                        }

                    }
                    #endregion

                }

                return View(myOfficeStaff);

            }

            return View();
            #endregion
        }
        public IActionResult GetInspectors()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            #region Load Inspectors under Opscon
            if (userRole == GeneralClass.TEAMLEAD || userRole == GeneralClass.SUPERVISOR)
            {
                var staff = _context.Staff.Where(x => x.ActiveStatus != false && x.DeleteStatus != true).ToList();
                var currentUserFD = staff.Where(x => x.StaffID == userID).FirstOrDefault();
                var myFieldOffice = _context.FieldOffices.Where(u => u.FieldOffice_id == currentUserFD.FieldOfficeID).FirstOrDefault();


                // #region use for live portal for field zone accuracy
                // var client = new WebClient();
                // var response2 = _restService.Response("/api/Branch/ZoneMapping/{email}/{apiHash}", null, "GET", null);
                // var zonesmapping = JsonConvert.DeserializeObject<List<BranchModel>>(response2.ToString());

                //NewDepot.Models.Staff selectedInspectors = null;
                // zones selectedZone = null;

                // selectedZone = zonesmapping.Where(a => a.branchId == myFieldOffice.FieldOffice_id ).FirstOrDefault();
                // if (selectedZone == null)
                // {
                //     //Loop thru the states in each zone for possible selection
                //     selectedZone = zonesmapping.Where(a => a.coveredFieldOffices.Select(s => s.Id).Contains(myFieldOffice.FieldOffice_id)).FirstOrDefault();
                // }

                // if (selectedZone != null)
                // {
                // }
                // #endregion

                var getSelectedOffice =
                                    (from u in _context.Staff.AsEnumerable()
                                     join r in _context.UserRoles on u.RoleID equals r.Role_id
                                     join fd in _context.FieldOffices on u.FieldOfficeID equals fd.FieldOffice_id
                                     join zfd in _context.ZonalFieldOffice on fd.FieldOffice_id equals zfd.FieldOffice_id
                                     join z in _context.ZonalOffice on zfd.Zone_id equals z.Zone_id
                                     where u.FieldOfficeID == currentUserFD.FieldOfficeID && u.DeleteStatus != true && zfd.DeleteStatus != true && z.DeleteStatus != true
                                     && r.RoleName == GeneralClass.INSPECTOR
                                     select new Staff_UserBranchModel
                                     {
                                         StaffId = u.StaffID,
                                         StaffFullName = u.FirstName + " " + u.LastName,
                                         RoleName = r.RoleName,
                                         StaffEmail = u.StaffEmail,
                                         ZoneId = z.Zone_id,
                                         ZoneFieldOfficeID = zfd.FieldOffice_id,
                                         ZoneName = z.ZoneName,
                                         FieldOffice = fd.OfficeName,
                                         FieldId = fd.FieldOffice_id,
                                         Active = u.ActiveStatus
                                     });

                List<Staff_UserBranchModel> pushStaff = new List<Staff_UserBranchModel>();
                if (getSelectedOffice != null)
                {

                    // Get all Inspectors in my Zone for possible assignment
                    #region Selecting Inspector from My Branch

                    foreach (var usr in getSelectedOffice)
                    {

                        pushStaff.Add(usr);

                    }
                }
                #endregion

                ViewBag.FieldOffice = _context.FieldOffices.Where(x => x.DeleteStatus != true);
                return View(pushStaff);
            }

            return View();
            #endregion
        }

        public IActionResult AssignInspector(string inspectors, string selectedApps)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int loginID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionLogin));

            if (userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Kindly log in again.") });
            }
            try
            {
                var getCurrentUserIP = _context.Logins.Where(l => l.LoginID == loginID).FirstOrDefault();
                var insps = inspectors.Split(',');
                var appIds = selectedApps.Split(',');
                // Loop thru each Inspectors to Assign Application
                var usr = userEmail.ToLower();
                var body = "";
                string webRootPath = _hostingEnvironment.WebRootPath;
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var file = Path.Combine(up, "Templates/InternalMemo.txt");

                using (var sr = new StreamReader(file))
                {

                    body = sr.ReadToEnd();
                }
                foreach (var sa in insps)
                {
                    var stfId = Convert.ToInt16(sa);
                    var staff = _context.Staff.Where(a => a.StaffID == stfId).FirstOrDefault();
                    var jApps = _context.JointAccountStaffs.Where(a => a.Staff.ToLower() == staff.StaffEmail.ToLower()).ToList();
                    List<applications> appToAssign = new List<applications>();
                    List<string> compNameNApp = new List<string>();
                    // Loop thru each Application to Assign to the Inspector and check if not previously assigned to the Inspector
                    foreach (var item in appIds)
                    {
                        var appId = Convert.ToInt16(item);
                        var jointAcc = _context.JointAccounts.Where(a => a.ApplicationId == appId && a.Opscon.ToLower() == userEmail.ToLower() && a.OperationsCompleted != true).FirstOrDefault();

                        if (jointAcc != null)
                        {

                            if (jApps.Where(a => a.JointAccountId == jointAcc.Id).FirstOrDefault() == null)
                            {
                                var application = _context.applications.Where(a => a.id == appId).FirstOrDefault();
                                var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                                var companyName = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();


                                appToAssign.Add(application);

                                var jstaff = new JointAccountStaffs()
                                {
                                    ApplicationId = appId,
                                    DateAdded = DateTime.Now,
                                    Staff = staff.StaffEmail,
                                    JointAccountId = jointAcc.Id
                                };

                                var getAppType = (from u in _context.Categories.AsEnumerable()
                                                  join p in _context.Phases.AsEnumerable() on u.id equals p.category_id
                                                  where u.id == application.category_id && p.id == application.PhaseId
                                                  select p).FirstOrDefault();
                                var type = (getAppType.category_id == 1 ? "New Depot License" : "Depot License Renewal").ToString() + "(" + getAppType.name + ")";
                                var msg = $"A new application " + type + " has been submitted on the depot portal and you are hereby notified for Joint Operation towards the issuing of the License/Approval." +
                                            " <p>Details of the Application is as follow:</p>";
                                msg += "<table class'table'>" +
                                    $"<tr><td>Application Reference</td><td><a href='{ _restService._url}/Process/ViewApplication/" + application.id + "'>" + application.reference + "</a></td></tr>" +
                                    $"<tr><td>Application Company</td><td><a href='{_restService._url}/Company/Detail/" + application.company_id + "'>" + companyName.name + "</a></td></tr>" +
                                    //$"<tr><td>Facility</td><td><a href='{_restService._url}/Facility/ViewFacility/" + generalClass.Encrypt(application.FacilityId.ToString()) + "'>" + facility.Name + "(" + facility.address_1 + ")</a></td></tr>" +
                                    $"<tr><td>Facility</td><td><a href='{_restService._url}/Facility/ViewFacility/" + application.FacilityId + "'>" + facility.Name + "(" + facility.address_1 + ")</a></td></tr>" +
                                    "<tr><td>Facility Address</td><td>" + facility.address_1 + "</td></tr>" +
                                    "</table><br /><br /><p>You will be notified on the progress and action required by you as the application process progresses.</p>";


                                compNameNApp.Add(msg);

                                _context.JointAccountStaffs.Add(jstaff);
                                jointAcc.Assigned = true;
                                int i = _context.SaveChanges();
                                if (i > 0)
                                {
                                    var ms = _context.MeetingSchedules.Where(a => a.ApplicationId == appId).OrderByDescending(a => a.Date).FirstOrDefault();
                                    if (ms != null)
                                    {
                                        string[] sf = { staff.StaffEmail };

                                        _helpersController.AddStaffToMeeting(ms.Id, sf, userEmail, getCurrentUserIP.Remote_Ip);


                                    }

                                    string comment = "Inpsector (" + staff != null ? userEmail : sa + ") nominated to work on application";
                                    _helpersController.SaveHistory(appId, userID, userEmail, GeneralClass.Move, comment);

                                    TempData["message"] = "Application has been assigned to the selected inspector(s).";
                                    TempData["msgType"] = "pass";
                                }
                                else
                                {
                                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, an error occured while updating joint inspection record") });

                                }

                                #region Send Email to Inspector

                                var subject = "New Application Assignment on Depot Portal";
                                var mailBase = body;
                                foreach (var subj in compNameNApp)
                                {
                                    var msgBody = string.Format(mailBase, subject, "", staff.FirstName, item);
                                    var emailMsg = _helpersController.SaveMessage(application.id, staff.StaffID, subject, msgBody, staff.StaffElpsID, "Staff");
                                    var sendEmail = _helpersController.SendEmailMessage(staff.StaffEmail, staff.FirstName, emailMsg, null);

                                }

                                #endregion

                            }
                        }

                    }
                }




            }
            catch (Exception ex)
            {
                TempData["message"] = "An error occured while performing your request. Please see comment and try again:" + ex.Message;
                TempData["msgType"] = "fail";

            }

            return RedirectToAction("MyDesk");

        }


        [HttpPost]
        [AllowAnonymous]
        public IActionResult SaveInspectionForm(AppFormApiSubmitModel model)
        {

            string result = ""; int save = 0;
            var respObj = new List<object>();
            var app = _context.applications.Where(a => a.id == model.ApplicationId).FirstOrDefault();
            var comp = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
            var facilitydetails = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
            var addressdetails = _context.addresses.Where(a => a.id == facilitydetails.AddressId).FirstOrDefault();
            var stagedetails = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();

            var frm = _context.Forms.Where(a => a.Id == model.FormId).FirstOrDefault();
            ViewBag.Form = frm.FriendlyName;
            var appForm = _context.ApplicationForms.Where(a => a.FormId == model.FormId.ToString() && a.ApplicationId == model.ApplicationId && a.Filled != true).FirstOrDefault();

            Guid groupId = Guid.Empty;

            if (appForm != null)
            {

                if (appForm.ValGroupId != null && appForm.ValGroupId != Guid.Empty)
                {
                    groupId = appForm.ValGroupId.GetValueOrDefault();
                }
                else
                {
                    groupId = Guid.NewGuid();
                }
                List<Fields> formField = _context.Fields.Where(a => a.FormId == model.FormId).ToList();

                var listOfFldValue = new List<FieldValues>();

                foreach (var item in model.FieldAndValue)
                {
                    string lowervalue = item.FieldName.ToLower().Trim();
                    string trim = item.FieldName.Replace(" ", "");

                    var formParameter = _context.Fields.Where(a => a.Label.Replace(" ", "") == trim && a.FormId == a.FormId).FirstOrDefault();

                    if (formParameter != null)
                    {
                        var fieldV = _context.FieldValues.Where(a => a.FieldId == formParameter.Id && a.GroupId == groupId).FirstOrDefault();
                        //get field id
                        var field = _context.Fields.Where(a => a.Label.Replace(" ", "") == trim).FirstOrDefault();

                        if (fieldV == null)
                        {

                            fieldV = new FieldValues()
                            {
                                GroupId = groupId,
                                FieldId = field.Id,
                                Value = item.Value
                            };
                            listOfFldValue.Add(fieldV);
                            _context.FieldValues.Add(fieldV);
                        }
                        else
                        {
                            fieldV.Value = item.Value;
                            listOfFldValue.Add(fieldV);
                        }

                        save = _context.SaveChanges();
                    }
                    else
                    {
                        result = "We could not find any form item with this FieldName:" + item.FieldName + ", please check again and resend";
                        return Json(new { success = false, message = result });

                    }

                }

                var userId = _context.Staff.Where(a => a.StaffEmail == model.InspectorEmail.Trim()).FirstOrDefault().StaffID;

                if (save > 0)
                {


                    #region Completing Process
                    appForm.Filled = model.Filled;
                    appForm.DateModified = DateTime.Now;
                    appForm.Reasons = model.Reasons;// reason;
                    appForm.Recommend = model.Recommend;// recommend;
                    appForm.ExtraReport1 = model.ExtraReport1;// ExtraReport1;
                    appForm.ExtraReport2 = model.ExtraReport2;// ExtraReport2;
                    appForm.ValGroupId = groupId;
                    //get the staff that filled the form
                    var stf = _context.Staff.Where(a => a.StaffEmail.ToLower() == model.InspectorEmail).FirstOrDefault();
                    if (stf != null)
                    {
                        appForm.StaffName = $"{stf.FirstName} {stf.LastName} ({stf.FirstName + " " + stf.LastName})";
                    }
                    if (_context.SaveChanges() > 0) { }
                    var appInspectionDetails = _context.MeetingSchedules.Where(a => a.Approved == true && a.StaffUserName == model.InspectorEmail && a.ApplicationId == model.ApplicationId).OrderByDescending(a => a.Date).FirstOrDefault();
                    var appInspectionDetails2 = _context.InspectionSchedules.Where(a => a.Accepted == true && a.StaffUserName == model.InspectorEmail && a.ApplicationId == model.ApplicationId).OrderByDescending(a => a.DateAdded).FirstOrDefault();


                    if (appInspectionDetails != null)
                    {
                        appInspectionDetails.Completed = true;
                        _context.SaveChanges();
                    }

                    #region Log history && send email notification to inspector
                    string comment = appForm.FormTitle + " inspection form filled & submitted via Smart Inspector application.";
                    _helpersController.SaveHistory(model.ApplicationId, stf.StaffID, stf.StaffEmail, GeneralClass.Move, comment);
                    _helpersController.LogMessages(comment + " by " + stf.StaffEmail, stf.StaffEmail);
                    string subject = "Inspection Form Submission For Application With Reference: " + app.reference;
                    string content = "You have successfully submitted " + appForm.FormTitle + " inspection form for application with the above reference number.";
                    var emailMsg = _helpersController.SaveMessage(model.ApplicationId, stf.StaffID, subject, content, stf.StaffElpsID.ToString(), "Staff");
                    var sendEmail = _helpersController.SendEmailMessage2Staff(stf.StaffEmail, stf.FirstName, emailMsg, null);

                    #endregion
                    result = "Inspection form saved successfully";
                    return Json(new { success = true, message = result });
                }
                #endregion



                else
                {
                    result = "An error occured while saving this inspection form.";
                    return Json(new { success = true, message = result });

                }
            }

            else
            {
                result = "Sorry, application form was not found.";
                return Json(new { success = false, message = result });

            }

        }



        ////[Authorize(Roles = "Staff, Admin, ITAdmin")]
        public IActionResult ViewApplication(string id, string option)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int loginID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionLogin));
            int Id = generalClass.DecryptIDs(id);
            int procid = generalClass.DecryptIDs(option);

            if (userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Kindly log in again.") });
            }
            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"].ToString();
                ViewBag.MsgType = TempData["msgType"].ToString();
                TempData.Clear();
            }

            if (TempData["Report"] != null)
                ViewBag.ExtraPayReport = TempData["Report"];


            if (Id > 0)
            {
                var app = _context.applications.Where(a => a.id == Id).FirstOrDefault();

                var appDetail = (from ap in _context.applications.AsEnumerable()
                                 join c in _context.companies.AsEnumerable() on ap.company_id equals c.id
                                 join fac in _context.Facilities.AsEnumerable() on ap.FacilityId equals fac.Id
                                 join cat in _context.Categories.AsEnumerable() on ap.category_id equals cat.id
                                 join phs in _context.Phases.AsEnumerable() on ap.PhaseId equals phs.id
                                 join his in _context.application_desk_histories on ap.id equals his.application_id into histor
                                 join sb in _context.SubmittedDocuments.AsEnumerable() on ap.id equals sb.AppID into subdoc
                                 where ap.DeleteStatus != true && ap.id == Id
                                 select new MyApps
                                 {
                                     Current_Permit = ap?.current_Permit,
                                     PaymentDescription = ap.PaymentDescription,
                                     Fee_Payable = (decimal)ap.fee_payable,
                                     appHistory = histor.OrderByDescending(x => x.id).FirstOrDefault(),
                                     appID = ap.id,
                                     Reference = ap.reference,
                                     category_id = cat.id,
                                     CategoryName = cat.name,
                                     PhaseName = phs.name,
                                     PhaseId = phs.id,
                                     ShortName = phs.ShortName,
                                     Status = ap.status,
                                     Date_Added = Convert.ToDateTime(ap.date_added),
                                     DateSubmitted = ap.CreatedAt == null ? Convert.ToDateTime(ap.date_added) : Convert.ToDateTime(ap.CreatedAt),
                                     Submitted = ap.submitted,
                                     CompanyDetails = c.name + " (" + c.Address + ") ",
                                     CompanyName = c.name,
                                     Company_Id = c.id,
                                     FacilityId = fac.Id,
                                     FacilityDetails = fac.Name,
                                     FacilityName = fac.Name,
                                     Year = ap.year,
                                     Type = ap.type,
                                     ApplicationDocs = subdoc.ToList()
                                 }).FirstOrDefault();


                if (app == null)
                {
                    TempData["msgType"] = "fail";
                    TempData["message"] = "Invalid application credentials";
                    return RedirectToAction("MyDesk");
                }

                if (app.status == GeneralClass.PaymentCompleted && app.current_desk > 0)
                {
                    app.status = GeneralClass.Processing;
                    _context.SaveChanges();
                }


                #region checkHQSupervisor&Inspector
                var HQ_Process = (from u in _context.application_desk_histories
                                  join st in _context.Staff on u.StaffID equals st.StaffID
                                  join rl in _context.UserRoles on st.RoleID equals rl.Role_id
                                  join lc in _context.Location on st.LocationID equals lc.LocationID
                                  where lc.LocationName == GeneralClass.HQ && u.application_id == app.id
                                  select new
                                  {
                                      st,
                                      u,
                                      rl,
                                      lc
                                  }
                                 ).ToList();
                if (userRole == GeneralClass.ADPDJ)
                {
                    var checkSupervisorProcess = HQ_Process.Where(x => x.rl.RoleName == GeneralClass.SUPERVISOR).FirstOrDefault();
                    ViewBag.HQPush = checkSupervisorProcess == null ? "Yes" : null;
                }
                if (userRole == GeneralClass.SUPERVISOR)
                {
                    var checkSupervisorProcess = HQ_Process.Where(x => x.rl.RoleName == GeneralClass.INSPECTOR).FirstOrDefault();
                    ViewBag.HQPush = checkSupervisorProcess == null ? "Yes" : null;
                }
                #endregion

                ViewBag.TranferCost = app.PhaseId == 6 ? '₦' + string.Format("{0:N}", app.TransferCost) : null;

                var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
                var mistdo = _context.MistdoStaff.Where(x => x.FacilityId == app.FacilityId && x.DeletedStatus == false).ToList();
                if (mistdo != null)
                {
                    ViewBag.MistdoStaff = mistdo;
                }
                #region Payment Description
                if (app.PaymentDescription == null)
                {
                    var apts = _context.ApplicationTanks.Where(a => a.ApplicationId == app.id).ToList();

                    double tnkV = 0;
                    int tnkCnt = 0;
                    if (apts.Count > 0)
                    {
                        tnkV = apts.Sum(a => a.Capacity);
                        tnkCnt = apts.Count;
                    }
                    else
                    {
                        var facTanks = _context.Tanks.Where(a => a.FacilityId == app.FacilityId).ToList();
                        tnkV = facTanks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                        tnkCnt = facTanks.Count;
                    }
                    var phase = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                    var feeDesc = _helpersController.CalculateAppFee(phase, app.current_Permit, tnkV, tnkCnt, false, 0, app.year);

                    _helpersController.LogMessages($"New app amount:: {feeDesc.Fee}");
                    if (feeDesc != null)
                    {
                        app.PaymentDescription = feeDesc.FeeDescription;
                        _context.SaveChanges();
                    }
                }
                #endregion

                List<OtherDocuments> otherDocuments = new List<OtherDocuments>();

                var submittedDoc = _context.SubmittedDocuments.Where(x => x.AppID == app.id && x.DeletedStatus != true);
                var allDoc = _context.ApplicationDocuments.Where(x => x.DeleteStatus != true);
                var othrDoc = allDoc.Where(x => !submittedDoc.Any(s => s.AppDocID == x.AppDocID)).ToList();

                foreach (var r in othrDoc)
                {
                    otherDocuments.Add(new OtherDocuments
                    {
                        LocalDocID = r.AppDocID,
                        DocName = r.DocName
                    });
                }

                var getDocuments = _context.ApplicationDocuments.Where(x => (x.PhaseId == app.PhaseId || x.PhaseId == 0) /*&&  x.docType == "Facility"*/ && x.DeleteStatus == false).ToList();

                List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                List<PresentDocuments> presentFacDocuments = new List<PresentDocuments>();
                List<PresentDocuments> presentCompDocuments = new List<PresentDocuments>();
                List<MissingDocument> missingDocuments = new List<MissingDocument>();
                List<BothDocuments> bothDocuments = new List<BothDocuments>();

                List<int> allDocuments = new List<int>();

                presentDocuments = (from u in _context.SubmittedDocuments
                                    join a in _context.ApplicationDocuments on u.AppDocID equals a.AppDocID
                                    where u.AppID == app.id && u.DeletedStatus != true
                                    select new PresentDocuments
                                    {
                                        Present = true,
                                        FileName = a.DocName,
                                        Source = u.DocSource,
                                        CompElpsDocID = (int)u.CompElpsDocID,
                                        DocTypeID = a.ElpsDocTypeID,
                                        LocalDocID = a.AppDocID,
                                        DocType = a.docType,
                                        TypeName = a.DocName

                                    }).ToList();

                if (getDocuments.Count() > 0)
                {
                    ViewData["FacilityElpsID"] = facility.Elps_Id;
                    ViewData["CompanyElpsID"] = company.elps_id;
                    #region Document Fetch Region
                    if (presentDocuments.Count() <= 0) //Old applications
                    {

                        List<LpgLicense.Models.Document> companyDoc = generalClass.getCompanyDocuments(company.elps_id.ToString());
                        List<LpgLicense.Models.FacilityDocument> facilityDoc = generalClass.getFacilityDocuments(facility.Elps_Id.ToString());
                        if (facilityDoc == null || companyDoc == null)
                        {
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                        }

                        //Facility documnents
                        var facDocuments = (from u in getDocuments
                                            join f in facilityDoc on u.ElpsDocTypeID equals f.Document_Type_Id
                                            where u.docType == "Facility"
                                            select new
                                            {
                                                facilityDoc = f,
                                                applicationDoc = u
                                            }).ToList();

                        facDocuments.ForEach(fDoc => {

                            presentFacDocuments.Add(new PresentDocuments
                            {
                                Present = true,
                                FileName = fDoc.facilityDoc.Name,
                                Source = fDoc.facilityDoc.Source,
                                CompElpsDocID = fDoc.facilityDoc.Id,
                                DocTypeID = fDoc.facilityDoc.Document_Type_Id,
                                LocalDocID = fDoc.applicationDoc.AppDocID,
                                DocType = fDoc.applicationDoc.docType,
                                TypeName = fDoc.applicationDoc.DocName

                            });
                        });

                        //Company documents
                        var compDocuments = (from u in getDocuments
                                             join c in companyDoc on u.ElpsDocTypeID.ToString() equals c.document_type_id.ToString()
                                             where u.docType == "Company"
                                             select new
                                             {
                                                 companyDoc = c,
                                                 applicationDoc = u
                                             }).ToList();

                        compDocuments.ForEach(cDoc => {

                            presentCompDocuments.Add(new PresentDocuments
                            {

                                Present = true,
                                FileName = cDoc.companyDoc.document_Name,
                                Source = cDoc.companyDoc.source,
                                CompElpsDocID = cDoc.companyDoc.id,
                                DocTypeID = int.Parse(cDoc.companyDoc.document_type_id),
                                LocalDocID = cDoc.applicationDoc.AppDocID,
                                DocType = cDoc.applicationDoc.docType,
                                TypeName = cDoc.applicationDoc.DocName

                            });
                        });

                        presentCompDocuments.ForEach(x =>
                        {
                            var checkAppDoc = (from u in presentDocuments where u.TypeName == x.TypeName && u.DocTypeID == x.DocTypeID select u).FirstOrDefault();
                            if (checkAppDoc == null)
                            {
                                presentDocuments.Add(x);
                                SubmittedDocuments submitDocs = new SubmittedDocuments()
                                {
                                    AppID = app.id,
                                    AppDocID = x.LocalDocID,
                                    CompElpsDocID = x.CompElpsDocID,
                                    CreatedAt = DateTime.Now,
                                    DeletedStatus = false,
                                    DocSource = x.Source
                                };
                                _context.SubmittedDocuments.Add(submitDocs);
                                _context.SaveChanges();
                            }
                            else
                            {
                                var getSubDoc = (from u in _context.SubmittedDocuments where u.AppID == app.id && u.AppDocID == checkAppDoc.LocalDocID select u).FirstOrDefault();

                                getSubDoc.CompElpsDocID = x.CompElpsDocID;
                                getSubDoc.DocSource = x.Source;
                                getSubDoc.UpdatedAt = DateTime.Now;
                                _context.SaveChanges();
                            }

                        });

                        presentFacDocuments.ForEach(x =>
                        {
                            var checkAppDoc = (from u in presentDocuments where u.TypeName == x.TypeName && u.DocTypeID == x.DocTypeID select u).FirstOrDefault();
                            if (checkAppDoc == null)
                            {
                                presentDocuments.Add(x);
                                SubmittedDocuments submitDocs = new SubmittedDocuments()
                                {
                                    AppID = app.id,
                                    AppDocID = x.LocalDocID,
                                    CompElpsDocID = x.CompElpsDocID,
                                    CreatedAt = DateTime.Now,
                                    DeletedStatus = false,
                                    DocSource = x.Source
                                };
                                _context.SubmittedDocuments.Add(submitDocs);
                                _context.SaveChanges();
                            }
                            else
                            {
                                var getSubDoc = (from u in _context.SubmittedDocuments where u.AppID == app.id && u.AppDocID == checkAppDoc.LocalDocID select u).FirstOrDefault();

                                getSubDoc.CompElpsDocID = x.CompElpsDocID;
                                getSubDoc.DocSource = x.Source;
                                getSubDoc.UpdatedAt = DateTime.Now;
                                _context.SaveChanges();
                            }

                        });
                    }
                    #endregion
                    var result = getDocuments.AsEnumerable().Where(x => !presentDocuments.AsEnumerable().Any(x2 => x2.LocalDocID == x.AppDocID));

                    foreach (var r in result.ToList())
                    {
                        missingDocuments.Add(new MissingDocument
                        {
                            Present = false,
                            DocTypeID = r.ElpsDocTypeID,
                            LocalDocID = r.AppDocID,
                            DocType = r.docType,
                            TypeName = r.DocName
                        });
                    }

                    bothDocuments.Add(new BothDocuments
                    {
                        missingDocuments = missingDocuments,
                        presentDocuments = presentDocuments.Distinct().ToList(),
                    });
                    _helpersController.LogMessages("Loading facility information and document for report upload : " + app.reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                    _helpersController.LogMessages("Displaying/Viewing more application details.  Reference : " + app.reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                    ViewBag.OtherDocument = otherDocuments;
                    ViewBag.MissingDocument = missingDocuments;
                    ViewBag.PresentDocument = presentDocuments;

                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong while trying to get facility documents for this application. Kindly contact support.") });
                }

                ViewBag.Location = (from s in _context.Staff
                                    join l in _context.Location on s.LocationID equals l.LocationID
                                    where s.StaffID == userID
                                    select l).FirstOrDefault()?.LocationName;

                var appReport = from r in _context.Reports
                                join s in _context.Staff on r.StaffId equals s.StaffID
                                orderby r.ReportId descending
                                where r.AppId == Id && r.DeletedStatus != true
                                select new AppReport
                                {
                                    ReportID = r.ReportId,
                                    Staff = s.StaffEmail,
                                    StaffID = (int)r.StaffId,
                                    Comment = r.Comment,
                                    Title = r.Title,
                                    CreatedAt = r.CreatedAt.ToString(),
                                    UpdatedAt = r.UpdatedAt == null ? "" : r.UpdatedAt.ToString()
                                };

                ViewBag.StaffAppReport = appReport;

                //check if app is an approval
                var isApproval = appDetail.ShortName == "SI" || appDetail.ShortName == "ATC" || appDetail.ShortName == "DM" ? "Yes" : "No";
                ViewBag.IsApproval = isApproval;
                //get current desk
                var currentDesk = from a in _context.applications
                                  join m in _context.MyDesk on a.id equals m.AppId
                                  join s in _context.Staff on m.StaffID equals s.StaffID
                                  join f in _context.FieldOffices on s.FieldOfficeID equals f.FieldOffice_id
                                  where a.id == app.id && m.HasWork != true
                                  select new CurrentDesk
                                  {
                                      Staff = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                                      StaffFieldOffice = f.OfficeName
                                  };

                appDetail.Current_Desk = currentDesk.FirstOrDefault();

                var appProc = _context.MyDesk.Where(a => a.AppId == Id && a.HasWork != true).FirstOrDefault();
                ViewBag.Holding = "";
                bool meProcessing = false;
                if (appProc != null)
                {
                    if (appProc.StaffID == userID)
                    {
                        meProcessing = true;
                        ViewBag.Holding = appProc.Holding;
                    }
                    if (procid == 0)
                    {
                        procid = appProc.ProcessID;
                    }

                    ViewBag.AppProcId = appProc.DeskID;
                    ViewBag.DeskID = appProc.DeskID;

                }


                var history = _context.application_desk_histories.Where(a => a.application_id == app.id).OrderByDescending(a => a.date);
                ViewBag.History = history.Take(2);
                ViewBag.historyID = history.FirstOrDefault().id;

                var appForms = _context.ApplicationForms.Where(a => a.ApplicationId == app.id).ToList();
                if (appForms.Count() > 0)
                {
                    var af = appForms.FirstOrDefault(a => a.Filled == true);
                    if (af != null)
                    {
                        ViewBag.appForm = af;
                        ViewBag.appFormTitle = appDetail.ShortName;
                    }
                }


                #region Show Field Report on the Application
                // 1: Get List of Assigned Inspectors
                // 2: Get All Report on the Joint Activity
                var reports = _context.JointAccountReports.Where(a => a.ApplicationId == app.id).ToList();
                var inspectors = (from j in _context.JointAccountStaffs.AsEnumerable()
                                  join st in _context.Staff on j.Staff equals st.StaffEmail
                                  join fd in _context.FieldOffices on st.FieldOfficeID equals fd.FieldOffice_id
                                  where j.ApplicationId == app.id
                                  select new JointStaffModel
                                  {
                                      FirstName = st.FirstName,
                                      LastName = st.LastName,
                                      Email = st.StaffEmail,
                                      FieldOffice = fd.OfficeName,
                                      SignedOff = j.SignedOff
                                  });

                var reportModel = new JointReportModel()
                {
                    JointStaff = inspectors.ToList(),
                    Reports = appReport.ToList()
                };

                ViewBag.Report = reportModel;

                foreach (var insp in inspectors.Where(n => n.Email == userEmail && n.OperationsCompleted != true))
                {
                    ViewBag.SignOff = false;
                }
                #endregion

                //Check if Application has schedule on it.
                var Sche = _context.MeetingSchedules.Where(a => a.ApplicationId == app.id).ToList();
                var appSche = Sche.OrderBy(a => a.Date).LastOrDefault();
                ViewBag.MyAppScheduleExpiry = false;
                if (appSche != null)
                {
                    if (appSche.ScheduleExpired == null)
                    {
                        //check if the date is still within
                        if (appSche.ApprovedDate != null && appSche.ApprovedDate.GetValueOrDefault().Date.AddDays(3) < DateTime.Now)
                        {
                            //expired already
                            //ViewApplication

                            appSche.Message = string.Format("{0}{1}{1} {2}", appSche.Message, Environment.NewLine, ViewBag.expired);

                            appSche.ScheduleExpired = true;
                            _context.SaveChanges();

                        }
                    }
                    ViewBag.Schedules = Sche;
                    #region
                    var message = "";
                    var typ = "";
                    bool wait = true;
                    if (appSche.ScheduleExpired == null)
                    {

                        if (appSche.Approved == null && appSche.ScheduleExpired == null) // && appSche.Accepted == null)
                        {
                            typ = "warning";
                            message = "Application schedule is yet to be approved.";
                        }
                        else if (appSche.Approved != null && appSche.Approved != true)
                        {
                            typ = "danger";
                            wait = false;
                            message = "Application schedule was NOT approved. Please see application history for more details.";
                        }
                        else if (appSche.ScheduleExpired != null && appSche.ScheduleExpired == true && appSche?.Accepted == false)
                        {
                            typ = "danger";
                            wait = false;
                            message = "Schedule has expired and the marketer failed to respond to the schedule within the stipulated days.";
                        }
                        else if (appSche.Accepted == null && appSche.ScheduleExpired != true)
                        {
                            typ = "warning";
                            message = "The applicant/marketer is yet to reponded to the schedule.";
                        }
                        else if (!appSche.Accepted.GetValueOrDefault())
                        {
                            typ = "danger";
                            wait = false;
                            message = "Applicant/Marketer has declined the schedule date for Meeting/Inspection";
                        }
                        else
                        {
                            typ = "success";
                            message = "Application schedule has been acepted by the marketer. All parties have been notified about the status of the application.";
                            wait = false;
                            // lets populates the 
                            var mia = _context.InspectionMeetingAttendees.Where(a => a.MeetingScheduleId == appSche.Id).ToList();
                            ViewBag.InspectMeetAtt = mia;
                            ViewBag.ScheduleApprove = "Yes";
                        }

                        ViewBag.scheMsg = message;
                        ViewBag.scheTyp = typ;
                        ViewBag.Wait = wait;
                    }
                    #endregion
                    ViewBag.CheckScheduledDate = "Yes";

                    if (appSche.StaffUserName.ToLower() == userEmail.ToLower())
                    {
                        ViewBag.MyAppScheduleExpiry = true;
                    }
                }


                var appPhase = _context.Phases.Where(p => p.id == app.PhaseId).FirstOrDefault();

                if (appPhase.ShortName == "DM" || appPhase.ShortName == "UWA" || appPhase.ShortName == "RC")//UWA  :: Update without Modification
                {
                    //check facilityTankModification Table with the ApplicationID
                    var facMod = _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault();
                    if (facMod != null)
                    {
                        string prv = "";
                        if (!string.IsNullOrEmpty(facMod.PrevProduct) && facMod.Type.ToLower().Contains("conver"))
                        {
                            prv = $"Converted from {facMod.PrevProduct}";
                        }

                        //ViewBag.facModification = $"Modification type: {facMod.Type} {prv}";
                        ViewBag.facModification = $"Modification type: {facMod.Type}";
                    }

                }
                ViewBag.meProcessing = meProcessing;
                var exts = _context.ApplicationExtraPayments.Where(C => C.ApplicationId == app.id).ToList();
                bool paid = false;
                foreach (var item in exts)
                {
                    if (item.Status == GeneralClass.PaymentPending && item.RRR != null)
                    {

                        var resp = CheckRRRPayment(item.RRR);
                        if (resp != null)
                        {
                            if ((resp.GetValue("message").ToString().ToLower() == "approved" && resp.GetValue("status").ToString() == "01") || (resp.GetValue("message").ToString().ToLower() == "successful" && resp.GetValue("status").ToString() == "00"))
                            {
                                paid = true;
                                item.DatePaid = resp.GetValue("transactiontime").ToString();
                                item.Status = GeneralClass.PaymentCompleted;
                            }
                        }
                    }
                }
                if (paid)
                {
                    _context.SaveChanges();

                }
                ViewBag.ExtraPay = exts;
                #region Tank Section
                var tnks = new List<Tanks>();
                var apTanks = (from t in _context.ApplicationTanks.AsEnumerable()
                               join tk in _context.Tanks.AsEnumerable() on t.TankId equals tk.Id
                               join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                               where t.ApplicationId == app.id
                               select new TankModel
                               {
                                   Name = t.TankName,
                                   MaxCapacity = t.Capacity.ToString(),
                                   ProductName = p.Name,
                                   Height = tk.Height,
                                   Decommissioned = tk.Decommissioned,
                                   Diameter = tk.Diameter,
                                   CreateAt = t.Date,
                                   ModifyType = tk.ModifyType
                               }).ToList();
                ViewBag.AppTanks = apTanks;

                var tks = (from t in _context.Tanks.AsEnumerable()
                           join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                           where t.FacilityId == app.FacilityId && t.DeletedStatus != true

                           //where t.FacilityId == app.FacilityId &&( t.Status==null || (t.Status.Contains("Approved")))
                           select new TankModel
                           {
                               Name = t.Name,
                               MaxCapacity = t.MaxCapacity.ToString(),
                               ProductName = p.Name,
                               Height = t.Height,
                               Decommissioned = t.Decommissioned,
                               Diameter = t.Diameter,
                               CreateAt = t.CreatedAt,
                               ModifyType = t.ModifyType
                           });
                ViewBag.Tanks = tks.ToList();


                #endregion


                ViewBag.ADCanPushToSupervisor = false;
                ViewBag.SPCanPushToInspector = false;
                var aph = _context.application_desk_histories.Where(a => a.UserName == userEmail && a.application_id == app.id).ToList();
                var apProc = _context.WorkProccess.Where(a => a.PhaseID == app.PhaseId && !a.DeleteStatus).ToList();

                #region check if AD/TeamLead/Supervisor can push to lower staff
                if ((userRole.Contains("AD")) && app.AppProcessed == true)
                {
                    //we need to check s/he has worked on this application(Pushed) it before now
                    if (aph != null || aph.Count > 0)
                    {
                        ViewBag.ADCanPushToSupervisor = true;
                    }


                }
                else if (userRole.Contains("Supervisor") && app.AppProcessed == true)
                {
                    //we need to check s/he has worked on this application(Pushed) it before now
                    if (aph.Count() >= 0)
                    {
                        ViewBag.SPCanPushToInspector = true;
                    }

                }
                else if (userRole.Contains("TeamLead") && app.AppProcessed == true)
                {
                    if (aph.Count() >= 0)
                    {
                        if (apProc.Count > 9) //This means current app process is for field office flow
                        {
                            ViewBag.SPCanPushToInspector = true;
                        }
                    }
                    var js = _context.JointAccounts.Where(a => a.ApplicationId == app.id && a.OperationsCompleted != true).FirstOrDefault();
                    if (js != null)
                    {
                        ViewBag.SPCanPushToInspector = true;
                    }
                }
                #endregion


                var products = _context.Products.ToList();
                ViewBag.Products = products;


                //get application facility state Id
                int fsid = 0;

                //get the Facility
                var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                var facState = address == null ? null : _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                fsid = facState != null ? facState.State_id : 0;

                List<MeetingVenue> venue = _helpersController.GetOfflineVenue(fsid, facState.StateName);
                ViewBag.Venue = new SelectList(venue, "Id", "Title");
                #region get Inspectors under Opscon
                if (userRole == GeneralClass.TEAMLEAD || userRole == GeneralClass.SUPERVISOR)
                {
                    var staff = _context.Staff.Where(x => x.ActiveStatus != false && x.DeleteStatus != true).ToList();
                    var currentUserFD = staff.Where(x => x.StaffID == userID).FirstOrDefault();

                    var getSelectedOffice =
                                        (from u in _context.Staff.AsEnumerable()
                                         join r in _context.UserRoles on u.RoleID equals r.Role_id
                                         join fd in _context.FieldOffices on u.FieldOfficeID equals fd.FieldOffice_id
                                         join zfd in _context.ZonalFieldOffice on fd.FieldOffice_id equals zfd.FieldOffice_id
                                         join z in _context.ZonalOffice on zfd.Zone_id equals z.Zone_id
                                         where u.FieldOfficeID == currentUserFD.FieldOfficeID && u.DeleteStatus != true && zfd.DeleteStatus != true && z.DeleteStatus != true
                                         && r.RoleName == GeneralClass.INSPECTOR
                                         select new Staff_UserBranchModel
                                         {
                                             StaffId = u.StaffID,
                                             StaffFullName = u.FirstName + " " + u.LastName,
                                             StaffEmail = u.StaffEmail,
                                             RoleName = r.RoleName,
                                             ZoneId = z.Zone_id,
                                             ZoneFieldOfficeID = zfd.FieldOffice_id,
                                             ZoneName = z.ZoneName,
                                             FieldOffice = fd.OfficeName,
                                             FieldId = fd.FieldOffice_id,
                                         });


                    Staff_UserBranchModel getStaff = new Staff_UserBranchModel();
                    List<Staff_UserBranchModel> pushStaff = new List<Staff_UserBranchModel>();
                    if (getSelectedOffice != null)
                    {

                        // Get all Inspectors in my Zone for possible assignment
                        #region Selecting Inspector from My Branch

                        foreach (var usr in getSelectedOffice)
                        {
                            pushStaff.Add(usr);

                        }
                    }
                    #endregion

                    ViewBag.Inspectors = pushStaff;
                }
                #endregion
                #region get Supervisor under AD
                if (userRole == GeneralClass.ADPDJ)
                {
                    var staff = _context.Staff.Where(x => x.ActiveStatus != false && x.DeleteStatus != true).ToList();
                    var currentUserFD = staff.Where(x => x.StaffID == userID).FirstOrDefault();

                    var getSelectedOffice =
                                        (from u in _context.Staff.AsEnumerable()
                                         join r in _context.UserRoles on u.RoleID equals r.Role_id
                                         join fd in _context.FieldOffices on u.FieldOfficeID equals fd.FieldOffice_id
                                         join zfd in _context.ZonalFieldOffice on fd.FieldOffice_id equals zfd.FieldOffice_id
                                         join z in _context.ZonalOffice on zfd.Zone_id equals z.Zone_id
                                         where u.FieldOfficeID == currentUserFD.FieldOfficeID && u.DeleteStatus != true && zfd.DeleteStatus != true && z.DeleteStatus != true
                                         && r.RoleName == GeneralClass.SUPERVISOR
                                         select new Staff_UserBranchModel
                                         {
                                             StaffFullName = u.FirstName + " " + u.LastName,
                                             StaffEmail = u.StaffEmail,
                                             RoleName = r.RoleName,
                                             ZoneId = z.Zone_id,
                                             ZoneFieldOfficeID = zfd.FieldOffice_id,
                                             ZoneName = z.ZoneName,
                                             FieldOffice = fd.OfficeName,
                                             FieldId = fd.FieldOffice_id,
                                         });


                    Staff_UserBranchModel getStaff = new Staff_UserBranchModel();
                    List<Staff_UserBranchModel> pushStaff = new List<Staff_UserBranchModel>();
                    if (getSelectedOffice != null)
                    {

                        // Get all Inspectors in my Zone for possible assignment
                        #region Selecting Inspector from My Branch

                        foreach (var usr in getSelectedOffice)
                        {

                            getStaff.StaffFullName = usr.StaffFullName;
                            getStaff.StaffEmail = usr.StaffEmail;
                            getStaff.FieldOffice = usr.FieldOffice + "( " + usr.ZoneName + " )";
                            pushStaff.Add(getStaff);

                        }
                    }
                    #endregion

                    ViewBag.Supervisors = pushStaff;
                }
                #endregion
                appDetail.bothDocuments = bothDocuments;
                if (meProcessing && (userRole.Contains("Supervisor") || userRole.Contains("Inspector")))
                {
                    return View("ProcessApplication", appDetail);
                }

                return View(appDetail);
            }
            else
            {
                TempData["msgType"] = "fail";
                TempData["message"] = "Invalid application provided";
                return RedirectToAction("MyDesk");
            }
        }



        ////[Authorize(Roles = "Staff, Admin")]
        public IActionResult History(string id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int AppId = generalClass.DecryptIDs(id);
            if (userRole == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });
            }
            if (AppId != 0)
            {
                var current = _context.MyDesk.Where(a => a.AppId == AppId && a.HasWork != true).ToList();
                //var application = _context.applications.Where(a => a.id == AppId).FirstOrDefault();

                var deskHist = (from ap in _context.applications
                                join a in _context.application_desk_histories on ap.id equals a.application_id
                                join comp in _context.companies on ap.company_id equals comp.id
                                join c in _context.Categories on ap.category_id equals c.id
                                join p in _context.Phases on ap.PhaseId equals p.id
                                where a.application_id == AppId
                                select new ApplicationDeskHistoryModel
                                {
                                    id = a.id,
                                    UserName = a.UserName,
                                    CategoryName = ap.category_id != 1 ? c.name : c.name + " " + (p.name),
                                    CompanyName = comp.name,
                                    ApplicationDate = ap.CreatedAt != null ? (DateTime)ap.CreatedAt : ap.date_added,
                                    DateSubmitted = ap.CreatedAt != null ? (DateTime)ap.CreatedAt : ap.date_added,
                                    Comment = a.comment,
                                    Status = a.status,
                                    Date = a.date,
                                    application_id = ap.id,

                                }).OrderByDescending(a => a.Date).ToList();

                if (current != null && current.Count() != 0)
                {
                    var st = (from stf in _context.Staff
                              join r in _context.UserRoles on stf.RoleID equals r.Role_id
                              join f in _context.FieldOffices on stf.FieldOfficeID equals f.FieldOffice_id
                              where stf.DeleteStatus != true && stf.StaffID == current.FirstOrDefault().StaffID
                              select new Staff_UserBranchModel
                              {

                                  RoleName = r.RoleName,
                                  FieldOffice = f.OfficeName,
                                  StaffEmail = stf.StaffEmail
                              }).FirstOrDefault();
                    if (st != null)
                    {
                        ViewBag.CurrentStaffDesk = st.StaffEmail;
                        ViewBag.FieldOffice = st.FieldOffice;
                        ViewBag.RoleName = st.RoleName;
                    }
                }
                else if (current == null)
                {
                    ViewBag.Message = "Application is not on anybody's desk currently";
                }
                //else
                //{
                //    ViewBag.Message = "You are not allowed to view this page.";
                //}
                ViewBag.Header = "Application History";
                return View(deskHist);
            }
            else
            {
                ViewBag.Message = "No application is selected to view history.";
            }
            return View();

        }
        public JObject CheckRRRPayment(string rrr)
        {
            try
            {

                var res = _restService.Response("/Payment/checkifpaid?id=r" + rrr, null, "GET", null);

                var resp = JsonConvert.DeserializeObject<JObject>(res.Content.ToString());

                if (resp != null && res.Content != null)
                {
                    if ((resp.GetValue("message").ToString().ToLower() == "approved" && resp.GetValue("status").ToString() == "01") || (resp.GetValue("message").ToString().ToLower() == "successful" && resp.GetValue("status").ToString() == "00"))
                    {
                        return resp;
                    }
                }
                return resp;

            }

            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                return null;
            }
        }

        /*
       * Viewing an application report created by a staff
       * 
       * id => encrypted report ID
       * 
       */
        //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult ViewReport(string id)
        {
            var report_id = generalClass.DecryptIDs(id.Trim());

            if (report_id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, No link was found for this application history.") });
            }
            else
            {
                var Request = from re in _context.Reports.AsEnumerable()
                              join ap in _context.applications.AsEnumerable() on re.AppId equals ap.id
                              join sf in _context.Staff.AsEnumerable() on re.StaffId equals sf.StaffID
                              join ro in _context.UserRoles.AsEnumerable() on sf.RoleID equals ro.Role_id
                              join c in _context.companies.AsEnumerable() on ap.company_id equals c.id
                              join f in _context.Facilities.AsEnumerable() on ap.FacilityId equals f.Id
                              join ad in _context.addresses.AsEnumerable() on f.AddressId equals ad.id
                              join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                              join cat in _context.Categories.AsEnumerable() on ap.category_id equals cat.id
                              join phs in _context.Phases.AsEnumerable() on ap.PhaseId equals phs.id
                              where ((re.ReportId == report_id))
                              select new ReportViewModel
                              {
                                  RefNo = ap.reference,
                                  CompanyName = c.name,
                                  CompanyAddress = c.Address,
                                  CompanyEmail = c.CompanyEmail,
                                  Category = phs.name,
                                  Products = _helpersController.GetFacilityProducts(f.Id),
                                  Facility = f.Name + " (" + ad.address_1 + "," + ad.city + "," + f.LGA + "," + sd.StateName + ")",
                                  Status = ap.status,
                                  Staff = sf.LastName + " " + sf.FirstName + " (" + sf.StaffEmail + " -- " + ro.RoleName + ")",
                                  ReportDate = re.CreatedAt,
                                  UpdatedAt = re.UpdatedAt,
                                  Subject = re.Title,
                                  Comment = re.Comment,
                              };

                if (Request.Count() > 0)
                {
                    ViewData["AppRef"] = Request.FirstOrDefault()?.RefNo;

                    _helpersController.LogMessages("Displaying application report for Application reference : " + Request.FirstOrDefault().RefNo, _helpersController.getSessionEmail());

                    return View(Request.ToList());
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application report not found or not in correct format. Kindly contact support.") });
                }
            }
        }


        #region Application Core Processes
        /// <summary>
        /// Used to Push Applications to the Next Processing personnel.
        /// </summary>
        /// <param name="selectedApps">String Array of Ids of the Applications to Push (No Specified Staff).</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult PushApplication(int supervisor, string[] supervisors, string[] inspectors, params string[] selectedApps)
        {
            string msg = string.Empty;
            string msgType = string.Empty;
            string actioneer = string.Empty;

            var Relievestaff = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRelieveStaff));

            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int loginID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionLogin));



            if (userName == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Application reference not passed. Please try again later") });
            }


            if (Relievestaff != null && Relievestaff != "Error")
            {
                actioneer = Relievestaff;
            }
            else
            {
                actioneer = userEmail;
            }
            foreach (var app in selectedApps)
            {
                int appId = Convert.ToInt16(app);

                //Update new Process
                var myDesk = _context.MyDesk.Where(a => a.AppId == appId && a.StaffID == userID && a.HasWork != true).FirstOrDefault();

                var application = _context.applications.Where(a => a.id == appId).FirstOrDefault();
                var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                var companyName = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();

                var result = "";
                //Check who pushed, then assign to the reciever
                if (userRole == GeneralClass.ADPDJ)
                {
                    foreach (var i in supervisors)
                    {
                        result = _helpersController.Assign(appId, myDesk.DeskID, myDesk.ProcessID, userEmail, userRole + " pushed application for processing.", i.ToString(), 0);
                    }
                }
                else
                {
                    foreach (var i in inspectors)
                    {
                        result = _helpersController.Assign(appId, myDesk.DeskID, myDesk.ProcessID, userEmail, userRole + " pushed application for processing.", i.ToString(), 0);
                    }
                }
                if (result.ToLower() == "not ok")
                {

                    myDesk.HasWork = false;
                    myDesk.HasPushed = false;
                    myDesk.UpdatedAt = null;
                    _context.SaveChanges();
                }
                else if (result.ToLower() == GeneralClass.ProcessCompleted)
                {
                    _helpersController.CreatePermit(application, facility);

                    myDesk.HasWork = true;
                    myDesk.HasPushed = true;
                    myDesk.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();


                    // Log in History

                    string comment = "Permit generated by " + userEmail + " for application";
                    _helpersController.SaveHistory(appId, userID, userEmail, GeneralClass.ProcessCompleted, comment);
                }
                else if (result.ToLower() == "ok") application.AllowPush = false;
                {
                    myDesk.HasWork = true;
                    myDesk.HasPushed = true;
                    myDesk.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                    // _helpersController.LogMessages("Yes, Next Assign returned OK");
                    #region Check if to Nofity ZN/FD
                    _context.SaveChanges();

                    var processor = _context.Staff.Where(a => a.StaffID == myDesk.StaffID).FirstOrDefault();

                    var user = processor.StaffEmail;
                    var pushedToRole = _context.UserRoles.Where(x => x.Role_id == processor.RoleID).FirstOrDefault().RoleName;

                    if ((userRole == GeneralClass.ADPDJ && pushedToRole.Contains("Supervisor")) || (userRole.Contains("TeamLead") && pushedToRole.Contains("Inspector")))
                    {

                        var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                        var requiredStaff = new List<Staff>();
                        // _helpersController.LogMessages("Yes, User is in AD Role, and roles contains Supervisor");
                        if (application.type.ToLower() != "renew" && userRole == GeneralClass.ADPDJ)
                        {

                            requiredStaff = (from m in _context.Staff
                                             join a in _context.UserRoles on m.RoleID equals a.Role_id
                                             where m.ActiveStatus == true && m.DeleteStatus != true &&
                                             a.RoleName.ToLower() == "opscon" || a.RoleName.ToLower() == "teamlead" || a.RoleName.ToLower() == "adops"
                                             select m).ToList();

                        }
                        else if (application.type.ToLower() == "renew")
                        {
                            //JointAccount
                            var js = _context.JointAccounts.Where(a => a.ApplicationId == application.id).FirstOrDefault();
                            if (js == null)
                            {

                                requiredStaff = (from m in _context.Staff
                                                 join a in _context.UserRoles on m.RoleID equals a.Role_id
                                                 where m.ActiveStatus == true && m.DeleteStatus != true && a.RoleName.ToLower() == "opscon" || a.RoleName.ToLower() == "adops"
                                                 select m).ToList();
                            }
                        }

                        if (requiredStaff.Count() > 0)
                        {
                            _helpersController.LogMessages($"Yes, {requiredStaff.Count} Required staff was found");
                            var joint = _helpersController.NotifyZNFD(appId, address.StateId, requiredStaff, userName);
                        }



                        if (application.AppProcessed != true && (userRole == GeneralClass.SUPERVISOR || userRole == GeneralClass.INSPECTOR))
                        {
                            application.AppProcessed = true;
                            _context.SaveChanges();

                        }

                    }
                    else
                    {
                    }

                    #endregion
                }


            }


            msg = "Application has been pushed to the next processing officer for appropriate action";
            msgType = "pass";


            TempData["message"] = msg;
            TempData["msgType"] = msgType;
            return RedirectToAction("MyDesk");
        }
        //[Authorize(Roles = "Admin")]
        public IActionResult Regeneratep(int id)
        {
            try
            {

                var vApp = _context.applications.Where(a => a.id == id).FirstOrDefault();
                var fac = _context.Facilities.Where(a => a.Id == vApp.FacilityId).FirstOrDefault();
                if (vApp != null && vApp.status == GeneralClass.Approved)
                {
                    var p = _context.permits.Where(a => a.application_id == vApp.id).FirstOrDefault();
                    if (p == null)
                    {
                        if (fac.CategoryCode == null)
                        {
                            //generate facility identification code
                            var getCode = _helpersController.GetFacilityIdentificationCode(fac.Id);
                        }
                        _helpersController.CreatePermit(vApp, fac, "re");
                        return Content("Permit has been re-generated successfully.");

                    }
                    else
                    {
                        int elpsPermitId = 0;
                        var comp = _context.companies.Where(a => a.id == vApp.company_id).FirstOrDefault();

                        if (!_helpersController.PostPermit(
                            new PermitAPIModel
                            {
                                Id= p.id,
                                CategoryName = _context.Categories.Where(ct => ct.id == vApp.category_id).FirstOrDefault().name,
                                Company_Id = Convert.ToInt32(comp.elps_id),
                                Date_Expire = p.date_expire,
                                Date_Issued = p.date_issued,
                                Permit_No = p.permit_no,
                                OrderId = vApp.reference,
                                Is_Renewed = p.is_renewed,

                            }, elpsPermitId))
                        {
                            throw new ArgumentException("Error Pushing Permit to ELPS!");
                        }
                        else
                        {
                            return Content("Permit has been re-generated successfully.");
                        }

                        p.elps_id = elpsPermitId;
                        _context.permits.Add(p);
                        _context.SaveChanges();
                        return Content("2");
                    }
                }
                return Content("This application has not been approved");

            }
            catch (Exception)
            {

                throw;
            }
        }

        //[Authorize(Roles = "Admin")]
        public IActionResult Regenerate4Record(int id)
        {
            try
            {
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                if (userEmail == "Error" || userID == 0)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Application reference not passed. Please try again later") });
                }

                var vApp = _context.applications.Where(a => a.id == id).FirstOrDefault();
                var fac = _context.Facilities.Where(a => a.Id == vApp.FacilityId).FirstOrDefault();
                if (vApp != null && vApp.status == GeneralClass.PaymentPending)
                {
                    var p = _context.permits.Where(a => a.application_id == vApp.id).FirstOrDefault();
                    if (p == null)
                    {
                        var pn = _helpersController.CreatePermit(vApp, fac, "ge");
                        var ppr = _context.application_Processings.Where(a => a.ApplicationId == vApp.id && a.Assigned == true && a.Processed != true).FirstOrDefault();
                        if (ppr != null)
                        {
                            ppr.Processed = true;
                            ppr.DateProcessed = DateTime.UtcNow.AddHours(1);
                            _context.SaveChanges();

                        }
                        return Content(pn);

                    }
                    else
                    {
                        return Content("2");
                    }
                }
                return Content("1");

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        public IActionResult Approve(string id, string AddPresentation, string w, int? appProcId)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userEmail == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again") });
            }

            int deskID = Convert.ToInt32(id);
            var appProc = _context.MyDesk.Where(a => a.DeskID == deskID).FirstOrDefault();
            var app = _context.applications.Where(a => a.id == appProc.AppId).FirstOrDefault();

            if (app != null)

            {
                if (userRole.Contains("Supervisor") || userRole.Contains("Inspector"))
                {

                    var appForms = _context.ApplicationForms.Where(a => a.ApplicationId == app.id).ToList();
                    if (appForms.Count() > 0)
                    {
                        var af = appForms.FirstOrDefault(a => a.Filled == false);
                        if (af != null)
                        {

                            ViewBag.AppForms = "PLEASE NOTE THAT THERE IS AN APPLICATION FORM THAT HAS NOT BEEN FILLED FOR THIS APPLICATION";

                        }
                    }
                    else
                    {

                        ViewBag.AppForms = "PLEASE NOTE THAT, NO APPLICATION FORM HAS BEEN ATTATCHED OR FILLED FOR THIS APPLICATION";

                    }
                }
            }

            ViewBag.AppProcId = id;
            ViewBag.AddPresentation = AddPresentation;
            var appDetail = (from ap in _context.applications.AsEnumerable()
                             join c in _context.companies.AsEnumerable() on ap.company_id equals c.id
                             join fac in _context.Facilities.AsEnumerable() on ap.FacilityId equals fac.Id
                             join cat in _context.Categories.AsEnumerable() on ap.category_id equals cat.id
                             join phs in _context.Phases.AsEnumerable() on ap.PhaseId equals phs.id
                             where ap.DeleteStatus != true && ap.id == app.id
                             select new MyApps
                             {
                                 appID = ap.id,
                                 Reference = ap.reference,
                                 CategoryName = cat.name,
                                 PhaseName = phs.name,
                                 Date_Added = Convert.ToDateTime(ap.date_added),
                                 CompanyName = c.name,
                                 FacilityName = fac.Name,
                                 Year = ap.year,
                                 Type = ap.type,
                                 Status = ap.status
                             }).FirstOrDefault();

            return View(appDetail);
        }

        [HttpPost]
        public IActionResult Approve(string comment, string DeskID, string[] selectedApps, string frommydesk = null)
        {
            string msg = string.Empty;
            string msgType = string.Empty;
            int deskID = Convert.ToInt32(DeskID);
            string response = "";
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            try
            {

                foreach (var b in selectedApps)
                {
                    int appId = Convert.ToInt16(b);


                    //get Processes
                    var mypos = _context.MyDesk.Where(a => a.DeskID == deskID && a.AppId == appId && a.StaffID == userID).FirstOrDefault();
                    if (userRole == GeneralClass.OOD)
                    {
                        mypos = _context.MyDesk.Where(a => a.AppId == appId && a.StaffID == userID).FirstOrDefault();
                    }

                    var usrBrnchId = 0; int processId = 0;
                    #region check if app is joint operation or not
                    if (mypos == null)
                    {

                        //could be a joint application assignment
                        var jointAppOperation = _context.JointAccounts.Where(a => a.ApplicationId == appId && a.Opscon.ToLower() == userEmail.ToLower() && a.OperationsCompleted != true).FirstOrDefault();

                        if (jointAppOperation != null && userRole == GeneralClass.TEAMLEAD)
                        {
                            processId = 0;
                        }
                    }
                    else
                    {
                        processId = mypos.ProcessID;
                    }
                    #endregion
                    //Assign to next processing staff
                    var result = _helpersController.Assign(appId, deskID, processId, userEmail, comment, null, usrBrnchId);

                    if (result == null)
                    {
                        return Json("An error occured while trying to update application status on ELPS.");
                    }
                    if (result.ToLower() == "not ok")
                    {
                        mypos.HasPushed = false;
                        mypos.HasWork = false;
                        mypos.UpdatedAt = DateTime.Now;
                        _context.SaveChanges();

                        msg += "There is an issue moving the application to the next processing staff for action. Please try again." + Environment.NewLine;
                        response = msg;
                    }
                    else if (result.ToLower().Contains("processing complete"))
                    {
                        var vApp = _context.applications.Where(a => a.id == appId).FirstOrDefault();
                        var fac = _context.Facilities.Where(a => a.Id == vApp.FacilityId).FirstOrDefault();
                        var comp = _context.companies.Where(a => a.id == vApp.company_id).FirstOrDefault();
                        var appPhase = _context.Phases.Where(a => a.id == vApp.PhaseId).FirstOrDefault();

                        //Update My Process
                        mypos.HasPushed = true;
                        mypos.HasWork = true;
                        mypos.UpdatedAt = DateTime.Now;
                        _context.SaveChanges();

                        var p = _helpersController.CreatePermit(vApp, fac);

                        //var rl = _context.UserRoles.Where(rl => rl.RoleName.Contains("Printer")).FirstOrDefault();
                        var rl = _context.UserRoles.Where(r => r.RoleName.ToLower() == "supervisor" && r.DeleteStatus != true).FirstOrDefault();
                        var usrs = _context.Staff.Where(us => us.RoleID == rl.Role_id && us.DeleteStatus != true && us.LocationID == 1).ToList();
                        msg += "You have APPROVED this application (" + vApp.reference + ")  and license/approval has been generated. License/Approval No: " + p + Environment.NewLine;
                        response = "Approve";

                        #region Update TankModification if Approved
                        if (appPhase.ShortName == "DM" || appPhase.ShortName == "ATC" || appPhase.ShortName == "UWA" || appPhase.ShortName == "NDT" || appPhase.ShortName == "RC")//UWA :: Update without Modification
                        {
                            //check facilityTankModification Table with the ApplicationID
                            var tanksToModify = _context.FacilityTankModifications.Where(a => a.ApplicationId == vApp.id).ToList();
                            var appTanks = (from t in _context.ApplicationTanks.AsEnumerable()
                                            join ft in _context.Tanks on t.TankId equals ft.Id
                                            where t.ApplicationId == appId
                                            select t).ToList();


                            if (appTanks.Count > 0)
                            {

                                foreach (var item in appTanks)
                                {
                                    var tnk = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                    if (tnk != null)
                                    {
                                        if (tanksToModify.Count > 0 && tanksToModify.FirstOrDefault()?.ModifyType == "Decommission")
                                        {
                                            tnk.Decommissioned = true;
                                        }
                                        else
                                        {

                                            var prd = _context.Products.Where(a => a.Id == item.ProductId).FirstOrDefault();
                                            string newTankName = tnk.Name.Split(" ")[0] != null? tnk.Name.Replace(tnk.Name.Split(" ")[0], prd.Name) : prd != null ? prd.Name + "Tank": "";  
                                            tnk.Name = newTankName;
                                            tnk.ProductId = item.ProductId;
                                            tnk.MaxCapacity = item.Capacity.ToString();

                                        }
                                        tnk.Status = GeneralClass.Approved;
                                        _context.SaveChanges();

                                    }

                                    //check other facility applications in processing and update app tanks to approved tanks
                                    var otherAppTanks = (from at in _context.ApplicationTanks
                                                         join ap in _context.applications on at.ApplicationId equals ap.id
                                                         where ap.FacilityId == vApp.Facility.Id && at.TankId == item.TankId && ap.status == GeneralClass.Processing
                                                         select at).ToList();
                                    if (otherAppTanks.Count() > 0)
                                    {
                                        otherAppTanks.ForEach(x =>
                                        {

                                            x.ProductId = (int)tnk.ProductId;
                                            x.TankName = tnk.Name;
                                            _context.SaveChanges();

                                        }); 
                                    }
                                }

                            }

                        }

                        #endregion

                        #region Change TakeOver Facility Name
                        if (appPhase.ShortName == "TO")
                        {
                            fac.Name = comp.name + " " + "Facility";
                            _context.SaveChanges();
                        }

                        #endregion

                        #region Completing Joint Operations
                        if (userRole.Contains("Supervisor") || userRole.Contains("Inspector"))
                        {
                            //var jointAppOperation = _context.JointApplications.Where(a => a.applicationId == appId).ToList();
                            var jointAppOperation = _context.JointAccounts.Where(a => a.ApplicationId == appId && a.OperationsCompleted != true).ToList();

                            bool lhis = false;
                            string ops = "";
                            foreach (var jointApp in jointAppOperation)
                            {
                                if (jointApp != null)
                                {
                                    if (jointApp.OperationsCompleted != true)
                                    {
                                        lhis = true;
                                        jointApp.OperationsCompleted = true;
                                        //jointApp. = DateTime.Now;
                                        ops += jointApp.Opscon + ", ";
                                        //get joint staff too and complete theirs too
                                        var jst = _context.JointAccountStaffs.Where(a => a.JointAccountId == jointApp.Id).ToList();
                                        foreach (var item in jst)
                                        {
                                            if (item != null)
                                            {
                                                if (item.SignedOff != true)
                                                {
                                                    item.SignedOff = true;
                                                    item.OperationsCompleted = true;
                                                    _context.SaveChanges();
                                                    ops += item.Staff + ", ";
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            if (lhis)
                            {

                                string commentH = "Auto completion of application (Joint Inspection) on the desk of :: " + ops;
                                _helpersController.SaveHistory(appId, userID, userEmail, GeneralClass.AutoCompletion, commentH);

                                #endregion

                            }
                            var app = _context.applications.Where(a => a.id == appId).FirstOrDefault();
                            var stf = _context.Staff.Where(x => x.StaffID == userID && x.DeleteStatus != true).FirstOrDefault();
                            string office = "";
                            if (stf != null)
                            {
                                var location = _context.Location.Where(x => x.LocationID == stf.LocationID).FirstOrDefault();
                                office = location.LocationName;
                            }
                            //if (app.AppProcessed != true && (userRole == GeneralClass.SUPERVISOR || (userRole == GeneralClass.INSPECTOR))
                            if (app.AppProcessed != true && userRole == GeneralClass.INSPECTOR && office == GeneralClass.HQ)
                            {
                                app.AppProcessed = true;
                                _context.SaveChanges();

                            }
                        }

                        //vApp.current_Permit = p; //assign application current permit
                        //_context.SaveChanges();

                        if (!p.ToLower().Contains("error"))
                        {
                            foreach (var item in usrs)
                            {

                                string body = "";
                                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                                string file = up + @"\\Templates\" + "InternalMemo.txt";
                                using (var sr = new StreamReader(file))
                                {

                                    body = sr.ReadToEnd();
                                }

                                string subject = "License/Approval has been Generated";
                                string mssg = "A license/approval has been generated for application with reference: " + vApp.reference + " for " + fac.Name + "(" + comp.name + "). License/Approval No: " + p + " and should now be ready for printing. ";

                                string msgBody = string.Format(body, subject, "", $"{item.StaffEmail}", mssg);
                                var emailMsg = _helpersController.SaveMessage(vApp.id, item.StaffID, subject, mssg, item.StaffElpsID.ToString(), "Staff");

                                var sendEmail = _helpersController.SendEmailMessage2Staff(item.StaffEmail, item.FirstName, emailMsg, null);

                                //send email to company
                                _helpersController.LogMessages("New application created successfully for facility => " + fac.Name + ". Application Reference : " + vApp.reference, userEmail);

                            }
                            var stf = _context.Staff.Where(st => st.StaffID == userID).FirstOrDefault();

                            string subject2 = "License/Approval for Application with Ref: " + vApp.reference;
                            string msg2 = "A license/approval has been generated for application with reference: " + vApp.reference + " for " + fac.Name + "(" + comp.name + "). License/Approval No: " + p + " and should now be ready for printing. ";

                            var emailMsg2 = _helpersController.SaveMessage(vApp.id, stf.StaffID, subject2, msg2, stf.StaffElpsID.ToString(), "Staff");
                            var sendEmail2 = _helpersController.SendEmailMessage2Staff(stf.StaffEmail, stf.FirstName, emailMsg2, null);

                            //send email to company

                            var emailMsg3 = _helpersController.SaveMessage(vApp.id, comp.id, subject2, msg2, comp.elps_id.ToString(), "Company");
                            var sendEmail3 = _helpersController.SendEmailMessage(comp.CompanyEmail.ToString(), comp.name, emailMsg3, null);

                            _helpersController.LogMessages("New application approved successfully for facility => " + fac.Name + ". Application Reference : " + vApp.reference, userEmail);

                            _helpersController.SaveHistory(appId, userID, GeneralClass.Approved, userEmail + "Final Approval For Application With Ref: " + vApp.reference);


                            TempData["message"] = frommydesk != null ? "Selected applications(s) final approval successful." : "Application has been approved and license/approval generated successfully.";
                            TempData["msgType"] = "success";
                        }

                    }
                    else
                    {


                        msg += "Application has been pushed to the next processing officer for appropriate action" + Environment.NewLine;
                        response = "Approve 1";

                        #region Completing Joint Operations
                        if (userRole.Contains("Supervisor") || userRole.Contains("Inspector"))
                        {
                            var jointAppOperation = _context.JointAccounts.Where(a => a.ApplicationId == appId && a.OperationsCompleted != true).ToList();
                            bool lhis = false;
                            string ops = "";
                            foreach (var jointApp in jointAppOperation)
                            {
                                if (jointApp != null)
                                {
                                    if (jointApp.OperationsCompleted != true)
                                    {
                                        lhis = true;
                                        jointApp.OperationsCompleted = true;
                                        //  jointApp.UpdatedAt = DateTime.Now;
                                        ops += jointApp.Opscon + ", ";
                                        //get joint staff too and complete theirs too
                                        var jst = _context.JointAccountStaffs.Where(a => a.JointAccountId == jointApp.Id).ToList();
                                        foreach (var item in jst)
                                        {
                                            if (item != null)
                                            {
                                                if (item.SignedOff != true)
                                                {
                                                    item.SignedOff = true;
                                                    item.OperationsCompleted = true;
                                                    _context.SaveChanges();
                                                    ops += item.Staff + ", ";
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            if (lhis)
                            {

                                string commentH = "Auto Completion of Application on the Desk of :: " + ops;
                                _helpersController.SaveHistory(appId, userID, userEmail, GeneralClass.AutoCompletion, commentH);

                                #endregion

                            }
                            var app = _context.applications.Where(a => a.id == appId).FirstOrDefault();

                            if (app.AppProcessed != true && (userRole == GeneralClass.SUPERVISOR || userRole == GeneralClass.INSPECTOR))
                            {
                                app.AppProcessed = true;
                                _context.SaveChanges();

                            }
                        }

                        if (userRole == GeneralClass.SUPERVISOR || userRole == GeneralClass.TEAMLEAD)
                        {
                            //var mydesk = processes.Where(a => a.SortOrder > mypos.SortOrder).OrderBy(a => a.SortOrder).ToList();
                            var mydesk = _context.MyDesk.Where(a => a.AppId == appId && a.StaffID == userID && a.HasWork != true).OrderBy(a => a.Sort).ToList();

                            var first = mydesk.FirstOrDefault();
                            if (first != null)
                            {
                                processId = first.ProcessID;
                                first.HasPushed = true;
                                first.HasWork = true;
                                first.AutoPushed = true;
                                first.UpdatedAt = DateTime.Now;
                                _context.SaveChanges();
                            }
                            var second = mydesk.Skip(1).FirstOrDefault();
                            if (second != null)
                            {
                                processId = second.ProcessID;
                                second.HasPushed = true;
                                second.HasWork = true;
                                second.AutoPushed = true;
                                second.UpdatedAt = DateTime.Now;
                                _context.SaveChanges();
                            }
                        }

                        //Update My Process
                        mypos.HasPushed = true;
                        mypos.HasWork = true;
                        mypos.UpdatedAt = DateTime.Now;
                        _context.SaveChanges();


                    }

                }
            }
            catch (Exception x)
            {
                _helpersController.LogMessages($"Approve Error:: {x.ToString()}");
            }

            msgType = "pass";

            TempData["message"] = msg;
            TempData["msgType"] = msgType;
            if (frommydesk != null)
            {
                TempData["msgType"] = "Selected application(s) have been successfully approved";
                return RedirectToAction("MyDesk");
            }
            return Json(response);
        }

        #region Application Rejection
        public IActionResult Reject(string id, string option)
        {

            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));


            int DeskId = Convert.ToInt32(id);
            int histID = Convert.ToInt32(option);


            if (userEmail == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again") });
            }

            var appProc = _context.MyDesk.Where(a => a.DeskID == DeskId).FirstOrDefault();
            var app = _context.applications.Where(a => a.id == appProc.AppId).FirstOrDefault();
            var fac = _context.Facilities.Where(a => a.Id == app.FacilityId).FirstOrDefault();
            var MyApp = (from ap in _context.applications
                         join c in _context.companies.AsEnumerable() on ap.company_id equals c.id
                         join facl in _context.Facilities.AsEnumerable() on ap.FacilityId equals fac.Id
                         join cat in _context.Categories.AsEnumerable() on ap.category_id equals cat.id
                         join phs in _context.Phases.AsEnumerable() on ap.PhaseId equals phs.id
                         join his in _context.application_desk_histories on ap.id equals his.application_id
                         where his.id == histID
                         select new MyApps
                         {
                             appHistory = his,
                             CategoryName = cat.name,
                             PhaseId = phs.id,
                             PhaseName = phs.name,
                             appID = ap.id,
                             Reference = ap.reference,
                             Date_Added = Convert.ToDateTime(ap.CreatedAt),
                             FacilityName = facl.Name,
                             CompanyName = c.name,
                             Status = ap.status

                         }).FirstOrDefault();


            if (appProc != null && app != null)
            {
                var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();

                //get Processes
                var processes = _context.MyDesk.Where(a => a.AppId == app.id).OrderBy(a => a.Sort).ToList();
                var mypos = processes.Where(a => a.HasWork != true).FirstOrDefault();

                if ((mypos.Sort == 2 && userRole.Contains("Supervisor")) || userRole.Contains("Inspector"))
                {

                    //Returns list of required Doc Types
                    //#region old get app doc

                    //var rf = appHelper.GetApplicationFiles(app, company.elps_id.GetValueOrDefault(), facility.Elps_Id.GetValueOrDefault());
                    //var docRemaining = rf.Where(a => a.Selected == false).ToList();

                    //var allCompanyDocs = _helpersController.GetCompanyDocuments((int)company.elps_id);

                    //ViewBag.ApplicationFile = rf.Where(a => a.Selected).ToList(); //appDocs;
                    //                                                              // _nextAppr.DocsRemaining(allCoyDocs, vAppProc.ApplicationId.ToString()); // (appDocs, vAppProc.ApplicationId.ToString());
                    //ViewBag.AppFileNotSubmittedYet = docRemaining; // docRemaining;// rqfNotAvailabl;

                    //int cid = company.elps_id.GetValueOrDefault();

                    //var allDocuments = _helpersController.GetElpsDocumentsTypes();
                    //List<Document_Type> docs = new List<Document_Type>();
                    //List<Document_Type> otherDocs = new List<Document_Type>();

                    //if (allDocuments != null)
                    //{
                    //    docs = JsonConvert.DeserializeObject<List<Document_Type>>(allDocuments.Value.ToString());
                    //}
                    //else
                    //{
                    //    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A netork failure has occured. Kindly ensure you have a strong internet connection and try again") });

                    //}
                    //foreach (var item in docs)
                    //{
                    //    if (!docRemaining.Contains(item)) // Not in Pending Documents
                    //    {
                    //        if (allCompanyDocs.FindAll(C => C.document_type_id == item.Id).Count > 0) // Not in Company Documents
                    //        {
                    //            item.Selected = true;
                    //        }
                    //        otherDocs.Add(item);
                    //    }
                    //}
                    //if (!(otherDocs.Where(d => d.Name.ToLower() == "other document").Count() > 0))
                    //{
                    //    var pick = docs.Where(d => d.Name.ToLower() == "other document").FirstOrDefault();
                    //    if (pick != null)
                    //        otherDocs.Add(pick);
                    //}

                    //ViewBag.OtherDocuments = otherDocs;
                    //#endregion
                    #region getAppDoc

                    List<OtherDocuments> otherDocuments = new List<OtherDocuments>();

                    var submittedDoc = _context.SubmittedDocuments.Where(x => x.AppID == app.id && x.DeletedStatus != true);

                    var getDocuments = _context.ApplicationDocuments.Where(x => x.PhaseId == MyApp.PhaseId && x.docType == "Facility" && x.DeleteStatus == false).ToList();

                    List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                    List<MissingDocument> missingDocuments = new List<MissingDocument>();
                    List<BothDocuments> bothDocuments = new List<BothDocuments>();


                    if (getDocuments.Count() > 0)
                    {
                        ViewData["FacilityElpsID"] = facility.Elps_Id;
                        ViewData["CompanyElpsID"] = company.elps_id;

                        List<LpgLicense.Models.FacilityDocument> facilityDoc = generalClass.getFacilityDocuments(facility.Elps_Id.ToString());

                        if (facilityDoc == null)
                        {
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                        }
                        getDocuments.ForEach(x =>
                        {
                            foreach (var fDoc in facilityDoc.ToList())
                            {
                                //if (fDoc.Document_Type_Id == getDocuments.FirstOrDefault().ElpsDocTypeID)
                                if (fDoc.Document_Type_Id == x.ElpsDocTypeID)
                                {
                                    presentDocuments.Add(new PresentDocuments
                                    {
                                        Present = true,
                                        FileName = fDoc.Name,
                                        Source = fDoc.Source,
                                        CompElpsDocID = fDoc.Id,
                                        DocTypeID = fDoc.Document_Type_Id,
                                        LocalDocID = getDocuments.FirstOrDefault().AppDocID,
                                        DocType = getDocuments.FirstOrDefault().docType,
                                        TypeName = getDocuments.FirstOrDefault().DocName

                                    });
                                }
                            }
                        });
                        var result = getDocuments.AsEnumerable().Where(x => !presentDocuments.AsEnumerable().Any(x2 => x2.LocalDocID == x.AppDocID));

                        foreach (var r in result.ToList())
                        {
                            missingDocuments.Add(new MissingDocument
                            {
                                Present = false,
                                DocTypeID = r.ElpsDocTypeID,
                                LocalDocID = r.AppDocID,
                                DocType = r.docType,
                                TypeName = r.DocName
                            });
                        }

                        bothDocuments.Add(new BothDocuments
                        {
                            missingDocuments = missingDocuments,
                            presentDocuments = presentDocuments,
                        });

                        _helpersController.LogMessages("Loading facility information and document for report upload : " + app.reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                        _helpersController.LogMessages("Displaying/Viewing more application details.  Reference : " + app.reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                        ViewBag.OtherDocuments = missingDocuments;
                        ViewBag.ApplicationDocs = presentDocuments;

                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying get staff report for application. Kindly contact support.") });
                    }
                    #endregion


                    ViewBag.checkerReject = "yes";
                    return View("RejectN", MyApp);

                }
                else
                {
                    ViewBag.Application = app;

                    return View("_MiniReject", MyApp);


                }

            }
            return View("Error");
        }


        /*
        * A application rejection process/method
        * 
        * RequiredDocs => a list of document id for rejection
        * 
        */


        [HttpPost]
        public IActionResult Reject(application_desk_histories model, string ToMarketer, int applicationid, int[] fileid, string[] newRequest, List<int> RequiredDocs, string chckr)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            string msgBody = string.Empty;
            string user_id = string.Empty;
            string referenceNo = string.Empty;

            var myDesk = _context.MyDesk.Where(a => a.AppId == model.application_id && a.HasWork != true).FirstOrDefault();
            applicationid = model.application_id;
            try
            {

                var app = _context.applications.Where(a => a.id == applicationid).FirstOrDefault();

                var appl = (from ap in _context.applications.AsEnumerable()
                            join c in _context.companies.AsEnumerable() on ap.company_id equals c.id
                            join fac in _context.Facilities.AsEnumerable() on ap.FacilityId equals fac.Id
                            join cat in _context.Categories.AsEnumerable() on ap.category_id equals cat.id
                            join phs in _context.Phases.AsEnumerable() on ap.PhaseId equals phs.id
                            join sb in _context.SubmittedDocuments.AsEnumerable() on ap.id equals sb.AppID into subdoc
                            where ap.DeleteStatus != true && ap.id == applicationid
                            select new MyApps
                            {
                                appID = ap.id,
                                Reference = ap.reference,
                                CategoryName = cat.name,
                                PhaseName = phs.name,
                                Status = ap.status,
                                Date_Added = Convert.ToDateTime(ap.date_added),
                                Submitted = ap.submitted,
                                CompanyDetails = c.name + " (" + c.Address + ") ",
                                CompanyName = c.name,
                                Company_Id = c.id,
                                FacilityId = fac.Id,
                                FacilityDetails = fac.Name,
                                FacilityName = fac.Name,
                                Year = ap.year,
                                Type = ap.type,
                                ApplicationDocs = subdoc.ToList()
                            }).FirstOrDefault();


                if (ToMarketer != null || userRole.Contains("Inspector"))
                {
                    if (CheckerReject(model, RequiredDocs, applicationid, fileid))
                    {
                        //update current staff desk table
                        myDesk.HasWork = true; myDesk.HasPushed = true; myDesk.UpdatedAt = DateTime.Now;
                        myDesk.Comment = model.comment;
                        _context.SaveChanges();

                        TempData["message"] = "Application has been rejected back to marketer.";
                        TempData["msgType"] = "success";
                        //_helpersController.SaveHistory(model.application_id, userID, userEmail, GeneralClass.Rejected, model.comment);

                        return RedirectToAction("MyDesk");
                    }



                    return RedirectToAction("MyDesk");
                }
                // For upper roles, return to the previous processor

                else if (NonCheckerReject(model, myDesk, applicationid))
                {
                    //update current staff desk table
                    myDesk.HasWork = true; myDesk.HasPushed = true; myDesk.UpdatedAt = DateTime.Now;
                    myDesk.Comment = model.comment;
                    _context.SaveChanges();

                    string histCmt = userEmail + " rejected application with comment:" + model.comment;
                    //_helpersController.SaveHistory(model.application_id, userID, userEmail, GeneralClass.Rejected, histCmt);

                    TempData["message"] = "Application has been rejected back to the previous staff.";
                    TempData["msgType"] = "success";
                    return RedirectToAction("MyDesk");
                }
                else
                {
                    //no rejection was done, return to View application for staff
                    TempData["msgType"] = "fail";
                    TempData["message"] = "An error occured while trying to reject this application";
                    return RedirectToAction("ViewApplication", new { id = generalClass.Encrypt(app.id.ToString()) });

                }
            }
            catch (Exception ex)
            {
                _helpersController.LogMessages(ex.ToString());
                return RedirectToAction("MyDesk");
            }


        }
        // Outright reject to Client for action before proceeding. Done by either the Supervisor or the Checker
        private bool CheckerReject(application_desk_histories model, List<int> RequiredDocs, int applicationid, int[] fileid)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {

                var myApp = (from proc in _context.MyDesk.AsEnumerable()
                             join appl in _context.applications.AsEnumerable() on proc.AppId equals appl.id
                             join c in _context.companies.AsEnumerable() on appl.company_id equals c.id
                             join fac in _context.Facilities.AsEnumerable() on appl.FacilityId equals fac.Id
                             join cat in _context.Categories.AsEnumerable() on appl.category_id equals cat.id
                             join phs in _context.Phases.AsEnumerable() on appl.PhaseId equals phs.id
                             where proc.StaffID == userID && proc.HasWork != true &&
                              appl.DeleteStatus != true && c.DeleteStatus != true
                             select new MyApps
                             {
                                 appID = appl.id,
                                 Reference = appl.reference,
                                 CategoryName = cat.name,
                                 PhaseName = phs.name,
                                 //Stage = s.StageName,
                                 Status = appl.status,
                                 Date_Added = Convert.ToDateTime(appl.date_added),
                                 Submitted = appl.submitted,
                                 CompanyDetails = c.name + " (" + c.Address + ") ",
                                 CompanyName = c.name,
                                 Company_Id = c.id,
                                 FacilityDetails = fac.Name,
                                 FacilityName = fac.Name,
                                 processID = proc.ProcessID,
                                 DateProcessed = proc.CreatedAt,
                                 Year = appl.year,
                                 Type = appl.type,
                                 Holding = proc.Holding,
                                 AllowPush = appl.AllowPush
                             }).FirstOrDefault();


                #region Add requested document(s)
                if (RequiredDocs != null)
                {
                    foreach (var r in RequiredDocs)
                    {
                        var checkDocs = _context.SubmittedDocuments.Where(x => x.AppDocID == r && x.AppID == applicationid && x.DeletedStatus != true);

                        if (checkDocs.Count() <= 0)
                        {
                            var submitDoc = new SubmittedDocuments()
                            {
                                AppID = applicationid,
                                AppDocID = r,
                                IsAdditional = true,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            _context.SubmittedDocuments.Add(submitDoc);
                        }
                    }
                    _context.SaveChanges();
                }


                #endregion



                var app = _context.applications.Where(a => a.id == applicationid).FirstOrDefault();
                var comp = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();

                var cElpsId = comp != null ? comp.elps_id : 0;
                app.status = GeneralClass.Rejected;
                //appRl.current_desk = null;
                _context.SaveChanges();
                //#region Update application to rejected on ELPS
                //var appAPI = new ApplicationAPIModel();
                //    appAPI.Status = app.status;
                //    appAPI.OrderId = app.reference;

                //    var param = JsonConvert.SerializeObject(appAPI);

                //    var url = ElpsServices._elpsBaseUrl;
                //    var email = ElpsServices._elpsAppEmail;
                //    var appHash = ElpsServices.appHash;
                //    url += "/api/Application/" + email + "/" + appHash;

                //    var paramDatas = _restService.parameterData("app", param);
                //    var output = _restService.Response("/api/Application/{email}/{apiHash}/{app}", paramDatas, "PUT");

                //    _helpersController.LogMessages($"Rejection:: {url}, pram:: {param}");
                //    _helpersController.LogMessages($"Rejection:: Output {output.Content.ToString()}");
                //    var respApp = JsonConvert.DeserializeObject<ApplicationAPIModel>(output.Content);

                //#endregion
                #endregion

                //Add Newly requested documents to the Application
                string extra = "";
                if (RequiredDocs != null && RequiredDocs.Count() > 0)
                {
                    extra = "<br /><br /><b>Please note:</b> The following are the newly requested documents for your application<br />";
                    extra += "<ul style=\"padding-left: 20px;\">";
                    foreach (var item in RequiredDocs)
                    {
                        #region Invalidate Rejected Documents on ELPS
                        //try
                        //{

                        //    url += "/api/UpdateDocument/" + item + "/" + cElpsId + "/" + false.ToString() + "/" + email + "/" + appHash;
                        //    _helpersController.LogMessages($"Rejection file:: {url}, pram:: {param}");
                        //    paramDatas = _restService.parameterData("item", item.ToString());
                        //    paramDatas = _restService.parameterData("id", cElpsId.ToString());
                        //    paramDatas = _restService.parameterData("false", false.ToString());
                        //    output = _restService.Response("/api/UpdateDocument/{email}/{apiHash}/{item}/{id}/{false}", paramDatas, "PUT");

                        //    _helpersController.LogMessages($"Rejection file output:: {output.Content}");
                        //    var tt = JsonConvert.DeserializeObject<Company_Document>(output.Content);
                        //}
                        //catch (Exception x)
                        //{

                        //    _helpersController.LogMessages($"Rejection file Error:: {x.ToString()}");

                        //}
                        #endregion
                        int documentId = Convert.ToInt16(item);

                        var dcApp = _context.ApplicationDocuments.Where(x => x.AppDocID == documentId).ToList();
                        extra += "<li>" + dcApp.FirstOrDefault().DocName + "</li>";
                    }
                    extra += "</ul>";


                    //loghis
                }
                #region send Mail
                var date = DateTime.Now;

                var dt = date.Day.ToString() + date.Month.ToString() + date.Year.ToString();
                var sn = string.Format("NMDPRA/PDJ/{0}/{1}", dt, app.company_id);
                var body = "";

                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var file = Path.Combine(up, "Templates/ApplicationRejection.txt");

                using (var sr = new StreamReader(file))
                {

                    body = sr.ReadToEnd();
                }
                string subject = "Rejection For Application with Ref. No:" + app.reference;
                string content = "Your application has been rejected back to you. Please see rejection comment for neceessary action: " + model.comment;


                // Send Mail to Company

                var emailMsg = _helpersController.SaveMessage(model.application_id, comp.id, subject, content, comp.elps_id.ToString(), "Company");
                var sendEmail = _helpersController.SendEmailMessage(comp.CompanyEmail.ToString(), comp.name, emailMsg, null);

                _helpersController.SaveHistory(model.application_id, userID, userEmail, GeneralClass.Rejected, model.comment);

                #endregion
                return true;
            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                return false;
            }
        }


        private bool NonCheckerReject(application_desk_histories model, MyDesk appProc, int applicationid)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {
                #region Old RejectProcess
                //var processes = _context.MyDesk.Where(a => a.AppId == app.id).OrderBy(a => a.Sort).ToList();
                //var currentIndex = appProc.Sort == 1 ? 1 : processes.IndexOf(appProc);
                //currentIndex -= 1;
                //var prevProc = processes[currentIndex];
                //while (prevProc.AutoPushed == true)
                //{
                //    currentIndex -= 1;
                //    prevProc = processes[currentIndex];
                //}
                #endregion
                var prevProc = new MyDesk();
                int staffToRejectTo = 0;
                int prevDK = 0;
                if (appProc.FromStaffID != 0)
                {
                    staffToRejectTo = (int)appProc.FromStaffID;
                    var prevDesK = _context.MyDesk.Where(a => a.StaffID == userID && a.AppId == applicationid).OrderByDescending(x => x.DeskID).FirstOrDefault();
                    prevDK = prevDesK != null ? prevDesK.DeskID : prevDK;
                    //prevProc = _context.MyDesk.Where(a => a.StaffID == staffToRejectTo && a.DeskID == (prevDK-1) && a.AppId == applicationid).FirstOrDefault();
                    prevProc = _context.MyDesk.Where(a => a.StaffID == staffToRejectTo && a.AppId == applicationid).OrderByDescending(x => x.DeskID).FirstOrDefault();

                }
                else if (model.StaffID != 0 && model.StaffID != null)
                {
                    //check application desk history
                    staffToRejectTo = (int)model.StaffID;
                    prevProc = _context.MyDesk.Where(a => a.StaffID == staffToRejectTo && a.Sort == (appProc.Sort - 1) && a.AppId == applicationid).FirstOrDefault();

                }
                else
                {
                    prevProc = _context.MyDesk.Where(a => a.Sort == appProc.Sort - 1 && a.AppId == applicationid).FirstOrDefault();
                }

                if (prevProc != null)
                {
                    var app = _context.applications.Where(a => a.id == applicationid).FirstOrDefault();

                    //Check if previous staff is still active on the system

                    var getStaff = _context.Staff.Where(a => a.StaffID == prevProc.StaffID).FirstOrDefault();

                    if (getStaff.ActiveStatus != true || getStaff.DeleteStatus == true) //Staff to reject to is not active or has been deleted from the system
                    {
                        var newStaff = _context.Staff.Where(a => a.FieldOfficeID == getStaff.FieldOfficeID && a.RoleID == getStaff.RoleID && a.ActiveStatus == true && a.DeleteStatus != true).FirstOrDefault();
                        if (newStaff != null)
                        {
                            //save staff info to mydesk
                            var newdesk = new MyDesk()
                            {
                                StaffID = newStaff.StaffID,
                                FromStaffID = prevProc?.FromStaffID,
                                HasWork = false,
                                HasPushed = false,
                                AppId = app.id,
                                Sort = prevProc.Sort,
                                ProcessID = (int)prevProc.ProcessID,
                                CreatedAt = DateTime.Now,
                                // Comment = actionComment
                            };
                            _context.MyDesk.Add(newdesk);
                            _context.SaveChanges();

                        }

                        if (prevProc.Sort == 2)
                        {
                            //application has been returned to either a Supervisor or an Inspector

                            if (app != null)
                            {
                                if (app.SupervisorProcessed == true)
                                {
                                    app.SupervisorProcessed = false;
                                    //get all the appProcess that was auto pushed and revert it
                                }
                                app.AppProcessed = false;
                                _context.SaveChanges();
                            }
                        }
                        string comment2 = newStaff != null
                            ? userEmail + " Returned application to staff (" + newStaff?.StaffEmail + "):" + model.comment
                            : userEmail + " Returned application to marketer : " + model.comment;


                        //update app table
                        app.current_desk = prevProc?.StaffID;
                        //app.current_desk = appProc.Sort == 1 ? null : prevProc?.StaffID;
                        app.status = appProc.Sort == 1 ? GeneralClass.Rejected : app.status;
                        int save = _context.SaveChanges();
                        if (save > 0)
                        {
                            _helpersController.SaveHistory(app.id, _helpersController.getSessionUserID(), userEmail, "Rejection", "Application was rejected to " + newStaff.StaffEmail + " ( Previous Staff: " + getStaff.StaffEmail + ") See comment: " + model.comment + ".");

                            string subject = "Rejection For Application With Ref: " + app.reference;
                            string content = userEmail + " has returned application (" + app.reference + ") to you because of the in-availability of " + getStaff.StaffEmail + " . See application history for more";
                            var saveMsg = _helpersController.SaveMessage(model.application_id, newStaff.StaffID, subject, content, newStaff.StaffElpsID, "Staff");

                            //var sendEmail = _helpersController.SendEmailMessage2Staff(getStaff.StaffEmail, getStaff.FirstName, emailMsg, null);

                            return true;
                        }
                    }




                    //Process for prev process
                    prevProc.HasWork = false;
                    prevProc.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();

                    if (prevProc.Sort == 2)
                    {
                        //application has been returned to either a Supervisor or an Inspector

                        if (app != null)
                        {
                            if (app.SupervisorProcessed == true)
                            {
                                app.SupervisorProcessed = false;
                                //get all the appProcess that was auto pushed and revert it
                            }
                            app.AppProcessed = false;
                            _context.SaveChanges();
                        }
                    }
                    string comment = getStaff != null
                        ? userEmail + " Returned application to staff (" + getStaff?.StaffEmail + "):" + model.comment
                        : userEmail + " Returned application to marketer : " + model.comment;


                    //update app table
                    app.current_desk = prevProc?.StaffID;
                    //app.current_desk = appProc.Sort == 1 ? null : prevProc?.StaffID;
                    app.status = appProc.Sort == 1 ? GeneralClass.Rejected : app.status;
                    int i = _context.SaveChanges();
                    if (i > 0)
                    {
                        _helpersController.SaveHistory(app.id, _helpersController.getSessionUserID(), userEmail, "Rejection", "Application was returned back to " + getStaff.StaffEmail + " with comment: " + model.comment + ".");

                        string subject = "Rejection For Application With Ref: " + app.reference;
                        string content = userEmail + " has returned application (" + app.reference + ") back to you. See application history for more";
                        var emailMsg = _helpersController.SaveMessage(model.application_id, getStaff.StaffID, subject, content, getStaff.StaffElpsID, "Staff");
                        //var sendEmail = _helpersController.SendEmailMessage2Staff(getStaff.StaffEmail, getStaff.FirstName, emailMsg, null);

                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Add Report
        /*
       * Saving application report
       * 
       * AppID => encrypted application id
       * txtReport => the comment for the report
       */
        //[Authorize(Policy = "ProcessingStaffRoles")]
        public JsonResult SaveReport(string AppID, string txtReport, string txtReportTitle, List<SubmitDoc> SubmittedDocuments)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            try
            {
                string result = "";

                int appID = Convert.ToInt32(AppID);

                //var appId = generalClass.Decrypt(AppID.ToString().Trim());

                if (appID == 0)
                {
                    result = "Application link error";
                }
                else
                {

                    Reports reports = new Reports()
                    {
                        AppId = appID,
                        StaffId = _helpersController.getSessionUserID(),
                        Title = txtReportTitle.ToUpper(),
                        StaffEmail = userEmail,

                        //ElpsDocId = SubmittedDocuments.FirstOrDefault()?.CompElpsDocID,
                        //AppDocId = SubmittedDocuments.FirstOrDefault()?.LocalDocID,
                        //DocSource = SubmittedDocuments.FirstOrDefault()?.DocSource,
                        Comment = txtReport,
                        CreatedAt = DateTime.Now,
                        DeletedStatus = false
                    };

                    _context.Reports.Add(reports);

                    if (_context.SaveChanges() > 0)
                    {

                        #region

                        var jointAcc = _context.JointAccounts.Where(a => a.ApplicationId == appID).FirstOrDefault();
                        if (jointAcc != null)
                        {
                            reports.JointAccountId = jointAcc.Id;
                            _context.SaveChanges();
                        }

                        #endregion

                        var apps = _context.applications.Where(x => x.id == appID);

                        result = "Report Saved";
                        _helpersController.LogMessages("Saving report for application : " + apps.FirstOrDefault().reference, _helpersController.getSessionEmail());
                        _helpersController.SaveHistory(appID, _helpersController.getSessionUserID(), userEmail, "Add Report", "A report has been added to this application.");

                    }
                    else
                    {
                        result = "Something went wrong trying to save your report";
                    }
                }
                _helpersController.LogMessages("Operation result from saving report : " + result, _helpersController.getSessionEmail());
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }
        }

        //[Authorize(Policy = "ProcessingStaffRoles")]
        public JsonResult GetReport(string ReportID)
        {
            try
            {
                string result = "";

                int rID = 0;

                var rid = generalClass.Decrypt(ReportID.Trim());

                if (rid == "Error")
                {
                    result = "0|Application report link error";
                }
                else
                {
                    rID = Convert.ToInt32(rid);

                    var get = _context.Reports.Where(x => x.ReportId == rID);

                    if (get.Count() > 0)
                    {
                        result = "1|" + get.FirstOrDefault().Comment + "|" + get.FirstOrDefault().Title;
                    }
                    else
                    {
                        result = "0|Error... cannot find this application report."
        ;
                    }
                }

                _helpersController.LogMessages("Displaying report. Report ID : " + rID, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }
        }

        //[Authorize(Policy = "ProcessingStaffRoles")]
        public JsonResult EditReport(string ReportID, string txtReport, string txtReportTitle, List<SubmitDoc> SubmittedDocuments)
        {
            try
            {
                string result = "";

                int rID = 0;

                var reportId = generalClass.Decrypt(ReportID.ToString().Trim());

                if (reportId == "Error")
                {
                    result = "Application link error";
                }
                else
                {
                    rID = Convert.ToInt32(reportId);

                    var get = _context.Reports.Where(x => x.ReportId == rID);

                    if (get.Count() > 0)
                    {
                        int appid = (int)get.FirstOrDefault().AppId;

                        get.FirstOrDefault().Comment = txtReport;
                        get.FirstOrDefault().UpdatedAt = DateTime.Now;
                        get.FirstOrDefault().Title = txtReportTitle.ToUpper();


                        if (_context.SaveChanges() > 0)
                        {
                            result = "Report Edited";


                            _helpersController.SaveHistory((int)get.FirstOrDefault().AppId, _helpersController.getSessionUserID(), _helpersController.getSessionEmail(), "Edit Report", "A report has been updated for this application.");
                        }
                        else
                        {
                            result = "Something went wrong trying to save your report";
                        }
                    }
                    else
                    {
                        result = "Something went wrong trying to find this report.";
                    }
                }

                _helpersController.LogMessages("Report update status :" + result + " Report ID : " + rID, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please contact support");
            }
        }



        /*
        * Deleting application report
        * 
        * ReportID => encryted report id
        */
        //[Authorize(Policy = "ProcessingStaffRoles")]
        public JsonResult DeleteReport(string ReportID)
        {
            try
            {
                string result = "";

                int rID = 0;

                var reportId = generalClass.Decrypt(ReportID.ToString().Trim());

                if (reportId == "Error")
                {
                    result = "Application link error";
                }
                else
                {
                    rID = Convert.ToInt32(reportId);
                }

                var get = _context.Reports.Where(x => x.ReportId == rID);

                if (get.Count() > 0)
                {
                    int appid = (int)get.FirstOrDefault().AppId;

                    get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();
                    get.FirstOrDefault().DeletedStatus = true;
                    get.FirstOrDefault().DeletedAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {

                        result = "Report Deleted";
                        _helpersController.SaveHistory((int)get.FirstOrDefault().AppId, _helpersController.getSessionUserID(), _helpersController.getSessionEmail(), "Delete Report", "A report has been deleted for this application.");

                    }
                    else
                    {
                        result = "Something went wrong trying to delete your report";
                    }
                }
                else
                {
                    result = "Something went wrong trying to find this report.";
                }

                _helpersController.LogMessages("Report delete status :" + result + " Report ID : " + rID, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }
        }

        #endregion


        #region Joint Inspection

        [HttpPost]
        public IActionResult AddReport(int appId, string report)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userRole == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }


            var jointAcc = _context.JointAccounts.Where(a => a.ApplicationId == appId).FirstOrDefault();
            if (jointAcc == null)
            {

                TempData["msgType"] = "fail";
                TempData["message"] = "This Application cannot take a report at the Moment!";
                return RedirectToAction("ViewApplication", new { id = generalClass.Encrypt(appId.ToString()) });
            }

            var rpt = new JointAccountReports();
            rpt.ReportDate = DateTime.Now;
            rpt.JointAccountId = jointAcc.Id;
            rpt.Report = report;
            rpt.Reportby = userEmail.ToLower();

            _context.JointAccountReports.Add(rpt);
            int i = _context.SaveChanges();
            if (i > 0)
            {
                string comment = userEmail + " added report to application.";
                _helpersController.SaveHistory(appId, userID, userEmail, GeneralClass.Processing, comment);

                TempData["msgType"] = "pass";
                TempData["message"] = "Report has been added successfully";
                return RedirectToAction("ViewApplication", new { id = generalClass.Encrypt(appId.ToString()) });
            }
            else
            {

                TempData["message"] = "An error occured while adding your report to application. Please try again.";
                TempData["msgType"] = "fail";
            }
            return RedirectToAction("ViewApplication", new { id = generalClass.Encrypt(appId.ToString()) });

        }

        [HttpPost]
        public IActionResult SignOff(int applicationId)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userRole == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }

            var jointAcc = _context.JointAccounts.Where(a => a.ApplicationId == applicationId && a.Assigned == true).FirstOrDefault();
            if (jointAcc == null)
            {

                TempData["message"] = "An error occured while processing your request. Please try again.";
                TempData["msgType"] = "fail";
            }
            else
            {
                var jStaff = _context.JointAccountStaffs.Where(a => a.JointAccountId == jointAcc.Id && a.Staff.ToLower() == userEmail.ToLower()).FirstOrDefault();

                if (jStaff != null)
                {
                    jStaff.SignedOff = true;
                    jStaff.OperationsCompleted = true;
                    jointAcc.OperationsCompleted = true;
                    int i = _context.SaveChanges();
                    if (i > 0)
                    {
                        string comment = "Inpsector (" + userEmail + ") signed off on application";
                        _helpersController.SaveHistory(applicationId, userID, userEmail, GeneralClass.Processing, comment);
                        var app = _context.applications.Where(a => a.id == applicationId).FirstOrDefault();

                        TempData["message"] = "You have successfully Signed off on this application (" + app.reference + ")";
                        TempData["msgType"] = "pass";
                    }
                }
            }
            return RedirectToAction("MyDesk");
        }

        public IActionResult Optin(int id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userRole == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }

            try
            {

                //Id is the ID of the Meeting/Inspection involved
                var ms = _context.MeetingSchedules.Where(a => a.Id == id).FirstOrDefault();
                if (ms != null)
                {
                    var im = _context.InspectionMeetingAttendees.Where(a => a.StaffEmail.ToLower() == userEmail.ToLower() && a.MeetingScheduleId == id).FirstOrDefault();
                    if (im == null)
                    {
                        im = new InspectionMeetingAttendees
                        {
                            Date = DateTime.Now,
                            MeetingScheduleId = id,
                            StaffEmail = userEmail
                        };
                        _context.InspectionMeetingAttendees.Add(im);
                        int i = _context.SaveChanges();
                        if (i > 0)
                        {
                            string comment = "Inpsector (" + userEmail + ") Signed Off on application";
                            _helpersController.SaveHistory(ms.ApplicationId, userID, userEmail, GeneralClass.Processing, comment);
                            TempData["msgType"] = "pass";
                            TempData["message"] = "You have Successfully Opted In for Meeting/Inspection";

                        }
                    }
                    else
                    {
                        TempData["msgType"] = "pass";
                        TempData["message"] = "You already Opted In for this Meeting/Inspection";

                    }
                    return RedirectToAction("ViewApplication", new { id = generalClass.Encrypt(ms.ApplicationId.ToString()) });
                }
                ViewBag.Errormessage = "Schedule does not Exist or has been moved";
                return View("appError");
            }
            catch (Exception)
            {
                ViewBag.Errormessage = "Some Error occured while Processing your Application";
                return View("appError");
                //throw;
            }
        }

        #endregion

        //[Authorize(Roles = "Staff, Admin")]
        public IActionResult ApplicationHistory(int? Id)
        {
            if (Id != null)
            {
                var oldAppProc = _context.application_Processings.Where(a => a.ApplicationId == Id && a.processor > 0).ToList();
                var newAppProc = _context.MyDesk.Where(a => a.AppId == Id && a.HasWork != true).FirstOrDefault();

                var current = oldAppProc.FirstOrDefault(a => a.Assigned == true && a.Processed == false);

                var deskHist = _context.application_desk_histories.Where(a => a.application_id == Id).GroupBy(a => a.application_id).ToList();
                var deskHis = (from u in _context.application_desk_histories.AsEnumerable()
                               join app in _context.applications.AsEnumerable() on u.application_id equals app.id
                               join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                               join comp in _context.companies.AsEnumerable() on app.company_id equals comp.id
                               where u.id == Id
                               select new ApplicationDeskHistoryModel
                               {
                                   Status = u.status,
                                   Comment = u.comment,
                                   CategoryName = cat.name,
                                   CompanyName = comp.name,
                                   Date = u.date,
                                   ApplicationDate = app.date_added,
                                   application_id = app.id,
                                   UserName = u.UserName,

                               }).GroupBy(a => a.application_id).ToList();

                if (current != null)
                {
                    var getStaffFieldOffice = (from u in _context.Staff
                                               join fd in _context.FieldOffices on u.FieldOfficeID equals fd.FieldOffice_id
                                               where u.StaffEmail.ToLower() == current.StaffEmail.ToLower()
                                               select fd
                                              ).FirstOrDefault();
                    ViewBag.UsrName = current.StaffEmail;
                    ViewBag.StaffFieldOffice = getStaffFieldOffice.OfficeName;
                }
                else if (newAppProc != null)
                {
                    var getStaffFieldOffice = (from u in _context.Staff
                                               join fd in _context.FieldOffices on u.FieldOfficeID equals fd.FieldOffice_id
                                               where u.StaffID == newAppProc.StaffID
                                               select fd).FirstOrDefault();
                    ViewBag.UsrName = current.StaffEmail;
                    ViewBag.StaffFieldOffice = getStaffFieldOffice.OfficeName;

                }
                else if (current == null)
                {
                    ViewBag.Message = "Application is not on anybody's desk currently";
                }
                //else
                //{
                //    ViewBag.Message = "You are not allowed to view this page.";
                //}
                ViewBag.Header = "Application History";
                return View(deskHis);
                //}
                //else
                //    ViewBag.Message = "There is no History on this application yet to display.";
            }
            else
            {
                ViewBag.Message = "No Application is selected to view history.";
            }
            return View();

        }


        public IActionResult RejectedApplication(string id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int AppId = generalClass.DecryptIDs(id);
            if (userRole == "Error" || AppId == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }
            var myComp = _context.companies.Where(C => C.id == userID).FirstOrDefault();

            try
            {
                var deskHist = _context.application_desk_histories.Where(a => a.application_id == AppId).OrderByDescending(a => a.date).FirstOrDefault();
                var myApp = (from app in _context.applications.AsEnumerable()
                             join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                             join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                             join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                             join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                             join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                             join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                             where app.DeleteStatus != true && c.DeleteStatus != true
                             && fac.DeletedStatus != true && app.id == AppId && app.status == GeneralClass.Rejected
                             select new MyApps
                             {

                                 appID = app.id,
                                 Reference = app.reference,
                                 CategoryName = cat.name,
                                 PhaseName = phs.name,
                                 //Stage = s.StageName,
                                 Status = app.status,
                                 Date_Added = Convert.ToDateTime(app.date_added),
                                 DateSubmitted = app.CreatedAt == null ? Convert.ToDateTime(app.date_added) : Convert.ToDateTime(app.CreatedAt),
                                 Submitted = app.submitted,
                                 CompanyDetails = c.name + " (" + c.Address + ") ",
                                 CompanyName = c.name,
                                 Company_Id = c.id,
                                 FacilityDetails = fac.Name,
                                 FacilityName = fac.Name,
                                 Year = app.year,
                                 Type = app.type,
                                 Address_1 = ad.address_1,
                                 City = ad.city,
                                 StateName = sd.StateName,
                                 appHistory = deskHist
                             }).FirstOrDefault();

                ViewBag.Header = "My Rejected Application History";
                return View(myApp);
            }
            catch (Exception e)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, see reason :" + e.Message) });
            }

        }


        #region Schedules/Meetings/Inspections

        public IActionResult ScheduleMeeting(string id, string option)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int AppId = generalClass.DecryptIDs(id);
            int? appProcId = generalClass.DecryptIDs(option);
            if (userRole == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            };
            ViewBag.ApplicationId = AppId;
            if (appProcId == null)
            {
                var apProc = _context.MyDesk.Where(a => a.AppId == AppId).OrderBy(a => a.Sort).Where(a => a.HasWork != true).FirstOrDefault();

                if (apProc != null)
                {
                    ViewBag.AppProcId = apProc.ProcessID;
                }
            }
            else
                ViewBag.AppProcId = appProcId;
            //get application facility state Id
            var app = _context.applications.Where(a => a.id == AppId).FirstOrDefault();
            int fsid = 0;
            string facStateName = "";
            if (app != null)
            {
                //get the Facility
                var fac = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var add = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();
                var facState = fac == null ? null : _context.States_UT.Where(x => x.State_id == add.StateId).FirstOrDefault();
                facStateName = facState.StateName;
                fsid = fac != null ? facState.State_id : 0;
            }
            List<MeetingVenue> venue = _helpersController.GetAllVenue(fsid, facStateName);
            ViewBag.Venue = new SelectList(venue, "Id", "Title");
            return View();
        }

        ////[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD, SUPERVISOR, HOOD, INSPECTOR")]
        public IActionResult AllSchedules(string id)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userName == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again later") });
            }

            int staff_id = 0;
            var staffID = generalClass.Decrypt(id);
            var sch = from s in _context.MeetingSchedules.AsEnumerable()
                      join mg in _context.ManagerScheduleMeetings.AsEnumerable() on s.Id equals mg.ScheduleId
                      join a in _context.applications.AsEnumerable() on s.ApplicationId equals a.id
                      join sf in _context.Staff.AsEnumerable() on s.StaffUserName equals sf.StaffEmail
                      join f in _context.Facilities.AsEnumerable() on a.FacilityId equals f.Id
                      join ad in _context.addresses.AsEnumerable() on f.AddressId equals ad.id
                      join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                      join phase in _context.Phases.AsEnumerable() on a.PhaseId equals phase.id
                      join c in _context.companies.AsEnumerable() on a.company_id equals c.id
                      select new MySchdule
                      {
                          ApplicationID = s.ApplicationId,
                          ScheduleID = s.Id,
                          FacilityID = f.Id,
                          FacilityName = f.Name + "(" + ad.address_1 + ", " + ad.city + ", " + sd.StateName + ")",
                          ContactName = f.ContactName,
                          ContactPhone = f.ContactNumber,
                          FieldOfficeID = (int)sf.FieldOfficeID,
                          AppOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == sf.FieldOfficeID).FirstOrDefault().OfficeName,
                          ScheduleDate = s.MeetingDate.ToString(),
                          ScheduleBy = sf.LastName + " " + sf.FirstName,
                          CompanyName = c.name,
                          CompanyID = c.id,
                          staffID = sf.StaffID,
                          CustomerRespons = s.Accepted == true ? 1 : 0,
                          CustomerResponse = s.Accepted == true ? "Accepted" : s.Accepted == false ? "Declined" : "Awaiting",
                          StaffComment = s.Message == null ? "No comment" : s.Message,
                          SupervisorComment = s.FinalComment != null ? s.FinalComment : "No comment",
                          CustomerComment = s.DeclineReason != null ? s.DeclineReason : "No comment",
                          //ScheduleType = s.Type,
                          Supervisor = s.ApprovedBy != null ? s.ApprovedBy : mg.UserId,
                          ScheduleLocation = s.Venue,
                          CreatedAt = s.Date,
                          UpdatedAt = s.UpdatedAt,
                          Accepted = s.Accepted,
                          AcceptanceDate = s.AcceptanceDate,
                          SupervisorApprovedd = s.Approved == true ? "Approved" : s.Approved == false ? "Declined" : "Awaiting",
                          SupervisorApproved = s.Approved == true ? 1 : s.Approved == false ? 2 : 0,
                          Approved = s.Approved,
                          WaiverRequest = s.WaiverRequest
                      };

            ViewData["ScheduleTitle"] = "All Schedules";
            ViewData["ScheduleStaffID"] = "";

            if (!string.IsNullOrWhiteSpace(id))
            {
                if (staffID == "Error")
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Schedule not found or not in correct format. Kindly contact support.") });
                }
                else
                {
                    staff_id = Convert.ToInt32(staffID);
                    sch = sch.Where(x => x.staffID == staff_id || x.Supervisor == userEmail);
                    ViewData["ScheduleTitle"] = "My Schedule";
                    ViewData["ScheduleStaffID"] = staffID;
                }
            }
            var fieldoffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID());

            if (!fieldoffice.FirstOrDefault().OfficeName.ToLower().Contains("head"))
            {
                sch = sch.Where(x => x.FieldOfficeID == _helpersController.getSessionOfficeID()); // get specific field/zonal office schedules
            }
            _helpersController.LogMessages("Displaying all schedules for " + ViewData["ScheduleTitle"], userEmail);
            var orderedSch = sch.OrderByDescending(x => x.CreatedAt).ToList();
            return View(orderedSch);
        }



        //All schedules
        ////[Authorize(Roles = "DIRECTOR, SUPPORT, IT ADMIN, SUPER ADMIN, HEAD GAS, AD, SUPERVISOR, HOOD, INSPECTOR")]
        public JsonResult ScheduleCalendar(string id)
        {
            var sch = from s in _context.MeetingSchedules.AsEnumerable()
                      join mg in _context.ManagerScheduleMeetings.AsEnumerable() on s.Id equals mg.ScheduleId
                      join a in _context.applications.AsEnumerable() on s.ApplicationId equals a.id
                      join sf in _context.Staff.AsEnumerable() on s.StaffUserName equals sf.StaffEmail
                      join f in _context.Facilities.AsEnumerable() on a.FacilityId equals f.Id
                      join c in _context.companies.AsEnumerable() on a.company_id equals c.id
                      join ad in _context.addresses.AsEnumerable() on f.AddressId equals ad.id
                      join st in _context.States_UT.AsEnumerable() on ad.StateId equals st.State_id
                      select new MySchdule
                      {
                          ScheduleID = s.Id,
                          FacilityID = f.Id,
                          FacilityName = f.Name,
                          FacilityAddress = ad.address_1 + ", " + ad.city + ". (" + st.StateName + ")",
                          FieldOfficeID = (int)sf.FieldOfficeID,
                          AppOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == sf.FieldOfficeID).FirstOrDefault().OfficeName,
                          ContactName = f.ContactName,
                          ContactPhone = f.ContactNumber,
                          ScheduleDate = s.MeetingDate.ToString(),
                          ScheduleBy = sf.LastName + " " + sf.FirstName,
                          ScheduleType = "Meeting/Inspection",
                          CompanyName = c.name,
                          CompanyID = c.id,
                          staffID = sf.StaffID,
                          CustomerRespons = s.Accepted == true ? 1 : 0,
                          CustomerResponse = s.Accepted == true ? "Accepted" : s.Accepted == false ? "Declined" : "Awaiting",
                          StaffComment = s.Message == null ? "No comment" : s.Message,
                          SupervisorComment = s.FinalComment != null ? s.FinalComment : "No comment",
                          CustomerComment = s.DeclineReason != null ? s.DeclineReason : "No comment",
                          SupervisorApproved = s.Approved == true ? 1 : s.Approved == false ? 2 : 0,
                          ScheduleLocation = s.Venue,
                          CreatedAt = s.Date,
                          UpdatedAt = s.UpdatedAt,
                          Supervisor = s.ApprovedBy != null ? s.ApprovedBy : mg.UserId,
                          // ApprovedBy = sf2.LastName + " " + sf2.FirstName
                      };
            var calendar = from s in sch
                           select new
                           {
                               id = s.ScheduleID,
                               title = s.ScheduleType.ToUpper(),
                               start = Convert.ToDateTime(s.ScheduleDate),
                               company = s.CompanyName.ToUpper(),
                               facility = s.FacilityName.ToUpper() + "(" + s.FacilityAddress.ToLower() + ")",
                               location = s.ScheduleLocation,
                               contact = s.ContactName + " - " + s.ContactPhone,
                               customerResponse = s.CustomerResponse,
                               customerComment = s.CustomerComment,
                               supervisor = s.Supervisor,
                               schedule = s.ScheduleBy,
                               staffComment = s.StaffComment,
                               FieldOfficeID = s.FieldOfficeID,
                               AppOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == s.FieldOfficeID).FirstOrDefault().OfficeName,
                               supervisorResponse = s.SupervisorApproved == 1 ? "Accepted" : s.SupervisorApproved == 2 ? "Rejected" : "Awaiting Response",
                               supervisorComment = s.SupervisorComment,
                               allDay = false,
                               staff_id = s.staffID,
                           };

            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            if (!string.IsNullOrEmpty(id))
            {
                calendar = calendar.Where(x => x.supervisor == userEmail || x.staff_id == Convert.ToInt32(id));
            }
            var fieldoffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID());

            if (!fieldoffice.FirstOrDefault().OfficeName.ToLower().Contains("head"))
            {
                calendar = calendar.Where(x => x.FieldOfficeID == _helpersController.getSessionOfficeID()); // get specific field/zonal office schedules
            }
            return Json(calendar.ToList());
        }

        [HttpPost]
        public IActionResult ScheduleMeeting(MeetingSchedules model, int ApplicationId, int appProcId)
        {

            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userName == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again later") });
            }

            //Check if schedule for application already exist
            var MSchedule = (from ms in _context.MeetingSchedules
                             where ms.StaffUserName.ToLower() == userEmail.ToLower()
&& ms.ApplicationId == ApplicationId && ms.ScheduleExpired != true
                             select ms).FirstOrDefault();

            if (MSchedule != null)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, this application still have a valid schedule created by you.") });
            }



            MeetingVenue venue = new MeetingVenue();
            Staff ub = new Staff();
            var currentProcess = _context.MyDesk.Where(a => a.DeskID == appProcId).FirstOrDefault();
            var staff = _context.Staff.Where(a => a.StaffID == userID).FirstOrDefault();
            var getSchedulerOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == staff.FieldOfficeID).FirstOrDefault();
            bool isHeadOffice = false;

            if (getSchedulerOffice != null && getSchedulerOffice.OfficeName.ToLower().Contains("head office"))
            {
                isHeadOffice = true;
            }


            var getStaffFD = (from u in _context.Staff
                              join fd in _context.FieldOffices on u.FieldOfficeID equals fd.FieldOffice_id
                              where u.StaffID == userID
                              select fd
                                              ).FirstOrDefault();
            var app = _context.applications.Where(a => a.id == ApplicationId).FirstOrDefault();
            var _appForm = _context.ApplicationForms.Where(a => a.ApplicationId == model.ApplicationId && a.Filled != true).OrderByDescending(b => b.Id).FirstOrDefault();
            var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
            var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();

            if (model.Venue != null)
            {
                venue = _helpersController.GetMeetingVenue(Convert.ToInt16(model.VenueId), "NMDPRA");
                model.Address = venue.Address;
                model.Venue = venue.Title;
            }
            else
            {

                venue = _helpersController.GetMeetingVenue((int)app.FacilityId, "client");
                model.Address = venue.Address;
                model.Venue = venue.Title;
            }

            model.StaffUserName = userEmail;
            model.SchedulerID = userID;


            try
            {
                Staff_UserBranchModel approver = new Staff_UserBranchModel();
                //var processes = _context.MyDesk.Where(a => a.ApplicationId == model.ApplicationId).ToList();

                #region Select the approver to approve the application Schedule
                if (userRole == GeneralClass.INSPECTOR && isHeadOffice == true)
                {
                    // meeting is scheduled by inspector in head office, so find supervisor
                    approver = (from stf in _context.Staff
                                join r in _context.UserRoles on stf.RoleID equals r.Role_id
                                where stf.DeleteStatus != true && stf.ActiveStatus != false
                              && r.RoleName.ToLower() == GeneralClass.ADPDJ && stf.FieldOfficeID == staff.FieldOfficeID
                                select new Staff_UserBranchModel
                                {
                                    Id = stf.StaffID,
                                    StaffId = stf.StaffID,
                                    FieldId = (int)stf.FieldOfficeID,
                                    RoleId = (int)stf.RoleID,
                                    RoleName = r.RoleName,
                                    Active = stf.ActiveStatus,
                                    DeletedStatus = stf.DeleteStatus,
                                    StaffEmail = stf.StaffEmail
                                }).FirstOrDefault();
                }
                else if (userRole == GeneralClass.INSPECTOR && isHeadOffice == false)
                {
                    // meeting is scheduled by inspector in field office, so find teamlead 

                    approver = (from stf in _context.Staff
                                join r in _context.UserRoles on stf.RoleID equals r.Role_id
                                where stf.DeleteStatus != true && stf.ActiveStatus != false
                                && r.RoleName.ToLower() == GeneralClass.TEAMLEAD && stf.FieldOfficeID == staff.FieldOfficeID
                                select new Staff_UserBranchModel
                                {
                                    Id = stf.StaffID,
                                    StaffId = stf.StaffID,
                                    FieldId = (int)stf.FieldOfficeID,
                                    RoleId = (int)stf.RoleID,
                                    RoleName = r.RoleName,
                                    Active = stf.ActiveStatus,
                                    DeletedStatus = stf.DeleteStatus,
                                    StaffEmail = stf.StaffEmail
                                }).FirstOrDefault();

                }

                #endregion


                if (userRole == GeneralClass.SUPERVISOR)
                {
                    model.ApprovedDate = DateTime.Now;
                    model.Approved = true;

                    model.Date = DateTime.Now;
                    model.MeetingDate = model.MeetingDate;
                    model.Message = model.Message;
                    _context.MeetingSchedules.Add(model);
                    _context.SaveChanges();
                    //get and update application form table
                    if (_appForm != null)
                    {
                        _appForm.StaffName = userEmail;
                        _context.SaveChanges();
                    }

                    //Keep application in holding
                    currentProcess.Holding = true;
                    _context.SaveChanges();


                    var manSMeetin = new ManagerScheduleMeetings();
                    manSMeetin.ScheduleId = model.Id;
                    manSMeetin.UserId = userEmail;
                    _context.ManagerScheduleMeetings.Add(manSMeetin);
                    _context.SaveChanges();
                    string[] stfE = { userEmail };
                    //Add Supervisor to attendees of this schedule
                    _helpersController.AddStaffToMeeting(model.Id, stfE, userEmail, "ipaddress");


                    #region Notify Client

                    string body = string.Empty;
                    var url = Url.Action("CompanySchedule", "Process", new { id = @generalClass.Encrypt(model.Id.ToString()) });

                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string file = up + @"\\Templates\" + "ScheduleMeeting.txt";
                    using (var sr = new StreamReader(file))
                    {
                        body = sr.ReadToEnd();
                    }

                    var subject = "Schedule For Meeting/Inspection For Application With Ref: " + app.reference;
                    var address = model.Address;

                    var msgBody = string.Format(body, subject, company.name, venue.Address, model.MeetingDate, model.Message, app.reference, facility.Name, DateTime.Now.Year, url); //, url, address);

                    var emailMsg = _helpersController.SaveMessage(model.ApplicationId, company.id, subject, msgBody, company.elps_id.ToString(), "Company");
                    var sendEmail = _helpersController.SendEmailMessage(company.CompanyEmail.ToString(), company.name, emailMsg, null);


                    #endregion


                    TempData["message"] = "You have successfully scheduled an inspection for application with ref: " + app.reference + ". A notification has been sent to company for acceptance.";
                    TempData["msgType"] = "success";
                    #endregion

                    string comment = userEmail + "(" + userRole + ")" + " scheduled a meeting/inspection with company";

                    _helpersController.SaveHistory(model.ApplicationId, userID, userEmail, GeneralClass.InspectionSchedule, comment);
                }
                else
                {
                    if (approver == null)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, we couldn't find staff to approve this schedule. Please contact support") });

                    }
                    model.ApprovedDate = null;
                    model.Date = DateTime.Now;
                    model.MeetingDate = model.MeetingDate;
                    model.Message = model.Message;
                    _context.MeetingSchedules.Add(model);
                    _context.SaveChanges();

                    //get and update application form table
                    if (_appForm != null)
                    {
                        _appForm.StaffName = userEmail;
                        _context.SaveChanges();
                    }

                    //Keep application in holding
                    currentProcess.Holding = true;
                    _context.SaveChanges();


                    var manSMeetin = new ManagerScheduleMeetings();
                    manSMeetin.ScheduleId = model.Id;
                    manSMeetin.UserId = approver.StaffEmail;
                    _context.ManagerScheduleMeetings.Add(manSMeetin);
                    _context.SaveChanges();
                    string[] stfE = { userEmail };
                    //Add Supervisor to attendees of this Schedule
                    _helpersController.AddStaffToMeeting(model.Id, stfE, userEmail, "ipaddress");
                    //Log History and send mail to approver
                    #region Send Email to Inspector

                    var getAppType = (from u in _context.Categories.AsEnumerable()
                                      join p in _context.Phases.AsEnumerable() on u.id equals p.category_id
                                      where u.id == app.category_id && p.id == app.PhaseId
                                      select p).FirstOrDefault();
                    var type = (app.type.ToLower() == "new" ? "New Depot License" : "Depot License Renewal").ToString() + "(" + getAppType.name + ")";
                    var msg = userEmail + '(' + userRole + ')' + " has scheduled a an inspection with " + company.name + " for " + model.MeetingDate + " at " + model.Venue
                                + " <p>Details of the Application is as follow:</p>";
                    msg += "<table class'table'>" +
                        $"<tr><td>Application Reference</td><td><a href='{ _restService._url}/Process/ViewApplication/" + generalClass.Encrypt(app.id.ToString()) + "'>" + app.reference + "</a></td></tr>" +
                        $"<tr><td>Application Company</td><td><a href='{_restService._url}/Companies/FullCompanyProfile/" + generalClass.Encrypt(app.company_id.ToString()) + "'>" + company.name + "</a></td></tr>" +
                        $"<tr><td>Facility</td><td><a href='{_restService._url}/Facility/ViewFacility/" + app.FacilityId + "'>" + facility.Name + "(" + facility.address_1 + ")</a></td></tr>" +
                        "<tr><td>Facility Address</td><td>" + facility.address_1 + "</td></tr>" +
                        "</table><br /><br /><p>";
                    var subject = "Inspection/Meeting Schedule for Application With Ref: " + app.reference;

                    var emailMsg = _helpersController.SaveMessage(app.id, approver.StaffId, subject, msg, staff.StaffElpsID, "Staff");
                    var sendEmail = _helpersController.SendEmailMessage2Staff(approver.StaffEmail, approver.StaffFullName, emailMsg, null);

                    TempData["message"] = "You have successfully scheduled an inspection for application with ref: " + app.reference + ". A notification has been sent to " + approver.RoleName + " for approval.";
                    TempData["msgType"] = "success";
                    #endregion

                    string comment = userEmail + "(" + userRole + ")" + " scheduled a meeting/inspection with Marketer";

                    _helpersController.SaveHistory(model.ApplicationId, userID, userEmail, GeneralClass.InspectionSchedule, comment);

                }
            }
            catch (Exception e)
            {
                TempData["message"] = "An error occured while processing your request. Please try again.";
                TempData["msgType"] = "fail";
                _helpersController.LogMessages(e.ToString());
            }


            return RedirectToAction("ViewApplication", new { id = generalClass.Encrypt(ApplicationId.ToString()) });
        }
        [HttpPost]
        public IActionResult EditScheduleMeeting(MeetingSchedules model, int ScheduleId, int ApplicationId, int appProcId)
        {

            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userName == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again later") });
            }
            MeetingVenue venue = new MeetingVenue();
            Staff ub = new Staff();
            var currentProcess = _context.MyDesk.Where(a => a.DeskID == appProcId).FirstOrDefault();
            var staff = _context.Staff.Where(a => a.StaffID == userID).FirstOrDefault();
            var getSchedulerOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == staff.FieldOfficeID).FirstOrDefault();
            bool isHeadOffice = false;

            if (getSchedulerOffice != null && getSchedulerOffice.OfficeName.ToLower().Contains("head office"))
            {
                isHeadOffice = true;
            }


            var getStaffFD = (from u in _context.Staff
                              join fd in _context.FieldOffices on u.FieldOfficeID equals fd.FieldOffice_id
                              where u.StaffID == userID
                              select fd
                                              ).FirstOrDefault();
            var app = _context.applications.Where(a => a.id == ApplicationId).FirstOrDefault();

            if (model.Venue != null)
            {
                venue = _helpersController.GetMeetingVenue(Convert.ToInt16(model.VenueId), "NMDPRA");
                model.Address = venue.Address;
                model.Venue = venue.Title;
            }
            else
            {

                venue = _helpersController.GetMeetingVenue((int)app.FacilityId, "client");
                model.Address = venue.Address;
                model.Venue = venue.Title;
            }

            model.StaffUserName = userEmail;

            try
            {
                var mSch = (from u in _context.MeetingSchedules
                            join mgr in _context.ManagerScheduleMeetings on u.Id equals mgr.ScheduleId
                            where u.Id == ScheduleId
                            select mgr).FirstOrDefault();

                var manager = mSch.UserId;
                if (manager == null)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, we couldn't find staff to approve this schedule. Please contact support") });

                }
                else
                {
                    var approver = (from stf in _context.Staff
                                    join r in _context.UserRoles on stf.RoleID equals r.Role_id
                                    join f in _context.FieldOffices on stf.FieldOfficeID equals f.FieldOffice_id
                                    where stf.DeleteStatus != true && stf.StaffEmail == manager
                                    select new Staff_UserBranchModel
                                    {

                                        RoleName = r.RoleName,
                                        FieldOffice = f.OfficeName,
                                        StaffEmail = stf.StaffEmail
                                    }).FirstOrDefault();

                    model.ApprovedDate = null;
                    model.MeetingDate = model.MeetingDate;
                    model.UpdatedAt = DateTime.Now;
                    var getSchedule = _context.MeetingSchedules.Where(x => x.Id == ScheduleId).FirstOrDefault();

                    if (getSchedule == null)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, this schedule does not exist.") });

                    }
                    DateTime prevInspDate = getSchedule.MeetingDate;
                    //update the schedule table
                    getSchedule.MeetingDate = model.MeetingDate;
                    getSchedule.AcceptanceDate = null;
                    getSchedule.Accepted = null;
                    getSchedule.ApprovedDate = null;
                    getSchedule.Approved = null;
                    getSchedule.UpdatedAt = DateTime.Now;
                    getSchedule.Venue = model.Venue;
                    getSchedule.ScheduleExpired = null;
                    getSchedule.Message = model.Message;
                    getSchedule.UpdatedAt = DateTime.Now;
                    getSchedule.DeclineReason = null;
                    //getSchedule.WaiverReason=null
                    _context.SaveChanges();

                    //get and update application form table
                    var _appForm = _context.ApplicationForms.Where(a => a.ApplicationId == model.ApplicationId && a.Filled != true).OrderByDescending(b => b.Id).FirstOrDefault();
                    if (_appForm != null)
                    {
                        _appForm.StaffName = userEmail;
                        _context.SaveChanges();
                    }

                    //Keep application in holding
                    currentProcess.Holding = true;
                    _context.SaveChanges();

                    string[] stfE = { userEmail };
                    //Add Supervisor to attendees of this Schedule
                    _helpersController.AddStaffToMeeting(model.Id, stfE, userEmail, "ipaddress");
                    //Log History and send mail to approver
                    #region Send Email to Inspector
                    var application = _context.applications.Where(a => a.id == ApplicationId).FirstOrDefault();
                    var company = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
                    var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();

                    var getAppType = (from u in _context.Categories.AsEnumerable()
                                      join p in _context.Phases.AsEnumerable() on u.id equals p.category_id
                                      where u.id == application.category_id && p.id == application.PhaseId
                                      select p).FirstOrDefault();
                    var type = (application.type.ToLower() == "new" ? "New Depot License" : "Depot License Renewal").ToString() + "(" + getAppType.name + ")";
                    var msg = userEmail + '(' + userRole + ')' + " has re-scheduled an inspection with " + company.name + " for " + model.MeetingDate + " at " + model.Venue
                                + " <p>Details of the Application is as follow:</p>";
                    msg += "<table class'table'>" +
                        $"<tr><td>Application Reference</td><td><a href='{ _restService._url}/Process/ViewApplication/" + generalClass.Encrypt(application.id.ToString()) + "'>" + application.reference + "</a></td></tr>" +
                        $"<tr><td>Application Company</td><td><a href='{_restService._url}/Company/Detail/" + application.company_id + "'>" + company.name + "</a></td></tr>" +
                        $"<tr><td>Facility</td><td><a href='{_restService._url}/Facility/ViewFacility/" + application.FacilityId + "'>" + facility.Name + "(" + facility.address_1 + ")</a></td></tr>" +
                        "<tr><td>Facility Address</td><td>" + facility.address_1 + "</td></tr>" +
                        "</table><br /><br /><p>";
                    var subject = "Inspection/Meeting Re-Scheduled for Application With Ref: " + application.reference;

                    var emailMsg = _helpersController.SaveMessage(application.id, approver.StaffId, subject, msg, staff.StaffElpsID, "Staff");
                    var sendEmail = _helpersController.SendEmailMessage2Staff(approver.StaffEmail, approver.StaffFullName, emailMsg, null);

                    TempData["message"] = "You have successfully re-scheduled an inspection for application with ref: " + application.reference + ". A notification has been sent to " + approver.RoleName + " for approval.";
                    TempData["msgType"] = "success";
                    #endregion

                    string comment = userEmail + "(" + userRole + ")" + " re-scheduled a meeting/inspection with Marketer. Previous scheduled date is: " + prevInspDate;

                    _helpersController.SaveHistory(model.ApplicationId, userID, userEmail, GeneralClass.InspectionSchedule, comment);

                }
            }
            catch (Exception e)
            {
                TempData["message"] = "An error occured while processing your request. Please try again.";
                TempData["msgType"] = "fail";
                _helpersController.LogMessages(e.ToString());
            }


            return RedirectToAction("ViewApplication", new { id = generalClass.Encrypt(ApplicationId.ToString()) });
        }

        #region Inspection Region

        public IActionResult ViewInspectionForm(string id)
        {
            int appid = generalClass.DecryptIDs(id.ToString().Trim());

            var af = _context.ApplicationForms.Where(a => a.ApplicationId == appid && a.Filled).FirstOrDefault();
            var app = _context.applications.Where(a => a.id == appid).FirstOrDefault();

            if (af != null)
            {
                switch (af.FormTitle)
                {
                    case "FacilityConditions":
                        return RedirectToAction("ViewFacilityCondition", new { id = appid });

                    case "TankLeakageTests":
                        return RedirectToAction("ViewTankLeakageTest", new { id = appid });
                    default:
                        int fid = 0;
                        int.TryParse(af.FormId, out fid);
                        var frm = _context.Forms.Where(a => a.Id == fid).FirstOrDefault();
                        if (frm == null)
                        {
                            //record form details for application
                            var phaseForms = _context.Forms.Where(a => a.Deleted != true).ToList();
                            var phaseForm = new Forms();
                            foreach (var f in phaseForms)
                            {
                                var otherPhases = f.OtherPhases != null ? f.OtherPhases : null;

                                if (f.PhaseId == app.PhaseId)
                                {
                                    phaseForm = f;
                                    break;

                                }
                                else if (otherPhases != null)
                                {

                                    if (otherPhases.Contains(app.PhaseId.ToString()) || f.PhaseId == app.PhaseId)
                                    {
                                        phaseForm = f;
                                        break;
                                    }
                                }
                            }
                            if (af != null)
                            {
                                af.FormId = phaseForm.Id.ToString();
                                frm = phaseForm;
                                _context.SaveChanges();
                            }
                        }

                        ViewBag.Form = frm.FriendlyName;
                        Guid gid = af.ValGroupId.GetValueOrDefault();
                        var fvs = _context.FieldValues.Where(a => a.GroupId == gid).ToList();
                        var application = _context.applications.Where(a => a.id == af.ApplicationId).FirstOrDefault();
                        var company = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
                        var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                        ViewBag.FormData = fvs;
                        ViewBag.CompanyName = company.name;
                        ViewBag.FacilityName = facility.Name;
                        ViewBag.AppFormId = id;
                        ViewBag.Recommend = af.Recommend;
                        ViewBag.Reasons = af.Reasons;
                        ViewBag.ReportFile1 = af.ExtraReport1;
                        ViewBag.ReportFile2 = af.ExtraReport2;
                        ViewBag.StaffName = af.StaffName;
                        ViewBag.AppForm = af;
                        ViewBag.FormTitle = af.FormTitle;
                        List<Fields> formFields = _context.Fields.Where(C => C.FormId == fid).ToList();
                        ViewBag.FormFields = formFields;


                        //get app or facility tanks 

                        var apTanks = (from t in _context.ApplicationTanks.AsEnumerable()
                                       join p in _context.Products on t.ProductId equals p.Id
                                       where t.ApplicationId == appid
                                       select new TankModel
                                       {
                                           Id = t.Id,
                                           Name = t.TankName,
                                           ProductName = p.Name,
                                           MaxCapacity = t.Capacity.ToString(),

                                       }).ToList();
                        var Tanks = (from t in _context.Tanks.AsEnumerable()
                                     join p in _context.Products on t.ProductId equals p.Id
                                     where t.FacilityId == facility.Id
                                     select new TankModel
                                     {
                                         Id = t.Id,
                                         Name = t.Name,
                                         ProductName = p.Name,
                                         Decommissioned = t.Decommissioned,
                                         HasATG = t.HasATG,
                                         Diameter = t.Diameter,
                                         Height = t.Height,
                                         MaxCapacity = t.MaxCapacity,
                                         ModifyType = t.ModifyType,

                                     }).ToList();
                        ViewBag.ApTanks = apTanks;
                        ViewBag.Tanks = Tanks;
                        var inspectionData = (from afm in _context.ApplicationForms.AsEnumerable()
                                              join ap in _context.applications.AsEnumerable() on afm.ApplicationId equals ap.id
                                              join comp in _context.companies.AsEnumerable() on ap.company_id equals comp.id
                                              join fac in _context.Facilities.AsEnumerable() on ap.FacilityId equals fac.Id
                                              join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                                              join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                                              join p in _context.Phases on ap.PhaseId equals p.id
                                              where afm.ApplicationId == appid
                                              select new InspectionDataModel
                                              {
                                                  AppTanks = apTanks.Count() > 0 ? apTanks : Tanks,
                                                  appForm = afm,
                                                  appID = ap.id,
                                                  Reference = ap.reference,
                                                  CategoryName = p.name,
                                                  PhaseName = p.name,
                                                  FacilityAddrss = ad.address_1,
                                                  Type = ap.type.ToUpper(),
                                                  Status = ap.status,
                                                  Date_Added = Convert.ToDateTime(ap.date_added),
                                                  DateSubmitted = ap.CreatedAt != null ? Convert.ToDateTime(ap.CreatedAt) : Convert.ToDateTime(ap.date_added),
                                                  CompanyDetails = comp.name + " (" + comp.Address + ") ",
                                                  CompanyName = comp.name,
                                                  FacilityDetails = fac.Name + " ( " + ad.address_1 + ")",
                                                  FacilityName = fac.Name,
                                                  StaffName = afm.StaffName
                                              }).ToList();
                        if (inspectionData.Count() > 0)
                        {
                            //get inspection field
                            inspectionData.FirstOrDefault().Fields = formFields;

                            //check if schedule was created
                            var schedule = _context.MeetingSchedules.Where(x => x.ApplicationId == inspectionData.FirstOrDefault().appID).FirstOrDefault();
                            if (schedule != null)
                            {
                                inspectionData.FirstOrDefault().FormCreatedAt = Convert.ToDateTime(schedule.MeetingDate);
                                inspectionData.FirstOrDefault().StaffName = schedule.StaffUserName;
                            }
                            else
                            {
                                inspectionData.FirstOrDefault().FormCreatedAt = Convert.ToDateTime(inspectionData.FirstOrDefault().appForm.DateModified);
                            }
                            inspectionData.FirstOrDefault().appForm.ExtraReport1 = af.ExtraReport1;
                            inspectionData.FirstOrDefault().appForm.ExtraReport2 = af.ExtraReport2;
                            var StaffCommentField = inspectionData.FirstOrDefault().Fields.Where(x => x.Label.ToLower().Contains("comment")).FirstOrDefault();
                            if (StaffCommentField != null)
                            {
                                ViewBag.StaffComment = fvs.Where(x => x.FieldId == StaffCommentField.Id).FirstOrDefault()?.Value;
                            }
                            else
                            {
                                ViewBag.StaffComment = inspectionData.FirstOrDefault().appForm.Reasons;
                            }

                            var staff = _context.Staff.Where(a => inspectionData.FirstOrDefault().StaffName.ToLower().Contains(a.StaffEmail.ToLower())).FirstOrDefault();
                            if (staff != null)
                            {
                                ViewBag.StaffOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == staff.FieldOfficeID).FirstOrDefault().OfficeName;
                                ViewBag.StaffRole = _context.UserRoles.Where(x => x.Role_id == staff.RoleID).FirstOrDefault().RoleName;
                            }
                            ViewData["AppRefNo"] = inspectionData.FirstOrDefault().Reference;
                            return View(inspectionData);

                        }
                        else
                        {
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, application form does not exist.") });

                        }

                }
            }
            ViewBag.Error = "Item does not Exist";
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, application form does not exist.") });

        }

        public IActionResult FillInspectionForm(string id, string option)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int loginID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionLogin));
            int appId = generalClass.DecryptIDs(id);
            int appProcId = generalClass.DecryptIDs(option);

            if (userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Kindly log in again.") });
            }
            ViewBag.CurrentStaff = userEmail;

            #region

            var application = _context.applications.Where(a => a.id == appId).FirstOrDefault();
            var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
            var company = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
            //Check if schedule for application already exist
            var MSchedule = (from ms in _context.MeetingSchedules
                             where ms.StaffUserName.ToLower() == userEmail.ToLower()
                             && ms.ApplicationId == appId
                             select ms).OrderByDescending(x => x.Id).FirstOrDefault();

            var appForm = _context.ApplicationForms.Where(a => a.ApplicationId == appId).FirstOrDefault();
            if (appForm == null)
            {

                var currentUser = MSchedule != null ? MSchedule.StaffUserName : userEmail;
                if (currentUser != null)
                {
                    //record appliction form newly
                    var ph = _context.Phases.Where(a => a.id == application.PhaseId).FirstOrDefault();
                    var phaseForms = _context.Forms.Where(a => a.Deleted != true).ToList();
                    var phaseForm = new Forms();

                    foreach (var f in phaseForms)
                    {
                        var otherPhases = f.OtherPhases != null ? f.OtherPhases : null;

                        if (f.PhaseId == ph.id)
                        {
                            phaseForm = f;
                            break;

                        }
                        else if (otherPhases != null)
                        {

                            if (otherPhases.Contains(ph.id.ToString()) || f.PhaseId == ph.id)
                            {
                                phaseForm = f;
                                break;
                            }
                        }
                    }
                    if (phaseForm != null)
                    {
                        var af = new ApplicationForms();
                        af.Confirmed = false;
                        af.ApplicationId = appId;
                        af.Date = DateTime.Now;
                        af.Filled = false;
                        af.DateModified = DateTime.Now;
                        af.FormId = phaseForm.Id.ToString();
                        af.DepartmentId = 00;
                        af.StaffName = currentUser;
                        af.FormTitle = phaseForm.FriendlyName;
                        _context.ApplicationForms.Add(af);
                        _context.SaveChanges();

                        appForm = af;
                    }

                }
                appForm.StaffName = userEmail;
                _context.SaveChanges();
            }

            appForm = _context.ApplicationForms.Where(a => a.ApplicationId == appId).FirstOrDefault();
            if (appForm != null)
            {

                var frm = _context.Forms.Where(a => a.Id.ToString() == appForm.FormId).FirstOrDefault();

                if (frm == null)
                {
                    var phaseForms = _context.Forms.Where(a => a.Deleted != true).ToList();
                    var phaseForm = new Forms();
                    foreach (var f in phaseForms)
                    {
                        var otherPhases = f.OtherPhases != null ? f.OtherPhases : null;
                        if (otherPhases != null)
                        {

                            if (otherPhases.Contains(application.PhaseId.ToString()) || f.PhaseId == application.PhaseId)
                            {
                                frm = f;
                                appForm.FormId = f.Id.ToString();
                                break;
                            }
                        }
                        else if (f.PhaseId == application.PhaseId)
                        {
                            frm = f;
                            appForm.FormId = f.Id.ToString();
                            break;

                        }
                    }
                }

                if (appForm.Filled != true) // Enable current user to be able to fill and save form inspection if it has not been filled
                {
                    appForm.StaffName = userEmail;
                    _context.SaveChanges();
                }


                ViewBag.Form = frm.FriendlyName;
                Guid gid = appForm.ValGroupId.GetValueOrDefault();

                var fv = (from fd in _context.FieldValues.AsEnumerable()
                          join ap in _context.ApplicationForms.AsEnumerable() on fd.GroupId equals ap.ValGroupId
                          where ap.ApplicationId == appId && ap.FormId.ToString() == appForm.FormId
                          select fd).ToList();

                ViewBag.CompanyName = company.name;
                ViewBag.FacilityName = facility.Name;
                ViewBag.FormId = appForm.FormId;
                ViewBag.AppFormId = appForm.Id;
                ViewBag.Recommend = appForm.Recommend;
                ViewBag.Recommend = appForm.Recommend;
                ViewBag.ReportFile1 = appForm.ExtraReport1;
                ViewBag.ReportFile2 = appForm.ExtraReport2;
                ViewBag.Reasons = appForm.Reasons;
                ViewBag.AppProcId = appProcId;
                ViewBag.Scheduler = (appForm.StaffName.Contains(userEmail) || appForm.StaffName.Contains(userName) )? true : false;
                List<FieldValues> formData = fv;

                ViewBag.FormData = formData;

                List<Fields> formField = _context.Fields.Where(C => C.FormId.ToString() == appForm.FormId).ToList();
                foreach (var item in formField)
                {
                    var formV = fv.FirstOrDefault(a => a.FieldId == item.Id);
                    //Assign Value if it exist.
                    if (formV != null)
                    {
                        if (item.DataType.ToLower() != "options")
                            item.OptionValue = formV.Value;
                        else
                            item.OptionValue = formV.Value + "," + item.OptionValue;

                    }
                    else
                    {
                        if (item.Label.ToLower() == "companyid".ToLower())
                        {
                            item.OptionValue = company.id.ToString();
                        }
                        if (item.Label.ToLower() == "ApplicationId".ToLower())
                        {
                            item.OptionValue = appForm.ApplicationId.ToString();
                        }
                    }
                }
                var appPhase = _context.Phases.Where(p => p.id == application.PhaseId).FirstOrDefault();
                ViewBag.FormTitle = appPhase.ShortName;
                return View(formField);
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, there is no inspection form found for this application.") });

            }
            #endregion
        }

        [HttpPost]
        public IActionResult FillForm(IFormCollection formCollection, List<IFormFile> filess, string Id, string companyId, string applicationId, int? rowsToCreate, string ExtraReport1, string ExtraReport2)
        {
            AlertBox alert = null;
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            var appFormId = formCollection["appFormId"];
            var appProcId = formCollection["appProcId"];
            try
            {

                var appForm = _context.ApplicationForms.Where(a => a.Id == Convert.ToInt16(appFormId)).FirstOrDefault();
                if (appForm != null)
                {
                    // Id = Id.ToLower();
                    List<Fields> formField = _context.Fields.Where(C => C.FormId.ToString() == appForm.FormId).ToList();

                    Guid groupId = Guid.Empty;
                    if (appForm.ValGroupId != null && appForm.ValGroupId != Guid.Empty)
                    {
                        groupId = appForm.ValGroupId.Value;
                    }
                    else
                    {
                        groupId = Guid.NewGuid();
                    }
                    List<IFormFile> files = new List<IFormFile>();

                    for (int i = 0; i < Request.Form.Files.Count; i++)
                    {
                        files.Add(Request.Form.Files[i]);
                    }

                    var listOfFldValue = new List<FieldValues>();
                    int id = 0;
                    foreach (var item in formField)
                    {
                        id++;
                        if (item.DataType.ToLower() == "table" && !string.IsNullOrEmpty(item.OptionValue))
                        {
                            string value = "";
                            List<string> options = item.OptionValue.Split('|').ToList();
                            var _cols = options[0].Trim();
                            var _rows = options[1].Trim();
                            var cols = _cols.Split('=')[1].Split(';');
                            var rows = _rows.Split('=')[1].Split(';');
                            if (rows[0].StartsWith("-1"))
                            {
                                // Dynamic
                                for (int i = 0; i < rowsToCreate.GetValueOrDefault(); i++)
                                {
                                    for (int j = 0; j < cols.Count(); j++) //Starts from 0
                                    {
                                        var label = item.Id + "_R" + i + "_C" + j;
                                        string formParameter = formCollection[label];
                                        var v = label + ":" + formParameter;
                                        var vv = string.IsNullOrEmpty(value) ? v : "\\" + v;
                                        value += vv;
                                    }
                                    value += (i + 1) == rowsToCreate.GetValueOrDefault() ? "" : "|";
                                }
                            }
                            else
                            {
                                // Normal
                                for (int i = 0; i < rows.Count(); i++)
                                {
                                    for (int j = 1; j < cols.Count(); j++)  //Starts from 1
                                    {
                                        var label = item.Id + "_R" + i + "_C" + j;
                                        string formParameter = formCollection[label];
                                        var v = label + ":" + formParameter;
                                        var vv = string.IsNullOrEmpty(value) ? v : "\\" + v;
                                        value += vv;
                                    }
                                    value += (i + 1) == rows.Count() ? "" : "|";
                                }
                            }
                            var fieldV = _context.FieldValues.Where(a => a.FieldId == item.Id && a.GroupId == groupId).FirstOrDefault();
                            if (fieldV == null)
                            {
                                fieldV = new FieldValues()
                                {
                                    GroupId = groupId,
                                    FieldId = item.Id,
                                    Value = value,
                                    ApplicationId = Convert.ToInt16(appForm.ApplicationId),
                                    FormId = Convert.ToInt16(appForm.FormId),
                                };
                                listOfFldValue.Add(fieldV);
                                _context.FieldValues.Add(fieldV);
                            }
                            else
                            {
                                fieldV.Value = value;
                                listOfFldValue.Add(fieldV);
                                _context.SaveChanges();
                            }
                        }
                        else if (item.DataType.ToLower() == "image")
                        {
                            //List<string> options = item.OptionValue.Split(';').ToList();
                            var file = files.Where(x => x.FileName.Contains(item.Id.ToString())).FirstOrDefault();
                            var value = string.Empty;

                            var pre = "img_" + item.Id + "_";
                            if (file == null)
                            {
                                string formParameter = formCollection[pre];
                                value += string.IsNullOrEmpty(formParameter) ? "" : formParameter + ";";
                            }
                            else if (file != null && file.Length > 0)
                            {
                                var source = _helpersController.UploadFormImages(file, pre, userEmail, pre.ToString());
                                value += source + ";";
                            }
                            //}

                            value = value.EndsWith(";") ? value.Substring(0, value.Length - 1) : value;
                            var fieldV = _context.FieldValues.Where(a => a.FieldId == item.Id && a.GroupId == groupId).FirstOrDefault();
                            if (fieldV == null)
                            {
                                fieldV = new FieldValues()
                                {
                                    GroupId = groupId,
                                    FieldId = item.Id,
                                    ApplicationId = Convert.ToInt16(appForm.ApplicationId),
                                    FormId = Convert.ToInt16(appForm.FormId),
                                    Value = value
                                };
                                listOfFldValue.Add(fieldV);
                                _context.FieldValues.Add(fieldV);
                            }
                            else
                            {
                                fieldV.Value = string.IsNullOrEmpty(value) ? fieldV.Value : value;
                                listOfFldValue.Add(fieldV);
                                _context.SaveChanges();
                            }

                        }
                        else
                        {
                            string formParameter = formCollection[item.Label];
                            if (formParameter != null)
                            {
                                var fieldV = _context.FieldValues.Where(a => a.FieldId == item.Id && a.GroupId == groupId).FirstOrDefault();

                                if (fieldV == null)
                                {
                                    fieldV = new FieldValues()
                                    {
                                        GroupId = groupId,
                                        FieldId = item.Id,
                                        Value = formParameter,
                                        ApplicationId = Convert.ToInt16(appForm.ApplicationId),
                                        FormId = Convert.ToInt16(appForm.FormId),
                                    };
                                    listOfFldValue.Add(fieldV);
                                    _context.FieldValues.Add(fieldV);
                                }
                                else
                                {
                                    fieldV.Value = formParameter;
                                    fieldV.ApplicationId = Convert.ToInt16(appForm.ApplicationId);
                                    fieldV.FormId = Convert.ToInt16(appForm.FormId);
                                    listOfFldValue.Add(fieldV);
                                    _context.SaveChanges();
                                }

                            }
                        }

                    }

                    _context.SaveChanges();

                    #region Finishing
                    var reason = formCollection["reason"];
                    var recommend = formCollection["recommend"].ToString().ToLower() == "yes" ? true : false; // : null;;
                    appForm.Filled = true;
                    appForm.DateModified = DateTime.Now;
                    appForm.Reasons = reason;
                    appForm.Recommend = recommend;
                    appForm.ExtraReport1 = ExtraReport1;
                    appForm.ExtraReport2 = ExtraReport2;
                    appForm.ValGroupId = groupId;
                    //get the staff that filled the form
                    var stf = _context.Staff.Where(a => a.StaffEmail.ToLower() == userEmail.ToLower()).FirstOrDefault();
                    if (stf != null)
                    {
                        appForm.StaffName = userEmail;
                    }
                    _context.SaveChanges();

                    string comment = "Application  inspection form filled & submitted";
                    _helpersController.SaveHistory(appForm.ApplicationId, userID, userEmail, GeneralClass.Move, comment);

                    TempData["msgType"] = "success";
                    TempData["message"] = "Application inspection form filled and submitted successfully.";

                    return RedirectToAction("ViewApplication", new { Id = generalClass.Encrypt(appForm.ApplicationId.ToString()), procid = appProcId });
                    #endregion
                }
                TempData["msgType"] = "fail";
                TempData["message"] = "An error occured while submitting inspection form for this application.";

            }
            catch (Exception x)
            {

                _helpersController.LogMessages($"Error from Fill Form:: {x.Message.ToString()}");

                TempData["msgType"] = "fail";
                TempData["message"] = "An error occured while submitting inspection form for this application. See why: " + x.Message;

            }
            return RedirectToAction("MyDesk");
        }

        public IActionResult MyPendingSchedules()
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var fieldoffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID()).FirstOrDefault();

            var staff = _context.Staff.Where(a => a.StaffEmail == userEmail.Trim()).FirstOrDefault();

            if (staff == null || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please sign in again.") });
            }

            #region Field/Zonal Office Schedule Approval
            if (userRole != GeneralClass.INSPECTOR)
            {
                var managerWaivers2 = _context.MeetingSchedules.Where(a => a.StaffUserName == userEmail).ToList();
                var managerWaivers = (from u in _context.MeetingSchedules
                                      join mgr in _context.ManagerScheduleMeetings on u.Id equals mgr.ScheduleId
                                      join app in _context.applications on u.ApplicationId equals app.id
                                      join stf in _context.Staff on u.SchedulerID equals stf.StaffID
                                      join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                      join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                                      join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                                      join phase in _context.Phases on app.PhaseId equals phase.id
                                      where stf.FieldOfficeID == fieldoffice.FieldOffice_id && u.ScheduleExpired != true
                                      select new MeetingSchedulesModel
                                      {
                                          Id = u.Id,
                                          FacilityName = fac.Name + "(" + ad.address_1 + ", " + ad.city + ", " + sd.State_id,
                                          Reference = app.reference,
                                          ScheduleExpired = u.ScheduleExpired,
                                          Date = u.Date,
                                          UpdatedAt = u.UpdatedAt == null ? "No Update" : u.UpdatedAt.ToString(),
                                          CompanyId = app.company_id,
                                          CompanyName = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault().name,
                                          CategoryName = phase.name,
                                          ApplicationId = app.id,
                                          StaffUserName = u.StaffUserName,
                                          MeetingDate = u.MeetingDate,
                                          Approved = u.Approved,
                                          WaiverRequest = u.WaiverRequest
                                      }).ToList();
                List<int> ids = new List<int>();
                foreach (var item in managerWaivers)
                {
                    if (!ids.Contains(item.ApplicationId))
                        ids.Add(item.ApplicationId);
                }

                List<MeetingSchedulesModel> toReturn = new List<MeetingSchedulesModel>();
                foreach (var _id in ids)
                {
                    var range = managerWaivers.FindAll(a => a.ApplicationId == _id);
                    var pick = range[range.Count() - 1];
                    if (pick.Approved == null)
                        toReturn.Add(pick);
                }

                ViewBag.managerSchmeeting = toReturn;
            }
            else
            {
                ViewBag.User = "Inspector";

                var managerWaivers = (from u in _context.MeetingSchedules
                                      join mgr in _context.ManagerScheduleMeetings on u.Id equals mgr.ScheduleId
                                      join app in _context.applications on u.ApplicationId equals app.id
                                      join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                      join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                                      join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                                      join phase in _context.Phases on app.PhaseId equals phase.id
                                      where u.StaffUserName == userEmail && u.ScheduleExpired != true
                                      select new MeetingSchedulesModel
                                      {
                                          Id = u.Id,
                                          FacilityName = fac.Name + "(" + ad.address_1 + ", " + ad.city + ", " + sd.State_id,
                                          Reference = app.reference,
                                          ScheduleExpired = u.ScheduleExpired,
                                          Date = u.Date,
                                          UpdatedAt = u.UpdatedAt == null ? "No Update" : u.UpdatedAt.ToString(),
                                          CompanyId = app.company_id,
                                          CompanyName = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault().name,
                                          CategoryName = phase.name,
                                          ApplicationId = app.id,
                                          StaffUserName = u.StaffUserName,
                                          MeetingDate = u.MeetingDate,
                                          Approved = u.Approved,
                                          Accepted = u.Accepted,
                                          WaiverRequest = u.WaiverRequest
                                      }).ToList();
                List<int> ids = new List<int>();
                foreach (var item in managerWaivers)
                {
                    if (!ids.Contains(item.ApplicationId))
                        ids.Add(item.ApplicationId);
                }

                List<MeetingSchedulesModel> toReturn = new List<MeetingSchedulesModel>();
                foreach (var _id in ids)
                {
                    var range = managerWaivers.FindAll(a => a.ApplicationId == _id);
                    var pick = range[range.Count() - 1];
                    toReturn.Add(pick);
                }

                ViewBag.managerSchmeeting = toReturn;
            }
            #endregion
            return View();
        }

        public IActionResult CalendarView(DateTime fromWhere)
        {
            ViewBag.CurrentMonth = fromWhere;
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var staff = _context.Staff.Where(a => a.StaffEmail == userEmail.Trim());

            if (staff == null || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please sign in again.") });
            }


            var myFieldOffice = staff.ToList();
            var userInBranch = new List<Staff>();
            foreach (var item in myFieldOffice)
            {
                var bb = _context.Staff.Where(C => C.FieldOfficeID == item.FieldOfficeID && C.LocationID == item.LocationID
                && C.DeleteStatus != true && C.ActiveStatus != false).ToList();
                userInBranch.AddRange(bb);
            }
            var schedules = _context.MeetingSchedules.Where(a => a.MeetingDate.Month == fromWhere.Month).ToList();
            var returnedSchedules = new List<MeetingSchedules>();
            foreach (var branch in userInBranch)
            {
                var found = schedules.Where(a => a.StaffUserName.ToLower() == branch.StaffEmail.ToLower()).ToList();
                foreach (var f in found)
                {
                    if (!returnedSchedules.Contains(f))
                        returnedSchedules.Add(f);
                }

            }

            return View(returnedSchedules);
        }

        public IActionResult InspectionCalendarView(DateTime fromWhere)
        {
            ViewBag.CurrentMonth = fromWhere;
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var myFieldOffice = _context.Staff.Where(a => a.StaffEmail == userEmail.Trim());

            if (myFieldOffice == null || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please sign in again.") });
            }


            var userInBranch = new List<Staff>();
            foreach (var item in myFieldOffice)
            {
                var bb = _context.Staff.Where(C => C.FieldOfficeID == item.FieldOfficeID && C.LocationID == item.LocationID).ToList();
                userInBranch.AddRange(bb);
            }
            var schedules = _context.InspectionSchedules.Where(a => a.MeetingDate.Month == fromWhere.Month).ToList();


            var returnedSchedules = new List<InspectionSchedules>();
            foreach (var branch in userInBranch)
            {
                var found = schedules.Where(a => a.StaffUserName.ToLower() == branch.StaffEmail.ToLower()).ToList();
                foreach (var f in found)
                {
                    if (!returnedSchedules.Contains(f))
                        returnedSchedules.Add(f);
                }

            }

            return View(returnedSchedules);
        }

        [HttpPost]
        public IActionResult ManagerSchedule(MeetingSchedules model, string actionA, string disapproveReason)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please sign in again.") });
            }

            model = _context.MeetingSchedules.Where(m => m.Id == model.Id).FirstOrDefault();
            string action = "";
            if (actionA == "Approve")
            {
                model.Approved = true;
                action = "approve";
            }
            else
            {
                model.Approved = false;
                action = "declined";

            }
            model.ApprovedBy = userEmail;
            model.ApprovedDate = DateTime.Now;
            var appl = _context.applications.Where(a => a.id == model.ApplicationId).FirstOrDefault();
            var company = _context.companies.Where(x => x.id == appl.company_id).FirstOrDefault();


            try
            {
                string historyComment = string.Empty;
                var history = new application_desk_histories();

                if (model.Approved.GetValueOrDefault())
                {
                    #region Notify Client

                    var msg = string.Format("Dear {0} {1}, You have been scheduled for a Meeting/Inspection at {2} {1} Reason: {3} {1} Date/Time: {4} {1}Address: {5}",
                    appl.company_id, Environment.NewLine, model.Venue, model.Message, model.Date, model.Address);

                    string body = string.Empty;
                    var url = Url.Action("CompanySchedule", "Process", new { id = generalClass.Encrypt(model.Id.ToString()) });

                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    string file = up + @"\\Templates\" + "ScheduleMeeting.txt";
                    using (var sr = new StreamReader(file))
                    {

                        body = sr.ReadToEnd();
                    }
                    var facility = _context.Facilities.Where(x => x.Id == appl.FacilityId).FirstOrDefault();

                    var subject = "Meeting/Inspection Schedule For Application With Ref: " + appl.reference;
                    var address = model.Address;
                    var venue = model.Venue.ToLower() == "your office" ? "Your Facility (" + model.Address + ")" : model.Venue + " (" + model.Address + ")";

                    var msgBody = string.Format(body, subject, company.name, venue, model.MeetingDate, model.Message, appl.reference, facility.Name, DateTime.Now.Year, url); //, url, address);

                    var emailMsg = _helpersController.SaveMessage(model.ApplicationId, company.id, subject, msgBody, company.elps_id.ToString(), "Company");
                    var sendEmail = _helpersController.SendEmailMessage(company.CompanyEmail.ToString(), company.name, emailMsg, null);


                    #endregion

                    #region Notify Opscon and FD Inspectors

                    model.FinalComment = disapproveReason;
                    TempData["message"] = "Meeting/Inspection schedule has been approved for " + company.name + "'s application.";
                    TempData["msgType"] = "pass";
                    historyComment = $"Meeting/Inspection schedule has been approved by {userEmail} ";

                }
                else
                {
                    model.FinalComment = disapproveReason;
                    TempData["message"] = "Meeting/Inspection schedule has been disapproved for " + company.name + "'s application.";
                    TempData["msgType"] = "warn";
                    historyComment = $"Meeting/Inspection schedule has been declined by " + userEmail + " with this reason: " + disapproveReason;
                }

                _context.SaveChanges();
                _helpersController.SaveHistory(model.ApplicationId, userID, userEmail, "Schedule", historyComment);
                var appProc = _context.MyDesk.Where(a => a.StaffID == appl.current_desk && a.AppId == appl.id).FirstOrDefault();
                if (appProc != null)
                {
                    appProc.Holding = false;
                    _context.SaveChanges();
                }//remove application from holding

                #region Remove Reminder since it has been worked upon
                var manrem = _context.ManagerReminders.Where(a => a.ScheduleId == model.Id).FirstOrDefault();
                if (manrem != null)
                {
                    _context.ManagerReminders.Remove(manrem);
                    _context.SaveChanges();
                }
                #endregion
                //send schedule approver action to scheduler
                var stf = _context.Staff.Where(st => st.StaffEmail == model.StaffUserName).FirstOrDefault();
                string subject2 = action == "approve" ? "Approval" : "Rejection" + " for schedule with " + company.name + " for application with reference: " + appl.reference;
                string content2 = userEmail + " has " + action + " your sceduled meeting/inspection for " + model.MeetingDate;
                var emailMsg2 = _helpersController.SaveMessage(appl.id, stf.StaffID, subject2, content2, stf.StaffElpsID, "Staff");
                var sendEmail2 = _helpersController.SendEmailMessage2Staff(stf.StaffEmail, stf.FirstName, emailMsg2, null);


                return RedirectToAction("MyPendingSchedules", "Process");

            }
            catch (Exception ex)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ex.Message) });
                throw;
            }

        }
        public IActionResult ManagerSchedule(int id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please sign in again.") });
            }

            var ms = _context.MeetingSchedules.Where(m => m.Id == id).FirstOrDefault();

            if (ms != null)
            {

                var mSch = (from u in _context.MeetingSchedules
                            join mgr in _context.ManagerScheduleMeetings on u.Id equals mgr.ScheduleId
                            join app in _context.applications on u.ApplicationId equals app.id
                            where u.Id == id
                            select mgr).FirstOrDefault();

                var appl = _context.applications.Where(a => a.id == ms.ApplicationId).FirstOrDefault();
                var facility = _context.Facilities.Where(x => x.Id == appl.FacilityId).FirstOrDefault();
                var company = _context.companies.Where(x => x.id == appl.company_id).FirstOrDefault();

                ViewBag.Facility = facility.Name;
                ViewBag.Company = company.name;

                //if (mSch.UserId != userEmail)
                //{
                //    ViewBag.Error = "Sorry you cannot act on this schedule";
                //}

                return View(ms);

            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, an error has occured. Please sign in again.") });

        }

        public IActionResult CompanySchedule(string id)
        {
            bool remove = false;
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int ID = generalClass.DecryptIDs(id);
            if (userID == 0 && ID != 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please sign in again.") });
            }
            var mSchedule = _context.MeetingSchedules.Where(a => a.Id == ID).FirstOrDefault();
            if (mSchedule != null)
            {
                var appl = _context.applications.Where(a => a.id == mSchedule.ApplicationId && a.company_id == userID).FirstOrDefault();
                if (appl == null)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, this application is not for you..") });
                }
                ViewBag.Referene = appl.reference;
                var tdate = DateTime.Now;
                if (mSchedule.ApprovedDate.GetValueOrDefault().AddDays(3) < tdate)
                {
                    remove = true;
                    ViewBag.expired = "Approved time for Confirmation exceeded, Confirmation was supposed to happen on or before " + mSchedule.ApprovedDate.GetValueOrDefault().AddDays(3);

                    if (mSchedule.ScheduleExpired == null)
                    {
                        var ms = _context.MeetingSchedules.Where(a => a.Id == mSchedule.Id).FirstOrDefault();

                        ms.Message = string.Format("{0}{1}{1} {2}", ms.Message, Environment.NewLine, ViewBag.expired);
                        ms.ScheduleExpired = true;
                        _context.SaveChanges();
                    }
                }
                else if (mSchedule.Accepted != null && mSchedule.Accepted.GetValueOrDefault())
                {
                    remove = true;
                    //Schedule not expired
                    ViewBag.accepted = true;

                }
                else if (mSchedule.Accepted != null && !mSchedule.Accepted.GetValueOrDefault())
                {
                    //Schedule not expired
                    ViewBag.accepted = false;

                }
                else if (mSchedule.MeetingDate < DateTime.Now)
                {
                    remove = true;
                    ViewBag.MeetingExpired = mSchedule.MeetingDate.ToString();
                }
                else
                {
                }
                if (remove)
                {
                    #region Remove Reminder since it has been worked upon

                    var manrem = _context.ManagerReminders.Where(a => a.ScheduleId == mSchedule.Id).FirstOrDefault();
                    if (manrem != null)
                    {
                        _context.ManagerReminders.Remove(manrem);
                        _context.SaveChanges();
                    }
                    #endregion #endregion
                }
                return View(mSchedule);
            }

            return View("Error");
        }

        [HttpPost]
        public IActionResult CompanySchedule(int id, string message, bool Accepted)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userEmail == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, application reference not passed. Please try again later") });
            }

            var ms = _context.MeetingSchedules.Where(a => a.Id == id).FirstOrDefault();
            if (ms == null)
            {
                ViewBag.errorMessage = "Could not find the Schedule/Meeting you are looking for";
                return View("apperror");
            }

            var appl = _context.applications.Where(a => a.id == ms.ApplicationId).FirstOrDefault();
            var facility = _context.Facilities.Where(x => x.Id == appl.FacilityId).FirstOrDefault();
            var company = _context.companies.Where(x => x.id == appl.company_id).FirstOrDefault();

            if (appl == null)
            {
                ViewBag.errorMessage = "The Schedule/Meeting is not attached to any application";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ViewBag.errorMessage) });
            }


            if (Accepted != true)
            {
                ms.AcceptanceDate = DateTime.Now;
                ms.Accepted = false;
                ms.DeclineReason = message;

            }
            else
            {
                ms.AcceptanceDate = DateTime.Now;
                ms.Accepted = true;
                if (message != null)
                    ms.DeclineReason = message;
            }


            _context.SaveChanges();
            string comment = (ms.Accepted == true ? "Client Accepted the Meeting Schedule" : "Client Declined the Meeting Schedule with reason: " + ms.DeclineReason);

            _helpersController.SaveHistory(ms.ApplicationId, userID, userEmail, "Schedule Confirmation", comment);


            #region Send Notification to all Parties Involved
            string body = "";
            string subject = "";
            string venue = "";
            string msgBody = "";
            //var jas = _context.JointApplications.Where(a => a.applicationId == ms.ApplicationId).ToList();
            var jas = _context.JointAccounts.Where(a => a.ApplicationId == ms.ApplicationId && a.OperationsCompleted != true).ToList();

            if (jas != null && jas.Count > 0)
            {

                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var file = Path.Combine(up, "Templates/OP_ScheduleMeeting.txt");

                using (var sr = new StreamReader(file))
                {

                    body = sr.ReadToEnd();
                }

                subject = "Schedule for a Joint Meeting/Inspection with Depot";
                if (ms.Accepted == true)
                {

                    foreach (var ja in jas)
                    {
                        if (ja != null)
                        {

                            var op = _context.Staff.Where(a => a.StaffEmail.ToLower() == ja.Opscon.ToLower()).FirstOrDefault();

                            //var stf = _staffRep.Where(a => a.UserId == ja.Opscon).FirstOrDefault();
                            venue = ms.Venue.ToLower() == "your office" ? ms.Venue + " (" + ms.Address + ")" : "Facility Address (" + ms.Address + ")";
                            msgBody = string.Format(body, subject, op.FirstName + " " + op.LastName, appl.reference, venue, ms.MeetingDate, company.name, facility.Name, ms.Message, DateTime.Now.Year);

                            var emailMsg3 = _helpersController.SaveMessage(appl.id, op.StaffID, subject, msgBody, op.StaffElpsID, "Staff");
                            var sendEmail3 = _helpersController.SendEmailMessage2Staff(op.StaffEmail, op.FirstName, emailMsg3, null);

                        }
                    }
                    #endregion

                    #region Then Inpsector(s)

                    var jStaff = _context.JointAccountStaffs.Where(a => a.ApplicationId == ms.ApplicationId).ToList();

                    var file2 = Path.Combine(up, "Templates/FD_ScheduleMeeting.txt");

                    using (var sr = new StreamReader(file2))
                    {

                        body = sr.ReadToEnd();
                    }

                    subject = "Schedule for a Joint Meeting/Inspection with Depot ";
                    foreach (var item in jStaff)
                    {
                        if (item != null)
                        {
                            var jntStaff = _context.Staff.Where(x => x.StaffEmail.ToLower() == item.Staff.ToLower()).FirstOrDefault();
                            venue = ms.Venue.ToLower() == "your office" ? ms.Venue + " (" + ms.Address + ")" : "Facility Address (" + ms.Address + ")";
                            msgBody = string.Format(body, subject, jntStaff.FirstName + " " + jntStaff.LastName, appl.reference, venue, ms.MeetingDate, company.name, facility.Name, ms.Message, DateTime.Now.Year);

                            var emailMsgg = _helpersController.SaveMessage(appl.id, jntStaff.StaffID, subject, msgBody, jntStaff.StaffElpsID, "Staff");
                            var sendEmaill = _helpersController.SendEmailMessage2Staff(jntStaff.StaffEmail, jntStaff.FirstName, emailMsgg, null);

                            string[] stfE = { item.Staff };
                            //Add Inspectors(If not already Added) to attendees of this Schedule
                            _helpersController.AddStaffToMeeting(ms.Id, stfE, userEmail, "ipaddress");

                        }
                    }

                }
                //alert AD and Suppervisor, of this Action.
                var ad = (from stf in _context.Staff
                          join r in _context.UserRoles on stf.RoleID equals r.Role_id
                          where stf.DeleteStatus != true && stf.ActiveStatus != false
                          && r.RoleName == GeneralClass.ADPDJ
                          select stf).FirstOrDefault();

                var appProc = _context.MyDesk.Where(a => a.StaffID == appl.current_desk && a.AppId == appl.id).FirstOrDefault();
                var processor = _context.Staff.Where(a => a.StaffID == appProc.StaffID).FirstOrDefault();

                var filee = Path.Combine(up, "Templates/InternalMemo.txt");

                using (var sr = new StreamReader(filee))
                {

                    body = sr.ReadToEnd();
                }

                //send company action on scheduled inspection to AD and scheduler
                subject = "Marketer Approved Meeting/Inspection";
                string ap = ms.Accepted == true ? "ACCEPTED" : "DECLINED";
                string rs = ms.DeclineReason != null ? "" : $"Reason: {ms.DeclineReason}";
                string msg = $"Application with Reference: {appl.reference} from {facility.Name}({company.name}) has {ap} the Meeting/Inspection Scheduled on {ms.MeetingDate}. {rs}";

                msgBody = string.Format(body, subject, "", $"{ad.FirstName} {ad.LastName}", msg);

                var emailMsg = _helpersController.SaveMessage(appl.id, ad.StaffID, subject, msgBody, ad.StaffElpsID, "Staff");
                var sendEmail = _helpersController.SendEmailMessage2Staff(ad.StaffEmail, ad.FirstName, emailMsg, null);

                msgBody = string.Format(body, subject, "", $"{processor.FirstName} {processor.LastName}", msg);
                var emailMsg2 = _helpersController.SaveMessage(appl.id, processor.StaffID, subject, msgBody, processor.StaffElpsID, "Staff");
                var sendEmail2 = _helpersController.SendEmailMessage2Staff(processor.StaffEmail, processor.FirstName, emailMsg2, null);

                #endregion
            }
            #endregion


            #region Remove Reminder since it has been worked upon
            var manrem = _context.ManagerReminders.Where(a => a.ScheduleId == ms.Id).FirstOrDefault();
            if (manrem != null)
            {
                _context.ManagerReminders.Remove(manrem);
                _context.SaveChanges();
            }
            #endregion

            ViewBag.Schedule = "Presentation Schedule";
            return View("ConfirmSchedule");
        }

        #endregion

        public IActionResult ViewStaffDesk(string userid, string stf)
        {
            var staff = _context.Staff.Where(a => a.StaffEmail == userid).FirstOrDefault();
            if (staff == null)
            {
                return View("Error");
            }
            var myApp = (from proc in _context.MyDesk.AsEnumerable()
                         join wp in _context.WorkProccess.AsEnumerable() on proc.ProcessID equals wp.ProccessID
                         join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                         join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                         join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                         join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                         join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                         join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                         join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                         where proc.StaffID == staff.StaffID && proc.HasWork != true && app.DeleteStatus != true && c.DeleteStatus != true
                         select new MyApps
                         {

                             appID = app.id,
                             Reference = app.reference,
                             CategoryName = cat.name,
                             PhaseName = phs.name,
                             //Stage = s.StageName,

                             Status = app.status,
                             Date_Added = Convert.ToDateTime(app.date_added),
                             DateSubmitted = app.CreatedAt == null ? Convert.ToDateTime(app.date_added) : Convert.ToDateTime(app.CreatedAt),
                             Submitted = app.submitted,
                             CompanyDetails = c.name + " (" + c.Address + ") ",
                             CompanyName = c.name,
                             Company_Id = c.id,
                             FacilityDetails = fac.Name,
                             FacilityName = fac.Name,
                             processID = proc.ProcessID,
                             currentDeskID = proc.DeskID,
                             DateProcessed = proc.CreatedAt,
                             Year = app.year,
                             Type = app.type,
                             Holding = proc.Holding,
                             Address_1 = ad.address_1,
                             City = ad.city,
                             StateName = sd.StateName,
                             LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                             Activity = wp.Sort == 1 ? "(10%) Approve/Reject" :
                                wp.Sort == 2 ? "(20%) Approve/Reject: Conduct Inspection" :
                                wp.Sort == 3 ? "(30%) Approve/Reject: Conduct Inspection" :
                                wp.Sort == 4 ? "(40%) Approve/Reject: Approve Inspection" :
                                wp.Sort == 5 ? "(50%) Approve/Reject" :
                                wp.Sort == 6 ? "(60%) Approve/Reject" :
                                wp.Sort == 7 ? "(70%) Approve/Reject" :
                                wp.Sort == 8 ? "(80%) Approve/Reject" :
                                wp.Sort == 9 ? "(85%) Approve/Reject" :
                                wp.Sort == 10 ? "(90%) Approve/Reject" :
                                wp.Sort == 11 ? "(95%) Approve/Reject" :
                                wp.Sort == 12 ? "(98%) Final Approval/Reject " : "Approval/License Granted"

                             //AllowPush=proc.AllowPush
                         }).ToList();



            var myAppsToReturn = new List<application_Processings>();
            var holdingApps = new List<application_Processings>();

            var Schedules = _context.MeetingSchedules.Where(s => s.StaffUserName.ToLower() == staff.StaffEmail.ToLower()).ToList();

            ViewBag.staff = stf;
            ViewBag.StaffUID = userid;
            ViewBag.Staff = staff.StaffID;
            return View(myApp);
        }
        [HttpPost]
        public IActionResult ReassignApp(int appId, string staffId)
        {
            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var staff = _context.Staff.Where(a => a.StaffEmail == staffId.Trim()).FirstOrDefault();
                if (staff == null)
                {
                    return Content($"Staff with Email {staffId} was not found");
                }
                var apProc = _context.application_Processings.Where(a => a.Id == appId).FirstOrDefault();
                if (apProc != null)
                {
                    apProc.processor = staff.StaffID;
                    _context.SaveChanges();

                    string comment = $"Application was moved to {staffId} desk by {userEmail}";

                    _helpersController.SaveHistory(apProc.ApplicationId, userID, userEmail, GeneralClass.Move, comment);

                    return Content("0");

                }
                return Content($"Application with the Id {appId} was not found");

            }
            catch (Exception x)
            {
                return Content($"Some Error Occured while moving the Application, Please try again or contact Support. Thanks");
            }
        }

        private string DistributionMessage(int code)
        {
            switch (code)
            {
                case 0:
                    //type = 1;
                    return "Jobs has been distributed evenly among other users|1";
                case 1:
                    //type = 2;
                    return "This user is not registered to any work role|2";
                case 2:
                    //type = 2;
                    return "No other user that is active in same role with the Staff you are trying to Relieve, create a user in same work role and branch before this operation|3";
                case 3:
                    //type = 0;
                    return "User has been activated|0";
                default:
                    //type = 2;
                    return "Error in processing your command. Please try again|2";
            }
        }
    }

    public class ErrorLeaveVM
    {
        public Leaves Leave { get; set; }
        public string ErrorMsg { get; set; }
        public FieldOffices UserBranch { get; set; }
    }
}

