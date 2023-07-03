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
    public class PaymentController : Controller
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

        public PaymentController(IHostingEnvironment hostingEnvironment, Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }

        //
        // GET: /Index/
        public ActionResult Index()
        {
            return View();
        }
        
        
        public ActionResult RemitaList()
        {
            if (TempData["Alert"] != null)
            {
                ViewBag.Alert = TempData["Alert"];
                TempData.Clear();
            }
            return View();
        }

        
        public ActionResult LazyLoadRemitaQuery(JQueryDataTableParamModel param)
        {
            //var allPayments = _context.remita_transactions.ToList();
            var allPayments = (from u in _context.remita_transactions
                               select new remita_transactions
                               {
                                   transaction_date=u.transaction_date!=null ? u.transaction_date : "",
                                   RRR=u.RRR!=null ? u.RRR : "",
                                   customer_name=u.customer_name!=null ? u.customer_name : "",
                                   approved_amount=u.approved_amount!=null ? u.approved_amount : "",
                                   reference_number=u.reference_number!=null ? u.reference_number : "",
                                   type=u.type!=null ? u.type : "",
                                   response_description=u.response_description!=null ? u.response_description : "",
                                   id=u.id
                               }).ToList();

            IEnumerable<remita_transactions> filteredPayments;
            var sortColumnIndex = 0 + 1;

            Func<remita_transactions, string> orderingFunction = (c => sortColumnIndex == 1 ? c.customer_name : sortColumnIndex == 2 ?
                c.transaction_date : sortColumnIndex == 3 ?
                c.approved_amount : sortColumnIndex == 4 ? c.reference_number : sortColumnIndex == 5 ? c.RRR : sortColumnIndex == 6 ?
                c.type : c.response_description);

            //var sortDirection = Request["sSortDir_0"]; // asc or desc
            var sortDirection = "asc"; // asc or desc
            List<remita_transactions> displayedTransactions = new List<remita_transactions>();

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var s = param.sSearch.ToLower();
                filteredPayments = allPayments.Where(C => C.transaction_date.Trim().ToLower().Contains(s) ||
                        C.type.Trim().ToLower().Contains(s) || C.reference_number.Trim().ToLower().Contains(s) ||
                        C.approved_amount.Trim().ToLower().Contains(s) || C.customer_name.Trim().ToLower().Contains(s) ||
                        C.response_description.Trim().ToLower().Contains(s) || C.RRR.Trim().ToLower().Contains(s));

                if (sortDirection == "asc")
                {
                    filteredPayments = filteredPayments.OrderBy(orderingFunction);
                }
                else
                {
                    filteredPayments = filteredPayments.OrderByDescending(orderingFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredPayments = allPayments.OrderBy(orderingFunction);
                }
                else
                {
                    filteredPayments = allPayments.OrderByDescending(orderingFunction);
                }
            }

            displayedTransactions = filteredPayments.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var result = from c in displayedTransactions.OrderByDescending(x => x.transaction_date)
                         select new[] { c.customer_name, c.transaction_date, c.approved_amount, c.reference_number, c.RRR, c.type, c.response_description, Convert.ToString(c.id) };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allPayments.Count(),
                iTotalDisplayRecords = filteredPayments.Count(),
                aaData = result
            });

        }

        public ActionResult PaymentStatus()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PaymentStatus(string orderid)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (!string.IsNullOrEmpty(orderid))
            {
                    try
                    {
                        var paramDatas = _restService.parameterData("Id", orderid);

                        _helpersController.LogMessages("Checking if generated RRR has already been paid. RRR => " + orderid, userEmail);

                        var rrrCheck = _restService.Response("/api/Payments/BankPaymentInfo/{CompId}/{email}/{apiHash}/" + orderid, paramDatas, "GET");

                        var resp = JsonConvert.DeserializeObject<RemitaResponse>(rrrCheck.Content);


                        return View(resp);
                    }
                    catch (Exception x)
                    {
                        var alert = new AlertBox()
                        {
                            ButtonType = AlertType.Failure,
                            Message = "An error occured while processing your request. Please try again.", //+ x.InnerException == null? x.Message : x.InnerException.InnerException == null ? x.InnerException.Message : x.InnerException.InnerException.Message,
                            Title = "Check Failed"
                        };
                        ViewBag.Alert = alert;
                    }
                }
            
            else
            {
                var alert = new AlertBox()
                {
                    ButtonType = AlertType.Failure,
                    Message = "Invalid Application reference number supplied",
                    Title = "Check Failed"
                };
                ViewBag.Alert = alert;
            }
            return View();
        }

        //[Authorize(Roles = ("Admin,NMDPRAAdmin, Account,Support"))]
        public ActionResult GiveValue()
        {
            if (TempData["Alert"] != null)
            {
                ViewBag.Alert = TempData["Alert"];
                TempData.Clear();
            }
            return View();
        }

        [HttpPost]
        public ActionResult GiveValue(string refnos)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var app = _context.applications.Where(a => a.reference == refnos).FirstOrDefault();
            if (app == null)
            {
                //return Content($"Application with Ref No:{ refnos} does not Exist");
                var alert = new AlertBox()
                {
                    ButtonType = AlertType.Warning,
                    Message = "Application with reference number " + refnos + " cannot be found. Please check and try again.",
                    Title = "Manual Value Not Successful"
                };
                ViewBag.Alert = alert;
            }
            else
            {
                var comp = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();
                var manual = _context.ManualRemitaValues.Where(a => a.RRR.Contains(app.reference)).FirstOrDefault();
                if (manual == null)
                {
                    manual = new ManualRemitaValues()
                    {
                        RRR = "",
                        AddedBy = userEmail,
                        Beneficiary = "NMDPRA",
                        Company = comp.name,
                        DateAdded = DateTime.Now,
                        FundingBank = "",
                        NetAmount = Convert.ToDouble(app.service_charge + app.fee_payable),
                        PaymentSource = "Bank",
                        Status = ""
                    };
                    _context.ManualRemitaValues.Add(manual);
                    _context.SaveChanges();
                    _helpersController.LogMessages(userName + "(" + userEmail + ") gave manual remitta value to application with reference:" + app.reference + " for company " + comp.name, userEmail);

                }

                var trans = _context.remita_transactions.Where(a => a.reference_number == app.reference).FirstOrDefault();
                if (trans == null)
                {
                    trans = new remita_transactions()
                    {
                        approved_amount = manual.NetAmount.ToString(),
                        customer_id = app.company_id,
                        customer_name = comp.name,
                        online_reference = "NMDPRA-Bank-M",
                        order_id = app.reference,
                        PaymentSource = "Bank",
                        payment_reference = "NMDPRA-Bank-M",
                        query_date = DateTime.Now.AddYears(-10),
                        reference_number = app.reference,
                        response_code = "01",
                        response_description = "Payment Completed",
                        RRR = "NMDPRA-Bank-M",
                        transaction_amount = manual.NetAmount.ToString(),
                        transaction_currency = "566",
                        transaction_date = DateTime.Now.ToString(),
                        type = "Offline"
                    };
                    _context.remita_transactions.Add(trans);
                }
                else
                {
                    trans.response_code = "01";
                    trans.response_description = "Payment Completed";
                }
                _context.SaveChanges();
                _helpersController.LogMessages(userName + "(" + userEmail + ") updated remita transaction table with manual remitta value for application with reference:" + app.reference + " for company " + comp.name, userEmail);

                var ap = _context.applications.Where(a => a.reference == trans.order_id).FirstOrDefault();

                if (ap != null)
                {
                    ap.status = "Payment Completed";
                    _context.SaveChanges();
                }
                //Call elps to do same

                var alert = new AlertBox()
                {
                    ButtonType = AlertType.Success,
                    Message = "Value given to application with reference number " + app.reference + " successfully",
                    Title = "Addition of manual value successful."
                };
                ViewBag.Alert = alert;
            }
            return View();
        }

        public IActionResult FindToGiveValue(string refno, string coyname)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var apps = new List<MyApps>();
            if (!string.IsNullOrEmpty(refno))
            {
                //Find by Reference Number
                var myApp = _helpersController.ApplicationDetails();
                var aps = myApp.Where(x => x.Reference == refno);
                apps.AddRange(aps);


            }
            else if (!string.IsNullOrEmpty(coyname))
            {
                var coys = _context.companies.Where(a => a.name.ToLower().Contains(coyname.ToLower())).ToList();
                foreach (var c in coys)
                {
                    if (c != null)
                    {
                        var myApp = _helpersController.ApplicationDetails();
                        var aps = myApp.Where(x => x.Company_Id == c.id);
                        if (aps.ToList().Count > 0)
                        {

                            apps.AddRange(aps);
                        }
                    }
                }
            }
            _helpersController.LogMessages(userName + "(" + userEmail + ") finding applications to give manual remitta value for application with reference:" + refno + " for company " + coyname, userEmail);

            return View(apps.Where(ap=> ap.Status== GeneralClass.PaymentPending));
        }
        //Revenue Report

        //public async Task<ActionResult> ApplicationReport()
        //{
        //   // var applications = await FilterApplications();

        //    return View(applications);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ApplicationReport(ApplicationReportViewModel model)
        //{
        //    var applications = await FilterApplications();

        //    if (!string.IsNullOrEmpty(model.StartDate) && !string.IsNullOrEmpty(model.EndDate))
        //        applications = applications.Where(x => x.Date >= model.Start && x.Date <= model.End).ToList();

        //    if (model.ApplicationType != null)
        //        applications =
        //            applications.Where(x => model.ApplicationType.Contains(x.StorageCapacity.Substring(0, 3))).ToList();

        //    if (model.State != null)
        //        applications = applications.Where(x => model.State.Contains(x.Facility.StateCode)).ToList();

        //    return View(applications);
        //}

        //public async Task<IActionResult> LicenseReport()
        //{
        //    var licenses = await FilterLicenses();

        //    return View(licenses);
        //}
        //[HttpPost]
        //public async Task<IActionResult> LicenseReport(ApplicationReportViewModel model)
        //{
        //    var licenses = await FilterLicenses();

        //    if (!string.IsNullOrEmpty(model.StartDate) && !string.IsNullOrEmpty(model.EndDate))
        //        licenses = licenses.Where(x => x.DateIssued >= model.Start && x.DateIssued <= model.End).ToList();

        //    if (model.ApplicationType != null)
        //        licenses =
        //            licenses.Where(x => model.ApplicationType.Contains(x.Application.StorageCapacity.Substring(0, 3))).ToList();

        //    if (model.State != null)
        //        licenses = licenses.Where(x => model.State.Contains(x.Application.Facility.StateCode)).ToList();

        //    return View(licenses);
        //}

        //public async Task<IActionResult> PaymentReport()
        //{
        //    var payments = await FilterPayment();

        //    ViewData["ToTal"] = payments.Sum(x => x.Amount).ToString("N2");

        //    return View(payments);
        //}

        //[HttpPost]
        //public async Task<IActionResult> PaymentReport(ApplicationReportViewModel model)
        //{
        //    var payments = await FilterPayment();

        //    if (!string.IsNullOrEmpty(model.StartDate) && !string.IsNullOrEmpty(model.EndDate))
        //        payments = payments.Where(x => x.TransactionDate >= model.Start && x.TransactionDate <= model.End).ToList();

        //    if (model.ApplicationType != null)
        //        payments =
        //            payments.Where(x => model.ApplicationType.Contains(x.Application.StorageCapacity.Substring(0, 3))).ToList();

        //    if (model.State != null)
        //        payments = payments.Where(x => model.State.Contains(x.Application.Facility.StateCode)).ToList();
        //    ViewData["ToTal"] = payments.Sum(x => x.Amount).ToString("N2");

        //    return View(payments);
        //}

        //private async Task<List<Application>> FilterApplications()
        //{
        //    var user = await _userManager.WhereEmailAsync(User.Identity.Name);
        //    var location = _context.FieldLocations.FirstOrDefault(x => x.Id == user.LLocation);
        //    var excluded = new[]
        //        {"Application Initiated", "Deleted", "Payment pending", "Payment Confirmed", "Documents Uploaded"};
        //    var applications = _application.GetAll()
        //        .Where(x => !excluded.Contains(x.Status)).ToList();

        //    if (location.FieldType.Equals("FD"))
        //    {
        //        var state = _context.States_UT.FirstOrDefault(x => x.FieldLocationId == user.LLocation);
        //        applications = applications.Where(x => x.Facility.StateCode.Equals(state.Code)).ToList();
        //    }
        //    else if (location.FieldType.Equals("ZN"))
        //    {
        //        var zonefields = _context.States_UT.Join(_context.ZoneFieldMappings, state => state.FieldLocationId,
        //                zone => zone.ZoneFieldId, (state, zone) => new {
        //                    zone.ZoneFieldId,
        //                    zone.FieldLocationId,
        //                    _context.States_UT.FirstOrDefault(x => x.FieldLocationId == zone.FieldLocationId).Code,
        //                    _context.States_UT.FirstOrDefault(x => x.FieldLocationId == zone.FieldLocationId).Name
        //                })
        //            .Where(x => x.ZoneFieldId == user.LLocation).ToList();
        //        applications = applications.Where(x => zonefields.Any(y => y.Code.Equals(x.Facility.StateCode))).ToList();
        //    }

        //    return applications;
        //}

        //private async Task<List<Permit>> FilterLicenses()
        //{

        //    var licenses = _permit.All();
        //    try
        //    {
        //        var user = await _userManager.WhereEmailAsync(User.Identity.Name);
        //        var location = _context.FieldLocations.FirstOrDefault(x => x.Id == user.LLocation);

        //        if (location.FieldType.Equals("FD"))
        //        {
        //            var state = _context.States_UT.FirstOrDefault(x => x.FieldLocationId == user.LLocation);
        //            licenses = licenses.Where(x => x.Application.Facility.StateCode.Equals(state.Code)).ToList();
        //        }
        //        else if (location.FieldType.Equals("ZN"))
        //        {
        //            var zonefields = _context.States_UT.Join(_context.ZoneFieldMappings, state => state.FieldLocationId,
        //                    zone => zone.ZoneFieldId, (state, zone) => new {
        //                        zone.ZoneFieldId,
        //                        zone.FieldLocationId,
        //                        _context.States_UT.FirstOrDefault(x => x.FieldLocationId == zone.FieldLocationId).Code,
        //                        _context.States_UT.FirstOrDefault(x => x.FieldLocationId == zone.FieldLocationId).Name
        //                    })
        //                .Where(x => x.ZoneFieldId == user.LLocation).ToList();
        //            licenses = licenses.Where(x => zonefields.Any(y => y.Code.Equals(x.Application.Facility.StateCode))).ToList();
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return licenses;
        //}

        //private async Task<List<PaymentLog>> FilterPayment()
        //{
        //    try
        //    {
        //        var payments = _payment.GetAll();
        //        payments = payments.Where(x =>
        //            x.Status.Equals("approved", StringComparison.OrdinalIgnoreCase)
        //            || x.Status.Equals("confirmed", StringComparison.OrdinalIgnoreCase)).ToList();

        //        var user = await _userManager.WhereEmailAsync(User.Identity.Name);
        //        var location = _context.FieldLocations.FirstOrDefault(x => x.Id == user.LLocation);

        //        if (location.FieldType.Equals("FD"))
        //        {
        //            var state = _context.States_UT.FirstOrDefault(x => x.FieldLocationId == user.LLocation);
        //            payments = payments.Where(x => x.Application.Facility.StateCode.Equals(state.Code)).ToList();
        //        }
        //        else if (location.FieldType.Equals("ZN"))
        //        {
        //            var zonefields = _context.States_UT.Join(_context.ZoneFieldMappings, state => state.FieldLocationId,
        //                    zone => zone.ZoneFieldId, (state, zone) => new {
        //                        zone.ZoneFieldId,
        //                        zone.FieldLocationId,
        //                        _context.States_UT.FirstOrDefault(x => x.FieldLocationId == zone.FieldLocationId).Code,
        //                        _context.States_UT.FirstOrDefault(x => x.FieldLocationId == zone.FieldLocationId).Name
        //                    })
        //                .Where(x => x.ZoneFieldId == user.LLocation).ToList();
        //            payments = payments.Where(x => zonefields.Any(y => y.Code.Equals(x.Application.Facility.StateCode))).ToList();
        //        }

        //        return payments;

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return null;
        //}

        //public IActionResult GetStaffDetail(string email) => Json(new { staff = _elps.GetStaff(email) });

        [HttpPost]
        public ActionResult ExtraPayment(string returnUrl, ApplicationExtraPaymentsModel model,decimal TransferCost)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));


            var ap = _context.applications.Where(a => a.id == model.ApplicationId).FirstOrDefault();
            if (ap == null)
            {
                TempData["Report"] = "The application ID supplied is no longer available or has been removed";
                return Redirect(returnUrl);
            }
            var exPay = new ApplicationExtraPayments();

            try
            {
                var coy = _context.companies.Where(a => a.id == ap.company_id).FirstOrDefault();
                var facility = _context.Facilities.Where(a => a.Id == ap.FacilityId).FirstOrDefault();
                var facAdd = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                var category = _context.Categories.Where(a => a.id == ap.category_id).FirstOrDefault();
                var phase = _context.Phases.Where(a => a.id == ap.PhaseId).FirstOrDefault();

                if (coy == null)
                {
                    TempData["Report"] = "Company not found. Please check and try again.";
                    return Redirect(returnUrl);
                }
                ViewBag.Company = coy;
                var checkExtra = _context.ApplicationExtraPayments.Where(x => x.ApplicationId == ap.id && x.Amount == model.Amount && x.Type == model.Type && x.Status == GeneralClass.PaymentPending);

                if (checkExtra.Count() > 0)
                {
                    string result = "An extra payment has already been generated for this application with the same amount and type.";
                    exPay = checkExtra.FirstOrDefault();

                    TempData["Report"] = result;
                    return Redirect(returnUrl);
                }
                else
                {

                    //UtilityHelper.LogMessages($"Extra Pay for :: {coy.Name}");
                    #region Create ExtraPay & Invoice
                    var refno = PaymentRef.RefrenceCode();
                   
                    refno = model.Type.Substring(0, 1) + refno.Substring(0, refno.Length - 1);

                        exPay.Amount = Math.Round(model.Amount, 2);
                        exPay.ApplicationId = ap.id;
                        exPay.Reference = refno.ToLower();
                        exPay.Type = model.Type;
                        exPay.UserName = userEmail;
                        exPay.Date = DateTime.Now;
                        exPay.Status = GeneralClass.PaymentPending;
                        exPay.Comment = model.Comment != null ? model.Comment : model.Comment_TO;
                        
                    _context.ApplicationExtraPayments.Add(exPay);
                    _context.SaveChanges();

                    _helpersController.LogMessages(userName + "(" + userEmail + ") added extra payment for application with reference:" + ap.reference + "(" + coy.name + ")", userEmail);

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
                    _helpersController.LogMessages(userName + "(" + userEmail + ") added invoice to extra payment for application with reference:" + ap.reference + "(" + coy.name + ")", userEmail);

                    #endregion


                    #region Remita Payment Split Builder
                    var amt = exPay.Amount.ToString().Split('.');
                    var amnt = amt.Count() > 1 ? exPay.Amount.ToString() : $"{amt[0]}.00";
                    var rmSplit = new LpgLicense.Models.RemitaSplit();
                    rmSplit.payerPhone = coy.contact_phone;
                    rmSplit.orderId = exPay.Reference;
                    rmSplit.CategoryName = category.name;
                    rmSplit.payerEmail = coy.CompanyEmail;
                    rmSplit.payerName = coy.name;
                    rmSplit.ReturnBankPaymentUrl = _configuration.GetSection("AmountSetting").GetSection("ReturnBankPaymentUrl").Value.ToString();
                    rmSplit.ReturnFailureUrl = Url.Action("ExtraPaymentFail", "Applications", new { id = exPay.Reference }, Request.Scheme);
                    rmSplit.ReturnSuccessUrl = Url.Action("ExtraPaymentSuccess", "Applications", new { id = exPay.Reference }, Request.Scheme);
                    rmSplit.ServiceCharge = "0";
                    rmSplit.totalAmount = exPay.Amount.ToString();
                    rmSplit.Amount = rmSplit.totalAmount;
                    //rmSplit.ServiceTypeId = model.serviceTypeId;

                   
                    var address = (from ad in _context.addresses
                                   join s in _context.States on ad.StateId equals s.Id
                                   where ad.id == facility.AddressId
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
                    var appOffice = _helpersController.GetApplicationOffice(ap.id).FirstOrDefault();

                    fields.Add(new CustomField
                    {
                        Name = "Field/Zonal Office",
                        Value = appOffice.OfficeName,
                        Type = "ALL"
                    });
                  
                    rmSplit.CustomFields = fields;

                    rmSplit.lineItems = _helpersController.BuildPartners(ap, rmSplit, "yes",Convert.ToDecimal(rmSplit.totalAmount),TransferCost);
                    var jn = JsonConvert.SerializeObject(rmSplit);

                    _helpersController.LogMessages(userName + " initiated extra payment split for application with reference:" + ap.reference + "(" + coy.name + ")", userEmail);
                    #endregion

                    #region Extra Payment API
                    try
                    {
                        _helpersController.LogMessages("Done generating remita extra payment for application with ref "+ap.reference +"("+ exPay.Reference + "), Posting to remita", userEmail);
                        var paramDatas = _restService.parameterData("CompId", coy.elps_id.ToString());
                        var response = _restService.Response("/api/Payments/ExtraPayment/{CompId}/{email}/{apiHash}", paramDatas, "POST", rmSplit);
                        
                        var resz = JsonConvert.DeserializeObject<JObject>(response.Content);
                        var resp = JsonConvert.DeserializeObject<Payment.PrePaymentResponse>(resz.ToString());

                        _helpersController.LogMessages($"Extra Pay Split :: {resz.ToString()}", userEmail);

                        #endregion
                        if (resp == null || string.IsNullOrEmpty(resp.RRR))
                        {
                            _context.ApplicationExtraPayments.Remove(exPay);
                            _context.SaveChanges();
                            _helpersController.LogMessages($"Extra payment output from Remita:: {resz.ToString()}", userEmail);
                          return RedirectToAction("Errorr", "Home", new { message = "Error occured while generating RRR. Please try again" });
                        }


                        if (TransferCost > 0)
                        {
                            //Post payment information to IGR
                            var RevenueItemId = _configuration.GetSection("RemitaSplit").GetSection("RevenueId").Value.ToString();

                                var revenueItems = new List<RevenueItem>
                              {

                                     new RevenueItem 
                                     {
                                       RevenueItemId = int.Parse( RevenueItemId),          
                                       //RevenueItemId = 12290,
                                       Amount = Convert.ToInt32(rmSplit.IGRFee),
                                       Quantity = 1 
                                     }

                             };

                            var revItems = JsonConvert.SerializeObject(revenueItems);

                            var IGR_URL = _configuration.GetSection("RemitaSplit").GetSection("IGR_URL").Value.ToString();

                            var igr = _helpersController.PostReferenceToIGR(IGR_URL.ToString(), "/api/addpayments", new

                            {

                                RevenueItems = revItems,

                                RRR = resp.RRR.Trim(),

                                ExternalPaymentReference = ap.reference,

                                State = address.StateName,

                                Address = address.Address,

                                CompanyName = coy.name,

                                Phone = facility.ContactNumber,

                                CompanyEmail = coy.CompanyEmail

                            });



                            if (!string.IsNullOrEmpty(igr.ToString()) && igr.ToString().ToLower().Contains("success"))

                            {
                                var newDesc = ap.PaymentDescription + "|";
                                var nIGRFee =int.Parse(rmSplit.IGRFee);

                                //update IGR fee and transfer cost column
                                var trimIGRFee = ap.PaymentDescription.ToLower().Contains("igr fee") ? ap.PaymentDescription.Substring(ap.PaymentDescription.LastIndexOf("IGR Fee:")) : null;

                                if (trimIGRFee != null)
                                {
                                    newDesc = ap.PaymentDescription.Replace(trimIGRFee, "");

                                    int IGRFee = string.IsNullOrEmpty(trimIGRFee) ? 0 : Convert.ToInt32(trimIGRFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray()));
                                     nIGRFee = IGRFee + int.Parse(rmSplit.IGRFee);
                                }
                                rmSplit.IGRFee = nIGRFee.ToString();
                                
                                ap.TransferCost= TransferCost;
                                ap.PaymentDescription = newDesc +"IGR Fee: N" + rmSplit.IGRFee;

                                _context.SaveChanges();

                            }

                            else

                            {
                                _context.ApplicationExtraPayments.Remove(exPay);
                                _context.SaveChanges();
                                return RedirectToAction("Errorr", "Home", new { message = "An error occured while posting payment information to IGR, please contact support." });

                            }
                        }



                        ViewBag.rrr = resp.RRR.Trim();
                        ViewBag.webPayData = rmSplit;


                        #region
                        //Create Payment Transaction
                        var ptrans = new remita_transactions();
                        ptrans.approved_amount = exPay.Amount.ToString();
                        ptrans.customer_id = ap.company_id;
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

                        _context.remita_transactions.Add(ptrans);
                        _context.SaveChanges();


                        exPay.RRR = resp.RRR;
                        _context.SaveChanges();

                        _helpersController.LogMessages("Extra payment table updated with RRR: " + exPay.RRR, userEmail);
                        #endregion

                        TempData["Report"] = "New RRR (" + ptrans.RRR + ") generated successfully for company: " + coy.name + "; Facility: " + facility.Name;
                        ViewBag.type = "pass";


                        #region Send Extra Payment E-Mail To Company
                        string subject = $"Extra Payment Generated For {model.Type}";

                        var tk = string.Format($"An Extra Payment RRR: {ptrans.RRR} has been generated for your application with reference number: {ap.reference} (" + category.name + " (" + phase.name + ")" + "): " +
                            "<br /><ul><li>Amount Generated: {0}</li><li>Payment Type: {1}</li><li>Remita RRR: {2}</li>" +
                            "<li>Payment Status: {3}</li><li>Payment Comment: {4}</li>" +
                            "<li>Facility Name: {5}</li><li>Facility Address: {6}</li> <br/>" +
                            "<p>Kindly note that your application will be pending until this payment is completed. </p>",
                            exPay.Amount.ToString("N2"), exPay.Type, ptrans.RRR, exPay.Status, exPay.Comment, facility.Name, facAdd.address_1 + " ," + facAdd.city);


                        var emailMsg = _helpersController.SaveMessage(ap.id, coy.id, subject, tk, coy.elps_id.ToString(), "Company");
                        var sendEmail = _helpersController.SendEmailMessage(coy.CompanyEmail.ToString(), coy.name, emailMsg, null);
                        #endregion

                    }
                    catch (Exception x)
                    {
                        _helpersController.LogMessages($"Extra payment Error Remita:: {x.Message.ToString()}", userEmail);
                        TempData["Report"] = "Error occured while generating RRR. Please try again.";
                        return Redirect(returnUrl);
                    }

                   

                }
            }
            catch (Exception ex)
            {
                _helpersController.LogMessages(ex.Message.ToString(), userEmail);
                TempData["Report"] = "An error occured while trying to generate extra payment RRR for this application.";
                ViewBag.type = "warn";
            }



            return Redirect(returnUrl);

        }

        [Authorize(Policy = "CompanyRoles")]
        public IActionResult ExtraPaymentSuccess(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }

            var apps = _context.ApplicationExtraPayments.Where(x => x.Reference == id);

            if (apps.Count() > 0)
            {
                var getApp = _context.applications.Where(x => x.id == apps.FirstOrDefault().Id);

                apps.FirstOrDefault().Status = GeneralClass.PaymentCompleted;
                apps.FirstOrDefault().DatePaid = DateTime.Now.ToString();
                //apps.FirstOrDefault().Type = "Online";
                _context.SaveChanges();

                var company = from a in _context.applications
                              join c in _context.companies on a.company_id equals c.id
                              where a.DeleteStatus == false && c.DeleteStatus == false && a.id == apps.FirstOrDefault().ApplicationId
                              select new
                              {
                                  c,
                                  a
                              };

                ViewData["AppRef"] = getApp.FirstOrDefault().reference;

                var phase = _context.Phases.Where(a => a.id == company.FirstOrDefault().a.PhaseId).FirstOrDefault();

                // Saving Messages
                string subject = phase.name + " Application Extra Payment Success : " + company.FirstOrDefault().a.reference;
                string content = "Extra payment made successfully for your application (" + phase.name + ") with reference number: " + company.FirstOrDefault().a.reference + " and payment RRR " + apps.FirstOrDefault().RRR + " for processing on DEPOT portal. Kindly find application details below.";
                
                string staffEmail = apps.FirstOrDefault().UserName.ToLower().Split("@")[0];
               
                var processor = _context.Staff.Where(a => a.StaffEmail.ToLower().Contains( staffEmail)).FirstOrDefault();
               _helpersController.SaveHistory(apps.FirstOrDefault().ApplicationId,processor.StaffID, "Payment", "Extra payment for application paid successfully with Reference RRR : " + apps.FirstOrDefault().RRR);

                // Send Mail to Company

                var emailMsg = _helpersController.SaveMessage(getApp.FirstOrDefault().id, (int)company.FirstOrDefault().a.company_id, subject, content, company.FirstOrDefault().c.elps_id.ToString(), "Company");

                var sendEmail = _helpersController.SendEmailMessage(company.FirstOrDefault().c.CompanyEmail.ToString(), company.FirstOrDefault().c.name, emailMsg, null);

               
                _helpersController.LogMessages("Extra payment for application paid successfully with Reference RRR : " + apps.FirstOrDefault().RRR, _helpersController.getSessionEmail());

                return View();
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or have been deleted. Kindly contact support.") });
            }
        }


        /*
         * Remita Extra Payment Failure action 
         * 
         * id => extra payment reference number from remita
         */
        [Authorize(Policy = "CompanyRoles")]
        public IActionResult ExtraPaymentFail(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }

            var apps = _context.ApplicationExtraPayments.Where(x => x.Reference == id);

            if (apps.Count() > 0)
            {
                var getApp = _context.applications.Where(x => x.id == apps.FirstOrDefault().Id);

                apps.FirstOrDefault().Status = GeneralClass.PaymentCompleted;
                apps.FirstOrDefault().DatePaid = DateTime.Now.ToString();
                //apps.FirstOrDefault().Type = "Online";
                _context.SaveChanges();

                var company = from a in _context.applications
                              join c in _context.companies on a.company_id equals c.id
                              where a.DeleteStatus == false && c.DeleteStatus == false && a.id == apps.FirstOrDefault().ApplicationId
                              select new
                              {
                                  c,
                                  a
                              };

                ViewData["AppRef"] = getApp.FirstOrDefault().reference;

                var phase = _context.Phases.Where(a => a.id == company.FirstOrDefault().a.PhaseId).FirstOrDefault();

                // Saving Messages
                string subject = phase.name + " Application Extra Payment Failure : " + company.FirstOrDefault().a.reference;
                string content = "Extra payment failed for your application (" + phase.name + ") with reference number: " + company.FirstOrDefault().a.reference + " and payment RRR " + apps.FirstOrDefault().RRR + " for processing on DEPOT portal. Kindly find application details below.";
                string staffEmail = apps.FirstOrDefault().UserName.ToLower().Split("@")[0];
                var processor = _context.Staff.Where(a => a.StaffEmail.ToLower().Contains(staffEmail)).FirstOrDefault();

                // Send Mail to Company
                var emailMsg = _helpersController.SaveMessage(getApp.FirstOrDefault().id, (int)company.FirstOrDefault().a.company_id, subject, content, company.FirstOrDefault().c.elps_id.ToString(), "Company");
                var sendEmail = _helpersController.SendEmailMessage(company.FirstOrDefault().c.CompanyEmail.ToString(), company.FirstOrDefault().c.name, emailMsg, null);


                _helpersController.LogMessages("Extra payment for application failed with Reference RRR : " + apps.FirstOrDefault().RRR, _helpersController.getSessionEmail());

                return View();
            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or have been deleted. Kindly contact support.") });
            }
        }


        //[Authorize(Policy = "ProcessingStaffRoles")]
        public JsonResult DeleteExtraPay(string ExtraPayID)
        {
            try
            {
                string result = "";

                if (string.IsNullOrWhiteSpace(ExtraPayID))
                {
                    result = "Error";
                }

                int exPayid = 0;

                var exPay_id = generalClass.Decrypt(ExtraPayID);

                if (exPay_id == "Error")
                {
                    result = "Error";
                }
                else
                {
                    exPayid = Convert.ToInt32(exPay_id);

                    var pay = _context.ApplicationExtraPayments.Where(x => x.Id == exPayid && x.Status == GeneralClass.PaymentPending);

                    if (pay.Count() > 0)
                    {
                        string staffEmail = pay.FirstOrDefault().UserName.ToLower().Split("@")[0];

                        var processor = _context.Staff.Where(a => a.StaffEmail.ToLower().Contains(staffEmail)).FirstOrDefault();
                        _helpersController.SaveHistory(pay.FirstOrDefault().ApplicationId, processor.StaffID, "Payment", "Extra payment for application was deleted with Reference RRR : " + pay.FirstOrDefault().RRR);
                            _context.ApplicationExtraPayments.Remove(pay.FirstOrDefault());

                            if (_context.SaveChanges() > 0)
                                result = "Extra Deleted";
                      
                    }
                    else
                    {
                        result = "Sorry, extra payment was not found or has been paid for.";
                    }
                }

                _helpersController.LogMessages("Removing extra payment status : " + result + ". Extra pay ID : " + exPayid, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please contact support");
            }
        }

    }
}
