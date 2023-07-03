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
using Rotativa.AspNetCore;
using System.Text;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
//using NewDepot.Controllers;
using Microsoft.AspNetCore.Http;
using LpgLicense.Models;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Controllers.Authentications;
using NewDepot.Controllers;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NewDepot.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        RestSharpServices _restService = new RestSharpServices();
        public IConfiguration _configuration;
        IHttpContextAccessor _httpContextAccessor;
        private readonly Depot_DBContext _context;
        SubmittedDocuments _appDocRep;
        ElpsResponse elpsResponse = new ElpsResponse();
        ApplicationHelper appHelper;
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;

        public ApplicationController(
            Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

            //newly added

        }

        public IActionResult index(int? id, int? apId, string s, string phase, string sDate, string eDate)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            ViewBag.phase = phase; ViewBag.sDate = sDate; ViewBag.eDate = eDate;
            var phases = _context.Phases.Where(a => a.name != null && a.DeleteStatus != true).OrderBy(a => a.name).Select(a => a.name).ToList();
            ViewBag.phases = phases;
            var applic = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true);

            List<string> Year = new List<string>();

            applic.ToList().ForEach(x =>
            {
                Year.Add(x.year.ToString());
            });
            ViewBag.Year = Year.Distinct();
            #region Admin Section
            //if ((userRole.Contains("Admin") || userRole.Contains("Support") || userRole.Contains("Account") || userRole.Contains("Staff")))
            if (!userRole.Contains("COMPANY"))
            {
                if (id == null)
                {
                    return View("AllApplications");
                }
                return RedirectToAction("ViewApplication", "Application", new { Id = id.ToString() });
            }
            #endregion
            #region Client Area
            else
            {
                if (id != null)
                {
                    var ap = (from app in _context.applications.AsEnumerable()
                              join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                              join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id
                              join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                              join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                              join ad in _context.addresses on facil.AddressId equals ad.id
                              join sb in _context.SubmittedDocuments.AsEnumerable() on app.id equals sb.AppID
                              where app.DeleteStatus != true && app.id == id && c.DeleteStatus != true
                              select new MyApps
                              {
                                  appID = app.id,
                                  Reference = app.reference,
                                  CategoryName = cat.name,
                                  PhaseName = phs.name,
                                  category_id = cat.id,
                                  FacilityId = facil.Id,
                                  PhaseId = phs.id,
                                  AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),
                                  Current_Permit = "",
                                  //Stage = s.StageName,
                                  Year = app.year,
                                  Address_1 = ad.address_1,
                                  Status = app.status,
                                  Date_Added = Convert.ToDateTime(app.date_added),
                                  DateSubmitted = Convert.ToDateTime(app.CreatedAt),
                                  Submitted = app.submitted,
                                  CompanyDetails = c.name,
                                  FacilityDetails = facil.Name,
                                  Type = app.type.ToUpper()
                              }).FirstOrDefault();
                    var appl = _context.applications.Where(a => a.id == id).FirstOrDefault();

                    if (ap != null && appl != null)
                    {
                        var comp = _context.companies.Where(a => a.CompanyEmail == userEmail).FirstOrDefault();
                        var fac = _context.Facilities.Where(a => a.Id == ap.FacilityId).FirstOrDefault();

                        var getDeskComment = _context.MyDesk.Where(x => x.AppId == id).AsEnumerable().LastOrDefault()?.Comment;

                        var appDocs = (from sd in _context.SubmittedDocuments
                                       join ad in _context.ApplicationDocuments on sd.AppDocID equals ad.AppDocID
                                       where sd.AppID == id && sd.DeletedStatus == false && ad.DeleteStatus != true
                                       && sd.CompElpsDocID != null
                                       select new AppDocuument
                                       {
                                           LocalDocID = sd.AppDocID,
                                           DocName = ad.DocName,
                                           EplsDocTypeID = ad.ElpsDocTypeID,
                                           CompanyDocID = (int)sd.CompElpsDocID,
                                           DocType = ad.docType,
                                           DocSource = sd.DocSource,
                                           isAddictional = (bool)sd.IsAdditional,
                                           DeletedStatus = sd.DeletedStatus
                                       }).ToList();
                        ViewBag.ApplicationDocs = appDocs;
                        ViewBag.DocsRemaining = appDocs.Where(a => a.DocSource == null && a.DeletedStatus != true).Count();

                        #region Tank Section
                        var tnks = new List<Tanks>();
                        var apTanks = (from t in _context.ApplicationTanks.AsEnumerable()
                                       join tk in _context.Tanks.AsEnumerable() on t.TankId equals tk.Id
                                       join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                                       where t.ApplicationId == appl.id
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
                                   where t.FacilityId == appl.FacilityId
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

                        _helpersController.LogMessages("Displaying application details for company to see. Application REF : " + appl.reference, _helpersController.getSessionEmail());

                        return View("AppDetail", ap);
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application View not found or not in correct format. Kindly contact support.") });
                    }

                }
                return RedirectToAction("MyApplications", new { id = id, s = s });
            }
            #endregion

        }


        [Authorize(Policy = "CompanyRoles")]
        public IActionResult MyApplications()
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (CompanyID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, company ID is incorrect. Please try again later") });
            }
            else
            {
                var company = _context.companies.Where(x => x.id == CompanyID);
                try
                {
                    var MyApps = (from app in _context.applications.AsEnumerable()
                                  join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                  join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                  join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                  join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                  join ad in _context.addresses on fac.AddressId equals ad.id
                                  //join sb in _context.SubmittedDocuments.AsEnumerable() on app.id equals sb.AppID
                                  // orderby app.id descending
                                  where app.DeleteStatus != true && c.id == CompanyID && c.DeleteStatus != true && app.isLegacy != true
                                  select new MyApps
                                  {

                                      RRR = _context.remita_transactions.Where(a => a.order_id == app.reference).FirstOrDefault()?.RRR,
                                      appID = app.id,
                                      Reference = app.reference,
                                      CategoryName = cat.name,
                                      PhaseName = phs.name,
                                      category_id = cat.id,
                                      FacilityId = fac.Id,
                                      PhaseId = phs.id,
                                      AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),
                                      //Stage = s.StageName,
                                      Address_1 = ad.address_1,
                                      Status = app.status,
                                      Date_Added = Convert.ToDateTime(app.date_added),
                                      DateSubmitted = Convert.ToDateTime(app.CreatedAt),
                                      Submitted = app.submitted,
                                      CompanyDetails = c.name + " (" + c.Address + ") ",
                                      FacilityDetails = fac.Name,
                                      Current_Permit = app.current_Permit
                                  });


                    _helpersController.LogMessages("Displaying company's application list...", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));
                    return View(MyApps.OrderByDescending(x => x.DateSubmitted).ToList());
                }
                catch (Exception e)
                {
                    return Json(e.Message);
                }
            }
        }

        public IActionResult ViewApp(string id)
        {
            if (generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName)) == "Error")
            {
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {
                var AppID = generalClass.Decrypt(id);
                int company_id = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID));
                List<ApplicationInformation> ApplicationInformation = new List<ApplicationInformation>();


                if (AppID == "Error" || company_id == 0)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, Application reference not passed. Please try again later") });
                }
                else
                {
                    var getCompany = _context.companies.Where(x => x.id == company_id && x.DeleteStatus != true).FirstOrDefault();

                    var getApp = _context.applications.Where(x => x.id.ToString() == AppID && x.DeleteStatus != true).FirstOrDefault();

                    if (getApp != null)
                    {

                        var getApp4Company = from app in _context.applications.AsEnumerable()
                                             join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                             join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                             join s in _context.Categories.AsEnumerable() on (int)app.category_id equals s.id
                                             join Phases in _context.Phases.AsEnumerable() on (int)app.PhaseId equals Phases.id
                                             where app.id.ToString() == AppID && app.company_id == company_id
                                             select new MyApps
                                             {
                                                 appID = app.id,
                                                 Reference = app.reference,
                                                 CategoryName = s.name,
                                                 PhaseName = Phases.name,
                                                 //Stage = s.StageName,
                                                 Status = app.status,
                                                 Date_Added = Convert.ToDateTime(app.date_added),
                                                 Submitted = app.submitted,
                                                 CompanyDetails = c.name + " (" + c.Address + ") ",
                                                 FacilityDetails = fac.Name,

                                             };

                        if (getApp4Company.Count() > 0)
                        {
                            ViewData["AppRefNO"] = getApp.reference;

                            var AppDocs = (from sd in _context.SubmittedDocuments
                                           join ad in _context.Document_Type_Applications on sd.AppDocID equals ad.DocumentTypeId
                                           select new ApplicationDocs
                                           {
                                               AppID = (int)sd.AppID,
                                               LocalDocID = sd.AppDocID,
                                               DocName = ad.DocumentTypeId.ToString(),
                                               EplsDocTypeID = ad.DocumentTypeId,
                                               CompElpsDocID = (int)sd.CompElpsDocID,
                                               //DocType = ad.DocumentTypeId,
                                               DocSource = sd.DocSource
                                           });

                            #region old section
                            //var getAppDocuments = _context.application_documents.Where(a => a.application_id == id).ToList();
                            var facility = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
                            var getApplicationFiles = appHelper.GetApplicationFiles(getApp, getCompany.elps_id.GetValueOrDefault(), facility.Elps_Id.GetValueOrDefault());
                            ViewBag.ApplicationDocs = getApplicationFiles.Where(a => a.Selected).ToList();
                            ViewBag.DocsRemaining = getApplicationFiles.Where(a => a.Selected == false).Count();

                            #endregion

                            ApplicationInformation.Add(new ApplicationInformation
                            {
                                ApplicationDetails = getApp4Company.ToList(),
                                ApplicationDocs = AppDocs.ToList()
                            });

                            _helpersController.LogMessages("Displaying full Application view with reference : " + getApp4Company.FirstOrDefault().Reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

                            return View(ApplicationInformation);

                        }
                        else
                        {
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps...! Something went wrong, Application not found. Please try again later") });
                        }
                    }
                }
                return View(ApplicationInformation);
            }
        }


        [Authorize(Policy = "AllStaffRoles")]
        public ActionResult ViewApplication(int Id, int? procid)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int loginID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionLogin));

            if (userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Kindly log in again.") });
            }


            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"];
                ViewBag.MsgType = TempData["msgType"];
            }
            if (TempData["Report"] != null)
                ViewBag.ExtraPayReport = TempData["Report"];

            if (Id > 0)
            {
                var getApps = _context.applications.Where(a => a.id == Id).ToList();
                var app = _context.applications.Where(a => a.id == Id).FirstOrDefault();

                var appDetail = (from ap in getApps
                                 join c in _context.companies on ap.company_id equals c.id
                                 join fac in _context.Facilities on ap.FacilityId equals fac.Id
                                 join cat in _context.Categories on ap.category_id equals cat.id
                                 join phs in _context.Phases on ap.PhaseId equals phs.id
                                 join his in _context.application_desk_histories on ap.id equals his.application_id into histor
                                 join sb in _context.SubmittedDocuments on ap.id equals sb.AppID into subdoc
                                 where ap.DeleteStatus != true && ap.id == Id
                                 select new MyApps
                                 {
                                     Current_Permit = ap.current_Permit != null ? ap.current_Permit : "",
                                     PaymentDescription = ap.PaymentDescription,
                                     Fee_Payable = (decimal)ap.fee_payable,
                                     appHistory = histor.OrderByDescending(x => x.id).FirstOrDefault(),
                                     appID = ap.id,
                                     Reference = ap.reference,
                                     category_id = cat.id,
                                     CategoryName = cat.name,
                                     PhaseName = phs.name,
                                     ShortName = phs.ShortName,
                                     PhaseId = phs.id,
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
                    return RedirectToAction("Dashboard", "Staffs");
                }
                var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
                var mistdo = _context.MistdoStaff.Where(x => x.FacilityId == app.FacilityId && x.DeletedStatus == false).ToList();
                if (mistdo != null)
                {
                    ViewBag.MistdoStaff = mistdo;
                }
                //#region getAppDoc

                List<OtherDocuments> otherDocuments = new List<OtherDocuments>();

                var submittedDoc = _context.SubmittedDocuments.Where(x => x.AppID == app.id && x.DeletedStatus != true);
                //var allDoc = _context.ApplicationDocuments.Where(x => x.PhaseId == appDetail.PhaseId && x.DeleteStatus != true);
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

                var getDocuments = _context.ApplicationDocuments.Where(x => (x.PhaseId == app.PhaseId || x.PhaseId == 0) && x.DeleteStatus != true).ToList();

                List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                List<PresentDocuments> presentFacDocuments = new List<PresentDocuments>();
                List<PresentDocuments> presentCompDocuments = new List<PresentDocuments>();
                List<MissingDocument> missingDocuments = new List<MissingDocument>();
                List<BothDocuments> bothDocuments = new List<BothDocuments>();

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
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying get facility documents for this application. Kindly contact support.") });
                }


                var products = _context.Products.ToList();
                ViewBag.Products = products;

                var appReport = from r in _context.Reports
                                join s in _context.Staff on r.StaffId equals s.StaffID
                                orderby r.ReportId descending
                                where r.AppId == Id && r.DeletedStatus == false
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
                    }

                    ViewBag.DeskID = appProc.DeskID;

                }


                var history = _context.application_desk_histories.Where(a => a.application_id == app.id).OrderByDescending(a => a.date);
                ViewBag.History = history.Take(2);
                ViewBag.historyID = history.Count() > 0 ? history.FirstOrDefault().id : 0;

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
                                      OperationsCompleted = j.OperationsCompleted,
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
                #endregion

                //Check if Application has schedule on it.
                var Sche = _context.MeetingSchedules.Where(a => a.ApplicationId == app.id).ToList();
                var appSche = Sche.OrderByDescending(a => a.Date).FirstOrDefault();
                ViewBag.MyAppScheduleExpiry = false;
                if (appSche != null)
                {
                    if (appSche.ScheduleExpired == null)
                    {
                        //check if the date is still within
                        if (appSche.ApprovedDate.GetValueOrDefault().Date.AddDays(3) < DateTime.Now)
                        {
                            //expired already
                            //ViewApplication

                            appSche.Message = string.Format("{0}{1}{1} {2}", appSche.Message, Environment.NewLine, ViewBag.expired);

                            appSche.ScheduleExpired = true;
                            _context.SaveChanges();
                            if (appSche.StaffUserName.ToLower() == userEmail.ToLower())
                            {
                                ViewBag.MyAppScheduleExpiry = true;
                            }
                        }
                    }
                    ViewBag.Schedules = Sche;
                    #region
                    var message = "";
                    var typ = "";
                    bool wait = true;
                    if (appSche.Approved == null && appSche.ScheduleExpired == null)
                    {
                        typ = "warning";
                        message = "Application schedule is yet to be approved.";
                    }
                    else
                    {

                        if (appSche.Approved != null && appSche.Approved == false)
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
                        else if (appSche.Accepted == null)
                        {
                            typ = "warning";
                            message = "The applicant/marketer is yet to respond to the schedule.";
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
                    }
                    ViewBag.CheckScheduledDate = appSche.Approved == true ? "Yes" : "No";
                    ViewBag.scheMsg = message;
                    ViewBag.scheTyp = typ;
                    ViewBag.Wait = wait;
                    #endregion
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

                        ViewBag.facModification = $"Modification type: {facMod.Type}";
                        if (prv != null)
                        {
                            ViewBag.PrevProduct = prv;
                        }
                    }
                }

                ViewBag.TranferCost = app.PhaseId == 6 ? '₦' + string.Format("{0:N}", app.TransferCost) : null;
                if (app.status == GeneralClass.PaymentCompleted && app.current_desk > 0)
                {
                    app.status = GeneralClass.Processing;
                    _context.SaveChanges();
                }


                ViewBag.Location = (from s in _context.Staff
                                    join l in _context.Location on s.LocationID equals l.LocationID
                                    where s.StaffID == userID
                                    select l).FirstOrDefault()?.LocationName;

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
                           //(t.Status == null || (t.Status.Contains("Approved")))
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
                if ((userRole.Contains("AD")) && app.AppProcessed == true)
                {
                    //we need to check s/he has worked on this application(Pushed) it before now
                    var aph = _context.application_desk_histories.Where(a => a.UserName == userEmail && a.application_id == app.id).ToList();
                    if (aph != null && aph.Count > 0)
                    {
                        ViewBag.ADCanPushToSupervisor = true;
                    }


                }
                if ((userRole.Contains("Supervisor")) && app.AppProcessed == true)
                {
                    //we need to check s/he has worked on this application(Pushed) it before now
                    var aph = _context.application_desk_histories.Where(a => a.UserName == userEmail && a.application_id == app.id).ToList();
                    if (aph != null && aph.Count == 1)
                    {
                        var apProc = _context.WorkProccess.Where(a => a.CategoryID == app.category_id && !a.DeleteStatus).ToList();
                        if (apProc.Count == 10)
                        {
                            ViewBag.SPCanPushToInspector = true;
                        }
                    }

                }

                //get schedule modal info
                ViewBag.AppProcId = appProc == null ? 0 : appProc.DeskID;
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

                return View(appDetail);
            }
            else
            {
                TempData["msgType"] = "fail";
                TempData["message"] = "Invalid application provided";
                return RedirectToAction("Dashboard", "Staffs");
            }
        }
        [HttpGet]
        public ActionResult EditApplication(int Id)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Kindly log in again.") });
            }

            if (Id > 0)
            {
                var getApps = _context.applications.Where(a => a.id == Id).ToList();
                var app = _context.applications.Where(a => a.id == Id).FirstOrDefault();

                var appDetail = (from ap in getApps
                                 join c in _context.companies on ap.company_id equals c.id
                                 join fac in _context.Facilities on ap.FacilityId equals fac.Id
                                 join cat in _context.Categories on ap.category_id equals cat.id
                                 join phs in _context.Phases on ap.PhaseId equals phs.id
                                 join his in _context.application_desk_histories on ap.id equals his.application_id into histor
                                 join sb in _context.SubmittedDocuments on ap.id equals sb.AppID into subdoc
                                 where ap.DeleteStatus != true && ap.id == Id
                                 select new MyApps
                                 {
                                     Current_Permit = ap.current_Permit != null ? ap.current_Permit : "",
                                     PaymentDescription = ap.PaymentDescription,
                                     Fee_Payable = (decimal)ap.fee_payable,
                                     appHistory = histor.OrderByDescending(x => x.id).FirstOrDefault(),
                                     appID = ap.id,
                                     Reference = ap.reference,
                                     category_id = cat.id,
                                     CategoryName = cat.name,
                                     PhaseName = phs.name,
                                     ShortName = phs.ShortName,
                                     PhaseId = phs.id,
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
                    return RedirectToAction("Dashboard", "Staffs");
                }
                var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
                var mistdo = _context.MistdoStaff.Where(x => x.FacilityId == app.FacilityId && x.DeletedStatus == false).ToList();
                if (mistdo != null)
                {
                    ViewBag.MistdoStaff = mistdo;
                }
                //#region getAppDoc

                List<OtherDocuments> otherDocuments = new List<OtherDocuments>();

                var submittedDoc = _context.SubmittedDocuments.Where(x => x.AppID == app.id && x.DeletedStatus != true);
                //var allDoc = _context.ApplicationDocuments.Where(x => x.PhaseId == appDetail.PhaseId && x.DeleteStatus != true);
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

                var getDocuments = _context.ApplicationDocuments.Where(x => (x.PhaseId == app.PhaseId || x.PhaseId == 0) && x.DeleteStatus != true).ToList();

                List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                List<PresentDocuments> presentFacDocuments = new List<PresentDocuments>();
                List<PresentDocuments> presentCompDocuments = new List<PresentDocuments>();
                List<MissingDocument> missingDocuments = new List<MissingDocument>();
                List<BothDocuments> bothDocuments = new List<BothDocuments>();

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
                                        TypeName = a.DocName,
                                        SubmitDocID= u.SubDocID
                                        
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
                                            join sb in _context.SubmittedDocuments on u.AppDocID equals sb.AppDocID
                                            where u.docType == "Facility"
                                            select new
                                            {
                                                facilityDoc = f,
                                                applicationDoc = u,
                                                submittedDoc = sb
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
                                TypeName = fDoc.applicationDoc.DocName,
                                SubmitDocID = fDoc.submittedDoc.SubDocID

                            });
                        });

                        //Company documents
                        var compDocuments = (from u in getDocuments
                                             join c in companyDoc on u.ElpsDocTypeID.ToString() equals c.document_type_id.ToString()
                                             join sb in _context.SubmittedDocuments on u.AppDocID equals sb.AppDocID
                                             where u.docType == "Company"
                                             select new
                                             {
                                                 companyDoc = c,
                                                 applicationDoc = u,
                                                 submittedDoc = sb
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
                                TypeName = cDoc.applicationDoc.DocName,
                                SubmitDocID = cDoc.submittedDoc.SubDocID

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
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying get facility documents for this application. Kindly contact support.") });
                }


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

                        ViewBag.facModification = $"Modification type: {facMod.Type}";
                        if (prv != null)
                        {
                            ViewBag.PrevProduct = prv;
                        }
                    }
                }

                ViewBag.TranferCost = app.PhaseId == 6 ? '₦' + string.Format("{0:N}", app.TransferCost) : null;
                if (app.status == GeneralClass.PaymentCompleted && app.current_desk > 0)
                {
                    app.status = GeneralClass.Processing;
                    _context.SaveChanges();
                }


                ViewBag.Location = (from s in _context.Staff
                                    join l in _context.Location on s.LocationID equals l.LocationID
                                    where s.StaffID == userID
                                    select l).FirstOrDefault()?.LocationName;

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
                           //(t.Status == null || (t.Status.Contains("Approved")))
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


                int fsid = 0;

                //get the Facility
                var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                var facState = address == null ? null : _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                fsid = facState != null ? facState.State_id : 0;

               
                appDetail.bothDocuments = bothDocuments;

                return View(appDetail);
            }
            else
            {
                TempData["msgType"] = "fail";
                TempData["message"] = "Invalid application provided.";
                return RedirectToAction("Dashboard", "Staffs");
            }
        }
        [HttpPost]
        public ActionResult EditApplication(int appID, string PaymentDescription, string Reference)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            
            var app = _context.applications.Where(a => a.id == appID).FirstOrDefault();
            if(app != null)
            {
                app.PaymentDescription = PaymentDescription.TrimEnd();
                app.reference = app.submitted != true ? Reference.Trim() : app.reference;
                _context.SaveChanges();
            }
            return RedirectToAction("EditApplication", "Application", new { Id = appID });
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




        //[Route("Application/{id:int?}")]
        public IActionResult AllProcessingApps()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var phases = _context.Phases.Where(a => a.name != null).OrderBy(a => a.name).Select(a => a.name).ToList();
            ViewBag.phases = phases;


            var apps = (from app in _context.applications.AsEnumerable()
                        join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                        join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                        join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                        join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                        join ad in _context.addresses on fac.AddressId equals ad.id
                        join sf in _context.States_UT.AsEnumerable() on ad.StateId equals sf.State_id
                        where app.DeleteStatus != true && app.submitted == true && app.status == "Processing"
                        select new MyApps
                        {
                            appID = app.id,
                            Reference = app.reference,
                            PhaseName = phs.name,
                            category_id = cat.id,
                            FacilityId = fac.Id,
                            Current_Permit = app.current_Permit != null ? app.current_Permit : "",
                            Address_1 = ad.address_1,
                            Status = app.status,
                            Date_Added = Convert.ToDateTime(app.date_added),
                            DateSubmitted = app.CreatedAt != null ? Convert.ToDateTime(app.CreatedAt) : Convert.ToDateTime(app.date_added),
                            CompanyName = c.name,
                            StateName = sf.StateName,
                            LGA = ad.LgaId != null ? _context.Lgas.Where(x => x.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                            FacilityName = fac.Name,
                            Year = app.year,
                        });

            List<AllProcessingModel> pc = new List<AllProcessingModel>();


            apps.ToList().ForEach(x => {

                var processing = (from p in _context.application_Processings.AsEnumerable()
                                  where p.ApplicationId == x.appID && p.processor > 0
                                  select new ProcessingModel
                                  {

                                      Sort = _context.vPhaseRoutings.Where(x => x.ProcessingRule_Id == p.ProcessingRule_Id).FirstOrDefault().SortOrder.ToString(),
                                      Processed = p.Processed.ToString(),
                                      oldStaffID = (int)p.processor,
                                      Staff = _context.UserBranches.Where(x => x.Id == p.processor).FirstOrDefault()?.UserEmail,
                                      myDesk = _context.MyDesk.Where(b => b.AppId == p.ApplicationId).OrderByDescending(b => b.Sort).ToList(),
                                      CurrentDesk = _context.UserBranches.Where(x => x.Id == p.processor && p.Processed != true).FirstOrDefault()?.UserEmail,
                                      CurrentProcDesk = _context.MyDesk.Where(b => b.AppId == p.ApplicationId && b.HasWork != true).FirstOrDefault()?.StaffID.ToString(),

                                  }).OrderBy(x => x.Sort).ToList();
                if (processing.Count() > 0)
                {
                    var unifiedProc = new AllProcessingModel()
                    {

                        AppReference = x.appID.ToString(),
                        State = x.StateName,
                        PhaseName = x.PhaseName,
                        Company = x.CompanyName,
                        CurrentDesk = processing.FirstOrDefault()?.CurrentDesk,
                        CurrentProcDesk = processing.FirstOrDefault()?.CurrentProcDesk,
                        processingModel = processing,
                        myDesk = processing.FirstOrDefault().myDesk
                    };
                    pc.Add(unifiedProc);

                }
            });


            ViewBag.StaffTable = _context.Staff.ToList();

            return View(pc);

            //return View();
        }
        public IActionResult AllRejectedApps()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var phases = _context.Phases.Where(a => a.name != null).OrderBy(a => a.name).Select(a => a.name).ToList();
            ViewBag.phases = phases;


            var apps = (from app in _context.applications.AsEnumerable()
                        join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                        join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                        join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                        join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                        join ad in _context.addresses on fac.AddressId equals ad.id
                        join sf in _context.States_UT.AsEnumerable() on ad.StateId equals sf.State_id
                        where app.DeleteStatus != true && app.submitted == true && app.status == "Rejected"
                        select new MyApps
                        {
                            appID = app.id,
                            Reference = app.reference,
                            PhaseName = phs.name,
                            category_id = cat.id,
                            FacilityId = fac.Id,
                            Current_Permit = app.current_Permit != null ? app.current_Permit : "",
                            Address_1 = ad.address_1,
                            Status = app.status,
                            Date_Added = Convert.ToDateTime(app.date_added),
                            DateSubmitted = app.CreatedAt != null ? Convert.ToDateTime(app.CreatedAt) : Convert.ToDateTime(app.date_added),
                            CompanyName = c.name,
                            StateName = sf.StateName,
                            LGA = ad.LgaId != null ? _context.Lgas.Where(x => x.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                            FacilityName = fac.Name,
                            Year = app.year,
                        });

            List<AllProcessingModel> pc = new List<AllProcessingModel>();


            apps.ToList().ForEach(x => {

                var processing = (from p in _context.application_Processings.AsEnumerable()
                                  where p.ApplicationId == x.appID && p.processor > 0
                                  select new ProcessingModel
                                  {

                                      Sort = p.SortOrder.ToString(),
                                      Processed = p.Processed.ToString(),
                                      oldStaffID = (int)p.processor,
                                      Staff = _context.UserBranches.Where(x => x.Id == p.processor).FirstOrDefault()?.UserEmail,
                                      myDesk = _context.MyDesk.Where(b => b.AppId == p.ApplicationId).OrderByDescending(b => b.Sort).ToList(),
                                      CurrentDesk = _context.UserBranches.Where(x => x.Id == p.processor && p.Processed != true).FirstOrDefault()?.UserEmail,
                                      CurrentProcDesk = _context.MyDesk.Where(b => b.AppId == p.ApplicationId && b.HasWork != true).FirstOrDefault()?.StaffID.ToString(),

                                  }).OrderBy(x => x.Sort).ToList();
                if (processing.Count() > 0)
                {
                    var unifiedProc = new AllProcessingModel()
                    {

                        AppReference = x.appID.ToString(),
                        State = x.StateName,
                        PhaseName = x.PhaseName,
                        Company = x.CompanyName,
                        CurrentDesk = processing.FirstOrDefault()?.CurrentDesk,
                        CurrentProcDesk = processing.FirstOrDefault()?.CurrentProcDesk,
                        processingModel = processing,
                        myDesk = processing.FirstOrDefault().myDesk
                    };
                    pc.Add(unifiedProc);

                }
            });


            ViewBag.StaffTable = _context.Staff.ToList();

            return View(pc);

            //return View();
        }
        public IActionResult AllApplicationss(int? id, string apptoday)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var phases = _context.Phases.Where(a => a.name != null).OrderBy(a => a.name).Select(a => a.name).ToList();
            ViewBag.phases = phases;

            var myApp = _helpersController.ApplicationDetails();
            var apps = myApp;

            List<string> Year = new List<string>();

            apps.ToList().ForEach(x =>
            {
                x.Id += 1;
                Year.Add(x.Year.ToString());
            });
            ViewBag.Year = Year.Distinct();

            if (!string.IsNullOrEmpty(apptoday))
            {
                apps = apps.Where(p => p.DateSubmitted > DateTime.Now.AddDays(-1)).ToList();

            }
            #region Admin Application Query
            if ((userRole.Contains("Admin") || userRole.Contains("Support") || userRole.Contains("Account") || userRole.Contains("Staff")))
            {

                if (id == null)
                {

                    return View(apps);
                }
                return RedirectToAction("ViewApplication", "Application", new { Id = id });
            }
            #endregion
            return View(apps);

            //return View();
        }

        [HttpPost]
        public IActionResult AllApplicationss(JQueryDataTableParamModel param, string status, string phase, string sdate, string edate, string year)
        {
            DateTime sd = string.IsNullOrEmpty(sdate) ? DateTime.Today.AddYears(-2).Date : Convert.ToDateTime(sdate).Date;
            DateTime ed = string.IsNullOrEmpty(edate) ? DateTime.UtcNow.AddHours(1) : Convert.ToDateTime(edate).Date.AddDays(1).AddSeconds(-1);

            IEnumerable<MyApps> allApplications = _helpersController.ApplicationDetails();


            List<string> Year = new List<string>();

            allApplications.ToList().ForEach(x =>
            {
                x.Id += 1;
                Year.Add(x.Year.ToString());
            });
            ViewBag.Year = Year.Distinct();
            if (!string.IsNullOrEmpty(phase))
            {
                allApplications = allApplications.Where(p => p.PhaseName.ToLower() == phase.ToLower());
            }
            if (!string.IsNullOrEmpty(year))
            {
                allApplications = allApplications.Where(p => p.Yearr.ToString() == year);
            }
            if (!string.IsNullOrEmpty(status))
            {
                allApplications = allApplications.Where(p => p.Status.ToString() == status);
            }

            else
            {
                allApplications = allApplications.Where(a => a.DateSubmitted >= sd && a.DateSubmitted <= ed);
            }
            IEnumerable<MyApps> filteredApplications;
            //var sortColumnIndex = Convert.ToInt32(Request.Form["iSortCol_0"]) + 1;
            var sortColumnIndex = 0 + 1;

            //Func<MyApps, string> orderingFunction = (c => sortColumnIndex == 1 ? c.CompanyName : sortColumnIndex == 2 ? c.Reference : sortColumnIndex == 3 ? c.PhaseName : c.Status);

            //var sortDirection = Request.Form["sSortDir_0"]; // asc or desc
            var phases = _context.Phases.Where(a => a.name != null).OrderBy(a => a.name).Select(a => a.name).ToList();
            ViewBag.phases = phases;

            var sortDirection = "asc"; // or desc
            List<MyApps> displayedApplications = new List<MyApps>();

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                param.sSearch = param.sSearch.ToLower();
                filteredApplications = allApplications.Where(C => C.CompanyName.ToLower().Contains(param.sSearch)
                    || C.Reference.ToLower().Contains(param.sSearch)
                    || C.PhaseName.ToLower().Contains(param.sSearch)
                    || C.Status.ToLower().Contains(param.sSearch)
                    || C.Date_Added.ToString().Contains(param.sSearch)
                    || C.StateName.ToLower().Contains(param.sSearch)
                    || C.Yearr.ToString().ToLower().Contains(param.sSearch)
                    );
            }
            else
            {
                filteredApplications = allApplications;
            }


            if (sortDirection == "asc")
            {
                #region Sort Ascending
                switch (sortColumnIndex)
                {
                    case 1:
                        filteredApplications = filteredApplications.OrderBy(a => a.CompanyName);
                        break;
                    case 2:
                        filteredApplications = filteredApplications.OrderBy(a => a.Reference);
                        break;
                    case 3:
                        filteredApplications = filteredApplications.OrderBy(a => a.PhaseName);
                        break;
                    case 4:
                        filteredApplications = filteredApplications.OrderBy(a => a.Status);
                        break;
                    default:
                        filteredApplications = filteredApplications.OrderBy(a => a.Date_Added);
                        break;
                }
                #endregion
            }
            else
            {
                #region Sort Descending
                switch (sortColumnIndex)
                {
                    case 1:
                        filteredApplications = filteredApplications.OrderByDescending(a => a.CompanyName);
                        break;
                    case 2:
                        filteredApplications = filteredApplications.OrderByDescending(a => a.Reference);
                        break;
                    case 3:
                        filteredApplications = filteredApplications.OrderByDescending(a => a.PhaseName);
                        break;
                    case 4:
                        filteredApplications = filteredApplications.OrderByDescending(a => a.Status);
                        break;
                    default:
                        filteredApplications = filteredApplications.OrderByDescending(a => a.Date_Added);
                        break;
                }
                #endregion
            }
            displayedApplications = filteredApplications.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var result = from c in displayedApplications
                         select new[] {Convert.ToString(c.Company_Id), c.CompanyName,
                          c.Reference, c.PhaseName, c.Status,c.Date_Added.ToString(),c.StateName };

            ViewBag.TotalRecords = allApplications.Count();
            ViewBag.TotalDisplayRecords = filteredApplications.Count();

            return View(filteredApplications.OrderByDescending(x => x.DateSubmitted));

        }
        public IActionResult RejectedApplications(int? id, int? apId, string s, string phase, string sDate, string eDate)
        {
            ViewBag.phase = phase; ViewBag.sDate = sDate; ViewBag.eDate = eDate;
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var phases = _context.Phases.Where(a => a.name != null).OrderBy(a => a.name).Select(a => a.name).ToList();
            ViewBag.phases = phases;

            #region Admin Application Query
            if ((userRole.Contains("Admin") || userRole.Contains("Support") || userRole.Contains("Account") || userRole.Contains("Staff")))
            {
                if (id == null)
                {
                    var myApp = _helpersController.ApplicationDetails();
                    var apps = myApp.Where(x => x.Status == GeneralClass.Rejected);

                    return View(apps);
                }
                return RedirectToAction("ViewApplication", "Process", new { Id = generalClass.Encrypt(id.ToString()) });
            }
            #endregion
            return View();
        }

        ////[Authorize(Roles = "Admin,Support,Staff,ITAdmin")]

        ////[Authorize(Roles = "Staff, Admin, ITAdmin")]
        //public IActionResult ViewApplication(int Id, int? procid)
        //{
        //    try
        //    {
        //        var context = new NewDepotContext();

        //        var userEmail = userEmail;
        //        if (TempData["Report"] != null)
        //        {
        //            ViewBag.Reports = TempData["Report"];
        //        }
        //        if (Id > 0)
        //        {
        //            var app = _context.applications.Where(a => a.id == Id).FirstOrDefault();
        //            if (app == null)
        //            {
        //                TempData["msgType"] = "fail";
        //                TempData["message"] = "Invalid application credentials";
        //                return RedirectToAction("MyDesk");
        //            }

        //            var fac = _context.Facilities.Where(a => a.id == app.FacilityId).FirstOrDefault();
        //            if (fac == null)
        //            {
        //                TempData["msgType"] = "fail";
        //                TempData["message"] = "Invalid application credentials";
        //                return RedirectToAction("Index");
        //            }
        //            //app.ApplicationDocs = _vAppDocRep.Where(a => a.Application_Id == app.id).ToList();

        //            var comp = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();
        //            //app.FacilityId=getApp.

        //            var getApplicationFiles = appHelper.getApplicationFiles(app, comp.elps_id.GetValueOrDefault(), fac.elps_id.GetValueOrDefault());

        //            var phasefcdocs = context.PhaseFacilityDocuments.Where(x => x.PhaseId == app.PhaseId).ToList();
        //            var ad = getApplicationFiles.Where(a => a.Selected == false && phasefcdocs.Any(x => x.Document_Type_Id == a.Document_Id)).ToList();

        //            var docs = appHelper.getAppFiles(app, comp.elps_id.GetValueOrDefault(), fac.elps_id.GetValueOrDefault());
        //            var selecteddocs = getApplicationFiles.Where(a => a.Selected).ToList();
        //            if (docs.Count > 0 && !selecteddocs.Any(x => x.type.Equals("facility", StringComparison.OrdinalIgnoreCase)))
        //                getApplicationFiles.AddRange(docs);

        //            ViewBag.ApplicationDocs = getApplicationFiles.Where(a => a.Selected).ToList();
        //            ViewBag.DocsRemaining = getApplicationFiles.Where(a => a.Selected == false).Count();
        //            ViewBag.DeskID = procid;

        //            var hist = _vAppDeskRep.Where(a => a.Application_Id == app.id).OrderByDescending(a => a.Date).Take(2);
        //            ViewBag.History = hist;

        //            if (!userRole.Contains("Inspector"))
        //            {
        //                var appForms = _appFormRep.Where(a => a.ApplicationId == app.id).ToList();
        //                if (appForms.Count() > 0)
        //                {
        //                    var af = appForms.FirstOrDefault(a => a.Filled == true);
        //                    if (af != null)
        //                    {
        //                        ViewBag.appForm = af;
        //                    }
        //                }
        //            }

        //            #region Show Field Report on the Application
        //            // 1: Get List of Assigned Inspectors
        //            var inspectors = _vJointStaffRep.Where(a => a.ApplicationId == app.id).ToList();
        //            // 2: Get All Report on the Joint Activity
        //            var reports = _vJointReportRep.Where(a => a.ApplicationId == app.id).ToList();
        //            var reportModel = new JointReportModel()
        //            {
        //                FD_Inspectors = inspectors,
        //                Reports = reports
        //            };
        //            ViewBag.Report = reportModel;
        //            #endregion

        //            //Get Current Desk
        //            var nextProc = _vAppProcRep.Where(a => a.ApplicationId == app.id && a.Assigned && !a.Processed).FirstOrDefault();
        //            if (nextProc != null)
        //            {
        //                var current = _StaffRep.Where(a => a.id == nextProc.Processor).FirstOrDefault();
        //                app.CurrentDesk = current != null ? current.StaffEmail : "";
        //            }

        //            var extraPayment = _appExtrPay.Where(C => C.ApplicationId == app.id).ToList();
        //            bool paid = false;
        //            foreach (var item in extraPayment)
        //            {
        //                if (item.Status == "Payment Pending")
        //                {

        //                    var response = NewRemitaResponse.CheckRRRPayment(item.RRR);
        //                    if (response != null && (response.status == "01" || response.status == "00"))
        //                    {
        //                        paid = true;
        //                        item.DatePaid = response.transactiontime;
        //                        item.Status = "Paid";
        //                        _appExtrPay.Edit(item);
        //                    }
        //                }
        //            }
        //            if (paid)
        //            {
        //                _appExtrPay.Save(userEmail, Request.UserHostAddress);

        //            }
        //            ViewBag.ExtraPay = extraPayment;
        //            if (Phase.ShortName == "DM" || Phase.ShortName == "UWA" || Phase.ShortName == "RC")//UWA  :: Update without Modification
        //            {
        //                //check facilityTankModification Table with the ApplicationID
        //                var facMod = _facModRep.Where(a => a.ApplicationId == app.id).FirstOrDefault();
        //                if (facMod != null)
        //                {
        //                    string prv = "";
        //                    if (!string.IsNullOrEmpty(facMod.PrevProduct))
        //                    {
        //                        prv = $"Converted from {facMod.PrevProduct}";
        //                    }

        //                    ViewBag.facModification = $"Modification type: {facMod.Type} {prv}";
        //                }

        //            }
        //            var tnks = new List<vTank>();
        //            var apTanks = _vAppTankRep.Where(a => a.ApplicationId == app.id).ToList();
        //            if (apTanks.Count > 0)
        //            {
        //                vTank t = null;
        //                foreach (var item in apTanks)
        //                {
        //                    t = new vTank
        //                    {
        //                        CompanyId = item.CompanyId,
        //                        Decommissioned = false,
        //                        Diameter = item.Diameter,
        //                        FacilityId = item.FacilityId,
        //                        FriendlyName = item.FriendlyName,
        //                        Height = item.Height,
        //                        Id = item.TankId,
        //                        MaxCapacity = item.Capacity.ToString(),
        //                        Name = item.TankName,
        //                        ProductId = item.ProductId,
        //                        ProductName = item.ProductName
        //                    };
        //                    tnks.Add(t);
        //                }
        //            }
        //            else
        //            {
        //                var tks = _vTankRep.Where(a => a.FacilityId == app.FacilityId).ToList();
        //                tnks = tks;
        //            }
        //            ViewBag.AppTanks = tnks;
        //            return View(app);
        //        }
        //        TempData["msgType"] = "fail";
        //        TempData["message"] = "Invalid application provided";
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception x)
        //    {
        //        _helpersController.LogMessages($"From ViewApplication, application Contriller: {x.ToString()}");
        //        throw;
        //    }
        //}

        public IActionResult ViewPermit(string id)
        {
            return RedirectToAction("ViewLicense", new { id = generalClass.Encrypt(id) });
            int Id = 0;
            int.TryParse(id, out Id);
            var permit = _context.permits.Where(a => a.application_id == Id).FirstOrDefault();
            if (permit != null)
            {

                var viewPdf = new ViewAsPdf();
                viewPdf.ViewName = "LicenseView";
                //apdf = await pdf.BuildFile(ControllerContext);

                return File(new MemoryStream(), "application/pdf");
            }
            return null;
        }
        public IActionResult RemovePermit(string id)
        {
            int Id = 0;
            int.TryParse(id, out Id);
            var permit = _context.permits.Where(a => a.id == Id).FirstOrDefault();
            if (permit != null)
            {
                _context.permits.Remove(permit);
                if (_context.SaveChanges() > 0)
                {
                    return Content("Permit has been removed successfully.");
                }
            }
            return null;
        }
        public async Task<IActionResult> DeleteP(int Id)
        {
            string response = "";
            
            var permit = _context.permits.Where(a => a.id == Id).FirstOrDefault();
            if (permit != null)
            {
                _context.permits.Remove(permit);
                if (_context.SaveChanges() > 0)
                {
                    response = "Deleted";
                }
                else
                {
                    response = "Something went wrong trying to delete this permit.";
                }
            }
            else
            {
                response = "Application phase id was not passed correctly.";
            }

            _helpersController.LogMessages("Deleting application Phase. Status : " + response + " Application Permit ID : " + permit.permit_no, _helpersController.getSessionEmail());

            return Json(response);
        }
        #region Application Initiation

        [Authorize(Policy = "CompanyRoles")]
        public IActionResult Apply()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            ViewBag.Err = TempData["ErrorMessage"];
            if (TempData["PermitUsage"] != null)

            {
                ViewBag.PermitUsage = TempData["PermitUsage"];
            }

            ViewBag.Category = _context.Categories.ToList(); ;
            ViewBag.sanction = _context.Sanctions.ToList();

            var company = _context.companies.Where(x => x.id == CompanyID && x.DeleteStatus != true && x.ActiveStatus != false).FirstOrDefault();

            if (company != null)
            {
                if (company.registered_address_id == null || company.registered_address_id <= 0)
                {
                    return RedirectToAction("CompanyInformation", "Companies", new { id = generalClass.Encrypt("Kindly update your company's registered address on the portal.") });
                }

                var states = _context.States_UT.Where(a => a.Country_id == 156).ToList();
                ViewBag.stats = states;
                return View();
            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session have expired..") });
        }
        [HttpPost]
        public IActionResult Apply(string permitId, int? phaseId, string category = null)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (!string.IsNullOrEmpty(permitId))
            {
                permitId = permitId.Trim().ToLower();
            }
            var comp = _context.companies.Where(a => a.id == userID && a.DeleteStatus != true && a.ActiveStatus != false).FirstOrDefault();


            ViewBag.Category = _context.Categories.Where(a => a.name != null && a.DeleteStatus != true).ToList(); ;
            ViewBag.sanction = _context.Sanctions.ToList();
            var phases = _context.Phases.Where(a => a.name != null && a.DeleteStatus != true).OrderBy(a => a.name).Select(a => a.name).ToList();
            var selectedPhase = _context.Phases.Where(a => a.id == phaseId && a.DeleteStatus != true).FirstOrDefault();
            ViewBag.phases = phases;
            //get the permit bearing the supllied permit number.
            var pm = (from p in _context.permits
                      join a in _context.applications on p.application_id equals a.id
                      join ph in _context.Phases on a.PhaseId equals ph.id
                      where p.permit_no.ToLower() == permitId.ToLower() && a.company_id == comp.id
                      select new permitsModel
                      {
                          PhaseId = a.PhaseId,
                          PhaseName = ph.name,
                          ApprovalType = ph.IssueType,
                          PhaseShortName = ph.ShortName,
                          Application_Id = a.id,
                          FacilityId = (int)a.FacilityId,
                          Category_id = a.category_id,
                          Permit_No = p.permit_no,
                          Date_Issued = p.date_issued,
                          Date_Expire = p.date_expire

                      }).FirstOrDefault();

            var lgy = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId).FirstOrDefault();
            if (lgy != null)
            {
                //check if legacy has not been used before
                if (lgy.IsUsed == true)
                {
                    ViewBag.ErrorMessage = "Sorry, you can't proceed with this application as the legacy permit has previously been used.";
                    return View();
                }
            }
            if (pm != null)
            {
                //check entered permit expiry date
                if (DateTime.Now > pm.Date_Expire && (pm.PhaseShortName == "NDT" || pm.PhaseShortName == "LTO" || pm.PhaseShortName == "LR" || pm.PhaseShortName == "TO" || pm.PhaseShortName == "RC"))
                {
                    //permit has expired, then check if expiry is even applicable to permit app type
                    if (pm.PhaseShortName == "NDT" && phaseId != 11 && phaseId != 5) //Calibration Expiry and you are not applying for recalibration or depot modification
                    {
                        ViewBag.ErrorMessage = "Sorry, the calibration approval number you have supplied has expired, kindy apply for re-calibration before applying for LTO.";
                        return View();
                    }
                    else if (pm.PhaseShortName == "RC" && phaseId != 11 && phaseId != 5) //Re-calibration Expiry and you are not applying for recalibration or depot modification
                    {
                        ViewBag.ErrorMessage = "Sorry, the re-calibration approval number you have supplied has expired, kindy apply for another re-calibration before applying for LTO.";
                        return View();
                    }
                    else if ((pm.PhaseShortName == "LTO" || pm.PhaseShortName == "TO") && phaseId != 7 && phaseId != 5) //License To Operate expiry and you are not applying for license renewal or depot modification
                    {

                        ViewBag.ErrorMessage = "Sorry, the LTO number you have supplied has expired, kindy apply for license renewal.";
                        return View();
                    }
                    else if (pm.PhaseShortName == "LR" && phaseId != 7 && phaseId != 5) //Licence Renewal expiry and you are not applying for license renewal or depot modification
                    {

                        ViewBag.ErrorMessage = "Sorry, the license number you have supplied has expired, kindy apply for license renewal.";
                        return View();
                    }

                }


                else
                {

                    #region check supplied permit suitability for selected application
                    if (pm.PhaseShortName == "SI" && selectedPhase.ShortName != "ATC")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval number you have entered is for Suitability and can only be used to apply for ATC (Approval to Construct). Kindly apply for calibration/recalibration before applying for LTO.";
                        return View();
                    }
                    if (pm.PhaseShortName != "SI" && selectedPhase.ShortName == "ATC")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for ATC (Approval to Construct).";
                        return View();
                    }
                    if ((pm.PhaseShortName != "ATC" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "NDT")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "NDT" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "RC")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if (selectedPhase.ShortName == "LTO")
                    {

                        var calibrationApp = (from ap in _context.applications
                                              join p in _context.Phases on ap.PhaseId equals p.id
                                              where ap.FacilityId == pm.FacilityId && ap.DeleteStatus != true && ap.submitted == true && p.ShortName == "NDT"
                                              select ap).FirstOrDefault();
                        if (pm.PhaseShortName != "NDT" && (pm.PhaseShortName == "ATC" && calibrationApp == null))
                            ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                    }
                    if ((pm.PhaseShortName != "ATC" && pm.PhaseShortName != "REG" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "DM")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "ATC" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "TO")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "LR")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && (selectedPhase.ShortName == "UWA" || selectedPhase.ShortName == "SAP"))
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }

                    #endregion
                    switch (pm.PhaseId)
                    {
                        case 1:
                        case 8:
                        case 2:

                            #region current permit is for ATC or regularization ==> move to Calibration (NDT)
                            //Current permit is ATC and selected phase is calibration/calibration is in progress, so proceed
                            if ((pm.PhaseId == 2 || pm.PhaseId == 8))
                            {
                                var calibrationApp = (from ap in _context.applications
                                                      join p in _context.Phases on ap.PhaseId equals p.id
                                                      where ap.FacilityId == pm.FacilityId && ap.DeleteStatus != true && ap.submitted == true && p.ShortName == "NDT"
                                                      select ap
                                                      ).FirstOrDefault();
                                if (phaseId == 3 || calibrationApp != null)
                                    return RedirectToAction("Application", "LTO", new { id = pm.Application_Id, phaseId = phaseId });
                            }
                            else if (pm.PhaseId == 2 && phaseId == null && !string.IsNullOrEmpty(category) && int.Parse(category) == 7)
                            {
                                if (int.Parse(category) == 7)
                                {
                                    int catid = int.Parse(category);
                                    phaseId = _context.Phases.Where(x => x.category_id == catid).FirstOrDefault().id;
                                    return RedirectToAction("Application", "LTO", new { id = pm.Application_Id, phaseId = phaseId });

                                }
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Sorry, the provided license number can not be used for this type of application.";
                            }
                            if (phaseId == 3 || phaseId == 2)
                                return RedirectToAction("TankInspection", "ATC", new { id = pm.FacilityId, phaseId = phaseId, permitId = pm.Permit_No });
                            break;
                        #endregion
                        case 3:
                            #region application for Tank Pressure Leak Test ==> move to LTO

                            if (phaseId == 4)
                            {
                                if (!pm.Permit_No.Contains("PLT") && !pm.Permit_No.Contains("CAL")) //check if provided permit number is for calibration before applying for LTO
                                {

                                    TempData["ErrorMessage"] = "The provided approval number is not for calibration, kindly provide your calibration approval number before you can proceed with LTO.";
                                    return RedirectToAction("Apply");
                                }
                                else if (phaseId == 4 && pm.Date_Expire > DateTime.UtcNow.AddHours(1))
                                {
                                    return RedirectToAction("ReviewFacilityTank", "Application", new { id = pm.FacilityId, permitId = permitId, category = category, phaseId = phaseId, leg = 1 });
                                    //return RedirectToAction("Application", "LTO", new { id = pm.Application_Id, phaseId = phaseId });
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "The provided calibration approval number is expired, kindly apply for recalibration before you can proceed with LTO.";
                                    return RedirectToAction("Apply");
                                }
                            }
                            break;
                        #endregion
                        default: break;

                    }

                    ViewBag.ErrorMessage = "The provided License/Approval/Approval Number Can not be used for this type of Application.";
                }
            }
            else
            {
                if (permitId.ToLower() == "atc" && phaseId == 2)
                {
                    var ps = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();
                    return RedirectToAction("Review", "LTO", new { permitId = permitId, category = ps.category_id, phaseId = phaseId, leg = 0 });

                }
                if (permitId.ToLower() == "reg" && (category == "1002" || category == "8"))
                {
                    var ps = (from u in _context.Categories
                              join p in _context.Phases on u.id equals p.category_id
                              where u.id.ToString() == category
                              select p
                              ).FirstOrDefault();

                    return RedirectToAction("Review", "LTO", new { permitId = permitId, category = ps.category_id, phaseId = ps.id, leg = 0 });

                }
                var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId).FirstOrDefault();
                if (lg != null)
                {
                    //check if legacy has not been used before
                    if (lg.IsUsed == true)
                    {
                        ViewBag.ErrorMessage = "Sorry, you can't proceed with this application as the legacy permit has previously been used.";

                    }
                    else
                    {
                        bool move = false;
                        var ps = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();
                        if (ps != null && ps.category_id == 1)
                        {
                            if (phaseId == 2 && (lg.AppType == "Suitability Inspection" || lg.AppType == "SI"))
                            {
                                move = true;
                            }
                            else if (phaseId == 3 && (lg.AppType == "ATC" || lg.AppType == "Approval To Construct"))
                            {
                                move = true;
                            }
                            else if (phaseId == 4 && (lg.AppType == "NDT" || lg.AppType == "Calibration/Integrity Tests(NDTs)"))
                            {
                                move = true;
                            }
                        }
                        if (move)
                        {
                            return RedirectToAction("Review", "LTO", new { permitId = permitId, category = ps.category_id, phaseId = phaseId, leg = 1 });
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Kindly ensure that your legacy approval/permit is the requirement for the next application phase";

                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Permit/License with the provided reference number was not found. Please, check the reference number and try again.";

                }

            }
            return View();
        }

        public IActionResult GetPhases(int Id)
        {
            var phase = _context.Phases.Where(a => a.category_id == Id).OrderBy(a => a.Stage).ToList();
            return Json(phase);
        }

        public IActionResult ContinueSuitability(string id)
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int appId = generalClass.DecryptIDs(id);

            if (CompanyID == 0 || appId == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, expired session or app ID wasn't passed correctly..") });
            }
            var facModel = (from app in _context.applications
                            join fac in _context.Facilities on app.FacilityId equals fac.Id
                            join ad in _context.addresses on fac.AddressId equals ad.id
                            join com in _context.companies on app.company_id equals com.id
                            join suit in _context.SuitabilityInspections on app.id equals suit.ApplicationId
                            where app.id == appId && app.DeleteStatus != true && fac.DeletedStatus != true
                            select new FacilityModel
                            {
                                Id = suit.Id,
                                Name = fac.Name,
                                FacilityCode = fac.CategoryCode == null ? "No code yet" : fac.CategoryCode,
                                City = ad.city,
                                Street = ad.address_1,
                                StateName = _context.States_UT.Where(l => l.State_id == ad.StateId).FirstOrDefault().StateName,
                                LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                                LGAId = ad.LgaId,
                                StateId = ad.StateId,
                                ContactName = fac.ContactName,
                                ContactNumber = fac.ContactNumber,
                                NoOfPumps = (int)fac.NoOfPumps,
                                NoOfTanks = (int)fac.NoOfTanks,
                                SizeOfLand = suit.SizeOfLand,
                                StationsWithin2KM = suit.StationsWithin2KM,
                                DistanceFromExistingStation = suit.DistanceFromExistingStation,
                                ISAlongPipeLine = suit.ISAlongPipeLine,
                                IsOnHighWay = suit.IsOnHighWay,
                                IsUnderHighTension = suit.IsUnderHighTension,
                                Reference = app.reference
                            });


            ViewData["Ref"] = facModel.FirstOrDefault().Reference;
            var states = _context.States_UT.Where(a => a.Country_id == 156).ToList();
            ViewBag.stats = states;
            return View(facModel.FirstOrDefault());
        }
        public IActionResult getSuitForm()
        {

            var states = _context.States_UT.Where(a => a.Country_id == 156).ToList();
            ViewBag.stats = states;
            return View();
        }



        [HttpPost]
        public IActionResult ContinueSuitability(SuitabilityInspections model, int Id, Address addres, string FacilityName, int PhaseId, int category, string ISAlongPipeLine, string IsUnderHighTension, string IsOnHighWay, string contactName, string contactNumber)
        {
            if (model != null)
            {
                int appId = 0;
                int companyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                //getting company elps id from loacal DB to save to elps facility
                var company = _context.companies.Where(x => x.id == companyID && x.DeleteStatus != true && x.ActiveStatus != false).FirstOrDefault();

                _helpersController.LogMessages("Trying to create new facility for " + company.name, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                // already exiting facility on local DB
                var facility = _context.Facilities.Where(x => x.Name.ToLower() == FacilityName.ToLower().Trim() && x.CompanyId == companyID).FirstOrDefault();

                _helpersController.LogMessages("checking if new facility for " + company.name + " already exits.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                if (facility == null)
                {
                    string err = "Sorry, this facility does not exit on the portal. Kindly create a new facility for your application";
                    _helpersController.LogMessages("Facility does not exits " + FacilityName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });
                }
                else
                {
                    _helpersController.LogMessages("Facility exits " + FacilityName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
                    //update facility
                    facility.Date = DateTime.Now;
                    facility.Name = FacilityName;
                    facility.NoofDriveIn = 1;
                    facility.NoOfDriveOut = 1;
                    facility.ContactName = contactName;
                    facility.ContactNumber = contactNumber;
                    _context.SaveChanges();

                    var facilityAddress = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();

                    if (facilityAddress != null)
                    {
                        facilityAddress.elps_id = 0;
                        facilityAddress.address_1 = addres.address_1;
                        facilityAddress.city = addres.city;
                        facilityAddress.LgaId = addres.LGAId;
                        facilityAddress.StateId = addres.stateId;
                        _context.SaveChanges();
                    }
                    var suitability = _context.SuitabilityInspections.Where(a => a.Id == Id).FirstOrDefault();
                    if (suitability != null)
                    {
                        appId = suitability.ApplicationId;

                        suitability.SizeOfLand = model.SizeOfLand;
                        suitability.ISAlongPipeLine = model.ISAlongPipeLine;
                        suitability.IsOnHighWay = model.IsOnHighWay;
                        suitability.IsUnderHighTension = model.IsUnderHighTension;
                    }
                    _context.SaveChanges();

                    #endregion


                    #region Update Facility on ELPS //Adeola to update
                    if (facility.Elps_Id == null || facility.Elps_Id.Value <= 0)
                    {

                        FacilityAPIModel facmodel = new FacilityAPIModel()
                        {
                            Name = facility.Name,
                            FacilityType = "Depot",
                            LGAId = (int)facilityAddress.LgaId,
                            City = facilityAddress.city,
                            StateId = facilityAddress.StateId,
                            StreetAddress = facilityAddress.address_1
                        };

                        var param = JsonConvert.SerializeObject(facmodel);
                        var paramDatas = _restService.parameterData("fac", param);
                        var response = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facmodel);
                        if (response.IsSuccessful == false)
                        {
                            _helpersController.LogMessages("Error posting facility with name: " + facmodel.Name + " to ELPS.", company.CompanyEmail);
                        }
                        var respApp = JsonConvert.DeserializeObject<FacilityAPIModel>(response.Content.ToString());

                        if (respApp != null && respApp.Id > 0)
                        {
                            facility.Elps_Id = respApp.Id;
                            _context.SaveChanges();
                        }
                    }
                    #endregion

                    var app = _context.applications.Where(a => a.id == appId).FirstOrDefault();
                    if (app == null)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, application was not found. Please contact support.") });

                    }
                    else
                    {
                        app.UpdatedAt = DateTime.Now;
                        //log history and messages
                        _helpersController.LogMessages(company.name + " applied for site suitability application", company.id.ToString());

                        return RedirectToAction("UploadApplicationDocument", new { id = generalClass.Encrypt(appId.ToString()) });

                    }

                }

            }
            return View();
        }
        public IActionResult ApplyForNewDepot(SuitabilityInspections model, Address addres, string FacilityName, int PhaseId, int category, string ISAlongPipeLine, string IsUnderHighTension, string IsOnHighWay, string contactName, string contactNumber)
        {
            if (model != null)
            {
                string result = "";
                int companyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                //getting company elps id from loacal DB to save to elps facility
                var company = _context.companies.Where(x => x.id == companyID && x.DeleteStatus != true && x.ActiveStatus != false).FirstOrDefault();

                _helpersController.LogMessages("Trying to create new facility for " + company.name, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                // already exiting facility on local DB
                var facility_count = _context.Facilities.Where(x => x.Name.Trim().ToLower() == FacilityName.Trim().ToLower() && x.CompanyId == companyID).Count();

                _helpersController.LogMessages("checking if new facility for " + company.name + " already exits.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                if (facility_count > 0)
                {
                    result = "Sorry, this facility already exits on the portal.";
                    _helpersController.LogMessages("Facility exits " + FacilityName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
                }
                else
                {
                    _helpersController.LogMessages("Facility does not exits " + FacilityName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));


                    var cat = _context.Categories.Where(a => a.id == category).FirstOrDefault();
                    var ps = _context.Phases.Where(a => a.category_id == cat.id && a.id == PhaseId).FirstOrDefault();

                    var apId = Suitability(model, FacilityName, company, cat, ps, addres, contactName, contactNumber);
                    if (apId <= 0)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, an error has occured while trying to save your application/facility. Please contact support.") });

                    }
                    else
                    {
                        string app_id = apId.ToString().Trim();
                        var app = _context.applications.Where(a => a.id == apId).FirstOrDefault();

                        //save facility suitability information
                        var suitInfo = new SuitabilityInspections()
                        {
                            SizeOfLand = model.SizeOfLand != null ? model.SizeOfLand : "200",
                            ISAlongPipeLine = model.ISAlongPipeLine != null ? model.ISAlongPipeLine : false,
                            IsOnHighWay = model.IsOnHighWay != null ? model.IsOnHighWay : false,
                            IsUnderHighTension = model.IsUnderHighTension != null ? model.IsUnderHighTension : false,
                            CompanyId = companyID,
                            FacilityId = (int)app.FacilityId,
                            ApplicationId = apId
                        };
                        _context.SuitabilityInspections.Add(suitInfo);
                        _context.SaveChanges();


                        string subject = "Application Initiated with Ref : " + app.reference;
                        string content = "You have initiated an application (" + cat.name + ") with reference Number " + app.reference + " on NMDPRA PDJ Depot portal. Kindly find other details below.";

                        var emailMsg = _helpersController.SaveMessage(app.id, company.id, subject, content, company.elps_id.ToString(), "Company");
                        var sendEmail = _helpersController.SendEmailMessage(company.CompanyEmail, company.name, emailMsg, null);

                        //log history and messages
                        _helpersController.LogMessages(company.name + " applied for site suitability Application", company.id.ToString());

                        return RedirectToAction("UploadApplicationDocument", new { id = generalClass.Encrypt(app_id) });

                    }

                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(result) });

                }

                return View(model);
            }
            return View();
        }

        private int Suitability(SuitabilityInspections model, string FacilityName, companies comp, Categories cat, Phases ps, Address addres, string contactName, string contactNumber)
        {
            try
            {
                #region Facility Address
                var add = new addresses
                {
                    elps_id = 0,
                    address_1 = addres.address_1,
                    city = addres.city,
                    country_id = 156,
                    LgaId = addres.LGAId,
                    StateId = addres.stateId,
                };
                _context.addresses.Add(add);
                _context.SaveChanges();
                #endregion

                #region Facility
                var fa = new Facilities();

                fa.AddressId = add.id;
                fa.Date = DateTime.Now;
                fa.Name = FacilityName;
                fa.CompanyId = comp.id;
                fa.NoofDriveIn = 1;
                fa.NoOfDriveOut = 1;
                fa.CategoryId = cat.id;
                fa.ContactName = contactName;
                fa.ContactNumber = contactNumber;

                _context.Facilities.Add(fa); _context.SaveChanges();
                #endregion

                // saves new application
                var getApp = _helpersController.RecordApplication(ps, 0, 0, comp.id, fa.Id, "New", "", comp.CompanyEmail, Request.Host.Value);



                //Check if Facility is added to ELPS, else add it
                #region Add Facility to Processing on ELPS
                var facility = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();


                if (facility != null && (facility.Elps_Id == null || facility.Elps_Id.Value <= 0))
                {
                    var facilityAddress = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();


                    FacilityAPIModel facmodel = new FacilityAPIModel()
                    {
                        Name = facility.Name,
                        CompanyId = comp.elps_id.GetValueOrDefault(),
                        FacilityType = "Depot",
                        LGAId = (int)facilityAddress.LgaId,
                        City = facilityAddress.city,
                        StateId = facilityAddress.StateId,
                        StreetAddress = facilityAddress.address_1
                    };

                    var param = JsonConvert.SerializeObject(facmodel);
                    var paramDatas = _restService.parameterData("fac", param);
                    //var output = _restService.Response("/api/Facility/Add/{email}/{apiHash}", paramDatas, "POST");
                    var response = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facmodel);
                    if (response.IsSuccessful == false)
                    {
                        _helpersController.LogMessages("Error posting facility with name: " + facmodel.Name + " to ELPS.", comp.CompanyEmail);
                    }
                    var respApp = JsonConvert.DeserializeObject<FacilityAPIModel>(response.Content.ToString());

                    if (respApp != null && respApp.Id > 0)
                    {
                        facility.Elps_Id = respApp.Id;
                        _context.SaveChanges();
                    }
                }
                #endregion

                model.CompanyId = comp.id;
                model.ApplicationId = getApp.id;
                model.FacilityId = fa.Id;
                //_context.SuitabilityInspections.Add(model);
                //_context.SaveChanges();


                return getApp.id;
            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                return 0;
            }

        }

        [HttpPost]
        public IActionResult Apply2(string permitId, int? phaseId, string category = null)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (!string.IsNullOrEmpty(permitId))
            {
                permitId = permitId.Trim().ToLower();
            }
            var comp = _context.companies.Where(a => a.id == userID && a.DeleteStatus != true && a.ActiveStatus != false).FirstOrDefault();


            ViewBag.Category = _context.Categories.Where(a => a.name != null && a.DeleteStatus != true).ToList(); ;
            ViewBag.sanction = _context.Sanctions.ToList();
            var phases = _context.Phases.Where(a => a.name != null && a.DeleteStatus != true).OrderBy(a => a.name).Select(a => a.name).ToList();
            var selectedPhase = _context.Phases.Where(a => a.id == phaseId && a.DeleteStatus != true).FirstOrDefault();
            ViewBag.phases = phases;
            //get the permit bearing the supllied permit number.
            var pm = (from p in _context.permits
                      join a in _context.applications on p.application_id equals a.id
                      join ph in _context.Phases on a.PhaseId equals ph.id
                      where p.permit_no.ToLower() == permitId.ToLower() && a.company_id == comp.id
                      select new permitsModel
                      {
                          PhaseId = a.PhaseId,
                          PhaseName = ph.name,
                          ApprovalType = ph.IssueType,
                          PhaseShortName = ph.ShortName,
                          Application_Id = a.id,
                          FacilityId = (int)a.FacilityId,
                          Category_id = a.category_id,
                          Permit_No = p.permit_no,
                          Date_Issued = p.date_issued,
                          Date_Expire = p.date_expire

                      }).FirstOrDefault();

            var lgy = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId).FirstOrDefault();
            if (lgy != null)
            {
                //check if legacy has not been used before
                if (lgy.IsUsed == true)
                {
                    ViewBag.ErrorMessage = "Sorry, you can't proceed with this application as the legacy permit has previously been used.";
                    return View();
                }
            }
            if (pm != null)
            {
                //check entered permit expiry date
                if (DateTime.Now > pm.Date_Expire && (pm.PhaseShortName == "NDT" || pm.PhaseShortName == "LTO" || pm.PhaseShortName == "LR" || pm.PhaseShortName == "TO" || pm.PhaseShortName == "RC"))
                {
                    //permit has expired, then check if expiry is even applicable to permit app type
                    if (pm.PhaseShortName == "NDT" && phaseId != 11 && phaseId != 5) //Calibration Expiry and you are not applying for recalibration or depot modification
                    {
                        ViewBag.ErrorMessage = "Sorry, the calibration approval number you have supplied has expired, kindy apply for re-calibration before applying for LTO.";
                        return View();
                    }
                    else if (pm.PhaseShortName == "RC" && phaseId != 11 && phaseId != 5) //Re-calibration Expiry and you are not applying for recalibration or depot modification
                    {
                        ViewBag.ErrorMessage = "Sorry, the re-calibration approval number you have supplied has expired, kindy apply for another re-calibration before applying for LTO.";
                        return View();
                    }
                    else if ((pm.PhaseShortName == "LTO" || pm.PhaseShortName == "TO") && phaseId != 7 && phaseId != 5) //License To Operate expiry and you are not applying for license renewal or depot modification
                    {

                        ViewBag.ErrorMessage = "Sorry, the LTO number you have supplied has expired, kindy apply for license renewal.";
                        return View();
                    }
                    else if (pm.PhaseShortName == "LR" && phaseId != 7 && phaseId != 5) //Licence Renewal expiry and you are not applying for license renewal or depot modification
                    {

                        ViewBag.ErrorMessage = "Sorry, the license number you have supplied has expired, kindy apply for license renewal.";
                        return View();
                    }

                }


                else
                {

                    #region check supplied permit suitability for dselected application
                    if (pm.PhaseShortName == "SI" && selectedPhase.ShortName != "ATC")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval number you have entered is for Suitability and can only be used to apply for ATC (Approval to Construct). Kindly apply for calibration/recalibration before applying for LTO.";
                        return View();
                    }
                    if (pm.PhaseShortName != "SI" && selectedPhase.ShortName == "ATC")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for ATC (Approval to Construct).";
                        return View();
                    }
                    if ((pm.PhaseShortName != "ATC" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "NDT")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "NDT" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "RC")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if (pm.PhaseShortName != "NDT" && selectedPhase.ShortName == "LTO")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "ATC" && pm.PhaseShortName != "REG" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "DM")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "ATC" && pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "TO")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && selectedPhase.ShortName == "LR")
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }
                    if ((pm.PhaseShortName != "LTO" && pm.PhaseShortName != "LR" && pm.PhaseShortName != "TO") && (selectedPhase.ShortName == "UWA" || selectedPhase.ShortName == "SAP"))
                    {
                        ViewBag.ErrorMessage = "Sorry, the approval/license number you have entered is for " + pm.PhaseName + " and it is not suitable for " + selectedPhase.name + ".";
                        return View();
                    }

                    #endregion
                    switch (pm.PhaseId)
                    {
                        case 1:
                        case 8:
                        case 2:

                            #region current permit is for ATC or regularization ==> move to Calibration (NDT)
                            //Current permit is ATC and selected phase is calibration so proceed
                            if ((pm.PhaseId == 2 || pm.PhaseId == 8) && phaseId == 3)
                            {
                                return RedirectToAction("Application", "LTO", new { id = pm.Application_Id, phaseId = phaseId });
                            }
                            else if (pm.PhaseId == 2 && phaseId == null && !string.IsNullOrEmpty(category) && int.Parse(category) == 7)
                            {
                                if (int.Parse(category) == 7)
                                {
                                    int catid = int.Parse(category);
                                    phaseId = _context.Phases.Where(x => x.category_id == catid).FirstOrDefault().id;
                                    return RedirectToAction("Application", "LTO", new { id = pm.Application_Id, phaseId = phaseId });

                                }
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Sorry, the provided license number can not be used for this type of application.";
                            }
                            if (phaseId == 3 || phaseId == 2)
                                return RedirectToAction("TankInspection", "ATC", new { id = pm.FacilityId, phaseId = phaseId, permitId = pm.Permit_No });
                            break;
                        #endregion
                        case 3:
                            #region application for Tank Pressure Leak Test ==> move to LTO

                            if (phaseId == 4)
                            {
                                if (!pm.Permit_No.Contains("PLT")) //check if provided permit number is for calibration before applying for LTO
                                {

                                    TempData["ErrorMessage"] = "The provided approval number is not for calibration, kindly provide your calibration approval number before you can proceed with LTO.";
                                    return RedirectToAction("Apply");
                                }
                                else if (phaseId == 4 && pm.Date_Expire > DateTime.UtcNow.AddHours(1))
                                {
                                    return RedirectToAction("ReviewFacilityTank", "Application", new { id = pm.FacilityId, permitId = permitId, category = category, phaseId = phaseId, leg = 1 });
                                    //return RedirectToAction("Application", "LTO", new { id = pm.Application_Id, phaseId = phaseId });
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "The provided calibration approval number is expired, kindly apply for recalibration before you can proceed with LTO.";
                                    return RedirectToAction("Apply");
                                }
                            }
                            break;
                        #endregion
                        default: break;

                    }

                    ViewBag.ErrorMessage = "The provided License/Approval/Approval Number Can not be used for this type of Application.";
                }
            }
            else
            {
                if (permitId.ToLower() == "atc" && phaseId == 2)
                {
                    var ps = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();
                    return RedirectToAction("Review", "LTO", new { permitId = permitId, category = ps.category_id, phaseId = phaseId, leg = 0 });

                }
                if (permitId.ToLower() == "reg" && (category == "1002" || category == "8"))
                {
                    var ps = _context.Phases.Where(a => a.category_id.ToString() == category).FirstOrDefault();
                    return RedirectToAction("Review", "LTO", new { permitId = permitId, category = ps.category_id, phaseId = ps.id, leg = 0 });

                }
                var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId).FirstOrDefault();
                if (lg != null)
                {
                    //check if legacy has not been used before
                    if (lg.IsUsed == true)
                    {
                        ViewBag.ErrorMessage = "Sorry, you can't proceed with this application as the legacy permit has previously been used.";

                    }
                    else
                    {
                        bool move = false;
                        var ps = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();
                        if (ps != null && ps.category_id == 1)
                        {
                            if (phaseId == 2 && (lg.AppType == "Suitability Inspection" || lg.AppType == "SI"))
                            {
                                move = true;
                            }
                            else if (phaseId == 3 && (lg.AppType == "ATC" || lg.AppType == "Approval To Construct"))
                            {
                                move = true;
                            }
                            else if (phaseId == 4 && (lg.AppType == "NDT" || lg.AppType == "Calibration/Integrity Tests(NDTs)"))
                            {
                                move = true;
                            }
                        }
                        if (move)
                        {
                            return RedirectToAction("Review", "LTO", new { permitId = permitId, category = ps.category_id, phaseId = phaseId, leg = 1 });
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Kindly ensure that your legacy approval/permit is the requirement for the next application phase";

                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Permit/License with the provided reference number was not found. Please, check the reference number and try again.";

                }

            }
            return View();
            // return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(errMessage) });
        }

        public IActionResult ReviewFacilityTank(int id, string permitId, string category, string phaseId, string sid)
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {

                if (CompanyID == 0)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, company ID is incorrect. Please try again later") });
                }
                var cid = int.Parse(category);
                var pid = int.Parse(phaseId);
                string phaseName = "";
                var phase = _context.Phases.Where(x => x.id == pid).FirstOrDefault();
                if (phase != null)
                {
                    phaseName = phase.name;
                }

                var fac = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
                if (fac != null)
                {

                    var FacAdd = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();
                    if (FacAdd != null)
                    {
                        var FacState = _context.States_UT.Where(a => a.State_id == FacAdd.StateId).FirstOrDefault();
                        ViewBag.FacilityAddress = FacAdd.address_1 + ", " + FacAdd.city + " " + FacState.StateName;
                    }


                    RenewModel renew = new RenewModel();
                    permits pm = null;
                    if (!string.IsNullOrEmpty(permitId))
                    {
                        pm = _context.permits.Where(a => a.permit_no.ToLower() == permitId.ToLower()).FirstOrDefault();//  new permits { PhaseId = phs.Id, Permit_No = permitId };

                    }
                    if (pm == null)
                    {
                        Legacies lg = null;
                        if (!string.IsNullOrEmpty(permitId))
                        {
                            lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId.ToLower()).FirstOrDefault();

                        }

                        if (lg != null)
                        {
                            ViewBag.legacy = lg;
                            ViewBag.frmLegacy = "yes";
                        }

                        renew = _helpersController.GetRenewalModel(lg, fac);
                    }
                    else
                    {
                        renew = _helpersController.GetRenewalModel(fac, pm);
                    }

                    //check if the facility already have tanks
                    ViewBag.PermitNumber = phase.ShortName.ToLower() == "reg" ? "REGULARIZATION" : phase.name.ToUpper();
                    var tnks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned).ToList();
                    if (tnks.Count > 0)
                    {
                        renew.Tanks = tnks;
                    }
                    if (!string.IsNullOrEmpty(category))
                    {
                        ViewBag.review = "yes";
                    }
                    if (string.IsNullOrEmpty(phaseId))
                    {

                        var phs = _context.Phases.Where(a => a.category_id == cid).FirstOrDefault();
                        if (phs != null)
                        {
                            phaseId = phs.id.ToString();

                        }
                    }
                    _helpersController.LogMessages($"Number of Tanks:: {renew.Tanks.Count} and Products:: {renew.Products.Count} inside addTanks for {renew.Company.name}");
                    ViewBag.SanctionId = sid;
                    ViewBag.category = category;
                    ViewBag.phaseId = phaseId;
                    ViewBag.phaseName = phaseName;
                    ViewBag.tanks = renew.Tanks;
                    ViewBag.facility = fac;
                    ViewBag.pumps = _context.Pumps.Where(x => x.FacilityId == fac.Id).ToList();
                    ViewBag.products = _context.Products.ToList();

                    var suit = _context.SuitabilityInspections.Where(st => st.FacilityId == fac.Id).FirstOrDefault();
                    if (suit != null)
                    {
                        ViewBag.landsize = suit.SizeOfLand;
                        ViewBag.isonhighway = suit.IsOnHighWay == true ? "Yes" : "No";
                        ViewBag.ispipeline = suit.ISAlongPipeLine == true ? "Yes" : "No";
                        ViewBag.IsUnderHighTension = suit.IsUnderHighTension == true ? "Yes" : "No";
                    }
                    return View();
                }
                ViewBag.ErrorMessage = "No Depot is Assocciated with the provided License Number.";
                return View();

            }
            catch (Exception x)
            {
                _helpersController.LogMessages("Error Fetching Facility/Tank Information For {phaseName} Application: " + x.Message.ToString(), CompanyName);


                ViewBag.ErrorMessage = "Sorry, an error occured while handling your request";


                return View();
            }
        }

        [HttpPost]
        public IActionResult ReviewFacilityTank(List<Pumps> Pumps, OtherModel om, string FacilityAddress, string AppType)
        {
            try
            {
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

                if (Pumps.Count > 0)
                {
                    var fac = _context.Facilities.Where(a => a.Id == om.facilityId).FirstOrDefault();
                    if (fac != null)
                    {
                        fac.Name = om.FacilityName;
                        fac.ContactName = om.ContactName;
                        fac.ContactNumber = om.ContactNumber;
                        fac.Date = DateTime.Now;
                        //fac.address_1 = FacilityAddress;

                        _context.SaveChanges();

                        //var suit = _context.SuitabilityInspections.Where(st => st.FacilityId == fac.Id).FirstOrDefault();
                        //if(suit != null)
                        //{
                        //    suit.ISAlongPipeLine = suit.ISAlongPipeLine;
                        //    suit.IsOnHighWay = suit.IsOnHighWay;
                        //    suit.SizeOfLand = om.SizeOfLand;
                        //    suit.IsUnderHighTension =om.IsUnderHighTension;
                        //    _context.SaveChanges();

                        //}

                        var facilityAddress = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();

                        #region Update Facility on ELPS 
                        if (fac.Elps_Id == null || fac.Elps_Id.Value <= 0)
                        {

                            FacilityAPIModel facmodel = new FacilityAPIModel()
                            {
                                Name = fac.Name,
                                FacilityType = "Depot",
                                LGAId = (int)facilityAddress.LgaId,
                                City = facilityAddress.city,
                                StateId = facilityAddress.StateId,
                                StreetAddress = facilityAddress.address_1
                            };

                            var param = JsonConvert.SerializeObject(facmodel);
                            var paramDatas = _restService.parameterData("fac", param);
                            var response = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facmodel);
                            if (response.IsSuccessful == false)
                            {
                                _helpersController.LogMessages("Error posting facility with name: " + facmodel.Name + " to ELPS.", userEmail);
                            }
                            var respApp = JsonConvert.DeserializeObject<FacilityAPIModel>(response.Content.ToString());

                            if (respApp != null && respApp.Id > 0)
                            {
                                fac.Elps_Id = respApp.Id;
                                _context.SaveChanges();
                            }
                        }
                        #endregion

                        Pumps pp;
                        foreach (var item in Pumps)
                        {
                            if (item.Id > 0)
                            {
                                // Edit Pump
                                pp = _context.Pumps.Where(a => a.Id == item.Id).FirstOrDefault();
                                if (pp == null)
                                {
                                    pp = new Pumps();
                                    pp.FacilityId = om.facilityId;
                                    pp.Manufacturer = string.IsNullOrEmpty(item.Manufacturer) ? "NA" : item.Manufacturer;
                                    pp.Name = item.Name;
                                    pp.TankId = item.TankId;
                                    _context.Pumps.Add(pp);
                                }
                                else
                                {
                                    pp.FacilityId = om.facilityId;
                                    pp.Manufacturer = string.IsNullOrEmpty(item.Manufacturer) ? "NA" : item.Manufacturer;
                                    pp.Name = item.Name;
                                }
                            }
                            else
                            {
                                pp = new Pumps();
                                pp.FacilityId = om.facilityId;
                                pp.Manufacturer = string.IsNullOrEmpty(item.Manufacturer) ? "NA" : item.Manufacturer;
                                pp.Name = item.Name;
                                pp.TankId = item.TankId;
                                _context.Pumps.Add(pp);
                            }
                        }
                        _context.SaveChanges();



                        if (om.phaseId == 11 || om.phaseId == 7)
                        {
                            AppType = "renew";
                        }
                        var ph = _context.Phases.Where(a => a.id == om.phaseId).FirstOrDefault();

                        if (ph != null)
                        {

                            switch (ph.category_id)
                            {

                                case 4:
                                    return RedirectToAction("Takeover", "LTO", new { id = om.facilityId, permitId = om.PermitNo, category = om.category });

                                default:
                                    break;
                            }
                        }


                        var app = _context.applications.Where(a => a.FacilityId == om.facilityId && a.PhaseId == om.phaseId
                        && a.company_id == fac.CompanyId && a.status == GeneralClass.PaymentPending).FirstOrDefault();
                        string pmNo = "atc";
                        //check if application is LTO and applicant does not have mistdo
                        //if (app != null && (ph.ShortName == "LTO" || ph.ShortName == "LR" || ph.ShortName == "TO"))
                        //{
                        //    var mistdo = _context.MistdoStaff.Where(x => x.FacilityId== om.facilityId && x.DeletedStatus == false).ToList();

                        //    if ( mistdo.Count() < 5)
                        //    {
                        //        return RedirectToAction("Mistdo", "Companies", new { id = generalClass.Encrypt(app.id.ToString()) });
                        //    }

                        //}
                        if (om.PermitNo != null)
                        {
                            pmNo = om.PermitNo.ToLower();
                        }
                        if (app != null)
                        {
                            var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == pmNo).FirstOrDefault();
                            if (lg != null)
                            {
                                lg.IsUsed = true;
                                _context.SaveChanges();
                            }

                            return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(app.id.ToString()) });
                        }
                        return RedirectToAction("CreateApplication", "LTO", new { id = om.facilityId, om.phaseId, pNo = string.IsNullOrEmpty(om.PermitNo) ? "" : om.PermitNo, type = string.IsNullOrEmpty(AppType) ? "new" : AppType });
                    }
                }
                string err = "Please fill atleast one loading arm for the system to be able to continue with your application";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });

            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                throw;
            }
        }

        #region Payment


        [AllowAnonymous]
        [HttpPost]
        public IActionResult RemitaConfirm(string orderId)
        {
            var app = _context.applications.Where(a => a.reference == orderId).FirstOrDefault();
            var company = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();

            var trans = _context.remita_transactions.Where(a => a.reference_number == orderId).FirstOrDefault();

            if (app == null && trans == null)
            {

                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application or remita transaction was not found. Kindly contact support.") });
            }
            else
            {

                app.status = GeneralClass.PaymentCompleted;
                _context.SaveChanges();

                trans.response_description = GeneralClass.Approved;
                trans.response_code = "01";
                _context.SaveChanges();

                var vapp = _context.applications.Where(a => a.id == app.id && a.DeleteStatus != true).FirstOrDefault();

                GiveRemitaValue(app, trans.id, trans.RRR, company.name, DateTime.Now);

                SendApplicationSubmittedMail(vapp, app.current_Permit);


                //Move to processing after Payment
                if (CreateProcessingRules(app.id))
                {
                    ViewBag.Message = "Transaction Approved";
                    ViewBag.RRR = trans.RRR;
                    ViewBag.OrderId = orderId;
                    ViewBag.AppId = app.id;
                    return RedirectToAction("PaymentSuccess", new { appId = app.id, rrr = trans.RRR, msg = "Transaction Approved", orderId = orderId });
                }
                return RedirectToAction("RemitaFailure", new { status = "Transaction Not Successful", rrr = trans.RRR, orderId = orderId });
            }

        }

        public IActionResult Payment(string apid, string refCode)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int appID = generalClass.DecryptIDs(apid);

            #region get Application  & Transaction
            if (string.IsNullOrEmpty(refCode) || appID <= 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, No Reference Code or Applicatioin Id was Supplied") });

            }

            var getApp = _context.applications.Where(a => a.id == appID).FirstOrDefault();
            var getApp2 = (from u in _context.applications
                           join c in _context.Categories on u.category_id equals c.id
                           join p in _context.Phases on u.PhaseId equals p.id
                           where u.id == appID
                           select new MyApps
                           {
                               app = u,
                               cat = c,
                               phs = p
                           }
                           ).FirstOrDefault();
            remita_transactions ptrans = _context.remita_transactions.Where(C => C.order_id == getApp.reference.ToLower()).FirstOrDefault();
            if (ptrans != null && !string.IsNullOrEmpty(ptrans.RRR))
            {
                ViewBag.rrr = ptrans.RRR.Trim();
                if (ptrans.RRR.ToLower() == "DPR-ELPS".ToLower() || ptrans.RRR.ToLower() == "DPR-Bank-M".ToLower())
                {
                    //0 Naira application Or Bank

                    if (CreateProcessingRules(getApp.id))
                    {
                        SendApplicationSubmittedMail(getApp, getApp.current_Permit);

                        ViewBag.OrderId = getApp.reference;
                        MyApps MyApp = (from app in _context.applications.AsEnumerable()
                                        join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                        join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id
                                        join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                        join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                        join ad in _context.addresses on facil.AddressId equals ad.id
                                        where app.DeleteStatus != true && app.id == getApp.id && c.DeleteStatus != true
                                        select new MyApps
                                        {
                                            appID = app.id,
                                            Reference = app.reference,
                                            CategoryName = cat.name,
                                            PhaseName = phs.name,
                                            category_id = cat.id,
                                            FacilityId = facil.Id,
                                            PhaseId = phs.id,
                                            AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),
                                            Current_Permit = "",
                                            //Stage = s.StageName,
                                            Address_1 = ad.address_1,
                                            Status = app.status,
                                            Date_Added = Convert.ToDateTime(app.date_added),
                                            Submitted = app.submitted,
                                            CompanyDetails = c.name + " (" + c.Address + ") ",
                                            FacilityDetails = facil.Name,

                                        }).FirstOrDefault();

                        return View("ApplicationSuccess", MyApp);

                    }
                    else
                    {
                        string err = "There was an error creating process rules for this application, please contact the NMDPRA support";
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });
                    }
                }
                var returnUrl = ElpsServices._elpsBaseUrl + "Payment/Pay?rrr=" + ViewBag.rrr;
                return Redirect(returnUrl);
            }
            else
            {

                //New Addition
                var company = _context.companies.Where(a => a.id == getApp.company_id).FirstOrDefault();

                if (company.elps_id.GetValueOrDefault() < 0)
                {

                }
                else
                {
                    var paramData = _restService.parameterData("id", company.elps_id.ToString());
                    var response = _restService.Response("/api/company/{id}/{email}/{apiHash}", paramData, "GET", null); // GET

                    if (response.IsSuccessful == true)
                    {
                        var companyModels = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);
                        company.CompanyEmail = companyModels.user_Id;
                        company.name = companyModels.name;
                        //Update Local company with elpsId
                        company.elps_id = Convert.ToInt32(companyModels.id);
                    }

                    _context.SaveChanges();
                }
                return View(getApp2);

            }
            #endregion
        }
        //[HttpPost]

        //[Authorize(Roles = ("Admin, DPRAdmin,Account,Support"))]
        public IActionResult RegenerateFee(string id)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            string refCode = id;
            var getApp = _context.applications.Where(a => a.reference == refCode).FirstOrDefault();
            if (getApp != null)
            {

                var fac = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
                //New Addition
                string output = "";
                var company = _context.companies.Where(a => a.id == getApp.company_id).FirstOrDefault();
                var address = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();
                var apts = _context.ApplicationTanks.Where(a => a.ApplicationId == getApp.id).ToList();
                double tnkV = 0;
                int tnkCnt = 0;
                if (apts.Count > 0)
                {
                    tnkV = apts.Sum(a => a.Capacity);
                    tnkCnt = apts.Count;
                }
                else
                {
                    var facTanks = _context.Tanks.Where(a => a.FacilityId == getApp.FacilityId).ToList();
                    tnkV = facTanks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                    tnkCnt = facTanks.Count;
                }
                Categories category = _context.Categories.Where(c => c.id == getApp.category_id).FirstOrDefault();
                var phase = _context.Phases.Where(a => a.id == getApp.PhaseId).FirstOrDefault();
                var descrptn = getApp.PaymentDescription != null && getApp.PaymentDescription.Contains("|") ? getApp.PaymentDescription.Split('|') : null;
                bool frmAtc = descrptn != null && descrptn.Count() > 1 ? true : false;
                _helpersController.LogMessages($"current app amount:: {getApp.fee_payable} and total volume: {tnkV}");
                var feeDesc = _helpersController.CalculateAppFee(phase, getApp.current_Permit, tnkV, tnkCnt, frmAtc);

                _helpersController.LogMessages($"New app amount:: {feeDesc.Fee}");
                if (getApp.status == "Payment Pending")
                {
                    getApp.fee_payable = feeDesc.Fee;
                    getApp.reference = generalClass.Generate_Application_Number();
                    getApp.PaymentDescription = feeDesc.FeeDescription + $" Previous reference ({refCode}) has been changed to: {getApp.reference}";

                    _helpersController.LogMessages($"New Reference Number:: {getApp.reference}");

                    _context.SaveChanges();
                }
                double Fee = Convert.ToDouble(getApp.service_charge + getApp.fee_payable);

                //Create Invoice before redirecting
                var invo = _context.invoices.Where(a => a.application_id == getApp.id).FirstOrDefault();


                if (invo == null)
                {
                    invo = new invoices();
                    invo.amount = Convert.ToDouble(getApp.fee_payable + getApp.service_charge);
                    invo.application_id = getApp.id;
                    invo.payment_code = getApp.reference;
                    invo.payment_type = string.Empty;
                    invo.status = Fee > 0 ? "Unpaid" : "Paid";
                    invo.date_added = Fee > 0 ? DateTime.Now.AddDays(-7) : DateTime.Now;

                    _context.invoices.Add(invo);
                    _context.SaveChanges();
                }
                else
                {
                    invo.amount = Convert.ToDouble(getApp.fee_payable + getApp.service_charge);

                    invo.payment_code = getApp.reference;
                    _context.SaveChanges();
                }

                string un = userEmail;
                #region Remita Payment Split Builder
                var rmSplit = new RemitaSplit();
                var listItem = new List<LpgLicense.Models.RPartner>();

                rmSplit.payerPhone = company.contact_phone;

                var fp = getApp.fee_payable.ToString().Split('.');
                rmSplit.orderId = getApp.reference;
                rmSplit.CategoryName = category.name;
                rmSplit.payerEmail = userEmail;
                rmSplit.payerName = company.name;
                rmSplit.AmountDue = fp[0];
                rmSplit.ReturnBankPaymentUrl = _configuration.GetSection("AmountSetting").GetSection("ReturnBankPaymentUrl").Value.ToString();
                rmSplit.ReturnFailureUrl = _configuration.GetSection("AmountSetting").GetSection("ReturnFailureUrl").Value.ToString();
                rmSplit.ReturnSuccessUrl = _configuration.GetSection("AmountSetting").GetSection("ReturnSuccessUrl").Value.ToString();
                rmSplit.ServiceCharge = getApp.service_charge.ToString();
                rmSplit.totalAmount = (Convert.ToDouble(rmSplit.AmountDue) + Convert.ToDouble(rmSplit.ServiceCharge)).ToString();
                var apItem = new List<ApplicationItem>();
                apItem.Add(new ApplicationItem { Group = category.name, Name = phase.name + ": " + phase.Description });
                apItem.Add(new ApplicationItem { Group = category.name, Name = "Facility Name: " + fac.Name + "(" + address.address_1 + ")" });
                apItem.Add(new ApplicationItem { Group = category.name, Name = "Payment Description: " + getApp.PaymentDescription });
                rmSplit.ApplicationItems = apItem;

                #endregion
                getApp.fee_payable = Convert.ToDecimal(fp[0]);
                JArray lineItems = new JArray();
                JObject lineItem1 = new JObject();
                lineItem1.Add("lineItemsId", "1");
                lineItem1.Add("beneficiaryName", _configuration.GetSection("RemitaSplit").GetSection("BeneficiaryName").Value.ToString());
                lineItem1.Add("beneficiaryAccount", _configuration.GetSection("RemitaSplit").GetSection("BeneficiaryAccount").Value.ToString());
                lineItem1.Add("bankCode", _configuration.GetSection("RemitaSplit").GetSection("BankCode").Value.ToString());
                lineItem1.Add("beneficiaryAmount", (getApp.fee_payable + getApp.service_charge).ToString());
                lineItem1.Add("deductFeeFrom", "1");

                lineItems.Add(lineItem1);

                #region IGR Take Over Payment Region
                if (category.name.ToLower().Contains("take over"))
                {
                    double fivepercent = 0.00;
                    var revenueID = _configuration.GetSection("RemitaSplit").GetSection("RevenueId").Value.ToString();
                    var revItems = new[] { new{
                        revenueID,
                        Amount = 0.00,
                        Quantity = 1 } };
                    double TFCost = Convert.ToDouble(getApp.TransferCost);
                    fivepercent = (5.00 / 100) * TFCost;
                    double TSA = TFCost - fivepercent;
                    revItems = new[] { new{
                        revenueID,
                        Amount = fivepercent,
                        Quantity = 1
                    }
                 };



                    var TargetAccount = _configuration.GetSection("RemitaSplit").GetSection("TargetAccount").Value.ToString();
                    var BankCode = _configuration.GetSection("RemitaSplit").GetSection("BankCode").Value.ToString();


                    listItem.Add(new LpgLicense.Models.RPartner
                    {
                        lineItemsId = "1",
                        beneficiaryName = "Beneficiary Target",
                        beneficiaryAccount = TargetAccount,
                        bankCode = BankCode,
                        beneficiaryAmount = fivepercent.ToString(),
                        deductFeeFrom = "0"
                    });



                }
                #endregion
                rmSplit.lineItems = listItem;
                var jn = JsonConvert.SerializeObject(rmSplit);

                #region remita split



                var paramDatas = _restService.parameterData("CompId", company.elps_id.ToString());

                var response = _restService.Response("api/Payments/{CompId}/{email}/{apiHash}", paramDatas, "POST", rmSplit);

                var resz = JsonConvert.DeserializeObject<JObject>(response.Content);

                var resp = JsonConvert.DeserializeObject<PrePaymentResponse>(resz.ToString());

                output = output.Replace("jsonp(", "");
                output = output.Replace(")", "");

                #endregion

                string eMsg = "";
                if (resp == null || string.IsNullOrEmpty(resp.RRR))
                {
                    if (!string.IsNullOrEmpty(eMsg))
                        ViewBag.RError = eMsg;
                    if (resp != null && string.IsNullOrEmpty(resp.RRR))
                        ViewBag.RError2 = "Status Code: " + resp.Status + "; Status Mesage: " + resp.StatusMessage;
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ViewBag.RError2) });
                }

                ViewBag.rrr = resp.RRR.Trim();
                ViewBag.webPayData = rmSplit;
                remita_transactions ptrans = _context.remita_transactions.Where(a => a.order_id == refCode).FirstOrDefault();// null;
                int tid = RecordRemitaTransaction(ptrans, Fee, getApp, resp, company.name);
                #endregion

                if (resp.RRR.ToLower() == "DPR-ELPS".ToLower() || resp.RRR.ToLower() == "DPR-Bank-M".ToLower())
                {
                    //0 Naira application Or Bank
                    var vapp = _context.applications.Where(a => a.id == getApp.id).FirstOrDefault();
                    SendApplicationSubmittedMail(vapp, getApp.current_Permit);

                    if (CreateProcessingRules(getApp.id))
                    {
                        SendApplicationSubmittedMail(vapp, getApp.current_Permit);

                        ViewBag.OrderId = getApp.reference;
                        MyApps MyApp = (from app in _context.applications.AsEnumerable()
                                        join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                        join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id
                                        join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                        join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                        join ad in _context.addresses on facil.AddressId equals ad.id
                                        where app.DeleteStatus != true && app.id == getApp.id && c.DeleteStatus != true
                                        select new MyApps
                                        {
                                            appID = app.id,
                                            Reference = app.reference,
                                            CategoryName = cat.name,
                                            PhaseName = phs.name,
                                            category_id = cat.id,
                                            FacilityId = facil.Id,
                                            PhaseId = phs.id,
                                            AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),
                                            Current_Permit = app?.current_Permit,
                                            //Stage = s.StageName,
                                            Address_1 = ad.address_1,
                                            Status = app.status,
                                            Date_Added = Convert.ToDateTime(app.date_added),
                                            Submitted = app.submitted,
                                            CompanyDetails = c.name + " (" + c.Address + ") ",
                                            FacilityDetails = facil.Name,

                                        }).FirstOrDefault();

                        return View("ApplicationSuccess", MyApp);
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong while trying to submit this application for processing. Kindly contact support.") });
                    }
                }


                else
                {

                    //Regeneration of fee was successful
                    var getApp2 = (from u in _context.applications
                                   join c in _context.Categories on u.category_id equals c.id
                                   join p in _context.Phases on u.PhaseId equals p.id
                                   where u.id == getApp.id
                                   select new MyApps
                                   {
                                       app = u,
                                       cat = c,
                                       phs = p
                                   }).FirstOrDefault();

                    return View(getApp2);
                }
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or have been deleted. Kindly contact support.") });

            }

        }

        [Authorize(Policy = "CompanyRoles")]
        public IActionResult PaymentFail(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }

            var apps = _context.applications.Where(x => x.reference == id && x.DeleteStatus != true);

            if (apps.Count() > 0)
            {
                remita_transactions ptrans = _context.remita_transactions.Where(a => a.order_id == apps.FirstOrDefault().reference).FirstOrDefault();

                _helpersController.LogMessages("Application payment failed. RRR => " + ptrans.RRR, _helpersController.getSessionEmail());

                return View(apps.ToList());
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or have been deleted. Kindly contact support.") });
            }
        }

        public IActionResult SubmitPayment(string apid, string refCode, bool bypass = false)

        {

            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            int id = generalClass.DecryptIDs(apid);

            refCode = generalClass.Decrypt(refCode);



            if (string.IsNullOrEmpty(refCode) || id <= 0)

            {

                string msg = "Sorry, no reference code or application ID was supplied.";

                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(msg) });



            }

            string subject = ""; string content = "";


            var getApp = _context.applications.Where(a => a.id == id).FirstOrDefault();

            var fac = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();

            var add = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();

            var state = _context.States_UT.Where(a => a.State_id == add.StateId).FirstOrDefault();



            //New Addition

            var company = _context.companies.Where(a => a.id == getApp.company_id).FirstOrDefault();

            var aptnks = _context.ApplicationTanks.Where(a => a.FacilityId == getApp.FacilityId && a.ApplicationId == getApp.id).ToList();

            double tnkV = 0;

            int tnkCount = 0;

            if (aptnks.Count > 0)

            {

                tnkV = aptnks.Sum(a => a.Capacity);

                tnkCount = aptnks.Count;

            }

            else

            {

                var facTanks = _context.Tanks.Where(a => a.FacilityId == getApp.FacilityId).ToList();

                tnkCount = facTanks.Count;

                tnkV = facTanks.Sum(a => Convert.ToDouble(a.MaxCapacity));

            }

            Categories category = _context.Categories.Where(c => c.id == getApp.category_id).FirstOrDefault();

            var phase = _context.Phases.Where(a => a.id == getApp.PhaseId).FirstOrDefault();

            var descrptn = getApp.PaymentDescription != null ? getApp.PaymentDescription.Split('|') : null;

            bool frmAtc = descrptn == null ? false : (descrptn.Count() > 1 ? true : false);

            var feeDesc = _helpersController.CalculateAppFee(phase, getApp.current_Permit, tnkV, tnkCount, frmAtc, (decimal)getApp.TransferCost);

            if (getApp.fee_payable != feeDesc.Fee)

            {

                getApp.fee_payable = feeDesc.Fee;

                getApp.PaymentDescription = feeDesc.FeeDescription;

                _context.SaveChanges();

            }

            double Fee = Convert.ToDouble(getApp.service_charge + getApp.fee_payable);



            //Create Invoice before redirecting

            var invo = _context.invoices.Where(a => a.application_id == getApp.id).FirstOrDefault();

            if (invo == null)

            {

                invo = new invoices();

                invo.amount = Convert.ToDouble(getApp.fee_payable + getApp.service_charge);

                invo.application_id = getApp.id;

                invo.payment_code = getApp.reference;

                invo.payment_type = string.Empty;

                invo.status = Fee > 0 ? "Unpaid" : "Paid";

                invo.date_added = DateTime.Now;

                invo.date_paid = Fee > 0 ? DateTime.Now.AddDays(-7) : DateTime.Now;

                _context.invoices.Add(invo);

                _context.SaveChanges();

            }


            if (getApp.status == GeneralClass.PaymentCompleted || phase.ShortName == "SI")

            {

                ViewBag.rrr = getApp.reference;

                //0 Naira application Or Bank

                //Assign application to a staff

                int stateID = _helpersController.GetApplicationState(getApp.id);

                #region appProcess

                // getting application process for single

                List<WorkProccess> process = _helpersController.GetAppProcess(getApp.PhaseId, getApp.category_id, 0, 0);

                if (process.Count <= 0)

                {

                    string err = "Something went wrong while trying to get work process for your application. Please try again or contact Support.";

                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });



                }

                var fm = _context.FacilityModifications.Where(a => a.ApplicationId == getApp.id).FirstOrDefault();

                string modType = null;

                if (fm != null)

                {

                    if (fm.Type.Contains("clusion") || fm.Type.Contains("version"))

                        modType = fm.Type;

                }

                int AppDropStaffID = _helpersController.ApplicationDropStaff(getApp.id, getApp.PhaseId, getApp.category_id, stateID, 0, modType);



                if (AppDropStaffID <= 0)

                {

                    string err = "Something went wrong while trying to send your application to a staff for processing. Please try again or contact Support.";

                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });



                }

                else

                {



                    //add to old table

                    var staffUN = _context.Staff.Where(x => x.StaffID == AppDropStaffID).FirstOrDefault();



                    MyDesk drop = new MyDesk()

                    {

                        ProcessID = process.FirstOrDefault().ProccessID,

                        Sort = process.FirstOrDefault().Sort,

                        AppId = getApp.id,

                        StaffID = AppDropStaffID,

                        FromStaffID = 0,

                        HasWork = false,

                        HasPushed = false,

                        CreatedAt = DateTime.Now

                    };



                    // dropping application on staff desk

                    _context.MyDesk.Add(drop);

                    int appDrop = _context.SaveChanges();



                    if (appDrop > 0)

                    {



                        //Check if app is submitted or re-submit

                        if (getApp.submitted == true)

                        {

                            #region Resubmission

                            subject = ("Application Re-Submitted With Reference: ") + getApp.reference;

                            content = string.Format("Your Licence Application has been Re-submited on the Depot portal. The process for your application with Reference Number: {0} will continue immediately.<br />", getApp.reference);

                            TempData["AppSubmitType"] = "Resubmit";

                            #endregion

                        }

                        else

                        {

                            #region Fresh Submission

                            TempData["AppSubmitType"] = "New";

                            subject = ("Application Submitted With Reference: ") + getApp.reference;

                            content = string.Format("Thank you for submitting your application on the Depot portal. Your application with Reference Number: {0} will be reviewed and permit issued within seventy-two (72) hour.<br />The table below shows the breakdown of the funds received.", getApp.reference);

                            #endregion

                        }







                        getApp.CreatedAt = DateTime.Now;

                        getApp.UpdatedAt = DateTime.Now;

                        getApp.status = GeneralClass.Processing;

                        getApp.submitted = true;

                        getApp.current_desk = AppDropStaffID;



                        if (_context.SaveChanges() > 0)

                        {

                            _helpersController.SaveHistory(getApp.id, AppDropStaffID, "Moved", "Application submitted and landed on staff desk for distribution.");





                            // Send Mail to Company


                            var emailMsg = _helpersController.SaveMessage(getApp.id, Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID))), subject, content, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionElpsID)), "Company");

                            var sendEmail = _helpersController.SendEmailMessage(company.CompanyEmail.ToString(), company.name, emailMsg, null);



                            ViewBag.OrderId = getApp.reference;

                            //Send Mail to Staff

                            string type = category.id == 1 ? category.name + "(" + phase.name + ")" : category.name;

                            string subj = "<b>" + type + "</b>" + " Application (" + getApp.reference + ") pushed to your desk";

                            string cont = "A " + "<b>" + type + "</b>" + " application with reference number " + getApp.reference + " has been submitted on your desk for processing.";



                            var processor = _context.Staff.Where(a => a.StaffID == AppDropStaffID).FirstOrDefault();



                            var emailMsg2 = _helpersController.SaveMessage(getApp.id, processor.StaffID, subject, cont, processor.StaffElpsID, "Staff");

                            var sendEmail2 = _helpersController.SendEmailMessage2Staff(processor.StaffEmail, processor.FirstName, emailMsg2, null);



                            var getAppList = _context.applications.Where(a => a.id == id).ToList();

                            _helpersController.UpdateElpsApplication(getAppList);



                            _helpersController.LogMessages("Application submitted successfully with reference : " + getApp.reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                            MyApps MyApp = (from app in _context.applications.AsEnumerable()

                                            join c in _context.companies.AsEnumerable() on app.company_id equals c.id

                                            join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id

                                            join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id

                                            join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id

                                            join ad in _context.addresses on fac.AddressId equals ad.id

                                            //join sb in _context.SubmittedDocuments.AsEnumerable() on app.id equals sb.AppID

                                            // orderby app.id descending

                                            where app.DeleteStatus != true && app.id == getApp.id && c.DeleteStatus != true

                                            select new MyApps

                                            {

                                                appID = app.id,

                                                Reference = app.reference,

                                                CategoryName = cat.name,

                                                PhaseName = phs.name,

                                                category_id = cat.id,

                                                FacilityId = fac.Id,

                                                PhaseId = phs.id,

                                                AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),

                                                Current_Permit = "",

                                                //Stage = s.StageName,

                                                Address_1 = ad.address_1,

                                                Status = app.status,

                                                Date_Added = Convert.ToDateTime(app.date_added),

                                                Submitted = app.submitted,

                                                CompanyDetails = c.name + " (" + c.Address + ") ",

                                                FacilityDetails = fac.Name,



                                            }).FirstOrDefault();



                            return View("ApplicationSuccess", MyApp);



                        }

                        else

                        {

                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("An error occured while creating process for this application.") });

                        }



                    }

                }

                #endregion

            }

            remita_transactions ptrans = _context.remita_transactions.Where(a => a.order_id == getApp.reference).FirstOrDefault();

            if (ptrans == null || string.IsNullOrEmpty(ptrans.RRR))

            {
                var address = (from ad in _context.addresses
                               join s in _context.States on ad.StateId equals s.Id
                               where ad.id == fac.AddressId
                               select new
                               {
                                   Address = ad.address_1,
                                   StateName = s.Name,
                                   LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                                   FullAddress = ad.address_1 + ", " + (ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city) + ", " + s.Name
                               }).FirstOrDefault();


                #region Remita Payment Split Builder

                var rmSplit = new RemitaSplit();

                rmSplit.payerPhone = company.contact_phone;

                var fp = getApp.fee_payable.ToString().Split('.');

                rmSplit.orderId = getApp.reference;

                rmSplit.CategoryName = category.name;

                rmSplit.payerEmail = userEmail;

                rmSplit.payerName = company.name;

                rmSplit.AmountDue = fp[0];

                rmSplit.ReturnBankPaymentUrl = _configuration.GetSection("AmountSetting").GetSection("ReturnBankPaymentUrl").Value.ToString();

                rmSplit.ReturnFailureUrl = _configuration.GetSection("AmountSetting").GetSection("ReturnFailureUrl").Value.ToString();

                rmSplit.ReturnSuccessUrl = _configuration.GetSection("AmountSetting").GetSection("ReturnSuccessUrl").Value.ToString();

                double serviceCharge = 0.00;

                if (category.FriendlyName == "TO")
                {
                    double TO = 0.05 * Convert.ToDouble(getApp.TransferCost);
                    serviceCharge = TO > 1000000.00 ? (0.05 * TO) : (0.05 * 1000000.00);
                }

                rmSplit.ServiceCharge = serviceCharge.ToString();

                rmSplit.totalAmount = (Convert.ToDouble(rmSplit.AmountDue) + Convert.ToDouble(rmSplit.ServiceCharge)).ToString();

                var apItem = new List<ApplicationItem>();

                apItem.Add(new ApplicationItem { Group = category.name, Name = phase.name + ": " + phase.Description });

                apItem.Add(new ApplicationItem { Group = category.name, Name = "Facility Name: " + fac.Name + "(" + address.FullAddress + ")" });

                apItem.Add(new ApplicationItem { Group = category.name, Name = "Payment Description: " + getApp.PaymentDescription });

                var fields = new List<CustomField>();

                fields.Add(new CustomField
                {
                    Name = "COMPANY BRANCH",
                    Value = company.name,
                    Type = "ALL"
                });
                fields.Add(new CustomField
                {
                    Name = "FACILITY ADDRESS",
                    Value = address.Address,
                    Type = "ALL"
                });
                fields.Add(new CustomField
                {
                    Name = "STATE",
                    Value = address.StateName,
                    Type = "ALL"
                });
                var appOffice = _helpersController.GetApplicationOffice(getApp.id).FirstOrDefault();

                fields.Add(new CustomField
                {
                    Name = "Field/Zonal Office",
                    Value = appOffice.OfficeName,
                    Type = "ALL"
                });

                rmSplit.ApplicationItems = apItem;
                rmSplit.CustomFields = fields;

                #endregion

                //getApp.fee_payable = decimal.Parse(rmSplit.totalAmount);
                getApp.service_charge = decimal.Parse(rmSplit.ServiceCharge);
                _context.SaveChanges();

                rmSplit.lineItems = _helpersController.BuildPartners(getApp, rmSplit, null, 0);

                var jn = JsonConvert.SerializeObject(rmSplit);
                var nw = JObject.Parse(jn);


                #region RRR Payment Generation

                _helpersController.LogMessages("Done generating payment for application with Ref : " + getApp.reference + " Posting to remita", userEmail);

                // demo rmSplit.serviceTypeId = "4430731";

                var paramDatas = _restService.parameterData("CompId", company.elps_id.ToString());

                var response = _restService.Response("api/Payments/{CompId}/{email}/{apiHash}", paramDatas, "POST", rmSplit);

                var resz = JsonConvert.DeserializeObject<JObject>(response.Content);

                var email = ElpsServices._elpsAppEmail;

                var apiHash = ElpsServices.appHash;
                string eMsg = "";

                #endregion
                if (resz == null)
                {
                    eMsg = "Sorry, an error occured while generating RRR for this application. Please try again.";
                    ViewBag.RError = eMsg;
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(eMsg) });
                }

                var resp = JsonConvert.DeserializeObject<PrePaymentResponse>(resz.ToString());


                if (resp == null && resp.Status == "025" && string.IsNullOrEmpty(resp.RRR))
                {

                    eMsg = "Sorry, an error occured while generating RRR for this application. Please try again.";
                    ViewBag.RError = eMsg;

                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(eMsg) });
                }

                else if (resp != null && string.IsNullOrEmpty(resp.RRR))
                {

                    eMsg = "Sorry, an error has occured while trying to generate RRR for this application, please contact support. Status Code: " + resp.Status + "; Status Mesage: " + resp.StatusMessage;

                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(eMsg) });

                }



                _helpersController.LogMessages("Done generating payment for application with Ref : " + getApp.reference + " Posting to remita", userEmail);

                ViewBag.rrr = resp.RRR.Trim();

                ViewBag.webPayData = rmSplit;



                if (category.name.ToLower().Contains("take over"))

                {

                    var RevenueItemId = _configuration.GetSection("RemitaSplit").GetSection("RevenueId").Value.ToString();

                    var revenueItems = new List<RevenueItem>

                     {

                     new RevenueItem { RevenueItemId = int.Parse( RevenueItemId), Amount = int.Parse(rmSplit?.IGRFee), Quantity = 1 }

                     };

                    var revItems = JsonConvert.SerializeObject(revenueItems);

                    var IGR_URL = _configuration.GetSection("RemitaSplit").GetSection("IGR_URL").Value.ToString();

                    var igr = _helpersController.PostReferenceToIGR(IGR_URL.ToString(), "/api/addpayments", new

                    {

                        RevenueItems = revItems,

                        RRR = resp.RRR.Trim(),

                        ExternalPaymentReference = getApp.reference,

                        State = state.StateName,

                        Address = add.address_1,

                        CompanyName = company.name,

                        Phone = fac.ContactNumber,

                        CompanyEmail = company.CompanyEmail

                    });





                    if (!string.IsNullOrEmpty(igr.ToString()) && igr.ToString().ToLower().Contains("success"))

                    {

                        getApp.PaymentDescription += "|IGR Fee: N" + rmSplit.IGRFee;

                        _context.SaveChanges(); //save new payment description
                    }

                    else

                    {
                        ViewBag.Message = "IGR Fee Error, Please contact support.";

                        ViewBag.OrderId = rmSplit.orderId;

                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ViewBag.Message) });

                    }

                }



                int tid = RecordRemitaTransaction(ptrans, Fee, getApp, resp, company.name);


                if (resp.RRR.ToLower() == "DPR-ELPS".ToLower() || resp.RRR.ToLower() == "DPR-Bank-M".ToLower())

                {

                    //0 Naira application Or Bank



                    var vapp = _context.applications.Where(a => a.id == getApp.id).FirstOrDefault();

                    //Assign application to a staff

                    int stateID = _helpersController.GetApplicationState(getApp.id);

                    #region appProcess

                    // getting application process for singles

                    List<WorkProccess> process = _helpersController.GetAppProcess(getApp.PhaseId, getApp.category_id, 0, 0);

                    if (process.Count <= 0)

                    {

                        string err = "Something went wrong while trying to get work process for your application. Please try again or contact Support.";

                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });



                    }

                    var fm = _context.FacilityModifications.Where(a => a.ApplicationId == getApp.id).FirstOrDefault();

                    string modType = null;

                    if (fm != null)

                    {

                        if (fm.Type.Contains("clusion") || fm.Type.Contains("version"))

                            modType = fm.Type;

                    }

                    int AppDropStaffID = _helpersController.ApplicationDropStaff(getApp.id, getApp.PhaseId, getApp.category_id, stateID, 0, modType);



                    if (AppDropStaffID <= 0)

                    {

                        string err = "Something went wrong while trying to send your application to a staff for processing. Please try again or contact Support.";

                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });



                    }

                    else

                    {



                        //add to old table

                        var staffUN = _context.Staff.Where(x => x.StaffID == AppDropStaffID).FirstOrDefault();



                        MyDesk drop = new MyDesk()

                        {

                            ProcessID = process.FirstOrDefault().ProccessID,

                            Sort = process.FirstOrDefault().Sort,

                            AppId = getApp.id,

                            StaffID = AppDropStaffID,

                            FromStaffID = 0,

                            HasWork = false,

                            HasPushed = false,

                            CreatedAt = DateTime.Now

                        };



                        // dropping application on staff desk

                        _context.MyDesk.Add(drop);

                        int appDrop = _context.SaveChanges();



                        if (appDrop > 0)

                        {



                            //Check if app is submitted or re-submit

                            if (getApp.submitted == true)

                            {

                                #region Resubmission

                                subject = ("Application Re-Submitted With Reference: ") + getApp.reference;

                                content = string.Format("Your Licence Application has been Re-submited on the Depot portal. The process for your application with Reference Number: {0} will continue immediately.<br />", getApp.reference);

                                TempData["AppSubmitType"] = "Resubmit";

                                #endregion

                            }

                            else

                            {

                                #region Fresh Submission

                                TempData["AppSubmitType"] = "New";

                                subject = ("Application Submitted With Reference: ") + getApp.reference;

                                content = string.Format("Thank you for submitting your application on the Depot portal. Your application with Reference Number: {0} will be reviewed and permit issued within seventy-two (72) hour.<br />The table below shows the breakdown of the funds received.", getApp.reference);

                                #endregion

                            }







                            getApp.CreatedAt = DateTime.Now;

                            getApp.UpdatedAt = DateTime.Now;

                            getApp.status = GeneralClass.Processing;

                            getApp.submitted = true;

                            getApp.current_desk = AppDropStaffID;



                            if (_context.SaveChanges() > 0)

                            {

                                _helpersController.SaveHistory(getApp.id, AppDropStaffID, "Moved", "Application submitted and landed on staff desk for distribution.");


                                // Send Mail to Company

                                var emailMsg = _helpersController.SaveMessage(getApp.id, Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID))), subject, content, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionElpsID)), "Company");

                                var sendEmail = _helpersController.SendEmailMessage(company.CompanyEmail.ToString(), company.name, emailMsg, null);


                                ViewBag.OrderId = getApp.reference;

                                //Send Mail to Staff

                                string type = category.id == 1 ? category.name + "(" + phase.name + ")" : category.name;

                                string subj = "<b>" + type + "</b>" + " Application (" + getApp.reference + ") pushed to your desk";

                                string cont = "A " + "<b>" + type + "</b>" + " application with reference number " + getApp.reference + " has been submitted on your desk for processing.";



                                var processor = _context.Staff.Where(a => a.StaffID == AppDropStaffID).FirstOrDefault();


                                var emailMsg2 = _helpersController.SaveMessage(getApp.id, processor.StaffID, subject, cont, processor.StaffElpsID, "Staff");

                                var sendEmail2 = _helpersController.SendEmailMessage2Staff(processor.StaffEmail, processor.FirstName, emailMsg2, null);


                                var getAppList = _context.applications.Where(a => a.id == id).ToList();

                                _helpersController.UpdateElpsApplication(getAppList);



                                _helpersController.LogMessages("Application submitted successfully with reference : " + getApp.reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                                MyApps MyApp = (from app in _context.applications.AsEnumerable()

                                                join c in _context.companies.AsEnumerable() on app.company_id equals c.id

                                                join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id

                                                join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id

                                                join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id

                                                join ad in _context.addresses on fac.AddressId equals ad.id

                                                //join sb in _context.SubmittedDocuments.AsEnumerable() on app.id equals sb.AppID

                                                // orderby app.id descending

                                                where app.DeleteStatus != true && app.id == getApp.id && c.DeleteStatus != true

                                                select new MyApps

                                                {

                                                    appID = app.id,

                                                    Reference = app.reference,

                                                    CategoryName = cat.name,

                                                    PhaseName = phs.name,

                                                    category_id = cat.id,

                                                    FacilityId = fac.Id,

                                                    PhaseId = phs.id,

                                                    AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),

                                                    Current_Permit = "",

                                                    //Stage = s.StageName,

                                                    Address_1 = ad.address_1,

                                                    Status = app.status,

                                                    Date_Added = Convert.ToDateTime(app.date_added),

                                                    Submitted = app.submitted,

                                                    CompanyDetails = c.name + " (" + c.Address + ") ",

                                                    FacilityDetails = fac.Name,



                                                }).FirstOrDefault();



                                return View("ApplicationSuccess", MyApp);



                            }

                            else

                            {

                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("An error occured while creating process for this application.") });

                            }



                        }

                    }

                    #endregion

                    stateID = _helpersController.GetApplicationState(getApp.id);

                    #region appProcess

                    // getting application process for single

                    process = _helpersController.GetAppProcess(getApp.PhaseId, getApp.category_id, 0, 0);

                    if (process.Count <= 0)

                    {

                        string err = "Something went wrong while trying to get work process for your application. Please try again or contact Support.";

                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });



                    }

                    fm = _context.FacilityModifications.Where(a => a.ApplicationId == getApp.id).FirstOrDefault();

                    modType = null;

                    if (fm != null)

                    {

                        if (fm.Type.Contains("clusion") || fm.Type.Contains("version"))

                            modType = fm.Type;

                    }

                    AppDropStaffID = _helpersController.ApplicationDropStaff(getApp.id, getApp.PhaseId, getApp.category_id, stateID, 0, modType);



                    if (AppDropStaffID <= 0)

                    {

                        string err = "Something went wrong while trying to send your application to a staff for processing. Please try again or contact Support.";

                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });



                    }

                    else

                    {



                        //add to old table

                        var staffUN = _context.Staff.Where(x => x.StaffID == AppDropStaffID).FirstOrDefault();



                        MyDesk drop = new MyDesk()

                        {

                            ProcessID = process.FirstOrDefault().ProccessID,

                            Sort = process.FirstOrDefault().Sort,

                            AppId = getApp.id,

                            StaffID = AppDropStaffID,

                            FromStaffID = 0,

                            HasWork = false,

                            HasPushed = false,

                            CreatedAt = DateTime.Now

                        };



                        // dropping application on staff desk

                        _context.MyDesk.Add(drop);

                        int appDrop = _context.SaveChanges();



                        if (appDrop > 0)

                        {



                            //Check if app is submitted or re-submit

                            if (getApp.submitted == true)

                            {

                                #region Resubmission

                                subject = ("Application Re-Submitted With Reference: ") + getApp.reference;

                                content = string.Format("Your Licence Application has been Re-submited on the Depot portal. The process for your application with Reference Number: {0} will continue immediately.<br />", getApp.reference);

                                TempData["AppSubmitType"] = "Resubmit";

                                #endregion

                            }

                            else

                            {

                                #region Fresh Submission

                                TempData["AppSubmitType"] = "New";

                                subject = ("Application Submitted With Reference: ") + getApp.reference;

                                content = string.Format("Thank you for submitting your application on the Depot portal. Your application with Reference Number: {0} will be reviewed and treated accordingly.<br />The table below shows the breakdown of the funds received.", getApp.reference);

                                #endregion

                            }







                            getApp.CreatedAt = DateTime.Now;

                            getApp.UpdatedAt = DateTime.Now;

                            getApp.status = GeneralClass.Processing;

                            getApp.submitted = true;

                            getApp.current_desk = AppDropStaffID;



                            if (_context.SaveChanges() > 0)

                            {

                                _helpersController.SaveHistory(getApp.id, AppDropStaffID, "Moved", "Application submitted and landed on staff desk for distribution.");





                                // Send Mail to Company



                                var emailMsg = _helpersController.SaveMessage(getApp.id, Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID))), subject, content, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionElpsID)), "Company");

                                var sendEmail = _helpersController.SendEmailMessage(company.CompanyEmail.ToString(), company.name, emailMsg, null);



                                ViewBag.OrderId = getApp.reference;

                                //Send Mail to Staff

                                string type = category.id == 1 ? category.name + "(" + phase.name + ")" : category.name;

                                string subj = "<b>" + type + "</b>" + " Application (" + getApp.reference + ") submitted on your desk";

                                string cont = "A " + "<b>" + type + "</b>" + " application with reference number " + getApp.reference + " has been submitted on your desk for processing.";



                                var processor = _context.Staff.Where(a => a.StaffID == AppDropStaffID).FirstOrDefault();



                                var emailMsg2 = _helpersController.SaveMessage(getApp.id, processor.StaffID, subject, cont, processor.StaffElpsID, "Staff");

                                var sendEmail2 = _helpersController.SendEmailMessage2Staff(processor.StaffEmail, processor.FirstName, emailMsg2, null);



                                var getAppList = _context.applications.Where(a => a.id == id).ToList();

                                _helpersController.UpdateElpsApplication(getAppList);



                                _helpersController.LogMessages("Application submitted successfully with reference : " + getApp.reference, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                                MyApps MyApp = (from app in _context.applications.AsEnumerable()

                                                join c in _context.companies.AsEnumerable() on app.company_id equals c.id

                                                join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id

                                                join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id

                                                join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id

                                                join ad in _context.addresses on fac.AddressId equals ad.id

                                                where app.DeleteStatus != true && app.id == getApp.id && c.DeleteStatus != true

                                                select new MyApps

                                                {

                                                    appID = app.id,

                                                    Reference = app.reference,

                                                    CategoryName = cat.name,

                                                    PhaseName = phs.name,

                                                    category_id = cat.id,

                                                    FacilityId = fac.Id,

                                                    PhaseId = phs.id,

                                                    AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),

                                                    Current_Permit = "",

                                                    //Stage = s.StageName,

                                                    Address_1 = ad.address_1,

                                                    Status = app.status,

                                                    Date_Added = Convert.ToDateTime(app.date_added),

                                                    Submitted = app.submitted,

                                                    CompanyDetails = c.name + " (" + c.Address + ") ",

                                                    FacilityDetails = fac.Name,



                                                }).FirstOrDefault();



                                return View("ApplicationSuccess", MyApp);



                            }

                            else

                            {

                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("An error occured while creating process for this application.") });

                            }



                        }

                    }

                    #endregion

                }

                else

                {

                    var returnUrl = ElpsServices._elpsBaseUrl + "Payment/Pay?rrr=" + resp.RRR.Trim();
                    return Redirect(returnUrl);

                }



            }

            var returnUr = ElpsServices._elpsBaseUrl + "Payment/Pay?rrr=" + ptrans.RRR.Trim();

            return Redirect(returnUr);

        }


        [Authorize(Policy = ("AdminRoles"))]
        public IActionResult GiveTempValue(string id)
        {
            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
                if (userRole == "Error")
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

                }
                var vapp = _context.applications.Where(a => a.reference == id).FirstOrDefault();
                if (vapp != null)
                {
                    var getApp = _context.applications.Where(a => a.id == vapp.id).FirstOrDefault();
                    var getCompany = _context.companies.Where(a => a.id == getApp.company_id).FirstOrDefault();
                    SendApplicationSubmittedMail(vapp, "");

                    if (CreateProcessingRules(vapp.id))
                    {
                        var resp = new PrePaymentResponse { RRR = $"TempRRR-{getApp.id}", StatusMessage = "Sucesss", Status = "01" };
                        RecordRemitaTransaction(null, Convert.ToDouble(getApp.fee_payable), getApp, resp, getCompany.name);
                        ViewBag.OrderId = id;

                        MyApps MyApp = (from app in _context.applications.AsEnumerable()
                                        join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                        join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id
                                        join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                        join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                        join ad in _context.addresses on facil.AddressId equals ad.id
                                        where app.DeleteStatus != true && app.id == getApp.id && c.DeleteStatus != true
                                        select new MyApps
                                        {
                                            appID = app.id,
                                            Reference = app.reference,
                                            CategoryName = cat.name,
                                            PhaseName = phs.name,
                                            category_id = cat.id,
                                            FacilityId = facil.Id,
                                            PhaseId = phs.id,
                                            AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),
                                            Current_Permit = "",
                                            //Stage = s.StageName,
                                            Address_1 = ad.address_1,
                                            Status = app.status,
                                            Date_Added = Convert.ToDateTime(app.date_added),
                                            Submitted = app.submitted,
                                            CompanyDetails = c.name + " (" + c.Address + ") ",
                                            FacilityDetails = facil.Name,

                                        }).FirstOrDefault();

                        return View("ApplicationSuccess", MyApp);
                    }

                    ViewBag.error = "Errror Creating Processing Rule";
                    string erro = "Error occured while processing this request, please contact NMDPRA support";
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(erro) });
                }
                ViewBag.errormessage = "Error Occured while processing the Request";
                string err = "Error occured while processing this request, please contact NMDPRA support";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });
            }
            catch (Exception x)
            {

                return Content(x.ToString());
            }
        }

        [Authorize(Policy = ("AdminRoles"))]
        public IActionResult RemitaTransactionDetail(string Id)
        {
            int rId = Convert.ToInt16(Id);
            var trans = _context.remita_transactions.Where(C => C.id == rId).FirstOrDefault();
            string txnref = "";
            if (trans != null)
            {
                txnref = trans.reference_number;
                var app = _context.applications.Where(a => a.reference == txnref).FirstOrDefault();

                if (app != null)
                {
                    var getCompany = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();

                    // Application existing and Value not given yet. Check if payment has been made for possible value
                    #region call Back api [Verify payment from REMITA]

                    //confirm API
                    var responsee = _restService.Response("/Payment/checkifpaid?id=r" + trans.RRR, null, "GET", null);
                    var resp = JsonConvert.DeserializeObject<JObject>(responsee.Content);

                    if ((resp.GetValue("message").ToString().ToLower() == "approved" && resp.GetValue("status").ToString() == "01") || (resp.GetValue("message").ToString().ToLower() == "successful" && resp.GetValue("status").ToString() == "00"))
                    {
                        var vapp = _context.applications.Where(a => a.id == app.id).FirstOrDefault();

                        #region Payment Valid and Approved, Give value
                        GiveRemitaValue(app, trans.id, trans.RRR, getCompany.name, DateTime.Now);

                        //send message to applicant
                        var category = _context.Phases.Where(p => p.id == vapp.PhaseId).FirstOrDefault();

                        string subject = category.name + " Application Payment made with Ref : " + vapp.reference;
                        string content = "You have made payment for your application (" + category.name + ") with reference Number " + vapp.reference + " for processing on NMDPRA Depot portal. Kindly find application details below.";
                        //var emailMsg = _helpersController.SaveMessage(vapp.id, getCompany.id, subject, content, getCompany.elps_id.ToString(), "Company");
                        //var sendEmail = _helpersController.SendEmailMessage(getCompany.CompanyEmail, getCompany.name, emailMsg, null);

                        //Update Remita_Transaction table
                        trans.transaction_date = resp.GetValue("transactiontime").ToString();
                        trans.query_date = DateTime.Now;
                        trans.response_code = resp.GetValue("status").ToString();
                        trans.response_description = resp.GetValue("message").ToString();
                        trans.type = "Bank";
                        _context.SaveChanges();

                        ViewBag.Alert = new AlertBox()
                        {
                            ButtonType = AlertType.Success,
                            Message = "Payment verified sucessfully and value has been given to application with reference number: " + app.reference + ". Applicant can login to their dashboard and submit the application.",
                            Title = "Payment Verified!"
                        };


                        #endregion
                    }
                    else
                    {
                        ViewBag.Alert = new AlertBox()
                        {
                            ButtonType = AlertType.Failure,
                            Message = "Payment cannot be verified from Remita. Please confirm that payment had been made with the RRR on the application.",
                            Title = "Payment Not Verified!"
                        };
                    }


                    #endregion
                    if ((app != null && app.status.Trim().ToLower() == "payment pending")
                    || (app != null && app.status.Trim().ToLower() == "payment completed" && trans.response_code != "01"))
                    {
                        // return Content("");
                    }

                    else
                    {
                        //https://elps.dpr.gov.ng/payment/checkifpaid?id=r330247144129
                        //check Extra payment
                        var extrapayment = _context.ApplicationExtraPayments.Where(a => a.Reference == txnref || a.RRR == txnref).FirstOrDefault();
                        if (extrapayment != null)
                        {
                            var getCompany2 = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();
                            var paramDatas2 = _restService.parameterData("id", trans.RRR);
                            //confirm method
                            var response2 = _restService.Response("/Payment/checkifpaid?id=r" + trans.RRR, null, "GET", null);
                            var resp2 = JsonConvert.DeserializeObject<JObject>(response2.Content);

                            if ((resp.GetValue("message").ToString().ToLower() == "approved" && resp.GetValue("status").ToString() == "01") || (resp.GetValue("message").ToString().ToLower() == "successful" && resp.GetValue("status").ToString() == "00"))
                            {
                                var vapp = _context.applications.Where(a => a.reference == trans.order_id).FirstOrDefault();

                                #region Payment Valid and Approved, Give value

                                var category = _context.Phases.Where(p => p.id == vapp.PhaseId).FirstOrDefault();
                                var staff = vapp.current_desk != null ? _context.Staff.Where(x => x.StaffID == vapp.current_desk).FirstOrDefault() : null;

                                if (staff != null)
                                {
                                    _helpersController.SaveHistory(vapp.id, staff.StaffID, "Payment", "Extra payment for application paid successfully with Reference RRR : " + trans.RRR);
                                }
                                // Saving Messages
                                string subject = "Application Extra Payment Success : " + vapp.reference;
                                string content = "Extra payment made successfully for your application (" + category.name + ") with reference number " + vapp.reference + " and payment RRR " + trans.RRR + " for processing on NMDPRA DEPOT portal. Kindly find application details below.";
                                var emailMsg = _helpersController.SaveMessage(vapp.id, (int)getCompany.id, subject, content, getCompany.elps_id.ToString(), "Company");
                                var sendEmail = _helpersController.SendEmailMessage(getCompany.CompanyEmail, getCompany.name, emailMsg, null);

                                _helpersController.LogMessages("Extra payment for application paid successfully with reference RRR : " + trans.RRR, getCompany.CompanyEmail);

                                //Update Remita_Transaction table
                                trans.transaction_date = resp2.GetValue("transactiontime").ToString();
                                trans.query_date = DateTime.Now;
                                trans.response_code = resp2.GetValue("status").ToString();
                                trans.response_description = resp2.GetValue("message").ToString();
                                trans.type = "Bank";
                                _context.remita_transactions.Add(trans);
                                _context.SaveChanges();
                                extrapayment.Status = "Paid";

                                _context.SaveChanges();

                                ViewBag.Alert = new AlertBox()
                                {
                                    ButtonType = AlertType.Success,
                                    Message = "Payment verified sucessfully and value has been given to application with reference number: " + app.reference + ". Applicant can login to their dashboard and submit the application.",
                                    Title = "Payment Verified!"
                                };


                                #endregion
                            }
                            else
                            {
                                ViewBag.Alert = new AlertBox()
                                {
                                    ButtonType = AlertType.Failure,
                                    Message = "Payment cannot be verified from Remita. Please confirm that payment had been made with the RRR on the application",
                                    Title = "Payment Not Verified!"
                                };
                            }


                        }
                    }
                    return View(trans);
                }

                TempData["Alert"] = new AlertBox()
                {
                    ButtonType = AlertType.Warning,
                    Message = "Application details not found or does not exist",
                    Title = "Application details not found."
                };

                return RedirectToAction("RemitaList", "Payment");
            }
            TempData["Alert"] = new AlertBox()
            {
                ButtonType = AlertType.Warning,
                Message = "Application Transaction not found or does not exist",
                Title = "Transaction details not found."
            };

            return RedirectToAction("RemitaList", "Payment");
        }


        private int RecordRemitaTransaction(remita_transactions ptrans, double Fee, applications app, PrePaymentResponse resp, string CompanyName, string pType = "Online")
        {
            try
            {
                if (ptrans == null)
                {
                    ptrans = new remita_transactions();
                    ptrans.approved_amount = Fee.ToString();
                    ptrans.customer_id = app.company_id;
                    ptrans.customer_name = CompanyName;
                    ptrans.online_reference = resp.RRR;
                    ptrans.order_id = app.reference;
                    ptrans.payment_reference = resp.RRR;
                    ptrans.PaymentSource = "Remita";
                    ptrans.reference_number = app.reference;
                    ptrans.response_code = resp.Status;
                    ptrans.response_description = resp.StatusMessage;
                    ptrans.RRR = resp.RRR;
                    ptrans.transaction_amount = Fee.ToString();
                    ptrans.transaction_currency = "566";
                    ptrans.transaction_date = string.IsNullOrEmpty(resp.Transactiontime) ? DateTime.Now.ToString() : resp.Transactiontime;
                    ptrans.type = pType;

                    _context.remita_transactions.Add(ptrans);
                }
                else
                {

                    string r = ptrans.RRR;
                    ptrans.approved_amount = Fee.ToString();
                    ptrans.customer_id = app.company_id;
                    ptrans.customer_name = CompanyName;
                    ptrans.online_reference = resp.RRR;
                    ptrans.order_id = app.reference;
                    ptrans.payment_reference = resp.RRR;
                    ptrans.PaymentSource = "Remita";
                    ptrans.reference_number = app.reference;
                    ptrans.response_code = resp.Status;
                    ptrans.response_description = resp.StatusMessage + r != resp.RRR ? $" :: Previous RRR-({r})" : "";
                    ptrans.RRR = resp.RRR;
                    ptrans.transaction_amount = Fee.ToString();
                    ptrans.transaction_currency = "566";
                    ptrans.transaction_date = string.IsNullOrEmpty(resp.Transactiontime) ? DateTime.Now.ToString() : resp.Transactiontime;
                    ptrans.type = pType;
                    _context.remita_transactions.Add(ptrans);
                }
                _context.SaveChanges();

            }
            catch (Exception ex)
            {

            }

            return ptrans.id;
        }

        private List<int> GetReqDocs(int catId, int phaseId)
        {
            var docs = new List<int>();
            var cDocs = _context.document_type_categories.Where(a => a.category_id == catId).ToList();
            foreach (var item in cDocs)
            {
                docs.Add(item.document_type_id);

            }

            var pDocs = _context.ApplicationDocuments.Where(a => a.PhaseId == phaseId).ToList();
            foreach (var item in pDocs)
            {
                docs.Add(item.ElpsDocTypeID);
            }
            return docs;
        }

        /// <summary>
        /// Callback URL for Remita after any transaction
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Remita(string orderId)
        {
            //record all the transaction
            var getApp = _context.applications.Where(a => a.reference == orderId).FirstOrDefault();
            var vapp = _context.applications.Where(a => a.id == getApp.id).FirstOrDefault();
            remita_transactions ptrans = _context.remita_transactions.Where(C => C.order_id == getApp.reference).FirstOrDefault();
            var amountDue = getApp.fee_payable + getApp.service_charge;
            var comp = _context.companies.Where(a => a.id == getApp.company_id).FirstOrDefault();
            #region record transaction

            var resp = CheckRRRPayment(ptrans.RRR);

            if ((resp.GetValue("message").ToString().ToLower() == "approved" && resp.GetValue("status").ToString() == "01") || (resp.GetValue("message").ToString().ToLower() == "successful" && resp.GetValue("status").ToString() == "00"))
            {
                var RRR = ptrans.RRR;

                var trans = _context.remita_transactions.Where(C => C.RRR == RRR && C.reference_number == orderId).FirstOrDefault();
                RemitaResponse rems = new RemitaResponse() { orderId = orderId, RRR = RRR, status = resp.GetValue("status").ToString(), statusmessage = resp.GetValue("message").ToString(), transactiontime = resp.GetValue("transactiontime").ToString() };

                #region Payment Valid and Approved, Give value

                #region Update PaymentTransaction
                ptrans.online_reference = orderId;
                ptrans.order_id = vapp.reference;       //Needs value
                ptrans.payment_reference = resp.GetValue("rrr").ToString();
                ptrans.PaymentSource = "Remita";
                ptrans.reference_number = vapp.reference;
                ptrans.response_code = resp.GetValue("status").ToString();
                ptrans.response_description = resp.GetValue("message").ToString();
                ptrans.RRR = resp.GetValue("rrr").ToString();
                ptrans.transaction_date = resp.GetValue("transactiontime").ToString();
                _context.remita_transactions.Add(ptrans);
                _context.SaveChanges();
                #endregion

                GiveRemitaValue(getApp, trans.id, RRR, comp.name, DateTime.Now);


                if (vapp.submitted != true)
                {
                    SendApplicationSubmittedMail(vapp, getApp.current_Permit);
                    //Move to after Payment
                    if (CreateProcessingRules(getApp.id))
                    {
                        ViewBag.Message = resp.GetValue("message").ToString();
                        ViewBag.RRR = resp.GetValue("rrr").ToString();
                        ViewBag.OrderId = resp.GetValue("orderId").ToString();
                        ViewBag.AppId = getApp.id;

                    }

                }
                return RedirectToAction("PaymentSuccess", new { appId = getApp.id, rrr = resp.GetValue("rrr").ToString(), msg = resp.GetValue("message").ToString(), orderId = resp.GetValue("orderId").ToString() });

                #endregion
            }
            return RedirectToAction("RemitaFailure", new { status = resp.GetValue("status").ToString(), rrr = resp.GetValue("rrr").ToString(), orderId = resp.GetValue("orderId").ToString() });


            #endregion
        }

        private void SendApplicationSubmittedMail(applications vapp, string Current_Permit)
        {
            #region Send Application Submitted Mail
            string subject = string.Empty; string content = "";
            string tk = string.Empty;
            string tbl = string.Empty;
            if (vapp.submitted == true)
            {
                #region Resubmission
                subject = "Application Re-Submitted - Ref. No.: " + vapp.reference;
                tk = string.Format("Your Licence Application has been Re-submited on the Depot portal. The process for your application with Reference Number: {0} will continue immediately.<br />", vapp.reference);
                content = string.Format("Your Licence Application has been Re-submited on the Depot portal. The process for your application with Reference Number: {0} will continue immediately.<br />", vapp.reference);
                TempData["AppSubmitType"] = "Resubmit";
                #endregion
            }
            else
            {
                #region Fresh Submission
                TempData["AppSubmitType"] = "New";
                subject = ("Application Submitted - Ref. No.: ") + vapp.reference;
                tk = string.Format("Thank you for submitting your application on the Depot portal. Your application with Reference Number: {0} will be reviewed accordingly.<br />The table below shows the breakdown of the funds received.", vapp.reference);
                content = string.Format("Thank you for submitting your application on the Depot portal. Your application with Reference Number: {0} will be reviewed accordingly.<br />The table below shows the breakdown of the funds received.", vapp.reference);
                #endregion
            }

            var facility = _context.Facilities.Where(a => a.Id == vapp.FacilityId).FirstOrDefault();
            var msg = new messages()
            {
                company_id = vapp.company_id,
                content = "Loading...",
                date = DateTime.Now,
                read = 0,
                subject = subject,
                sender_id = "Application"
            };

            _context.messages.Add(msg);

            var sn = msg.id;
            var body = "";
            var getCompany = _context.companies.Where(x => x.id == vapp.company_id).FirstOrDefault();

            var emailMsg = _helpersController.SaveMessage(vapp.id, Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID))), subject, content, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionElpsID)), "Company");
            var sendEmail = _helpersController.SendEmailMessage(getCompany.CompanyEmail.ToString(), getCompany.name, emailMsg, null);


            var msgbody = string.Format(body, subject, emailMsg, sn);
            var mm = _context.messages.Where(m => m.id == msg.id).FirstOrDefault();
            mm.content = msgbody;
            _context.SaveChanges();


            #endregion
        }

        public IActionResult PaymentSuccess(string appId, string rrr, string msg, string orderId)
        {
            ViewBag.Message = msg;
            ViewBag.RRR = rrr;
            ViewBag.OrderId = orderId;
            ViewBag.AppId = appId;

            var id = Convert.ToInt16(appId);
            var vApp = _context.applications.Where(a => a.id == id).FirstOrDefault();
            var ap = (from app in _context.applications.AsEnumerable()
                      join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                      join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id
                      join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                      join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                      join ad in _context.addresses on facil.AddressId equals ad.id
                      join sb in _context.SubmittedDocuments.AsEnumerable() on app.id equals sb.AppID
                      where app.DeleteStatus != true && app.id == id && c.DeleteStatus != true
                      select new MyApps
                      {
                          appID = app.id,
                          Reference = app.reference,
                          CategoryName = cat.name,
                          PhaseName = phs.name,
                          Address_1 = ad.address_1,
                          StateName = _context.States_UT.Where(l => l.State_id == ad.StateId).FirstOrDefault().StateName,
                          LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                          Status = app.status,
                          FacilityDetails = facil.Name,
                          Type = app.type.ToUpper()
                      }).FirstOrDefault();

            ViewBag.vApp = ap;

            return View("PaymentSuccess");
        }

        public IActionResult RemitaFailure(string status, string rrr, string orderid)
        {
            ViewBag.Message = status;
            ViewBag.RRR = rrr;
            ViewBag.OrderId = orderid;
            return RedirectToAction("PaymentFail", "Application", new { id = orderid });

        }

        private void GiveRemitaValue(applications getApp, int tid, string RRR, string companyName, DateTime transactionDate)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));



            double amountDue = Convert.ToDouble(getApp.service_charge + getApp.fee_payable);

            var invo = _context.invoices.Where(a => a.application_id == getApp.id).FirstOrDefault();


            if (invo == null)
            {
                invo = new invoices();
                invo.payment_type = "Remita";
                invo.receipt_no = _helpersController.GenerateReceiptNo(amountDue, getApp.id); //receiptNo; 
                invo.status = "Paid";
                invo.amount = amountDue;
                invo.application_id = getApp.id;
                invo.PaymentTransaction_Id = tid;
                invo.payment_code = RRR;
                invo.date_paid = DateTime.Now;
                invo.date_added = transactionDate;
                _context.invoices.Add(invo);
            }
            else
            {
                invo.payment_type = "Remita";
                if (string.IsNullOrEmpty(invo.receipt_no))
                    invo.receipt_no = _helpersController.GenerateReceiptNo(amountDue, getApp.id); //receiptNo; 
                invo.status = "Paid";
                invo.amount = amountDue;
                invo.PaymentTransaction_Id = tid;
                invo.payment_code = RRR;
                invo.date_paid = DateTime.Now;
            }
            if (_context.SaveChanges() > 0)
            {
                getApp.payment_id = tid;

                if (getApp.status == GeneralClass.PaymentPending)
                    getApp.status = GeneralClass.PaymentCompleted;

                getApp.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                _helpersController.LogMessages("Giving of manual remita value to application with ref: " + getApp.reference, userEmail);

            }

        }

        #region Mistdo



        /*
         * Viewing company mistdo staff
         * 
         * id => encrypted company id
         */
        public IActionResult ViewMistdo(string id)
        {
            var _id = generalClass.DecryptIDs(id.Trim());

            if (_id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, No link was found for this application history.") });
            }
            else
            {
                var getMistdo = from m in _context.MistdoStaff.AsEnumerable()
                                join c in _context.companies.AsEnumerable() on m.CompanyId equals c.id
                                where m.CompanyId == _id && m.DeletedStatus == false
                                select new MistdoStaffs
                                {
                                    CompanyName = c.name,
                                    FullName = m.FullName,
                                    PhoneNo = m.PhoneNo,
                                    CertificateNo = m.CertificateNo,
                                    Email = m.Email,
                                    IssuedDate = m.IssuedDate.Value.Date,
                                    ExpiryDate = m.ExpiryDate.Value.Date,
                                    CreatedAt = (DateTime)m.CreatedAt,
                                    MistdoServiceId = m.MistdoServerId
                                };

                ViewData["CompanyName"] = getMistdo.FirstOrDefault().CompanyName;

                return View(getMistdo.ToList());

            }
        }

        #endregion
        #region extra payment for calibration

        [HttpPost]
        public IActionResult ExtraPayment(string returnUrl, ApplicationExtraPayments model)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var getApp = _context.applications.Where(a => a.id == model.ApplicationId).FirstOrDefault();
            var getFac = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
            var FacAdd = _context.addresses.Where(a => a.id == getFac.AddressId).FirstOrDefault();
            var getCat = _context.Categories.Where(a => a.id == getApp.category_id).FirstOrDefault();
            var ps = _context.Phases.Where(a => a.id == getApp.PhaseId).FirstOrDefault();
            if (getApp == null)
            {
                TempData["Report"] = "The Application ID supplied is no longer available or has been removed.";
                return Redirect(returnUrl);
            }

            try
            {
                var coy = _context.companies.Where(a => a.id == getApp.company_id).FirstOrDefault();
                if (coy == null)
                {
                    TempData["Report"] = "Company details not found. Please check and try again.";
                    return Redirect(returnUrl);
                }
                ViewBag.Company = coy;

                #region Create ExtraPay & Invoice
                var refno = _helpersController.ReferenceCode();
                refno = model.Type.Substring(0, 1) + refno.Substring(0, refno.Length - 1);
                var exPay = new ApplicationExtraPayments()
                {
                    Amount = Math.Round(model.Amount, 2),
                    ApplicationId = getApp.id,
                    Reference = refno.ToLower(),
                    Type = model.Type,
                    UserName = userEmail,
                    Date = DateTime.Now,
                    Status = GeneralClass.PaymentPending,

                    Comment = model.Comment
                };
                _context.ApplicationExtraPayments.Add(exPay);
                _context.SaveChanges();
                var invo = new invoices();
                invo.amount = Convert.ToDouble(model.Amount);
                invo.application_id = -1;
                invo.payment_code = refno;
                invo.payment_type = "Remita";
                invo.status = "Unpaid";
                invo.date_added = DateTime.Now;
                invo.date_paid = DateTime.Now.AddDays(-7);

                _context.invoices.Add(invo);
                _context.SaveChanges();
                #endregion

                string output = "";


                #region Remita Payment Split Builder
                var amt = exPay.Amount.ToString().Split('.');
                var amnt = amt.Count() > 1 ? exPay.Amount.ToString() : $"{amt[0]}.00";
                var rmSplit = new RemitaSplit();
                rmSplit.payerPhone = coy.contact_phone;
                rmSplit.orderId = exPay.Reference;
                rmSplit.CategoryName = getCat.name;
                rmSplit.payerEmail = coy.CompanyEmail;
                rmSplit.payerName = coy.name;
                rmSplit.AmountDue = amnt;// exPay.Amount.ToString(); ;
                rmSplit.ServiceCharge = getApp.service_charge.ToString();// 0.ToString();
                rmSplit.totalAmount = exPay.Amount.ToString();  //amnt;// ;

                JArray lineItems = new JArray();
                JObject lineItem1 = new JObject();
                lineItem1.Add("lineItemsId", "1");
                lineItem1.Add("beneficiaryName", _configuration.GetSection("RemitaSplit").GetSection("BeneficiaryName").Value.ToString());
                lineItem1.Add("beneficiaryAccount", _configuration.GetSection("RemitaSplit").GetSection("BeneficiaryAccount").Value.ToString());
                lineItem1.Add("bankCode", _configuration.GetSection("RemitaSplit").GetSection("BankCode").Value.ToString());
                lineItem1.Add("beneficiaryAmount", (getApp.fee_payable + getApp.service_charge).ToString());
                lineItem1.Add("deductFeeFrom", "1");

                lineItems.Add(lineItem1);

                //rmSplit.lineItems = lineItems;

                var address = (from ad in _context.addresses
                               join s in _context.States on ad.StateId equals s.Id
                               where ad.id == getFac.AddressId
                               select new
                               {
                                   Address = ad.address_1,
                                   StateName = s.Name,
                                   LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                                   FullAddress = ad.address_1 + ", " + (ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city) + ", " + s.Name
                               }).FirstOrDefault();
                var fields = new List<CustomField>();

                fields.Add(new CustomField
                {
                    Name = "COMPANY BRANCH",
                    Value = coy.name,
                    Type = "ALL"
                });
                fields.Add(new CustomField
                {
                    Name = "FACILITY ADDRESS",
                    Value = address.Address,
                    Type = "ALL"
                });
                fields.Add(new CustomField
                {
                    Name = "STATE",
                    Value = address.StateName,
                    Type = "ALL"
                });
                var appOffice = _helpersController.GetApplicationOffice(getApp.id).FirstOrDefault();

                fields.Add(new CustomField
                {
                    Name = "Field/Zonal Office",
                    Value = appOffice.OfficeName,
                    Type = "ALL"
                });
                var apItem = new List<ApplicationItem>();

                apItem.Add(new ApplicationItem { Group = getCat.name, Name = ps.name + ": " + ps.Description });
                apItem.Add(new ApplicationItem { Group = getCat.name, Name = "Facility Name: " + getFac.Name + "(" + address.FullAddress + ")" });
                rmSplit.ApplicationItems = apItem;
                rmSplit.CustomFields = fields;
                var jn = JsonConvert.SerializeObject(rmSplit);

                #region remita split

                _helpersController.LogMessages("Done generating extra payment for DEPOT application : " + getApp.reference + " Posting to remita", userEmail);

                var paramDatas = _restService.parameterData("CompId", coy.elps_id.ToString());

                #endregion
                try
                {
                    var response = _restService.Response("/api/Payments/{CompId}/{email}/{apiHash}", paramDatas, "POST", jn);

                    output = (string)JsonConvert.DeserializeObject<JObject>(response.Content);

                }
                catch (Exception x)
                {
                    _helpersController.LogMessages($"Extra payment error from Remita:: {x.ToString()}");
                    TempData["Report"] = "Error occured while generating RRR. Please try again.";
                    return Redirect(returnUrl);

                }

                output = output.Replace("jsonp(", "");
                output = output.Replace(")", "");
                _helpersController.LogMessages($"Extra Pay Split :: {output}");
                #endregion

                var resp = JsonConvert.DeserializeObject<PrePaymentResponse>(output);

                if (resp == null || string.IsNullOrEmpty(resp.RRR))
                {
                    _helpersController.LogMessages($"Output from Remita:: {output}");
                    TempData["Report"] = "Error occured while generating RRR. Please try again.";
                    return Redirect(returnUrl);
                }

                ViewBag.rrr = resp.RRR.Trim();
                ViewBag.webPayData = rmSplit;


                #region
                //Create Payment Transaction
                var ptrans = new remita_transactions();
                ptrans.approved_amount = exPay.Amount.ToString();
                ptrans.customer_id = getApp.company_id;
                ptrans.customer_name = coy.name;
                ptrans.online_reference = resp.RRR;
                ptrans.order_id = exPay.Reference;
                ptrans.payment_reference = resp.RRR;
                ptrans.PaymentSource = "Remita";
                ptrans.reference_number = exPay.Reference;
                ptrans.response_code = resp.Status;
                ptrans.response_description = resp.StatusMessage;
                ptrans.RRR = resp.RRR;
                ptrans.transaction_amount = exPay.Amount.ToString();
                ptrans.transaction_currency = "566";
                ptrans.transaction_date = string.IsNullOrEmpty(resp.Transactiontime) ? DateTime.Now.ToString() : resp.Transactiontime;
                ptrans.type = exPay.Type;

                exPay.RRR = resp.RRR;
                _context.SaveChanges();
                #endregion
                TempData["Report"] = "New RRR (" + ptrans.RRR + ") generated successfully for Company: " + userName + "; Facility: " + getFac.Name;
                ViewBag.type = "pass";


                #region Send Application Initiation Mail
                string subject = $"Extra Payment Generated for {model.Type}";

                var apDetails = "";
                var tk = string.Format($"An Extra Payment RRR: {ptrans.RRR} has been generated for your application with reference number: {getApp.reference} (" + getCat.name + " (" + ps.name + ")" + "): " +
                    "<br /><ul><li>Amount Generated: {0}</li><li>Payment Type: {1}</li><li>Remita RRR: {2}</li>" +
                    "<li>Payment Status: {3}</li><li>Payment Comment: {4}</li>" +
                    "<li>Facility Name: {5}</li><li>Facility Address: {6}</li> <br/>" +
                    "<p>Please Note that your application will be pending untill this payment is confirmed</p>",
                    exPay.Amount.ToString("N2"), exPay.Type, ptrans.RRR, exPay.Status, exPay.Comment, getFac.Name, FacAdd.address_1);


                apDetails = tk;
                var msgBody = string.Format(subject, apDetails);


                var emailMsg = _helpersController.SaveMessage(getApp.id, getApp.company_id, subject, msgBody, coy.elps_id.ToString(), "Company");
                var sendEmail = _helpersController.SendEmailMessage(coy.CompanyEmail.ToString(), coy.name, emailMsg, null);


            }
            catch (Exception ex)
            {
                _helpersController.LogMessages(ex.ToString());
                TempData["Report"] = "An error occured while generating extra payment RRR for this application";
                ViewBag.type = "warn";
            }


            return Redirect(returnUrl);

        }





        #endregion
        #region my Applications

        public IActionResult MyPermits()
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();
            if (comp != null)
            {

                //var pms = _context.permits.Where(a => a.company_id == comp.id ).OrderByDescending(a => a.date_issued).ToList();
                var perModel = (from p in _context.permits
                                join app in _context.applications on p.application_id equals app.id
                                join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                where app.DeleteStatus != true && p.company_id == comp.id
                                select new permitsModel
                                {
                                    Id = p.id,
                                    FacilityName = fac.Name,
                                    CheckApprovalType = (phs.ShortName == "NDT" || phs.ShortName == "LTO" || phs.ShortName == "LR" || phs.ShortName == "TO" || phs.ShortName == "RC") ? "Yes" : "",
                                    Is_Renewed = p.is_renewed,
                                    Date_Issued = p.date_issued,
                                    Date_Expire = p.date_expire,
                                    Category_id = cat.id,
                                    Permit_No = p.permit_no,
                                    CategoryName = cat.name,
                                    PhaseName = phs.name,
                                    Application_Id = app.id,
                                    Reference = app.reference,
                                    ModifyType = _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault() != null ? _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault().Type : "",
                                    OrderId = app.reference.ToString()
                                }).OrderBy(a => a.Date_Issued).ToList();

                return View(perModel);
            }

            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, company could not be found. Please try again") });
        }

        public IActionResult ResubmitApplication(string id)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int AppId = generalClass.DecryptIDs(id);
            if (userEmail == "Error" || AppId == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, session expired or application ID is not passed correctly. Please try again") });

            }
            var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();
            var getApp = _context.applications.Where(a => a.id == AppId).FirstOrDefault();
            var fac = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
            //get the phase that the Application belongs to and also find its category.

            var rqDocs = appHelper.GetApplicationFiles(getApp, comp.elps_id.GetValueOrDefault(), fac.Elps_Id.GetValueOrDefault());// getApplicationFiles(ap, comp.elps_id.GetValueOrDefault());

            var docs = appHelper.GetApplicationFiles(getApp, comp.elps_id.GetValueOrDefault(), fac.Elps_Id.GetValueOrDefault());
            //var docs = appHelper.GetAppFiles(getApp, comp.elps_id.GetValueOrDefault(), fac.Elps_Id.GetValueOrDefault());
            var selecteddocs = rqDocs.Where(a => a.Selected).ToList();
            if (docs.Count > 0 && !selecteddocs.Any(x => x.Type.Equals("facility", StringComparison.OrdinalIgnoreCase)))
                rqDocs.AddRange(docs);

            ViewBag.companyId = comp.elps_id;
            ViewBag.FacilityId = fac.Elps_Id;
            ViewBag.appId = getApp.id;

            return View(rqDocs);
        }

        #region new Application Document Upload
        /*
         * Upload company's document first time
         * 
         * id => encrypted application id
         */
        [Authorize(Roles = "COMPANY")]
        public IActionResult UploadApplicationDocument(string id) // application id
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            
            int app_id = Convert.ToInt32(generalClass.Decrypt(id));
            string appid = generalClass.Encrypt("1503");

            var ElpsFacilityID = "";
            var appDetails = from app in _context.applications
                             join fac in _context.Facilities on app.FacilityId equals fac.Id
                             join company in _context.companies on app.company_id equals company.id
                             join cat in _context.Categories on app.category_id equals cat.id
                             join phs in _context.Phases on app.PhaseId equals phs.id
                             from doc in _context.ApplicationDocuments

                             where ((doc.PhaseId == 0 && doc.docType.ToLower() == "company") || doc.PhaseId == app.PhaseId) && app.DeleteStatus != true && doc.DeleteStatus != true
                              && fac.DeletedStatus != true && company.DeleteStatus != true && app.id == app_id
                             select new
                             {
                                 CategoryId = cat.id,
                                 PhaseId = phs.id,
                                 AppID = app.id,
                                 AppRef = app.reference,
                                 FacilityName = fac.Name,
                                 LocalFacilityID = fac.Id,
                                 ElpsFacilityID = fac.Elps_Id,
                                 LocalCompanyID = company.id,
                                 ElpsCompanyID = company.elps_id,
                                 AppDocID = doc.AppDocID,
                                 EplsDocTypeID = doc.ElpsDocTypeID,
                                 DocName = doc.DocName,
                                 docType = doc.docType,
                                 AppCategory = cat.name,
                                 AppType = app.type
                             };

            List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
            List<MissingDocument> missingDocuments = new List<MissingDocument>();
            List<PresentDocuments> presentDocuments2 = new List<PresentDocuments>();
            List<MissingDocument> missingDocuments2 = new List<MissingDocument>();
            List<BothDocuments> bothDocuments = new List<BothDocuments>();

            if (appDetails.Count() > 0)
            {

                if (appDetails.FirstOrDefault().ElpsFacilityID == null)
                {
                    //This simply means that facility has not been posted to ELPS
                    #region Update Facility on ELPS 

                    var facility = _context.Facilities.Where(a => a.Id == appDetails.FirstOrDefault().LocalFacilityID).FirstOrDefault();
                    var facilityAddress = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();


                    FacilityAPIModel facmodel = new FacilityAPIModel()
                    {
                        Name = facility.Name,
                        FacilityType = "Depot",
                        LGAId = (int)facilityAddress.LgaId,
                        City = facilityAddress.city,
                        StateId = facilityAddress.StateId,
                        StreetAddress = facilityAddress.address_1
                    };

                    var param = JsonConvert.SerializeObject(facmodel);
                    var paramDatas = _restService.parameterData("fac", param);
                    var response = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facmodel);
                    if (response.IsSuccessful == false)
                    {
                        _helpersController.LogMessages("Error posting facility with name: " + facmodel.Name + " to ELPS.", userEmail);
                    }
                    var respApp = JsonConvert.DeserializeObject<FacilityAPIModel>(response.Content.ToString());

                    if (respApp != null && respApp.Id > 0)
                    {
                        facility.Elps_Id = respApp.Id;
                        _context.SaveChanges();

                        ElpsFacilityID = respApp.Id.ToString();
                    }
                    #endregion
                }

                ElpsFacilityID = ElpsFacilityID != "" ? ElpsFacilityID : appDetails.FirstOrDefault().ElpsFacilityID.ToString();

                ViewData["FacilityName"] = appDetails.FirstOrDefault().FacilityName;
                ViewData["AppStage"] = appDetails.FirstOrDefault().AppCategory;
                ViewData["AppType"] = appDetails.FirstOrDefault().AppType.ToUpper();
                ViewData["AppID"] = appDetails.FirstOrDefault().AppID;
                ViewData["CompanyElpsID"] = appDetails.FirstOrDefault().ElpsCompanyID;
                ViewData["FacilityElpsID"] = ElpsFacilityID;
                ViewData["AppReference"] = appDetails.FirstOrDefault().AppRef;

                List<LpgLicense.Models.Document> companyDoc = generalClass.getCompanyDocuments(appDetails.FirstOrDefault().ElpsCompanyID.ToString());
                List<LpgLicense.Models.FacilityDocument> facilityDoc = generalClass.getFacilityDocuments(appDetails.FirstOrDefault().ElpsFacilityID.ToString());

                if (companyDoc == null)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                }

                foreach (var appDoc in appDetails.ToList())
                {
                    if (appDoc.docType == "Company")
                    {
                        foreach (var cDoc in companyDoc)
                        {
                            if (cDoc.document_type_id == appDoc.EplsDocTypeID.ToString())
                            {
                                presentDocuments.Add(new PresentDocuments
                                {
                                    Present = true,
                                    FileName = cDoc.fileName,
                                    Source = cDoc.source,
                                    CompElpsDocID = cDoc.id,
                                    DocTypeID = Convert.ToInt32(cDoc.document_type_id),
                                    LocalDocID = appDoc.AppDocID,
                                    DocType = appDoc.docType,
                                    TypeName = cDoc.documentTypeName
                                });
                            }

                        }
                    }
                    else
                    {
                        if (facilityDoc == null && (appDetails.FirstOrDefault().PhaseId != 1 && appDetails.FirstOrDefault().CategoryId != 8 && appDetails.FirstOrDefault().CategoryId != 1002))
                        {
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });

                        }
                        if (facilityDoc != null)
                        {
                            foreach (var fDoc in facilityDoc)
                            {
                                if (fDoc.Document_Type_Id == appDoc.EplsDocTypeID)
                                {
                                    presentDocuments.Add(new PresentDocuments
                                    {
                                        Present = true,
                                        FileName = fDoc.Name,
                                        Source = fDoc.Source,
                                        CompElpsDocID = fDoc.Id,
                                        DocTypeID = fDoc.Document_Type_Id,
                                        LocalDocID = appDoc.AppDocID,
                                        DocType = appDoc.docType,
                                        TypeName = appDoc.DocName

                                    });
                                }
                            }
                        }
                    }
                }

                var result = appDetails.AsEnumerable().Where(x => !presentDocuments.AsEnumerable().Any(x2 => x2.LocalDocID == x.AppDocID));


                foreach (var r in result)
                {
                    missingDocuments.Add(new MissingDocument
                    {
                        Present = false,
                        DocTypeID = r.EplsDocTypeID,
                        LocalDocID = r.AppDocID,
                        DocType = r.docType,
                        TypeName = r.DocName
                    });
                }

                presentDocuments = presentDocuments.GroupBy(x => x.TypeName).Select(c => c.FirstOrDefault()).ToList();


                var allAppsDoc = _context.ApplicationDocuments.Where(x => x.DeleteStatus != true);
                var excludedDocs = allAppsDoc.AsEnumerable().Where(x => !appDetails.AsEnumerable().Any(c => c.AppDocID == x.AppDocID && c.DocName.ToLower() == x.DocName.ToLower())).ToList();



                var appDetails2 = from app in _context.applications
                                  join fac in _context.Facilities on app.FacilityId equals fac.Id
                                  join company in _context.companies on app.company_id equals company.id
                                  join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                  join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                  join subd in _context.SubmittedDocuments on app.id equals subd.AppID
                                  join doc in _context.ApplicationDocuments on subd.AppDocID equals doc.AppDocID
                                  where app.DeleteStatus != true && doc.DeleteStatus != true
                                   && fac.DeletedStatus != true && company.DeleteStatus != true && app.id == app_id
                                   && subd.IsAdditional == true
                                  select new
                                  {
                                      AppID = app.id,
                                      AppRef = app.reference,
                                      FacilityName = fac.Name,
                                      LocalFacilityID = fac.Id,
                                      ElpsFacilityID = fac.Elps_Id,
                                      LocalCompanyID = company.id,
                                      ElpsCompanyID = company.elps_id,
                                      AppDocID = doc.AppDocID,
                                      EplsDocTypeID = doc.ElpsDocTypeID,
                                      DocName = doc.DocName,
                                      docType = doc.docType,
                                      AppCategory = cat.name,
                                      AppType = app.type,
                                      SubmitDocID = subd.SubDocID
                                  };

                if (appDetails2.Count() > 0)
                {
                    List<LpgLicense.Models.Document> companyDoc2 = generalClass.getCompanyDocuments(appDetails2.FirstOrDefault().ElpsCompanyID.ToString());
                    List<LpgLicense.Models.FacilityDocument> facilityDoc2 = generalClass.getFacilityDocuments(appDetails2.FirstOrDefault().ElpsFacilityID.ToString());

                    if (companyDoc2 == null || facilityDoc2 == null)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                    }

                    foreach (var appDoc in appDetails2.ToList())
                    {
                        if (appDoc.docType == "Company")
                        {
                            foreach (var cDoc in companyDoc2)
                            {
                                if (cDoc.document_type_id == appDoc.EplsDocTypeID.ToString())
                                {
                                    presentDocuments2.Add(new PresentDocuments
                                    {
                                        SubmitDocID = appDoc.SubmitDocID,
                                        Present = true,
                                        FileName = cDoc.fileName,
                                        Source = cDoc.source,
                                        CompElpsDocID = cDoc.id,
                                        DocTypeID = Convert.ToInt32(cDoc.document_type_id),
                                        LocalDocID = appDoc.AppDocID,
                                        DocType = appDoc.docType,
                                        TypeName = cDoc.documentTypeName
                                    });
                                }
                            }
                        }
                        else
                        {
                            foreach (var fDoc in facilityDoc2)
                            {
                                if (fDoc.Document_Type_Id == appDoc.EplsDocTypeID)
                                {
                                    presentDocuments2.Add(new PresentDocuments
                                    {
                                        SubmitDocID = appDoc.SubmitDocID,
                                        Present = true,
                                        FileName = fDoc.Name,
                                        Source = fDoc.Source,
                                        CompElpsDocID = fDoc.Id,
                                        DocTypeID = fDoc.Document_Type_Id,
                                        LocalDocID = appDoc.AppDocID,
                                        DocType = appDoc.docType,
                                        TypeName = appDoc.DocName

                                    });
                                }
                            }
                        }
                    }
                    var result2 = appDetails2.AsEnumerable().Where(x => !presentDocuments2.AsEnumerable().Any(x2 => x2.LocalDocID == x.AppDocID));

                    foreach (var r in result2)
                    {
                        missingDocuments2.Add(new MissingDocument
                        {
                            SubmitDocID = r.SubmitDocID,
                            Present = false,
                            DocTypeID = r.EplsDocTypeID,
                            LocalDocID = r.AppDocID,
                            DocType = r.docType,
                            TypeName = r.DocName
                        });
                    }

                    presentDocuments2 = presentDocuments2.GroupBy(x => x.TypeName).Select(c => c.FirstOrDefault()).ToList();
                }

                bothDocuments.Add(new BothDocuments
                {
                    missingDocuments = missingDocuments,
                    presentDocuments = presentDocuments,
                    presentDocuments2 = presentDocuments2,
                    missingDocuments2 = missingDocuments2,
                    AdditionalDoc = excludedDocs.GroupBy(x => x.DocName).Select(c => c.FirstOrDefault()).ToList()
                });
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Trying to find Application(Type, Stage or documents). Kindly contact support.") });
            }

            _helpersController.LogMessages("Displaying company upload documents for " + ViewData["FacilityName"], generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return View(bothDocuments.ToList());
        }


        /*
         * Customer adding more documents for uploads 
         * 
         * 
         */
        public JsonResult AddMoreDocuments(int AppID, int AppDocID)
        {
            string result = "";

            var getAppDoc = _context.ApplicationDocuments.Where(x => x.AppDocID == AppDocID && x.DeleteStatus != true).FirstOrDefault();

            var checkDoc = (from sb in _context.SubmittedDocuments
                            join ad in _context.ApplicationDocuments on sb.AppDocID equals ad.AppDocID
                            where (sb.AppDocID == AppDocID || ad.DocName.ToLower() == getAppDoc.DocName.ToLower())
                            && sb.AppID == AppID && sb.DeletedStatus != true && ad.DeleteStatus != true
                            select sb).ToList();

            if (checkDoc.Count() > 0)
            {
                result = "This document has already been added to your uploads.";
            }
            else
            {

                SubmittedDocuments submitDoc = new SubmittedDocuments()
                {
                    AppID = AppID,
                    AppDocID = AppDocID,
                    IsAdditional = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.SubmittedDocuments.Add(submitDoc);

                if (_context.SaveChanges() > 0)
                {
                    result = "Done";
                }
                else
                {
                    result = "Something went wrong trying to add this documents.";
                }
            }

            _helpersController.LogMessages("Trying to add more document for uploads.... Status :  " + result + ", Document ID : " + AppDocID + ", Application ID : " + AppID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(result);
        }


        /*
         * Removing already submitted document for the first upload, and not reuploading.
         * 
         */
        public JsonResult RemoveAdditionalDocuments(int AppID, int SubmitDocID)
        {
            var result = "";

            var checkDoc = _context.SubmittedDocuments.Where(x => x.AppID == AppID && x.SubDocID == SubmitDocID);

            if (checkDoc.Count() > 0)
            {
                _context.SubmittedDocuments.Remove(checkDoc.FirstOrDefault());

                if (_context.SaveChanges() > 0)
                {
                    result = "Done";
                }
                else
                {
                    result = "Something went wrong trying to remove this document.";
                }
            }
            else
            {
                result = "The document you want to remove was not found in the initial added documents. Please try again.";
            }

            return Json(result);
        }

        /*
         * Getting the list of all document request by staff to be 
         * submitted by the customer.
         * 
         * id => encrypted application id.
         */
        [Authorize(Roles = "COMPANY")]
        public IActionResult ReUploadDocument(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }

            int app_id = Convert.ToInt32(generalClass.Decrypt(id));
            var appDetails2 = from app in _context.applications
                                  //join dk in _context.MyDesk on app.current_desk equals dk.StaffID
                              join fac in _context.Facilities on app.FacilityId equals fac.Id
                              join company in _context.companies on app.company_id equals company.id
                              join cat in _context.Categories on app.category_id equals cat.id
                              join phs in _context.Phases on app.PhaseId equals phs.id
                              join subd in _context.SubmittedDocuments on app.id equals subd.AppID
                              join doc in _context.ApplicationDocuments on subd.AppDocID equals doc.AppDocID
                              where /*subd.CompElpsDocID == null && subd.IsAdditional == true &&*/
                              app.DeleteStatus != true && doc.DeleteStatus != true
                              && fac.DeletedStatus != true && company.DeleteStatus != true && app.id == app_id
                              select new
                              {
                                  PhaseId = phs.id,
                                  AppID = app.id,
                                  AppRef = app.reference,
                                  FacilityName = fac.Name,
                                  LocalFacilityID = fac.Id,
                                  ElpsFacilityID = fac.Elps_Id,
                                  LocalCompanyID = company.id,
                                  ElpsCompanyID = company.elps_id,
                                  AppDocID = doc.AppDocID,
                                  ElpsDocID = subd.CompElpsDocID,
                                  EplsDocTypeID = doc.ElpsDocTypeID,
                                  DocName = doc.DocName,
                                  docType = doc.docType,
                                  AppCategory = cat.name,
                                  AppType = app.type,
                                  DocSource = subd.DocSource,
                                  DeskComment = _context.MyDesk.Where(d => d.AppId == app_id).OrderByDescending(d => d.DeskID).FirstOrDefault().Comment,
                                  SubmitDocID = subd.SubDocID
                              };


            List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
            List<MissingDocument> missingDocuments = new List<MissingDocument>();
            List<BothDocuments> bothDocuments = new List<BothDocuments>();

            if (appDetails2.Count() > 0)
            {

                ViewData["FacilityName"] = appDetails2.FirstOrDefault().FacilityName;
                ViewData["AppStage"] = appDetails2.FirstOrDefault().AppCategory;
                ViewData["AppType"] = appDetails2.FirstOrDefault().AppType.ToUpper();
                ViewData["AppID"] = appDetails2.FirstOrDefault().AppID;
                ViewData["CompanyElpsID"] = appDetails2.FirstOrDefault().ElpsCompanyID;
                ViewData["FacilityElpsID"] = appDetails2.FirstOrDefault().ElpsFacilityID;
                ViewData["AppReference"] = appDetails2.FirstOrDefault().AppRef;
                ViewData["DeskComment"] = appDetails2.FirstOrDefault().DeskComment;


                List<LpgLicense.Models.Document> companyDoc2 = generalClass.getCompanyDocuments(appDetails2.FirstOrDefault().ElpsCompanyID.ToString());

                List<LpgLicense.Models.FacilityDocument> facilityDoc2 = generalClass.getFacilityDocuments(appDetails2.FirstOrDefault().ElpsFacilityID.ToString());

                if (companyDoc2 == null || facilityDoc2 == null)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                }

                foreach (var appDoc in appDetails2.ToList())
                {
                    if (appDoc.docType == "Company")
                    {
                        var cDoc = companyDoc2.OrderByDescending(x => x.id).Where(x => x.document_type_id == appDoc.EplsDocTypeID.ToString()).FirstOrDefault();

                        if (cDoc != null && cDoc.document_type_id == appDoc.EplsDocTypeID.ToString())
                        {
                            presentDocuments.Add(new PresentDocuments
                            {
                                SubmitDocID = appDoc.SubmitDocID,
                                Present = true,
                                FileName = cDoc.fileName,
                                Source = cDoc.source,
                                CompElpsDocID = cDoc.id,
                                DocTypeID = Convert.ToInt32(cDoc.document_type_id),
                                LocalDocID = appDoc.AppDocID,
                                DocType = appDoc.docType,
                                TypeName = cDoc.documentTypeName
                            });
                        }

                    }
                    else
                    {
                        var fDoc = facilityDoc2.OrderByDescending(x => x.Id).Where(x => x.Document_Type_Id == appDoc.EplsDocTypeID || x.Id == appDoc.ElpsDocID).FirstOrDefault();

                        if (fDoc != null)
                        {
                            presentDocuments.Add(new PresentDocuments
                            {
                                SubmitDocID = appDoc.SubmitDocID,
                                Present = true,
                                FileName = fDoc.Name,
                                Source = fDoc.Source,
                                CompElpsDocID = fDoc.Id,
                                DocTypeID = fDoc.Document_Type_Id,
                                LocalDocID = appDoc.AppDocID,
                                DocType = appDoc.docType,
                                TypeName = appDoc.DocName

                            });
                        }
                        else
                        {
                            var cDoc = companyDoc2.Where(x => x.id == appDoc.ElpsDocID).FirstOrDefault();
                            if (cDoc != null)
                            {
                                presentDocuments.Add(new PresentDocuments
                                {
                                    SubmitDocID = appDoc.SubmitDocID,
                                    Present = true,
                                    FileName = cDoc.fileName,
                                    Source = cDoc.source,
                                    CompElpsDocID = cDoc.id,
                                    DocTypeID = Convert.ToInt32(cDoc.document_type_id),
                                    LocalDocID = appDoc.AppDocID,
                                    DocType = "Company",
                                    TypeName = cDoc.documentTypeName
                                });
                            }

                        }

                    }
                }
                //Update submitted document with new records
                if (presentDocuments.Count() > 0)
                {
                    //foreach (var p in presentDocuments.Where(p=>p.DocType.ToLower()=="facility"))
                    foreach (var p in presentDocuments)
                    {
                        var sub = (from doc in _context.ApplicationDocuments
                                   join sb in _context.SubmittedDocuments on doc.AppDocID equals sb.AppDocID
                                   where sb.AppID == app_id && doc.ElpsDocTypeID == p.DocTypeID
                                   //&& sb.CompElpsDocID!=null || sb.DocSource!= null
                                   select sb).FirstOrDefault();
                        if (sub != null)
                        {
                            sub.CompElpsDocID = p.CompElpsDocID;
                            sub.DocSource = p.Source;
                            _context.SaveChanges();

                        }

                    }

                }


                var result2 = from app in _context.applications
                              join subd in _context.SubmittedDocuments on app.id equals subd.AppID
                              join doc in _context.ApplicationDocuments on subd.AppDocID equals doc.AppDocID
                              where (subd.CompElpsDocID == null || subd.DocSource == null)
                              && app.DeleteStatus != true && doc.DeleteStatus != true && app.id == app_id && subd.IsAdditional != true
                              select new
                              {
                                  AppID = app.id,
                                  AppRef = app.reference,
                                  AppDocID = doc.AppDocID,
                                  EplsDocTypeID = doc.ElpsDocTypeID,
                                  DocName = doc.DocName,
                                  docType = doc.docType,
                                  AppType = app.type,
                                  DocSource = subd.DocSource,
                                  DeskComment = _context.MyDesk.Where(d => d.AppId == app_id).OrderByDescending(d => d.DeskID).FirstOrDefault().Comment,
                                  SubmitDocID = subd.SubDocID
                              };

                var allAppsDoc = _context.ApplicationDocuments.Where(x => x.DeleteStatus != true);
                var excludedDocs = allAppsDoc.AsEnumerable().Where(x => !appDetails2.AsEnumerable().Any(c => c.AppDocID == x.AppDocID && c.DocName.ToLower() == x.DocName.ToLower())).ToList();
                foreach (var r in result2)
                {
                    var missingDoc = missingDocuments.Where(x => x.DocTypeID == r.EplsDocTypeID).FirstOrDefault();
                    var presDoc = presentDocuments.Where(x => x.DocTypeID == r.EplsDocTypeID).FirstOrDefault();
                    if (missingDoc == null && presDoc == null)
                    {

                        missingDocuments.Add(new MissingDocument
                        {
                            SubmitDocID = r.SubmitDocID,
                            Present = false,
                            DocTypeID = r.EplsDocTypeID,
                            LocalDocID = r.AppDocID,
                            DocType = r.docType,
                            TypeName = r.DocName
                        });
                    }
                }

                presentDocuments = presentDocuments.GroupBy(x => x.TypeName).Select(c => c.FirstOrDefault()).ToList();

                var getRemainingDoc = _context.SubmittedDocuments.AsEnumerable().Where(a => a.AppID == app_id && a.DeletedStatus != true && !presentDocuments.AsEnumerable().Any(x => x.LocalDocID == a.AppDocID)).ToList();



                if (getRemainingDoc.Count > 0)
                {
                    foreach (var x in getRemainingDoc)
                    {
                        var appDoc = (from ad in _context.ApplicationDocuments
                                      where (ad.AppDocID == x.AppDocID)
                                      select ad).FirstOrDefault();


                        if (x.CompElpsDocID != null && x.DocSource != null)
                        {

                            PresentDocuments pd = new PresentDocuments()
                            {
                                FileName = appDoc?.DocName,
                                Source = x.DocSource,
                                CompElpsDocID = (int)x.CompElpsDocID,
                                SubmitDocID = x.SubDocID,
                                Present = true,
                                LocalDocID = x.AppDocID,
                                DocTypeID = appDoc.ElpsDocTypeID,
                                DocType = appDoc?.docType,
                                TypeName = appDoc?.DocName
                            };
                            presentDocuments.Add(pd);
                        }
                        else
                        {
                            if (presentDocuments.Where(x => x.TypeName.ToLower() == appDoc?.DocName.ToLower()).FirstOrDefault() == null)
                            {
                                missingDocuments.Add(new MissingDocument
                                {
                                    SubmitDocID = x.SubDocID,
                                    Present = false,
                                    DocTypeID = appDoc.ElpsDocTypeID,
                                    LocalDocID = x.AppDocID,
                                    DocType = appDoc?.docType,
                                    TypeName = appDoc?.DocName
                                });
                            }
                            else
                            {
                                //Remove already uploaded documents from list of required documents
                                x.DeletedStatus = true;
                                x.DeletedAt = DateTime.Now;
                                _context.SaveChanges();

                            }
                        }
                    }
                }

                bothDocuments.Add(new BothDocuments
                {
                    missingDocuments = missingDocuments.Distinct().ToList(),
                    presentDocuments = presentDocuments,
                    AdditionalDoc = excludedDocs.GroupBy(x => x.DocName).Select(c => c.FirstOrDefault()).ToList()
                });
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }

            _helpersController.LogMessages("Displaying company Re-upload documents for " + ViewData["FacilityName"], generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return View(bothDocuments.ToList());
        }



        /*
         * Submitting application documents for the first time
         * AppID => encrypted application ID 
         * SubmittedDocuments => a list of documents to be submitted
         */
        [Authorize(Roles = "COMPANY")]
        public IActionResult SubmitDocuments(int AppID, List<SubmitDoc> SubmittedDocuments)
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();

            var result = "";
            foreach (var item in SubmittedDocuments)
            {
                var check_doc = _context.SubmittedDocuments.Where(x => x.AppID == AppID && x.AppDocID == item.LocalDocID);

                if (check_doc.Count() <= 0)
                {
                    SubmittedDocuments submitDocs = new SubmittedDocuments()
                    {
                        AppID = AppID,
                        AppDocID = item.LocalDocID,
                        CompElpsDocID = item.CompElpsDocID,
                        CreatedAt = DateTime.Now,
                        DeletedStatus = false,
                        DocSource = item.DocSource
                    };
                    _context.SubmittedDocuments.Add(submitDocs);
                }
                else
                {
                    check_doc.FirstOrDefault().AppID = AppID;
                    check_doc.FirstOrDefault().AppDocID = item.LocalDocID;
                    check_doc.FirstOrDefault().CompElpsDocID = item.CompElpsDocID;
                    check_doc.FirstOrDefault().DocSource = item.DocSource;
                    check_doc.FirstOrDefault().UpdatedAt = DateTime.Now;
                }
            }

            int done = _context.SaveChanges();

            if (done > 0)
            {
                //updating application status
                var getApp = _context.applications.Where(x => x.id == AppID && x.DeleteStatus != true).FirstOrDefault();

                if (getApp != null)
                {
                    var facility = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
                    var facAddress = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    //get the phase that the Application belongs to and also find its category.
                    var cat = _context.Categories.Where(a => a.id == getApp.category_id).FirstOrDefault();
                    var phas = _context.Phases.Where(a => a.id == getApp.PhaseId).FirstOrDefault();

                    //Now check if this application payment has been completed
                    if (getApp.status == GeneralClass.PaymentCompleted)
                    {

                        //Move to processing after Payment
                        if (CreateProcessingRules(getApp.id))
                        {
                            SendApplicationSubmittedMail(getApp, getApp.current_Permit);

                            getApp.status = "Processing";
                            getApp.submitted = true;
                            getApp.CreatedAt = DateTime.Now;
                            getApp.UpdatedAt = DateTime.Now;
                            _context.SaveChanges();

                            _helpersController.SaveHistory(getApp.id, (int)getApp.current_desk, "Submission", "Application submitted and landed on staff desk.");
                            _helpersController.LogMessages("Application submission", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                            string reslink = "/Application/SubmitRedirect/?apid=" + generalClass.Encrypt(getApp.id.ToString()) + "&refCode=" + generalClass.Encrypt(getApp.reference);
                            result = "2|" + reslink;
                        }
                        else
                        {
                            result = "0|Something went wrong while trying to create process rule for this application.";
                        }
                    }
                    else
                    {
                        //change app status
                        getApp.status = "Payment Pending";

                        int chnge = _context.SaveChanges();
                        string subject = (getApp.type.ToLower() == "new" ? "Application Initiated: " : "Permit Renewal Initiated: ") + getApp.reference;

                        var msgCheck = _context.messages.Where(a => a.subject.ToLower().Trim() == subject.ToLower().Trim()).FirstOrDefault();
                        if (msgCheck == null)
                        {
                            #region Send Application Initiation Mail

                            var msg = new messages();
                            msg.AppId = getApp.id;
                            msg.company_id = comp.id;
                            msg.content = "Loading...";
                            msg.date = DateTime.Now;
                            msg.read = 0;
                            msg.UserType = "Company";
                            msg.subject = subject;
                            msg.sender_id = "Application";
                            msg.UserID = 0;
                            _context.messages.Add(msg);
                            int i = _context.SaveChanges();
                            if (i > 0)
                            {
                                var sn = msg.id;
                                var body = "";
                                var apDetails = "";

                                #region mail type
                                var mType = "";
                                mType = cat.name + " (" + phas.name + ")";

                                #endregion
                                var tk = string.Format("You have initiated a new application for " + mType + " on NMDPRA Depot portal. Details of your application are as follows: " +
                                    "<br /><ul><li>Tracking Number: {0}</li><li>Application Type: {1}</li><li>Permit Category: {2}</li>" +
                                    "<li>Amount Due: {3}</li><li>Payment Status: Unpaid</li><li>Application Period: {4}</li>",
                                    getApp.reference, getApp.type, cat.name, getApp.fee_payable + getApp.service_charge, getApp.year);

                                var src = "<ul>";
                                src += "<li>" + facility.Name + " (" + facAddress.address_1 + ")</li>";
                                src += "</ul>";
                                var services = "<p>Facility:<br>" + src + "<br /></p>";


                                apDetails = tk + services;
                                var msgBody = string.Format(body, subject, apDetails, sn);


                                var mm = _context.messages.Where(m => m.id == msg.id).FirstOrDefault();
                                if (mm != null)
                                {
                                    mm.content = msgBody;
                                    _context.SaveChanges();
                                    var emailMsg = _helpersController.SaveMessage(getApp.id, comp.id, subject, tk, comp.elps_id.ToString(), "Company");
                                    //var sendEmail = _helpersController.SendEmailMessage(subject, comp.CompanyEmail.ToString(), comp.name, apDetails, null);
                                    var sendEmail = _helpersController.SendEmailMessage(comp.CompanyEmail.ToString(), comp.name, emailMsg, null);

                                }
                                else
                                {
                                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, message record wasn't found.") });

                                }
                            }
                            else
                            {
                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("An error occured while updating message record.") });
                            }
                            #endregion
                        }
                        _helpersController.LogMessages("All documents uploaded successfully", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                        string reslink = "/Application/Payment/?apid=" + generalClass.Encrypt(getApp.id.ToString()) + "&refCode=" + generalClass.Encrypt(getApp.reference);
                        result = "1|" + reslink;
                    }
                }

                else
                {
                    result = "0|Something went wrong trying to fetch this application.";
                }
            }
            else
            {
                result = "0|Something went wrong trying to save submitted documents.";
            }

            _helpersController.LogMessages(result, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
            return Json(result);

        }

        public IActionResult SubmitRedirect(string apid)
        {
            int appID = generalClass.DecryptIDs(apid);

            MyApps MyApp = (from app in _context.applications.AsEnumerable()
                            join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                            join facil in _context.Facilities.AsEnumerable() on app.FacilityId equals facil.Id
                            join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                            join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                            join ad in _context.addresses on facil.AddressId equals ad.id
                            where app.DeleteStatus != true && app.id == appID && c.DeleteStatus != true
                            select new MyApps
                            {
                                appID = app.id,
                                Reference = app.reference,
                                CategoryName = cat.name,
                                PhaseName = phs.name,
                                category_id = cat.id,
                                FacilityId = facil.Id,
                                PhaseId = phs.id,
                                AppPermit = _context.permits.Where(x => x.application_id == app.id).FirstOrDefault(),
                                Current_Permit = "ok",
                                Address_1 = ad.address_1,
                                Status = app.status,
                                Date_Added = Convert.ToDateTime(app.date_added),
                                Submitted = app.submitted,
                                CompanyDetails = c.name + " (" + c.Address + ") ",
                                FacilityDetails = facil.Name,

                            }).FirstOrDefault();

            return View("ApplicationSuccess", MyApp);
        }

        /*
         * Re-submting company's documents
         */
        [Authorize(Roles = "COMPANY")]
        public JsonResult ReSubmitDocuments(int AppID, List<SubmitDoc> ReSubmittedDocuments)
        {
            var result = "";
            foreach (var item in ReSubmittedDocuments)
            {
                var check_doc = _context.SubmittedDocuments.Where(x => x.AppID == AppID && x.AppDocID == item.LocalDocID);

                if (check_doc.Count() > 0)
                {
                    check_doc.FirstOrDefault().AppID = AppID;
                    check_doc.FirstOrDefault().AppDocID = item.LocalDocID;
                    check_doc.FirstOrDefault().CompElpsDocID = item.CompElpsDocID;
                    check_doc.FirstOrDefault().UpdatedAt = DateTime.Now;
                    check_doc.FirstOrDefault().DocSource = item.DocSource;
                    check_doc.FirstOrDefault().DeletedStatus = false;
                }
            }

            int done = _context.SaveChanges();

            if (done > 0)
            {
                //updating application status
                var get = _context.applications.Where(x => x.id == AppID && x.DeleteStatus != true);
                var company = _context.companies.Where(x => x.id == get.FirstOrDefault().company_id && x.DeleteStatus != true);

                if (get.Count() > 0 && company.Count() > 0)
                {
                    // getting application type and stage 
                    var type = from ts in _context.Categories
                               join t in _context.Phases on ts.id equals t.category_id
                               where ts.id == get.FirstOrDefault().category_id
                               select new
                               {
                                   Category = t.name + " Application (" + t.name + " - " + t.ShortName + ")",
                                   ShortName = t.ShortName
                               };

                    // returning application back to officer
                    var desk = _context.MyDesk.Where(x => x.AppId == AppID && x.StaffID == get.FirstOrDefault().current_desk).OrderByDescending(x => x.Sort);

                    if (desk == null || desk.Count() <= 0)
                    {

                        desk = _context.MyDesk.Where(x => x.AppId == AppID).OrderByDescending(x => x.Sort);
                    }
                    int staffID = desk.FirstOrDefault().StaffID;

                    get.FirstOrDefault().status = GeneralClass.Processing;
                    get.FirstOrDefault().UpdatedAt = DateTime.Now;

                    desk.FirstOrDefault().HasWork = false;
                    desk.FirstOrDefault().UpdatedAt = DateTime.Now;

                    int saved = _context.SaveChanges();

                    if (saved > 0)
                    {
                        _helpersController.SaveHistory(get.FirstOrDefault().id, desk.FirstOrDefault().StaffID, "Moved", "Application was re-submitted to staff.");
                        result = "Resubmitted";

                        // Saving Messages
                        string subject = type.FirstOrDefault().ShortName + " Application Re-Submitted With Ref : " + get.FirstOrDefault().reference;
                        string content = "You have re-submitted your application (" + type.FirstOrDefault().Category + ") with Reference Number " + get.FirstOrDefault().reference + " for processing on NMDPRA PDJ Depot portal. Kindly find other details below.";
                        var emailMsg = _helpersController.SaveMessage(get.FirstOrDefault().id, company.FirstOrDefault().id, subject, content, company.FirstOrDefault().elps_id.ToString(), "Company");
                        var sendEmail = _helpersController.SendEmailMessage(company.FirstOrDefault().CompanyEmail.ToString(), company.FirstOrDefault().name, emailMsg, null);



                        var getApps = _context.applications.Where(x => x.id == get.FirstOrDefault().id);

                        string subj = "Application (" + getApps.FirstOrDefault().reference + ") Re-submitted For Your Action";
                        string cont = "Application with reference number " + getApps.FirstOrDefault().reference + " has been resubmitted for processing.";

                        var getStaff = _context.Staff.Where(a => a.StaffID == staffID).FirstOrDefault();

                        var emailMsg2 = _helpersController.SaveMessage(get.FirstOrDefault().id, getStaff.StaffID, subj, cont, getStaff.StaffElpsID, "Staff");
                        var sendEmail2 = _helpersController.SendEmailMessage2Staff(getStaff.StaffEmail, getStaff.FirstName, emailMsg2, null);

                    }
                    else
                    {
                        result = "Something went wrong trying to update application status.";
                    }
                }
                else
                {
                    result = "Something went wrong. Application and Company was not found or has been deleted.";
                }
            }
            else
            {
                result = "Something went wrong trying to save re-submitted documents.";
            }
            _helpersController.LogMessages("Resubmit application" + result, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
            return Json(result);
        }

        #endregion
        [HttpPost]
        public JsonResult ResubmitApplication(string companyId, string appId)
        {

            try
            {
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();

                var getApp = _context.applications.Where(a => a.id == Convert.ToInt32(appId)).FirstOrDefault();
                var facility = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
                var facAdd = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                var cat = _context.Categories.Where(a => a.id == getApp.category_id).FirstOrDefault();
                var ps = _context.Phases.Where(a => a.id == getApp.PhaseId).FirstOrDefault();

                //get the phase that the Application belongs to and also find its category.
                var rqDocs = appHelper.GetApplicationFiles(getApp, comp.elps_id.GetValueOrDefault(), facility.Elps_Id.GetValueOrDefault());// getApplicationFiles(ap, comp.elps_id.GetValueOrDefault());
                var phasefcdocs = _context.PhaseFacilityDocuments.Where(x => x.PhaseId == getApp.PhaseId).ToList();
                var ad = rqDocs.Where(a => a.Selected == false && phasefcdocs.Any(x => x.document_type_id == a.Document_Id)).ToList();

                if (ad.Count > 0)
                {
                    //Some Documents not Uploaded yet
                    return Json("Kindly upload all required document(s).");
                }


                // returning application back to officer; get the application rejector
                var desk = _context.MyDesk.Where(x => x.AppId == getApp.id && x.StaffID == getApp.current_desk).OrderByDescending(x => x.DeskID).FirstOrDefault();
                if (desk != null)
                {
                    int staffID = desk.StaffID;

                    getApp.status = GeneralClass.Processing;
                    getApp.UpdatedAt = DateTime.Now;

                    desk.HasWork = false;
                    desk.UpdatedAt = DateTime.Now;

                    int saved = _context.SaveChanges();

                    if (saved > 0)
                    {

                        #region Send Application Resubmitted Mail to Company and Staff
                        //company
                        string subject = "Application Re-Submitted - Ref. No.: " + getApp.reference;
                        string tk = string.Format("Your Application has been Re-submited on the Depot portal. The process for your application with Reference Number: {0} will continue immediately.<br />", getApp.reference);
                        TempData["AppSubmitType"] = "Resubmit"; var src = "<table class='table table-bordered table-striped'>" +
         "<tr><td><b>Faciity Name:</b></td><td>" + facility.Name + "(" + facAdd.address_1 + ")</td></tr>" +
          "<tr><td><b>Application:</b></td><td>" + ps.name + "</td></tr></table>";

                        var apDetails = tk + src;

                        var emailMsg = _helpersController.SaveMessage(getApp.id, Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID))), subject, apDetails, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionElpsID)), "Company");
                        var sendEmail = _helpersController.SendEmailMessage(comp.CompanyEmail, comp.name, emailMsg, null);

                        //staff

                        var stf = _context.Staff.Where(st => st.StaffID == staffID).FirstOrDefault();
                        string subject2 = "Re-submission of Application With Ref: " + getApp.reference;
                        string content2 = comp.name + " has re-submitted their <b>" + ps.name + "</b> application for processing. Kindly go to your desk to see application details.";
                        var emailMsg2 = _helpersController.SaveMessage(getApp.id, stf.StaffID, subject2, content2, stf.StaffElpsID, "Staff");
                        var sendEmail2 = _helpersController.SendEmailMessage2Staff(stf.StaffEmail, stf.FirstName, emailMsg2, null);

                        #endregion


                        return Json("Success.");
                    }
                    else
                    {
                        string error = "An error occured while trying to re-submit your application for processing. Please contact support.";
                        return Json(error);

                    }
                }
                else
                {
                    string error = "Unable to find desk information for processing";
                    return Json(error);

                }
            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                //check the payment Status from remita through Elps;
                ViewBag.Error = "error";
                return Json(x.Message.ToString());

            }

        }

        [Authorize]
        public IActionResult DeleteApplication(int Id)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var app = _context.applications.Where(a => a.id == Id && a.company_id == userID).FirstOrDefault();
            if (app != null)
            {
                var getApp = _context.applications.Where(a => a.id == Id).FirstOrDefault();
                if (getApp != null)
                {
                    getApp.company_id = -1 * getApp.company_id;
                    getApp.date_modified = DateTime.UtcNow.AddHours(1);
                    getApp.DeleteStatus = true;

                    if (_context.SaveChanges() > 0)
                    {
                        //check if current permit is for legacy
                        var lg = _context.Legacies.Where(a => a.LicenseNo.Trim().ToLower() == getApp.current_Permit.Trim().ToLower()).FirstOrDefault();
                        if (lg != null)
                        {
                            lg.IsUsed = false;
                            _context.SaveChanges();
                        }

                    }
                    return RedirectToAction("MyApplications");
                }
            }
            string error = "Item does not Exist or has been removed";
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(error) });
        }
        [Authorize(Policy = "AdminRoles")]
        public IActionResult Deleted()
        {
            var apps = _context.applications.Where(a => a.company_id < 0 || a.DeleteStatus == true).ToList().OrderByDescending(a => a.date_added);
            return View(apps);

        }
        [Authorize(Policy = "AdminRoles")]
        public IActionResult UndoDelete(int id)
        {
            var getApp = _context.applications.Where(a => a.id == id).FirstOrDefault();
            if (getApp != null)
            {
                var checklegpermit = (from app in _context.applications
                                      join lg in _context.Legacies on app.current_Permit.ToLower() equals lg.LicenseNo.ToLower()
                                      where app.current_Permit == getApp.current_Permit && lg.IsUsed == true
                                      && app.DeleteStatus != true && app.company_id > 0
                                      select app).FirstOrDefault();

                if (checklegpermit != null)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, this legacy approval/permit number has already been used for another application.") });

                }


                getApp.company_id = getApp.company_id < 0 ? -1 * getApp.company_id : getApp.company_id;
                getApp.date_modified = DateTime.UtcNow.AddHours(1);
                getApp.DeleteStatus = false;

                if (_context.SaveChanges() > 0)
                {
                    //check if current permit is for legacy
                    var lg = _context.Legacies.Where(a => a.LicenseNo.Trim().ToLower() == getApp.current_Permit.Trim().ToLower()).FirstOrDefault();
                    if (lg != null)
                    {
                        lg.IsUsed = true;
                        _context.SaveChanges();
                    }

                }
                return RedirectToAction("ViewApplication", "Application", new { Id = id.ToString() });
            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, application does not exist on the system..") });

        }
        #endregion

        #region Permit Viewing
        public IActionResult Permits(int? id, string sdate, string edate)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            return RedirectToAction("Licenses", new { id = id, sdate = sdate, edate = edate });
            if (userRole.Contains("Staff") || userRole.Contains("Admin") || userRole.Contains("ITAdmin") || userRole.Contains("Director") || userRole.Contains("Account"))
            {

                List<permits> permits = new List<permits>();
                if (id != null && id > 0)
                {
                    var comp = _context.companies.Where(a => a.id == id.Value).FirstOrDefault();
                    #region Source from ELPS

                    #endregion

                    permits = _context.permits.Where(p => p.company_id == id).OrderByDescending(a => a.date_issued).ToList();
                    ViewBag.NewTitle = "All Permits/Licenses by \"" + comp.name + "\"";
                    return View("AllPermits", permits);
                }
                else
                {
                    DateTime sd = string.IsNullOrEmpty(sdate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(sdate).Date;
                    DateTime ed = string.IsNullOrEmpty(edate) ? DateTime.Now.Date : DateTime.Parse(edate).Date;

                    ViewBag.StartDate = sd.ToShortDateString();
                    ViewBag.EndDate = ed.ToShortDateString();

                    ViewBag.Category = _context.Categories.ToList();
                    int LicenseId = Convert.ToInt16(_configuration.GetSection("AmountSetting").GetSection("LicenseId").Value.ToString());
                    ViewBag.LicenseId = LicenseId;
                    var perModel = (from p in _context.permits
                                    join app in _context.applications on p.application_id equals app.id
                                    join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                    join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                    join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                    join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                    where app.DeleteStatus != true && p.company_id == id
                                    select new permitsModel
                                    {

                                        Id = p.id,
                                        Application_Id = app.id,
                                        Reference = app.reference,
                                        Date_Issued = p.date_issued,
                                        Date_Expire = p.date_expire,
                                        Category_id = cat.id,
                                        Permit_No = p.permit_no,
                                        CompanyName = c.name,
                                        FacilityName = fac.Name,
                                        CategoryName = cat.name,
                                        PhaseName = phs.name,
                                        OrderId = p.elps_id.ToString()
                                    }).OrderBy(a => a.Date_Issued).ToList();
                    return View("CompanyPermits", perModel);
                }
            }

            else
            {
                var comp = _context.companies.Where(a => a.CompanyEmail == userEmail).FirstOrDefault();
                var permits = new List<permits>();
                #region Source from ELPS
                //using (WebClient client = new WebClient())
                //{
                //    // pegetApplicationFilesorms an HTTP POST
                //    try
                //    {

                //        //client.Headers[HttpRequestHeader.Accept] = "application/json";
                //        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                //        var url = MKURL;

                //        string output = string.Empty;

                //        url += "Permits/" + comp.elps_id + "/" + MKEM + "/" + _helpersController.getHash(MKEM, MK);
                //        output = client.DownloadString(url);


                //        permits = JsonConvert.DeserializeObject<List<permits>>(output);

                //    }
                //    catch (Exception x)
                //    {
                //        // throw new ArgumentException(x.Message);
                //    }
                //}
                #endregion

                permits = _context.permits.Where(a => a.company_id == comp.id).ToList();
                return View(permits);
            }
        }

        #endregion

        //#region Permit Viewing


        public IActionResult Licenses(int? id, string sdate, string edate)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userRole != GeneralClass.COMPANY)
            //if (userRole!= GeneralClass.COMPANY &&( userRole.Contains("Admin") || userRole.Contains("ITAdmin") || userRole.Contains("Director") || userRole.Contains("Account")))
            {

                List<permits> permits = new List<permits>();
                if (id != null && id > 0)
                {
                    var comp = _context.companies.Where(a => a.id == id.Value).FirstOrDefault();

                    //permits = _permitsRep.Where(p => p.company_id == id).OrderByDescending(a => a.date_issued).ToList();
                    var perModel = (from p in _context.permits
                                    join app in _context.applications on p.application_id equals app.id
                                    join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                    join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                    join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                    join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                    where app.DeleteStatus != true && p.company_id == id
                                    select new permitsModel
                                    {
                                        Id = p.id,
                                        Application_Id = app.id,
                                        Reference = app.reference,
                                        Date_Issued = p.date_issued,
                                        Date_Expire = p.date_expire,
                                        CheckApprovalType = (phs.ShortName == "NDT" || phs.ShortName == "LTO" || phs.ShortName == "LR" || phs.ShortName == "TO" || phs.ShortName == "RC") ? "Yes" : "",
                                        ModifyType = _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault() != null ? _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault().Type : "",
                                        Category_id = cat.id,
                                        Is_Renewed = p.is_renewed,
                                        Permit_No = p.permit_no,
                                        CategoryName = cat.name,
                                        PhaseName = phs.name,
                                        CompanyName = c.name,
                                        FacilityName = fac.Name,
                                        OrderId = app.reference.ToString()
                                    }).OrderByDescending(a => a.Date_Issued).ToList();

                    ViewBag.NewTitle = "All Permits/Licenses for \"" + comp.name + "\"";

                    return View("AllPermits", perModel);
                }
                else
                {
                    DateTime sd = string.IsNullOrEmpty(sdate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(sdate).Date;
                    DateTime ed = (string.IsNullOrEmpty(edate) ? DateTime.Now.Date : DateTime.Parse(edate).Date).Date.AddHours(23).AddMinutes(59);

                    ViewBag.StartDate = sd.ToShortDateString();
                    ViewBag.EndDate = ed.ToShortDateString();
                    var perModel = (from p in _context.permits
                                    join app in _context.applications on p.application_id equals app.id
                                    join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                    join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                    join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                    join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                    where app.DeleteStatus != true
                                    select new permitsModel
                                    {
                                        Id = p.id,
                                        Application_Id = app.id,
                                        Reference = app.reference,
                                        Date_Issued = p.date_issued,
                                        Date_Expire = p.date_expire,
                                        CheckApprovalType = (phs.ShortName == "NDT" || phs.ShortName == "LTO" || phs.ShortName == "LR" || phs.ShortName == "TO" || phs.ShortName == "RC") ? "Yes" : "",
                                        ModifyType = _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault() != null ? _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault().Type : "",
                                        Category_id = cat.id,
                                        CompanyName = c.name,
                                        FacilityName = fac.Name,
                                        Is_Renewed = p.is_renewed,
                                        Permit_No = p.permit_no,
                                        CategoryName = cat.name,
                                        PhaseName = phs.name,
                                        OrderId = app.reference.ToString()
                                    }).OrderByDescending(a => a.Date_Issued).ToList();



                    return View("AllPermits", perModel);
                }
            }

            else
            {
                var comp = _context.companies.Where(a => a.CompanyEmail == userEmail).FirstOrDefault();
                var permits = new List<permits>();
                var perModel = (from p in _context.permits
                                join app in _context.applications on p.application_id equals app.id
                                join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                where app.DeleteStatus != true && p.company_id == comp.id
                                select new permitsModel
                                {
                                    Id = p.id,
                                    Date_Issued = p.date_issued,
                                    Date_Expire = p.date_expire,
                                    CheckApprovalType = (phs.ShortName == "NDT" || phs.ShortName == "LTO" || phs.ShortName == "LR" || phs.ShortName == "TO" || phs.ShortName == "RC") ? "Yes" : "",
                                    Category_id = cat.id,
                                    Permit_No = p.permit_no,
                                    CategoryName = cat.name,
                                    PhaseName = phs.name,
                                    Is_Renewed = p.is_renewed,
                                    Application_Id = app.id,
                                    Reference = app.reference,
                                    CompanyName = c.name,
                                    FacilityName = fac.Name,
                                    OrderId = app.reference.ToString()
                                }).OrderByDescending(a => a.Date_Issued).ToList();
                return View("CompanyPermits", perModel);

            }
        }


        //[Authorize(Roles = "Printer, Admin, HDS")]
        public IActionResult PrintLicenses(bool printed = false)
        {
            var permits = new List<permits>();
            var perModel = (from p in _context.permits
                            join app in _context.applications on p.application_id equals app.id
                            join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                            join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                            join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                            join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                            where app.DeleteStatus != true /*&& p.company_id == comp.id*/
                            select new permitsModel
                            {
                                Id = p.id,
                                Date_Issued = p.date_issued,
                                Date_Expire = p.date_expire,
                                CheckApprovalType = (phs.ShortName == "NDT" || phs.ShortName == "LTO" || phs.ShortName == "LR" || phs.ShortName == "TO" || phs.ShortName == "RC") ? "Yes" : "",
                                Category_id = cat.id,
                                Is_Renewed = p.is_renewed,
                                Permit_No = p.permit_no,
                                CategoryName = cat.name,
                                PhaseName = phs.name,
                                Application_Id = app.id,
                                Reference = app.reference,
                                Printed = p.Printed,
                                OrderId = app.reference.ToString()
                            }).OrderBy(a => a.Date_Issued).ToList();


            ViewBag.Printed = printed;
            if (printed)
            {
                ViewBag.Header = "Already Printed Licenses";
                perModel = perModel.Where(x => x.Printed == true).ToList();
            }
            else
            {
                ViewBag.Header = "Licenses Awaiting Printing";
                perModel = perModel.Where(x => x.Printed != true).ToList();
            }
            return View(perModel);
        }


        public IActionResult PreviewLicense(string id, string types)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            int appId = Convert.ToInt16(id);
            var app = _context.applications.Where(a => a.id == appId).FirstOrDefault();

            if (app != null)
            {
                var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();

                #region Handle Company DPRID
                var company = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();
                if (string.IsNullOrEmpty(company.DPR_Id))
                {

                    string dprid = "";
                    if (_helpersController.GenerateNMDPRAID_Preview(address.StateId, out dprid))
                    {

                    }
                }
                #endregion

                string viewName = "";
                var viewPdf = new ViewAsPdf();

                viewName = "PreviewNew";

                var ps = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                if (ps.IssueType == "Approval")
                {
                    viewPdf.ViewName = "Approval" + viewName;
                    viewPdf.Model = getApprovalPdf_Preview(app.id);
                }
                else
                {
                    viewPdf.ViewName = "License" + viewName;
                    viewPdf.Model = getPdf_Preview(app.id);
                }

                if (viewPdf.Model == null)
                {

                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network and try again.") });

                }


                return new ViewAsPdf(viewPdf.ViewName, viewPdf.Model)
                {
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,

                };

            }
            return null;
        }
        public IActionResult ViewLicense(string id, string type, string field, string permitID)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            int appId = Convert.ToInt16(id);
            int permitId = Convert.ToInt16(permitID);
            var permit = _context.permits.Where(a => a.id == permitId).FirstOrDefault();

            if (permit != null)
            {
                var app = _context.applications.Where(a => a.id == appId).FirstOrDefault();
                var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();

                #region Handle Company DPRID
                var company = _context.companies.Where(a => a.id == permit.company_id).FirstOrDefault();
                if (string.IsNullOrEmpty(company.DPR_Id))
                {

                    string dprid = "";
                    if (_helpersController.GenerateNMDPRAID(address.StateId, out dprid))
                    {

                        company.DPR_Id = dprid;
                        _context.SaveChanges();
                        _helpersController.LogMessages("DPR ID generated for " + company.name, userEmail);

                    }
                }
                #endregion

                string viewName = "";
                var viewPdf = new ViewAsPdf();


                if (type == "preview")
                {
                    viewName = "PreviewNew";
                }
                else
                {

                    viewName = "viewNew";
                }

                if (type == "new" && viewName == "Preview")
                {
                    viewPdf.ViewName = "ViewLicenseNPre";
                }
                else if (type == "new" && viewName == "view")
                {
                    viewPdf.ViewName = "ViewLicenseN";
                }


                var ps = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                if (ps.IssueType == "Approval")
                {

                    viewPdf.ViewName = "Approval" + viewName;
                    if (field != null)
                    {
                        viewPdf.Model = getApprovalPdf(permit.application_id, field);
                    }
                    else
                    {
                        viewPdf.Model = getApprovalPdf(permit.application_id);
                    }
                }
                else
                {

                    viewPdf.ViewName = "License" + viewName;
                    viewPdf.Model = getPdf(permit.id);
                }

                ViewBag.Field = field;



                if (viewPdf.Model == null)
                {

                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network and try again.") });

                }

                //var pdfBytes = viewPdf.ViewName.BuildFile(CreateDummyControllerContext("SiteSuperReports"));                //byte[] pd = BuildFile(viewName;s

                return new ViewAsPdf(viewPdf.ViewName, viewPdf.Model)
                {
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,

                };

            }
            return null;
        }


        /// <summary>
        ///// Showing Multiple Licenses for Printing/Preview
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <returns></returns>

        public PermitViewModel getPdf(int id)
        {

            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var permit = _context.permits.Where(a => a.id == id).FirstOrDefault();

                if (permit != null)
                {

                    var application = _context.applications.Where(a => a.id == permit.application_id).FirstOrDefault();
                    var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                    var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    var state = _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                    var company = _context.companies.Where(c => c.id == permit.company_id).FirstOrDefault();

                    var getCategory = _context.Categories.Where(ct => ct.id == application.category_id).FirstOrDefault();

                    List<KeyValuePair<string, double>> products = new List<KeyValuePair<string, double>>();

                    var tnks = (from at in _context.ApplicationTanks
                                join t in _context.Tanks on at.TankId equals t.Id
                                where at.ApplicationId == application.id
                                select at).ToList().GroupBy(x => x.ProductId).Select(c => c).ToList();

                    if (tnks.Count > 0)
                    {
                        foreach (var tank in tnks)
                        {
                            var prod = _context.Products.Where(x => x.Id == tank.Key).FirstOrDefault();
                            products.Add(new KeyValuePair<string, double>(prod.Name.ToString(), tank.Sum(a => a.Capacity)));
                        }
                    }
                    else
                    {
                        //var tanks = _context.Tanks.AsEnumerable().Where(a => a.FacilityId == application.FacilityId && !a.Decommissioned).GroupBy(a => a.Name);
                        var tanks = _context.Tanks.AsEnumerable().Where(a => a.FacilityId == application.FacilityId && !a.Decommissioned && (a.Status == null || a.Status == GeneralClass.Approved)).GroupBy(a => a.ProductId);

                        foreach (var tank in tanks)
                        {
                            var prod = _context.Products.Where(x => x.Id == tank.Key).FirstOrDefault();
                            products.Add(new KeyValuePair<string, double>(prod.Name, tank.Sum(a => Convert.ToDouble(a.MaxCapacity))));

                        }

                    }
                    var vAppProc = _context.MyDesk.Where(a => a.AppId == application.id).OrderByDescending(a => a.Sort).FirstOrDefault();
                    int approver = vAppProc.StaffID;
                    var me = _context.Staff.Where(a => a.StaffID == approver).FirstOrDefault();

                    //old signature
                    var signature = _context.Signatories.Where(a => a.Position == "Director" && a.StartDate >= permit.date_issued && (a.EndDate == null || a.EndDate <= permit.date_issued)).FirstOrDefault();

                    var newSignatory = (from st in _context.Staff
                                        join r in _context.UserRoles on st.RoleID equals r.Role_id
                                        where st.DeleteStatus != true && st.ActiveStatus != false && r.RoleName == GeneralClass.AUTHORITY && r.DeleteStatus != true
                                        select st).FirstOrDefault();


                    string signatur_n = "";
                    string nam_n = "";
                    //New Signature
                    if (newSignatory != null)
                    {
                        signatur_n = string.IsNullOrEmpty(newSignatory.SignaturePath) ? "" : newSignatory.SignaturePath;
                        nam_n = string.IsNullOrEmpty(newSignatory.SignatureName) ? "" : newSignatory.SignatureName;
                    }

                    var getIssueDateView = permit.date_issued < GeneralClass.DPR_ChangeDate ? "Old View" : "New View";
                    var coyNameSplit = _helpersController.LineSplitter(company.name, 35);
                    var _state = state.StateName.ToLower().Replace("state", "");
                    string stateOgetApplicationFilesct = _state.ToLower().Contains("abuj") ? " FCT" : " STATE";
                    var compAddress = address.address_1.ToLower().Replace(address.city.ToLower(), "").Replace(_state.ToLower(), "").Replace("state", "").Trim(',') + " " + address.city.ToLower().Replace(_state, "") + ", " + _state + stateOgetApplicationFilesct;
                    var statee = state.StateName.ToLower().Replace("state", "");
                    var fad = address.address_1.ToLower().Replace(statee.ToLower(), "").Trim().Trim(',').Trim().ToLower();
                    var city = "";
                    if (!string.IsNullOrEmpty(address.city))
                    {
                        city = address.city.ToLower().Trim(',').Trim() + ",";
                        fad = fad.Replace(city, "");
                    }
                    decimal ttp = 0;
                    var extrapayment = _context.ApplicationExtraPayments.Where(a => a.ApplicationId == application.id && a.Status == "Paid").ToList();
                    ttp = extrapayment.Sum(a => a.Amount);
                    string stateOgetApplicationFilesct1 = statee.ToLower().Contains("abuj") ? " FCT" : " STATE";
                    fad = fad.Replace(statee, "").Replace("state", "") + " " + city + " " + statee + stateOgetApplicationFilesct1;
                    var facAddSplit = _helpersController.LineSplitter(fad.Replace(", ,", ","), 45);

                    //verify: https://depot.dpr.gov.ng/Validate/s

                    var qrData = $"License No: {permit.permit_no}{Environment.NewLine}";
                    qrData += $"Depot Name: {facility.Name}{Environment.NewLine}";
                    qrData += $"Location: {city},{state}{Environment.NewLine}";
                    foreach (var item in products)
                    {
                        qrData += $"{item.Key}: {item.Value}ltrs{Environment.NewLine}";

                    }
                    qrData += $"Capacity: {products.Sum(a => a.Value).ToString("N0")}{Environment.NewLine}";

                    qrData += $"Date Issued: {permit.date_issued.ToLongDateString()}{Environment.NewLine}";
                    qrData += $"Expiry Date: {permit.date_expire.ToLongDateString()}{Environment.NewLine}";
                    qrData += $"https://depot.dpr.gov.ng/Validate/s/{permit.permit_no}";
                    string signatur = "";
                    string nam = "";
                    if (signature != null)
                    {
                        signatur = string.IsNullOrEmpty(signature.Signature) ? "" : signature.Signature;
                        nam = string.IsNullOrEmpty(signature.Name) ? "" : signature.Name;
                    }
                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var signPath = Path.Combine(up, "OldDepotStyle/Content/Signatures/DG.png");

                    PermitViewModel permitVM = new PermitViewModel()
                    {
                        PrintView = getIssueDateView,
                        CompanyIdCode = company.DPR_Id,
                        PermitFor = getCategory.name,
                        CategoryName = getCategory.name,
                        CompanyNameL1 = coyNameSplit[0],
                        CompanyNameL2 = coyNameSplit[1],
                        CoyAddL1 = compAddress.ToUpper(),
                        CoyAddL2 = "",
                        ExpiryDate = permit.date_expire,
                        FacilityAddress1 = facAddSplit[0].ToUpper(),
                        FacilityAddress2 = facAddSplit[1].ToUpper(),
                        Fee = application.fee_payable + ttp,
                        IssueDate = permit.date_issued,
                        LicenseTitle = getCategory.FriendlyName,
                        PermitNo = permit.permit_no,
                        Products = products.OrderByDescending(a => a.Key).ToList(),
                        CoyState = _state,
                        FacIdentitificationCode = facility.IdentificationCode,
                        Signature = signPath,
                        Signature_N = signatur_n,
                        QrCodeImg = _helpersController.GenerateQRCode(qrData)

                    };

                    _helpersController.LogMessages("Get Permit PDF for application with ref:" + application.reference, userEmail);

                    return permitVM;
                }
                // _helpersController.LogMessages("We couldn'nt find permit");
                return null;

            }
            catch (Exception x)
            {

                _helpersController.LogMessages($"Error in :getPdf :: {x.Message.ToString()}");
                return null;
            }
        }
        public PermitViewModel getPdf_Preview(int id)
        {

            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var application = _context.applications.Where(a => a.id == id).FirstOrDefault();

                if (application != null)
                {

                    var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                    var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    var state = _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                    var company = _context.companies.Where(c => c.id == application.company_id).FirstOrDefault();

                    var getCategory = _context.Categories.Where(ct => ct.id == application.category_id).FirstOrDefault();

                    List<KeyValuePair<string, double>> products = new List<KeyValuePair<string, double>>();

                    var tnks = (from at in _context.ApplicationTanks
                                join t in _context.Tanks on at.TankId equals t.Id
                                where at.ApplicationId == application.id
                                select at).ToList().GroupBy(x => x.ProductId).Select(c => c).ToList();

                    if (tnks.Count > 0)
                    {
                        foreach (var tank in tnks)
                        {
                            var prod = _context.Products.Where(x => x.Id == tank.Key).FirstOrDefault();
                            products.Add(new KeyValuePair<string, double>(prod.Name.ToString(), tank.Sum(a => a.Capacity)));
                        }
                    }
                    else
                    {
                        var tanks = _context.Tanks.AsEnumerable().Where(a => a.FacilityId == application.FacilityId && !a.Decommissioned).GroupBy(a => a.ProductId);

                        foreach (var tank in tanks)
                        {
                            var prod = _context.Products.Where(x => x.Id == tank.Key).FirstOrDefault();
                            products.Add(new KeyValuePair<string, double>(prod.Name, tank.Sum(a => Convert.ToDouble(a.MaxCapacity))));

                        }

                    }
                    var vAppProc = _context.MyDesk.Where(a => a.AppId == application.id).OrderByDescending(a => a.Sort).FirstOrDefault();
                    int approver = vAppProc.StaffID;
                    var me = _context.Staff.Where(a => a.StaffID == approver).FirstOrDefault();

                    var newSignatory = (from st in _context.Staff
                                        join r in _context.UserRoles on st.RoleID equals r.Role_id
                                        where st.DeleteStatus != true && st.ActiveStatus != false && r.RoleName == GeneralClass.AUTHORITY && r.DeleteStatus != true
                                        select st).FirstOrDefault();


                    string signatur_n = "";
                    string nam_n = "";
                    //New Signature
                    if (newSignatory != null)
                    {
                        signatur_n = string.IsNullOrEmpty(newSignatory.SignaturePath) ? "" : newSignatory.SignaturePath;
                        nam_n = string.IsNullOrEmpty(newSignatory.SignatureName) ? "" : newSignatory.SignatureName;
                    }

                    var getIssueDateView = application.status == GeneralClass.Approved ? "Old View" : "New View";
                    var coyNameSplit = _helpersController.LineSplitter(company.name, 35);
                    var _state = state.StateName.ToLower().Replace("state", "");
                    string stateOgetApplicationFilesct = _state.ToLower().Contains("abuj") ? " FCT" : " STATE";
                    var compAddress = address.address_1.ToLower().Replace(address.city.ToLower(), "").Replace(_state.ToLower(), "").Replace("state", "").Trim(',') + " " + address.city.ToLower().Replace(_state, "") + ", " + _state + stateOgetApplicationFilesct;
                    var statee = state.StateName.ToLower().Replace("state", "");
                    var fad = address.address_1.ToLower().Replace(statee.ToLower(), "").Trim().Trim(',').Trim().ToLower();
                    var city = "";
                    if (!string.IsNullOrEmpty(address.city))
                    {
                        city = address.city.ToLower().Trim(',').Trim() + ",";
                        fad = fad.Replace(city, "");
                    }
                    decimal ttp = 0;
                    var extrapayment = _context.ApplicationExtraPayments.Where(a => a.ApplicationId == application.id && a.Status == "Paid").ToList();
                    ttp = extrapayment.Sum(a => a.Amount);
                    string stateOgetApplicationFilesct1 = statee.ToLower().Contains("abuj") ? " FCT" : " STATE";
                    fad = fad.Replace(statee, "").Replace("state", "") + " " + city + " " + statee + stateOgetApplicationFilesct1;
                    var facAddSplit = _helpersController.LineSplitter(fad.Replace(", ,", ","), 45);

                    //verify: https://depot.dpr.gov.ng/Validate/s
                    string permitNO = _helpersController.GenerateNewPermitNo_Preview(application.id, "N", "001", DateTime.Now.Year.ToString(), application.PhaseId);
                    var qrData = $"License No: {permitNO}{Environment.NewLine}";
                    qrData += $"Depot Name: {facility.Name}{Environment.NewLine}";
                    qrData += $"Location: {city},{state}{Environment.NewLine}";
                    foreach (var item in products)
                    {
                        qrData += $"{item.Key}: {item.Value}ltrs{Environment.NewLine}";

                    }
                    qrData += $"Capacity: {products.Sum(a => a.Value).ToString("N0")}{Environment.NewLine}";

                    qrData += $"Date Issued: {DateTime.Now.ToLongDateString()}{Environment.NewLine}";
                    qrData += $"Expiry Date: {DateTime.Now.ToLongDateString()}{Environment.NewLine}";
                    qrData += $"https://depot.dpr.gov.ng/Validate/s/";

                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var signPath = Path.Combine(up, "OldDepotStyle/Content/Signatures/DG.png");

                    PermitViewModel permitVM = new PermitViewModel()
                    {
                        PrintView = getIssueDateView,
                        CompanyIdCode = company.DPR_Id,
                        PermitFor = getCategory.name,
                        CategoryName = getCategory.name,
                        CompanyNameL1 = coyNameSplit[0],
                        CompanyNameL2 = coyNameSplit[1],
                        CoyAddL1 = compAddress.ToUpper(),
                        CoyAddL2 = "",
                        ExpiryDate = DateTime.Now.AddMonths(3),
                        FacilityAddress1 = facAddSplit[0].ToUpper(),
                        FacilityAddress2 = facAddSplit[1].ToUpper(),
                        Fee = application.fee_payable + ttp,
                        IssueDate = DateTime.Now,
                        LicenseTitle = getCategory.FriendlyName,
                        PermitNo = permitNO,
                        Products = products.OrderByDescending(a => a.Key).ToList(),
                        CoyState = _state,
                        FacIdentitificationCode = facility.IdentificationCode,
                        Signature = signPath,
                        Signature_N = signatur_n,
                        QrCodeImg = _helpersController.GenerateQRCode(qrData)

                    };

                    return permitVM;
                }
                return null;

            }
            catch (Exception x)
            {

                _helpersController.LogMessages($"Error in :getPdf :: {x.Message.ToString()}");
                return null;
            }
        }

        public SuitabilityLetterModel getApprovalPdf(int id, string field = null)
        {

            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var application = _context.applications.Where(a => a.id == id).FirstOrDefault();

                if (application != null)
                {

                    var permit = _context.permits.Where(a => a.application_id == application.id).FirstOrDefault();
                    var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                    var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    var state = _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                    var comp = _context.companies.Where(c => c.id == permit.company_id).FirstOrDefault();
                    var compAdd = _context.addresses.Where(a => a.id == comp.registered_address_id).FirstOrDefault();
                    var compState = _context.States_UT.Where(x => x.State_id == 0).FirstOrDefault();

                    if (compAdd != null)
                    {
                        compState = _context.States_UT.Where(x => x.State_id == compAdd.StateId).FirstOrDefault();
                    }
                    var category = _context.Categories.Where(ct => ct.id == application.category_id).FirstOrDefault();
                    var Phase = _context.Phases.Where(p => p.id == application.PhaseId).FirstOrDefault();
                    var sch = _context.MeetingSchedules.Where(a => a.Accepted == true && a.Approved == true && a.ApplicationId == application.id).OrderByDescending(a => a.Date).FirstOrDefault();

                    var vAppProc = _context.MyDesk.Where(a => a.AppId == application.id).OrderByDescending(a => a.Sort).FirstOrDefault();
                    int approver = vAppProc.StaffID;
                    var me = _context.Staff.Where(a => a.StaffID == approver).FirstOrDefault();
                    var atnks = _context.ApplicationTanks.Where(a => a.FacilityId == application.FacilityId && a.ApplicationId == application.id).ToList();
                    var tanks = _context.Tanks.Where(a => a.FacilityId == application.FacilityId && !a.Decommissioned).ToList();


                    FieldOffices brch = new FieldOffices();
                    bool isZ;
                    //var brch = elpsCaller.GetBranch(myUB.BranchId);

                    brch = _context.FieldOffices.Where(x => x.FieldOffice_id == me.FieldOfficeID).FirstOrDefault();

                    string position = "";
                    if (Phase.ShortName.ToUpper() == "SI")
                    {
                        position = "AD";
                    }
                    else
                    {
                        position = "Director";
                    }


                    //Tank count figure and word
                    string tnkCountInWords = "";
                    int tankChangeCount = 0;
                    string DMType = "";
                    var signature = _context.Signatories.Where(a => a.Position == position && a.StartDate >= permit.date_issued && (a.EndDate == null || a.EndDate <= permit.date_issued)).FirstOrDefault();

                    var newSignatory = (from st in _context.Staff
                                        join r in _context.UserRoles on st.RoleID equals r.Role_id
                                        where st.DeleteStatus != true && st.ActiveStatus != false
                                        && ((Phase.IssueType.ToLower() == "approval" && r.RoleName == GeneralClass.ED) || (Phase.IssueType.ToLower() != "approval" && r.RoleName == GeneralClass.AUTHORITY))
                                        && r.DeleteStatus != true
                                        select st).FirstOrDefault();


                    string signatur = "";
                    string nam = "";
                    string signatur_n = "";
                    string nam_n = "";
                    if (signature != null)
                    {
                        signatur = string.IsNullOrEmpty(signature.Signature) ? "" : signature.Signature;
                        nam = string.IsNullOrEmpty(signature.Name) ? "" : signature.Name;
                    }

                    //New Signature
                    if (newSignatory != null)
                    {
                        signatur_n = string.IsNullOrEmpty(newSignatory.SignaturePath) ? "" : newSignatory.SignaturePath;
                        nam_n = string.IsNullOrEmpty(newSignatory.SignatureName) ? "" : newSignatory.SignatureName;
                    }

                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var signPath = Path.Combine(up, "OldDepotStyle/Content/Signatures/DG.png");

                    var phcode = new[] { "SI" };
                    var getIssueDateView = permit.date_issued < GeneralClass.DPR_ChangeDate ? "Old View" : "New View";

                    var model = new SuitabilityLetterModel
                    {

                        PrintView = getIssueDateView,
                        RefNo = permit.permit_no,
                        CompanyName = comp.name,
                        Address = compAdd.address_1.ToUpper(),
                        City = compAdd.city,
                        State = compState != null ? compState.StateName : "",
                        Date = _helpersController.GetDatePad(permit.date_issued.ToString("dd")) + permit.date_issued.ToString(" MMMM ") + permit.date_issued.ToString("yyy "),// CalendarHelper.ShortDate(permit.date_issued); //.ToLongDateString();
                        DateApproved = permit.date_issued,                                                                                                                     // CalendarHelper.ShortDate(permit.date_issued); //.ToLongDateString();
                                                                                                                                                                               //FacilityAddress = address.FacilityAddress(),
                        FacilityAddress = address.address_1,
                        DateApplied = application.CreatedAt != null ? application.CreatedAt.Value.ToLongDateString() : application.date_added.ToLongDateString(),
                        Signature = signPath.Trim(),
                        SignedBy = me.ToString(),
                        Signature_N = signatur_n.Trim(),
                        SignedBy_N = newSignatory.FirstName + " " + newSignatory.LastName.ToString(),
                        Office = brch.OfficeName.ToUpper(),
                        FacStateName = state.StateName,
                        IsZopscon = false,
                        PhaseShortName = Phase.ShortName.ToUpper()
                    };
                    var body = "";

                    _helpersController.LogMessages(model == null ? "Returning empty model" : "USERRR: (" + Phase.ShortName + ") " + model.RefNo);
                    string phaseShortName = "";
                    if (field != null)
                    {
                        model.IsFieldView = "yes";
                    }
                    #region check application/facility tanks to determine template to be used
                    string ATK = ""; string Bit = ""; string PMS = "";

                    if (atnks.Count() > 0)
                    {
                        foreach (var at in atnks)
                        {
                            var ProductName = _context.Products.Where(p => p.Id == at.ProductId).FirstOrDefault()?.Name;
                            if (ProductName != null)
                            {

                                if (ProductName.ToLower().Contains("bitumen"))
                                {
                                    Bit = "Yes";
                                }
                                if (ProductName.ToLower().Contains("atk"))
                                {
                                    ATK = "Yes";
                                }
                                if (ProductName.ToLower().Contains("pms"))
                                {
                                    PMS = "Yes";
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var at in tanks)
                        {
                            var ProductName = _context.Products.Where(p => p.Id == at.ProductId).FirstOrDefault()?.Name;
                            if (ProductName != null)
                            {
                                if (ProductName.ToLower().Contains("bitumen"))
                                {
                                    Bit = "Yes";
                                }
                                if (ProductName.ToLower().Contains("atk"))
                                {
                                    ATK = "Yes";
                                }
                                if (ProductName.ToLower().Contains("pms"))
                                {
                                    PMS = "Yes";
                                }
                            }
                        }
                    }


                    #endregion
                    switch (Phase.ShortName.ToUpper())
                    {
                        case "SI":
                            {
                                phaseShortName = "SI";
                                break;
                            }
                        case "REG":
                            {
                                phaseShortName = "REG";
                                break;
                            }
                        case "ATC":
                        case "CWA":
                            {

                                //Now check if products contains ATK, Bitumen or Both

                                phaseShortName = "ATC";

                                break;
                            }

                        case "NDT":
                        case "RC":
                            {
                                phaseShortName = "NDT";
                                break;
                            }
                        case "DM":
                            {
                                phaseShortName = "DM";

                                break;
                            }

                        case "UWA":
                            {
                                phaseShortName = "DM";
                                break;
                            }
                        case "SAP":
                            {
                                phaseShortName = "SAP";
                                break;
                            }

                    }

                    var file = Path.Combine(up, "Templates/ApprovalTemplate/" + phaseShortName + ".txt");
                    using (var sr = new StreamReader(file.Trim()))
                    {

                        body = sr.ReadToEnd();
                    }

                    string stn = "";
                    var appOffice = _helpersController.GetApplicationOffice(application.id).FirstOrDefault();
                    //string zonalOrFieldID = appOffice.ZonalOrField.ToString()=="FD"? "Field| "+ appOffice.OfficeName : "Zonal| "+ appOffice.OfficeName;
                    string zonalOrFieldID = appOffice.OfficeName;
                    if (zonalOrFieldID == null)
                    {
                        return null;
                    }

                    else
                    {
                        stn = appOffice.OfficeName;
                        model.FacilityZonalOrFeildOffice = zonalOrFieldID;
                    }
                    switch (Phase.ShortName.ToUpper())
                    {
                        case "SI":
                            {
                                // only one data: DateApplied
                                var suit = _context.SuitabilityInspections.Where(a => a.ApplicationId == application.id).FirstOrDefault();
                                if (suit != null)
                                {
                                    model.SizeOfLand = suit.SizeOfLand;
                                    model.ZonalOrFeild = stn;
                                }

                                model.Body = string.Format(body, model.DateApplied, stn);
                                break;
                            }
                        case "REG":
                            {
                                // only one data: DateApplied
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                if (atnks.Count > 0)
                                {
                                    foreach (var item in atnks)
                                    {
                                        var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity.ToString("N0")}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }
                                else
                                {
                                    foreach (var item in tanks)
                                    {

                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }

                                tnks = sb.ToString();

                                model.Body = string.Format(body, model.FacilityAddress, model.FacilityZonalOrFeildOffice, model.DateApplied, sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString(), tnks);
                                break;
                            }

                        case "ATC":
                        case "CWA":
                            {
                                // only one data: DateApplied
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                if (atnks.Count > 0)
                                {
                                    foreach (var item in atnks)
                                    {
                                        var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity.ToString("N0")}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }
                                else
                                {
                                    foreach (var item in tanks)
                                    {

                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }

                                tnks = sb.ToString();

                                var letterdate = sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString();
                                var ATCLet = new DM_ATCLetter();
                                ATCLet.FacilityAddress = model.FacilityAddress;
                                ATCLet.DateApplied = model.DateApplied;
                                ATCLet.ScheduleDate = letterdate;
                                ATCLet.DateApproved = _context.permits.Where(x => x.application_id == application.id).FirstOrDefault().date_issued;
                                ATCLet.StateName = state.StateName == "Delta State" ? "Warri" : state.StateName;
                                ATCLet.TanksText = tnks;
                                
                                var getbody = _helpersController.ATCLetter(ATCLet);

                                model.Body = string.Format(getbody);
                                break;
                            }
                        case "NDT":
                        case "RC":
                            {
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                //Calibration and re-calibration
                                if (Phase.ShortName.ToUpper() == "NDT")
                                {


                                    if (atnks.Count > 0)
                                    {
                                        foreach (var item in atnks)
                                        {
                                            var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                            var product = _context.Products.Where(p => p.Id == it.ProductId).FirstOrDefault();

                                            var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{product.Name}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity}</td><td>NA</td></tr>";
                                            sb.Append(td);
                                            c++;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item in tanks)
                                        {
                                            var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.ProductName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>NA</td></tr>";
                                            sb.Append(td);
                                            c++;
                                        }
                                    }

                                    tnks = sb.ToString();
                                }
                                else
                                {
                                    var tnksm = _context.FacilityModifications.Where(a => a.ApplicationId == application.id).ToList();
                                    foreach (var item in tnksm)
                                    {
                                        var td = "";
                                        var aptnk = _context.ApplicationTanks.Where(a => a.FacilityId == item.FacilityId && a.ApplicationId == item.FacilityId).ToList();
                                        aptnk.ForEach(atnk =>
                                        {
                                            var it = _context.Tanks.Where(a => a.Id == atnk.TankId).FirstOrDefault();
                                            var product = _context.Products.Where(p => p.Id == atnk.ProductId).FirstOrDefault();
                                            td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{it.Name}</td><td>{product.Name}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{it.MaxCapacity}</td><td>{it.ModifyType}</td></tr>";

                                        });
                                        sb.Append(td);
                                        c++;
                                    }
                                    tnks = sb.ToString();
                                }
                                model.Body = string.Format(body, model.FacilityAddress, model.DateApplied, sch == null ? model.DateApplied : sch.ApprovedDate.GetValueOrDefault().ToLongDateString(), model.CompanyName, tnks, stn);//, state.StateName

                                break;

                            }
                        case "DM":
                        case "UWA":
                            {
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                int cnt = 0;
                                string td = "";
                                string prevProd = "";
                                string activ = "Active";
                                var facMod = _context.FacilityModifications.Where(a => a.ApplicationId == application.id).FirstOrDefault();

                                if (facMod != null)
                                {

                                    prevProd = facMod.PrevProduct;
                                    if (facMod.Type == "Conversion")
                                    {
                                        phaseShortName = "DMC";

                                    }
                                    if (facMod.Type.Contains("Inclusion"))
                                    {
                                        phaseShortName = "DMI";
                                    }
                                    DMType = facMod.Type;
                                    foreach (var item in tanks.OrderBy(a => a.Name))
                                    {
                                        activ = "Active"; 
                                        var productName = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault().Name;

                                        var tm = atnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                        if (tm != null)
                                        {
                                            tankChangeCount += 1;
                                            if (facMod.Type == "ATC" || facMod.Type.Contains("Inclusion"))
                                            {
                                                activ = "";

                                            }
                                            if (facMod.Type.Contains("Decommission"))
                                            {
                                                activ = "Inactive";
                                            }
                                            if (facMod.Type == "Conversion")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.MaxCapacity}</td><td>{activ}(Converted)</td></tr>";

                                            }
                                            else if (facMod.Type == "Inclusion")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.MaxCapacity}</td><td>{activ}({facMod.Type})</td></tr>";

                                            }
                                            else
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}({facMod.Type})</td></tr>";
                                            }
                                        }
                                        else
                                        {
                                            if (phaseShortName == "DMC")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>";


                                            }
                                            else
                                            {
                                                td =phaseShortName =="DM" ?  $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>"
                                                                     : $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>";

                                            }
                                        }

                                        sb.Append(td);
                                        c++;
                                    }


                                }
                                else
                                {
                                    var modificationTnks = _context.FacilityTankModifications.Where(a => a.ApplicationId == application.id).ToList();
                                    if (modificationTnks.Count > 0)
                                    {
                                        var _tm = modificationTnks.Where(a => a.Type == "Convert" || a.Type.Contains("Inclusion")).FirstOrDefault();
                                        if (_tm != null)
                                        {
                                            phaseShortName = "DMC";
                                        }
                                        foreach (var item in tanks.OrderBy(a => a.Name))
                                        {
                                            var product = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();

                                            var tm = modificationTnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                            if (tm != null)
                                            {
                                                if (facMod.Type == "ATC" || facMod.Type.Contains("Inclusion"))
                                                {
                                                    activ = "";
                                                }
                                                if (tm.Type.Contains("Decommission"))
                                                {
                                                    activ = "Inactive";
                                                }
                                                else if (tm.Type.Contains("Conver"))
                                                {
                                                    tm.Type = "Converted";
                                                    var tks = _context.Tanks.Where(a => a.Id == tm.TankId).FirstOrDefault();

                                                    prevProd += string.IsNullOrEmpty(prevProd) ? tm.PrevProduct + " to " + product.Name : " and " + tm.PrevProduct + " to " + product.Name;

                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.MaxCapacity}</td><td>{activ}({tm.Type})</td></tr>";

                                                }
                                                else
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}({tm.ModifyType})</td></tr>";

                                                }
                                            }
                                            else
                                            {
                                                if (phaseShortName == "DMC")
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>


                                                }
                                                else
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>


                                                }
                                            }
                                            sb.Append(td);
                                            c++;
                                        }

                                    }
                                    else
                                    {
                                        foreach (var item in tanks)
                                        {
                                            var product = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();
                                            var tm = atnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                            if (tm != null)
                                            {

                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>

                                            }
                                            else
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>

                                            }
                                            sb.Append(td);
                                            c++;
                                        }
                                    }
                                }
                                tnks = sb.ToString();
                                double fontSize = 1.2;
                                if (atnks.Count <= 3)
                                {
                                    fontSize = 1.4;
                                }

                                var pth = Path.Combine(up, "Templates/ApprovalTemplate");

                                using (var sr = new StreamReader(pth + "/" + phaseShortName + ".txt"))
                                {

                                    body = sr.ReadToEnd();
                                }
                                tnkCountInWords = GeneralClass.NumWords(Convert.ToDouble(tankChangeCount));

                                if (phaseShortName == "DMC")
                                {

                                    model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, stn, model.CompanyName, prevProd, tnks, application.date_added.Year + 1);//, state.StateName

                                }
                                else if (phaseShortName == "DMI")
                                {
                                    model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, model.CompanyName, tnks, fontSize, phaseShortName.Equals("DMI") ? $",{address?.city}, {state?.StateName}".ToUpper() : "", tankChangeCount, tnkCountInWords, state.StateName, application.date_added.Year + 1);

                                }
                                else //DM Letter
                                {
                                    var letterdate = sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString();
                                    var DMLet = new DM_ATCLetter();
                                    DMLet.CompanyName = model.CompanyName;
                                    DMLet.FacilityAddress = model.FacilityAddress;
                                    DMLet.DateApplied = model.DateApplied;
                                    DMLet.ScheduleDate = letterdate;
                                    DMLet.DateApproved = _context.permits.Where(x=> x.application_id == application.id).FirstOrDefault().date_issued;
                                    DMLet.StateName = state.StateName == "Delta State" ? "Warri" : state.StateName;
                                    DMLet.TanksText = tnks;
                                    DMLet.ModifyType = DMType;
                                    var getbody = _helpersController.DMLetter(DMLet, tankChangeCount.ToString(), tnkCountInWords);
                                    model.Body = string.Format(getbody);
                                    //model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, "Monday, January 25, 2021", model.CompanyName, tnks, fontSize, phaseShortName.Equals("DM") ? $",{address?.city}, {state?.StateName}".ToUpper() : "", tanks.Count, tnkCountInWords);//, state.StateName
                                }
                                break;
                                //break;
                            }
                        case "SAP":
                            {
                                model.Body = string.Format(body, model.FacilityAddress);
                                break;
                            }
                        default:
                            break;
                    }

                    _helpersController.LogMessages(phaseShortName + " approval generated for application with ref:" + application.reference, userEmail);
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {

                _helpersController.LogMessages($"Error while loading Approval view: {ex.Message.ToString()}");
                return null;
            }
        }
        public SuitabilityLetterModel getApprovalPdf_Preview(int id)
        {

            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var application = _context.applications.Where(a => a.id == id).FirstOrDefault();

                if (application != null)
                {

                    //var permit = _context.permits.Where(a => a.application_id == application.id).FirstOrDefault();
                    var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                    var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    var state = _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                    var comp = _context.companies.Where(c => c.id == application.company_id).FirstOrDefault();
                    var compAdd = _context.addresses.Where(a => a.id == comp.registered_address_id).FirstOrDefault();
                    var compState = _context.States_UT.Where(x => x.State_id == 0).FirstOrDefault();

                    if (compAdd != null)
                    {
                        compState = _context.States_UT.Where(x => x.State_id == compAdd.StateId).FirstOrDefault();
                    }
                    var category = _context.Categories.Where(ct => ct.id == application.category_id).FirstOrDefault();
                    var Phase = _context.Phases.Where(p => p.id == application.PhaseId).FirstOrDefault();
                    var sch = _context.MeetingSchedules.Where(a => a.Accepted == true && a.Approved == true && a.ApplicationId == application.id).OrderByDescending(a => a.Date).FirstOrDefault();

                    var atnks = _context.ApplicationTanks.Where(a => a.FacilityId == application.FacilityId && a.ApplicationId == application.id).ToList();
                    var tanks = _context.Tanks.Where(a => a.FacilityId == application.FacilityId && !a.Decommissioned).ToList();

                    var brch = _helpersController.GetApplicationOffice(application.id).FirstOrDefault();
                    string position = "";
                    if (Phase.ShortName.ToUpper() == "SI")
                    {
                        position = "AD";
                    }
                    else
                    {
                        position = "Director";
                    }


                    //Tank count figure and word
                    string tnkCountInWords = "";
                    int tankChangeCount = 0;
                    string DMType = "";


                    var newSignatory = (from st in _context.Staff
                                        join r in _context.UserRoles on st.RoleID equals r.Role_id
                                        where st.DeleteStatus != true && st.ActiveStatus != false
                                        && ((Phase.IssueType.ToLower() == "approval" && r.RoleName == GeneralClass.ED) || (Phase.IssueType.ToLower() != "approval" && r.RoleName == GeneralClass.AUTHORITY))
                                        && r.DeleteStatus != true
                                        select st).FirstOrDefault();


                    string signatur_n = "";
                    string nam_n = "";
                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var signPath = Path.Combine(up, "OldDepotStyle/Content/Signatures/DG.png");

                    //New Signature
                    if (newSignatory != null)
                    {
                        signatur_n = string.IsNullOrEmpty(newSignatory.SignaturePath) ? "" : newSignatory.SignaturePath;
                        nam_n = string.IsNullOrEmpty(newSignatory.SignatureName) ? "" : newSignatory.SignatureName;
                    }


                    var getIssueDateView = application.status == GeneralClass.Approved ? "Old View" : "New View";

                    var model = new SuitabilityLetterModel
                    {

                        PrintView = getIssueDateView,
                        RefNo = _helpersController.GenerateNewPermitNo_Preview(application.id, "N", "001", DateTime.Now.Year.ToString(), application.PhaseId),
                        CompanyName = comp.name,
                        Address = compAdd.address_1.ToUpper(),
                        City = compAdd.city,
                        State = compState != null ? compState.StateName : "",
                        Date = _helpersController.GetDatePad(DateTime.Now.ToString("dd")) + DateTime.Now.ToString(" MMMM ") + DateTime.Now.ToString("yyy "),
                        DateApproved = DateTime.Now,

                        FacilityAddress = address.address_1,
                        DateApplied = application.CreatedAt.Value.ToLongDateString(),
                        Signature = signPath.Trim(),
                        SignedBy = userEmail,
                        Signature_N = signatur_n.Trim(),
                        SignedBy_N = newSignatory.FirstName + " " + newSignatory.LastName.ToString(),
                        Office = brch.OfficeName.ToUpper(),
                        FacStateName = state.StateName,
                        IsZopscon = false,
                        PhaseShortName = Phase.ShortName.ToUpper()
                    };
                    var body = "";

                    _helpersController.LogMessages(model == null ? "Returning empty model" : "USERRR: (" + Phase.ShortName + ") " + model.RefNo);
                    string phaseShortName = "";

                    #region check application/facility tanks to determine template to be used
                    string ATK = ""; string Bit = ""; string PMS = "";

                    if (atnks.Count() > 0)
                    {
                        foreach (var at in atnks)
                        {
                            var ProductName = _context.Products.Where(p => p.Id == at.ProductId).FirstOrDefault()?.Name;

                            if (ProductName.ToLower().Contains("bitumen"))
                            {
                                Bit = "Yes";
                            }
                            if (ProductName.ToLower().Contains("atk"))
                            {
                                ATK = "Yes";
                            }
                            if (ProductName.ToLower().Contains("pms"))
                            {
                                PMS = "Yes";
                            }
                        }
                    }
                    else
                    {
                        foreach (var at in tanks)
                        {
                            var ProductName = _context.Products.Where(p => p.Id == at.ProductId).FirstOrDefault()?.Name;

                            if (ProductName.ToLower().Contains("bitumen"))
                            {
                                Bit = "Yes";
                            }
                            if (ProductName.ToLower().Contains("atk"))
                            {
                                ATK = "Yes";
                            }
                            if (ProductName.ToLower().Contains("pms"))
                            {
                                PMS = "Yes";
                            }
                        }
                    }


                    #endregion
                    switch (Phase.ShortName.ToUpper())
                    {
                        case "SI":
                            {
                                phaseShortName = "SI";
                                break;
                            }
                        case "REG":
                            {
                                phaseShortName = "REG";
                                break;
                            }
                        case "ATC":
                        case "CWA":
                            {

                                //Now check if products contains ATK, Bitumen or Both

                                phaseShortName = "ATC";

                                break;
                            }

                        case "NDT":
                        case "RC":
                            {
                                phaseShortName = "NDT";
                                break;
                            }
                        case "DM":
                            {
                                phaseShortName = "DM";

                                break;
                            }

                        case "UWA":
                            {
                                phaseShortName = "DM";
                                break;
                            }
                        case "SAP":
                            {
                                phaseShortName = "SAP";
                                break;
                            }

                    }

                    var file = Path.Combine(up, "Templates/ApprovalTemplate/" + phaseShortName + ".txt");
                    using (var sr = new StreamReader(file.Trim()))
                    {

                        body = sr.ReadToEnd();
                    }

                    string stn = "";
                    var appOffice = _helpersController.GetApplicationOffice(application.id).FirstOrDefault();
                    string zonalOrFieldID = appOffice.OfficeName;
                    if (zonalOrFieldID == null)
                    {
                        return null;
                    }

                    else
                    {
                        stn = appOffice.OfficeName;
                        model.FacilityZonalOrFeildOffice = zonalOrFieldID;
                    }
                    switch (Phase.ShortName.ToUpper())
                    {
                        case "SI":
                            {
                                // only one data: DateApplied
                                var suit = _context.SuitabilityInspections.Where(a => a.ApplicationId == application.id).FirstOrDefault();
                                if (suit != null)
                                {
                                    model.SizeOfLand = suit.SizeOfLand;
                                    model.ZonalOrFeild = stn;
                                }

                                model.Body = string.Format(body, model.DateApplied, stn);
                                break;
                            }
                        case "REG":
                            {
                                // only one data: DateApplied
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                if (atnks.Count > 0)
                                {
                                    foreach (var item in atnks)
                                    {
                                        var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity.ToString("N0")}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }
                                else
                                {
                                    foreach (var item in tanks)
                                    {

                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }

                                tnks = sb.ToString();

                                model.Body = string.Format(body, model.FacilityAddress, model.FacilityZonalOrFeildOffice, model.DateApplied, sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString(), tnks);
                                break;
                            }

                        case "ATC":
                        case "CWA":
                            {
                                // only one data: DateApplied
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                if (atnks.Count > 0)
                                {
                                    foreach (var item in atnks)
                                    {
                                        var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity.ToString("N0")}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }
                                else
                                {
                                    foreach (var item in tanks)
                                    {

                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }

                                tnks = sb.ToString();

                                var letterdate = sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString();
                                var ATCLet = new DM_ATCLetter();
                                ATCLet.FacilityAddress = model.FacilityAddress;
                                ATCLet.DateApplied = model.DateApplied;
                                ATCLet.ScheduleDate = letterdate;
                                ATCLet.DateApproved = _context.permits.Where(x => x.application_id == application.id).FirstOrDefault().date_issued;
                                ATCLet.StateName = state.StateName == "Delta State" ? "Warri" : state.StateName;
                                ATCLet.TanksText = tnks;

                                var getbody = _helpersController.ATCLetter(ATCLet);

                                model.Body = string.Format(getbody);
                                break;
                            }
                        case "NDT":
                        case "RC":
                            {
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                //Calibration and re-calibration
                                if (Phase.ShortName.ToUpper() == "NDT")
                                {


                                    if (atnks.Count > 0)
                                    {
                                        foreach (var item in atnks)
                                        {
                                            var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                            var product = _context.Products.Where(p => p.Id == it.ProductId).FirstOrDefault();

                                            var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{product.Name}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity}</td><td>NA</td></tr>";
                                            sb.Append(td);
                                            c++;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item in tanks)
                                        {
                                            var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.ProductName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>NA</td></tr>";
                                            sb.Append(td);
                                            c++;
                                        }
                                    }

                                    tnks = sb.ToString();
                                }
                                else
                                {
                                    var tnksm = _context.FacilityModifications.Where(a => a.ApplicationId == application.id).ToList();
                                    foreach (var item in tnksm)
                                    {
                                        var td = "";
                                        var aptnk = _context.ApplicationTanks.Where(a => a.FacilityId == item.FacilityId && a.ApplicationId == item.FacilityId).ToList();
                                        aptnk.ForEach(atnk =>
                                        {
                                            var it = _context.Tanks.Where(a => a.Id == atnk.TankId).FirstOrDefault();
                                            var product = _context.Products.Where(p => p.Id == atnk.ProductId).FirstOrDefault();
                                            td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{it.Name}</td><td>{product.Name}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{it.MaxCapacity}</td><td>{it.ModifyType}</td></tr>";

                                        });
                                        sb.Append(td);
                                        c++;
                                    }
                                    tnks = sb.ToString();
                                }
                                model.Body = string.Format(body, model.FacilityAddress, model.DateApplied, sch == null ? model.DateApplied : sch.ApprovedDate.GetValueOrDefault().ToLongDateString(), model.CompanyName, tnks, stn);//, state.StateName

                                break;

                            }
                        case "DM":
                        case "UWA":
                            {
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                int cnt = 0;
                                string td = "";
                                string prevProd = "";
                                string activ = "Active";
                                var facMod = _context.FacilityModifications.Where(a => a.ApplicationId == application.id).FirstOrDefault();

                                if (facMod != null)
                                {

                                    prevProd = facMod.PrevProduct;
                                    if (facMod.Type == "Conversion")
                                    {
                                        phaseShortName = "DMC";

                                    }
                                    if (facMod.Type.Contains("Inclusion"))
                                    {
                                        phaseShortName = "DMI";
                                    }
                                    DMType = facMod.Type;

                                    foreach (var item in tanks.OrderBy(a => a.Name))
                                    {
                                        activ = "Active";
                                        var productName = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault().Name;

                                        var tm = atnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                        if (tm != null)
                                        {
                                            tankChangeCount += 1;
                                            if (facMod.Type == "ATC" || facMod.Type.Contains("Inclusion"))
                                            {
                                                activ = "";

                                            }
                                            if (facMod.Type.Contains("Decommission"))
                                            {
                                                activ = "Inactive";
                                            }
                                            else if (facMod.Type == "Conversion")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.MaxCapacity}</td><td>{activ}(Converted)</td></tr>";

                                            }
                                            else if (facMod.Type == "Inclusion")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.MaxCapacity}</td><td>{activ}({facMod.Type})</td></tr>";

                                            }
                                            else
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}({facMod.Type})</td></tr>";
                                            }
                                        }
                                        else
                                        {
                                            if (phaseShortName == "DMC")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>";


                                            }
                                            else
                                            {
                                                td = phaseShortName == "DM" ? $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}({facMod.Type})</td></tr>"
                                                                     : $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>";

                                            }
                                        }

                                        sb.Append(td);
                                        c++;
                                    }


                                }
                                else
                                {
                                    var modificationTnks = _context.FacilityTankModifications.Where(a => a.ApplicationId == application.id).ToList();
                                    if (modificationTnks.Count > 0)
                                    {
                                        var _tm = modificationTnks.Where(a => a.Type == "Convert" || a.Type.Contains("Inclusion")).FirstOrDefault();
                                        if (_tm != null)
                                        {
                                            phaseShortName = "DMC";
                                        }
                                        foreach (var item in tanks.OrderBy(a => a.Name))
                                        {
                                            var product = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();

                                            var tm = modificationTnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                            if (tm != null)
                                            {
                                                if (facMod.Type == "ATC" || facMod.Type.Contains("Inclusion"))
                                                {
                                                    activ = "";
                                                }
                                                if (tm.Type.Contains("Decommission"))
                                                {
                                                    activ = "Inactive";
                                                }
                                                else if (tm.Type.Contains("Conver"))
                                                {
                                                    tm.Type = "Converted";
                                                    var tks = _context.Tanks.Where(a => a.Id == tm.TankId).FirstOrDefault();

                                                    prevProd += string.IsNullOrEmpty(prevProd) ? tm.PrevProduct + " to " + product.Name : " and " + tm.PrevProduct + " to " + product.Name;

                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.MaxCapacity}</td><td>{activ}({tm.Type})</td></tr>";

                                                }
                                                else
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}({tm.ModifyType})</td></tr>";

                                                }
                                            }
                                            else
                                            {
                                                if (phaseShortName == "DMC")
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>


                                                }
                                                else
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>


                                                }
                                            }
                                            sb.Append(td);
                                            c++;
                                        }

                                    }
                                    else
                                    {
                                        foreach (var item in tanks)
                                        {
                                            var product = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();
                                            var tm = atnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                            if (tm != null)
                                            {

                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>

                                            }
                                            else
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>

                                            }
                                            sb.Append(td);
                                            c++;
                                        }
                                    }
                                }
                                tnks = sb.ToString();
                                double fontSize = 1.2;
                                if (atnks.Count <= 3)
                                {
                                    fontSize = 1.4;
                                }

                                var pth = Path.Combine(up, "Templates/ApprovalTemplate");

                                using (var sr = new StreamReader(pth + "/" + phaseShortName + ".txt"))
                                {

                                    body = sr.ReadToEnd();
                                }
                                tnkCountInWords = GeneralClass.NumWords(Convert.ToDouble(tankChangeCount));

                                if (phaseShortName == "DMC")
                                {

                                    model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, stn, model.CompanyName, prevProd, tnks, application.date_added.Year + 1);//, state.StateName

                                }
                                else if (phaseShortName == "DMI")
                                {
                                    model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, model.CompanyName, tnks, fontSize, phaseShortName.Equals("DMI") ? $",{address?.city}, {state?.StateName}".ToUpper() : "", tankChangeCount, tnkCountInWords, state.StateName, application.date_added.Year + 1);

                                }
                                else //DM Letter
                                {
                                    var letterdate = sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString();
                                    var DMLet = new DM_ATCLetter();
                                    DMLet.CompanyName = model.CompanyName;
                                    DMLet.FacilityAddress = model.FacilityAddress;
                                    DMLet.DateApplied = model.DateApplied;
                                    DMLet.ScheduleDate = letterdate;
                                    DMLet.DateApproved = _context.permits.Where(x => x.application_id == application.id).FirstOrDefault().date_issued;
                                    DMLet.StateName = state.StateName == "Delta State" ? "Warri" : state.StateName;
                                    DMLet.TanksText = tnks;
                                    DMLet.ModifyType = DMType;
                                    var getbody = _helpersController.DMLetter(DMLet, tankChangeCount.ToString(), tnkCountInWords);


                                    model.Body = string.Format(getbody);
                                    //model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, "Monday, January 25, 2021", model.CompanyName, tnks, fontSize, phaseShortName.Equals("DM") ? $",{address?.city}, {state?.StateName}".ToUpper() : "", tanks.Count, tnkCountInWords);//, state.StateName
                                }
                                break;
                                //break;
                            }
                        case "SAP":
                            {
                                model.Body = string.Format(body, model.FacilityAddress);
                                break;
                            }
                        default:
                            break;
                    }

                    _helpersController.LogMessages(phaseShortName + " license generated for application with ref:" + application.reference, userEmail);
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {

                _helpersController.LogMessages($"Error while loading License view: {ex.Message.ToString()}");
                return null;
            }
        }



        public IActionResult CompanyApplications(string id)
        {
            int compId = generalClass.DecryptIDs(id);
            var comp = _context.companies.Where(a => a.id == compId).FirstOrDefault();

            var myApp = _helpersController.ApplicationDetails();
            var apps = myApp.Where(x => x.Company_Id == compId);

            ViewBag.Title = "Applications For " + comp.name;
            return View(apps);
        }
        public IActionResult CompanyLicenses(string id)
        {
            int compId = generalClass.DecryptIDs(id);
            var company = _context.companies.Where(a => a.id == compId).FirstOrDefault();

            List<permits> permits = new List<permits>();
            if (compId > 0)
            {

                var perModel = (from p in _context.permits
                                join app in _context.applications on p.application_id equals app.id
                                join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                                join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                                join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                                join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                                where app.DeleteStatus != true && c.id == compId
                                select new permitsModel
                                {
                                    Id = p.id,
                                    Application_Id = app.id,
                                    Reference = app.reference,
                                    Date_Issued = p.date_issued,
                                    Date_Expire = p.date_expire,
                                    CheckApprovalType = (phs.ShortName == "NDT" || phs.ShortName == "LTO" || phs.ShortName == "LR" || phs.ShortName == "TO" || phs.ShortName == "RC") ? "Yes" : "",
                                    ModifyType = _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault() != null ? _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault().Type : "",
                                    Category_id = cat.id,
                                    Is_Renewed = p.is_renewed,
                                    Permit_No = p.permit_no,
                                    CategoryName = cat.name,
                                    PhaseName = phs.name,
                                    CompanyName = c.name,
                                    FacilityName = fac.Name,
                                    OrderId = app.reference.ToString()
                                }).OrderByDescending(a => a.Date_Issued).ToList();

                ViewBag.Title = "All Permits/Licenses for \"" + company.name + "\"";

                return View(perModel);
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, company ID was not in correct format. Kindly contact support.") });
            }

        }


        [Authorize(Policy = "AdminRoles")]
        public IActionResult ReroutApp(int id)
        {
            if (CreateProcessingRules(id))
            {
                return Content("Successful");
            }
            else
            {
                return Content("Failiure");
            }
        }
        //[Authorize(Roles = "Admin,Support,ITAdmin")]
        public IActionResult UpdateRoutApp(int? id)
        {
            var startdate = new DateTime(2019, 7, 1);
            int count = 0;
            if (id != null)
            {

                var apps = _context.applications.Where(a => a.DeleteStatus != true && (a.status == GeneralClass.Processing || a.status == GeneralClass.Rejected) && a.id == id).FirstOrDefault();
                if (CreateProcessingRules(apps.id))
                {
                    count++;
                }

            }
            if (count > 0)
            {
                return Content("Done");
            }
            else
            {
                return Content("Failiure");
            }
        }

        #region create processing rule/method

        public bool CreateProcessingRules(int Id)
        {

            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var getApp = _context.applications.Where(x => x.id == Id && x.DeleteStatus != true).FirstOrDefault();

            try
            {
                if (getApp == null)
                    return false;

                #region Application Processing

                //check if app is currently on staff desk
                int stateID = _helpersController.GetApplicationState(getApp.id);
                // getting application work process flow
                List<WorkProccess> process = _helpersController.GetAppProcess(getApp.PhaseId, getApp.category_id, 0, 0);
                if (process.Count <= 0)
                {
                    string err = "Something went wrong while trying to get work process for your application. Please try again or contact Support.";
                    return false;
                }
                var fm = _context.FacilityModifications.Where(a => a.ApplicationId == getApp.id).FirstOrDefault();
                string modType = null;
                if (fm != null)
                {
                    if (fm.Type.Contains("clusion") || fm.Type.Contains("version"))
                        modType = fm.Type;
                }
                int AppDropStaffID = _helpersController.ApplicationDropStaff(getApp.id, getApp.PhaseId, getApp.category_id, stateID, 0, modType);

                if (AppDropStaffID <= 0)
                {
                    string err = "Something went wrong while trying to send your application to a staff for processing. Please try again or contact Support.";
                    return false;

                }
                else
                {

                    //add to old table
                    var staffUN = _context.Staff.Where(x => x.StaffID == AppDropStaffID).FirstOrDefault();

                    MyDesk drop = new MyDesk()
                    {
                        ProcessID = process.FirstOrDefault().ProccessID,
                        Sort = process.FirstOrDefault().Sort,
                        AppId = getApp.id,
                        StaffID = AppDropStaffID,
                        FromStaffID = 0,
                        HasWork = false,
                        HasPushed = false,
                        CreatedAt = DateTime.Now
                    };

                    // dropping application on staff desk
                    _context.MyDesk.Add(drop);

                    getApp.current_desk = AppDropStaffID;
                    int appDrop = _context.SaveChanges();

                    if (appDrop > 0)
                    {
                        return true;
                    }
                }


                #endregion
            }
            catch (WebException wex)
            {
                _helpersController.LogMessages(wex.ToString());
                var statusCode = ((HttpWebResponse)wex.Response).StatusCode;
                return false;
            }
            return false;

        }
        #endregion
        #endregion
    }
}


