using LpgLicense.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NewDepot.Helpers;
using NewDepot.Models;
using System;
using System.Linq;
using System.Security.Claims;
using NewDepot.Controllers;
using NewDepot.Controllers.Authentications;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace NewDepot.Controllers
{
    [Authorize]

    public class StaffsController : Controller
    {
        private readonly Depot_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;

        ElpsResponse elpsResponse = new ElpsResponse();
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;

        public StaffsController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }
        public JsonResult MyDeskCount()
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            var mydesk = (from proc in _context.MyDesk.AsEnumerable()
                          join wp in _context.WorkProccess.AsEnumerable() on proc.ProcessID equals wp.ProccessID
                          join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                          where proc.StaffID == userID && proc.HasWork != true && app.DeleteStatus != true
                          select proc).ToList();

            if(userRole.ToUpper() == GeneralClass.ED_STA.ToUpper()) 
                {
                    mydesk = (from proc in _context.MyDesk.AsEnumerable()
                              join wp in _context.WorkProccess.AsEnumerable() on proc.ProcessID equals wp.ProccessID
                              join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                              join stf in _context.Staff.AsEnumerable() on proc.StaffID equals stf.StaffID
                              join rl in _context.UserRoles.AsEnumerable() on stf.RoleID equals rl.Role_id
                              where rl.RoleName == GeneralClass.ED && proc.HasWork != true && app.DeleteStatus != true
                              select proc).ToList();

            } 
            if(userRole.ToUpper() == GeneralClass.ACE_STA.ToUpper()) 
                {
                    mydesk = (from proc in _context.MyDesk.AsEnumerable()
                              join wp in _context.WorkProccess.AsEnumerable() on proc.ProcessID equals wp.ProccessID
                              join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                              join stf in _context.Staff.AsEnumerable() on proc.StaffID equals stf.StaffID
                              join rl in _context.UserRoles.AsEnumerable() on stf.RoleID equals rl.Role_id
                              where rl.RoleName == GeneralClass.ACE_STA && proc.HasWork != true && app.DeleteStatus != true
                              select proc).ToList();

            }




            return Json(mydesk.Count().ToString());
        }
        public JsonResult MyJACount()
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            
            #region get Joint Application Desk(s)

            var jointAcc = _context.JointAccounts.Where(a => a.Opscon.ToLower() == userEmail.ToLower() && a.OperationsCompleted != true).ToList();

            var jointAcct = _context.JointAccountStaffs.Where(a => a.Staff.ToLower() == userEmail.ToLower() && a.OperationsCompleted != true).ToList();

            #endregion
            int mydeskCount = jointAcc.Count() + jointAcct.Count();

            return Json(mydeskCount.ToString());
        }

        public JsonResult MyMessageCount()
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var mysch = _context.messages.Where(x => x.UserID == userID && x.read != 1 && x.UserType.ToLower()=="staff").Count();
            return Json(mysch);
        }
        public JsonResult MyValidScheduleCount()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var mysch = _context.MeetingSchedules.Where(x => x.StaffUserName == userEmail && x.ScheduleExpired== null).Count();
            return Json(mysch);
        }
        public JsonResult MyScheduleApprovalCount()
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var fieldoffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID()).FirstOrDefault();
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            int schCount = 0;

            if (fieldoffice != null)
            {
                if (fieldoffice.OfficeName.ToLower().Contains("head"))
                {
                    var mysch = (from u in _context.MeetingSchedules
                                 join mgr in _context.ManagerScheduleMeetings on u.Id equals mgr.ScheduleId
                                 join app in _context.applications on u.ApplicationId equals app.id
                                 join phase in _context.Phases on app.PhaseId equals phase.id
                                 where u.Approved == null && u.ScheduleExpired == null
                                 && phase.FlowType == GeneralClass.HQ_FLOW
                                 select u);
                    schCount = mysch.Count();
                }
                else
                {

                    var mysch = (from u in _context.MeetingSchedules
                                 join mgr in _context.ManagerScheduleMeetings on u.Id equals mgr.ScheduleId
                                 join stf in _context.Staff on u.SchedulerID equals stf.StaffID
                                 join app in _context.applications on u.ApplicationId equals app.id
                                 join phase in _context.Phases on app.PhaseId equals phase.id
                                 where stf.FieldOfficeID== fieldoffice.FieldOffice_id && mgr.UserId == userEmail && u.Approved == null && u.ScheduleExpired == null
                                 select u);
                    schCount = mysch.Count();

                }
            }
            return Json(schCount);
        }
        public JsonResult LegacyAppsCount()
        {
            var LegCount = _context.Legacies.Where(x => x.Status == GeneralClass.Processing && x.DeleteStatus == false).Count();
            return Json(LegCount);
        }


        public IActionResult Dashboard()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            
            if (userRole=="Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }
            else
            {
                StaffDashBoardModel model = new StaffDashBoardModel();


                    var myDesks = (from proc in _context.MyDesk.AsEnumerable()
                               join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                               where proc.StaffID == userID && proc.HasWork != true && app.DeleteStatus != true
                               select proc).ToList();

                if (userRole.ToUpper() == GeneralClass.ED_STA.ToUpper())
                {

                    myDesks = (from proc in _context.MyDesk.AsEnumerable()
                              join wp in _context.WorkProccess.AsEnumerable() on proc.ProcessID equals wp.ProccessID
                              join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                              join stf in _context.Staff.AsEnumerable() on proc.StaffID equals stf.StaffID
                              join rl in _context.UserRoles.AsEnumerable() on stf.RoleID equals rl.Role_id
                              where rl.RoleName == GeneralClass.ED && proc.HasWork != true && app.DeleteStatus != true
                              select proc).ToList();

                }
                if (userRole.ToUpper() == GeneralClass.ACE_STA.ToUpper())
                {

                    myDesks = (from proc in _context.MyDesk.AsEnumerable()
                              join wp in _context.WorkProccess.AsEnumerable() on proc.ProcessID equals wp.ProccessID
                              join app in _context.applications.AsEnumerable() on proc.AppId equals app.id
                              join stf in _context.Staff.AsEnumerable() on proc.StaffID equals stf.StaffID
                              join rl in _context.UserRoles.AsEnumerable() on stf.RoleID equals rl.Role_id
                              where rl.RoleName == GeneralClass.AUTHORITY && proc.HasWork != true && app.DeleteStatus != true
                              select proc).ToList();

                }


                model.OnMyDesk = myDesks.ToList().Count;


                var getAll = (from ap in _context.applications
                                 where ap.DeleteStatus != true && ap.isLegacy!=true && ap.company_id > 0
                                 select ap);
                
                var getAllApp = (from ap in _context.applications
                                 where ap.DeleteStatus != true && ap.isLegacy!=true && ap.submitted == true
                                  && ap.status!= GeneralClass.PaymentPending
                                 select ap);

                int processedCount = 0;
                    if (getAllApp.Count() > 0)
                    {
                        var fromHistory = (from u in _context.application_desk_histories
                                           join ap in _context.applications on u.application_id equals ap.id
                                           join dk in _context.MyDesk on u.application_id equals dk.AppId
                                           where ap.DeleteStatus != true && ap.isLegacy != true && ap.submitted == true
                                           && u.UserName.ToLower() == userEmail.ToLower() && dk.HasWork == true
                                           select u).ToList();

                         processedCount = fromHistory.Count();
                        #region Filter By App/Action count not Action count only
                        var apprv = new List<application_desk_histories>();
                        var apprv2 = _context.applications.Where(x=>x.status== GeneralClass.Approved);
                        foreach (var item in fromHistory.Where(a => a.status == GeneralClass.Approved).ToList())
                        {
                            var pck = apprv.Where(a => a.application_id == item.application_id).FirstOrDefault();
                            if (pck == null)
                                apprv.Add(item);
                        }

                        var decl = new List<application_desk_histories>();
                        foreach (var item in fromHistory.Where(a => a.status == GeneralClass.Rejected).ToList())
                        {
                            var pck = decl.Where(a => a.application_id == item.application_id).FirstOrDefault();
                            if (pck == null)
                                decl.Add(item);
                        }
                        var totalAppCount = new List<application_desk_histories>();

                        foreach (var item in fromHistory.Where(a => a.status.ToLower() == "approved" || a.status.ToLower() == "approved").ToList())
                        {
                            var pck = totalAppCount.Where(a => a.application_id == item.application_id).FirstOrDefault();
                            if (pck == null)
                                totalAppCount.Add(item);
                        }

                        #endregion
                        model.Approved = apprv2.Count();
                        model.Declined = decl.Count();
                        model.TotalApplications = fromHistory.GroupBy(x=> x.application_id).Distinct().Count(); 
                    }
                

                var deskCount = model.OnMyDesk;
                model.TodayApplications = getAll.Where(p => p.CreatedAt > DateTime.Now.AddDays(-1)).ToList().Count();
                model.AdTotalApplications = getAllApp.Where(x => x.submitted == true).Count();
                model.Processing = getAllApp.Where(x => x.status == GeneralClass.Processing).Count();
                model.AdApproved = getAllApp.Where(x => x.status == GeneralClass.Approved).Count();
                model.AdRejected = getAllApp.Where(x => x.status == GeneralClass.Rejected).Count();

                model.TotalPermits = getAllApp.Where(a => a.status.ToLower() == GeneralClass.Approved.ToLower()).Count();

                model.OnMyDesk = deskCount;
                model.StaffRole = userRole;
                
                return View(model);
            }

            
        }

        public IActionResult CSApplications(string workedon, string status = null)
        {
            List<FieldOffices> getFieldOffice = new List<FieldOffices>();
            List<ZonalOffice> getZonalOffice = new List<ZonalOffice>();

            var getCategory = _context.Categories.Where(x => x.DeleteStatus != true);
            var getPhase = _context.Phases.Where(x => x.DeleteStatus != true);
            var getProduct = _context.Products.Where(x => x.DeletedStatus != true);
            var fieldoffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID());

            if (fieldoffice.FirstOrDefault().OfficeName.ToLower().Contains("head"))
            {
                getFieldOffice = _context.FieldOffices.Where(x => x.DeleteStatus != true).ToList();
                getZonalOffice = _context.ZonalOffice.Where(x => x.DeleteStatus != true).ToList();
            }
            else
            {
                getFieldOffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID()).ToList();
                getZonalOffice = (from zf in _context.ZonalFieldOffice.AsEnumerable()
                                  join zo in _context.ZonalOffice.AsEnumerable() on zf.Zone_id equals zo.Zone_id
                                  where zf.FieldOffice_id == _helpersController.getSessionOfficeID()
                                  select new ZonalOffice
                                  {
                                      Zone_id = zo.Zone_id,
                                      ZoneName = zo.ZoneName
                                  }).ToList();
            }

            List<SearchList> searchLists = new List<SearchList>();

            searchLists.Add(new SearchList
            {
                categories = getCategory.ToList(),
                phases = getPhase.ToList(),
                offices = getFieldOffice.ToList(),
                zonalOffices = getZonalOffice.ToList(),
            });

            #region get Count
            var myApps = _helpersController.ApplicationDetails();
            ViewBag.SuitabilityCount = myApps.Where(x => x.CategoryName.ToLower().Contains("suitability")).Count();
            ViewBag.ModificationCount = myApps.Where(x => x.CategoryName.ToLower().Contains("modification")).Count();
            ViewBag.TakeOverCount = myApps.Where(x => x.CategoryName.ToLower().Contains("take over")).Count();
            ViewBag.RegularizationCount = myApps.Where(x => x.CategoryName.ToLower().Contains("regularization")).Count();
            ViewBag.ATCCount = myApps.Where(x => x.CategoryName.ToLower().Contains("approval to construct")).Count();
            ViewBag.LTOCount = myApps.Where(x => x.CategoryName.ToLower().Contains("license to operate")).Count();
            ViewBag.LRCount = myApps.Where(x => x.CategoryName.ToLower().Contains("renwal")).Count();
            ViewBag.CalibrationCount = myApps.Where(x => x.ShortName.ToLower().Contains("ndt")).Count();
            ViewBag.ReCalibrationCount = myApps.Where(x => x.CategoryName.ToLower().Contains("recalibration")).Count();
            ViewBag.ApplicationCount = myApps.Count();
            #endregion

            ViewBag.Title = "Applications Reports";
            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO") { 

                ViewBag.Title = getStaff.FirstOrDefault().FieldOffice + "Applications Reports";
                 myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                ViewBag.Title = getStaff.FirstOrDefault().FieldOffice + "Applications Reports";
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else
            {
                ViewBag.HQ = true;

            }

            //check status
            myApps = status != null ? myApps.Where(x => x.Status == status).ToList() : myApps;

            List<string> Year = new List<string>();
            myApps.ToList().ForEach(x =>
            {
                Year.Add(x.Year.ToString());
            });
            ViewBag.MyApps = myApps;
            ViewBag.Year = Year.Distinct();
            return View(searchLists.ToList());
        }

        public IActionResult MyMessages()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var mysch = _context.messages.Where(x => x.UserID == userID && x.UserType.ToLower() == "staff");
            return View(mysch.ToList());
        }

        public IActionResult ViewMessage(string id, string option)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            if (userEmail == "Error")
            {
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(option))
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Message not found or not in correct format. Kindly contact support.") });
                }

                int staff_id = 0;
                int msg_id = 0;

                var c_id = generalClass.Decrypt(id);
                var m_id = generalClass.Decrypt(option);

                if (c_id == "Error" || m_id == "Error")
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Message not found or not in correct format. Kindly contact support.") });
                }
                else
                {
                    staff_id = Convert.ToInt32(c_id);
                    msg_id = Convert.ToInt32(m_id);

                    var msg = _context.messages.Where(x => x.id == msg_id);
                    var app = _context.applications.Where(a => a.id == msg.FirstOrDefault().AppId).FirstOrDefault();
                    var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                    var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    var facState = address == null ? null : _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();

                    ViewBag.application = app;
                    ViewBag.Facility = facility.Name + "(" + address.address_1 + ", " + address.city + " " + facState.StateName + ")";
                    msg.FirstOrDefault().read = 1;
                    _context.SaveChanges();

                    ViewData["MessageTitle"] = msg.FirstOrDefault().subject;

                    _helpersController.LogMessages("Displaying single company's message...", userEmail);
                    return View(msg.FirstOrDefault());
                }
            }
        }

        public List<MyApps> ReportDetails()
        {


            var app = (from a in _context.applications
                       join f in _context.Facilities on a.FacilityId equals f.Id
                       join c in _context.companies on a.company_id equals c.id
                       join ad in _context.addresses on f.AddressId equals ad.id
                       join st in _context.States_UT on ad.StateId equals st.State_id
                       join sof in _context.FieldOfficeStates on st.State_id equals sof.StateId
                       join of in _context.FieldOffices on sof.FieldOffice_id equals of.FieldOffice_id
                       join zf in _context.ZonalFieldOffice on of.FieldOffice_id equals zf.FieldOffice_id
                       join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                       join ca in _context.Categories on a.category_id equals ca.id
                       join at in _context.Phases on a.PhaseId equals at.id
                       where a.DeleteStatus != true && a.isLegacy != true && a.submitted==true
                       select new MyApps
                       {
                           appID = a.id,
                           Reference = a.reference,
                           CompanyName = c.name,
                           CompanyDetails = c.CompanyEmail,
                           PhaseName = at.name,
                           LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                           FacilityId = f.Id,
                           Company_Id = c.id,
                           FacilityDetails = f.Name + " (" + ad.address_1 + ")",
                           OfficeId = of.FieldOffice_id,
                           Year = a.year,
                           //FlowType = a.FlowType,
                           OfficeName = of.OfficeName,
                           ZoneName = z.ZoneName,
                           ZoneId = z.Zone_id,
                           CategoryName = at.name.ToUpper(),
                           Products = _helpersController.GetFacilityProducts(f.Id),
                           ShortName = at.ShortName,
                           Type = a.type.ToUpper(),
                           Status = a.status,
                           StateName = st.StateName,
                           Date_Added = a.date_added,
                           days = a.CreatedAt!=null? DateTime.Now.Day - ((DateTime)a.CreatedAt).Day : DateTime.Now.Day - ((DateTime)a.date_added).Day,
                           dateString = a.CreatedAt!=null? a.CreatedAt.Value.Date.ToString("yyyy-MM-dd") : a.date_added.Date.ToString("yyyy-MM-dd"),
                           DateSubmitted = a.CreatedAt != null ? (DateTime)a.CreatedAt : (DateTime)a.date_added
                       });

            return app.GroupBy(x => x.appID).Select(x => x.FirstOrDefault()).OrderByDescending(x => x.appID).ToList();
        }


        public List<StaffDesk> GetStaff()
        {
            var getStaff = from s in _context.Staff.AsEnumerable()
                           join l in _context.Location.AsEnumerable() on s.LocationID equals l.LocationID
                           join f in _context.FieldOffices.AsEnumerable() on s.FieldOfficeID equals f.FieldOffice_id
                           join z in _context.ZonalFieldOffice.AsEnumerable() on s.FieldOfficeID equals z.FieldOffice_id
                           join zo in _context.ZonalOffice.AsEnumerable() on z.Zone_id equals zo.Zone_id
                           where s.StaffID == _helpersController.getSessionUserID()
                           select new StaffDesk
                           {
                               Location = l.LocationName,
                               FieldOffice = f.OfficeName,
                               ZoneId = z.Zone_id,
                               ZonalOffice = zo.ZoneName
                           };
            return getStaff.ToList();
        }


        #region User Section

        ////[Authorize(Roles = "Manager, Approver, ManagerPlus, Opscon, AD, HDS, AdOps, TeamLead")]
        public ActionResult StaffDesk()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userRole == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }
            var stf = _context.Staff.Where(st => st.StaffID == userID).FirstOrDefault();

            if (TempData["message"] != null)
            {
                ViewBag.Msg = TempData["message"].ToString();
                ViewBag.Type = TempData["msgType"].ToString();
            }
          
            if (stf!=null)
            {
                //get myZoneStates
                var getZoneState =(from zf in _context.ZonalFieldOffice
                              join zfs in _context.ZoneStates on zf.Zone_id equals zfs.Zone_id
                              where zf.FieldOffice_id == stf.FieldOfficeID && zf.DeleteStatus == false 
                              select zfs).FirstOrDefault();

                List<int> myZoneStaff = _helpersController.FieldOfficeStaff(getZoneState.State_id);

                List<Staff_UserBranchModel> returnUser = new List<Staff_UserBranchModel>();

                foreach (var branch in myZoneStaff)
                {

                    List<Staff_UserBranchModel> myDeptStaff = new List<Staff_UserBranchModel>();
                    if (userRole.Contains("Approver"))
                    {
                        myDeptStaff = (from s in _context.Staff
                                  join r in _context.UserRoles on s.RoleID equals r.Role_id
                                  where stf.DeleteStatus != true && s.FieldOfficeID == branch
                                  select new Staff_UserBranchModel
                                  {
                                      Id = s.StaffID,
                                      StaffId = s.StaffID,
                                      FieldId = (int)s.FieldOfficeID,
                                      RoleId = (int)s.RoleID,
                                      RoleName=r.RoleName,
                                      Active = stf.ActiveStatus,
                                      DeletedStatus = stf.DeleteStatus,
                                      StaffEmail = stf.StaffEmail,
                                      StaffFullName = s.FirstName + " " + s.LastName
                                  }).ToList();

                    }
                    else
                    {
                        if (User.IsInRole("TeamLead"))
                        {
                            myDeptStaff = (from s in _context.Staff
                                           join r in _context.UserRoles on s.RoleID equals r.Role_id
                 where stf.DeleteStatus != true && s.FieldOfficeID == branch && r.RoleName!=GeneralClass.ADOPS&& r.RoleName!=GeneralClass.OPSCON
                                           select new Staff_UserBranchModel
                                           {
                                               Id = s.StaffID,
                                               StaffId = s.StaffID,
                                               FieldId = (int)s.FieldOfficeID,
                                               RoleId = (int)s.RoleID,
                                               RoleName = r.RoleName,
                                               Active = stf.ActiveStatus,
                                               DeletedStatus = stf.DeleteStatus,
                                               StaffEmail = stf.StaffEmail,
                                               StaffFullName = s.FirstName + " " + s.LastName
                                           }).ToList();

                           
                        }
                        else
                        {
                            myDeptStaff = (from s in _context.Staff
                                           join r in _context.UserRoles on s.RoleID equals r.Role_id
                                           where s.DeleteStatus != true && s.FieldOfficeID == branch
                                           select new Staff_UserBranchModel
                                           {
                                               Id = s.StaffID,
                                               StaffId = s.StaffID,
                                               FieldId = (int)s.FieldOfficeID,
                                               RoleId = (int)s.RoleID,
                                               RoleName = r.RoleName,
                                               Active = s.ActiveStatus,
                                               DeletedStatus = s.DeleteStatus,
                                               StaffEmail = s.StaffEmail,
                                               StaffFullName=s.FirstName+" "+s.LastName
                                           }).ToList();


                        }
                    }

                    foreach (var staff in myDeptStaff.Where(a => a.StaffEmail != userEmail))
                    {
                       
                                returnUser.Add(staff);
                          
                        
                    }

                }
                return View(returnUser.OrderBy(s => s.FieldOffice).ToList());
            }
            return View();
        }

        //
        // GET: /Users/Details/5
        public IActionResult Details(string id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (id== null)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, staff ID was not passed correctly. Please try again") });

            }
            var stf = _context.Staff.Where(st => st.StaffEmail.ToLower() == id.ToLower()).FirstOrDefault();

            ViewBag.UserName = stf.FirstName + " " + stf.LastName + " (" + stf.StaffEmail + ")";

            return View(stf);
        }

        /// <summary>
        /// Get the Jobs on Staffs desk
        /// </summary>
        /// <param name="id">UserBranchID</param>
        /// <returns></returns>
        public IActionResult GetMyDeskCount(int id)
        {


            if (id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, staff ID was not passed correctly. Please try again") });

            }
            var stf = _context.Staff.Where(st => st.StaffID == id).FirstOrDefault();

            var staffApps = _context.MyDesk.Where(a => a.StaffID == id && a.HasWork != true);

            #region get Joint Application Desk(s)

            var jointAcc = _context.JointAccounts.Where(a => a.Opscon.ToLower() == stf.StaffEmail.ToLower() && a.OperationsCompleted != true).ToList();

            var jointAcct = _context.JointAccountStaffs.Where(a => a.Staff.ToLower() == stf.StaffEmail.ToLower() && a.OperationsCompleted != true).ToList();

            #endregion
            int mydeskCount = staffApps.Count() + jointAcc.Count() + jointAcct.Count();
            return Json(staffApps.Count());
        }


        //public ActionResult GetMyDeskCount_initial(int id)
        //{
        //    var myJobCount = _vAppProcRep.FindBy(C => C.Processor == id && !C.Processed).ToList();
        //    var myBranch = _vUserBranch.FindBy(a => a.Id == id).FirstOrDefault();

        //    var myAppsToReturn = new List<vApplication_Processing>();
        //    var holdingApps = new List<vApplication_Processing>();

        //    var Schedules = _vMSA.FindBy(s => s.StaffUserName.ToLower() == myBranch.UserEmail.ToLower()).ToList();

        //    #region Schedule Feedback
        //    if (Schedules.Count() > 0)
        //    {
        //        foreach (var item in myJobCount)
        //        {
        //            item.ScheduleBooked = false;
        //            var checker = Schedules.Where(s => s.ApplicationId == item.ApplicationId).OrderBy(s => s.Id).ToList();
        //            if (checker != null && checker.Count() > 0)
        //            {
        //                var chkTouse = checker[checker.Count() - 1];
        //                if (chkTouse.WaiverRequest != null && chkTouse.WaiverRequest.Value == true)
        //                {
        //                    #region Waiver Schedule
        //                    item.ScheduleBooked = true;
        //                    if (chkTouse.Approved != null && chkTouse.Approved.Value == true)
        //                    {
        //                        #region Manager Has Approve
        //                        item.ManagerApproved = true;
        //                        item.ScheduleCompleted = true;
        //                        #endregion
        //                    }
        //                    else
        //                        item.ManagerApproved = false;

        //                    if (!item.ManagerApproved)
        //                    {
        //                        holdingApps.Add(item);
        //                    }
        //                    else
        //                    {
        //                        myAppsToReturn.Add(item);
        //                    }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    #region Normal Schedule
        //                    item.ScheduleBooked = true;
        //                    if (chkTouse.Approved != null && chkTouse.Approved.Value == true)
        //                    {
        //                        #region Manager Has Approve
        //                        item.ManagerApproved = true;

        //                        if (chkTouse.Accepted != null && chkTouse.Accepted.Value == true)
        //                        {
        //                            // Client Has Approve
        //                            item.ClientAccepted = "YES";
        //                        }
        //                        else if (!chkTouse.Accepted != null && chkTouse.Accepted.Value == false)
        //                            item.ClientAccepted = "NO";
        //                        else
        //                        {
        //                            item.ClientAccepted = "WAITING";
        //                            //If waiting, check if not more than 3 days
        //                            var now = UtilityHelper.CurrentTime;
        //                            if (chkTouse.Date.AddDays(3) < now)
        //                            {
        //                                //Schedule Expired
        //                                item.Expired = true;
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    else
        //                        item.ManagerApproved = false;

        //                    if (item.ManagerApproved && item.ClientAccepted.ToLower() == "yes")
        //                    {
        //                        item.ScheduleCompleted = true;
        //                    }

        //                    if (!item.ScheduleBooked || (item.ScheduleBooked && item.Expired))
        //                    {
        //                        myAppsToReturn.Add(item);
        //                    }
        //                    else if (item.ScheduleCompleted)
        //                    {
        //                        myAppsToReturn.Add(item);
        //                    }
        //                    else
        //                    {
        //                        holdingApps.Add(item);
        //                    }
        //                    #endregion
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        myAppsToReturn.AddRange(myJobCount);
        //    }
        //    #endregion

        //    return Json(myAppsToReturn.Count());
        //}
        #endregion
    }
}
