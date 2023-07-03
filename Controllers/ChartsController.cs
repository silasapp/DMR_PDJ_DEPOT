
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
    //[Authorize]
    public class ChartsController : Controller
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

        public ChartsController(
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
        private JsonResult ApplicationPayments(PaymentReportViewModel model = null)
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

            return Json(reportModel);
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



        public JsonResult PaymentCategoryChart()
        {

            var paidInvoices = _context.invoices.Where(iv => iv.status.ToLower() == "paid").OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            List<PaymentReportModel> reportModel2 = new List<PaymentReportModel>();

            int IGRTotal = 0;
            int FGTotal = 0;
            int ProcessingFeeTotal = 0;
           
           
            paidInvoices.ForEach(invoice =>
            {
                var app = _context.applications.Where(a => a.id == invoice.application_id && a.PaymentDescription != null).FirstOrDefault();
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
                    var trimStatutoryFee = app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Statutory Fee:")).Split(Environment.NewLine.ToCharArray())[0];
                    var trimProcessingFee = app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Processing Fee:"));
                    var trimIGRFee = app.PaymentDescription.Contains("IGR") ? app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("IGR Fee:")) : null;

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


                    var rm = new PaymentReportModel();
                    rm.Category = ps.name;
                    rm.FG = FGTotal.ToString(); rm.ProcessingFee = ProcessingFeeTotal.ToString(); rm.NMDPRAIGR = IGRTotal.ToString();
                    rm.NMDPRACORC = invoice.payment_type.Contains("IGR") ? "Not Applicable" : "Not Applicable";
                    rm.Contractor = invoice.payment_type.Contains("IGR") ? "Not Applicable" : "Not Applicable";
                    rm.Amount = invoice.amount;
                    rm.TotalAmount = invoice.amount;
                    rm.Fee = app.fee_payable;
                    rm.Charge = app.service_charge;
                    rm.StateName = stateName;
                    reportModel.Add(rm);
                }
            }
            );
           
            List<string> AllCat = new List<string>();
            reportModel.ToList().ForEach(x =>
            {
                AllCat.Add(x.Category.ToString());
            });

            AllCat.Distinct().ToList().ForEach(cat => {

                var rm = new PaymentReportModel();
                rm.Category = cat;
                rm.FG = reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.FG)).ToString();
                rm.ProcessingFee = reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.ProcessingFee)).ToString();
                rm.NMDPRAIGR = reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.NMDPRAIGR)).ToString();
                rm.NMDPRACORC = "Not Applicable";
                rm.Contractor = "Not Applicable";
                rm.TotalCatAmount = Convert.ToInt64(reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.FG)) + reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.ProcessingFee)) + reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.NMDPRAIGR)));

                reportModel2.Add(rm);
            });
            return Json(reportModel2);
        }
        public JsonResult PaymentStateChart()
        {

            var paidInvoices = _context.invoices.Where(iv => iv.status.ToLower() == "paid").OrderBy(o => o.date_paid).ToList();  //This allows only the paid for Applications only to be used

            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            List<PaymentReportModel> reportModel2 = new List<PaymentReportModel>();

            int IGRTotal = 0;
            int FGTotal = 0;
            int ProcessingFeeTotal = 0;
           
           
            paidInvoices.ForEach(invoice =>
            {
                var app = _context.applications.Where(a => a.id == invoice.application_id && a.PaymentDescription != null).FirstOrDefault();
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
                    var trimStatutoryFee = app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Statutory Fee:")).Split(Environment.NewLine.ToCharArray())[0];
                    var trimProcessingFee = app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Processing Fee:"));
                    var trimIGRFee = app.PaymentDescription.Contains("IGR") ? app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("IGR Fee:")) : null;

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


                    var rm = new PaymentReportModel();
                    rm.Category = ps.name;
                    rm.FG = FGTotal.ToString(); rm.ProcessingFee = ProcessingFeeTotal.ToString(); rm.NMDPRAIGR = IGRTotal.ToString();
                    rm.NMDPRACORC = invoice.payment_type.Contains("IGR") ? "Not Applicable" : "Not Applicable";
                    rm.Contractor = invoice.payment_type.Contains("IGR") ? "Not Applicable" : "Not Applicable";
                    rm.Amount = invoice.amount;
                    rm.TotalAmount = invoice.amount;
                    rm.Fee = app.fee_payable;
                    rm.Charge = app.service_charge;
                    rm.StateName = stateName;
                    reportModel.Add(rm);
                }
            }
            );
           
            List<string> AllState = new List<string>();
            reportModel.ToList().ForEach(x =>
            {
                AllState.Add(x.StateName.ToString());
            });

            AllState.Distinct().ToList().ForEach(cat => {

                var rm = new PaymentReportModel();
                rm.StateName = cat;
                rm.FG = reportModel.Where(r => r.StateName == cat).Sum(x => Convert.ToInt64(x.FG)).ToString();
                rm.ProcessingFee = reportModel.Where(r => r.StateName == cat).Sum(x => Convert.ToInt64(x.ProcessingFee)).ToString();
                rm.NMDPRAIGR = reportModel.Where(r => r.StateName == cat).Sum(x => Convert.ToInt64(x.NMDPRAIGR)).ToString();
                rm.NMDPRACORC = "Not Applicable";
                rm.Contractor = "Not Applicable";
                rm.TotalCatAmount = Convert.ToInt64(reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.FG)) + reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.ProcessingFee)) + reportModel.Where(r => r.Category == cat).Sum(x => Convert.ToInt64(x.NMDPRAIGR)));

                reportModel2.Add(rm);
            });
            return Json(reportModel2);
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
            //string tooltip = "{series.name}: <b>{point.percentage:.1f}%</b>";           //"{series.name}: <b>{point.percentage:.1f}%</b>";
            // ViewBag.pChart = _chartHelper.pieChart(dt, title, name, "Chart1", tooltip);

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
            List<ReportModel> ReportModel = new List<ReportModel>();
            try
            {
                var appss = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true).ToList();

                // Use date range, but if not supplied, use last 100 days as the default
                DateTime sd = model == null ? DateTime.Today.AddDays(-100).Date : (model.mindate == null ? DateTime.Today.AddDays(-100).Date : DateTime.Parse(model.mindate).Date);
                DateTime ed = model == null ? DateTime.Now : (model.maxdate == null ? DateTime.Now : DateTime.Parse(model.maxdate).Date.AddHours(23).AddMinutes(59));
                string categoryy = model == null ? "" : model.category == null ? "" : model.category;
                List<applications> applications = _context.applications.Where(a => a.DeleteStatus != true && a.submitted == true && a.CreatedAt >= sd && a.CreatedAt <= ed).OrderBy(o => o.CreatedAt).ToList();
                List<string> categories = GetCategoryList(applications);
                //Series[] series = new Series[categories.Count()];
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

                    //series[counter] = new Series { Name = category, Data = new Data(objct.ToArray()) };
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
                                       }).Where(x => model.state.Contains(x.StateName));


                var stateApplications = new List<applications>();

                applicationsfac.ToList().ForEach(a =>
                {
                    stateApplications.Add(a.app);
                });

                applications = model == null ? applications : (model.state.Count == 0 ? applications :
                    stateApplications);
                //Add into new model

                ReportModel.Add(new ReportModel()
                {
                    basicReportModel = arm,
                    applications = applications
                });
                string yAxis = "Application Rate";
                string title = "Application Rate";
                title += (string.IsNullOrEmpty(sd.ToShortDateString()) ? " for the last 100 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

                string tooltip = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
                //string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>₦ {point.y:.2f}</b></td></tr>";
                //ViewBag.ApplicationChart = _chartHelper.LineChart(series, groups, title, yAxis, "AppChart", tooltip);
                #endregion

                #region Combined
                List<object> obj = new List<object>();
                //Series[] seriesCom = new Series[1];
                //int sum = 0;
                //string grp = string.Empty;
                for (var i = 0; i < groups.Count; i++)
                {
                    int sum = applications.Where(a => Convert.ToDateTime(a.CreatedAt).ToShortDateString() == groups[i]).Count();
                    obj.Add(sum);
                }
                // seriesCom[0] = new Series { Name = "All Applications", Data = new Data(obj.ToArray()) };

                string yAxisCom = "Application Rate";
                string titleCom = "Combined Application Rate";
                titleCom += (string.IsNullOrEmpty(sd.ToShortDateString()) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

                string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
                //string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>₦ {point.y:.2f}</b></td></tr>";
                //ViewBag.CombinedChart = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "CombinedChart", tooltipCom);
                #endregion

                ViewBag.Counter = arm;

                ReportModel = model == null ? ReportModel : (model.state.Count == 0 ? ReportModel : ReportModel.Where(x => model.state.Contains(applicationsfac.FirstOrDefault().StateName)).ToList());
                return ReportModel;
            }
            catch (Exception e)
            {
                return ReportModel;

            }

        }
        //License Fiter and Summary
        //[Authorize(Roles = "Admin,Support,AD,Account,Director,ITAdmin,HDS,TeamLead,Supervisor")]
        public IActionResult PermitNLicenseSummary()
        {
            var application = FilterLicense();

            ViewData["Office"] = GetStates();

            return View(application);
        }

        //[Authorize(Roles = "Admin,Support,Account,Director, AD,ITAdmin,HDS")]
        [HttpPost]
        public IActionResult PermitNLicenseSummary(ReportModel model, string year)
        {
            var application = FilterLicense(model, year);
            ViewData["Office"] = GetStates();

            return View(application);

        }
        private List<ReportModel> FilterLicense(ReportModel model = null, string year = null)
        {
            //var yr = model != null ? model.year : "";

            // Use date range, but if not supplied, use last 100 days as the default
            DateTime sd = model == null ? DateTime.Today.AddDays(-100).Date : (model.mindate == null ? DateTime.Today.AddDays(-100).Date : DateTime.Parse(model.mindate).Date);
            DateTime ed = model == null ? DateTime.Now : (model.maxdate == null ? DateTime.Now : DateTime.Parse(model.maxdate).Date.AddHours(23).AddMinutes(59));

            List<permits> permits = _context.permits.Where(a => a.date_issued >= sd && a.date_issued <= ed).OrderBy(o => o.date_issued).ToList();
            List<string> categories = new List<string>(); // GetCategoryList(applications);
            foreach (var item in permits)
            {
                if (!categories.Contains(item.categoryName.Trim()))
                    categories.Add(item.categoryName.Trim());
            }

            int counter = 0;

            List<string> groups = new List<string>();
            var diff = (ed - sd).TotalDays;
            for (int i = 0; i < diff; i++)
            {
                groups.Add(sd.AddDays(i).ToShortDateString());
            }
            int yr = year != "" ? Convert.ToInt16(year) : 0;

            List<BasicReportModel> arm = new List<BasicReportModel>();

            #region Seperated
            foreach (var category in categories)
            {
                List<object> objct = new List<object>();
                var touse = permits.Where(ap => ap.categoryName.Trim().ToLower() == category.ToLower()).ToList();

                var armodel = new BasicReportModel();
                armodel.Category = category;
                armodel.Count = touse.Count();
                arm.Add(armodel);
                for (int i = 0; i < touse.Count; i++)
                {
                    touse[i].dateString = touse[i].date_issued.ToShortDateString();
                }

                for (var i = 0; i < groups.Count; i++)
                {
                    var gi = groups[i];
                    int sum = touse.Where(a => a.dateString == gi).Count();
                    objct.Add(sum);
                }

                //series[counter] = new Series { Name = category, Data = new Data(objct.ToArray()) };
                counter++;

                string yAxis = "Permit Rate";
                //string title = "Permit Rate";
                //title += (string.IsNullOrEmpty(sd.ToShortDateString()) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

                //string tooltip = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
                // ViewBag.ApplicationChart = _chartHelper.LineChart(series, groups, title, yAxis, "AppChart", tooltip);
            }

            #endregion

            #region Combined
            List<object> obj = new List<object>();
            //Series[] seriesCom = new Series[1];
            for (var i = 0; i < groups.Count; i++)
            {
                int sum = permits.Where(a => a.date_issued.ToShortDateString() == groups[i]).Count();
                obj.Add(sum);
            }
            //seriesCom[0] = new Series { Name = "All Applications", Data = new Data(obj.ToArray()) };

            //string yAxisCom = "Permit Rate";
            //string titleCom = "Combined Permit Rate";

            //titleCom += (string.IsNullOrEmpty(sd.ToShortDateString()) ? " for the last 100 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());
            //  string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            //ViewBag.CombinedChart = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "CombinedChart", tooltipCom);
            #endregion

            ViewBag.Counter = arm;
            string categoryy = model == null ? "" : model.category == null ? "" : model.category;

            permits = categoryy == "" ? permits : (_context.permits.Where(x => x.categoryName.ToLower() == categoryy.ToLower())).ToList();
            string issueType = model == null ? "" : model.issueType == null ? "" : model.issueType;

            //application according to state
            //check state
            var permitsfac = (from u in _context.applications.AsEnumerable()
                              join perm in _context.permits.AsEnumerable() on u.id equals perm.application_id
                              join fac in _context.Facilities.AsEnumerable() on u.FacilityId equals fac.Id
                              join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                              join std in _context.States_UT.AsEnumerable() on ad.StateId equals std.State_id
                              select new MyApps
                              {
                                  AppPermit = perm,
                                  app = u,
                                  StateName = std.StateName
                              })/*.Where(x => model.state.Contains(x.StateName))*/;


            var statePermits = new List<permits>();

            permitsfac.ToList().ForEach(a =>
            {
                statePermits.Add(a.AppPermit);
            });


            permits = issueType == "" ? permits : (_context.permits.Where(x => x.categoryName == issueType).ToList());
            permits = model == null ? permits : (model.state.Count == 0 ? permits : statePermits);

            //Add into new model

            List<ReportModel> ReportModel = new List<ReportModel>();
            ReportModel.Add(new ReportModel()
            {
                basicReportModel = arm,
                permitsRepo = permits
            });

            ViewBag.Counter = arm;

            //reportModel = model == null ? ReportModel : (model.state.Count == 0 ? ReportModel : ReportModel.Where(x => model.state.Contains(x.applications.FirstOrDefault().StateName)).ToList());
            return ReportModel;


        }

        public IActionResult ApplicationReport(string startDate, string endDate)
        {
            // Use date range, but if not supplied, use last 30 days as the default
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            List<applications> applications = _context.applications.Where(a => a.submitted != true && a.CreatedAt >= sd && a.CreatedAt <= ed).OrderBy(o => o.date_added).ToList();
            List<string> categories = GetCategoryList(applications);
            //Series[] series = new Series[categories.Count()];
            int counter = 0;

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
                var categry = _context.Phases.Where(a => a.name.ToLower() == category.ToLower()).FirstOrDefault();

                var touse = applications.Where(ap => ap.PhaseId == categry.id).ToList();

                List<object> objct = new List<object>();

                var armodel = new BasicReportModel();
                armodel.Category = category;
                armodel.Count = touse.Count();
                arm.Add(armodel);

                for (var i = 0; i < groups.Count; i++)
                {
                    int sum = touse.Where(a => Convert.ToDateTime(a.CreatedAt).ToShortDateString() == groups[i]).Count();
                    objct.Add(sum);
                }

                // series[counter] = new Series { Name = category, Data = new Data(objct.ToArray()) };
                counter++;
            }
            //string yAxis = "Application Rate";
            //string title = "Application Rate";
            //title += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            //string tooltip = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            ////string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>₦ {point.y:.2f}</b></td></tr>";
            //ViewBag.ApplicationChart = _chartHelper.LineChart(series, groups, title, yAxis, "AppChart", tooltip);
            #endregion

            #region Combined
            List<object> obj = new List<object>();
            //Series[] seriesCom = new Series[1];
            //int sum = 0;
            //string grp = string.Empty;
            for (var i = 0; i < groups.Count; i++)
            {
                int sum = applications.Where(a => Convert.ToDateTime(a.CreatedAt).ToShortDateString() == groups[i]).Count();
                obj.Add(sum);
            }
            //seriesCom[0] = new Series { Name = "All Applications", Data = new Data(obj.ToArray()) };

            //string yAxisCom = "Application Rate";
            //string titleCom = "Combined Application Rate";
            //titleCom += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            //string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            ////string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>₦ {point.y:.2f}</b></td></tr>";
            //ViewBag.CombinedChart = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "CombinedChart", tooltipCom);
            #endregion

            ViewBag.Counter = arm;

            return View();

        }

        //[Authorize(Roles = "Admin,Support,Account,Director, AD,ITAdmin,HDS")]
        public IActionResult PermitReport(string startDate, string endDate)
        {
            // Use date range, but if not supplied, use last 30 days as the default
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-29).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            List<permits> permits = _context.permits.Where(a => a.date_issued >= sd && a.date_issued <= ed).OrderBy(o => o.date_issued).ToList();
            List<string> categories = new List<string>(); // GetCategoryList(applications);
            foreach (var item in permits)
            {
                if (!categories.Contains(item.categoryName.Trim()))
                    categories.Add(item.categoryName.Trim());
            }

            // Series[] series = new Series[categories.Count()];
            int counter = 0;

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
                var touse = permits.Where(ap => ap.categoryName.Trim().ToLower() == category.ToLower()).ToList();

                var armodel = new BasicReportModel();
                armodel.Category = category;
                armodel.Count = touse.Count();
                arm.Add(armodel);
                for (int i = 0; i < touse.Count; i++)
                {
                    touse[i].dateString = touse[i].date_issued.ToShortDateString();
                }

                for (var i = 0; i < groups.Count; i++)
                {
                    var gi = groups[i];
                    int sum = touse.Where(a => a.dateString == gi).Count();
                    objct.Add(sum);
                }

                //series[counter] = new Series { Name = category, Data = new Data(objct.ToArray()) };
                counter++;

                string yAxis = "Permit Rate";
                string title = "Permit Rate";
                title += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

                string tooltip = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
                //ViewBag.ApplicationChart = _chartHelper.LineChart(series, groups, title, yAxis, "AppChart", tooltip);
            }

            #endregion

            #region Combined
            List<object> obj = new List<object>();
            // Series[] seriesCom = new Series[1];
            //int sum = 0;
            //string grp = string.Empty;
            for (var i = 0; i < groups.Count; i++)
            {
                int sum = permits.Where(a => a.date_issued.ToShortDateString() == groups[i]).Count();
                obj.Add(sum);
            }
            //seriesCom[0] = new Series { Name = "All Applications", Data = new Data(obj.ToArray()) };

            string yAxisCom = "Permit Rate";
            string titleCom = "Combined Permit Rate";
            titleCom += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            //ViewBag.CombinedChart = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "CombinedChart", tooltipCom);
            #endregion

            ViewBag.Counter = arm;

            return View();
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
                var app = _context.applications.Where(C => C.PhaseId == category.id && C.DeleteStatus != true).ToList();

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
            ViewBag.PieChart = _chartHelper.pieChart(dt, title, name, "Chart1", tooltip);
            #endregion


            return View();
        }

        public IActionResult ApplicationCountReport(string Id = "general")
        {
            List<Categories> categories = _context.Categories.Where(x => x.DeleteStatus != true).ToList();
            //  List<Application> allApplication = new List<Application>();
            if (!string.IsNullOrEmpty(Id) && Id.ToLower().Contains("general"))
            {
                categories = categories.FindAll(C => C.name.ToLower().Contains("general"));
            }
            else if (!string.IsNullOrEmpty(Id) && Id.ToLower().Contains("major"))
            {
                categories = categories.FindAll(C => C.name.ToLower().Contains("major"));
            }
            else if (!string.IsNullOrEmpty(Id) && Id.ToLower().Contains("special"))
            {
                categories = categories.FindAll(C => C.name.ToLower().Contains("special"));
            }



            foreach (var item in categories)
            {
                int Appcount = _context.applications.Where(C => C.category_id == item.id && C.DeleteStatus != true && C.submitted == true).Count();
                int PermitCount = _context.permits.Where(C => C.categoryName.Trim().ToLower().Contains(item.name.Trim().ToLower())).Count();


                item.ApplicationCount = Appcount;
                item.PermitCount = PermitCount;


                //services.AddRange(_serviceRep.Where(C => C.Category_Id == item.Id).ToList());
            }


            return View();
            //return View(new AppCountModel { Categories = categories, Services = services, Specifications = specifications });
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
    }

}