//public bool CreateProcessingRules(int Id)
//{

//    var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
//    var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
//    var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
//    int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

//    applications app = _context.applications.Where(C => C.id == Id).FirstOrDefault();
//    #region
//    try
//    {
//        if (app == null)
//            return false;

//        #region Application Processing

//        //check if processing rule has been created for application phase
//        List<CategoryRoutings> appCategoryRouting = _context.CategoryRoutings.Where(C => C.category_id == app.category_id && !C.Deleted).ToList();

//        List<PhaseRoutings> appPhaseRouting = _context.PhaseRoutings.Where(C => C.PhaseId == app.PhaseId && !C.Deleted).ToList();
//        int stff = 0;

//        if (appPhaseRouting == null)
//        {
//            return false;
//        }
//        //check if app is currently on staff desk
//        application_Processings existingProcess = _context.application_Processings.Where(C => C.ApplicationId == app.id && C.Processed != true).FirstOrDefault();
//        application_desk_histories existingProcess_hist = _context.application_desk_histories.Where(C => C.application_id == app.id).FirstOrDefault();

//        if (existingProcess != null)
//        {
//            stff = (int)existingProcess.processor;
//            return true;
//        }
//        else
//        {
//            var apH = _context.application_desk_histories.Where(a => a.application_id == app.id && a.status == GeneralClass.Rejected).OrderByDescending(a => a.date).FirstOrDefault();
//            if (apH == null)
//            {
//                //get the Staff
//                var st = (from stf in _context.Staff
//                          join b in _context.Staff on stf.StaffEmail equals b.StaffEmail
//                          where stf.DeleteStatus != true select new Staff_UserBranchModel
//                          {
//                              Id = stf.StaffID,
//                              StaffId = stf.StaffID,
//                              FieldId = b.FieldOfficeID,
//                              RoleId = b.RoleID,
//                              Active = stf.ActiveStatus,
//                              DeletedStatus = stf.DeleteStatus,
//                              StaffEmail = stf.StaffEmail
//                          }).FirstOrDefault();



