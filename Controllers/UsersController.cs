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
    [Authorize]
    public class UsersController : Controller
    {
        SubmittedDocuments _appDocRep;
        RestSharpServices _restSharpServices = new RestSharpServices();
        public IConfiguration _configuration;
        IHttpContextAccessor _httpContextAccessor;
        private readonly Depot_DBContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        ElpsResponse elpsResponse = new ElpsResponse();
        ApplicationHelper appHelper;
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;

        public UsersController(IHostingEnvironment hostingEnvironment, Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }


        //[Authorize(Policy = "ADRMRoles")]
        public IActionResult Staff()
        {
            var response = _restSharpServices.Response("api/Accounts/Staff/{email}/{apiHash}");

            if (response != null)
            {
                var staffList = JsonConvert.DeserializeObject<List<LpgLicense.Models.Staff>>(response.Content);
                ViewBag.StaffList = "";
                if (staffList != null)
                {
                    if (staffList.Count() <= 0)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, A Network Error Has Occured. Kindly Check Your Internet Connection") });
                    }
                    else
                    {
                        ViewBag.StaffList = staffList;
                    }
                }
                else { 
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, A Network Error Has Occured. Kindly Check Your Internet Connection") });
            }
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, A Network Error Has Occured. Kindly Check Your Internet Connection") });

            }
                return View();
        }


        /*
         * Getting all staff on elps
         */
        //[Authorize(Policy = "AdminRoles")]
        public JsonResult GetAllElpsStaff()
        {
            var response = _restSharpServices.Response("api/Accounts/Staff/{email}/{apiHash}");

            if (response.ErrorException != null)
            {
                return Json(_restSharpServices.ErrorResponse(response));
            }
            else
            {
                return Json( SimpleJson.SimpleJson.DeserializeObject(response.Content));
            }
        }



        /*
         * Getting aall staff on elps by email
         */
        //[Authorize(Policy = "AdminRoles")]
        public JsonResult GetElpsStaff(string staffemail)
        {
            var paramData = new List<ParameterData>();

            if (staffemail != null)
            {

                paramData.Add(new ParameterData
                {
                    ParamKey = "staffEmail",
                    ParamValue = staffemail.Trim()
                });

                var response = _restSharpServices.Response("api/Accounts/Staff/{staffEmail}/{email}/{apiHash}", paramData);
                if (response.ErrorException != null)
                {
                    return Json(_restSharpServices.ErrorResponse(response));
                }
                else
                {
                    return Json(SimpleJson.SimpleJson.DeserializeObject<LpgLicense.Models.Staff>(response.Content));
                }
            }
            else
            {
                return Json("An error has occured. Staff email wasn't passed");
            }
        }
        /*
         * Creating staff on local system
         */
        //[Authorize(Policy = "AdminRoles")]
        public JsonResult CreateStaff(string ElpsHashID, string Email, string FirstName, string LastName, int RoleId, int FieldOfficeID, int LocationID, IFormFile StaffSignature)
        {
            string response = "";
            var newFileName = "";
            string db_path = "";

            var _staff = (from s in _context.Staff where s.StaffEmail == Email select s);
          
             if (_staff.Count() > 0)
            {
                response = "This staff already exits.";
            }
            else
            {

                

                    if (StaffSignature != null)
                {
                    if (StaffSignature.Length > 0)
                    {
                        var randoneGuid = generalClass.Generate_Receipt_Number();
                        string extention = Path.GetFileName(StaffSignature.FileName);
                        newFileName = randoneGuid + "_" + extention;
                        

                        var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var uploadsFolder = Path.Combine(up, "images/Signature");
                        
                        string filePath = Path.Combine(uploadsFolder, newFileName);

                        db_path = "~/images/Signature/" + newFileName;

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            StaffSignature.CopyTo(fileStream);
                        }
                    }
                }

                Models.Staff staff = new Models.Staff()
                {
                    StaffElpsID = ElpsHashID.Trim(),
                    FieldOfficeID = FieldOfficeID,
                    RoleID = RoleId,
                    StaffEmail = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    CreatedAt = DateTime.UtcNow.AddHours(1),
                    ActiveStatus = true,
                    DeleteStatus = false,
                    LocationID = LocationID,
                    Theme = "Light",
                    SignaturePath = db_path,
                    SignatureName = newFileName,
                    CreatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID)))
                };

                _context.Staff.Add(staff);
                int saved = _context.SaveChanges();

                if (saved > 0)
                {


                    var office = (from f in _context.FieldOffices where f.FieldOffice_id == FieldOfficeID select f).FirstOrDefault();
                    string location = office.OfficeName.ToLower().Contains("head office") ? "HQ" : "FO";
                    var loc = (from l in _context.Location where l.LocationName == location select l).FirstOrDefault();
                    staff.LocationID = loc?.LocationID;
                    _context.SaveChanges();


                    response = "Staff Created";
                }
                else
                {
                    response = "Staff not created. Try again later.";
                }
            }

            _helpersController.LogMessages("Creating new staff. Status : " + response + " for staff with email : " + Email, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

            return Json(response);
        }



        /*
         * Getting list of staff record on local system
         */
        //[Authorize(Policy = "ADRMRoles")]
        public JsonResult GetStaffRecord()
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

            var getStaff = from s in _context.Staff.AsEnumerable()
                           //join cs in _context.Staff.AsEnumerable() on s.CreatedBy equals cs.StaffID into cstaff
                           //from cst in cstaff.DefaultIfEmpty()
                           join r in _context.UserRoles.AsEnumerable() on s.RoleID equals r.Role_id into role
                           from ro in role.DefaultIfEmpty()
                           join f in _context.FieldOffices.AsEnumerable() on s.FieldOfficeID equals f.FieldOffice_id into field
                           from fd in field.DefaultIfEmpty()
                           join l in _context.Location.AsEnumerable() on s.LocationID equals l.LocationID into location
                           from lo in location.DefaultIfEmpty()
                           where s.DeleteStatus == false
                           select new
                           {
                               StaffID = s.StaffID,
                               FirstName = s.FirstName!= null ? s.FirstName:" ",
                               LastName = s.LastName!= null ? s.LastName:" ",
                               StaffEmail = s.StaffEmail,
                               FieldOffice = fd == null ? "" : fd.OfficeName,
                               Role = ro == null ? "" : ro.RoleName,
                               ActiveStatus = s.ActiveStatus == true ? "Activated" : "Deactivated",
                               CreatedAt =s.CreatedAt !=null? s.CreatedAt.ToString() : "NA",
                               LocationName = lo == null ? "" : lo.LocationName,
                               Signature =s.SignatureName==null?"NA": s.SignatureName,
                               //UpdatedAt = s.UpdatedAt.ToString(),
                               //CreatedBy = cst == null ? "" : cst.FirstName + " " + cst.LastName,
                               deskPath= "/Deskes/StaffDeskApps/" + generalClass.Encrypt( s.StaffID.ToString() ),
                               deskCount = _context.MyDesk.Where(x => x.StaffID == s.StaffID && x.HasWork == false).AsEnumerable().GroupBy(x => x.AppId).Count(),
                               SignaturePath = s.SignaturePath == null ?"NA" : s.SignaturePath
                           };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getStaff = sortColumn == "firstName" ? getStaff.OrderByDescending(c => c.FirstName) :
                               sortColumn == "lastName" ? getStaff.OrderByDescending(c => c.LastName) :
                               sortColumn == "staffEmail" ? getStaff.OrderByDescending(c => c.StaffEmail) :
                               sortColumn == "fieldOffice" ? getStaff.OrderByDescending(c => c.FieldOffice) :
                               sortColumn == "role" ? getStaff.OrderByDescending(c => c.Role) :

                               sortColumn == "createdAt" ? getStaff.OrderByDescending(c => c.CreatedAt) :

                               getStaff.OrderByDescending(c => c.StaffID + " " + sortColumnDir);
                }
                else
                {
                    getStaff = sortColumn == "firstName" ? getStaff.OrderBy(c => c.FirstName) :
                               sortColumn == "lastName" ? getStaff.OrderBy(c => c.LastName) :
                               sortColumn == "staffEmail" ? getStaff.OrderBy(c => c.StaffEmail) :
                               sortColumn == "fieldOffice" ? getStaff.OrderBy(c => c.FieldOffice) :
                               sortColumn == "role" ? getStaff.OrderBy(c => c.Role) :

                               sortColumn == "createdAt" ? getStaff.OrderBy(c => c.CreatedAt) :

                               getStaff.OrderBy(c => c.StaffID + " " + sortColumnDir);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getStaff = getStaff.Where(c => c.Role.ToUpper().Contains(txtSearch.ToUpper()) || c.FieldOffice.ToUpper().Contains(txtSearch.ToUpper()) || c.StaffEmail.ToUpper().Contains(txtSearch.ToUpper()) || c.FirstName.ToUpper().Contains(txtSearch.ToUpper()) || c.LastName.ToUpper().Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch.ToString()));
            }

            var totalRecord = getStaff.ToList();

            totalRecords = totalRecord.Count();
            var data = getStaff.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all staff records...", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });
        }





        /*
         * Editing local staff information
         */
        //[Authorize(Policy = "AdminRoles")]
        public JsonResult Editstaff(int StaffID, int RoleId, int OfficeID, string FirstName, string LastName, int LocationID, IFormFile StaffSignature)
        {
            string response = "";
            var newFileName = "";
            string db_path = "";

            var _staff = (from s in _context.Staff where s.StaffID == StaffID && s.DeleteStatus == false select s);
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            if (_staff.Count() > 0)
            {

                var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var rootFolder = Path.Combine(up, "images/Signature");

                var signatureName = _staff.FirstOrDefault().SignatureName == null ? "xxx" : _staff.FirstOrDefault().SignatureName;
                string deletePath = Path.Combine(rootFolder, signatureName);

                if (System.IO.File.Exists(deletePath))
                {
                    System.IO.File.Delete(deletePath);
                }

                if (StaffSignature != null)
                {
                    if (StaffSignature.Length > 0)
                    {
                        var randoneGuid = generalClass.Generate_Receipt_Number();
                        string extention = Path.GetFileName(StaffSignature.FileName);
                        newFileName = randoneGuid + "_" + extention;
                        var uploadsFolder = Path.Combine(up, "images/Signature");
                        string filePath = Path.Combine(rootFolder, newFileName);

                        db_path = "~/images/Signature/" + newFileName;

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            StaffSignature.CopyTo(fileStream);
                        }
                    }
                }

                var mydesk = _context.MyDesk.Where(a => a.StaffID == _staff.FirstOrDefault().StaffID && a.HasWork != true).OrderByDescending(a => a.Sort).FirstOrDefault();
                int i = 0;
                if (userRole!= GeneralClass.SUPER_ADMIN && mydesk != null &&  (_staff.FirstOrDefault().RoleID != RoleId || _staff.FirstOrDefault().FieldOfficeID != OfficeID || _staff.FirstOrDefault().LocationID != LocationID))
                {
                    //There is an attempt to change staff role/office/location but staff have pending application(s) on desk
                    i = 1;
                }
                else
                {
                    
                    _staff.FirstOrDefault().RoleID = RoleId;
                    _staff.FirstOrDefault().FieldOfficeID = OfficeID;

                    var office = (from f in _context.FieldOffices where f.FieldOffice_id == OfficeID select f).FirstOrDefault();
                    string location = office.OfficeName.ToLower().Contains("head office") ? "HQ" : "FO";
                    var loc = (from l in _context.Location where l.LocationName == location select l).FirstOrDefault();
                    _staff.FirstOrDefault().LocationID = loc?.LocationID;

                }
                _staff.FirstOrDefault().FirstName = FirstName.ToUpper();
                _staff.FirstOrDefault().LastName = LastName.ToUpper();
                _staff.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                _staff.FirstOrDefault().DeleteStatus = false;
                _staff.FirstOrDefault().SignatureName = newFileName;
                _staff.FirstOrDefault().SignaturePath = db_path;
                _staff.FirstOrDefault().UpdatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID)));

                int updated = _context.SaveChanges();

                if (updated > 0)
                {
                    response = i > 0? "Staff profile has been updated except role/office, kindly move application(s) on staff desk before changing staff role/office. " : "Staff Updated";
                }
                else
                {
                    response = "Nothing was updated. Try again!";
                }
            }
            else
            {
                response = "The selected staff was not found.";
            }

            _helpersController.LogMessages("Updating staff details. Status : " + response + " Staff ID : " + StaffID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

            return Json(response);
        }



        /*
         * Deactivating a staff
         */
        //[Authorize(Policy = "AdminRoles")]
        public JsonResult DeactivateStaff(int StaffID, string Status)
        {
            bool status = Status.Trim() == "Activated" ? true : false;

            string response = "";

            var _staff = from s in _context.Staff where s.StaffID == StaffID && s.DeleteStatus == false select s;

            if (_staff.Count() > 0)
            {


                var checkstaffdesk = from s in _context.MyDesk where s.StaffID == StaffID && s.HasWork != true select s;
                if (checkstaffdesk.Count() > 0 && Status.Contains("Deactivate"))
                {
                    response = "Sorry, you can't " + Status + " this staff because there are pending jobs on staff desk. Kindly re-route jobs on staff desk to another staff.";
                }

                else
                {
                    _staff.FirstOrDefault().ActiveStatus = status;
                    _staff.FirstOrDefault().UpdatedAt = DateTime.Now;
                    _staff.FirstOrDefault().UpdatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID)));


                    int done = _context.SaveChanges();

                    if (done > 0)
                    {
                        response = "Done";
                    }
                    else
                    {
                        response = "Something went wrong trying to " + Status + " this staff. Try again.";
                    }
                }
            
                _helpersController.LogMessages("Deactivating Staff. Status : " + response + " Staff ID : " + StaffID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

            }
            else
            {
                response = "This staff was not found.";
            }
            return Json(response);

        }

        /*
         * Removing Staff
         */
        //[Authorize(Policy = "AdminRoles")]
        public JsonResult RemoveStaff(int StaffID)
        {
            string response = "";
            var _staff = from s in _context.Staff where s.StaffID == StaffID && s.DeleteStatus == false select s;

            if (_staff.Count() > 0)
            {
                var checkstaffdesk = from s in _context.MyDesk where s.StaffID == StaffID && s.HasWork != true select s;
                if (checkstaffdesk.Count() > 0)
                {
                    response = "Sorry, you can't delete this staff because there are pending jobs on staff desk. Kindly re-route jobs on staff desk to another staff.";
                }

                else
                {

                    _staff.FirstOrDefault().ActiveStatus = false;
                    _staff.FirstOrDefault().DeleteStatus = true;
                    _staff.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID)));
                    _staff.FirstOrDefault().DeletedAt = DateTime.UtcNow.AddHours(1);

                    int done = _context.SaveChanges();

                    if (done > 0)
                    {
                        response = "Staff Removed";
                    }
                    else
                    {
                        response = "Something went wron trying to remove this staff. Try again.";
                    }
                }
            }
            else
            {
                response = "This staff was not found.";
            }

            _helpersController.LogMessages("Removing Staff. Status : " + response + " Staff ID : " + StaffID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

            return Json(response);
        }




        //[Authorize(Policy = "ADRMRoles")]
        public IActionResult StaffLogins(int id)
        {
            var login = from l in _context.Logins
                        join r in _context.UserRoles on l.RoleID equals r.Role_id
                        join s in _context.Staff on l.UserID equals s.StaffID
                        join f in _context.FieldOffices on s.FieldOfficeID equals f.FieldOffice_id
                        where r.RoleName!= GeneralClass.COMPANY
                        select new UserLogins
                        {
                            ID = s.StaffID,
                            Name = s.LastName + " " + s.FirstName,
                            Email = s.StaffEmail,
                            Role = r.RoleName,
                            HostName = l.HostName,
                            FieldOffice = f.OfficeName,
                            MacAddress = l.MacAddress,
                            LocalIp = l.Local_Ip,
                            RemoteIp = l.Remote_Ip,
                            UserAgent = l.UserAgent,
                            Status = l.LoginStatus,
                            LogInTime = l.LoginTime,
                            LogOutTime = l.LogoutTime
                        };

            ViewData["LoginTitle"] = "All Staff Logins";

            if (login.Count() > 0)
            {
                if (id != 0)
                {
                    login = login.Where(x => x.ID == id);
                    ViewData["LoginTitle"] = "All Logins for " + login.FirstOrDefault()?.Name;
                }
                _helpersController.LogMessages("Displaying " + ViewData["LoginTitle"], generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

                return View(login.AsEnumerable());
            }
            else
            {
                return View(login.AsEnumerable());
            }
        }


        /*
         * Displaying all acctivities done by all or specific user.
         */
        //[Authorize(Policy = "ADRMRoles")]
        public IActionResult Activities(string id)
        {
            var act = _context.AuditLogs.OrderByDescending(x => x.EventDateUTC).ToList();
            ViewData["ActivityTitle"] = "Activity for all users";

            if (id != null)
            {
                act = act.Where(x => x.UserId == id).ToList();
                ViewData["ActivityTitle"] = "Activity for " + id;
            }

            return View(act);
        }




        //[Authorize(Policy = "ADRMRoles")]
        public IActionResult Company()
        {
            var company = _context.companies.ToList();
            _helpersController.LogMessages("Showing all companies profile", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return View(company);
        }

      
        /*
        * An action to perform company activation, remove, restore or deactivation
        */
        //[Authorize(
        //")]
        public JsonResult CompanyAction(string CompID, string option, string response)
        {
            string result = "";

            if (string.IsNullOrWhiteSpace(CompID))
            {
                result = "Error, Company link is broken or not in correct format.";
            }

            int compid = 0;
            var comp_id = generalClass.Decrypt(CompID);

            if (comp_id == "Error")
            {
                result = "Error, Company link is broken or not in correct format.";
            }
            else
            {
                compid = Convert.ToInt32(comp_id);

                var company = _context.companies.Where(x => x.id == compid);
                var getLoginParty = _context.companies.Where(x => x.name == company.FirstOrDefault().name);
                if (company.Count() > 0)
                {
                    if (option == "activate")
                    {
                        company.FirstOrDefault().ActiveStatus = true;
                        company.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                        if (getLoginParty.Count() > 0)
                        {
                            getLoginParty.FirstOrDefault().ActiveStatus = true;
                            getLoginParty.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                        }
                    }
                    else if (option == "deactivate")
                    {
                        company.FirstOrDefault().ActiveStatus = false;
                        company.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                        if (getLoginParty.Count() > 0)
                        {
                            getLoginParty.FirstOrDefault().ActiveStatus = false;
                            getLoginParty.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                        }
                    }
                    else if (option == "delete")
                    {
                        company.FirstOrDefault().DeleteStatus = true;
                        company.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));
                        company.FirstOrDefault().DeletedAt = DateTime.UtcNow.AddHours(1);
                        company.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                        if (getLoginParty.Count() > 0)
                        {
                            getLoginParty.FirstOrDefault().DeleteStatus = true;
                            getLoginParty.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));
                            getLoginParty.FirstOrDefault().DeletedAt = DateTime.UtcNow.AddHours(1);
                            getLoginParty.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                            getLoginParty.FirstOrDefault().ActiveStatus = true;
                        }
                    }
                    else if (option == "restore")
                    {
                        company.FirstOrDefault().DeleteStatus = false;
                        company.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                        if (getLoginParty.Count() > 0)
                        {
                            getLoginParty.FirstOrDefault().DeleteStatus = false;
                            getLoginParty.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);

                        }
                    }
                    else
                    {
                        result = "The option entered does not match the current operation. Please contact support for this operation.";
                    }

                    if (_context.SaveChanges() > 0)
                    {
                        result = response;
                    }
                    else
                    {
                        result = "Something went wrong tying to perform current operation, please try again later.";
                    }
                }
                else
                {
                    result = "Something went wrong. Company cannot be found.";
                }
            }

            _helpersController.LogMessages("Company Actiion : " + option + " Status : " + result + " Company ID : " + compid, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
            return Json(result);
        }

        
        //[Authorize(Policy = "ADRMRoles")]
        public IActionResult CompanyLogins(int id)
          {
            var login = from l in _context.Logins
                        join r in _context.UserRoles on l.RoleID equals r.Role_id
                        join c in _context.companies on l.UserID equals c.id
                        where r.RoleName == GeneralClass.COMPANY
                        select new UserLogins
                        {
                            ID = c.id,
                            Name = c.name,
                            Email = c.CompanyEmail,
                            Role = r.RoleName,
                            HostName = l.HostName,
                            MacAddress = l.MacAddress,
                            LocalIp = l.Local_Ip,
                            RemoteIp = l.Remote_Ip,
                            UserAgent = l.UserAgent,
                            Status = l.LoginStatus,
                            LogInTime = l.LoginTime,
                            LogOutTime = (DateTime)l.LogoutTime
                        };

            ViewData["LoginTitle"] = "All Company's Logins";

            if (login.Count() > 0)
            {
                if (id != 0)
                {
                    login = login.Where(x => x.ID == id);
                    ViewData["LoginTitle"] = "All Logins for " + login.FirstOrDefault().Name;
                }
                _helpersController.LogMessages("Displaying " + ViewData["LoginTitle"], generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
                return View(login.ToList());
            }
            else
            {
                return View();
            }
        }




        /*
        * To check if user is still logged in
        */
        public JsonResult CheckSession()
        {
            try
            {
                string result = "";

                if (string.IsNullOrWhiteSpace(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)))
                {
                    result = "true";
                }
                return Json(result);

            }
            catch (Exception ex)
            {
                return Json("true");
            }
        }

    }


}