//var paidInvoices = (from ap in _context.applications.AsEnumerable()
//                    join iv in _context.invoices.AsEnumerable() on ap.id equals iv.application_id
//                    join fac in _context.Facilities.AsEnumerable() on ap.FacilityId equals fac.Id
//                    join phs in _context.Phases.AsEnumerable() on ap.PhaseId equals phs.id
//                    join ad in _context.addresses on fac.AddressId equals ad.id
//                    join st in _context.States_UT.AsEnumerable() on ad.StateId equals st.State_id
//                    where ap.DeleteStatus != true && iv.status.ToLower() == "paid"
//                    group ap by new
//                    {
//                        phs.name,
//                        phs.id,
//                        st.StateName
//                    }
//                            into g
//                    select new PaymentReportModel
//                    {
//                        Category = g.Key.name,
//                        FG = FGTotal.ToString(),
//                        ProcessingFee = ProcessingFeeTotal.ToString(),
//                        NMDPRAIGR = IGRTotal.ToString(),
//                        NMDPRACORC = "Not Applicable",
//                        Contractor = "Not Applicable",
//                        Amount = Convert.ToDouble(g.Where(p => p.PhaseId == g.Key.id).Sum(x => x.fee_payable)),
//                        TotalAmount = Convert.ToDouble(g.Where(p => p.PhaseId == g.Key.id).Sum(x => x.fee_payable)),
//                        Fee = g.Where(p => p.PhaseId == g.Key.id).Sum(x => x.fee_payable),
//                        Charge = g.Where(p => p.PhaseId == g.Key.id).Sum(x => x.service_charge),
//                        StateName = g.Key.StateName,
//                        trimStatutoryFee = g.Key..PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Statutory Fee:")).Split(Environment.NewLine.ToCharArray())[0];
//trimProcessingFee = app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("Processing Fee:"));
//trimIGRFee = app.PaymentDescription.Contains("IGR") ? app.PaymentDescription.Substring(app.PaymentDescription.LastIndexOf("IGR Fee:")) : null;


       // }).FirstOrDefault();