//                _context.Staff.Where(a => a.StaffEmail == apH.UserName).FirstOrDefault();
//                if (st != null)
//                {
//                    //check if the User is still active
//                    if (st.Active == true)
//                    {
//                        stff = st.Id;
//                    }
//                    else
//                    {
//                        //look for Staff on the same role and same branch 
//                        var st2 = (from stf in _context.Staff
//                                   join b in _context.Staff on stf.StaffEmail equals b.StaffEmail
//                                   where stf.DeleteStatus != true && b.FieldOfficeID == st.FieldId
//                                   && b.RoleID == st.RoleId && b.ActiveStatus == true
//                                   select new Staff_UserBranchModel
//                                   {
//                                       Id = stf.StaffID,
//                                       StaffId = stf.StaffID,
//                                       FieldId = b.FieldOfficeID,
//                                       RoleId = b.RoleID,
//                                       Active = stf.ActiveStatus,
//                                       DeletedStatus = stf.DeleteStatus,
//                                       StaffEmail = stf.StaffEmail
//                                   }).FirstOrDefault();

//                        if (st2 != null)
//                        {
//                            stff = st.Id;
//                        }
//                    }
//                }
//            }
//            else
//            {
//                stff = (int)apH.StaffID;
//            }
//        }

//        if (_helpersController.Assign(app.id, Id, processId, userEmail, "Application landed on staff desk",null, stff).ToLower() == "ok")
//        {
//            _helpersController.LogMessages($"Created Processing Rules for App {app.reference} and moved to processing staff to Work on it", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

