using Newtonsoft.Json;
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
using System.Globalization;
using NewDepot.Models.Stored_Procedures;

namespace NewDepot.Controllers
{
    [Authorize]
    public class ATCController : Controller
    {
        private readonly Depot_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;

        ElpsResponse elpsResponse = new ElpsResponse();
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;

        public ATCController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        public IActionResult Index()
        {
            return View();
        }

        // GET: ATC
        public IActionResult Application(int id, int phaseId, string permitId, int? lg)
        {
        
                try
                {
                    var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                    var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                    int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                    var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();

                    var ps = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();

                    var cat = _context.Categories.Where(a => a.id == ps.category_id).FirstOrDefault();
                    SuitabilityInspections suitability = null;
                    int appid = 0;
                    suitability = _context.SuitabilityInspections.Where(c => c.ApplicationId == id || c.FacilityId == id).FirstOrDefault();
                    var app = _context.applications.Where(c => c.id == id && c.DeleteStatus!=true).FirstOrDefault();

                    if (comp != null && app!=null)
                    {
                        var tnks = _context.Tanks.Where(a => a.FacilityId == suitability.FacilityId && !a.Decommissioned).ToList();
                        var totalVol = tnks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                        
                        #region
                       // var appl = _helpersController.RecordApplication(ps, totalVol, tnks.Count, comp.id, suitability.FacilityId, "New", permitId, CompanyEmail, null);// = new Application();

                        
                        
                        var atc = new ATCs()
                        {
                            ApplicationId = app.id,
                            CompanyId = comp.id,
                            SuitabilityId = suitability.Id,
                            FacilityId = suitability.FacilityId
                        };

                        _context.ATCs.Add(atc);
                        _context.SaveChanges();

                        appid = app.id;
                        //Create Invoice before redirecting
                        var invo = new invoices();
                        invo.amount = Convert.ToDouble(app.fee_payable + app.service_charge);
                        invo.application_id = app.id;
                        invo.payment_code = app.reference;
                        invo.payment_type = string.Empty;
                        invo.status = app.fee_payable > 0 ? "Unpaid" : "Paid";
                        invo.date_added = DateTime.Now;
                        invo.date_paid = DateTime.Now.AddDays(-7);

                        _context.invoices.Add(invo);
                        _context.SaveChanges();

                        return RedirectToAction("UploadApplicationDocument", "Application", new { id =generalClass.Encrypt( app.id.ToString() ) });

                        #endregion
                    }

                    throw new ArgumentException();
                }
                catch (Exception x)
                {
                    _helpersController.LogMessages(x.ToString());
                    ViewBag.ErrorMessage = "An error occured while processing this request. Please try again.";

                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("An Error occured while initiating your application. Please try again.") });
                }
            }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ApplicationId</param>
        /// <param name="phaseId"></param>
        /// <param name="permitId">Permit Id: Optional</param>
        /// <param name="leg">Legacy: Optional</param>
        /// <param name="mtype">Modification Type, Incase of modification: Optional</param>
        /// <returns></returns>
       // //[Authorize(Roles = "Company")]
        public IActionResult TankInspection(int id, int phaseId, string permitId, string leg, string mtype, string continuee=null)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var comp = _context.companies.Where(a => a.user_id == userEmail).FirstOrDefault();
            if (comp == null)
            {
     return RedirectToAction("Errorr", "Home", new { message = "Sorry, company could not be found." });
            }
            var fetchSuitID = new permits();
            var fac = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
            if (fac != null)
            {

                if (phaseId == 2 && continuee != null) //continuation of ATC application so fetch suitability ref for facility.
                {
                     fetchSuitID = (from p in _context.permits
                                       join a in _context.applications on p.application_id equals a.id
                                       join ph in _context.Phases on a.PhaseId equals ph.id
                                       where a.FacilityId == fac.Id && ph.id == 1
                                       select p).FirstOrDefault();
                }



                //Each Stations should have atleast 3 thanks
                ViewBag.tnks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned).ToList();
                ViewBag.legacy = leg;
                ViewBag.facility = fac;
                ViewBag.phaseId = phaseId.ToString();
                ViewBag.permitNo = permitId!= null ? permitId.ToString(): fetchSuitID.permit_no.ToString();
                ViewBag.ModificationType = mtype;
                ViewBag.products = _context.Products.ToList();
                return View();

            }
            
       return RedirectToAction("Errorr", "Home", new { message = "Sorry, facility could not be found." });
        }


        public IActionResult AllProducts(int id)
        {
            ViewBag.id = id;
            return View(_context.Products.ToList());
        }

        public IActionResult FacilityTanks(int id, int fid)
        {
            ViewBag.id = id;
            return View(_context.Tanks.Where(a => a.FacilityId == fid && !a.Decommissioned).ToList());
        }

        public IActionResult TankTest0001(int id, int phaseId)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userEmail == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again") });
            }
            try
            {
                var application = _context.applications.Where(a => a.id == id).FirstOrDefault();
                var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                var comp = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
                var ps = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();
                var cat = _context.Categories.Where(a => a.id == ps.category_id).FirstOrDefault();
                if (comp != null)
                {
                    
                        try
                        {
                            #region
                            var tnks = _context.Tanks.Where(a => a.FacilityId == application.FacilityId).ToList();
                            var pmps = _context.Pumps.Where(a => a.FacilityId == application.FacilityId).ToList();

                            var totalVol = tnks.Sum(a => Convert.ToInt32(a.MaxCapacity));
                            var fee = 0.00m;
                            #region fee

                            if (ps.PriceByVolume)
                            {
                                fee +=_helpersController.GetPricePerVolume(totalVol);
                            }
                            else
                            {
                                fee +=(decimal)ps.Price;
                            }
                            if (ps.ProcessingFeeByTank)
                            {
                                fee += ps.ProcessingFee * tnks.Count;
                            }
                            else
                            {
                                fee += ps.ProcessingFee;
                            }
                            #endregion
                            //var appl = new Application()
                            //{
                            //    Category_Id = cat.Id,
                            //    Company_Id = comp.Id,
                            //    Current_Desk = 0,
                            //    Date_Added = DateTime.Now,
                            //    Date_Modified = DateTime.Now,
                            //    Fee_Payable = fee,// GetPricePerVolume.Amount(totalVol, cat.Name, "New"),// ps.Price,
                            //    Reference = PaymentRef.RefrenceCode(),
                            //    Service_Charge = ps.ServiceCharge,
                            //    Status = ApplicationStatus.PaymentPending,
                            //    Year = DateTime.Now.Year,
                            //    //FacilityId = tankTest.FacilityId,
                            //    Type = "New",
                            //    PhaseId = ps.Id,
                            //    AllowPush = true
                            //};

                            //_appRep.Add(appl);
                            //_appRep.Save(userEmail, Request.UserHostAddress);

                            //var atc = new ATC()
                            //{
                            //    ApplicationId = appl.Id,
                            //    CompanyId = comp.Id,
                            //    SuitabilityId = tankTest.Id,
                            //    FacilityId = tankTest.FacilityId
                            //};
                            //_atcRep.Add(atc);
                            //_atcRep.Save(userEmail, Request.UserHostAddress);

                            //appid = appl.Id;

                            //trans.Complete();

                            //return RedirectToAction("UploadApplicationDocument", "Application", new { id = appl.Id });
                            //return RedirectToAction("Payment", "Application", new { id = appl.Id, refcode = appl.reference });

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ex.Message) });
                        }
                    
                }
                string err = "Sorry, company could not be found. ";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(err) });
            }
        }

        public IActionResult Removetank(string id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userEmail == "Error" || userID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, your session has expired. Please login again") });
            }

            int tid = 0;
            if (int.TryParse(id, out tid))
            {
                var tnk = _context.Tanks.Where(a => a.Id == tid && !a.Decommissioned).FirstOrDefault();
               
                
                if (tnk != null)
                {
                    string tankName = tnk.Name;
                    string tankCapacity = tnk.MaxCapacity;
                    var facility = _context.Facilities.Where(x => x.Id == tnk.FacilityId).FirstOrDefault();
                    var company = _context.companies.Where(x => x.id == facility.CompanyId).FirstOrDefault();


                    _context.Tanks.Remove(tnk);
                     int rmv= _context.SaveChanges();
                    if (rmv > 0) {
                        _helpersController.LogMessages(userEmail + " deleted tank (" + tankName + "," + tankCapacity + ") for " + company.name + "'s" + facility.Name + " facility.");
                    }
                    return Content("0");
                }
                return Content("2");
            }
            else
            {
                return Content("1");
            }
        }

    }
}