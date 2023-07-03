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
   [Route("api")]
   [ApiController]
    public class InspectionsController : Controller
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

        public InspectionsController(IHostingEnvironment hostingEnvironment, Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }


        // GET: Inspections
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ApplicationForm(int id)
        {
            var af = _context.ApplicationForms.Where(a => a.ApplicationId == id && a.Filled).FirstOrDefault();
          
            if (af != null)
            {
                switch (af.FormId)
                {
                    case "FacilityConditions":
                        return RedirectToAction("ViewFacilityCondition", new { id = id });

                    case "TankLeakageTests":
                        return RedirectToAction("ViewTankLeakageTest", new { id = id });
                        
                    default:
                        int fid = 0;
                        int.TryParse(af.FormId, out fid);
                        var frm = _context.Forms.Where(a => a.Id == fid).FirstOrDefault();
                        ViewBag.Form = frm.FriendlyName;
                        Guid gid = af.ValGroupId.GetValueOrDefault();
                        var fvs = _context.FieldValues.Where(a => a.FormId == fid && a.GroupId == gid).ToList();
                        var application = _context.applications.Where(a => a.id == af.ApplicationId).FirstOrDefault();
                        var company = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
                        var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();

                        ViewBag.CompanyName = company.name;
                        ViewBag.FacilityName = facility.Name;
                        ViewBag.AppFormId = id;
                        ViewBag.Recommend = af.Recommend;
                        ViewBag.Reasons = af.Reasons;
                        ViewBag.StaffName = af.StaffName;
                        ViewBag.AppForm = af;
                        List<Fields> formFields =_context.Fields.Where(C => C.FormId == fid).ToList();
                        var formV = fvs.FirstOrDefault();

                        foreach (var item in formFields)
                        {
                            formV = fvs.FirstOrDefault(a => a.FieldId == item.Id);
                            //Assign Value if it exist.
                            if (formV != null) { 
                             item.hiddenValue = formV.Value;
                        }
                        }
                        return View(formFields);
                }
            }
            ViewBag.Error = "Item does not Exist";
            return View("Error");
        }
        public IActionResult ViewInspectionForm(string id)
        {
          int appid = generalClass.DecryptIDs(id.ToString().Trim());

            var af = _context.ApplicationForms.Where(a => a.ApplicationId == appid && a.Filled).FirstOrDefault();

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
                        ViewBag.Form = frm.FriendlyName;
                        Guid gid = af.ValGroupId.GetValueOrDefault();
                        var fvs = _context.FieldValues.Where(a => a.FormId == fid && a.GroupId == gid).ToList();
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
                                     where t.FacilityId== facility.Id
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
                        var inspectionData = (from afm in _context.ApplicationForms
                                              join fv in _context.FieldValues on afm.ApplicationId equals fv.ApplicationId
                                              join ap in _context.applications on afm.ApplicationId equals ap.id
                                              join comp in _context.companies on ap.company_id equals comp.id
                                              join fac in _context.Facilities.AsEnumerable() on ap.FacilityId equals fac.Id
                                              join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                                              join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                                              join c in _context.Categories on ap.category_id equals c.id
                                              join p in _context.Phases on ap.PhaseId equals p.id
                                              join m in _context.MeetingSchedules on ap.id equals m.ApplicationId
                                              where afm.ApplicationId == appid
                                              select new InspectionDataModel
                                              {
                                                  AppTanks = apTanks.Count()> 0? apTanks: Tanks,
                                                  Fields = _context.Fields.Where(f => f.FormId.ToString() == afm.FormId).ToList(),
                                                  FormCreatedAt = Convert.ToDateTime(m.MeetingDate),
                                                  appForm = afm,
                                                  appID = ap.id,
                                                  Reference = ap.reference,
                                                  CategoryName = c.name,
                                                  PhaseName = p.name,
                                                  FacilityAddrss=ad.address_1,
                                                  Type = ap.type.ToUpper(),
                                                  Status = ap.status,
                                                  Date_Added = Convert.ToDateTime(ap.date_added),
                                                  DateSubmitted = Convert.ToDateTime(ap.CreatedAt),
                                                  CompanyDetails = comp.name + " (" + comp.Address + ") ",
                                                  CompanyName = comp.name,
                                                  Company_Id = c.id,
                                                  FacilityDetails = fac.Name,
                                                  FacilityName = fac.Name,
                                                  StaffName=m.StaffUserName
                                              });
                        inspectionData.FirstOrDefault().appForm.ExtraReport1 = af.ExtraReport1;
                        inspectionData.FirstOrDefault().appForm.ExtraReport2 = af.ExtraReport2;
                        var StaffCommentField = inspectionData.FirstOrDefault().Fields.Where(x => x.Label.ToLower().Contains("comment")).FirstOrDefault();
                        if(StaffCommentField != null)
                        {
                            ViewBag.StaffComment = inspectionData.FirstOrDefault().FieldValues.Where(x => x.FieldId == StaffCommentField.Id).FirstOrDefault().Value;
                        }
                      

                        var staff = _context.Staff.Where(a => a.StaffEmail == inspectionData.FirstOrDefault().StaffName).FirstOrDefault();

                        ViewBag.StaffOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == staff.FieldOfficeID).FirstOrDefault().OfficeName;
                        ViewBag.StaffRole = _context.UserRoles.Where(x => x.Role_id == staff.RoleID).FirstOrDefault().RoleName;
                        ViewData["AppRefNo"] = inspectionData.FirstOrDefault().Reference;
                        return View(inspectionData);

                }
            }
                ViewBag.Error = "Item does not Exist";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, application form does not exist.") });
            
        }
        public IActionResult FilledInspectionForm(int id)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (userName == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again later") });
            }

            var af = _context.ApplicationForms.Where(a => a.ApplicationId == id && a.Filled).ToList();
            if (af != null && af.Count > 0)
            {

                var application = _context.applications.Where(a => a.id == af.FirstOrDefault().ApplicationId).FirstOrDefault();
                _helpersController.LogMessages(userEmail + " viewed inspection form for application with ref: " + application.reference, userID.ToString());
                return View(af);

            }

            return View();
        }
        #region Smart Inspector API [HttpGet]
        [Route("GetSingleInspection")]
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetSingleInspection(string email, string applicationid)
        {
           
            List<ApplicationDetailsModel> AllAppInfo = new List<ApplicationDetailsModel>();
            DateTime? tdate = DateTime.Now;
            var getApplication = (from app in _context.applications
                                  where app.id.ToString() == applicationid.Trim() && app.DeleteStatus != true && app.company_id > 0
                                     select app).FirstOrDefault();
            if (getApplication != null)
            {

                var CheckInspection = (from u in _context.MeetingSchedules
                                       join appform in _context.ApplicationForms on u.ApplicationId equals appform.ApplicationId
                                       join app in _context.applications on u.ApplicationId equals app.id
                                       join stf in _context.Staff on u.StaffUserName.ToLower() equals stf.StaffEmail.ToLower()
                                       where u.StaffUserName.ToLower() == email.ToLower() && u.Accepted == true && appform.ApplicationId.ToString() == applicationid
                                       && u.Completed != true && app.DeleteStatus != true && app.company_id > 0

                                       select new
                                       {
                                           StaffID=stf.StaffEmail,
                                           AppRef=app.reference,
                                           FormId = appform.FormId,
                                           StaffUserName = u.StaffUserName,
                                           MeetingDate = u.MeetingDate,
                                           ApplicationId = u.ApplicationId,
                                           AD = u.AcceptanceDate
                                       });


                if (CheckInspection.Count() > 0)
                {
                    
                        var pendingInspections = _context.ApplicationForms.Where(a => a.ApplicationId.ToString() == applicationid && a.Filled != true);

                        if (pendingInspections.Count() > 0)
                        {
                            foreach (var app in pendingInspections)
                            {

                                var appDetails = _context.ApplicationForms.Where(a => a.ApplicationId == app.ApplicationId).FirstOrDefault();

                                if (appDetails != null)
                                {
                                   
                                    var application = _context.applications.Where(a => a.id == app.ApplicationId).FirstOrDefault();
                                    var companydetails = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
                                    var facDetails = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                                    var addressdetails = _context.addresses.Where(a => a.id == facDetails.AddressId).FirstOrDefault();
                                    var meetingdetails = _context.MeetingSchedules.Where(a => a.ApplicationId == appDetails.Id && a.StaffUserName == app.StaffName).FirstOrDefault();

                                    if (facDetails != null)
                                    {

                                        var result = new ApplicationDetailsModel
                                        {
                                            ApplicationId = appDetails.ApplicationId,
                                            CompanyId = facDetails.CompanyId,
                                            FacilityId = facDetails.Id,
                                            FacilityLocation = addressdetails.address_1,
                                            CompanyName = companydetails.name,
                                            ApplicationType = application.type.ToUpper(),
                                            FacilityName = facDetails.Name,
                                            InspectorEmail = app.StaffName,
                                            InspectionDate = CheckInspection.FirstOrDefault().MeetingDate,
                                            InspectionContactName = facDetails.ContactName,
                                            InspectionContactNumber = facDetails.ContactNumber,
                                            FormId = app.FormId,
                                            FormType = app.FormId == "1" ? "SITE SUITABILITY" : (app.FormId == "3" ? "LTO / TO/ LICENSE RENEWAL" : "CALIBRATION//RECALIBRATION")
                                        };

                                        AllAppInfo.Add(result);

                                        //var inspectorInfo = _context.Staff.Where(x => x.StaffEmail.ToLower() == getSchedules.StaffUserName.ToLower()).FirstOrDefault();

                                    }
                                    else
                                    {
                                        return Json(new { success = false, message = "Sorry, facility details could not be found.", totalRecords = 0 });
                                    }
                                }
                                else
                                {
                                    return Json(new { success = false, message = "Sorry, application details wasn't found.", totalRecords = 0 });
                                }
                            }
                            int recordsFiltered = AllAppInfo.Count();
                            var datas = AllAppInfo.ToList();

                            _helpersController.LogMessages(email + " fetched inspection form for application with ref: " + CheckInspection.FirstOrDefault().AppRef + " on Smart Inspector.", email);

                            if (AllAppInfo.Count() > 0)
                            {
                                return Json(new { success = true, message = "", totalRecords = recordsFiltered, data = datas });
                            }
                            else
                            {
                                return Json(new { success = false, message = "Sorry, no record was found.", totalRecords = recordsFiltered, data = datas });
                            }

                        }
                        else
                        {
                            return Json(new { success = false, message = "Sorry, no pending inspection found for this application.", totalRecords = 0 });
                        }

                    
                }
                else
                {
                    return Json(new { success = false, message = "Sorry, no pending inspection found for this application.", totalRecords = 0 });
                }

            }
            else
            {
                return Json(new { success = false, message = "Sorry, this application does not exist.", totalRecords = 0 });
            }

        }
        /*
       * API For SmartInspector
       * 
       * Get all pending inspection(s) for a particular user.
       * 
       * email => user email address
       * token => encrypted string token
       * 
       */
        [Route("GetAllPendingInspections")]
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetAllPendingInspections(string email, string formid)
        {
            try
            {
              
                DateTime? tdate = DateTime.Now;

                string returnMessage = "";
                List<ApplicationDetailsModel> AllAppInfo = new List<ApplicationDetailsModel>();
                  var getSchedules = (from u in _context.MeetingSchedules
                                         join appform in _context.ApplicationForms on u.ApplicationId equals appform.ApplicationId
                                         join app in _context.applications on u.ApplicationId equals app.id
                                         where u.Accepted == true && u.Completed != true && app.DeleteStatus!= true && app.company_id > 0
                                         select u).FirstOrDefault();
                if (getSchedules != null)
                {

                    var CheckInspection = (from u in _context.MeetingSchedules
                                           join appform in _context.ApplicationForms on u.ApplicationId equals appform.ApplicationId
                                           join app in _context.applications on u.ApplicationId equals app.id
                                           where u.Accepted == true && u.Completed != true && app.DeleteStatus != true && app.company_id > 0
                                           select new
                                           {
                                               FormId = appform.FormId,
                                               StaffUserName = u.StaffUserName,
                                               MeetingDate = u.MeetingDate,
                                               ApplicationId = u.ApplicationId,
                                               AD = u.AcceptanceDate
                                           }).ToList();

                    returnMessage = "Sorry, there is no pending inspection found.";

                    if (formid != null && email != null)
                    {
                        CheckInspection = CheckInspection.Where(u => u.StaffUserName.ToLower() == email.ToLower().Trim() && u.FormId == formid.Trim()).ToList();

                        returnMessage = "Sorry, there is no pending inspection found for this staff.";
                    }
                    else if (formid != null)
                    {
                        CheckInspection = CheckInspection.Where(u => u.FormId == formid.Trim()).ToList();

                        returnMessage = "Sorry, there is no pending inspection found for this form type.";

                    }
                    else if (email != null)
                    {
                        CheckInspection = CheckInspection.Where(u => u.StaffUserName.ToLower() == email.ToLower().Trim()).ToList();

                        returnMessage = "No Pending Inspection(s) Found For This Staff";
                    }

                    
                    
                    if (CheckInspection.Count() > 0)
                    {
                      
                        foreach (var app in CheckInspection)
                        {

                            var appDetails = _context.ApplicationForms.Where(a => a.ApplicationId == app.ApplicationId).FirstOrDefault();

                            if (appDetails != null)
                            {

                                var application = _context.applications.Where(a => a.id == app.ApplicationId).FirstOrDefault();
                                var companydetails = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
                                var facDetails = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                                var meetingdetails = _context.MeetingSchedules.Where(a => a.ApplicationId == appDetails.Id && a.StaffUserName.ToLower() == app.StaffUserName.ToLower().Trim()).FirstOrDefault();

                                if (facDetails != null)
                                {
                                    var addressdetails = _context.addresses.Where(a => a.id == facDetails.AddressId).FirstOrDefault();

                                    var result = new ApplicationDetailsModel
                                    {
                                        ApplicationId = appDetails.ApplicationId,
                                        CompanyId = facDetails.CompanyId,
                                        FacilityId = facDetails.Id,
                                        FacilityLocation = addressdetails.address_1,
                                        CompanyName = companydetails.name,
                                        ApplicationType = application.type.ToUpper(),
                                        FacilityName = facDetails.Name,
                                        InspectorEmail = app.StaffUserName,
                                        InspectionDate = app.MeetingDate != null ? (DateTime)app.MeetingDate : DateTime.Now.AddDays(1),
                                        InspectionContactName = facDetails.ContactName,
                                        InspectionContactNumber = facDetails.ContactNumber,
                                        FormId = app.FormId,
                                        FormType = app.FormId == "1" ? "SITE SUITABILITY" : (app.FormId == "3" ? "LTO/TO/LICENSE RENEWAL" : "CALIBRATION/RECALIBRATION")
                                    };

                                    AllAppInfo.Add(result);

                                }
                            }
                        }
                        int recordsFiltered = AllAppInfo.Count();
                        var datas = AllAppInfo.ToList();

                        if (AllAppInfo.Count() > 0)
                        {
                            return Json(new { success = true, message = "", totalRecords = recordsFiltered, data = datas });
                        }
                        else
                        {
                            return Json(new { success = false, message = "No Record Found", totalRecords = recordsFiltered, data = datas });
                        }

                    }

                    else
                    {
                        return Json(new { success = false, message = returnMessage });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Sorry, there is no pending inspection." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }



        //save inspection form for Smart Inspector
        [Route("SaveInspectionForm")]
        [HttpPost]
        [AllowAnonymous]
        public JsonResult SaveInspectionForm(AppFormApiSubmitModel model) { 
       
                string result = ""; int save = 0;
                var respObj = new List<object>();
                var app = _context.applications.Where(a => a.id == model.ApplicationId).FirstOrDefault();
                var comp = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
                var facilitydetails = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var addressdetails = _context.addresses.Where(a => a.id == facilitydetails.AddressId).FirstOrDefault();
                var stagedetails = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();

            var frm = _context.Forms.Where(a => a.Id == model.FormId).FirstOrDefault();
            ViewBag.Form = frm.FriendlyName;
            var appForm = _context.ApplicationForms.Where(a => a.FormId == model.FormId.ToString() && a.ApplicationId == model.ApplicationId ).FirstOrDefault();

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
                                ApplicationId= model.ApplicationId,
                                Value = item.Value
                            };
                            listOfFldValue.Add(fieldV);
                            _context.FieldValues.Add(fieldV);
                        }
                        else
                        {
                            fieldV.Value = item.Value;
                            fieldV.ApplicationId= model.ApplicationId;
                            listOfFldValue.Add(fieldV);
                        }

                       save += _context.SaveChanges();
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
                    var appInspectionDetails = _context.MeetingSchedules.Where(a =>  a.StaffUserName == model.InspectorEmail && a.ApplicationId == model.ApplicationId).OrderByDescending(a => a.Date).FirstOrDefault();
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

                    if (emailMsg.Count() > 0)
                    {
                        var sendEmail = _helpersController.SendEmailMessage2Staff(stf.StaffEmail, stf.FirstName, emailMsg, null);
                    }
                   
                    #endregion
                    result = "Inspection form saved successfully";
                    return Json(new { success = true, message = result });
                }
                #endregion



                else
                {
                    result = "An error occured while saving this inspection form.";
                    return Json(new { success = false, message = result });

                }
            }

            else
            {
                result = "Sorry, application form was not found.";
                return Json(new { success = false, message = result });

            }

        }




        /*
         * API for SmartInspector
         * 
         * Saving ATC, LTO inspection forms
         * 
         * forms => A list of FormsInspection (List<InspectionForm>  and List<TankFarmInspection>)
         * appid => Application ID
         */
        /*
         * API for SmartInspector
         * 
         * Saving Site Suitability inspection forms
         * 
         * SiteForms => A list of SiteSuitability (List<SiteSuitability>) 
         * AppId => Application ID
         * 
         */


        #endregion

        public IActionResult SiteSuitability(string id)
        {

            if (!(string.IsNullOrWhiteSpace(id)))
            {

                int AppID = 0;

                var appID = id;


                AppID = Convert.ToInt32(appID);

                ViewData["AppID"] = AppID;


                var details = _context.SuitabilityInspections.Where(a => a.ApplicationId == AppID);

                if (details.Count() > 0)
                {

                    var appdetails = _context.applications.Where(a => a.id == AppID).FirstOrDefault();
                    var companydetails = _context.companies.Where(x => x.id == appdetails.company_id).FirstOrDefault();
                    var facilitydetails = _context.Facilities.Where(x => x.Id == appdetails.FacilityId).FirstOrDefault();
                    var addressdetails = _context.addresses.Where(a => a.id == facilitydetails.AddressId).FirstOrDefault();
                    //var meetingdetails = _context.MeetingSchedules.Where(a => a.ApplicationId == appdetails.id && a.StaffUserName == app.StaffName).FirstOrDefault();
                    var stagedetails = _context.Phases.Where(a => a.id == appdetails.PhaseId).FirstOrDefault();

                    if (appdetails != null && stagedetails != null && facilitydetails != null && companydetails != null && addressdetails != null)
                    {
                        ViewData["CompanyName"] = companydetails.name;
                        ViewData["RefNo"] = appdetails.reference;
                        ViewData["FacilityName"] = facilitydetails.Name;
                        ViewData["AppCategory"] = appdetails.type + " (" + stagedetails.name + ")";
                        ViewData["FacilityAddress"] = addressdetails.address_1;


                        _helpersController.LogMessages("Displaying site suitability informatioin for inspection", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
                    }
                    return View(details.ToList());

                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                return View("Error");
            }

        }



        #region DRMS-DEPOT SECTION
        [AllowAnonymous]
        public JsonResult DepotThroughput(int id)
        {
            var depot = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
            if (depot != null)
            {
                return Json(new { code = 1, throughPut = depot.MaxCapacity });
            }
            else
            {
                return Json(new { code = 0, throughPut = 0 });
            }
        }

        [AllowAnonymous, Route("Depots/{id}/{hash}")]
        public IActionResult Depot(string id, string hash)
        {
            string elpsSecretKey = _configuration.GetSection("ElpsKeys").GetSection("AppSecKey").Value.ToString();
            var hsh = PaymentRef.getHash(id.ToString() + elpsSecretKey);

            if (hsh == hash)
            {

                var facs = (from facil in _context.Facilities.AsEnumerable()
                            join c in _context.companies.AsEnumerable() on facil.CompanyId equals c.id
                            join ad in _context.addresses.AsEnumerable() on facil.AddressId equals ad.id
                            join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                            where c.CompanyEmail.ToLower() == id.ToLower()
                            select new FacilityModel
                            {
                                City = ad.city,
                                CompanyEmail = c.CompanyEmail,
                                ContactName = facil.ContactName,
                                ContactNumber = facil.ContactNumber,
                                Id = facil.Id,
                                Name = facil.Name,
                                NoOfPumps = _context.Pumps.Where(x => x.FacilityId == facil.Id).ToList().Count(),
                                NoOfTanks = _context.Tanks.Where(x => x.FacilityId == facil.Id).ToList().Count(),
                                StateName = sd.StateName,
                                Street = ad.address_1,
                                FacilityCode = facil.IdentificationCode,
                                LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,

                            }).ToList();

                return Json(facs);
            }

            else
            {
                return Json(new { status = "Sorry, provided hash key value does not match." });

            }

        }

        [AllowAnonymous, Route("Depots/Facility/{id:int}/{hash}")]
        public JsonResult DepotFacility(int id, string hash)
        {

            string elpsSecretKey = _configuration.GetSection("ElpsKeys").GetSection("AppSecKey").Value.ToString();
            var hsh = PaymentRef.getHash(id.ToString() + elpsSecretKey);

            if (hsh == hash)
            {

                var facs = (from facil in _context.Facilities.AsEnumerable()
                            join c in _context.companies.AsEnumerable() on facil.CompanyId equals c.id
                            join ad in _context.addresses.AsEnumerable() on facil.AddressId equals ad.id
                            join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                            where facil.Id == id
                            select new FacilityModel
                            {
                                City = ad.city,
                                CompanyEmail = c.CompanyEmail,
                                ContactName = facil.ContactName,
                                ContactNumber = facil.ContactNumber,
                                Id = facil.Id,
                                Name = facil.Name,
                                NoOfPumps = _context.Pumps.Where(x => x.FacilityId == facil.Id).ToList().Count(),
                                NoOfTanks = _context.Tanks.Where(x => x.FacilityId == facil.Id).ToList().Count(),
                                StateName = sd.StateName,
                                Street = ad.address_1,
                                FacilityCode = facil.IdentificationCode,
                                LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,

                            }).ToList();
                facs.ForEach(x =>
                {

                    var tnks = _context.Tanks.Where(a => a.FacilityId == id).ToList();
                    var tnkL = new List<TankModel>();
                    TankModel t = null;
                    foreach (var item in tnks)
                    {
                        t = new TankModel
                        {
                            Diameter = item.Diameter,
                            FacilityId = id,
                            Height = item.Height,
                            Id = item.Id,
                            MaxCapacity = item.MaxCapacity,
                            Name = item.Name,
                            ProductName = _context.Products.Where(x => x.Id == item.ProductId).FirstOrDefault()?.Name,
                        };
                        tnkL.Add(t);

                    }
                    x.Tanks = tnkL;
                    var pmps = _context.Pumps.Where(a => a.FacilityId == id).ToList();
                    var pmpL = new List<LoadingArmModel>();
                    LoadingArmModel l = null;
                    foreach (var item in pmps)
                    {
                        l = new LoadingArmModel
                        {
                            TankId = item.TankId,

                            Id = item.Id,
                            Name = item.Name
                        };
                        pmpL.Add(l);
                    }
                    x.LoadingArms = pmpL;
                });
                return Json(facs);
            }
            else
            {
                return Json(new { status = "Sorry, provided hash key value does not match." });

            }
        }

#endregion

    }
}
//var tok = generalClass.Decrypt(token); //UXpreVFqUTJRVFZDUlVVeVF3PT0=
//if (tok == GeneralClass.TOKEN)
//{ }
//else
//{
//    return Json(new { success = false, message = "Unauthorize User" });
//}

