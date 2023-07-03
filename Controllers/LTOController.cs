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
using System.Globalization;
//using Microsoft.Win32.TaskScheduler;

namespace NewDepot.Controllers
{
    [Authorize]
    public class LTOController : Controller
    {

        private readonly Depot_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;

        ElpsResponse elpsResponse = new ElpsResponse();
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;


        public LTOController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"> ApplictionId</param>
        /// <param name="phaseId">Phase at which the Application was Made</param>
        /// <param name="permitId">permitId is Supplied in the Case of Legacy application</param>
        /// <returns></returns>
        //ATC id of the Facility is used here
        public IActionResult Application(int id, int phaseId, string permitId, string frmlegacy)
        {

            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));



            var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();


            var app = _context.applications.Where(a => a.id == id).FirstOrDefault();
            if (!string.IsNullOrEmpty(frmlegacy))
            {
                var tt = _context.TankLeakTests.Where(a => a.FacilityId == id).FirstOrDefault();
                if (tt != null)
                {

                }
            }
            if (app != null)
            {
                //at this  point we are also surpposed to check if the facility has been inspected by the NMDPRA Officers.

                var fac = _context.Facilities.Where(a => a.Id == app.FacilityId).FirstOrDefault();
                if (fac != null)
                {

                    if (fac.CompanyId == comp.id)
                    {

                        //Each Stations should have atleast 3 thanks
                        var ph = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();
                        if (ph != null)
                        {
                            ViewBag.PhaseName = ph.name;
                        }
                        ViewBag.tnks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned).ToList();
                        ViewBag.facility = fac;
                        ViewBag.phaseId = phaseId;
                        ViewBag.permitNo = permitId;
                        ViewBag.frmlegacy = frmlegacy;
                        ViewBag.products = _context.Products.ToList();
                        return View();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Company Id on the Facility provided is Quite different from your Facility, Please review and Try again";

                    }
                }

