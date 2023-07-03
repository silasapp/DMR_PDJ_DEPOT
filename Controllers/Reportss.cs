//using DotNet.Highcharts.Options;
//using DotNet.Highcharts;
//using DotNet.Highcharts.Enums;
//using DotNet.Highcharts.Helpers;

using Highsoft.Web.Mvc.Charts;

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
using Highsoft.Web.Mvc.Charts.Rendering;

namespace NewDepot.Controllers
{
    [Authorize]
    public class ReportssController : Controller
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
        GeneralClass.ChartHelper _chartHelper;

        public ReportssController(
            Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _chartHelper = new GeneralClass.ChartHelper();

            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

            //newly added

        }

        //
        // GET: /Reportss/
        //[Authorize(Roles = "Admin,Support,AD,Account,Director,ITAdmin,HDS,TeamLead,Supervisor")]
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

        #region old GetStates
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
            var appss = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true).ToList();
            DateTime sd = model == null ? DateTime.Today.AddDays(-29).Date : (model.mindate == null ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(model.mindate).Date);
            DateTime ed = model == null ? DateTime.Now : (model.maxdate == null ? DateTime.Now : DateTime.Parse(model.maxdate).Date.AddHours(23).AddMinutes(59));

            var paidInvoices = _context.invoices.Where(iv => iv.status.ToLower() == "paid").Where(a => a.date_paid >= sd && a.date_paid <= ed).OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            //List<applications> listOfApplications = new List<applications>();
            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();

            var apps = appss.Where(a => paidInvoices.Any(y => y.application_id == a.id) && a.status.ToLower() != "payment panding".ToLower()).ToList();
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
                var app = _context.applications.Where(a => a.id == receipt.applicationid).FirstOrDefault();

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
            string name = "Payment Summary";
            string tooltip = "{series.name}: <b>{point.percentage:.1f}%</b>";           //"{series.name}: <b>{point.percentage:.1f}%</b>";
            ViewBag.pChart = _chartHelper.pieChart(dt, title, name, "Chart1", tooltip);

            return PartialView();
        }

        //[Authorize(Roles = "Admin,ITAdmin")]
        public JsonResult StaffMetrics(string startDate, string endDate, string view)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            //Get all staff
            var staffList = GetStaff(string.Empty);
            var history = _context.application_desk_histories.ToList();

            var current = _context.MyDesk.Where(a => a.HasWork != true).ToList();

            ColumnSeries[] series = new ColumnSeries[3];
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

            List<int> appr = new List<int>();
            List<int> proc = new List<int>();
            List<int> rjct = new List<int>();
            List<string> staffname = new List<string>();
            List<string> category = new List<string>() { "Approved", "Processing", "Rejected" };
            int totalProPer = 0;
            int totalApPer = 0;
            int totalRejPer = 0;
            var AppReportModel = new List<AppReportModel>();
            
            staffModelList.ForEach(item =>
            {

                appr.Add(item.Approved);
                proc.Add(item.Processing);
                rjct.Add(item.Rejected);
                staffname.Add(item.StaffName);                     
                //}
                //AppReportModel.Add(appreport);
            });
            appr.ForEach(x =>
            {
                totalApPer += x;
            });
            proc.ForEach(x =>
            {
                totalProPer += x;
            });
            rjct.ForEach(x =>
            {
                totalRejPer += x;
            });

                var appreport = new AppReportModel()
                {
                    category = "Approved",
                    value = totalApPer,
                    total = totalApPer,
                    //staffname = item.StaffName

                };
                var pappreport = new AppReportModel()
                {
                    category = "Processing",
                    value = totalProPer,
                    total = totalProPer,
                    //staffname = item.StaffName

                };
                var rappreport = new AppReportModel()
                {
                    category = "Rejected",
                    value = totalRejPer,
                    total = totalRejPer,
                    //staffname = item.StaffName

                };

            AppReportModel.Add(appreport);
            AppReportModel.Add(rappreport);
            AppReportModel.Add(pappreport);

            string yAxis = "Number Processed";
            string title = "";
            string tooltip = @"function() { return ''+ this.x +': '+ this.y; }";
            string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>{point.y}</b></td></tr>";
            ViewBag.Chart = "";

            return Json(AppReportModel);

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

                ColumnSeries[] series = new ColumnSeries[3];
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

                proc.ForEach(x =>
                {
                    series[0] = new ColumnSeries { Name = "Processing", YAxis = x.ToString() };

                });
                appr.ForEach(x =>
                {
                    series[1] = new ColumnSeries { Name = "Processing", YAxis = x.ToString() };

                });
                rjct.ForEach(x =>
                {
                    series[2] = new ColumnSeries { Name = "Processing", YAxis = x.ToString() };

                });
                string yAxis = "Number Processed"; //yName.BranchName + "(" + yName.DealerName + ") Gross Profit";
                string title = "";
                string tooltip = @"function() { return ''+ this.x +': '+ this.y; }";
                string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>{point.y}</b></td></tr>";


                var chartOptions = new Highcharts
                {
                    Title = new Title
                    {
                        Text = "Monthly Average Rainfall"
                    },
                    Subtitle = new Subtitle
                    {
                        Text = "Source: WorldClimate.com"
                    },

                    XAxis = new List<XAxis> {
        new XAxis {
          Categories = new List <string> {"Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"}
        }
      },
                    YAxis = new List<YAxis> {
        new YAxis {
          Min = 0,
            Title = new YAxisTitle {
              Text = "Rainfall (mm)"
            }
        }
      },
                    Tooltip = new Tooltip
                    {
                        HeaderFormat = "<span style='font-size:10px'>{point.key}</span><table style='font-size:12px'>",
                        PointFormat = "<tr><td style='color:{series.color};padding:0'>{series.name}: </td><td style='padding:0'><b>{point.y:.1f} mm</b></td></tr>",
                        FooterFormat = "</table>",
                        Shared = true,
                        UseHTML = true
                    },
                    PlotOptions = new PlotOptions
                    {
                        Column = new PlotOptionsColumn
                        {
                            PointPadding = 0.2,
                            BorderWidth = 0
                        }
                    },
                    Series = new List<Series> {
        new ColumnSeries {
          Name = "Tokyo",
            Data = @ViewData["tokyoData"] as List<ColumnSeriesData>
        },
        new ColumnSeries {
          Name = "New York",
            Data = @ViewData["nyData"] as List<ColumnSeriesData>
        },
        new ColumnSeries {
          Name = "Berlin",
            Data = @ViewData["berlinData"] as List< ColumnSeriesData>
        },
        new ColumnSeries {
          Name = "London",
            Data = @ViewData["londonData"] as List<ColumnSeriesData>
        }
      }
                };

                chartOptions.ID = "chart";
                var renderer = new HighchartsRenderer(chartOptions);

                ViewBag.Chart = renderer;
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

        /// <summary>
        ///     Returns 2 Charts (Line chart and Column chart) and also the summary 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        /// 
        //[Authorize(Roles = "Admin,Support,AD,Account,Director,ITAdmin,HDS,TeamLead,Supervisor")]
        public IActionResult ApplicationSummary()
        {
            var application = FilterApplication();

            ViewData["Office"] = GetStates();

            return View(application);
        }

        //[Authorize(Roles = "Admin,Support,Account,Director, AD,ITAdmin,HDS")]
        [HttpPost]
        public IActionResult ApplicationSummary(ReportModel model, string year)
        {
            var application = FilterApplication(model, year);
            ViewData["Office"] = GetStates();

            //return View(application.applications.OrderByDescending(x => x.DateSubmitted).ToList());
            return View(application);

        }
        private List<ReportModel> FilterApplication(ReportModel model = null, string year = null)
        {
            var appss = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true).ToList();

            // Use date range, but if not supplied, use last 100 days as the default
            DateTime sd = model == null ? DateTime.Today.AddDays(-100).Date : (model.mindate == null ? DateTime.Today.AddDays(-100).Date : DateTime.Parse(model.mindate).Date);
            DateTime ed = model == null ? DateTime.Now : (model.maxdate == null ? DateTime.Now : DateTime.Parse(model.maxdate).Date.AddHours(23).AddMinutes(59));
            string categoryy = model == null ? "" : model.category == null ? "" : model.category;
            List<applications> applications = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true && a.CreatedAt >= sd && a.CreatedAt <= ed).OrderBy(o => o.CreatedAt).ToList();
            List<string> categories = GetCategoryList(applications);
            ColumnSeries[] series = new ColumnSeries[categories.Count()];
            int counter = 0;
            //application according category
            var categry = _context.Phases.Where(a => a.name.ToLower() == categoryy.ToLower()).FirstOrDefault();

            applications = categry == null ? applications : (applications.Where(x => x.PhaseId == categry.id).ToList());
            int yr = year != "" ? Convert.ToInt16(year) : 0;
            //short yar = year != null ? short.Parse(year):0;

            //application according year
            applications = year == null ? applications : (appss.Where(x => x.year == yr).ToList());

            //


            List<ApplicationModel> reportModel = new List<ApplicationModel>();

            //
            List<string> groups = new List<string>();
            var diff = (ed - sd).TotalDays;
            for (int i = 0; i < diff; i++)
            {
                groups.Add(sd.AddDays(i).ToShortDateString());
            }

            List<BasicReportModel> arm = new List<BasicReportModel>();

            #region Seperated
            foreach (var category in categories)
            {
                List<object> objct = new List<object>();
                var categryy = _context.Phases.Where(a => a.name.ToLower() == category.ToLower()).FirstOrDefault();

                var touse = applications.Where(ap => ap.PhaseId == categryy.id).ToList();

                var armodel = new BasicReportModel();
                armodel.Category = category;
                armodel.Count = touse.Count();
                arm.Add(armodel);

                for (var i = 0; i < groups.Count; i++)
                {
                    int sum = touse.Where(a => Convert.ToDateTime(a.CreatedAt).ToShortDateString() == groups[i]).Count();
                    objct.Add(sum);
                }

                objct.ForEach(x =>
                {
                    series[0] = new ColumnSeries { Name = category, YAxis = x.ToString() };

                });
                counter++;
            }


            //check state
            var applicationsfac = (from u in _context.applications.AsEnumerable()
                                   join fac in _context.Facilities.AsEnumerable() on u.FacilityId equals fac.Id
                                   join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                                   join std in _context.States_UT.AsEnumerable() on ad.StateId equals std.State_id
                                   select new MyApps
                                   {

                                       app = u,
                                       StateName = std.StateName
                                   });


            var stateApplications = new List<applications>();

            applicationsfac.ToList().ForEach(a =>
            {
                stateApplications.Add(a.app);
            });

            applications = model == null ? applications : (model.state.Count == 0 ? applications :
                stateApplications);
            //Add into new model

            List<ReportModel> ReportModel = new List<ReportModel>();
            ReportModel.Add(new ReportModel()
            {
                basicReportModel = arm,
                applications = applications
            });
            string yAxis = "Application Rate";
            string title = "Application Rate";
            title += (string.IsNullOrEmpty(sd.ToShortDateString()) ? " for the last 100 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            string tooltip = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            #endregion
            
            #region Combined
            List<object> obj = new List<object>();
            //int sum = 0;
            //string grp = string.Empty;
            for (var i = 0; i < groups.Count; i++)
            {
                int sum = applications.Where(a => Convert.ToDateTime(a.CreatedAt).ToShortDateString() == groups[i]).Count();
                obj.Add(sum);
            }

            List<int> appr = new List<int>();
            List<int> proc = new List<int>();
            List<int> rjct = new List<int>();
            List<string> staffname = new List<string>();
          
           

            string yAxisCom = "Application Rate";
            string titleCom = "Combined Application Rate";
            titleCom += (string.IsNullOrEmpty(sd.ToShortDateString()) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            //ViewBag.CombinedChart = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "CombinedChart", tooltipCom);
            #endregion

            ViewBag.Counter = arm;
          
                           
            ReportModel = model == null ? ReportModel : (model.state.Count == 0 ? ReportModel : ReportModel.Where(x => model.state.Contains(applicationsfac.FirstOrDefault().StateName)).ToList());
           
            
            return ReportModel;


        }
        


        //        //services.AddRange(_serviceRep.Where(C => C.Category_Id == item.Id).ToList());
        //    }


        //    return View();
        //    //return View(new AppCountModel { Categories = categories, Services = services, Specifications = specifications });
        //}

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
    }

}