//            return true;
//            // Change app status to Processing
//            app.status = GeneralClass.Processing;
//            app.submitted = true;
//            app.UpdatedAt = DateTime.Now;
//            _context.SaveChanges();

//            if (app.fee_payable > 0)
//            {
//                #region Update Application to Processing on ELPS
//                var appAPI = new ApplicationAPIModel();
//                appAPI.Status = app.status;
//                appAPI.OrderId = app.reference;

//                var param = JsonConvert.SerializeObject(appAPI);
//                var paramDatas = _restService.parameterData("app", param);
//                var output = _restService.Response("/api/Application/{email}/{apiHash}/{app}", paramDatas, "PUT");



//                try
//                {
//                    var respApp = JsonConvert.DeserializeObject<ApplicationAPIModel>(output.Content.ToString());
//                }
//                catch (Exception x)
//                {
//                    _helpersController.LogMessages(x.ToString(), generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

//                }

//                #endregion
//            }

//        }
//        throw new ArgumentException("Error while processing submission.");
//        #endregion

//    }
//    catch (WebException wex)
//    {
//        _helpersController.LogMessages(wex.ToString());
//        // Elmah.ErrorSignal.FromCurrentContext().Raise(wex);
//        var statusCode = ((HttpWebResponse)wex.Response).StatusCode;
//        //throw;
//        return false;
//    }
//    catch (Exception ex)
//    {
//        _helpersController.LogMessages(ex.ToString());
//        //throw;
//        return false;
//    }
//    #endregion

//}