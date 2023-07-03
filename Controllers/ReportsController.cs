
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewDepot.Models;
using NewDepot.Helpers;
using System.IO;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using System.Data.Sql;
using System.Transactions;
using Rotativa.AspNetCore;
using System.Text;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
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
using NewDepot.Models.Stored_Procedures;

namespace NewDepot.Controllers
{
    [Authorize]
    public class ReportsController : Controller
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
        private StoredProcedure _storedProcedure;

        public ReportsController(
            Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, StoredProcedure sp)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _storedProcedure = sp;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

            //newly added

        }


        #region New Report
        public async Task<IActionResult> Index()
        {
            return View(await _context.Reports.ToListAsync());
        }

        public JsonResult GetSevenDaysApplications()
        {

            List<MyApps> myApps =_helpersController.ApplicationDetails();

            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.CreatedAt > DateTime.Now.AddDays( - 60)
                      group a by new
                      {
                          a.dateString
                      }
                      into g
                      select new AdminDashBoardModel
                      {
                          Date = g.Key.dateString,
                          Processing = g.Where(x => x.Status == GeneralClass.Processing).Count(),
                          Paymentpending = g.Where(x => x.Status == GeneralClass.PaymentPending).Count(),
                          Approved = g.Where(x => x.Status == GeneralClass.Approved).Count(),
                      };

            
            return Json(get?.ToList());

        }
        public JsonResult GetApplicationTypes(string id)
        {
            var cat = id != null ? id.Trim() : "";

           List<MyApps> myApps = _helpersController.ApplicationDetails();


            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
                //myApps = myApps.Where(x => )).ToList();
            }

            if (cat != "")
            {
                var get = from a in myApps
                          where a.PhaseName == cat
                          group a by new
                          {
                              a.Type
                          }
                          into g
                          select new
                          {
                              value = g.Key.Type,
                              count = g.Count()
                          };

                return Json(get.ToList());
            }
            else
            {
                var get = from a in myApps
                          where a.PhaseName == cat
                          group a by new
                          {
                              a.Type
                          }
                          into g
                          select new
                          {
                              value = g.Key.Type,
                              count = g.Count()
                          };
                return Json(get.ToList());

            }

        }
        public JsonResult CategoryApplicationChart(ReportModel model = null, string year = null)
        {
           List<MyApps> myApps = _helpersController.ApplicationDetails();


            var appChart = (from app in myApps
                            select new AppReportModel
                            {
                                category = app.CategoryName,
                            }).ToList();
            var AppReportModel = new List<AppReportModel>();

            List<string> AllCategories = appChart.Select(x => x.category).Distinct().ToList();

            for (int i = 0; i < AllCategories.Count(); i++)
            {
                AppReportModel.Add(new AppReportModel
                {
                    category = AllCategories[i],
                    categoryvalue = appChart.Where(x => x.category == AllCategories[i]).Count(),
                });
            }


            return Json(AppReportModel);

        }

        public JsonResult StateApplicationChart(ReportModel model = null, string year = null)
        {

            DateTime sd = string.IsNullOrEmpty(model.mindate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(model.mindate).Date;
            DateTime ed = string.IsNullOrEmpty(model.maxdate) ? DateTime.Now : DateTime.Parse(model.maxdate).Date.AddHours(23).AddMinutes(59);

            List<MyApps> myApps = _helpersController.ApplicationDetails();

            var appChart = (from app in myApps
                            select new AppReportModel
                            {
                                state = app.StateName,
                            }).ToList();
            var AppReportModel = new List<AppReportModel>();

            List<string> AllStates = appChart.Select(x => x.state).Distinct().ToList();

            for (int i = 0; i < AllStates.Count(); i++)
            {
                AppReportModel.Add(new AppReportModel
                {
                    state = AllStates[i],
                    statevalue = appChart.Where(x => x.state == AllStates[i]).Count(),
                });
            }

            return Json(AppReportModel);

        }

        public JsonResult StatusApplicationChart(ReportModel model = null, string year = null)
        {
           List<MyApps> myApps = _helpersController.ApplicationDetails();

            var appss = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true).ToList();

            DateTime sd = string.IsNullOrEmpty(model.mindate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(model.mindate).Date;
            DateTime ed = string.IsNullOrEmpty(model.maxdate) ? DateTime.Now : DateTime.Parse(model.maxdate).Date.AddHours(23).AddMinutes(59);
            

            var appChart = (from app in myApps
                            select new AppReportModel
                            {
                                status = app.Status,
                            }).ToList();
            var AppReportModel = new List<AppReportModel>();

            List<string> AllStatuses = appChart.Select(x => x.status).Distinct().ToList();


            for (int i = 0; i < AllStatuses.Count(); i++)
            {
                AppReportModel.Add(new AppReportModel
                {
                    status = AllStatuses[i],
                    statusvalue = appChart.Where(x => x.status == AllStatuses[i]).Count(),
                });
            }

            return Json(AppReportModel);

        }

        public IActionResult Applications(string status = null)
        {
            List<FieldOffices> getFieldOffice = new List<FieldOffices>();
            List<ZonalOffice> getZonalOffice = new List<ZonalOffice>();
           List<MyApps> myApps = _helpersController.ApplicationDetails();

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
                getZonalOffice = (from zf in _context.ZonalFieldOffice
                                  join zo in _context.ZonalOffice on zf.Zone_id equals zo.Zone_id
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
            

            ViewBag.SuitabilityCount = myApps.Where(x => x.CategoryName.ToLower().Contains("suitability")).Count();
            ViewBag.ModificationCount = myApps.Where(x => x.CategoryName.ToLower().Contains("modification")).Count();
            ViewBag.TakeOverCount = myApps.Where(x => x.CategoryName.ToLower().Contains("take over")).Count();
            ViewBag.RegularizationCount = myApps.Where(x => x.CategoryName.ToLower().Contains("regularization")).Count();
            ViewBag.ATCCount = myApps.Where(x => x.CategoryName.ToLower().Contains("approval to construct")).Count();
            ViewBag.LTOCount = myApps.Where(x => x.CategoryName.ToLower().Contains("license to operate")).Count();
            ViewBag.LRCount = myApps.Where(x => x.CategoryName.ToLower().Contains("renewal")).Count();
            ViewBag.CalibrationCount = myApps.Where(x => x.ShortName.ToLower().Contains("ndt")).Count();
            ViewBag.ReCalibrationCount = myApps.Where(x => x.CategoryName.ToLower().Contains("recalibration")).Count();
            ViewBag.ApplicationCount = myApps.Count();
            ViewBag.SanctionCount = myApps.Where(x => x.CategoryName.ToLower() == "pay sanction").Count();

            #endregion




            List<string> Year = new List<string>();
            myApps.ToList().ForEach(x =>
            {
                Year.Add(x.Year.ToString());
            });
            ViewBag.Year = Year.Distinct();
            return View(searchLists.ToList());
        }

        public JsonResult ApplicationReport()
        {
            List<MyApps> myApps = ApplicationDetails_FZone();

            string result = "";

            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            var year = string.Join(",", (HttpContext.Request.Form["year[0][]"].ToList()));
            var category = string.Join(",", (HttpContext.Request.Form["category[0][]"].ToList()));
            var type = string.Join(",", (HttpContext.Request.Form["type[0][]"].ToList()));
            var stage = string.Join(",", (HttpContext.Request.Form["stage[0][]"].ToList()));
            var status = string.Join(",", (HttpContext.Request.Form["status[0][]"].ToList()));
            var office = string.Join(",", (HttpContext.Request.Form["office[0][]"].ToList()));
            var zone = string.Join(",", (HttpContext.Request.Form["zone[0][]"].ToList()));

            var dateFrom = HttpContext.Request.Form["dateFrom"].FirstOrDefault();
            var dateTo = HttpContext.Request.Form["dateTo"].FirstOrDefault();

            int i = 0;

            //
            

            var appSearch = (from g in myApps
                             select new ReportModel
                             {
                                 category_id = g.CategoryId,
                                 PhaseId=g.PhaseId,
                                 Submitted = g.Submitted,
                                 count = i+1,
                                 AppId = g.appID,
                                 RefNo = g.Reference,
                                 Category = g.CategoryName.ToUpper(),
                                 Type = g.Type.ToUpper(),
                                 Year = g.Year,
                                 State = g.StateName.ToUpper(),
                                 Lga = g.LGA,
                                 CompanyName = g.CompanyName.ToUpper(),
                                 FacId = g.FacilityId!= null ?(int)g.FacilityId:0,
                                 Facility = g.FacilityName.ToUpper(),
                                 FacilityAddress = g.Address_1,
                                 Products = g.Products,
                                 FieldOffice = g.OfficeName,
                                 CurrentDesk = g.currentDeskID!= 0 && g.currentDeskID!= null? _context.Staff.Where(x=> x.StaffID == g.currentDeskID).FirstOrDefault().StaffEmail : "Company",
                                 TotalDays = g.days != 0 ? g.days.ToString(): "NA",
                                 ZonalOffice = g.ZoneName,
                                 OfficeId = g.OfficeId,
                                 //ZonalId = g.ApplicationOffice.ZonalOfficeId,
                                 Status = g.Status,
                                 DateApplied = g.Date_Added.ToString(),
                                 DateSubmitted = g.DateSubmitted.ToString(),
                             }).ToList();


            var querySearch = new List<ReportModel>();
            var querySearch2 = new List<MyApps>();
            string condition = "";


            if (year != null && year.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                querySearch = new List<ReportModel>();

                foreach (var p in year.Split(',').ToList())
                {

                    var apps = searchResult.Where(x => x.Year == Convert.ToInt16(p.Trim())).ToList();

                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });

                }

            }

            if (category != null && category.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                querySearch = new List<ReportModel>();

                foreach (var p in category.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.PhaseId == Convert.ToInt16(p.Trim())).ToList();

                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });
                }

            }

            if (type != null && type.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                querySearch = new List<ReportModel>();

                foreach (var p in type.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.Type == p.Trim()).ToList();

                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });
                }

            }


            if (status != null && status.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                querySearch = new List<ReportModel>();

                foreach (var p in status.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.Status == p.Trim()).ToList();

                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });
                }
            }

            if (office != null && office.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                querySearch = new List<ReportModel>();

                foreach (var p in office.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.FieldOffice!= null && x.FieldOffice.ToLower() == p.ToLower()).ToList();

                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });
                }
            }

            if (zone != null && zone.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                querySearch = new List<ReportModel>();

                foreach (var p in zone.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.ZonalOffice != null && x.ZonalOffice.ToLower() == p.Trim().ToLower()).ToList();
                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });
                }

            }


            if (dateFrom != null && dateTo != null && dateFrom != "" && dateTo != "")
            {
                condition = "Yes";

                var toDate = Convert.ToDateTime(dateTo.Trim()).Date.ToString("yyyy-MM-dd");
                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");

                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                var apps = searchResult.Where(x => x.Submitted == true && Convert.ToDateTime(x.DateSubmitted).Date >= Convert.ToDateTime(fromDate).Date && Convert.ToDateTime(x.DateSubmitted).Date <= Convert.ToDateTime(toDate).Date).ToList();
                querySearch = new List<ReportModel>();
                apps.ForEach(ap =>
                {
                    querySearch.Add(ap);
                });
            }

            else if (dateTo != null && dateFrom != "")
            {
                condition = "Yes";

                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch;
                var apps = searchResult.Where(x => x.Submitted == true && Convert.ToDateTime(x.DateSubmitted) >= Convert.ToDateTime(fromDate)).ToList();
                querySearch = new List<ReportModel>();
                apps.ForEach(ap =>
                {
                    querySearch.Add(ap);
                });
            }
            var get = condition != "" ? querySearch : appSearch;
            var query = (from g in get
                         select new
                         {
                             count = i++,
                             AppId = g.AppId,
                             RefNo = g.RefNo,
                             Category = g.Category.ToUpper(),
                             Type = g.Type.ToUpper(),
                             Year = g.Year,
                             State = g.State.ToUpper(),
                             Lga = g.Lga,
                             CompanyName = g.CompanyName,
                             FacId = g.FacId,
                             Facility = g.Facility.ToUpper(),
                             FacilityAddress = g.FacilityAddress,
                             Products = g.Products,
                             FieldOffice = g.FieldOffice,
                             CurrentDesk = g.CurrentDesk,
                             TotalDays = g.TotalDays,
                             ZonalOffice = g.ZonalOffice,
                             OfficeId = g.OfficeId,
                             ZonalId = g.OfficeId,
                             Status = g.Status,
                             DateApplied = g.DateApplied,
                             DateSubmitted = g.DateSubmitted,
                         });


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    query = sortColumn == "companyName" ? query.ToList().OrderByDescending(c => c.CompanyName) :
                               sortColumn == "category" ? query.OrderByDescending(c => c.Category) :
                               sortColumn == "type" ? query.OrderByDescending(c => c.Type) :
                               sortColumn == "status" ? query.OrderByDescending(c => c.Status) :
                               sortColumn == "dateApplied" ? query.OrderByDescending(c => c.DateApplied) :
                               sortColumn == "dateSubmitted" ? query.OrderByDescending(c => c.DateSubmitted) :
                               query.OrderByDescending(c => c.CompanyName);
                }
                else
                {
                    query = sortColumn == "companyName" ? query.OrderBy(c => c.CompanyName) :
                               sortColumn == "category" ? query.OrderBy(c => c.Category) :
                               sortColumn == "type" ? query.OrderBy(c => c.Type) :
                               sortColumn == "status" ? query.OrderBy(c => c.Status) :
                               sortColumn == "dateApplied" ? query.OrderBy(c => c.DateApplied) :
                               sortColumn == "dateSubmitted" ? query.OrderBy(c => c.DateSubmitted) :
                               query.OrderBy(c => c.CompanyName);
                }

            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                query = query.Where(c => c.RefNo.Contains(txtSearch.ToUpper()) || c.Products.Contains(txtSearch.ToUpper()) || c.CompanyName.Contains(txtSearch.ToUpper()) || c.ZonalOffice.Contains(txtSearch.ToUpper()) || c.FieldOffice.Contains(txtSearch.ToUpper()) || c.State.Contains(txtSearch.ToUpper()) || c.Facility.Contains(txtSearch.ToUpper()) || c.Status.Contains(txtSearch.ToUpper()) || c.Category.Contains(txtSearch.ToUpper()) || c.Type.Contains(txtSearch.ToUpper()));
            }

            totalRecords = query.GroupBy(x => new { x.AppId }).Select(x => x.FirstOrDefault()).ToList().Count();

            var data = query.Skip(skip).Take(pageSize).GroupBy(x => new { x.AppId, x.Category }).Select(x => x.FirstOrDefault()).ToList().OrderBy(x => x.Category).ThenByDescending(x => x.AppId);

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, result = result });

        }
        public IActionResult TransactionReports()
        {
            //List<MyApps> myApps = ApplicationDetails_FZone();

            List<FieldOffices> getFieldOffice = new List<FieldOffices>();
            List<ZonalOffice> getZonalOffice = new List<ZonalOffice>();
            ViewBag.Office = "";
            var getCategory = _context.Categories.Where(x => x.DeleteStatus != true);
            var getPhase = _context.Phases.Where(x => x.DeleteStatus != true);
            var getState = _context.States_UT.Where(x => x.Country_id == 156 && x.DeleteStatus != true);
            var fieldoffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID());

            if (fieldoffice.FirstOrDefault().OfficeName.ToLower().Contains("head"))
            {
                ViewBag.Office = "head";
                getFieldOffice = _context.FieldOffices.Where(x => x.DeleteStatus != true).ToList();
                getZonalOffice = _context.ZonalOffice.Where(x => x.DeleteStatus != true).ToList();
            }
            else
            {
                getFieldOffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID()).ToList();
                getZonalOffice = (from zf in _context.ZonalFieldOffice
                                  join zo in _context.ZonalOffice on zf.Zone_id equals zo.Zone_id
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
                states = getState.ToList()
            });

            var query = (from g in _context.invoices
                         join a in _context.applications on g.application_id equals a.id
                         where  a.isLegacy != true && !g.payment_code.ToLower().Contains("b") 
                         select new
                         {
                             PhaseName =_context.Phases.Where(x => x.id == a.PhaseId).FirstOrDefault().name,
                             Status = g.status,
                             TotalAmount = g.amount,
                             Channel = _context.remita_transactions.Where(x => x.order_id == a.reference).FirstOrDefault() == null ? "online" : _context.remita_transactions.Where(x => x.reference_number == a.reference).FirstOrDefault().type,
                             AppStatus = g.status,
                             Year = g.date_added.Value.Year.ToString(),
                             CategoryName = _context.Categories.Where(x => x.id == a.category_id).FirstOrDefault().name,
                             ShortName = _context.Phases.Where(x => x.id == a.PhaseId).FirstOrDefault().ShortName,
                         }).ToList();


            ViewBag.TotalAmount = query.Where(x => x.Status.ToLower() == "paid").ToList();
            ViewBag.TotalAmount = query.Where(x => x.Status.ToLower() == "paid").Sum(x=> x.TotalAmount);
            ViewBag.PaidSum = query.Where(x => x.Status.ToLower() == "paid").Sum(x => x.TotalAmount);
            ViewBag.UnpaidSum = query.Where(x => x.Status.ToLower() == "unpaid").Sum(x => x.TotalAmount);
            ViewBag.OfflineSum = query.Where(x => x.Channel.ToLower() == "offline").Sum(x => x.TotalAmount);
            ViewBag.OnlineSum = query.Where(x => x.Channel.ToLower().Contains("online")).Sum(x => x.TotalAmount);
            ViewBag.ProcessingSum = query.Where(x => x.AppStatus.ToLower() == "processing").Sum(x => x.TotalAmount);
            ViewBag.CompletedSum = query.Where(x => x.AppStatus.ToLower() == "approved").Sum(x => x.TotalAmount);

            ViewBag.SuitabilitySum = query.Where(x => x.Status.ToLower() == "paid" && x.PhaseName.ToLower().Contains("suitability")).Sum(x => x.TotalAmount);
            ViewBag.ModificationSum = query.Where(x => x.Status.ToLower() == "paid" && x.PhaseName.ToLower().Contains("modification")).Sum(x => x.TotalAmount);
            ViewBag.TakeOverSum = query.Where(x => x.Status.ToLower() == "paid" && x.PhaseName.ToLower().Contains("take over")).Sum(x => x.TotalAmount);
            ViewBag.RegularizationSum = query.Where(x => x.PhaseName.ToLower().Contains("regularization")).Sum(x => x.TotalAmount);
            ViewBag.ATCSum = query.Where(x => x.Status.ToLower() == "paid" && x.PhaseName.ToLower().Contains("approval to construct")).Sum(x => x.TotalAmount);
            ViewBag.LTOSum = query.Where(x => x.Status.ToLower() == "paid" && x.PhaseName.ToLower().Contains("license to operate")).Sum(x => x.TotalAmount);
            ViewBag.LRSum = query.Where(x => x.Status.ToLower() == "paid" && x.PhaseName.ToLower().Contains("renewal")).Sum(x => x.TotalAmount);
            ViewBag.CalibrationSum = query.Where(x => x.Status.ToLower() == "paid" && x.ShortName.ToLower().Contains("ndt")).Sum(x => x.TotalAmount);
            ViewBag.ReCalibrationSum = query.Where(x => x.Status.ToLower() == "paid" && x.PhaseName.ToLower().Contains("recalibration")).Sum(x => x.TotalAmount);
            ViewBag.SanctionSum = query.Where(x => x.Status.ToLower() == "paid" && x.CategoryName.ToLower() == "pay sanction").Sum(x => x.TotalAmount);

            List<string> Year = new List<string>();
            query.ToList().ForEach(x =>
            {
                Year.Add(x.Year.ToString());
            });
            ViewBag.Year = Year.Distinct();

            return View(searchLists.ToList());
        }
        public JsonResult TransactionReport()
        {

            string result = "";
            List<PaymentReportModel> AllPayments = PaymentDetails();


            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            var channel = string.Join(",", (HttpContext.Request.Form["channel[0][]"].ToList()));
            var year = string.Join(",", (HttpContext.Request.Form["year[0][]"].ToList()));
            var category = string.Join(",", (HttpContext.Request.Form["category[0][]"].ToList()));
            var type = string.Join(",", (HttpContext.Request.Form["type[0][]"].ToList()));
            var stage = string.Join(",", (HttpContext.Request.Form["stage[0][]"].ToList()));
            var state = string.Join(",", (HttpContext.Request.Form["state[0][]"].ToList()));
            var status = string.Join(",", (HttpContext.Request.Form["status[0][]"].ToList()));
            var office = string.Join(",", (HttpContext.Request.Form["office[0][]"].ToList()));
            var zone = string.Join(",", (HttpContext.Request.Form["zone[0][]"].ToList()));


            var dateFrom = HttpContext.Request.Form["dateFrom"].FirstOrDefault();
            var dateTo = HttpContext.Request.Form["dateTo"].FirstOrDefault();


            var Invoices = _context.invoices.Where(x=> !x.payment_code.ToLower().Contains("b")).ToList() /*.Where(iv => iv.status.ToLower() == "paid")*/.ToList();  //This allows only the paid for Applications only to be used

            
            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            List<PaymentReportModel> reportModel2 = new List<PaymentReportModel>();
            reportModel = AllPayments;


            string condition = "";
            if (state != null && state.Count() > 0)
            {
                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                condition = "Yes";
                foreach (var p in state.Split(',').ToList())
                {
                    var rpModel = searchResult.Where(x => x.StateName.ToUpper() == p.ToUpper()).ToList();

                    rpModel.ForEach(ap =>
                    {
                        reportModel2.Add(ap);
                    });
                }

            }
            if (category != null && category.Count() > 0)
            {
                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                condition = "Yes";
                foreach (var p in category.Split(',').ToList())
                {
                    var rpModel = searchResult.Where(x => x.Category.ToUpper() == p.ToUpper()).ToList();

                    rpModel.ForEach(ap =>
                    {
                        reportModel2.Add(ap);
                    });
                }

            }

            if (type != null && type.Count() > 0)
            {
                condition = "Yes";

                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                foreach (var p in type.Split(',').ToList())
                {
                    var rpModel = searchResult.Where(x => x.Channel.ToLower() == p.ToLower()).ToList();

                    rpModel.ForEach(ap =>
                    {
                        reportModel2.Add(ap);
                    });
                }

            }
            if (year != null && year.Count() > 0)
            {
                condition = "Yes";
                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                foreach (var p in year.Split(',').ToList())
                {
                    var rpModel = searchResult.Where(x => x.Date.Value.Year.ToString() == p).ToList();

                    rpModel.ForEach(ap =>
                    {
                        reportModel2.Add(ap);
                    });
                }

            }

            if (status != null && status.Count() > 0)
            {
                condition = "Yes";
                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                foreach (var p in status.Split(',').ToList())
                {
                    var rpModel = searchResult.Where(x => x.PaymentStatus == p).ToList();

                    rpModel.ForEach(ap =>
                    {
                        reportModel2.Add(ap);
                    });
                }
            }

            if (office != null && office.Count() > 0)
            {
                condition = "Yes";
                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                foreach (var p in office.Split(',').ToList())
                {
                    var rpModel = searchResult.Where(x => x.Office == p).ToList();
                    rpModel.ForEach(ap =>
                    {
                        reportModel2.Add(ap);
                    });
                }

            }

            if (zone != null && zone.Count() > 0)
            {
                condition = "Yes";
                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                foreach (var p in zone.Split(',').ToList())
                {
                    var rpModel = searchResult.Where(x => x.ZonalOffice == p).ToList();
                    rpModel.ForEach(ap =>
                    {
                        reportModel2.Add(ap);
                    });
                }
            }

            if (dateFrom != null && dateTo != null && dateFrom != "" && dateTo != "")
            {
                condition = "Yes";

                var toDate = Convert.ToDateTime(dateTo.Trim()).Date.ToString("yyyy-MM-dd");
                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");
                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                reportModel2 = new List<PaymentReportModel>();

                var rpModel = searchResult.Where(x => Convert.ToDateTime(x.Date) >= Convert.ToDateTime(fromDate) && Convert.ToDateTime(x.Date) <= Convert.ToDateTime(toDate)).ToList();
                rpModel.ForEach(ap =>
                {
                    reportModel2.Add(ap);
                });
            }
            else if (dateFrom != "" && dateFrom != null)
            {
                condition = "Yes";

                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");
                var toDate = Convert.ToDateTime(dateTo.Trim()).Date.ToString("yyyy-MM-dd");

                var searchResult = reportModel2.Count() > 0 ? reportModel2 : reportModel;
                var rpModel = searchResult.Where(x => Convert.ToDateTime(x.Date) >= Convert.ToDateTime(fromDate) && Convert.ToDateTime(x.Date) <= Convert.ToDateTime(toDate)).ToList();

                reportModel2 = new List<PaymentReportModel>();
                rpModel.ForEach(ap =>
                {
                    reportModel2.Add(ap);
                });
            }



            var get = condition != "" ? reportModel2 : reportModel;

            int i = 0;
            var query = from g in get
                        select new
                        {
                            PaymentStatus = g.PaymentStatus.ToLower() == "paid" ? "PAYMENT COMPLETED" : "PAYMENT PENDING",
                            TransId = i++,
                            Count = i++,
                            RefNo = g.ReferenceNo,
                            RRR = g.PaymentRef,
                            Channel = g.Channel,
                            ReceiptNo = g.ReceiptNo,
                            Category = g.Category,
                            Type = g.Type,
                            State = g.StateName,
                            Lga = g.LGA,
                            CompanyName = g.CompanyName,
                            Facilities = g.FacilityName,
                            FacilityAddress = g.FacilityAddress,
                            //Products = g.TransProducts,
                            FieldOffice = g.Office,
                            ZonalOffice = g.ZonalOffice,
                            OfficeId = g.Office,
                            ZonalId = g.ZonalOffice,
                            Status = g.Status,
                            TransDate = g.Date == null ? "" : g.Date.ToString(),
                            Amount = g.Fee,
                            ExtraAmount = g.TotalExtraAmount,
                            ServiceCharge = g.Charge,
                            TotalAmount = g.TotalAmount
                        };


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    query = sortColumn == "companyName" ? query.OrderByDescending(c => c.CompanyName) :
                               sortColumn == "category" ? query.OrderByDescending(c => c.Category) :
                               sortColumn == "type" ? query.OrderByDescending(c => c.Type) :
                               sortColumn == "status" ? query.OrderByDescending(c => c.Status) :
                               sortColumn == "transDate" ? query.OrderByDescending(c => c.TransDate) :
                               query.OrderByDescending(c => c.CompanyName);
                }
                else
                {
                    query = sortColumn == "companyName" ? query.OrderBy(c => c.CompanyName) :
                               sortColumn == "category" ? query.OrderBy(c => c.Category) :
                               sortColumn == "type" ? query.OrderBy(c => c.Type) :
                               sortColumn == "status" ? query.OrderBy(c => c.Status) :
                               sortColumn == "transDate" ? query.OrderBy(c => c.TransDate) :
                               query.OrderBy(c => c.CompanyName);
                }

            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                query = query.Where(c => c.RefNo.Contains(txtSearch.ToUpper()) || c.RRR.Contains(txtSearch.ToUpper()) || c.CompanyName.Contains(txtSearch.ToUpper()) || c.ZonalOffice.Contains(txtSearch.ToUpper()) || c.FieldOffice.Contains(txtSearch.ToUpper()) || c.State.Contains(txtSearch.ToUpper()) || c.Facilities.Contains(txtSearch.ToUpper()) || c.Status.Contains(txtSearch.ToUpper()) || c.Category.Contains(txtSearch.ToUpper()) || c.Type.Contains(txtSearch.ToUpper()));
            }


            totalRecords = query.GroupBy(x => new { x.TransId }).Select(x => x.FirstOrDefault()).ToList().Count();

            var data = query.Skip(skip).Take(pageSize).GroupBy(x => new { x.TransId, x.Category }).Select(x => x.FirstOrDefault()).ToList().OrderBy(x => x.Category).ThenByDescending(x => x.TransId);


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, result = result });

        }
        public IActionResult PermitsReport()
        {
            List<FieldOffices> getFieldOffice = new List<FieldOffices>();
            List<ZonalOffice> getZonalOffice = new List<ZonalOffice>();

            var getCategory = _context.Categories.Where(x => x.DeleteStatus != true);
            var getPhase = _context.Phases.Where(x => x.DeleteStatus != true);
            var fieldoffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID());

            if (fieldoffice.FirstOrDefault().OfficeName.ToLower().Contains("head"))
            {
                getFieldOffice = _context.FieldOffices.Where(x => x.DeleteStatus != true).ToList();
                getZonalOffice = _context.ZonalOffice.Where(x => x.DeleteStatus != true).ToList();
            }
            else
            {
                getFieldOffice = _context.FieldOffices.Where(x => x.DeleteStatus != true && x.FieldOffice_id == _helpersController.getSessionOfficeID()).ToList();
                getZonalOffice = (from zf in _context.ZonalFieldOffice
                                  join zo in _context.ZonalOffice on zf.Zone_id equals zo.Zone_id
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
                zonalOffices = getZonalOffice.ToList()
            });
            var query = (from g in _context.permits
                         join a in _context.applications on g.application_id equals a.id
                         join ca in _context.Categories on a.category_id equals ca.id
                         join at in _context.Phases on a.PhaseId equals at.id
                         where a.DeleteStatus != true && a.isLegacy != true
                         select new
                         {
                             PhaseName = at.name,
                             PermitType = at.IssueType,
                             Year = g.date_issued.Year,
                             CheckApprovalType = (at.ShortName == "NDT" || at.ShortName == "LTO" || at.ShortName == "LR" || at.ShortName == "TO" || at.ShortName == "RC") ? "Yes" : "",
                             DateExpire = g.date_expire,
                             //ModifyType = _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault() != null ? _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault().Type : "",
                             CategoryName = at.name,
                             ShortName = at.ShortName,
                         }).ToList();

            ViewBag.TotalPermit = query.Count();
            ViewBag.ApprovalCount = query.Where(x => x.PermitType.ToLower().Contains("approval")).Count();
            ViewBag.LicenseCount = query.Where(x => x.PermitType.ToLower().Contains("license")).Count();
            ViewBag.ClearanceCount = query.Where(x => x.PermitType.ToLower().Contains("clearance")).Count();

            ViewBag.SanctionCount = query.Where(x => x.CategoryName.ToLower() == "pay sanction").Count();
            ViewBag.SuitabilityCount = query.Where(x => x.PhaseName.ToLower().Contains("suitability")).Count();
            ViewBag.ModificationCount = query.Where(x => x.PhaseName.ToLower().Contains("modification")).Count();
            ViewBag.TakeOverCount = query.Where(x => x.PhaseName.ToLower().Contains("take over")).Count();
            ViewBag.RegularizationCount = query.Where(x => x.PhaseName.ToLower().Contains("regularization")).Count();
            ViewBag.ATCCount = query.Where(x => x.PhaseName.ToLower().Contains("approval to construct")).Count();
            ViewBag.LTOCount = query.Where(x => x.PhaseName.ToLower().Contains("license to operate")).Count();
            ViewBag.LRCount = query.Where(x => x.PhaseName.ToLower().Contains("renewal")).Count();
            ViewBag.CalibrationCount = query.Where(x => x.ShortName.ToLower().Contains("ndt")).Count();
            ViewBag.ReCalibrationCount = query.Where(x => x.PhaseName.ToLower().Contains("recalibration")).Count();
            ViewBag.ExpiryCount = query.Where(x => DateTime.Now > x.DateExpire && x.CheckApprovalType == "Yes").Count();
            ViewBag.ValidCount = query.Where(x => DateTime.Now <= x.DateExpire).Count();

            List<string> Year = new List<string>();
            query.ToList().ForEach(x =>
            {
                Year.Add(x.Year.ToString());
            });
            ViewBag.Year = Year.Distinct();

            return View(searchLists.ToList());
        }
        public JsonResult PermitReports()
        {

            string result = "";


            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            var year = string.Join(",", (HttpContext.Request.Form["year[0][]"].ToList()));
            var category = string.Join(",", (HttpContext.Request.Form["category[0][]"].ToList()));
            var type = string.Join(",", (HttpContext.Request.Form["type[0][]"].ToList()));

            var dateFrom = HttpContext.Request.Form["dateFrom"].FirstOrDefault();
            var dateTo = HttpContext.Request.Form["dateTo"].FirstOrDefault();


            var appSearch = (from p in _context.permits
                             join g in _context.applications on p.application_id equals g.id
                             join f in _context.Facilities on g.FacilityId equals f.Id
                             join c in _context.companies on g.company_id equals c.id
                             join ad in _context.addresses on f.AddressId equals ad.id
                             join st in _context.States_UT on ad.StateId equals st.State_id
                             join sof in _context.FieldOfficeStates on st.State_id equals sof.StateId
                             join of in _context.FieldOffices on sof.FieldOffice_id equals of.FieldOffice_id
                             join zf in _context.ZonalFieldOffice on of.FieldOffice_id equals zf.FieldOffice_id
                             join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                             join ca in _context.Categories on g.category_id equals ca.id
                             join at in _context.Phases on g.PhaseId equals at.id
                             where g.DeleteStatus != true && g.isLegacy != true
                             select new MyApps
                             {
                                 appID = g.id,
                                 IssueType= at.IssueType,
                                 CategoryName = at.name.ToUpper(),
                                 category_id = at.id,
                                 ShortName=at.ShortName,
                                 Year = g.year,
                                 Type = at.IssueType,
                                 CompanyName = c.name.ToUpper(),
                                 Products = _helpersController.GetFacilityProducts(f.Id),
                                 OfficeName = of.OfficeName.ToUpper(),
                                 ZoneName = z.ZoneName.ToUpper(),
                                 Status = g.status,
                                 DateSubmitted = (DateTime)p.date_issued,
                                 AppPermit= p,
                                 Reference = g.reference,
                                 StateName = st.StateName,
                                 LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                                 FacilityDetails = ad.address_1 + ", " + ad.city,
                                 CompanyDetails = c.name.ToUpper(),
                                 FacilityName = f.Name.ToUpper(),
                                 OfficeId = of.FieldOffice_id,
                                 ZoneId = z.Zone_id
                                
                             });


            var querySearch = new List<MyApps>();

            string condition = "";

            if (year != null && year.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch.ToList();
                querySearch = new List<MyApps>();

                foreach (var p in year.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.DateSubmitted.Year == Convert.ToInt16(p.Trim())).ToList();
                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });

                }

            }

            if (category != null && category.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch.ToList();
                querySearch = new List<MyApps>();

                foreach (var p in category.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.category_id == Convert.ToInt16(p.Trim())).ToList();
                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });

                }

            }

            if (type != null && type.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch.ToList();
                querySearch = new List<MyApps>();

                foreach (var p in type.Split(',').ToList())
                {
                    var apps = searchResult.Where(x => x.Type == p.Trim()).ToList();
                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });
                }

            }


            if (dateFrom != null && dateTo != null && dateFrom != "" && dateTo != "")
            {
                condition = "Yes";
                var toDate = Convert.ToDateTime(dateTo.Trim()).Date.ToString("yyyy-MM-dd");
                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");

                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch.ToList();
                var apps = searchResult.Where(x => x.Submitted == true && Convert.ToDateTime(x.DateSubmitted) >= Convert.ToDateTime(fromDate) && Convert.ToDateTime(x.DateSubmitted) <= Convert.ToDateTime(toDate)).ToList();
                querySearch = new List<MyApps>();

                apps.ForEach(ap =>
                {
                    querySearch.Add(ap);
                });

            }

            else if (dateTo != null && dateFrom != "")
            {
                condition = "Yes";
                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");

                var searchResult = querySearch.Count() > 0 ? querySearch : appSearch.ToList();
                var apps = searchResult.Where(x => x.Submitted == true && Convert.ToDateTime(x.DateSubmitted) >= Convert.ToDateTime(fromDate)).ToList();
                querySearch = new List<MyApps>();

                apps.ForEach(ap =>
                {
                    querySearch.Add(ap);
                });
            }
            var get = condition != "" ? querySearch : appSearch.ToList();

            int i = 0;
            var query = from g in get
                        select new
                        {
                            count = i++,
                            PermitId = g.AppPermit.id,
                            PermitNo = g.AppPermit.permit_no,
                            Category = g.CategoryName.ToUpper(),
                            Type = g.Type.ToUpper(),
                            Reference = g.Reference,
                            State = g.StateName,
                            Lga = g.LGA,
                            FacilityAddress = g.FacilityDetails,
                            CompanyName = g.CompanyDetails.ToUpper(),
                            Facilities = g.FacilityName.ToUpper(),
                            FieldOffice = g.OfficeName,
                            ZonalOffice = g.ZoneName,
                            OfficeId = g.OfficeId,
                            ZonalId = g.ZoneId,
                            IssuedDate = g.AppPermit.date_issued,
                            ExpiryDate = (g.ShortName == "NDT" || g.ShortName == "LTO" || g.ShortName == "LR" || g.ShortName == "TO" || g.ShortName == "RC") ? g.AppPermit.date_expire.ToString() : "Not Applicable",
                            PermitType = g.IssueType,
                            PrintStatus = g.AppPermit.Printed == true ? "yes" : "false"
                            //ApprovedBy = sf.LastName + " " + sf.FirstName
                        };


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    query = sortColumn == "companyName" ? query.OrderByDescending(c => c.CompanyName) :
                               sortColumn == "category" ? query.OrderByDescending(c => c.Category) :
                               sortColumn == "type" ? query.OrderByDescending(c => c.Type) :
                               sortColumn == "state" ? query.OrderByDescending(c => c.State) :
                               sortColumn == "reference" ? query.OrderByDescending(c => c.Reference) :
                               sortColumn == "issuedDate" ? query.OrderByDescending(c => c.IssuedDate) :
                               sortColumn == "expiryDate" ? query.OrderByDescending(c => c.ExpiryDate) :
                               query.OrderByDescending(c => c.CompanyName);
                }
                else
                {
                    query = sortColumn == "companyName" ? query.OrderBy(c => c.CompanyName) :
                               sortColumn == "category" ? query.OrderBy(c => c.Category) :
                               sortColumn == "type" ? query.OrderBy(c => c.Type) :
                               sortColumn == "state" ? query.OrderByDescending(c => c.State) :
                               sortColumn == "reference" ? query.OrderByDescending(c => c.Reference) :
                               sortColumn == "issuedDate" ? query.OrderBy(c => c.IssuedDate) :
                               sortColumn == "expiryDate" ? query.OrderBy(c => c.ExpiryDate) :
                               query.OrderBy(c => c.CompanyName);
                }

            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                query = query.Where(c => c.State.Contains(txtSearch.ToUpper()) || c.Reference.Contains(txtSearch.ToUpper()) || c.CompanyName.Contains(txtSearch.ToUpper()) || c.ZonalOffice.Contains(txtSearch.ToUpper()) || c.FieldOffice.Contains(txtSearch.ToUpper()) || c.Facilities.Contains(txtSearch.ToUpper()) || c.Category.Contains(txtSearch.ToUpper()) || c.Type.Contains(txtSearch.ToUpper()));
            }

            totalRecords = query.GroupBy(x => x.PermitId).Select(x => x.FirstOrDefault()).ToList().Count();

            var data = query.Skip(skip).Take(pageSize).GroupBy(x => new { x.State, x.PermitId }).Select(x => x.FirstOrDefault()).ToList().OrderBy(x => x.Category).ThenByDescending(x => x.PermitId);

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, result = result });

        }
        public IActionResult FacilityReport()
        {
            var getStates = _context.States_UT.Where(x => x.Country_id == 156 && x.DeleteStatus != true);

            List<SearchList> searchLists = new List<SearchList>();

            searchLists.Add(new SearchList
            {
                states = getStates.ToList()
            });

            ViewBag.FacilityCount = _context.Facilities.Where(x => x.DeletedStatus != true).Count();

            return View(searchLists.ToList());
        }
        public JsonResult FacilityReports()
        {

            string result = "";

            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            var state = string.Join(",", (HttpContext.Request.Form["states[0][]"].ToList()));

            var dateFrom = HttpContext.Request.Form["dateFrom"].FirstOrDefault();
            var dateTo = HttpContext.Request.Form["dateTo"].FirstOrDefault();

            

            var facSearch = (from f in _context.Facilities
                            join c in _context.companies on f.CompanyId equals c.id
                            join ad in _context.addresses on f.AddressId equals ad.id
                            join st in _context.States_UT on ad.StateId equals st.State_id
                            where f.DeletedStatus!= true

                            select new MyApps
                            {
                                FacilityId = f.Id,
                                Company_Id = c.id,
                                CompanyName = c.name.ToUpper(),
                                FacilityName = f.Name.ToUpper(),
                                Address_1 = ad.address_1 + ", " + ad.city,
                                LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                                Contact = f.ContactName + " (" + f.ContactNumber + ")",
                                StateName = st.StateName.ToUpper(),
                                TanksCount = _context.Tanks.Where(x => x.FacilityId == f.Id).Count()>0? _context.Tanks.Where(x => x.FacilityId == f.Id).Count().ToString():"0",
                                Capacity = _helpersController.GetTanksCapacity(f.Id),
                                Products = _helpersController.GetFacilityProducts(f.Id),
                                CreatedAt = f.Date
                            }).ToList();



            var querySearch = new List<MyApps>();



            string condition = "";

            if (state != null && state.Count() > 0)
            {
                condition = "Yes";
                var searchResult = querySearch.Count() > 0 ? querySearch : facSearch.ToList();
                querySearch = new List<MyApps>();

                foreach (var p in state.Split(',').ToList())
                {

                    var apps = searchResult.Where(x => x.StateName == p.Trim()).ToList();
                    apps.ForEach(ap =>
                    {
                        querySearch.Add(ap);
                    });
                }

            }


            if (dateFrom != null && dateTo != null && dateFrom != "" && dateTo != "")
            {
                condition = "Yes";
                var searchResult2 = querySearch.Count() > 0 ? querySearch : facSearch.ToList();
                querySearch = new List<MyApps>();

                var toDate = Convert.ToDateTime(dateTo.Trim()).Date.ToString("yyyy-MM-dd");
                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");

                var apps = searchResult2.Where(x => Convert.ToDateTime(x.CreatedAt) >= Convert.ToDateTime(fromDate) && Convert.ToDateTime(x.CreatedAt) <= Convert.ToDateTime(toDate)).ToList();
                apps.ForEach(ap =>
                {
                    querySearch.Add(ap);
                });

            }

            else if (dateTo != null && dateFrom != "")
            {
                condition = "Yes";
                var fromDate = Convert.ToDateTime(dateFrom.Trim()).Date.ToString("yyyy-MM-dd");

                var searchResult2 = querySearch.Count() > 0 ? querySearch : facSearch.ToList();
                querySearch = new List<MyApps>();

                var apps = searchResult2.Where(x => Convert.ToDateTime(x.CreatedAt) >= Convert.ToDateTime(fromDate)).ToList();
                apps.ForEach(ap =>
                {
                    querySearch.Add(ap);
                });
            }
            var get = condition != "" ? querySearch : facSearch.ToList();

            int i = 0;
            var query = from g in get
                        select new
                        {
                            count = i++,
                            FacilityId = g.FacilityId,
                            CompanyName = g.CompanyName.ToUpper(),
                            FacilityName = g.FacilityName.ToUpper(),
                            FacilityAddress = g.Address_1,
                            Contact = g.Contact,
                            LGA = g.LGA,
                            State = g.StateName.ToUpper(),
                            TanksCount =g.TanksCount,
                            Capacity = g.Capacity.ToString(),
                            Products = g.Products,
                            CreatedAt = g.CreatedAt
                        };


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    query = sortColumn == "companyName" ? query.OrderByDescending(c => c.CompanyName) :
                               sortColumn == "facilityName" ? query.OrderByDescending(c => c.FacilityName) :
                               sortColumn == "createdAt" ? query.OrderByDescending(c => c.CreatedAt) :
                               query.OrderByDescending(c => c.State);
                }
                else
                {
                    query = sortColumn == "companyName" ? query.OrderBy(c => c.CompanyName) :
                               sortColumn == "facilityName" ? query.OrderBy(c => c.FacilityName) :
                               sortColumn == "createdAt" ? query.OrderBy(c => c.CreatedAt) :
                               query.OrderBy(c => c.CompanyName);
                }

            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                query = query.Where(c => c.Contact.Contains(txtSearch.ToUpper()) || c.LGA.Contains(txtSearch.ToUpper()) || c.CompanyName.Contains(txtSearch.ToUpper()) || c.FacilityName.Contains(txtSearch.ToUpper()) || c.State.Contains(txtSearch.ToUpper()));
            }

            totalRecords = query.GroupBy(x => x.FacilityId).Select(x => x.FirstOrDefault()).ToList().Count();

            var data = query.Skip(skip).Take(pageSize).GroupBy(x => new { x.State, x.FacilityId }).Select(x => x.FirstOrDefault()).OrderBy(x => x.State).ToList().OrderByDescending(x => x.State);

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data, result = result });

        }

     
        public List<PaymentReportModel> PaymentDetails()
        {
            List<PaymentReportModel> paymentDetails = new List<PaymentReportModel>();

            var apps = _storedProcedure.Payments.FromSqlRaw("EXECUTE dbo.AllPayments").ToList();

            var AllPayments = apps.Where(x=> x.DeleteStatus != true ).GroupBy(x => x.ReferenceNo).Select(x => x.FirstOrDefault()).ToList();
           
            for (int a = 0; a < AllPayments.Count(); a++)
            {
                paymentDetails.Add(new PaymentReportModel
                {
                    ApplicationID = apps[a].ApplicationID,
                    ReferenceNo = apps[a]?.ReferenceNo,
                    LGA = apps[a]?.FacilityLGA,
                    CompanyName = apps[a]?.CompanyName,
                    FacilityAddress = apps[a]?.FacilityAddress + " " + apps[a]?.FacilityLGA,
                    Category = apps[a]?.Category,
                    Date = Convert.ToDateTime(apps[a]?.Date),
                    ReceiptNo = apps[a]?.ReceiptNo,
                    Amount = Convert.ToDouble( apps[a]?.Amount),
                    TotalAmount = Convert.ToDouble(apps[a]?.Amount),
                    Fee = apps[a]?.Fee,
                    Charge = apps[a]?.service_charge,
                    StateName = apps[a]?.StateName,
                    FacilityName = apps[a]?.FacilityName,
                    PaymentRef = apps[a]?.PaymentRef,
                    PaymentStatus = apps[a]?.PaymentStatus,
                    Status = apps[a]?.Status,
                    Channel = apps[a]?.Channel,
                    Type = apps[a]?.Type,
                    Office = apps[a]?.Office,
                    ZonalOffice = apps[a]?.ZonalOffice
                    });
            }

            return paymentDetails;
        }
        public List<MyApps> ApplicationDetails_FZone()
        {
            List<MyApps> myApps = new List<MyApps>();
            //var opt = new DbContextOptions<StoredProcedure>();
            //var _sp = new StoredProcedure(opt);
            var apps = _storedProcedure.AllApps.FromSqlRaw("EXECUTE dbo.AllApplications").ToList();

            var AllApplications = apps.Where(x=> x.DeleteStatus != true ).GroupBy(x => x.appID).Select(x => x.FirstOrDefault()).ToList();
           
            for (int a = 0; a < apps.GroupBy(x=>x.appID).Select(x=> x.FirstOrDefault()).Count(); a++)
            {
                myApps.Add(new MyApps
                {
                    appID = apps[a].appID,
                    Reference = apps[a].Reference,
                    LGA = apps[a]?.FacilityLGA,
                    CompanyEmail = apps[a].CompanyEmail,
                    CompanyName= apps[a].CompanyName,
                    Company_Id = (int)apps[a].CompanyId,
                    FacilityId = apps[a].FacilityId,
                    Address_1 = apps[a].FacilityAddress + " "+ apps[a].FacilityCity,
                    FacilityDetails = apps[a].FacilityName + " (" + apps[a].FacilityAddress + ")",
                    OfficeId = apps[a].OfficeId,
                    OfficeName = apps[a].OfficeName,
                    ZoneName = apps[a].ZoneName,
                    ZoneId = apps[a].ZoneId,
                    FacilityName = apps[a].FacilityName,
                    FlowType = apps[a].FlowType,
                    CheckApprovalType = apps[a].ApprovalType,
                    category_id = Convert.ToInt16( apps[a].CategoryId),
                    CategoryId  = Convert.ToInt16( apps[a].CategoryId),
                    PhaseId = Convert.ToInt16(apps[a].PhaseId),
                    CategoryName = apps[a].CategoryName,
                    PhaseName = apps[a].PhaseName,
                    ShortName = apps[a].ShortName,
                    //
                    //Products = _helpersController.GetFacilityProducts(Convert.ToInt16( apps[a].FacilityId)),
                    Type = apps[a].Type,
                    Status = apps[a].Status,
                    StateName = apps[a].StateName,
                    Submitted = apps[a].Submitted,
                    Fee_Payable = Convert.ToDecimal( apps[a].Fee_Payable),
                    Date_Added = Convert.ToDateTime( apps[a].Date_Added),
                    Year = Convert.ToInt16( apps[a].Year),
                    currentDeskID = apps[a].currentDeskID,
                    days = apps[a].CreatedAt != null ? DateTime.Now.Day - ((DateTime)apps[a].CreatedAt).Day : DateTime.Now.Day - ((DateTime)apps[a].Date_Added).Day,
                    dateString = apps[a].CreatedAt != null ? apps[a].CreatedAt.Value.Date.ToString("yyyy-MM-dd") : apps[a].Date_Added.Value.Date.ToString("yyyy-MM-dd"),
                    DateSubmitted = apps[a].CreatedAt != null ? (DateTime)apps[a].CreatedAt : (DateTime)apps[a].Date_Added
                });
            }

            return myApps;
        }

        //public static string GetFacilityProducts(int facility_id)
        //{
        //    string result = "";
        //    List<string> product = new List<string>();
        //    var _context = new Depot_DBContext();

        //    if (facility_id > 0)
        //    {
        //        var products = from fp in _context.Facilities
        //                       join t in _context.Tanks on fp.Id equals t.FacilityId
        //                       join p in _context.Products on t.ProductId equals p.Id
        //                       where fp.Id == facility_id && (t.Status == null || (t.Status.Contains("Approved")))
        //                       select new
        //                       {
        //                           ProductName = p.Name
        //                       };

        //        if (products.Count() > 0)
        //        {
        //            for (int p = 0; p < products.Count(); p++)
        //            {
        //                product.Add(products.ToList()[p].ProductName);
        //            }

        //            result = string.Join(", ", product.ToList());
        //        }
        //        else
        //        {
        //            result = "No available product";
        //        }
        //    }
        //    else
        //    {
        //        result = "No facility product found.";
        //    }

        //    return result;
        //}


        public List<StaffDesk> GetStaff()
        {
            var getStaff = from s in _context.Staff
                           join l in _context.Location on s.LocationID equals l.LocationID
                           join f in _context.FieldOffices on s.FieldOfficeID equals f.FieldOffice_id
                           join z in _context.ZonalFieldOffice on s.FieldOfficeID equals z.FieldOffice_id
                           join zo in _context.ZonalOffice on z.Zone_id equals zo.Zone_id
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



        public IActionResult CategoryReport(string id)
        {
            ViewData["CategoryName"] = id;

           List<MyApps> myApps = _helpersController.ApplicationDetails();

            var categoryApps = myApps.Where(x => x.PhaseName.ToUpper() == id);
            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                categoryApps = categoryApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                categoryApps = categoryApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            ViewData["CategoryCount"] = categoryApps.Count();
            ViewData["ProcessingCount"] = categoryApps.Where(x => x.Status == GeneralClass.Processing).Count();
            ViewData["RejectionCount"] = categoryApps.Where(x => x.Status == GeneralClass.Rejected).Count();
            ViewData["ApprovalCount"] = categoryApps.Where(x => x.Status == GeneralClass.Approved).Count();

            return View();
        }




        public JsonResult GetPhasess(string id)
        {
            var cat = "";

            cat = id;

           List<MyApps> myApps = _helpersController.ApplicationDetails();


            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.PhaseName == cat
                      group a by new
                      {
                          a.Type
                      }
                      into g
                      select new
                      {
                          value = g.Key.Type,
                          count = g.Count()
                      };

            return Json(get.ToList());

        }

        public JsonResult GetApplicationStatus(string id)
        {
            var cat = "";
            cat = id;

           List<MyApps> myApps = _helpersController.ApplicationDetails();

            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
                //myApps = myApps.Where(x =>  && )).ToList();
            }

            var get = from a in myApps
                      where a.PhaseName == cat
                      group a by new
                      {
                          a.Status
                      }
                      into g
                      select new
                      {
                          value = g.Key.Status,
                          count = g.Count()
                      };

            return Json(get.ToList());

        }



        public JsonResult GetNewApplicationStatus(string id)
        {
            var cat = "";

            cat = id;


           List<MyApps> myApps = _helpersController.ApplicationDetails();

            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.PhaseName == cat && a.Type.ToLower() == "new"
                      group a by new
                      {
                          a.Status
                      }
                      into g
                      select new
                      {
                          value = g.Key.Status,
                          count = g.Count()
                      };

            return Json(get.ToList());

        }
        public JsonResult GetReNewApplicationStatus(string id)
        {
           List<MyApps> myApps = _helpersController.ApplicationDetails();

            var cat = "";

            cat = id;
            
            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.PhaseName == cat && a.Type.ToLower() == "renew"
                      group a by new
                      {
                          a.Status
                      }
                      into g
                      select new
                      {
                          value = g.Key.Status,
                          count = g.Count()
                      };

            return Json(get.ToList());

        }



        public JsonResult GetModApplicationStatus(string id)
        {
           List<MyApps> myApps = _helpersController.ApplicationDetails();

            var cat = "";

            cat = id;

            

            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.PhaseName == cat
                      group a by new
                      {
                          a.Status
                      }
                      into g
                      select new
                      {
                          value = g.Key.Status,
                          count = g.Count()
                      };

            return Json(get.ToList());

        }




        public JsonResult GetTakeApplicationStatus(string id)
        {
            var cat = "";
           List<MyApps> myApps = _helpersController.ApplicationDetails();

            cat = id;
            

            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.PhaseName == cat
                      group a by new
                      {
                          a.Status
                      }
                      into g
                      select new
                      {
                          value = g.Key.Status,
                          count = g.Count()
                      };

            return Json(get.ToList());

        }




        public JsonResult GetApplicationPerDay(string id)
        {
            var cat = "";
            cat = id;
            
            var getStaff = GetStaff();
           List<MyApps> myApps = _helpersController.ApplicationDetails();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.PhaseName == cat
                      group a by new
                      {
                          date = Convert.ToDateTime(a.Date_Added).ToShortDateString()
                      }
                      into g
                      select new
                      {
                          date = g.Key.date,
                          value = g.Count()
                      };

            return Json(get.ToList());

        }



        public JsonResult GetApplicationStageStatus(string id)
        {
            var cat = "";
            cat = id;
           List<MyApps> myApps = _helpersController.ApplicationDetails();
            var getStaff = GetStaff();

            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      where a.PhaseName == cat
                      group a by new
                      {
                          a.ShortName
                      }
                      into g
                      select new
                      {
                          shortname = g.Key.ShortName,
                          processing = g.Where(x => x.Status == GeneralClass.Processing).Count(),
                          rejected = g.Where(x => x.Status == GeneralClass.Rejected).Count(),
                          tanksRequired = g.Where(x => x.Status == GeneralClass.TanksRequired).Count(),
                          approved = g.Where(x => x.Status == GeneralClass.Approved).Count(),
                          paymentpending = g.Where(x => x.Status == GeneralClass.PaymentPending).Count(),
                          paymentcompleted = g.Where(x => x.Status == GeneralClass.PaymentCompleted).Count(),
                          documentrequired = g.Where(x => x.Status == GeneralClass.DocumentsRequired).Count(),
                      };

            return Json(get.ToList());

        }



        public JsonResult GetDashboardStatus()
        {
            
            var getStaff = GetStaff();
           List<MyApps> myApps = _helpersController.ApplicationDetails();


            if (getStaff.FirstOrDefault().Location == "FO")
            {
                myApps = myApps.Where(x => x.OfficeId == _helpersController.getSessionOfficeID()).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "ZO")
            {
                myApps = myApps.Where(x => (x.OfficeId == _helpersController.getSessionOfficeID() || x.ZoneId == getStaff.FirstOrDefault().ZoneId)).ToList();
            }
            else if (getStaff.FirstOrDefault().Location == "HQ")
            {
            }

            var get = from a in myApps
                      group a by new
                      {
                          a.Status
                      }
                      into g
                      select new StatusCountModel
                      {
                          value = g.Key.Status,
                          count = g.Count()
                      };

            

            var appChart = (from app in myApps
                            select new AppReportModel
                            {
                                category = app.CategoryName,
                            }).ToList();
            var AppReportModel = new List<AppReportModel>();

            List<string> AllCategories = appChart.Select(x => x.category).Distinct().ToList();

            for (int i = 0; i < AllCategories.Count(); i++)
            {
                AppReportModel.Add(new AppReportModel
                {
                    category = AllCategories[i],
                    categoryvalue = appChart.Where(x => x.category == AllCategories[i]).Count(),
                });
            }
            AppReportModel.FirstOrDefault().statusCountModel = get.OrderByDescending(x => x.count).ToList();

         
            return Json(AppReportModel);
        }

        #endregion
        public ActionResult PaymentCategory()
        {

            var paidInvoices = _context.invoices.Where(iv => !iv.payment_code.ToLower().Contains("b") && (iv.status.ToLower() == "paid" || iv.status.ToLower() == GeneralClass.PaymentCompleted.ToLower()) ).OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            List<PaymentReportModel> reportModel2 = new List<PaymentReportModel>();

            int IGRTotal = 0;
            int FGTotal = 0;
            int ProcessingFeeTotal = 0;

            //paidInvoices.ForEach(invoice =>
            foreach(var invoice in  paidInvoices)
            {
                var app = _context.applications.Where(a => a.id == invoice.application_id /*&& a.PaymentDescription != null*/).FirstOrDefault();
                string stateName = "";
                if (app != null)
                {
                    var facility = _context.Facilities.Where(f => f.Id == app.FacilityId).FirstOrDefault();
                    if (facility != null)
                    {
                        var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                        if (address != null)
                        {
                            var state = _context.States_UT.Where(a => a.State_id == address.StateId).FirstOrDefault();
                            stateName = state?.StateName;
                        }

                    }
                    var ps = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                    var trimStatutoryFee = app.PaymentDescription != null ?app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Statutory Fee:")).Split(Environment.NewLine.ToCharArray())[0]:null;
                    var trimProcessingFee = app.PaymentDescription != null? app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Processing Fee:")):null;
                    var trimIGRFee = app.PaymentDescription != null?( ps.ShortName == "TO" && app.PaymentDescription.ToLower().Contains("igr fee") ? app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("IGR Fee:")) : null):null;

                    var SFeee = string.IsNullOrEmpty(trimStatutoryFee) ? "0" : trimStatutoryFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray());
                    var sf = SFeee.Replace(",", "").TrimEnd(Environment.NewLine.ToCharArray()).Trim();
                    int SFee = Convert.ToInt32(Convert.ToDouble(SFeee.Replace(",", "").Trim()));
                    var PFeee = string.IsNullOrEmpty(trimProcessingFee) ? "0" : trimProcessingFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray());
                    var pf = !PFeee.Contains(Environment.NewLine.ToString()) ? PFeee.Replace(", ", "").TrimEnd(Environment.NewLine.ToCharArray()).Trim() : PFeee.Replace(", ", "").Split(Environment.NewLine.ToCharArray())[0];
                    int PFee = Convert.ToInt32(Convert.ToDouble(pf.Replace(",", "").Trim()));
                    int IGRFee = string.IsNullOrEmpty(trimIGRFee) ? 0 : Convert.ToInt32(trimIGRFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray()));

                    IGRTotal = IGRFee;
                    FGTotal = app.PaymentDescription!= null ? SFee : Convert.ToInt32(app.fee_payable);
                    ProcessingFeeTotal = PFee;


                    var rm = new PaymentReportModel();
                    rm.Category = ps.name;
                    rm.FG = FGTotal.ToString(); rm.ProcessingFee = ProcessingFeeTotal.ToString(); rm.NMDPRAIGR = IGRTotal.ToString();
                    rm.NMDPRACORC = app.PaymentDescription != null? app.PaymentDescription.Contains("IGR") ? "Not Applicable" : "Not Applicable" : "Not Applicable";
                    rm.Contractor = app.PaymentDescription != null? app.PaymentDescription.Contains("IGR") ? "Not Applicable" : "Not Applicable": "Not Applicable";
                    rm.Amount = Convert.ToDouble(app?.fee_payable);
                    rm.TotalAmount = Convert.ToDouble(app?.fee_payable);
                    rm.Fee = app.fee_payable;
                    rm.Charge = app.service_charge;
                    rm.StateName = stateName;
                    reportModel.Add(rm);
                }
            }
            //);
            List<string> AllCat = new List<string>();
            reportModel.ToList().ForEach(x =>
            {
                AllCat.Add(x.Category.ToString());
            });

            foreach(var cat in AllCat.Distinct().ToList()) {

                var rm = new PaymentReportModel();
                rm.Category = cat;
                rm.FG = reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.FG)).ToString();
                rm.ProcessingFee = reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.ProcessingFee)).ToString();
                rm.NMDPRAIGR =cat.ToLower() == "take over"? reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.NMDPRAIGR)).ToString(): "00000";
                rm.NMDPRACORC = "Not Applicable";
                rm.Contractor = "Not Applicable";
                rm.TotalCatAmount = Convert.ToInt64(reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.FG)) + reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.ProcessingFee)) + reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.NMDPRAIGR)));

                reportModel2.Add(rm);
            };

            var totalCat = reportModel2.Sum(x => x.TotalCatAmount);

            return View(reportModel2);
        }

        public ActionResult PaymentBreakDown(string category)
        {
            var paidInvoices = _context.invoices.Where(iv => iv.status.ToLower() == "paid").OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            List<PaymentReportModel> reportModel2 = new List<PaymentReportModel>();
            int IGRTotal = 0;
            int FGTotal = 0;
            int ProcessingFeeTotal = 0;


            paidInvoices.ForEach(invoice =>
            {
                var app = _context.applications.Where(a =>a.company_id>0 && a.id == invoice.application_id && a.PaymentDescription != null && a.status.ToLower() != "payment pending".ToLower()).FirstOrDefault();
                string stateName = "";

                if (app != null)
                {
                    var rm = new PaymentReportModel();
                    var facility = _context.Facilities.Where(f => f.Id == app.FacilityId).FirstOrDefault();
                    if (facility != null)
                    {
                        var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                        if (address != null)
                        {
                            var state = _context.States_UT.Where(a => a.State_id == address.StateId).FirstOrDefault();
                            stateName = state?.StateName;
                        }

                    }
                    var Company = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();

                    var ps = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                    var trimStatutoryFee = app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Statutory Fee:")).Split(Environment.NewLine.ToCharArray())[0];
                    var trimProcessingFee = app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Processing Fee:"));
                    var trimIGRFee = ps.ShortName=="TO" && app.PaymentDescription.Contains("IGR") ? app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("IGR Fee:")) : null;

                    var SFeee = string.IsNullOrEmpty(trimStatutoryFee) ? "0" : trimStatutoryFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray());
                    var sf = SFeee.Replace(",", "").TrimEnd(Environment.NewLine.ToCharArray()).Trim();
                    int SFee = Convert.ToInt32(Convert.ToDouble(SFeee.Replace(",", "").Trim()));
                    var PFeee = string.IsNullOrEmpty(trimProcessingFee) ? "0" : trimProcessingFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray());
                    var pf = !PFeee.Contains(Environment.NewLine.ToString()) ? PFeee.Replace(", ", "").TrimEnd(Environment.NewLine.ToCharArray()).Trim() : PFeee.Replace(", ", "").Split(Environment.NewLine.ToCharArray())[0];
                    int PFee = Convert.ToInt32(Convert.ToDouble(pf.Replace(",", "").Trim()));
                    int IGRFee = string.IsNullOrEmpty(trimIGRFee) ? 0 : Convert.ToInt32(trimIGRFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray()));

                    IGRTotal += IGRFee;
                    FGTotal += SFee;
                    ProcessingFeeTotal += PFee;

                    rm.ApplicationID = app.id;
                    rm.PaymentBreakdown = app.PaymentDescription;
                    rm.ReferenceNo = app.reference;
                    rm.Category = ps.name;
                    rm.Date = (DateTime)invoice.date_paid;
                    rm.CompanyName = Company.name;
                    rm.FacilityName = facility.Name;
                    rm.ReceiptNo = invoice.receipt_no;
                    rm.Category = ps.name;
                    rm.FG = FGTotal.ToString(); 
                    rm.ProcessingFee = ProcessingFeeTotal.ToString();
                    rm.NMDPRAIGR = IGRTotal.ToString();
                    rm.NMDPRACORC = invoice.payment_type.Contains("IGR") ? "Not Applicable" : "Not Applicable";
                    rm.Contractor = invoice.payment_type.Contains("IGR") ? "Not Applicable" : "Not Applicable";
                    rm.Amount = invoice.amount;
                    rm.TotalAmount = invoice.amount;
                    rm.Fee = app.fee_payable;
                    rm.Charge = app.service_charge;
                    rm.StateName = stateName;
                    reportModel.Add(rm);
                }

            });
            reportModel2 = reportModel.Where(p => p.Category == category.Trim()).ToList();
            ViewBag.Title = category + " Application Payment";
            return View(reportModel2);
        }
        public ActionResult ExtraPayments()
        {

            var ExtraPayments = (from u in _context.ApplicationExtraPayments
                                 join a in _context.applications on u.ApplicationId equals a.id
                                 join c in _context.companies on a.company_id equals c.id
                                 join f in _context.Facilities on a.FacilityId equals f.Id
                                 join ad in _context.addresses on f.AddressId equals ad.id
                                 join st in _context.States on ad.StateId equals st.Id
                                 where a.DeleteStatus != true && f.DeletedStatus != true && (u.Status == GeneralClass.PaymentCompleted || u.Status.ToLower() == "paid")
                                 select new ApplicationExtraPaymentsModel
                                 {
                                     Type= _context.Phases.Where(x=> x.id == a.PhaseId).FirstOrDefault().name.ToUpper(),
                                     ApplicationId = u.ApplicationId,
                                     Amount = u.Amount,
                                     Comment = u.Comment,
                                     Status = u.Status.ToLower() == "paid" ? GeneralClass.PaymentCompleted.ToUpper() : u.Status.ToUpper(),
                                     RRR = u.RRR != null ? u.RRR.ToString() : "No RRR generated",
                                     Reference = a.reference,
                                     UserName = u.UserName != null ? u.UserName : "Empty",
                                     Date = u.Date,
                                     DatePaid = u.DatePaid != null ? u.DatePaid : "No Date",
                                     FacilityName = f.Name,
                                     FacilityAddress = ad.address_1 + " " + ad.city + " " + st.Name,
                                     CompanyName = c.name,
                                     FacilityLGA = ad.LgaId != 0 ? _context.Lgas.Where(x => x.Id == ad.LgaId).FirstOrDefault()!= null?_context.Lgas.Where(x => x.Id == ad.LgaId).FirstOrDefault().Name:"":"",
                                 }).ToList().OrderBy(x=> x.ApplicationId);

            ExtraPayments.FirstOrDefault().TotalPayment = ExtraPayments.Sum(x => x.Amount);
            return View(ExtraPayments.ToList());
        }

        #region  report
        public IActionResult PaymentSummary()
        {
            var payments = FilterPayments();

            ViewData["Office"] = GetStates();

            return View(payments.Where(x => x.TotalAmount != 0).OrderByDescending(x => x.Date).ToList());
        }

        //[Authorize(Roles = "Admin,Support,AD,Account,Director,ITAdmin,HDS,Supervisor,TeamLead")]
        [HttpPost]
        public IActionResult PaymentSummary(PaymentReportViewModel model)
        {
            var payments = FilterPayments(model);

            ViewData["Office"] = GetStates();

            return View(payments.Where(x => x.TotalAmount != 0).OrderByDescending(x => x.Date).ToList());
        }

        #region  GetStates
        //private Dictionary<string, string> GetStates()
        //{
        //    var client = new WebClient();
        //    string output = client.DownloadString(ElpsServices._elpsBaseUrl + "Branch/All/" + ElpsServices.ApiEmail + "/" + ElpsServices.ApiHash);
        //    var branches = JsonConvert.DeserializeObject<List<vBranch>>(output);

        //    output = client.DownloadString(ElpsServices.ApiBaseUrl + $"Branch/AllZones/" + ElpsServices.ApiEmail + "/" + ElpsServices.ApiHash);
        //    var zones = JsonConvert.DeserializeObject<List<vZone>>(output);

        //    var userBranches = _context.Staff.ToList;

        //    //var user = UserManager.Users.FirstOrDefault(x => x.Email.Equals(User.Identity.Name));

        //    var offices = new Dictionary<string, string>();
        //    var userbranch = userBranches.FirstOrDefault(x => x.UserEmail.Equals(User.Identity.Name));

        //    if (userbranch != null)
        //    {
        //        var branch = branches.FirstOrDefault(x => x.Id == userbranch.BranchId);
        //        if (branch != null)
        //        {
        //            if (branch.Name.ToLower().Equals("head office"))
        //                offices = branches.GroupBy(y => y.StateName).Select(x => x.FirstOrDefault()).ToDictionary(t => t.StateName, t => t.Name);
        //            else if (zones.Any(x => x.StateName.Equals(branch.StateName)))
        //            {
        //                var z = zones.FirstOrDefault(x => x.StateName.Equals(branch.StateName));
        //                output = client.DownloadString(ElpsServices.ApiBaseUrl + $"Branch/StatesInZone/{z.Id}/" + ElpsServices.ApiEmail + "/" + ElpsServices.ApiHash);
        //                var states = JsonConvert.DeserializeObject<List<vZoneState>>(output);

        //                offices = states.GroupBy(y => y.StateName).Select(x => x.FirstOrDefault()).ToDictionary(t => t.StateName, t => t.ZoneName);
        //            }
        //            else
        //                offices.Add(branch.StateName, branch.Name);
        //        }

        //    }
        //    else
        //        offices = branches.GroupBy(y => y.StateName).Select(x => x.FirstOrDefault()).ToDictionary(t => t.StateName, t => t.Name);

        //    return offices;
        //}
        #endregion
        private Dictionary<string, string> GetStates()
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var allStaff = _context.Staff.Where(x => x.DeleteStatus != true).ToList();

            var userbranch = _context.Staff.Where(u => u.StaffEmail == userEmail).FirstOrDefault();

            var offices = new Dictionary<string, string>();

            if (userbranch != null)
            {
                var branch = _context.FieldOffices.Where(x => x.FieldOffice_id == userbranch.FieldOfficeID).FirstOrDefault();
                var zone = _context.ZonalFieldOffice.Where(x => x.FieldOffice_id == branch.FieldOffice_id).FirstOrDefault();
                var zon = _context.ZoneStates.Where(x => x.Zone_id == zone.Zone_id).FirstOrDefault();
                var state = _context.States_UT.Where(x => x.State_id == zon.State_id).FirstOrDefault();

                //Adeola to redo
                offices.Add(state.StateName, branch.OfficeName);

                return offices;

            }
            else
                // offices = ZonalFieldOffice.GroupBy(y => y.StateName).Select(x => x.FirstOrDefault()).ToDictionary(t => t.StateName, t => t.Name);

                return offices;
        }
        private List<PaymentReportModel> FilterPayments(PaymentReportViewModel model = null)
        {
            var appss = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true && a.company_id > 0).ToList();
            DateTime sd = model == null ? DateTime.Today.AddDays(-29).Date : (model.mindate == null ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(model.mindate).Date);
            DateTime ed = model == null ? DateTime.Now : (model.maxdate == null ? DateTime.Now : DateTime.Parse(model.maxdate).Date.AddHours(23).AddMinutes(59));

            var paidInvoices = _context.invoices.Where(iv => iv.status.ToLower() == "paid").Where(a => a.date_paid >= sd && a.date_paid <= ed).OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            //List<applications> listOfApplications = new List<applications>();
            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();

            var apps = appss.Where(a => paidInvoices.Any(y => y.application_id == a.id) && a.status.ToLower() != "payment pending".ToLower()).ToList();
            foreach (var invoice in paidInvoices)
            {
                var app = apps.Where(a => a.id == invoice.application_id).FirstOrDefault();
                if (app != null)
                {
                    var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
                    var fac = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                    var add = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();
                    var facState = fac == null ? null : _context.States_UT.Where(x => x.State_id == add.StateId).FirstOrDefault();

                    var rm = new PaymentReportModel();
                    rm.ApplicationID = app.id;
                    rm.Category = _context.Phases.Where(x => x.id == app.PhaseId).FirstOrDefault().name;
                    rm.Date = (DateTime)invoice.date_paid;
                    rm.CompanyName = company.name;
                    rm.ReceiptNo = invoice.receipt_no;
                    rm.Amount = model == null ? invoice.amount : GetPaymentType(invoice, app, model.type);
                    rm.TotalAmount = invoice.amount;
                    rm.Fee = app.fee_payable;
                    rm.Charge = app.service_charge;
                    rm.StateName = facState.StateName;
                    reportModel.Add(rm);
                }
            }

            return reportModel;
        }
        private double GetPaymentType(invoices invoice, applications app, string type = null)
        {
            if (string.IsNullOrEmpty(type))
                return invoice.amount;
            else
            {
                switch (type.ToLower())
                {
                    case "servicecharge":
                        return Convert.ToDouble(app.service_charge);
                    case "application":
                        return Convert.ToDouble(app.fee_payable);
                    default:
                        return invoice.amount;
                }
            }
        }


        public IActionResult AjaxifyPaymentSummary(JQueryDataTableParamModel param, string startDate, string endDate, string t = "combined")
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            var paidInvoices = _context.invoices.Where(iv => iv.status.ToLower() == "paid").Where(a => a.date_paid >= sd && a.date_paid <= ed).OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            var apps = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true && a.company_id > 0).ToList();
            foreach (var invoice in paidInvoices)
            {
                var app = apps.Where(a => a.id == invoice.application_id).FirstOrDefault();
                if (app != null)
                {
                    var category = _context.Categories.Where(a => a.id == app.category_id).FirstOrDefault();
                    var phase = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                    var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();

                    var rm = new PaymentReportModel();
                    rm.ApplicationID = app.id;
                    rm.Category = phase.name;
                    rm.Date = (DateTime)invoice.date_paid;
                    rm.CompanyName = company.name;
                    rm.ReceiptNo = invoice.receipt_no;
                    rm.Amount = (t.ToLower() == "servicecharge" ? Convert.ToDouble(app.service_charge) : (t.ToLower() == "application" ? Convert.ToDouble(app.fee_payable) : invoice.amount));
                    reportModel.Add(rm);
                }
            }

            IEnumerable<PaymentReportModel> filter;
            var sortColIndex = 0 + 1;
            //var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<PaymentReportModel, string> orderFunction = (c => sortColIndex == 1 ? c.ReceiptNo.Trim()
                : sortColIndex == 2 ? c.ApplicationID.ToString() : sortColIndex == 3 ? c.CompanyName.Trim() : sortColIndex == 4 ? c.Category.Trim() : c.Amount.ToString());

            var sortDirection = "desc";
            List<PaymentReportModel> returnedModel = new List<PaymentReportModel>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filter = reportModel.Where(a => a.Amount.ToString().Contains(param.sSearch) || a.ApplicationID.ToString().Contains(param.sSearch) ||
                        a.Category.Trim().Contains(param.sSearch) || a.CompanyName.Trim().Contains(param.sSearch) || a.ReceiptNo.Trim().Contains(param.sSearch));

                if (sortDirection.ToLower() == "asc")
                {
                    filter = filter.OrderBy(orderFunction);
                }
                else
                {
                    filter = filter.OrderByDescending(orderFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filter = reportModel.OrderBy(orderFunction);
                }
                else
                {
                    filter = reportModel.OrderByDescending(orderFunction);
                }
            }
            #endregion

            returnedModel = filter.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            //var result = from c in returnedModel
            //             select new[] { c.ReceiptNo, c.ApplicationID, c.CompanyName, c.Category, c.amount.ToString("N2"), c.Date.ToString() };

            var result = from c in returnedModel.OrderByDescending(a => a.Date)
                         select new[] { c.ReceiptNo, c.ApplicationID.ToString(), c.CompanyName, c.Category, c.Amount.ToString(), c.Date.ToString() };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = reportModel.Count(),
                iTotalDisplayRecords = filter.Count(),
                aaData = result
            });
        }

        //[Authorize(Roles = "Admin,Support,AD,Account,Director,ITAdmin,HDS")]
        public IActionResult PaymentExport(string startDate, string endDate, string t = "combined")
        {
            var sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(startDate).Date;
            var ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            var paidInvoices = _context.Receipts.Where(a => a.date_paid >= sd && a.date_paid <= ed).ToList();  //This allows only the paid for Applications only to be used
            ViewBag.ReturnType = t;

            ViewBag.StartDate = sd;
            ViewBag.EndDate = ed;
            ViewBag.Header = (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate) ? "Receipts in the Last 30 days" : "Receipts between " + sd.ToShortDateString() + " and " + ed.ToShortDateString());
            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            //var apps = _appRep.Where(a => a.Submitted).ToList();
            var cates = _context.Categories.Where(a => a.DeleteStatus != true).ToList();

            foreach (var receipt in paidInvoices)
            {
                var app = _context.applications.Where(a => a.id == receipt.applicationid ).FirstOrDefault();

                if (app != null)
                {
                    var categry = _context.Categories.Where(a => a.id == app.category_id).FirstOrDefault();
                    var phase = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                    var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();

                    var rm = new PaymentReportModel();
                    rm.Category = phase.name;
                    rm.ID = receipt.id;
                    rm.ApplicationID = receipt.applicationid;
                    rm.ReferenceNo = receipt.applicationreference;
                    rm.Date = (DateTime)receipt.date_paid;
                    rm.CompanyName = company.name;
                    rm.ReceiptNo = receipt.receiptno; //.Receipt_No;
                    rm.Fee = (int)cates.Where(a => a.name.Trim().ToLower() == categry.name.Trim().ToLower()).FirstOrDefault().Price;
                    rm.Charge = (int)cates.Where(a => a.name.Trim().ToLower() == categry.name.Trim().ToLower()).FirstOrDefault().ServiceCharge;
                    rm.Amount = receipt.amount;
                    rm.PaymentRef = receipt.RRR;

                    reportModel.Add(rm);
                }
            }
            return View(reportModel.OrderBy(a => a.ID).ToList());
        }

        public List<PaymentSummaryTable> GetSummaryTable(List<PaymentReportModel> model)
        {
            List<PaymentSummaryTable> returnModel = new List<PaymentSummaryTable>();
            foreach (var item in model)
            {
                if (returnModel.Where(m => m.Category.Trim().ToLower() == item.Category.Trim().ToLower()).Count() <= 0)
                    returnModel.Add(new PaymentSummaryTable() { Category = item.Category.Trim() });
            }

            #region Generating the Table
            foreach (var item in returnModel)
            {
                if (item.Category.ToLower() == "general")
                {
                    var touse = model.Where(a => a.Category.Trim().ToLower() == "general").ToList();
                    double amount = touse.Sum(a => a.TotalAmount);
                    double fee = Convert.ToDouble(touse.Sum(a => a.Fee));
                    double amountToShare = ((amount * .985) - (fee) - (10 * touse.Count()));
                    item.Distribution = GenerateTableRow(amountToShare, fee);
                }
                else if (item.Category.ToLower() == "major")
                {
                    var touse = model.Where(a => a.Category.Trim().ToLower() == "major").ToList();
                    double sCharge = Convert.ToDouble(touse.Sum(a => a.Charge));
                    double fee = Convert.ToDouble(touse.Sum(a => a.Fee));
                    double amountToShare = (sCharge - 265 - (10 * touse.Count()));
                    item.Distribution = GenerateTableRow(amountToShare, fee);
                }
                else if (item.Category.ToLower() == "specialized")
                {
                    var touse = model.Where(a => a.Category.Trim().ToLower() == "specialized").ToList();
                    double sCharge = Convert.ToDouble(touse.Sum(a => a.Charge));
                    double fee = Convert.ToDouble(touse.Sum(a => a.Fee));
                    double amountToShare = (sCharge - 550 - (10 * touse.Count()));
                    item.Distribution = GenerateTableRow(amountToShare, fee);
                }
            }
            #endregion

            return returnModel;
        }

        private List<Distribution> GenerateTableRow(double ats, double fee)
        {
            List<Distribution> result = new List<Distribution>();
            Distribution d;
            d.Field = "FG";
            d.Value = fee;
            result.Add(d);

            d.Field = "NMDPRA";
            d.Value = ats * 0.1;
            result.Add(d);

            d.Field = "BrandOneMaxFront";
            d.Value = ats * 0.9;
            result.Add(d);

            d.Field = "Total";
            d.Value = fee + ats;
            result.Add(d);

            return result;
        }

        //[Authorize(Roles = "Admin,Support,AD,Account,Director,ITAdmin,HDS")]
        public IActionResult PaymentSummaryDashboard(string charttype)
        {
            DateTime sd = DateTime.Today.AddDays(-29).Date;
            DateTime ed = DateTime.Now.Date;

            var paidInvoices = _context.invoices.Where(iv => iv.status.ToLower() == "paid").Where(a => a.date_paid > sd && a.date_paid < ed).OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            //List<applications> listOfApplications = new List<applications>();
            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            var apps = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true).ToList();
            foreach (var invoice in paidInvoices)
            {
                var app = apps.Where(a => a.id == invoice.application_id).FirstOrDefault();

                if (app != null)
                {
                    var category = _context.Categories.Where(a => a.id == app.category_id).FirstOrDefault();
                    var phase = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                    var company = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();

                    var rm = new PaymentReportModel();
                    rm.ApplicationID = app.id;
                    rm.Category = phase.name;
                    rm.Date = (DateTime)invoice.date_paid;
                    rm.CompanyName = company.name;
                    rm.ReceiptNo = invoice.receipt_no;
                    rm.Amount = invoice.amount;
                    reportModel.Add(rm);
                }
            }

            List<string> cates = new List<string>();
            foreach (var item in apps)
            {
                var phase = _context.Phases.Where(a => a.id == item.PhaseId).FirstOrDefault();

                if (!cates.Contains(phase.name.Trim()))
                    cates.Add(phase.name.Trim());
            }

            object[] dt = new object[cates.Count()];
            int i = 0;
            var grandTotal = reportModel.Sum(ap => ap.Amount);
            foreach (var item in cates)
            {
                List<object> obj = new List<object>();

                var touse = reportModel.Where(ap => ap.Category.Trim().ToLower() == item.ToLower()).ToList();
                decimal sum = 0;

                var choosen = touse.Where(a => a.Category.Trim().ToLower() == item.ToLower()).ToList();
                sum = (decimal)(!string.IsNullOrEmpty(charttype) && charttype.ToLower() == "percentage" ? ((choosen.Count() > 0) ? ((choosen.Sum(a => a.Amount) / grandTotal) * 100) : 0) : ((choosen.Count() > 0) ? (choosen.Sum(a => a.Amount)) : 0));
                obj.Add(item);
                obj.Add(sum);
                dt[i] = obj.ToArray();
                i += 1;
            }

            string title = (!string.IsNullOrEmpty(charttype) && charttype.ToLower() == "percentage" ? "Percentage Payment Distribution" : "Category Payment Distribution");
            string name = " Payment Summary";

            return PartialView();
        }

        //[Authorize(Roles = "Admin,ITAdmin")]
        public IActionResult StaffMetrics(string startDate, string endDate, string view)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            //Get all staff
            var staffList = GetStaff(string.Empty);
            var history = _context.application_desk_histories.ToList();

            var current = _context.MyDesk.Where(a => a.HasWork != true).ToList();

            //Series[] series = new Series[3];
            List<StaffProcessModel> staffModelList = new List<StaffProcessModel>();
            if (staffList != null && staffList.Count > 0)
            {

                foreach (var staff in staffList)
                {
                    if (staff != null)
                    {
                        StaffProcessModel staffModel = new StaffProcessModel();
                        staffModel.StaffName = staff.FirstName + " " + staff.LastName;
                        var staffEmail = _context.Staff.Where(a => a.StaffID == staff.StaffID).FirstOrDefault().StaffEmail;

                        staffModel.Approved = history.Where(s => s.UserName.ToLower() == staffEmail.ToLower() && s.status.ToLower() == GeneralClass.Approved.ToLower()).ToList().Count;
                        staffModel.Rejected = history.Where(s => s.UserName.ToLower() == staffEmail.ToLower() && s.status.ToLower() == GeneralClass.Rejected.ToLower()).ToList().Count;
                        staffModel.Processing = current.Where(p => p.StaffID == staff.StaffID).ToList().Count;

                        staffModelList.Add(staffModel);
                    }
                }
            }

            //Group = staffName
            //Series = APpr, Proc, Rejct

            List<object> appr = new List<object>();
            List<object> proc = new List<object>();
            List<object> rjct = new List<object>();
            List<string> staffname = new List<string>();

            foreach (var item in staffModelList)
            {
                appr.Add(item.Approved);
                proc.Add(item.Processing);
                rjct.Add(item.Rejected);
                staffname.Add(item.StaffName);
            }

            //series[0] = new Series { Name = "Processing", Data = new Data(proc.ToArray()) };
            //series[1] = new Series { Name = "Approved", Data = new Data(appr.ToArray()) };
            //series[2] = new Series { Name = "Rejected", Data = new Data(rjct.ToArray()) };

            string yAxis = "Number Processed";
            string title = "";
            string tooltip = @"function() { return ''+ this.x +': '+ this.y; }";
            string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>{point.y}</b></td></tr>";
            //ViewBag.Chart = _chartHelper.MultiBarChart(series, staffname, yAxis, title, "Report", tooltip, pointer);

            return View();

        }

        //[Authorize(Roles = "Opscon, AdOps, Supervisor, TeamLead")]
        public IActionResult MyStaffMetrics()
        {
            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

                //Get all staff
                var staffList = userRole.Contains("Opscon") ? GetStaff("Opscon") : userRole.Contains("AdOps") ? GetStaff("AdOps") : userRole.Contains("Supervisor") ? GetStaff("Supervisor") : GetStaff("TeamLead");

                var history = _context.application_desk_histories.ToList();

                var current = _context.MyDesk.Where(a => a.HasWork != true).ToList();

                // Series[] series = new Series[3];
                List<StaffProcessModel> staffModelList = new List<StaffProcessModel>();
                if (staffList != null && staffList.Count > 0)
                {

                    foreach (var staff in staffList)
                    {
                        if (staff != null)
                        {

                            StaffProcessModel staffModel = new StaffProcessModel();
                            staffModel.StaffName = staff.FirstName + " " + staff.LastName;
                            var staffEmail = _context.Staff.Where(a => a.StaffID == staff.StaffID).FirstOrDefault().StaffEmail;

                            staffModel.Approved = history.Where(s => s.UserName.ToLower() == staffEmail.ToLower() && s.status.ToLower() == GeneralClass.Approved.ToLower()).ToList().Count;
                            staffModel.Rejected = history.Where(s => s.UserName.ToLower() == staffEmail.ToLower() && s.status.ToLower() == GeneralClass.Rejected.ToLower()).ToList().Count;
                            staffModel.Processing = current.Where(p => p.StaffID == staff.StaffID).ToList().Count;

                            staffModelList.Add(staffModel);
                        }
                    }
                }

                //Group = staffName
                //Series = APpr, Proc, Rejct

                List<object> appr = new List<object>();
                List<object> proc = new List<object>();
                List<object> rjct = new List<object>();
                List<string> staffname = new List<string>();

                foreach (var item in staffModelList)
                {
                    appr.Add(item.Approved);
                    proc.Add(item.Processing);
                    rjct.Add(item.Rejected);
                    staffname.Add(item.StaffName);
                }

                //series[0] = new Series { Name = "Processing", Data = new Data(proc.ToArray()) };
                //series[1] = new Series { Name = "Approved", Data = new Data(appr.ToArray()) };
                //series[2] = new Series { Name = "Rejected", Data = new Data(rjct.ToArray()) };

                string yAxis = "Number Processed"; //yName.BranchName + "(" + yName.DealerName + ") Gross Profit";
                string title = "";
                string tooltip = @"function() { return ''+ this.x +': '+ this.y; }";
                string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>{point.y}</b></td></tr>";
                //ViewBag.Chart = _chartHelper.MultiBarChart(series, staffname, yAxis, title, "Report", tooltip, pointer);

                return View("StaffMetrics");
            }
            catch (Exception x)
            {

                return Content(x.ToString());
            }
        }

        public List<NewDepot.Models.Staff> GetStaff(string range)
        {
            var manager = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            var getStaff = _context.Staff.Where(u => u.StaffEmail == manager).FirstOrDefault();


            List<NewDepot.Models.Staff> Staff = new List<NewDepot.Models.Staff>();
            if (!string.IsNullOrEmpty(range) && range.ToLower().Trim() == "AdOps" || range.ToLower().Trim() == "Opscon" || range.ToLower().Trim() == "TeamLead")
            {
                //Get All irrespective of branch
                var myDeptStaff = _context.Staff.Where(u => u.FieldOfficeID == getStaff.FieldOfficeID).ToList();
                foreach (var item in myDeptStaff)
                {
                    var stf = _context.Staff.Where(s => s.DeleteStatus != true && s.StaffEmail == item.StaffEmail).FirstOrDefault();
                    Staff.Add(stf);
                }
            }
            else if (!string.IsNullOrEmpty(range) && range.ToLower().Trim() == "Supervisor")
            {
                //Get for manager

                if (userRole.Contains("ManagerPlus"))
                {
                    var myDeptStaff = _context.Staff.Where(u => u.FieldOfficeID == getStaff.FieldOfficeID).ToList();
                    foreach (var item in myDeptStaff)
                    {
                        var stf = _context.Staff.Where(s => s.DeleteStatus != true && s.StaffEmail == item.StaffEmail).FirstOrDefault();
                        Staff.Add(stf);
                    }
                }
                else
                {
                    var myDeptStaff = _context.Staff.Where(u => u.FieldOfficeID == getStaff.FieldOfficeID).ToList();
                    foreach (var item in myDeptStaff)
                    {
                        var stf = _context.Staff.Where(s => s.DeleteStatus != true && s.DeleteStatus != true && s.StaffEmail == item.StaffEmail).FirstOrDefault();
                        Staff.Add(stf);
                    }
                }
            }
            else
            {
                //Get All
                //var Users = UserManager.Users.Where(C => C.Roles.Select(y => y.RoleId).Contains("45ea949b-c11f-4de0-9042-fb2e8d12d89d")).ToList();
                var Users = _context.Staff.Where(s => s.DeleteStatus != true).ToList();
                foreach (var staff in Users)
                {
                    var stf = _context.Staff.Where(s => s.StaffID == staff.StaffID).FirstOrDefault();
                    Staff.Add(stf);
                }
            }

            return Staff;
        }


        public IActionResult StaffProcessingRate(string startDate, string endDate)
        {
            // Use date range, but if not supplied, use last 7 days as the default//
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-7).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var hist = _context.application_desk_histories.Where(a => a.UserName.ToLower() == userEmail.ToLower() && a.date >= sd && a.date <= ed).OrderBy(o => o.date).ToList();

            var current = _context.MyDesk.Where(a => a.HasWork != true).ToList();


            List<string> groups = new List<string>();
            var diff = (ed - sd).TotalDays;
            for (int i = 0; i < diff; i++)
            {
                groups.Add(sd.AddDays(i).ToShortDateString());
            }

            #region ProcessRate: Line Chart
            List<object> obj = new List<object>();
            // Series[] seriesCom = new Series[1];
            //int sum = 0;
            //string grp = string.Empty;
            for (var i = 0; i < groups.Count; i++)
            {
                int sum = hist.Where(a => a.date.ToShortDateString() == groups[i]).Count();
                obj.Add(sum);
            }
            // seriesCom[0] = new Series { Name = "Processed", Data = new Data(obj.ToArray()) };

            string yAxisCom = "Process Rate";
            string titleCom = "Application Processing Rate";
            titleCom += (string.IsNullOrEmpty(startDate) ? " for the last 7 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            //foreach (var dt in groups)
            //{

            //}
            string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            // ViewBag.ProcessRate = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "ProcessRate", tooltipCom);
            #endregion

            #region PieChart
            List<string> cates = new List<string>();
            foreach (var item in hist)
            {
                var app = _context.applications.Where(C => C.id == item.application_id && C.DeleteStatus != true).FirstOrDefault();
                var category = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();

                if (!cates.Contains(category.name.Trim()))
                    cates.Add(category.name.Trim());
            }


            object[] dt = new object[cates.Count()];
            int index = 0;
            var grandTotal = hist.Count();
            foreach (var item in cates)
            {
                List<object> obcj = new List<object>();
                var category = _context.Phases.Where(a => a.name.ToLower() == item.ToLower()).FirstOrDefault();
                var app = _context.applications.Where(C => C.PhaseId == category.id && C.DeleteStatus != true && C.company_id >0).ToList();

                var histy = new List<application_desk_histories>();
                app.ForEach(a =>
                {
                    histy = hist.Where(h => h.application_id == a.id).ToList();
                });
                var choosen = histy;

                obcj.Add(item);
                obcj.Add(choosen.Count());
                dt[index] = obcj.ToArray();
                index += 1;
            }

            string title = "Processed Category Distribution";
            string name = "Processed";
            string tooltip = "{series.name}: <b>{point.percentage:.1f}%</b>";           //"{series.name}: <b>{point.percentage:.1f}%</b>";
            ///ViewBag.PieChart = _chartHelper.pieChart(dt, title, name, "Chart1", tooltip);
            #endregion


            return View();
        }


        #region Reusables

        /// <summary>
        ///     Returns the List of Categories in a Group of Applications
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<string> GetCategoryList(List<applications> list)
        {
            List<string> cates = new List<string>();
            foreach (var item in list)
            {
                var category = _context.Phases.Where(a => a.id == item.PhaseId).FirstOrDefault();

                if (!cates.Contains(category.name.Trim()))
                    cates.Add(category.name.Trim());
            }
            return cates;
        }

        #endregion

        #endregion
    }

}