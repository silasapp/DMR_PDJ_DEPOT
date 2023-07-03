using System;
using System.Collections.Generic;
using System.Linq;
using NewDepot.Controllers.Configurations;
using NewDepot.Helpers;
using NewDepot.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace NewDepot.Controllers
{

    [Authorize(Policy = "AllStaffRoles")]

    public class DeskesController : Controller
    {
        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();
        RestSharpServices _restService = new RestSharpServices();


        public DeskesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        public IActionResult Index()
        {
            return View();
        }




        public IActionResult StaffDesk(string id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var staffDesk = (from sf in _context.Staff.AsEnumerable()
                        join fo in _context.FieldOffices.AsEnumerable() on sf.FieldOfficeID equals fo.FieldOffice_id
                        join zf in _context.ZonalFieldOffice.AsEnumerable() on fo.FieldOffice_id equals zf.FieldOffice_id
                        join z in _context.ZonalOffice.AsEnumerable() on zf.Zone_id equals z.Zone_id
                        join r in _context.UserRoles on sf.RoleID equals r.Role_id
                        where !r.RoleName.Contains("Support") && !r.RoleName.Contains("Admin") && r.RoleName != GeneralClass.ADMIN && r.RoleName != GeneralClass.IT_ADMIN
                        && r.RoleName != GeneralClass.ICT_ADMIN && sf.StaffID != _helpersController.getSessionUserID()
                        && r.RoleName != userRole && sf.LastName!=null && sf.FirstName!=null 
                             select new StaffDesk
                        {
                            AppCount = _context.MyDesk.Where(x => x.StaffID == sf.StaffID && x.HasWork != true).AsEnumerable().GroupBy(x => x.AppId).Count(),
                            //AllAppCount = _context.MyDesk.Where(x => x.StaffID == sf.StaffID).AsEnumerable().GroupBy(x => x.AppId).Count(),
                            StaffName = sf?.LastName.ToUpper() + " " + sf.FirstName.ToUpper(),
                            StaffEmail = sf?.StaffEmail,
                            StaffID = (int)sf?.StaffID,
                            StaffRole = r.RoleName,
                            FieldOffice = fo.OfficeName,
                            ZonalOffice = z.ZoneName,
                            OfficeId = fo.FieldOffice_id,
                            ZoneId = z.Zone_id,
                            ActiveStatus = sf.ActiveStatus == true ? "Active" : "De-activated",
                            DeletedStatus = sf.DeleteStatus == true ? "Deleted" : "Active"
                        }).ToList();

            var getStaff = from s in _context.Staff.AsEnumerable()
                           join l in _context.Location.AsEnumerable() on s.LocationID equals l.LocationID
                           join f in _context.FieldOffices.AsEnumerable() on s.FieldOfficeID equals f.FieldOffice_id
                           join z in _context.ZonalFieldOffice.AsEnumerable() on s.FieldOfficeID equals z.FieldOffice_id
                           join zo in _context.ZonalOffice.AsEnumerable() on z.Zone_id equals zo.Zone_id
                           where s.StaffID == _helpersController.getSessionUserID()
                           select new
                           {
                               Location = l.LocationName,
                               OfficeName = f.OfficeName,
                               ZonalId = z.Zone_id,
                               ZoneName = zo.ZoneName
                           };

            ViewData["StaffDesk"] = "All Staff Desk";

            //if (id == "_location")
            
                if (getStaff.FirstOrDefault().Location == "FO")
                {
                    staffDesk = staffDesk.Where(x => x.StaffID != userID && x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
                    if (userRole == GeneralClass.TEAMLEAD)
                    {
                        staffDesk = staffDesk.Where(x => x.StaffRole == GeneralClass.INSPECTOR && x.OfficeId == _helpersController.getSessionOfficeID()).ToList();

                    }
                    ViewData["StaffDesk"] = "All Staff Desk For " + getStaff.FirstOrDefault()?.OfficeName;
                }
                else if (getStaff.FirstOrDefault().Location == "ZO")
                {
                    staffDesk = staffDesk.Where(x => x.StaffID != userID && (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZonalId)).ToList();
                    if (userRole == GeneralClass.TEAMLEAD)
                    {
                        staffDesk = staffDesk.Where(x => x.StaffRole == GeneralClass.INSPECTOR && x.OfficeId == _helpersController.getSessionOfficeID()).ToList();

                    }
                    ViewData["StaffDesk"] = "All Staff Desk For " + getStaff.FirstOrDefault().ZoneName;
                }
                else if (getStaff.FirstOrDefault().Location == "HQ")
                {
                    if (userRole == GeneralClass.ADPDJ)
                    {
                        staffDesk = staffDesk.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
                        staffDesk = staffDesk.Where(x =>  x.StaffRole  == GeneralClass.SUPERVISOR || x.StaffRole == GeneralClass.INSPECTOR && x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
                        ViewData["StaffDesk"] = "All Staff Desk For " + getStaff.FirstOrDefault().OfficeName;

                    }
                    if ( userRole == GeneralClass.SUPERVISOR)
                    {
                        staffDesk = staffDesk.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
                        staffDesk = staffDesk.Where(x =>  x.StaffRole == GeneralClass.INSPECTOR && x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
                        ViewData["StaffDesk"] = "All Staff Desk For " + getStaff.FirstOrDefault().OfficeName;

                    }
                }
            
             
            _helpersController.LogMessages("Displaying applications with staff.", _helpersController.getSessionEmail());

            return View(staffDesk.ToList());
        }




        public IActionResult StaffDeskApps(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Staff identification link for getting applications is broken or not in correct format.") });
            }

            int staffID = 0;

            var staff_id = generalClass.Decrypt(id);

            if (staff_id == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Staff identification link for getting applications is broken or not in correct format.") });
            }
            else
            {
                staffID = Convert.ToInt32(staff_id);

                var Staffs = _context.Staff.Where(x => x.StaffID == staffID);

              
                var staffDesk = from d in _context.MyDesk.AsEnumerable()
                                join a in _context.applications.AsEnumerable() on d.AppId equals a.id
                                join c in _context.companies.AsEnumerable() on a.company_id equals c.id
                                join f in _context.Facilities.AsEnumerable() on a.FacilityId equals f.Id
                                join sf in _context.Staff.AsEnumerable() on d.StaffID equals sf.StaffID into Staff
                                join cd in _context.Staff.AsEnumerable() on a.current_desk equals cd.StaffID into staffs
                                from cds in staffs.DefaultIfEmpty()
                                join ad in _context.addresses.AsEnumerable() on f.AddressId equals ad.id
                                join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                                join cat in _context.Categories.AsEnumerable() on a.category_id equals cat.id
                                join phs in _context.Phases.AsEnumerable() on a.PhaseId equals phs.id
                                where a.DeleteStatus != true && d.StaffID == staffID && d.HasWork != true
                                select new MyApps
                                {
                                    currentDeskID = d.DeskID,
                                    appID = a.id,
                                    Reference = a.reference,
                                    hasWorked = d.HasWork,
                                    Status = a.status,
                                    CompanyName = c.name,
                                    CurrentStaff = cds?.LastName.ToUpper() + " " + cds?.FirstName.ToUpper(),
                                    CurrentOffice=cds!=null? _context.FieldOffices.Where(f=>f.FieldOffice_id== cds.FieldOfficeID).FirstOrDefault().OfficeName :"No office",
                                    Date_Added = d.CreatedAt,
                                    ProcessedOn = d?.UpdatedAt,
                                    ProcessingDays = a.CreatedAt != null ? _helpersController.ProcessingDays(d.CreatedAt, d?.UpdatedAt, a?.CreatedAt) : _helpersController.ProcessingDays(d.CreatedAt, d?.UpdatedAt, a?.date_added),
                                    FacilityName = f.Name,
                                    DateSubmitted = a.CreatedAt != null ? (DateTime)a.CreatedAt : (DateTime)a.date_added,
                                    Company_Id = c.id,
                                    CategoryName = phs.name + " - " + phs.ShortName ,
                                    Staff = Staff.FirstOrDefault().LastName.ToUpper() + " " + Staff.FirstOrDefault().FirstName.ToUpper()
                             
                                };

                var staffDeskResult = staffDesk.GroupBy(x => x.appID).Select(c => c.FirstOrDefault()).ToList().OrderByDescending(x => x.Date_Added);

                ViewData["StaffDeskDetails"] = staffDeskResult.Where(x=> x.hasWorked!=true).Count() + " pending application(s) for " + Staffs.FirstOrDefault().LastName.ToUpper() + " " + Staffs.FirstOrDefault().FirstName.ToUpper();
                ViewData["OriginalStaffID"] = staffID;

                _helpersController.LogMessages("Displaying applications on a staff desk.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail)));

                return View(staffDeskResult);
            }
        }



        /*
        * Getting staffs to re-rout application too
        * NOTE : the new staffs must be in the same role with the previous staff
        * 
        * staff => encrypted previous staff id
        */

        public JsonResult GetRouteStaff(string staff)
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            int staffID = 0;

            var staff_id = generalClass.Decrypt(staff);

            if (staff_id == "Error")
            {
                return Json("Application report link error");
            }
            else
            {
                staffID = Convert.ToInt32(staff_id);

                var previousStaff = _context.Staff.Where(x => x.StaffID == staffID);
                var staffs = _context.Staff.Where(x => x.RoleID == previousStaff.FirstOrDefault().RoleID && x.FieldOfficeID == previousStaff.FirstOrDefault().FieldOfficeID
                && x.LocationID == previousStaff.FirstOrDefault().LocationID && x.ActiveStatus == true && x.DeleteStatus != true && x.StaffID != previousStaff.FirstOrDefault().StaffID);

                totalRecords = staffs.Count();
                var data = staffs.Skip(skip).Take(pageSize).ToList();

                _helpersController.LogMessages("Displaying list of staff to reroute application to.", _helpersController.getSessionEmail());

                return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });
            }
        }




        /*
         * Rerouting application from a previous staff to a new staff.
         * 
         * staffID => the new staff to route the application to
         * AppID => a list of all application ids of previous staff applications
         */

        public JsonResult RerouteApps(int staffID, string previousStaff, List<int> AppID)
        {
            try { 
            string result = "";
            int done = 0;

            int previous_staff = generalClass.DecryptIDs(previousStaff);

            if (previous_staff == 0)
            {
                result = "Something went wrong cannot find reference for previous selected staff.";
            }
            else
            {
                    foreach (var app in AppID)
                    {
                        var dsk = _context.MyDesk.Where(x => x.AppId == app && x.StaffID == previous_staff);

                        if (dsk.Count() > 0)
                        {
                            foreach (var d in dsk.ToList())
                            {
                                d.StaffID = staffID;
                                d.UpdatedAt = DateTime.Now;
                                done += _context.SaveChanges();
                            }
                        }

                        var apps = _context.applications.Where(x => x.id == app && x.current_desk == previous_staff && x.isLegacy != true && x.DeleteStatus != true);

                        if (apps.Count() > 0)
                        {
                            apps.FirstOrDefault().current_desk = staffID;

                            done += _context.SaveChanges();
                        }


                        if (done > 0)
                        {
                            result = "1|" + "Application(s) re-routed successfully ";

                            var staffName = _context.Staff.Where(x => x.StaffID == staffID);
                            var prevStaff = _context.Staff.Where(x => x.StaffID == previous_staff);

                            string subj = "Application Rerouted To Your Desk";
                            string cont = "New set of applications has been rerouted from " + prevStaff.FirstOrDefault().LastName + " " + prevStaff.FirstOrDefault().FirstName + " desk to your desk.";
                            var emailMsg = _helpersController.SaveMessage(app, staffName.FirstOrDefault().StaffID, subj, cont, staffName.FirstOrDefault().StaffElpsID.ToString(), "Staff");
                            var msg = _helpersController.SendEmailMessage2Staff(staffName.FirstOrDefault().StaffEmail, staffName.FirstOrDefault().LastName + " " + staffName.FirstOrDefault().FirstName, emailMsg, null);
                            
                            _helpersController.SaveHistory(app, staffName.FirstOrDefault().StaffID, "Re-Routed", "Application re-routed to staff desk for processing.");

                        }
                        else
                        {
                            result = "0|Something went wrong trying to reroute one or more application. Please try again later.";
                        }
                    }
            }

            _helpersController.LogMessages(result + "App ID : " + string.Join(",", AppID), _helpersController.getSessionEmail());

            return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please contact support");
            }
        }

    }
}