                ViewBag.errorMessage = "We could not find the Application or Facility with this Id, review your application and Try again";

            }
            else
            {
                ViewBag.errorMessage = "We could not find the Application or Facility with this Id, review your application and Try again";

            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ViewBag.errorMessage) });
        }

        public IActionResult AddTanks(int id, string permitId, string category, string phaseId, string sid)
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.Err = TempData["ErrorMessage"];
            }
            try
            {

                var cid = int.Parse(category);
                var pid = int.Parse(phaseId);
                var phs = _context.Phases.Where(a => a.category_id == cid && a.id == pid).FirstOrDefault();
                var permit = (from p in _context.permits
                              join a in _context.applications on p.application_id equals a.id
                              where p.permit_no.ToLower() == permitId.ToLower()
                              select a).FirstOrDefault();

                var fac = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();

                if (permit != null && fac == null)
                {
                    fac = _context.Facilities.Where(a => a.Id == permit.FacilityId).FirstOrDefault();
                }
                if (fac != null)
                {
                    //then check if the facility is even for the current applicant and it is not a takeover application
                    if (fac.CompanyId != userID && phs.ShortName != "TO")
                    {

                        string ErrorMessag = "Sorry, the facility attached to the supplied permit is not yours.";
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessag) });
                    }



                    ViewBag.FacilityName = fac.Name;
                    RenewModel renew = new RenewModel();
                    permits pm = null;
                    if (!string.IsNullOrEmpty(permitId))
                    {
                        pm = _context.permits.Where(a => a.permit_no.ToLower() == permitId.ToLower()).FirstOrDefault();//  new permits { PhaseId = phs.Id, Permit_No = permitId };
                        if (pm != null)
                        {
                            //check if current application is payment of sanction for Facility Upgrade Without Approval (UWA)
                            if (phs != null && phs.ShortName == "UWA")
                            {
                                return RedirectToAction("ReviewFacilityTank", "Application", new { id = fac.Id, permitId = permitId, category = category, phaseId = phaseId, leg = 1 });

                            }
                        }
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
                    var tnks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned && (a.Status == null || a.Status.Contains("Approved"))).ToList();

                    if (tnks.Count > 0)
                    {
                        renew.Tanks = tnks;
                    }
                    if (!string.IsNullOrEmpty(category))
                    {
                        ViewBag.review = "yes";
                    }

                    if (phs != null)
                    {
                        phaseId = phs.id.ToString();
                        ViewBag.PhaseName = phs.name;
                    }

                    _helpersController.LogMessages($"Number of Tanks:: {renew.Tanks.Count} and Products:: {renew.Products.Count} inside addTanks for {renew.Company.name}");
                    ViewBag.SanctionId = sid;
                    ViewBag.category = category;// phs.category_id;
                    ViewBag.phaseId = phaseId;// phs.Id;
                    return View(renew);//"Renew",
                }
                ViewBag.ErrorMessage = "No Depot is Assocciated with the provided License Number.";
                string ErrorMessage = ViewBag.ErrorMessage;
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
            }
            catch (Exception x)
            {
                _helpersController.LogMessages("Add Tanks Error :: " + x.ToString());

                string ErrorMessage = "Sorry Some Error Occured while Handling Your Request";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
            }
        }

        [HttpPost]
        public IActionResult AddTanks(List<Tanks> tnks, OtherModel om, string modificationType, string[] Parameters)
        {
            try
            {
                int facilityID = om.facilityId != null ? om.facilityId : tnks.FirstOrDefault().FacilityId;

                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                List<Tanks> apTanks = new List<Tanks>();
                List<int?> productIDs = new List<int?>();

                if (tnks.Count > 0)
                {
                    List<String> checkTP = new List<String>();
                    tnks.ForEach(tk =>
                    {
                        var prd = _context.Products.Where(a => a.Id == tk.ProductId).FirstOrDefault().Name;
                        if (!tk.Name.ToLower().Contains(prd.ToLower()))
                        {
                            tk.Name = prd.ToUpper() + " " + tk.Name;
                        }
                        //break;
                    });
                    var getCategory = _context.Phases.Where(ph => ph.id == om.phaseId).FirstOrDefault();
                    if (checkTP.Count() > 0)
                    {
                        return RedirectToAction("AddTanks", "LTO", new { id = facilityID, permitId = om.PermitNo, Category = getCategory.category_id, phaseId = om.phaseId });
                    }

                    #region check selected tanks
                    int selectedTnkCount = 0;
                    foreach (var tnk in tnks.Where(x => x.AppTank == "Yes"))
                    {
                        selectedTnkCount += 1;
                        apTanks.Add(tnk);
                    }

                    if (apTanks.Count <= 0)
                    {
                        foreach (var tnk in tnks)
                        {
                            apTanks.Add(tnk);
                        }
                    }
                    //check app category

                    int isRenewal = 0;
                    if (getCategory != null)
                    {
                        isRenewal = getCategory.id;
                    }
                    #endregion
                    if (selectedTnkCount > 0 || isRenewal > 0 || modificationType == "" || modificationType == "undefined")

                    {

                        var comp = _context.companies.Where(a => a.CompanyEmail == userEmail).FirstOrDefault();
                        if (comp != null)
                        {
                            var AppTanks = new List<Tanks>();
                            Tanks tnk = null;
                            tnk = new Tanks();
                            string convert = "";
                            string convert2 = "";

                            foreach (var item in apTanks)
                            {

                                if (!string.IsNullOrEmpty(item.Name))
                                {
                                    if (item.Id > 0)
                                    {
                                        tnk = _context.Tanks.Where(a => a.Id == item.Id).FirstOrDefault();
                                        if (tnk != null)
                                        {
                                            var tankName = tnk.Name;
                                            tnk.HasATG = string.IsNullOrEmpty(item.HasATG.ToString()) ? false : true;

                                            //tnk.MaxCapacity = item.MaxCapacity; //don't overwrite these two ok
                                            //tnk.Name = item.Name;

                                            tnk.NewTankDetails = !item.Name.ToLower().Contains(tnk.Name.ToLower()) ? item.Name + "|" + item.ProductId + "|" + item.MaxCapacity : null;
                                            tnk.Position = "Surface Tank";

                                            //check if product was changed and that its modification of type Conversion
                                            if (!string.IsNullOrEmpty(modificationType) && !string.IsNullOrEmpty(item.AppTank))
                                            {
                                                tnk.ModifyType = modificationType;
                                                if (modificationType == "Conversion")
                                                {
                                                    var prodIds = new List<int> { (int)tnk.ProductId, (int)item.ProductId };
                                                    var products = _context.Products.Where(a => prodIds.Contains(a.Id)).ToList();
                                                    var oldProd = products.Where(a => a.Id == tnk.ProductId).FirstOrDefault();
                                                    var NewProd = products.Where(a => a.Id == item.ProductId).FirstOrDefault();
                                                    if (oldProd != null && NewProd != null)
                                                    {
                                                        string cmsg = $"{oldProd.Name} ({tankName}) to {NewProd.Name}";
                                                        if (convert == "")
                                                        {
                                                            convert2 = cmsg;
                                                        }
                                                        convert = string.IsNullOrEmpty(convert2) ? cmsg : convert2 != cmsg? $"{cmsg}, {convert2}"  : cmsg;
                                                    }
                                                }
                                            }

                                            tnk.Diameter = item.Diameter;
                                            tnk.Height = item.Height;

                                            //Do not change facility tanks yet until modification is approved
                                            if (modificationType != "" && modificationType != "undefined")
                                            {
                                                tnk.AppTank = "Yes";
                                                tnk.Status = "Processing";
                                            }

                                            _context.SaveChanges();

                                        }
                                    }
                                    else
                                    {
                                        var tnkk = new Tanks();
                                        tnkk.FacilityId = facilityID;
                                        tnkk.CompanyId = comp.id;
                                        tnkk.HasATG = item.HasATG ? true : true;
                                        tnkk.MaxCapacity = item.MaxCapacity;
                                        tnkk.Name = item.Name;
                                        tnkk.Position = "Surface Tank";// item.Position; Conversion 
                                        tnkk.Diameter = item.Diameter;
                                        tnkk.Height = item.Height;
                                        tnkk.Decommissioned = false;

                                        tnkk.AppTank = "Yes";
                                        _context.Tanks.Add(tnkk);
                                        tnk = tnkk;
                                        int d = _context.SaveChanges();
                                        if (om.phaseId == 9 || (!string.IsNullOrEmpty(modificationType) && !string.IsNullOrEmpty(item.AppTank)))
                                        {
                                            tnkk.Status = "Processing";
                                            tnkk.ModifyType = modificationType;


                                            if (modificationType == "Conversion")
                                            {
                                                var prodIds = new List<int> { (int)tnk.ProductId, (int)item.ProductId };
                                                var products = _context.Products.Where(a => prodIds.Contains(a.Id)).ToList();
                                                var oldProd = products.Where(a => a.Id == tnk.ProductId).FirstOrDefault();
                                                var NewProd = products.Where(a => a.Id == item.ProductId).FirstOrDefault();
                                                if (oldProd != null && NewProd != null)
                                                {
                                                    convert = string.IsNullOrEmpty(convert) ? $"{oldProd.Name} to {NewProd.Name}" : $" and {oldProd.Name} to {NewProd.Name}";
                                                }
                                            }
                                            else
                                            {
                                                tnkk.ProductId = item.ProductId;
                                            }
                                            _context.SaveChanges();

                                        }


                                    }
                                    productIDs.Add(item.ProductId);
                                    if (!string.IsNullOrEmpty(item.Name))
                                    {

                                        //tnk.ProductId = item.ProductId;
                                        AppTanks.Add(tnk);

                                    }
                                }


                            }


                            TempData["PermitNumber"] = om.PermitNo;
                            TempData["category"] = om.category;
                            TempData["frmLegacy"] = om.frmLegacy;
                            if (!string.IsNullOrEmpty(om.ModificationType))
                            {
                                TempData["ModificationType"] = om.ModificationType;

                            }
                            if (!string.IsNullOrEmpty(om.review) && om.review == "Renew")
                            {
                                TempData["AppType"] = "renew";

                            }
                            var app = _context.applications.Where(a => a.FacilityId == facilityID && a.PhaseId == om.phaseId && a.company_id == comp.id && a.status == GeneralClass.PaymentPending && a.DeleteStatus != true).FirstOrDefault();
                            if (app != null)
                            {
                                var ph = _context.Phases.Where(a => a.id == om.phaseId).FirstOrDefault();
                                var totalVol = AppTanks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                                var _leg = om.PermitNo != null ? _context.Legacies.Where(a => a.LicenseNo.ToLower() == om.PermitNo.ToLower()).FirstOrDefault() : null;
                                bool frmATCc = false;
                                //check legacy table
                                if (_leg != null)
                                {
                                    if (_leg.AppType == "ATC")
                                        frmATCc = true;

                                }
                                var fd = _helpersController.CalculateAppFee(ph, om.PermitNo, totalVol, AppTanks.Count, frmATCc);
                                //update app table with newly generated app fee
                                app.fee_payable = Math.Round(fd.Fee);
                                app.PaymentDescription = fd.FeeDescription;
                                _context.SaveChanges();

                                UpdateAppTanks(AppTanks, app.id, productIDs);
                                if (om.phaseId == 5 || om.phaseId == 9)
                                {
                                    tnk.Status += "|" + app.id.ToString(); _context.SaveChanges();
                                    RecordTankModification(modificationType, convert, om.phaseId, app);//record or update existing application
                                }

                                return RedirectToAction("AddPumps", new { id = facilityID, phaseId = om.phaseId });
                            }
                            else
                            {
                                //create application and Record the Tanks for which the Application
                                var ph = _context.Phases.Where(a => a.id == om.phaseId).FirstOrDefault();
                                var totalVol = AppTanks.Sum(a => Convert.ToDouble(a.MaxCapacity));

                                var AppType = "new";
                                if (om.phaseId == 11 || om.phaseId == 7)
                                {
                                    AppType = "renew";
                                }
                                bool frmATC = false;
                                if (om.category == "4")
                                {

                                    var license = _context.permits.Where(a => a.permit_no.ToLower() == om.PermitNo.ToLower()).FirstOrDefault();
                                    var cat = _context.Categories.Where(a => a.id.ToString() == om.category).FirstOrDefault();
                                    var phs = _context.Phases.Where(a => a.id == om.phaseId).FirstOrDefault();
                                    if (license == null)
                                    {

                                        var _leg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == om.PermitNo.ToLower()).FirstOrDefault();

                                        //check legacy table
                                        if (_leg != null)
                                        {
                                            if (_leg.AppType == "ATC")
                                                frmATC = true;

                                        }
                                    }
                                    else
                                    {
                                        if (phs.name == "Approval To Construct")
                                            frmATC = true;

                                    }

                                }

                                totalVol = apTanks.Sum(a => Convert.ToDouble(a.MaxCapacity)); // check tank capacity volume to ensure correct capacity for payment

                                app = _helpersController.RecordApplication(ph, totalVol, AppTanks.Count, tnk.CompanyId, tnk.FacilityId, AppType, om.PermitNo, userEmail, userEmail, frmATC);// = new Application();
                                UpdateAppTanks(AppTanks, app.id, productIDs);
                            }


                            if (om.phaseId == 5 || om.phaseId == 9)
                            {
                                //this will cover for Tanks Modification and Tanks Recalibration
                                tnk.Status += "|" + app.id.ToString(); _context.SaveChanges();
                                RecordTankModification(modificationType, convert, om.phaseId, app);
                            }


                            if (om.category == "5")
                            {
                                if (om.phaseId == 8 || om.phaseId == 9)
                                {
                                    return RedirectToAction("PaySanction", new { id = facilityID, cat = om.category, phaseId = om.phaseId });
                                }
                            }


                            if (om.phaseId == 2)
                            {
                                var pm = _context.permits.Where(a => a.permit_no.ToLower() == om.PermitNo.ToLower()).FirstOrDefault();
                                if (pm != null)
                                {
                                    return RedirectToAction("Application", "ATC", new { id = pm.application_id, phaseId = om.phaseId });
                                }
                            }

                            return RedirectToAction("AddPumps", new { id = facilityID, phaseId = om.phaseId });

                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "You have not made any selection for this application";

                    }

                }
            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.Message.ToString());
                string Error = "Sorry, an error occured while processing your request, please try again.";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(Error) });
            }

            string ErrorMessage = "Sorry, Some Error Occured while Processing your request, Please try again";
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
        }


        private void UpdateAppTanks(List<Tanks> tnks, int id, List<int?> prodIDs)
        {
            int x = 0;
            var ats = _context.ApplicationTanks.Where(a => a.ApplicationId == id).ToList();
            foreach (var item in ats)
            {
                var t = tnks.Where(a => a.Id == item.TankId).FirstOrDefault();
                if (t == null)
                {
                    _context.ApplicationTanks.Remove(item);
                }
            }
            foreach (var item in tnks)
            {

                if (item.NewTankDetails != null)
                {
                    string tName = item.NewTankDetails.Split("|")[0];
                    string pId = item.NewTankDetails.Split("|")[1];
                    string tCapacity = item.NewTankDetails.Split("|")[2];

                    item.Name = tName;
                    item.ProductId = Convert.ToInt16(pId);
                    if (prodIDs != null && prodIDs.Count() > 0)
                    {
                        item.ProductId = prodIDs[x];
                    }
                    item.MaxCapacity = tCapacity;
                }


                var at = ats.Where(a => a.TankId == item.Id).FirstOrDefault();
                if (at != null)
                {
                    at.Capacity = Convert.ToDouble(item.MaxCapacity);
                    if (prodIDs != null && prodIDs.Count() > 0)
                    {
                        at.ProductId = Convert.ToInt16(prodIDs[x]);
                        x++;
                    }

                }
                else
                {
                    at = new ApplicationTanks();
                    at.ApplicationId = id;
                    at.Capacity = Convert.ToDouble(item.MaxCapacity);
                    at.CompanyId = item.CompanyId;
                    at.Date = DateTime.UtcNow.AddHours(1);
                    at.FacilityId = item.FacilityId;
                    at.ProductId = prodIDs[x] != null? Convert.ToInt16(prodIDs[x]) : (int)item.ProductId;
                    at.TankId = item.Id;
                    at.TankName = item.Name;
                    _context.ApplicationTanks.Add(at);
                    x++;
                }
            }
            _context.SaveChanges();
        }

        private void RecordTankModification(string modificationType, string convert, int phaseId, applications app)
        {
            try
            {

                var fm = _context.FacilityModifications.Where(a => a.ApplicationId == app.id).FirstOrDefault();
                //Create the Facility Modification
                if (fm == null)
                {

                    fm = new FacilityModifications();
                    fm.ApplicationId = app.id;
                    fm.Date = DateTime.Now;
                    fm.FacilityId = (int)app.FacilityId;
                    fm.PhaseId = phaseId;
                    fm.Type = modificationType;
                    fm.PrevProduct = convert;
                    _context.FacilityModifications.Add(fm);
                    _context.SaveChanges();
                }
                else
                {

                    fm.Type = modificationType;
                    fm.PrevProduct = convert;
                    _context.SaveChanges();

                }
            }
            catch (Exception)
            {

                throw;
            }


        }
        [Authorize(Policy = "AdminRoles")]
        public IActionResult UpdateAppTank(string Id)
        {

            //check temp message
            if (TempData["messageAE"] != null)
            {
                ViewBag.Err = TempData["messageAE"];
            }
            else if (TempData["messageAS"] != null)
            {
                ViewBag.Success = TempData["messageAS"];
            }

            int id = generalClass.DecryptIDs(Id);

            var ap = _context.applications.Where(a => a.id == id).FirstOrDefault();
            if (ap != null)
            {
                var model = new RenewModel();

                model.Tanks = _context.Tanks.Where(a => a.FacilityId == ap.FacilityId && a.DeletedStatus != true).ToList();
                model.AppTanks = _context.ApplicationTanks.Where(a => a.FacilityId == ap.FacilityId && a.ApplicationId == id).ToList();
                model.Products = _context.Products.ToList();
                ViewBag.phaseId = ap.PhaseId.ToString();
                ViewBag.ApplicationId = ap.id;
                ViewBag.Ref = ap.reference;

                var facMod = _context.FacilityModifications.Where(a => a.ApplicationId == ap.id).FirstOrDefault();
                if (facMod != null)
                {
                    string prv = "";
                    if (!string.IsNullOrEmpty(facMod.PrevProduct))
                    {
                        prv = $"Conversion from {facMod.PrevProduct}";
                    }

                    //ViewBag.facModification = $"Modification type: {facMod.Type}";
                    ViewBag.facModification = facMod.Type;
                }
                return View(model);
            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("This application does not exist") });

        }
        //[HttpPost, Authorize(Roles = "Admin,Support")]
        [HttpPost]
        public IActionResult UpdateAppTank(List<Tanks> tnks, int ApplicationId, string ModifyType)
        {
            var ap = _context.applications.Where(a => a.id == ApplicationId).FirstOrDefault();
            int count = 0; 
            string convertMsg = "";
            string convertMsg2 = "";

            if (ap != null)
            {
                try
                {
                    var AppTanks = new List<ApplicationTanks>();
                    ApplicationTanks tnk = null;
                    int i = 0;
                    List<string> tanksConvert = new List<string>();
                    var facMod = _context.FacilityModifications.Where(a =>a.ApplicationId == ApplicationId).FirstOrDefault();
                    var app = _context.applications.Where(a => a.DeleteStatus != true && a.id == ApplicationId).FirstOrDefault();

                    if (facMod == null)
                    {
                        var fm = new FacilityModifications();
                        fm.ApplicationId = app.id;
                        fm.Date = DateTime.Now;
                        fm.FacilityId = (int)app.FacilityId;
                        fm.PhaseId = app.PhaseId;
                        fm.Type = ModifyType;
                        fm.PrevProduct = "";
                        _context.FacilityModifications.Add(fm);
                        _context.SaveChanges();
                        facMod = fm;
                    }
                    foreach (var item in tnks)
                    {

                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            if (item.Id > 0)
                            {
                                tnk = _context.ApplicationTanks.Where(a => a.TankId == item.Id && a.ApplicationId == ApplicationId).FirstOrDefault();
                                var factnk = _context.Tanks.Where(a => a.Id == item.Id).FirstOrDefault();
                                if (tnk != null)
                                {
                                    //check if tank has been deselected i.e removed
                                    if (string.IsNullOrEmpty(item.AppTank))
                                    {
                                        _context.ApplicationTanks.Remove(tnk); i = _context.SaveChanges();
                                    }
                                    else
                                    {
                                        //tnk.HasATG = item.HasATG ? true : false;
                                        if (Convert.ToDouble(tnk.Capacity) != Convert.ToDouble(item.MaxCapacity))
                                        {
                                            item.ModifyType = "Upgrade";
                                        }

                                        //Now check if modification table has previous tank name
                                        //var checkMod = facMod.Where(x => x.PrevProduct.Contains(factnk.Name)).FirstOrDefault();
                                        if (facMod != null)
                                        {
                                            var oldprd = _context.Products.Where(a => a.Id == factnk.ProductId).FirstOrDefault()?.Name;
                                            var newprd = _context.Products.Where(a => a.Id == item.ProductId).FirstOrDefault()?.Name;
                                            facMod.Type = ModifyType;
                                            count++;
                                            string cmsg = $"{oldprd} ({factnk.Name}) to {newprd}";
                                            if (convertMsg == "")
                                            {
                                                convertMsg2 = cmsg;
                                            }
                                            convertMsg = string.IsNullOrEmpty(convertMsg2) ? cmsg : convertMsg2 != cmsg ? $"{cmsg}, {convertMsg2}" : cmsg;
                                            facMod.PrevProduct = convertMsg;

                                        }

                                        //change in app tanks
                                        tnk.Capacity = Convert.ToDouble(item.MaxCapacity);
                                        tnk.TankName = item.Name;
                                        if (ModifyType != null && ModifyType.ToLower() != "conversion")
                                        {
                                            factnk.ProductId = (int)item.ProductId;
                                            factnk.Name = item.Name;
                                            factnk.MaxCapacity = item.MaxCapacity;
                                            factnk.UpdatedAt = DateTime.Now;
                                        }

                                        tnk.ProductId = (int)item.ProductId;
                                        tnk.UpdatedAt = DateTime.Now;
                                        //change in facility tanks
                                        i = _context.SaveChanges();

                                    }
                                }
                                else //add it to application tank(s)
                                {
                                    if (!string.IsNullOrEmpty(item.AppTank))
                                    {

                                        tnk = new ApplicationTanks();
                                        tnk.Capacity = Convert.ToDouble(item.MaxCapacity);
                                        tnk.TankName = item.Name;
                                        tnk.ProductId = (int)item.ProductId;
                                        tnk.ApplicationId = ap.id;
                                        tnk.FacilityId = (int)ap.FacilityId;
                                        tnk.TankId = item.Id;
                                        tnk.CompanyId = ap.company_id;
                                        tnk.Date = DateTime.Now;
                                        _context.ApplicationTanks.Add(tnk);
                                        i = _context.SaveChanges();

                                    }
                                }
                            }
                            else
                            {
                                //tank doesn't exist at all so add to both facility tanks and app tanks table
                                var tnkk = new Tanks();
                                tnkk.FacilityId = (int)ap.FacilityId;
                                tnkk.CompanyId = ap.company_id;
                                tnkk.HasATG = item.HasATG ? true : false;
                                tnkk.MaxCapacity = item.MaxCapacity;
                                tnkk.Name = item.Name;
                                tnkk.DeletedStatus = false;
                                tnkk.Status = ap.status == GeneralClass.Approved ? "Approved" : "Processing|" + ap.id.ToString();
                                tnkk.Position = "Surface Tank";
                                tnkk.ProductId = item.ProductId;
                                tnkk.Diameter = item.Diameter;
                                tnkk.Height = item.Height;
                                tnkk.Decommissioned = false;
                                _context.Tanks.Add(tnkk);
                                i = _context.SaveChanges();
                                if (i > 0)
                                {
                                    tnk = new ApplicationTanks();
                                    tnk.Capacity = Convert.ToDouble(item.MaxCapacity);
                                    tnk.TankName = item.Name;
                                    tnk.ProductId = (int)item.ProductId;
                                    tnk.ApplicationId = ap.id;
                                    tnk.FacilityId = (int)ap.FacilityId;
                                    tnk.TankId = item.Id;
                                    tnk.CompanyId = ap.company_id;
                                    tnk.Date = DateTime.Now;
                                    _context.ApplicationTanks.Add(tnk);
                                    i = _context.SaveChanges();

                                }
                            }

                            if (!string.IsNullOrEmpty(item.AppTank))
                            {
                                AppTanks.Add(tnk);
                                i = _context.SaveChanges();

                            }
                        }


                    }


                    var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                    if (i > 0)
                    {
                        TempData["messageAS"] = "Application tank(s) modification was successful";
                        TempData["message"] = "Application tank(s) modification was successful";
                        TempData["msgType"] = "success";
                        _helpersController.LogMessages("Updated application tanks table for application with ref:" + ap.reference, userEmail);

                    }

                    return RedirectToAction("ViewApplication", "Application", new { id = ap.id });
                }
                catch (Exception x)
                {
                    TempData["messageAE"] = "An error occured while modifying this application tank(s).";
                    return RedirectToAction("UpdateAppTank", "LTO", new { id = generalClass.Encrypt(ap.id.ToString()) });
                }
            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("This application does not exist") });
        }
        [Authorize(Policy = "AdminRoles")]
        public IActionResult UpdateFacilityTank(string Id, string appId)
        {
            //check temp message
            if (TempData["messageE"] != null)
            {
                ViewBag.Err = TempData["messageE"];
            }
            else if (TempData["messageS"] != null)
            {
                ViewBag.Success = TempData["messageS"];
            }
            int id = generalClass.DecryptIDs(Id);
            int appid = generalClass.DecryptIDs(appId);

            var fac = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
            if (fac != null)
            {
                var model = new RenewModel();

                model.Tanks = _context.Tanks.Where(a => a.FacilityId == fac.Id && a.DeletedStatus != true).ToList();
                model.Products = _context.Products.ToList();
                ViewBag.FacilityName = fac.Name.ToString();
                ViewBag.FacilityId = fac.Id;
                ViewBag.AppId = appid;

                return View(model);
            }
            ViewBag.Error = "This facility does not exist";
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("This facility does not exist") });

        }
        // [HttpPost, Authorize(Roles = "Admin,Support")]
        [HttpPost]
        public IActionResult UpdateFacilityTank(List<Tanks> tnks, int facilityId, int appId)
        {
            var fac = _context.Facilities.Where(a => a.Id == facilityId).FirstOrDefault();
            if (fac != null)
            {
                try
                {
                    var AppTanks = new List<Tanks>();
                    Tanks tnk = null;
                    int i = 0;

                    foreach (var item in tnks)
                    {

                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            if (item.Id > 0)
                            {
                                tnk = _context.Tanks.Where(a => a.Id == item.Id).FirstOrDefault();
                                var aptnk = _context.ApplicationTanks.Where(a => a.TankId == item.Id).ToList();

                                if (tnk != null)
                                {

                                    //tnk.HasATG = item.HasATG ? true : false;
                                    if (Convert.ToDouble(tnk.MaxCapacity) != Convert.ToDouble(item.MaxCapacity))
                                    {
                                        item.ModifyType = "Upgrade";
                                    }
                                    tnk.MaxCapacity = item.MaxCapacity;
                                    tnk.Name = item.Name;
                                    tnk.ProductId = item.ProductId;
                                    tnk.Position = item.Position;
                                    tnk.Diameter = item.Diameter;
                                    tnk.Height = item.Height;
                                    tnk.UpdatedAt = DateTime.Now;
                                    i = _context.SaveChanges();

                                    //update app tanks
                                    if (aptnk.Count() > 0)
                                    {
                                        aptnk.ForEach(t => {

                                            t.Capacity = Convert.ToDouble(item.MaxCapacity);
                                            t.TankName = item.Name;
                                            t.ProductId = (int)item.ProductId;
                                            t.UpdatedAt = DateTime.Now;
                                            i = _context.SaveChanges();

                                        });
                                    }
                                }
                            }
                            else
                            {

                                tnk = new Tanks();
                                tnk.FacilityId = (int)fac.Id;
                                tnk.CompanyId = fac.CompanyId;
                                tnk.HasATG = item.HasATG ? true : false;
                                tnk.MaxCapacity = item.MaxCapacity;
                                tnk.Name = item.Name;
                                tnk.DeletedStatus = false;
                                tnk.Position = "Surface Tank";
                                tnk.ProductId = item.ProductId;
                                tnk.Diameter = item.Diameter;
                                tnk.Height = item.Height;
                                tnk.Decommissioned = false;
                                _context.Tanks.Add(tnk);
                                i = _context.SaveChanges();
                            }

                            if (!string.IsNullOrEmpty(item.AppTank))
                            {
                                AppTanks.Add(tnk);
                            }
                        }


                    }
                    if (i > 0)
                    {
                        TempData["messageS"] = "Facility tank(s) modification was successful";
                    }
                    var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

                    _helpersController.LogMessages("Updated tanks table for facility with name:" + fac.Name, userEmail);
                    return RedirectToAction("ViewApplication", "Application", new { id = appId });
                }
                catch (Exception x)
                {
                    TempData["messageE"] = "An error occured while modifying this facility tank(s).";
                    return RedirectToAction("UpdateFacilityTank", "LTO", new { Id = generalClass.Encrypt(fac.Id.ToString()) });

                }
            }

            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("This application does not Exist") });
        }

        public JsonResult DeleteFacilityTank(string TankID)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            string result = ""; string fName = "";
            var tnks = _context.Tanks.Where(a => a.Id == Convert.ToInt32(TankID)).FirstOrDefault();
            if (tnks != null)
            {
                //check app tanks to delete tank
                var ats = _context.ApplicationTanks.Where(a => a.TankId == Convert.ToInt32(TankID)).FirstOrDefault();
                if (ats != null)
                {
                    _context.ApplicationTanks.Remove(ats);
                    _context.SaveChanges();
                }
                string tName = tnks.Name;
                var fac = _context.Facilities.Where(a => a.Id == tnks.FacilityId).FirstOrDefault();
                fName = fac != null ? fac.Name : "";
                _context.Tanks.Remove(tnks);
                if (_context.SaveChanges() > 0)
                {
                    result = "Tank Removed";
                    _helpersController.LogMessages("Deletion of tank with name for facility:" + fName, userEmail);

                }
                else
                {
                    result = "An error occured while trying to delete this tank.";
                }
            }
            return Json(result);
        }
        public IActionResult AddPumps(int id, int phaseId)
        {
            if (TempData["PermitNumber"] != null)
            {
                ViewBag.PermitNumber = TempData["PermitNumber"];

            }
            if (TempData["AppType"] != null)
            {

                ViewBag.AppType = TempData["AppType"];

            }
            if (TempData["category"] != null)
            {
                ViewBag.category = TempData["category"];
            }
            if (TempData["frmLegacy"] != null)
            {
                ViewBag.frmLegacy = TempData["frmLegacy"];
            }
            if (TempData["ModificationType"] != null)
            {
                ViewBag.ModificationType = TempData["ModificationType"];
            }


            var fac = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
            if (fac != null)
            {
                //get all the tanks belonging to this facility
                //var tnks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned && (a.Status== null || a.Status.Contains("Approved"))).ToList();
                var tnks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned).ToList();
                ViewBag.pumps = _context.Pumps.Where(a => a.FacilityId == id).ToList();
                ViewBag.tanks = tnks;
                ViewBag.facility = fac;
                ViewBag.phaseId = phaseId;
                return View();
            }
            ViewBag.ErrorMessage = "Sorry couldnt find facility information.";
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry couldnt find facility information.") });
        }

        [HttpPost]
        public IActionResult AddPumps(List<Pumps> Pumps, OtherModel om, string AppType)
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {

                if (Pumps.Count > 0)
                {
                    var fac = _context.Facilities.Where(a => a.Id == om.facilityId).FirstOrDefault();
                    if (fac != null)
                    {
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
                                    pp.TankId = item.TankId;
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


                        var app = _context.applications.Where(a => a.DeleteStatus != true && a.FacilityId == om.facilityId && a.PhaseId == om.phaseId
                        && a.company_id == fac.CompanyId && a.status == GeneralClass.PaymentPending).FirstOrDefault();
                        string pmNo = "atc";
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

                            //check if application is LTO and applicant does not have mistdo
                            //var mistdo = _context.MistdoStaff.Where(x => x.FacilityId == fac.Id && x.DeletedStatus == false).ToList();
                            //if (ph != null)
                            //{
                            //    if ((ph.ShortName == "LTO" || ph.ShortName == "TO" || ph.ShortName == "LR") && mistdo.Count() < 2)
                            //    {
                            //        return RedirectToAction(
                            //        , "Companies", new { id = generalClass.Encrypt(app.id.ToString()) });

                            //    }
                            //}

                            return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(app.id.ToString()) });
                        }
                        return RedirectToAction("CreateApplication", new { id = om.facilityId, om.phaseId, pNo = string.IsNullOrEmpty(om.PermitNo) ? "" : om.PermitNo, type = string.IsNullOrEmpty(AppType) ? "new" : AppType });
                    }
                }
                string ErrorMessage = "Please fill atleast one loading arm for the system to be able to continue with your application";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                throw;
            }
        }

        public IActionResult PaySanction(int id, string cat, string phaseId)
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();

            //Id is the FacilityId
            var fac = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
            if (fac != null)
            {
                int pid = 0;
                int.TryParse(phaseId, out pid);
                //get the phase application : 
                var ph = _context.Phases.Where(a => a.id == pid).FirstOrDefault();
                if (ph == null)
                {
                    string ErrorMessage = "Sorry, this phase wasn't recognized.";
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
                }
                //for now lets just get price form Phase table if the application does not exist yet 
                var app = _context.applications.Where(a => a.FacilityId == id && a.PhaseId == ph.id && a.company_id == fac.CompanyId && a.status == GeneralClass.PaymentPending).FirstOrDefault();
                if (app != null)
                {
                    return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(app.id.ToString()) });
                }


                //get all the thanks that belongs to this facility and use it to calculate the Price that is due for the Facility
                var tnks = _context.Tanks.Where(a => a.FacilityId == id && !a.Decommissioned).ToList();

                var totalVol = tnks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                app = _helpersController.RecordApplication(ph, totalVol, tnks.Count, comp.id, fac.Id, "New", "", CompanyEmail, "host");// = new Application();


                return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(app.id.ToString()) });
            }
            ViewBag.ErrorMessage = "We couldn't find the facility you are looking for";
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("We couldn't find the facility you are looking for") });

        }
        public IActionResult CreateApplication(int id, int phaseId, string pNo, string type)
        {
            try
            {
                var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();


                var fac = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
                if (fac != null)
                {

                    //for now, lets get price form Phase table if the application does not exist yet 
                    var app = _context.applications.Where(a => a.FacilityId == id && a.PhaseId == phaseId && a.company_id == fac.CompanyId && a.status == GeneralClass.PaymentPending).FirstOrDefault();

                    //get the phase application 
                    var ph = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();



                    if (ph == null)
                    {
                        ViewBag.error = "Sorry we couldn't recognise this phase";
                        return View("Error");
                    }
                    var cat = _context.Categories.Where(a => a.id == ph.category_id).FirstOrDefault();
                    //get all the thanks that belongs to this facility and use it to calculate the Price that is due for the Facility
                    var tnks = _context.Tanks.Where(a => a.FacilityId == id && !a.Decommissioned).ToList();
                    var pmps = _context.Pumps.Where(a => a.FacilityId == id).ToList();

                    var totalVol = tnks.Sum(a => Convert.ToDouble(a.MaxCapacity));

                    app = _helpersController.RecordApplication(ph, totalVol, tnks.Count, comp.id, fac.Id, type, pNo, CompanyEmail, "host");// = new Application();


                    if (type.ToLower() == "renew")
                    {

                        var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == pNo.ToLower()).FirstOrDefault();
                        if (lg != null)
                        {
                            lg.IsUsed = true;
                            _context.SaveChanges();
                        }

                    }


                    if (phaseId == 3)
                    {
                        var tankInsp = new TankInspections();
                        tankInsp.FacilityId = id;
                        tankInsp.Date = DateTime.Now;
                        tankInsp.ApplicationId = app.id;
                        tankInsp.NumberOfDriveIn = fac.NoofDriveIn;
                        tankInsp.NumberOfDriveOut = fac.NoOfDriveOut;
                        tankInsp.NumberOfPumps = pmps.Count;
                        tankInsp.NumberOfTanks = tnks.Count;
                        _context.TankInspections.Add(tankInsp);
                        _context.SaveChanges();

                        var tt = new TankLeakTests
                        {
                            ApplicationId = app.id,
                            DateAdded = DateTime.Now,
                            FacilityId = id,
                            TanksTotalCapacity = tnks.Sum(a => Convert.ToInt32(a.MaxCapacity))
                        };
                        _context.SaveChanges();
                    }

                    //Create Invoice before redirecting
                    var invo = new invoices();
                    invo.amount = Convert.ToDouble(app.fee_payable);
                    invo.application_id = app.id;
                    invo.payment_code = app.reference;
                    invo.payment_type = string.Empty;
                    invo.status = app.fee_payable > 0 ? "Unpaid" : "Paid";
                    invo.date_added = DateTime.Now;
                    invo.date_paid = DateTime.Now.AddDays(-7);

                    _context.invoices.Add(invo);
                    _context.SaveChanges();

                    if (app != null)
                    {
                        //Misto Plementation
                        //if (ph != null)
                        //{
                        //    //check if application is LTO and applicant does not have mistdo
                        //var mistdo = _context.MistdoStaff.Where(x => x.CompanyId == app.company_id && x.FacilityId == om.facilityId && x.DeletedStatus == false).ToList();

                        //    if (ph.ShortName == "LTO" && mistdo.Count() < 1)
                        //    {
                        //        app.status = GeneralClass.MistdoRequired; _context.SaveChanges();
                        //        return RedirectToAction("Mistdo", "Companies", new { id = generalClass.Encrypt(app.id.ToString()) });
                        //    }
                        //}
                        return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(app.id.ToString()) });
                    }



                }

                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, facility was not found.") });

            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                throw;
            }
        }

        public IActionResult getproducts(int id)
        {
            ViewBag.id = id;
            return View(_context.Products.ToList());
        }

        public IActionResult getTanks(int id, int fid)
        {
            ViewBag.id = id;
            return View(_context.Tanks.Where(a => a.FacilityId == fid && !a.Decommissioned).ToList());
        }

        public IActionResult Renew(int id, string permitId)
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();

            if (comp != null)
            {
                var pm = _context.permits.Where(a => a.id == id ).FirstOrDefault();
                if (pm != null)
                {
                    bool checkCompanyPermit = pm.company_id == CompanyID ? true : false;
                    var getApp = _context.applications.Where(x => x.id == pm.application_id).FirstOrDefault();

                    if (checkCompanyPermit == false && getApp!=null && getApp.company_id != CompanyID)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("The license number provided does not belong to your company. Please check the license number and try again.") });
                    }

                    var fac = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
                    if (fac != null)
                    {
                        var renew = _helpersController.GetRenewalModel(fac, pm);
                        var ps = _context.Phases.Where(a => a.name.ToLower() == "license renewal").FirstOrDefault();
                        if (ps != null)
                        {
                            ViewBag.category = ps.category_id;
                            ViewBag.phaseId = ps.id;
                        }
                        if (getApp.isLegacy == true)
                        {
                            var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId.ToLower()).FirstOrDefault();
                            var renw = _helpersController.GetRenewalModel(lg);
                            ViewBag.FacilityID = fac.Id;
                            ViewBag.legacy = lg;
                            ViewBag.frmLegacy = "yes";
                            ViewBag.stats = _context.States_UT.Where(a => a.Country_id == 156).ToList();
                            ViewBag.review = "yes";
                            return View("Renew", renw);
                        }
                        else
                        {
                            ViewBag.review = "Renew";
                            return View(renew);
                        }
                    }
                    ViewBag.ErrorMessage = "No Depot is Assocciated with the provided License Number.";

                }
                else {
                    var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId.ToLower()).FirstOrDefault();
                    if (lg != null)
                    {
                        var renw = _helpersController.GetRenewalModel(lg);
                        ViewBag.legacy = lg;
                        ViewBag.frmLegacy = "yes";
                        ViewBag.stats = _context.States_UT.Where(a => a.Country_id == 156).ToList();
                        ViewBag.review = "yes";
                        return View("Renew", renw);

                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("The license reference provided does not exist on the portal, kindly check the ref or apply for legacy. Please check the license number and try again.") });

                    }
                }
                
            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Session has expired, kindly login again.") });

        }

        public IActionResult RenewLN(string permitId)
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();

            permits pm = null;
            pm = _context.permits.Where(a => a.permit_no.ToLower() == permitId.ToLower() && a.company_id == CompanyID).FirstOrDefault();
            var getApp = _context.applications.Where(x => x.id == pm.application_id).FirstOrDefault();
            var ps = _context.Phases.Where(a => a.id == getApp.PhaseId).FirstOrDefault();

            if (pm == null)
            {

                var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId.ToLower()).FirstOrDefault();
                if (lg != null)
                {
                    var renw = _helpersController.GetRenewalModel(lg);
                    ViewBag.legacy = lg;
                    ViewBag.frmLegacy = "yes";
                    ViewBag.stats = _context.States_UT.Where(a => a.Country_id == 156).ToList();
                    ViewBag.review = "yes";
                    return View("Renew", renw);

                }
            }
            if (ps != null)
            {
                if (ps.name.ToLower() == "license to operate")
                {
                    var fac = _context.Facilities.Where(a => a.Id == getApp.FacilityId).FirstOrDefault();
                    if (fac != null)
                    {
                        var renew = _helpersController.GetRenewalModel(fac, pm);

                        ViewBag.review = "yes";
                        //return View("Renew", renew);
                        //check if application is LTO and applicant does not have mistdo
                        //var mistdo = _context.MistdoStaff.Where(x => x.CompanyId == _helpersController.getSessionUserID() &&x.FacilityId== fac.Id && x.DeletedStatus == false).ToList();

                        //    if (ps.ShortName == "LTO" && mistdo.Count() < 1)
                        //    {
                        //        return RedirectToAction("Mistdo", "Companies", new { id = generalClass.Encrypt(getApp.id.ToString()) });

                        //    }


                        //LTO application should be redirected to facility/tank review page

                        return RedirectToAction("ReviewFacilityTank", "Application", new { id = fac.Id, permitId = pm.id, category = ps.category_id, phaseId = ps.id, leg = 1 });
                    }
                    ViewBag.ErrorMessage = "No Depot is Assocciated with the provided License Number.";
                }
                else
                {
                    ViewBag.ErrorMessage = "Only a License to Operate a depot is renewable.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "License Number provided can not be found. Please check the License Number and try again.";
            }

            string ErrorMessage = ViewBag.ErrorMessage;
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permitId">LicenceId</param>
        /// <param name="category"></param>
        /// <param name="phaseId"></param>
        /// <param name="leg"></param>
        /// <param name="sId">Sanction Id, in the case of Sanction</param>
        /// <returns></returns>
        public IActionResult Review(string permitId, string category, string phaseId, string leg, string sId)
        {
            try
            {
                var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();
                int cid = int.Parse(category);
                //get the phase
                var ph = _context.Phases.Where(a => a.category_id == cid).FirstOrDefault();
                var app = (from p in _context.permits
                           join a in _context.applications on p.application_id equals a.id
                           where p.permit_no.ToLower() == permitId.ToLower()
                           select a).FirstOrDefault();


                if (!string.IsNullOrEmpty(permitId))
                {
                    permitId = permitId.Trim();
                }
                if (string.IsNullOrEmpty(phaseId))
                {
                    int c = 0;
                    int.TryParse(category, out c);
                    var p = _context.Phases.Where(a => a.category_id == c).FirstOrDefault();
                    if (p != null)
                    {
                        phaseId = p.id.ToString();
                    }
                }
                permits pm = null;
                int pid = 0;
                if (category == "5")
                {
                    phaseId = sId;
                    if (sId == "8")
                    {
                        //Built without Approval, so no Existing license Number
                        ViewBag.SanctionId = sId;
                        ViewBag.legacy = new Legacies();
                        ViewBag.stats = _context.States_UT.Where(a => a.Country_id == 156).ToList();
                        ViewBag.category = category;
                        ViewBag.permit_no = permitId;
                        ViewBag.phaseId = phaseId;
                        ViewBag.review = "yes";
                        ViewBag.City = "";
                        return View();
                    }

                }

                if (permitId.ToLower() == "atc" || permitId.ToLower() == "reg")
                {
                    ViewBag.frmLegacy = "No";
                    ViewBag.stats = _context.States_UT.Where(a => a.Country_id == 156).ToList();
                    ViewBag.category = category;
                    ViewBag.permit_no = permitId;
                    if (string.IsNullOrEmpty(phaseId))
                    {
                        if (ph != null)
                        {
                            phaseId = ph.id.ToString();
                        }
                    }
                    ViewBag.phaseId = phaseId;
                    ViewBag.review = "yes";
                    ViewBag.City = "";
                    return View();
                }

                pm = _context.permits.Where(a => a.permit_no.ToLower() == permitId.Trim().ToLower()).FirstOrDefault();
                if (pm == null)
                {
                    var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId.ToLower()).FirstOrDefault();
                    if (lg != null)
                    {
                        if (!(bool)lg.IsUsed)
                        {
                            var ap = _context.applications.Where(a => a.current_Permit.ToLower() == permitId.ToLower() && a.company_id == CompanyID).FirstOrDefault();
                            if (ap != null)
                            {
                                var f = _context.Facilities.Where(a => a.Id == ap.FacilityId).FirstOrDefault();
                                if (f != null)
                                {
                                    return RedirectToAction("AddTanks", "LTO", new { id = f.Id, permitId = permitId, Category = category, phaseId = phaseId });
                                }
                            }

                            string city = "";
                            var sb = new StringBuilder();
                            if (!string.IsNullOrEmpty(lg.FacilityAddress))
                            {
                                var s = lg.FacilityAddress.Split(',');
                                if (s.Count() > 1)
                                {
                                    city = s[s.Count() - 1];

                                    for (int i = 0; i <= s.Count() - 2; i++)
                                    {
                                        sb.Append(s[i]).Append(", ");
                                    }
                                    lg.FacilityAddress = sb.ToString();
                                }
                            }



                            ViewBag.legacy = lg;
                            ViewBag.frmLegacy = "yes";
                            ViewBag.stats = _context.States_UT.Where(a => a.Country_id == 156).ToList();
                            ViewBag.category = category;
                            ViewBag.permit_no = permitId;
                            if (string.IsNullOrEmpty(phaseId))
                            {
                                if (ph != null)
                                {
                                    phaseId = ph.id.ToString();
                                }
                            }
                            ViewBag.phaseId = phaseId;
                            ViewBag.review = "yes";
                            ViewBag.City = city;
                            return View();

                        }
                        else
                        {
                            var ap = _context.applications.Where(a => a.current_Permit.ToLower() == permitId.ToLower()).FirstOrDefault();
                            if (ap != null && ap.company_id == CompanyID)
                            {
                                if (category == "6")
                                {
                                    //allow to aaply for Recalibration
                                    return RedirectToAction("AddTanks", "LTO", new { id = ap.FacilityId, permitId = permitId, Category = category, phaseId = phaseId, sid = sId });

                                }
                            }
                            string empty = ap != null ? comp.name : "NA";
                            string dt = ap != null ? ap.date_added.ToLongDateString() : "NA";
                            TempData["PermitUsage"] = $"Permit Number \"{permitId}\" has been Used by {empty} on {dt}, Please Check again!";
                            return RedirectToAction("Apply", "Application");

                        }
                    }
                    else
                    {
                        TempData["PermitUsage"] = $"Permit Number \"{permitId}\"  does not exist, Please check and try again.";
                        return RedirectToAction("Apply", "Application");

                    }
                }
                else
                {
                    pid = pm.id;
                    if (app == null)
                    {
                        app = _context.applications.Where(a => a.id == pm.application_id).FirstOrDefault();

                    }
                    //check if the Application category is for take over
                    if (category == "4")
                    {

                        //check if the Permit belongs to the User
                        if (pm.company_id != CompanyID)
                        {
                            if (ph.name == "Approval To Construct" || ph.name == "Take Over" || ph.name == "License To Operate" || ph.name == "License Renewal" || ph.name == "Calibration/Integrity Tests(NDTs)")
                            {
                                return RedirectToAction("Takeover", "LTO", new { permitId = permitId, Category = category });
                            }
                            else
                            {
                                TempData["PermitUsage"] = $"Sorry, this Permit Number \"{permitId}\"  cannot be Used for a \"Take Over\" Process!. This can only happen at \"Approval To Construct, License To Operate, License Renewal, Calibration/Integrity Tests(NDTs)\" Stages";
                                return RedirectToAction("Apply", "Application");

                                ViewBag.ErrorMessage = "Sorry, this Permit cannot be Used for a \"Take Over\" Process!";

                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, this Permit cannot be Used for a \"Take Over\" Process!") });

                            }
                        }
                        else
                        {
                            TempData["PermitUsage"] = $"Sorry, you can only apply for 'Take Over' when the Permit does not belong to your Registered company";
                            return RedirectToAction("Apply", "Application");

                        }
                    }

                }

                switch (category)
                {
                    //Application is Calibration
                    case "7":

                        if (app != null && (app.PhaseId == 2 || app.PhaseId == 8)) //Permit entered is not for ATC or Regularization
                        {
                            var f = _context.Facilities.Where(a => a.Id == app.FacilityId).FirstOrDefault();
                            if (f != null)
                            {
                                return RedirectToAction("AddTanks", new { id = app.FacilityId, phaseId = phaseId, permitId = permitId, Category = category });
                            }
                        }
                        else
                        {
                            TempData["PermitUsage"] = "Sorry, the permit/approval number you have supplied is neither for Approval To Construct nor Regularization, hence, can not be used to apply for calibration.";
                            return RedirectToAction("Apply", "Application");
                        }
                        break;
                    case "2":
                        if (ph.id == 4 || ph.id == 6 || ph.id == 7)
                        {
                            return RedirectToAction("Renew", "LTO", new { id = pid, permitId = permitId, Category = category });
                        }
                        else
                        {

                            TempData["PermitUsage"] = $"Only LTO License can be Renewed, please Check you License again, {pm.permit_no} is for {ph.name}";
                            return RedirectToAction("Apply", "Application");

                        }
                    case "3":
                        //modification, do Tanks Modification
                        var ps = _context.Phases.Where(a => a.name.ToLower().Contains("modification")).FirstOrDefault();
                        if (ps == null)
                        {

                            TempData["PermitUsage"] = "We Could not find the appropriate phase for your application, Please contact support.";
                            return RedirectToAction("Apply", "Application");
                        }

                        return RedirectToAction("AddTanks", new { id = app.FacilityId, phaseId = phaseId, permitId = permitId, Category = category });

                    case "5":
                        if (phaseId == "10")
                        {
                            //int id, int phaseId, string pNo, string type
                            return RedirectToAction("CreateApplication", new { id = app.FacilityId, phaseId = phaseId, pNo = permitId, type = "New" });
                        }
                        if (phaseId == "9")
                        {
                            return RedirectToAction("AddTanks", new { id = app.FacilityId, phaseId = phaseId, permitId = permitId, Category = category });

                        }
                        break;
                    case "6":
                        //recalibration application
                        // it can either come from NDTs or Recalibration
                        return RedirectToAction("AddTanks", new { id = app.current_Permit, phaseId = phaseId, permitId = permitId, Category = category });

                        if (ph.id == 11 || ph.id == 6)
                        {

                        }
                        else
                        {

                            ViewBag.ErrorMessage = "Only Calibration or Recalibration Permit is accepted for this application";
                            string errmsg = "Only Calibration or Recalibration Permit is accepted for this application";
                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(errmsg) });

                        }
                        break;
                    default:
                        break;
                }

                TempData["PermitUsage"] = "System can not Process your Request Please review it and try again.";
                return RedirectToAction("Apply", "Application");
            }
            catch (Exception x)
            {
                _helpersController.LogMessages(x.ToString());
                //return Content(x.ToString());
                ViewBag.ErrorMessage = "Some Errors Occured while Processing your Request, Please try again.";

                string errmsg = "Some Errors Occured while Processing your Request, Please try again.";
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(errmsg) });
            }
        }
        [HttpPost]
        public IActionResult Review(SuitabilityInspections suitability, Address addres, OtherModel om)
        {
            string errmsg = "";
            if (addres != null && suitability != null)
            {
                var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();

                //get phaseId fot LTO 
                bool recFac = true;
                var fac = _context.Facilities.Where(a => a.AddressId == addres.id && a.CompanyId == CompanyID).FirstOrDefault();//lk
                                                                                                                                // already exiting facility on local DB
                var facility_count = _context.Facilities.Where(x => x.Name.Trim().ToLower() == om.FacilityName.Trim().ToLower() && x.CompanyId == CompanyID).Count();

                _helpersController.LogMessages("checking if new facility for " + CompanyName + " already exits.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

                if (facility_count > 0)
                {
                    errmsg = "Sorry, this facility already exits on the portal.";
                }
                else
                {

                    Facilities fa = null;
                    addresses add = null;
                    if (recFac)
                    {
                        #region Facility Address

                        add = new addresses();
                        add.address_1 = addres.address_1;
                        add.city = addres.city;
                        add.country_id = 156;
                        add.LgaId = addres.LGAId;
                        add.StateId = addres.stateId;
                        add.elps_id = 0;
                        _context.addresses.Add(add);
                        _context.SaveChanges();
                        #endregion


                        #region Facility
                        fa = new Facilities();
                        fa.AddressId = add.id;
                        fa.Date = DateTime.Now;
                        fa.Name = om.FacilityName;
                        fa.CompanyId = CompanyID;
                        fa.NoofDriveIn = 1;
                        fa.NoOfDriveOut = 1;
                        fa.CategoryId = int.Parse(om.category);//renewal Category
                        fa.ContactName = om.ContactName;
                        fa.ContactNumber = om.ContactNumber;
                        _context.Facilities.Add(fa);
                        _context.SaveChanges();
                        #endregion
                        _helpersController.LogMessages("New Facility Created With Name: " + fa.Name, CompanyID.ToString());

                    }
                    else
                    {
                        fa = _context.Facilities.Where(a => a.Id == fac.Id).FirstOrDefault();
                        add = _context.addresses.Where(a => a.id == fac.AddressId).FirstOrDefault();
                        _helpersController.LogMessages("Facility Exist");
                    }

                    //Check if Facility is added to ELPS, else add it
                    #region Add Facility to Processing on ELPS

                    FacilityAPIModel facmodel = new FacilityAPIModel()
                    {
                        Name = fa.Name,
                        CompanyId = comp.elps_id.GetValueOrDefault(),
                        FacilityType = "Depot",
                        LGAId = (int)add.LgaId,
                        City = add.city,
                        StateId = add.StateId,
                        StreetAddress = add.address_1
                    };
                    _helpersController.LogMessages("Facility model created for elps");


                    var respApp = _helpersController.PushFacility(facmodel);

                    if (respApp != null && respApp.Id > 0)
                    {
                        fa.Elps_Id = respApp.Id;
                        _context.SaveChanges();
                    }

                    #endregion
                    suitability.ISAlongPipeLine = string.IsNullOrEmpty(om.ISAlongPipeLine) ? false : true;
                    suitability.IsOnHighWay = string.IsNullOrEmpty(om.IsOnHighWay) ? false : true;
                    suitability.IsUnderHighTension = om.IsUnderHighTension == null ? false : true;

                    suitability.CompanyId = CompanyID;
                    suitability.ApplicationId = -1;
                    suitability.FacilityId = fa.Id;
                    _context.SuitabilityInspections.Add(suitability);
                    _context.SaveChanges();

                    if (om.category == "1")
                    {
                        switch (om.phaseId)
                        {
                            case 2:
                            #region application was for suitability==> move to ATC
                            //permit must be of Suitability Inspection- SI
                            //return RedirectToAction("Application", "ATC", new { id = fa.Id, phaseId = om.phaseId, permitId = om.PermitNo, leg = om.frmLegacy });
                            //break;
                            #endregion
                            case 3:
                                #region application for ATC ==> move to Tanks Pressure Leak Test
                                //permit must be of Approval to Contruct- ATC
                                return RedirectToAction("TankInspection", "ATC", new { id = fa.Id, phaseId = om.phaseId, permitId = om.PermitNo, leg = om.frmLegacy });
                            //break;
                            #endregion
                            case 4:
                                #region application for Tanks Pressure Leak Test ==> move to LTO
                                //permit must be of Preasure Leak Test- PLT

                                return RedirectToAction("AddTanks", "LTO", new { id = fa.Id, permitId = om.PermitNo, Category = om.category, phaseId = om.phaseId, sid = om.SanctionId });

                                // break;
                                #endregion

                        }


                    }
                    return RedirectToAction("AddTanks", "LTO", new { id = fa.Id, permitId = om.PermitNo, Category = om.category, phaseId = om.phaseId, sid = om.SanctionId });
                }



            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(errmsg) });

        }

        public IActionResult ApproveSponsor(int id)
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();

            var mc = _context.MarketingCompanies.Where(a => a.Id == id).FirstOrDefault();
            if (mc != null)
            {
                var fac = _context.Facilities.Where(a => a.Id == mc.FacilityId).FirstOrDefault();
                if (fac != null)
                {
                    // fac.tanks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned).ToList();

                }
                ViewBag.mcId = id;
                return View(fac);
            }
            return View("Error");
        }

        [HttpPost]
        public IActionResult ApproveSponsor(int id, string approve, string reason)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var comp = _context.companies.Where(a => a.user_id == userEmail).FirstOrDefault();


            var mc = _context.MarketingCompanies.Where(a => a.Id == id).FirstOrDefault();
            if (mc != null)
            {
                if (mc.SponsorId == comp.elps_id.GetValueOrDefault())
                {
                    var fac = _context.Facilities.Where(a => a.Id == mc.FacilityId).FirstOrDefault();
                    if (approve == "approve")
                    {
                        mc.IsApproved = true;
                        mc.Reason = reason;
                        //Move Next
                    }
                    else
                    {
                        mc.IsApproved = false;
                    }
                    mc.ApprovedBy = userEmail;
                    mc.Reason = reason;
                    _context.SaveChanges();

                    ViewBag.msg = "Thanks for your Response";
                    return View("confirmApprove");
                }
            }
            return View("Error");
        }


        public IActionResult Modification(string id, string permitId, string category)//int id, int phaseId
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            try
            {
                if (!string.IsNullOrEmpty(permitId))
                {
                    permitId = permitId.Trim();

                }
                //id is the Facility(Depot ID)
                Facilities fac = null;
                if (!string.IsNullOrEmpty(id))
                {
                    var Id = int.Parse(id);
                    fac = _context.Facilities.Where(a => a.Id == Id).FirstOrDefault();

                }
                if (fac == null)
                {

                    var pm = _context.permits.Where(a => a.permit_no.ToLower() == permitId.Trim().ToLower() && a.company_id == userID).FirstOrDefault();
                    if (pm != null)
                    {
                        // = _context.Facilities.Where(a => a.per == pm.fac).FirstOrDefault();

                    }
                }

                if (fac == null)
                {
                    var app = _context.applications.Where(a => a.current_Permit.ToLower() == permitId.ToLower()).FirstOrDefault();
                    if (app != null)
                    {

                        fac = _context.Facilities.Where(a => a.Id == app.FacilityId).FirstOrDefault();

                    }

                }

                if (fac != null)
                {
                    var ps = _context.Phases.Where(a => a.name.ToLower() == "depot modification").FirstOrDefault();

                    ViewBag.tnks = _context.Tanks.Where(a => a.FacilityId == fac.Id && !a.Decommissioned).ToList();
                    ViewBag.facility = fac;
                    ViewBag.phaseId = ps == null ? 5 : ps.id;
                    ViewBag.permitId = permitId;
                    ViewBag.products = _context.Products.ToList();
                    return View();
                }
                ViewBag.Errormessage = "Please check your permit number again and make sure you entered it currectly";
                string ErrorMessage = ViewBag.Errormessage;
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
            }
            catch (Exception x)
            {

                ViewBag.Errormessage = "Some Error Occured while Processing your Request";
                string ErrorMessage = ViewBag.Errormessage;
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
            }
        }

        [HttpPost]
        public IActionResult Modification(int facilityId, int phaseId, string type, string permitId)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var comp = _context.companies.Where(a => a.user_id == userEmail).FirstOrDefault();

            try
            {
                Facilities fac = null;
                fac = _context.Facilities.Where(a => a.Id == facilityId).FirstOrDefault();
                if (fac == null)
                {
                    ViewBag.errorMessage = "Depot to be modified could not be found, please go back and review your application";
                }
                if (fac != null)
                {

                    //for now lets just get price form Phase table if the application does not exist yet 
                    var app = _context.applications.Where(a => a.FacilityId == facilityId && a.PhaseId == phaseId && a.status == GeneralClass.PaymentPending).FirstOrDefault();
                    if (app != null)
                    {
                        return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(app.id.ToString()) });
                    }

                    //get the phase application : LTO this time
                    var ph = _context.Phases.Where(a => a.id == phaseId).FirstOrDefault();
                    if (ph == null)
                    {
                        string ErrorMessag = "Sorry Phase could not be found. Please ensure correct application phase is selected";

                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessag) });
                    }
                    var cat = _context.Categories.Where(a => a.id == ph.category_id).FirstOrDefault();
                    var tnks = _context.Tanks.Where(a => a.FacilityId == facilityId && !a.Decommissioned).ToList();
                    var pmps = _context.Pumps.Where(a => a.FacilityId == facilityId).ToList();

                    var totalVol = tnks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                    var ppId = Convert.ToInt16(_configuration.GetSection("AmountSetting").GetSection("PriceByTankId").Value.ToString());

                    //check if they have applied before or this is the first for this Facility;
                    //var tc = _context.FacilityModifications.Where(a => a.FacilityId == facilityId).FirstOrDefault();
                    //var typ = tc == null ? "new" : "renew";

                    app = _helpersController.RecordApplication(ph, totalVol, tnks.Count, comp.id, fac.Id, "New", permitId, userEmail, null);// = new Application();



                    //Create the Facility Modification
                    var fm = new FacilityModifications();
                    fm.ApplicationId = app.id;
                    fm.Date = DateTime.Now;
                    fm.FacilityId = facilityId;
                    fm.PhaseId = phaseId;
                    fm.Type = type;

                    _context.SaveChanges();

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

                    // MailHelper.SendEmail(userEmail, subject, msgBody);

                    return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(app.id.ToString()) });

                }

                ViewBag.ErrorMessage = "Sorry we couldnt find the Facility in Question.";

                string ErrorMessage = ViewBag.ErrorMessage;
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });
            }
            catch (Exception x)
            {
                //return Content(x.ToString());
                ViewBag.Error = "Some Error Occured while Processing your Request";

                string ErrorMessag = ViewBag.Error;
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessag) });
            }
            // return View();
        }

        #region Take Over
        public IActionResult TakeOver(string id, string permitId, string category = null)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            Facilities fa = null;
            var user = userEmail.ToLower();
            ViewBag.NewCompany = userName;
            var pm = new permits();
            if (!string.IsNullOrEmpty(id))
            {
                var Id = int.Parse(id);
                fa = _context.Facilities.Where(a => a.Id == Id).FirstOrDefault();

            }
            if (fa == null)
            {

                pm = _context.permits.Where(a => a.permit_no.ToLower() == permitId.ToLower()).FirstOrDefault();
                if (pm != null)
                {
                    var app = _context.applications.Where(a => a.id == pm.application_id).FirstOrDefault();
                    fa = _context.Facilities.Where(a => a.Id == app.FacilityId).FirstOrDefault();
                }
            }
            if (fa == null)
            {

                var app = _context.applications.Where(a => a.current_Permit.ToLower() == permitId.ToLower()).FirstOrDefault();
                if (app != null)
                {
                    fa = _context.Facilities.Where(a => a.Id == app.FacilityId).FirstOrDefault();

                }

            }


            var prod = _context.Products.ToList();
            _helpersController.LogMessages("Processing for Legacy TakeOver (" + pm.permit_no + ") >>> Ln: 2451", userName);
            #region LEGACY
            var l_permit = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId.ToLower()).FirstOrDefault();
            if (l_permit != null)
            {
                _helpersController.LogMessages("TO Leg: License Found");
                ViewBag.Legacy = true;
                ViewBag.Permit = l_permit;

                if (l_permit.IsUsed == true)
                {
                    _helpersController.LogMessages("Take-Over Legacy: Take-Over already initiated for: " + l_permit.LicenseNo);
                    var app = _context.applications.Where(a => a.current_Permit.ToLower() == l_permit.LicenseNo.ToLower()).FirstOrDefault();
                    if (app != null)
                    {
                        fa = _context.Facilities.Where(a => a.Id == app.FacilityId).FirstOrDefault();
                        var coy = _context.companies.Where(a => a.id == fa.CompanyId).FirstOrDefault();
                        ViewBag.Company = coy;
                        var oa = _context.addresses.Where(a => a.id == coy.registered_address_id).FirstOrDefault();
                        var ostate = _context.States_UT.Where(a => a.State_id == oa.StateId).FirstOrDefault();
                        if (oa != null)
                        {

                            ViewBag.OwnerAddress = $"{oa.address_1}, {oa.city}, {ostate.StateName}";
                        }
                        ViewBag.Phase = "TO";

                        fa.Tanks = (from t in _context.Tanks.AsEnumerable()
                                    join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                                    where t.FacilityId == fa.Id && t.Decommissioned != true && (t.Status == null || (t.Status.Contains("Approved")))
                                    select new TankModel
                                    {
                                        Id = t.Id,
                                        Name = t.Name,
                                        MaxCapacity = t.MaxCapacity.ToString(),
                                        ProductName = p.Name,
                                        Height = t.Height,
                                        Decommissioned = t.Decommissioned,
                                        Diameter = t.Diameter,
                                        CreateAt = t.CreatedAt,
                                        ModifyType = t.ModifyType
                                    }).ToList();
                        fa.Pumps = _context.Pumps.Where(a => a.FacilityId == fa.Id).ToList();
                        var address = _context.addresses.Where(a => a.id == fa.AddressId).FirstOrDefault();
                        var state = _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                        ViewBag.LGA = _context.Lgas.Where(l => l.Id == address.LgaId).FirstOrDefault().Name;
                        ViewBag.FacAddress = address.address_1;
                        ViewBag.FacState = state.StateName;

                        return View(fa);

                    }
                    else
                    {
                        string ErrorMessage2 = "Sorry we could not find the application attached to this license, please contact Support.";
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage2) });

                    }
                }
            }
            #endregion
            #region Non LEGACY
            if (fa != null)
            {
                var permit = _context.permits.Where(a => a.permit_no.ToLower() == permitId.ToLower()).FirstOrDefault();
                if (permit == null)
                {

                    var lg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == permitId.ToLower()).FirstOrDefault();
                    if (lg != null)
                    {
                        ViewBag.Permit = lg;
                        ViewBag.legacy = true;
                        ViewBag.lg = lg;
                        permit = new permits();
                        permit.categoryName = "Take Over";
                        permit.permit_no = permitId;
                    }

                }
                else
                {
                    _helpersController.LogMessages($"permit takeover {JsonConvert.SerializeObject(permit)}", userName);
                    ViewBag.Permit = permit;
                    ViewBag.legacy = false;
                }


                var coy = _context.companies.Where(a => a.id == fa.CompanyId).FirstOrDefault();
                ViewBag.Company = coy;
                var oa = _context.addresses.Where(a => a.id == coy.registered_address_id).FirstOrDefault();
                var ostate = _context.States_UT.Where(a => a.State_id == oa.StateId).FirstOrDefault();
                if (oa != null)
                {

                    ViewBag.OwnerAddress = $"{oa.address_1}, {oa.city}, {ostate.StateName}";
                }
                ViewBag.Phase = "TO";

                fa.Tanks = (from t in _context.Tanks.AsEnumerable()
                            join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                            where t.FacilityId == fa.Id && t.Decommissioned != true && (t.Status == null || (t.Status.Contains("Approved")))
                            select new TankModel
                            {
                                Id = t.Id,
                                Name = t.Name,
                                MaxCapacity = t.MaxCapacity.ToString(),
                                ProductName = p.Name,
                                Height = t.Height,
                                Decommissioned = t.Decommissioned,
                                Diameter = t.Diameter,
                                CreateAt = t.CreatedAt,
                                ModifyType = t.ModifyType
                            }).ToList();
                fa.Pumps = _context.Pumps.Where(a => a.FacilityId == fa.Id).ToList();
                var address = _context.addresses.Where(a => a.id == fa.AddressId).FirstOrDefault();
                var state = _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                ViewBag.LGA = _context.Lgas.Where(l => l.Id == address.LgaId).FirstOrDefault().Name;
                ViewBag.FacAddress = address.address_1;
                ViewBag.FacState = state.StateName;

                return View(fa);
            }
            #endregion
            ViewBag.errorMessage = "Sorry we could not find the facility to be taken over, Please review your Application and try again";


            string ErrorMessage = ViewBag.ErrorMessage;
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt(ErrorMessage) });

        }

        [HttpPost]
        public IActionResult TakeOver(FacilityToVM model)
        {
            if (model.TransferCost <= 0)
            {
                TempData["Message"] = "Invalid Facility Transfer Cost, please provide the cost of acquiring the facility and try again";
                return RedirectToAction("TakeOver", new { id = model.facilityId, permitId = model.LicenseNo });
            }
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();

            var owner = new companies();
            var user = userEmail.ToLower();

            try
            {

                var myCompany = comp;

                var _leg = _context.Legacies.Where(a => a.LicenseNo.ToLower() == model.LicenseNo.ToLower()).FirstOrDefault();

                bool frmATC = false;
                var license = _context.permits.Where(a => a.permit_no.ToLower() == model.LicenseNo.ToLower()).FirstOrDefault();
                if (license == null)
                {

                    //check legacy table
                    if (_leg != null)
                    {
                        if (_leg.AppType == "ATC")
                            frmATC = true;
                        owner.DPR_Id = _leg.CompId.ToString();

                    }

                }
                else
                {
                    if (license.categoryName == "Approval To Construct")
                        frmATC = true;
                    owner = _context.companies.Where(a => a.id == license.company_id).FirstOrDefault();
                }

                var fa = _context.Facilities.Where(a => a.Id == model.facilityId).FirstOrDefault();
                fa.Name = model.facName;

                var ps = _context.Phases.Where(a => a.name.ToLower() == "take over").FirstOrDefault();

                #region Create/Modify Facility
                //
                if (!(fa.Id > 0))
                {
                    fa.CategoryId = ps.category_id;
                    _context.Facilities.Add(fa);
                }
                _context.SaveChanges();
                #endregion

                var tnks = _context.Tanks.Where(a => a.FacilityId == fa.Id && !a.Decommissioned).ToList();
                var totalVol = tnks.Sum(a => Convert.ToDouble(a.MaxCapacity));

                #region Create
                var ap = _context.applications.Where(a => a.FacilityId == fa.Id && a.PhaseId == ps.id && a.company_id == myCompany.id
                            && a.status == GeneralClass.PaymentPending && a.DeleteStatus != true).FirstOrDefault();
                if (ap == null)
                {
                    ap = _helpersController.RecordApplication(ps, totalVol, tnks.Count, myCompany.id, fa.Id, "New", model.LicenseNo, userEmail, null, frmATC, model.TransferCost);// = new Application();
                    UpdateAppTanks(tnks, ap.id, null);

                }
                #endregion


                //#region TakeOver
                //TakeOvers to = new TakeOvers();
                //to.ApplicationId = ap.id;
                //to.CompanyId = myCompany.id;
                //to.Date = DateTime.Now;
                //to.FacilityId = fa.Id;
                //to.OldCompanyId = owner.id > 0 ? owner.id : 0;
                //to.OldCompanyNMDPRAId = owner.DPR_Id;
                //_context.TakeOvers.Add(to);
                //_context.SaveChanges();
                //#endregion

                ap.TransferCost = model.TransferCost;
                _context.SaveChanges();

                var currentApplication = (from p in _context.permits
                                          join a in _context.applications on p.application_id equals a.id
                                          where p.permit_no == ap.current_Permit
                                          select a).FirstOrDefault();
                double tnkV = 0;
                int tnkCnt = 0;
                var oldphase = new Phases();

                if (currentApplication != null)
                {
                    var apts = _context.ApplicationTanks.Where(a => a.ApplicationId == currentApplication.id).ToList();

                    if (apts.Count() > 0)
                    {
                        tnkV = apts.Sum(a => a.Capacity);
                        tnkCnt = apts.Count;
                    }
                    else
                    {
                        var facTanks = _context.Tanks.Where(a => a.FacilityId == currentApplication.FacilityId).ToList();
                        tnkV = facTanks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                        tnkCnt = facTanks.Count();
                    }
                    oldphase = _context.Phases.Where(a => a.id == currentApplication.PhaseId).FirstOrDefault();
                }
                else
                {
                    var facTanks = _context.Tanks.Where(a => a.FacilityId == ap.FacilityId).ToList();
                    tnkV = facTanks.Sum(a => Convert.ToDouble(a.MaxCapacity));
                    tnkCnt = facTanks.Count();
                }
                var phase = _context.Phases.Where(a => a.id == ap.PhaseId).FirstOrDefault();
                var frmAtc = frmATC == true ? true : oldphase?.ShortName == "ATC" ? true : false;
                var feeDesc = _helpersController.CalculateAppFee(phase, ap.current_Permit, tnkV, tnkCnt, frmAtc, model.TransferCost);

                _helpersController.LogMessages($"New app amount:: {feeDesc.Fee}");
                if (feeDesc != null)
                {
                    ap.fee_payable = feeDesc.Fee;
                    ap.PaymentDescription = feeDesc.FeeDescription;
                }
                _context.SaveChanges();
                //Create Invoice before redirecting
                var invo = new invoices();
                invo.amount = Convert.ToDouble(ap.fee_payable + ap.service_charge);
                invo.application_id = ap.id;
                invo.payment_code = ap.reference;
                invo.payment_type = string.Empty;
                invo.status = ap.fee_payable > 0 ? "Unpaid" : "Paid";
                invo.date_added = DateTime.Now;
                invo.date_paid = DateTime.Now.AddDays(-7);
                _context.invoices.Add(invo);
                _context.SaveChanges();


                return RedirectToAction("UploadApplicationDocument", "Application", new { id = generalClass.Encrypt(ap.id.ToString()) });

            }
            catch (Exception ex)
            {
                _helpersController.LogMessages(ex.ToString());

                return RedirectToAction("TakeOver", new { pemitId = model.LicenseNo, Category = 4 });
            }

        }


        private List<Tanks> BuildTanks(Legacies findLegacy, List<Products> prod, int coyId, int facId)
        {
            List<Tanks> tanks = new List<Tanks>();
            for (int pi = 1; pi <= findLegacy.PMS_Tanks; pi++)
            {
                tanks.Add(new Tanks()
                {
                    CompanyId = coyId,
                    FacilityId = facId,
                    MaxCapacity = ((int)(findLegacy.PMSVol / findLegacy.PMS_Tanks)).ToString(),
                    Name = "PMS Tank" + pi,
                    ProductId = prod.Where(a => a.Name.ToLower() == "pms").FirstOrDefault().Id
                });
            }
            for (int ai = 1; ai <= findLegacy.AGO_Tanks; ai++)
            {
                tanks.Add(new Tanks()
                {
                    CompanyId = coyId,
                    FacilityId = facId,
                    MaxCapacity = ((int)(findLegacy.AGOVol / findLegacy.AGO_Tanks)).ToString(),
                    Name = "AGO Tank" + ai,
                    ProductId = prod.Where(a => a.Name.ToLower() == "ago").FirstOrDefault().Id
                });
            }
            for (int di = 1; di <= findLegacy.DPK_Tanks; di++)
            {
                tanks.Add(new Tanks()
                {
                    CompanyId = coyId,
                    FacilityId = facId,
                    MaxCapacity = ((int)(findLegacy.DPKVol / findLegacy.DPK_Tanks)).ToString(),
                    Name = "DPK Tank" + di,
                    ProductId = prod.Where(a => a.Name.ToLower() == "dpk").FirstOrDefault().Id
                });
            }
            return tanks;
        }


        //public IActionResult TakeOver(int id)
        //{

        //    var lto = _ltoRep.FindBy(a => a.ApplicationId == id).FirstOrDefault();
        //    if (lto != null)
        //    {
        //        var fa = _context.Facilities.Where(a => a.Id == lto.FacilityId).FirstOrDefault();
        //        var ps = _context.Phases.Where(a => a.Name == "Take Over").FirstOrDefault();
        //        var cat = _context.Categories.Where(a => a.Id == ps.category_id).FirstOrDefault();
        //        var comp = comp;// _context.companies.Where(a => a.user_id == userEmail).FirstOrDefault();
        //        var ap = new Application();
        //        ap.category_id = ps.category_id;
        //        ap.Company_Id = comp.Id;
        //        ap.Date_Added = DateTime.Now;
        //        ap.Date_Modified = DateTime.Now;
        //        ap.FacilityId = fa.Id;
        //        ap.fee_payable = ps.Price;
        //        ap.PhaseId = ps.Id;
        //        ap.Reference = PaymentRef.RefrenceCode();
        //        ap.service_charge = ps.ServiceCharge;
        //        ap.status = GeneralClass.PaymentPending;// "Payment Pending";
        //        ap.Type = "New";
        //        ap.Year = DateTime.Today.Year;
        //        ap.AllowPush = true;

        //        _appRep.Add(ap);
        //        _appRep.Save(userEmail, Request.UserHostAddress);


        //        var to = new TakeOver();
        //        to.ApplicationId = ap.Id;
        //        to.CompanyId = comp.Id;
        //        to.Date = DateTime.Now;
        //        to.FacilityId = fa.Id;
        //        to.OldCompanyId = fa.CompanyId;

        //        _takeRep.Add(to);
        //        _takeRep.Save(userEmail, Request.UserHostAddress);


        //        #region Send Application Initiation Mail
        //        //var app = _context.applications.Where(a => a.Id == ap.Id).FirstOrDefault();
        //        string subject = "Application Initiated: " + ap.Reference;


        //        var msg = new NewDepot.ModelsMessage();
        //        msg.Company_Id = comp.Id;
        //        msg.Content = "Loading...";
        //        msg.Date = DateTime.Now;
        //        msg.Read = 0;
        //        msg.subject = subject;
        //        msg.sender_id = "Application";
        //        _msgRep.Add(msg);
        //        _msgRep.Save(userEmail, Request.UserHostAddress);


        //        //var dt = comp.Date.Day.ToString() + comp.Date.Month.ToString() + comp.Date.Year.ToString();
        //        var sn = msg.Id;
        //        var body = "";
        //        using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
        //        {
        //            body = sr.ReadToEnd();
        //        }
        //        var apDetails = "";
        //        var tk = string.Format("Thank you for submitting your application for Suitability Inspection. Details of your application are as follows: <br /><ul><li>Tracking Number: {0}</li><li>Application Type: {1}</li><li>Permit Category: {2}</li><li>Amount Due: {3}</li><li>Payment Status: Unpaid</li><li>Application Period: {4}</li>", ap.Reference,
        //       ap.Type, cat.Name, ap.fee_payable + ap.service_charge, ap.Year);


        //        var src = "<ul>";

        //        src += "<li>" + fa.Name + ": " + ps.Description + " </li>";

        //        src += "</ul>";
        //        var services = "<p>Facility:<br>" + src + "<br /></p>";


        //        apDetails = tk + services;
        //        //string subject = "Application Initiated: " + app.Reference;
        //        var msgBody = string.Format(body, subject, apDetails, sn);


        //        var mm = _msgRep.FindBy(m => m.Id == msg.Id).FirstOrDefault();
        //        mm.Content = msgBody;
        //        _msgRep.Edit(mm);
        //        _msgRep.Save(userEmail, Request.UserHostAddress);

        //        #endregion


        //        //Create Invoice before redirecting
        //        var invo = new Invoice();
        //        invo.Amount = Convert.ToDouble(ap.fee_payable + ap.service_charge);
        //        invo.Application_Id = ap.Id;
        //        invo.Payment_Code = ap.Reference;
        //        invo.Payment_Type = string.Empty;
        //        invo.status = "Unpaid";
        //        invo.Date_Added = DateTime.Now;
        //        invo.Date_Paid = DateTime.Now.AddDays(-7);

        //        _invoiceRep.Add(invo);
        //        _invoiceRep.Save(userEmail, Request.UserHostAddress);

        //        //trans.Complete();
        //        MailHelper.SendEmail(userEmail, subject, msgBody);


        //        return RedirectToAction("Payment", new { id = ap.Id, refCode = ap.Reference });
        //    }
        //    //var  = new LTO();
        //    return View();
        //}

        #endregion
    }


}