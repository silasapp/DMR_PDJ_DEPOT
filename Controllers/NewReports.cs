
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

namespace NewDepot.Controllers
{
    [Authorize]
    public class NewReportsController : Controller
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

        public NewReportsController(
            Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

            //newly added

        }



        #region New Report
        public IActionResult ApplicationReport( List<string> status, List<string> type, List<string> year)
        {
            List<FieldOffices> getFieldOffice = new List<FieldOffices>();
            List<ZonalOffice> getZonalOffice = new List<ZonalOffice>();
            List<MyApps> newAppList = new List<MyApps>();
            var allApplications = _helpersController.ApplicationDetails();
            string CriteriaSearch = "";

            ViewBag.CriteriaSearch = CriteriaSearch ;

            #region search List creation
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
            #endregion


            List<string> Year = new List<string>();
            allApplications.ToList().ForEach(x =>
            {
                Year.Add(x.Year.ToString());
            });
            ViewBag.Year = Year.Distinct();
            ViewBag.SearchList = searchLists.ToList();
            var resultData = newAppList.Count() > 0? newAppList: allApplications;

            #region get Count

            ViewBag.SuitabilityCount = resultData.Where(x => x.CategoryName.ToLower().Contains("suitability")).Count();
            ViewBag.ModificationCount = resultData.Where(x => x.CategoryName.ToLower().Contains("modification")).Count();
            ViewBag.TakeOverCount = resultData.Where(x => x.CategoryName.ToLower().Contains("take over")).Count();
            ViewBag.RegularizationCount = resultData.Where(x => x.CategoryName.ToLower().Contains("regularization")).Count();
            ViewBag.ATCCount = resultData.Where(x => x.CategoryName.ToLower().Contains("approval to construct")).Count();
            ViewBag.LTOCount = resultData.Where(x => x.CategoryName.ToLower().Contains("license to operate")).Count();
            ViewBag.LRCount = resultData.Where(x => x.CategoryName.ToLower().Contains("renewal")).Count();
            ViewBag.CalibrationCount = resultData.Where(x => x.ShortName.ToLower().Contains("ndt")).Count();
            ViewBag.ReCalibrationCount = resultData.Where(x => x.CategoryName.ToLower().Contains("recalibration")).Count();
            ViewBag.ApplicationCount = resultData.Where(x => x.Submitted == true).Count();
            ViewBag.SanctionCount = resultData.Where(x => x.CategoryName.ToLower() == "pay sanction").Count();

            #endregion

            return View(resultData);
        }
   [HttpPost]
        public IActionResult ApplicationReport(SearchList searchList, List<string> status, List<string> type, List<string> year)
        {
            List<FieldOffices> getFieldOffice = new List<FieldOffices>();
            List<ZonalOffice> getZonalOffice = new List<ZonalOffice>();
            List<MyApps> newAppList = new List<MyApps>();
            var allApplications = _helpersController.ApplicationDetails();
            string CriteriaSearch = "";
            if (searchList != null)
            {
                CriteriaSearch = "Search Result For: ";

                if (searchList.offices != null)
                {
                    searchList.offices.ForEach(of =>
                    {
                        var apps = allApplications.Where(x => x.OfficeName.ToUpper() == of.OfficeName.ToUpper()).ToList();

                        foreach (var app in apps)
                        {
                            newAppList.Add(app);

                        }
                        CriteriaSearch = CriteriaSearch +"Field Office=> "+ of.OfficeName+",";
                    });
                }

                if (searchList.zonalOffices != null)
                {
                    searchList.zonalOffices.ForEach(of =>
                {
                    var apps = allApplications.Where(x => x.ZoneName.ToUpper() == of.ZoneName.ToUpper()).ToList();

                    foreach (var app in apps)
                    {
                        newAppList.Add(app);
                    }
                    CriteriaSearch = CriteriaSearch + "Zonal Office=> " + of.ZoneName + ",";

                });
                }
                if (searchList.states != null)
                {
                    searchList.states.ForEach(of =>
                {
                    var apps = allApplications.Where(x => x.StateName.ToUpper() == of.StateName.ToUpper()).ToList();

                    foreach (var app in apps)
                    {
                        newAppList.Add(app);
                    }
                    CriteriaSearch = CriteriaSearch + "State=> " + of.StateName + ",";

                });
                }
                if (searchList.phases != null)
                {
                    searchList.phases.ForEach(of =>
                {
                    var apps = allApplications.Where(x => x.PhaseName.ToUpper() == of.name.ToUpper()).ToList();

                    foreach (var app in apps)
                    {
                        newAppList.Add(app);
                    }
                    CriteriaSearch = CriteriaSearch + "Category=> " + of.name + ",";

                });
                }
               
            }

            if(status.Count () > 0)
            {
                status.ForEach(st =>
                {
                    var apps = allApplications.Where(x => x.Status.ToUpper() == st.ToUpper()).ToList();

                    foreach (var app in apps)
                    {
                        newAppList.Add(app);
                    }
                    CriteriaSearch = CriteriaSearch + "Status=> " + st +",";

                });
                
            }
            if (type.Count () > 0)
            {
                type.ForEach(st =>
                {
                    var apps = allApplications.Where(x => x.Type.ToUpper() == st.ToUpper()).ToList();

                    foreach (var app in apps)
                    {
                        newAppList.Add(app);
                    }
                    CriteriaSearch = CriteriaSearch + "Type=> " + st + ",";

                });

            } 
            if(year.Count () > 0)
            {
                type.ForEach(st =>
                {
                    var apps = allApplications.Where(x => x.Year.ToString() == st).ToList();

                    foreach (var app in apps)
                    {
                        newAppList.Add(app);
                    }
                    CriteriaSearch = CriteriaSearch + "Year=> " + st + ",";

                });
            }

            ViewBag.CriteriaSearch = CriteriaSearch ;

            #region search List creation
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
            #endregion


            List<string> Year = new List<string>();
            allApplications.ToList().ForEach(x =>
            {
                Year.Add(x.Year.ToString());
            });
            ViewBag.Year = Year.Distinct();
            ViewBag.SearchList = searchLists.ToList();
            var resultData = newAppList.Count() > 0? newAppList: allApplications;

            #region get Count

            ViewBag.SuitabilityCount = resultData.Where(x => x.CategoryName.ToLower().Contains("suitability")).Count();
            ViewBag.ModificationCount = resultData.Where(x => x.CategoryName.ToLower().Contains("modification")).Count();
            ViewBag.TakeOverCount = resultData.Where(x => x.CategoryName.ToLower().Contains("take over")).Count();
            ViewBag.RegularizationCount = resultData.Where(x => x.CategoryName.ToLower().Contains("regularization")).Count();
            ViewBag.ATCCount = resultData.Where(x => x.CategoryName.ToLower().Contains("approval to construct")).Count();
            ViewBag.LTOCount = resultData.Where(x => x.CategoryName.ToLower().Contains("license to operate")).Count();
            ViewBag.LRCount = resultData.Where(x => x.CategoryName.ToLower().Contains("renewal")).Count();
            ViewBag.CalibrationCount = resultData.Where(x => x.ShortName.ToLower().Contains("ndt")).Count();
            ViewBag.ReCalibrationCount = resultData.Where(x => x.CategoryName.ToLower().Contains("recalibration")).Count();
            ViewBag.ApplicationCount = resultData.Where(x => x.Submitted == true).Count();
            ViewBag.SanctionCount = resultData.Where(x => x.CategoryName.ToLower() == "pay sanction").Count();

            #endregion

            return View(resultData);
        }