//var suitForm = new SuitabilityInspection()
//                    {
//                        CompanyId
//ApplicationId
//Fire & Gas Detection Systems
//Fire & Explosion Protection
//Safety Process (Implementation of Safety Studies)
//Fire Clearance Zone around Facility Fence
//Fire/Portable Water Facilities
//Safety Requirements
//Power Source
//Good health Protection & Promotion Programes
//Physical security Plan of Facility
//Number of Tanks vis-à-vis Capacity
//Testing of Tanks, Pipelines & Valves
//Requirements/Integrity of Bound walls
//Product Spill Contingency Plan Activation Records
//First Tier response for Product Spill
//Environmental Management Plan Implementation
//Environmental Management System Audit
//Waste Treatment & Monitoring Facilities
//Functional Laboratory
//Tanker Parking Lot
//Tanker Truck Ullaging Area
//General House Keeping / Grass Maintenance
//    ApplicationId = model.ApplicationId,
//    CompanyId = model.CompanyId,
//    SizeOfLand = model.SizeOfLand,
//    ISAlongPipeLine = model.ISAlongPipeLine,
//    IsOnHighWay = model.IsOnHighWay,
//    IsUnderHighTension = model.IsUnderHighTension,
//    DistanceFromExistingStation = model.DistanceFromExistingStation,
//    StationsWithin2KM = model.StationsWithin2KM,
//    FacilityId = model.FacilityId
//};
//_suitRep.Add(suitForm);
//_suitRep.Save(userEmail, Request.UserHostAddress);