        public IActionResult TransactionReports()
        {
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
                states = getState.ToList()
            });

            var query = (from g in _context.invoices
                         join a in _context.applications on g.application_id equals a.id
                         where a.isLegacy != true && !g.payment_code.ToLower().Contains("b")
                         select new
                         {
                             PhaseName = _context.Phases.Where(x => x.id == a.PhaseId).FirstOrDefault().name,
                             Status = g.status,
                             TotalAmount = g.amount,
                             Channel = _context.remita_transactions.Where(x => x.order_id == a.reference).FirstOrDefault() == null ? "online" : _context.remita_transactions.Where(x => x.reference_number == a.reference).FirstOrDefault().type,
                             AppStatus = g.status,
                             Year = g.date_added.Value.Year.ToString(),
                             CategoryName = _context.Categories.Where(x => x.id == a.category_id).FirstOrDefault().name,
                             ShortName = _context.Phases.Where(x => x.id == a.PhaseId).FirstOrDefault().ShortName,
                         }).ToList();


            ViewBag.TotalAmount = query.Where(x => x.Status.ToLower() == "paid").ToList();
            ViewBag.TotalAmount = query.Where(x => x.Status.ToLower() == "paid").Sum(x => x.TotalAmount);
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


            var Invoices = _context.invoices.Where(x => !x.payment_code.ToLower().Contains("b")).ToList() /*.Where(iv => iv.status.ToLower() == "paid")*/.ToList();  //This allows only the paid for Applications only to be used

            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            List<PaymentReportModel> reportModel2 = new List<PaymentReportModel>();
            List<MyApps> myApps = ReportDetails();
            var AllApps = myApps;
            int x = 0;
            //var apps = AllApps.Where(a => Invoices.Any(y => y.application_id == a.Id) && a.Status.ToLower() != "payment pending".ToLower()).ToList();
            foreach (var invoice in Invoices)
            {
                var app = AllApps.Where(a => a.appID == invoice.application_id).FirstOrDefault();
                if (app != null)
                {
                    var company = _context.companies.Where(x => x.id == app.Company_Id).FirstOrDefault();
                    var fac = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                    var add = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();
                    var facState = fac == null ? null : _context.States_UT.Where(x => x.State_id == add.StateId).FirstOrDefault();
                    var Channely = _context.remita_transactions.Where(x => x.reference_number == app.Reference).FirstOrDefault();

                    var rm = new PaymentReportModel();
                    rm.ID = x++;
                    rm.ApplicationID = app.appID;
                    rm.Category = app.CategoryName;
                    rm.Date = (DateTime)invoice.date_paid;
                    rm.CompanyName = company.name;
                    rm.ReceiptNo = invoice.receipt_no;
                    rm.Amount = invoice.amount;
                    rm.TotalAmount = invoice.amount;
                    rm.TotalExtraAmount = _context.ApplicationExtraPayments.Where(x => x.ApplicationId == app.appID && (x.Status.ToLower() == "paid"
                                              || x.Status.ToLower() == GeneralClass.PaymentCompleted.ToLower())).Sum(x => x.Amount);
                    rm.Fee = _context.applications.Where(x => x.id == app.appID).FirstOrDefault()?.fee_payable;
                    rm.Charge = _context.applications.Where(x => x.id == app.appID).FirstOrDefault()?.service_charge;
                    rm.StateName = facState.StateName;
                    rm.PaymentStatus = invoice.status;
                    rm.Status = app.Status;
                    rm.Channel = Channely != null ? Channely.type : "Online";
                    rm.Type = invoice.payment_type;
                    rm.Office = app.OfficeName;
                    rm.ZonalOffice = app.ZoneName;
                    reportModel.Add(rm);
                }
            }

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
                        join a in _context.applications on g.ApplicationID equals a.id
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
                        where a.DeleteStatus != true && a.isLegacy != true
                        select new
                        {
                            PaymentStatus = g.PaymentStatus.ToLower() == "paid" ? "PAYMENT COMPLETED" : "PAYMENT PENDING",
                            TransId = i++,
                            Count = i++,
                            RefNo = a.reference,
                            RRR = g.PaymentRef,
                            Channel = g.Channel,
                            ReceiptNo = g.ReceiptNo,
                            Category = at.name.ToUpper(),
                            Type = a.type.ToUpper(),
                            State = st.StateName.ToUpper(),
                            Lga = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                            CompanyName = c.name.ToUpper(),
                            Facilities = f.Name.ToUpper(),
                            FacilityAddress = ad.address_1 + ", " + ad.city,
                            //Products = g.TransProducts,
                            FieldOffice = of.OfficeName.ToUpper(),
                            ZonalOffice = z.ZoneName.ToUpper(),
                            OfficeId = of.FieldOffice_id,
                            ZonalId = z.Zone_id,
                            Status = g.Status,
                            TransDate = g.Date == null ? "" : g.Date.ToString(),
                            Amount = a.fee_payable,
                            ExtraAmount = g.TotalExtraAmount,
                            ServiceCharge = a.service_charge,
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
                zonalOffices = getZonalOffice.ToList()
            });
            var query = (from g in _context.permits.AsEnumerable()
                         join a in _context.applications.AsEnumerable() on g.application_id equals a.id
                         join ca in _context.Categories.AsEnumerable() on a.category_id equals ca.id
                         join at in _context.Phases.AsEnumerable() on a.PhaseId equals at.id
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
                                 CategoryName = at.name.ToUpper(),
                                 category_id = at.id,
                                 Year = g.year,
                                 Type = at.IssueType,
                                 CompanyName = c.name.ToUpper(),
                                 Products = _helpersController.GetFacilityProducts(f.Id),
                                 OfficeName = of.OfficeName.ToUpper(),
                                 ZoneName = z.ZoneName.ToUpper(),
                                 Status = g.status,
                                 DateSubmitted = (DateTime)p.date_issued,
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
                        join p in _context.permits on g.appID equals p.application_id
                        join a in _context.applications on p.application_id equals a.id
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
                        where a.DeleteStatus != true && a.isLegacy != true
                        select new
                        {
                            count = i++,
                            PermitId = p.id,
                            PermitNo = p.permit_no,
                            Category = at.name.ToUpper(),
                            Type = a.type.ToUpper(),
                            Reference = a.reference,
                            State = st.StateName,
                            Lga = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                            FacilityAddress = ad.address_1 + ", " + ad.city,
                            CompanyName = c.name.ToUpper(),
                            Facilities = f.Name.ToUpper(),
                            FieldOffice = of.OfficeName.ToUpper(),
                            ZonalOffice = z.ZoneName.ToUpper(),
                            OfficeId = of.FieldOffice_id,
                            ZonalId = z.Zone_id,
                            IssuedDate = p.date_issued,
                            ExpiryDate = (at.ShortName == "NDT" || at.ShortName == "LTO" || at.ShortName == "LR" || at.ShortName == "TO" || at.ShortName == "RC") ? p.date_expire.ToString() : "Not Applicable",
                            PermitType = at.IssueType,
                            PrintStatus = p.Printed == true ? "yes" : "false"
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


                             select new MyApps
                             {
                                 FacilityId = f.Id,
                                 Company_Id = c.id,
                                 CompanyName = c.name.ToUpper(),
                                 FacilityName = f.Name.ToUpper(),
                                 Address_1 = ad.address_1 + ", " + ad.city,
                                 Contact = f.ContactName + " (" + f.ContactNumber + ")",
                                 StateName = st.StateName.ToUpper(),
                                 TanksCount = _context.Tanks.Where(x => x.FacilityId == f.Id).Count() > 0 ? _context.Tanks.Where(x => x.FacilityId == f.Id).Count().ToString() : "0",
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
                        join f in _context.Facilities on g.FacilityId equals f.Id
                        join c in _context.companies on f.CompanyId equals c.id
                        join ad in _context.addresses on f.AddressId equals ad.id
                        join st in _context.States_UT on ad.StateId equals st.State_id

                        select new
                        {
                            count = i++,
                            FacilityId = f.Id,
                            CompanyName = c.name.ToUpper(),
                            FacilityName = f.Name.ToUpper(),
                            FacilityAddress = ad.address_1 + ", " + ad.city,
                            Contact = g.Contact,
                            LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                            State = st.StateName.ToUpper(),
                            TanksCount = _context.Tanks.Where(x => x.FacilityId == f.Id).Count().ToString(),
                            Capacity = _context.Tanks.Where(x => x.FacilityId == f.Id)?.ToList().Sum(x => x.Capacity).ToString(),
                            Products = _helpersController.GetFacilityProducts(f.Id),
                            CreatedAt = f.Date.ToString()
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


        public List<MyApps> ReportDetails()
        {
            //var _context = new Depot_DBContext();

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
                       where a.DeleteStatus != true && a.company_id > 0 && a.isLegacy != true && a.submitted == true
                       select new MyApps
                       {
                           appID = a.id,
                           Reference = a.reference,
                           Submitted = a.submitted,
                           CompanyName = c.name,
                           PhaseName = at.name,
                           CompanyDetails = c.CompanyEmail,
                           LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                           FacilityId = f.Id,
                           Company_Id = c.id,
                           FacilityName = f.Name,
                           category_id = ca.id,
                           Address_1 = ad.address_1,
                           FacilityDetails = f.Name + " (" + ad.address_1 + ")",
                           OfficeId = of.FieldOffice_id,
                           Year = a.year,
                           OfficeName = of.OfficeName,
                           ZoneName = z.ZoneName,
                           ZoneId = z.Zone_id,
                           CategoryName = ca.name.ToLower().Contains("pay sanction") ? "Pay Sanction" : at.name.ToUpper(),
                           Products = "GetFacilityProducts(f.Id)",
                           ShortName = at.ShortName,
                           Type = a.type.ToUpper(),
                           Status = a.status,
                           StateName = st.StateName,
                           currentDeskID = _context.MyDesk.Where(x => x.AppId == a.id && x.HasWork != true).FirstOrDefault() != null ? _context.MyDesk.Where(x => x.AppId == a.id && x.HasWork != true).FirstOrDefault().StaffID : 0,
                           CurrentStaff = _context.MyDesk.Where(x => x.AppId == a.id && x.HasWork != true).FirstOrDefault() != null ? _context.Staff.Where(x => x.StaffID == _context.MyDesk.Where(x => x.AppId == a.id && x.HasWork != true).FirstOrDefault().StaffID).FirstOrDefault().StaffEmail : "Company",
                           Date_Added = a.date_added,
                           days = a.CreatedAt != null ? DateTime.Now.Day - ((DateTime)a.CreatedAt).Day : DateTime.Now.Day - ((DateTime)a.date_added).Day,
                           dateString = a.CreatedAt != null ? a.CreatedAt.Value.Date.ToString("yyyy-MM-dd") : a.date_added.Date.ToString("yyyy-MM-dd"),
                           DateSubmitted = a.CreatedAt != null ? (DateTime)a.CreatedAt : (DateTime)a.date_added
                       });

            return app.OrderByDescending(x => x.appID).ToList();
        }

        //public static string GetFacilityProducts(int facility_id)
        //{
        //    string result = "";
        //    List<string> product = new List<string>();
        //    //var _context = new Depot_DBContext(DbContextOptions opt);

        //    if (facility_id > 0)
        //    {
        //        var products = from fp in _context.Facilities.AsEnumerable()
        //                       join t in _context.Tanks.AsEnumerable() on fp.Id equals t.FacilityId
        //                       join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
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
            List<MyApps> myApps = ReportDetails();
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



        #region old report
        public IActionResult PaymentSummary()
        {
            var payments = FilterPayments();

            //ViewData["Office"] = GetStates();

            return View(payments.Where(x => x.TotalAmount != 0).OrderByDescending(x => x.Date).ToList());
        }

        //[Authorize(Roles = "Admin,Support,AD,Account,Director,ITAdmin,HDS,Supervisor,TeamLead")]
        [HttpPost]
        public IActionResult PaymentSummary(PaymentReportViewModel model)
        {
            var payments = FilterPayments(model);

            //ViewData["Office"] = GetStates();

            return View(payments.Where(x => x.TotalAmount != 0).OrderByDescending(x => x.Date).ToList());
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
                    rm.Amount = model == null ? invoice.amount : //GetPaymentType(invoice, app, model.type);
                    rm.TotalAmount = invoice.amount;
                    rm.Fee = app.fee_payable;
                    rm.Charge = app.service_charge;
                    rm.StateName = facState.StateName;
                    reportModel.Add(rm);
                }
            }

            return reportModel;
        }


        #endregion
        #endregion
    }

}