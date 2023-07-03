using LpgLicense.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NewDepot.Helpers;
using NewDepot.Models;
using System;
using System.Linq;
using System.Security.Claims;
using NewDepot.Controllers.Authentications;
using NewDepot.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Net.Http;

namespace NewDepot.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly Depot_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;
        
        ElpsResponse elpsResponse = new ElpsResponse();
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;
        HttpClient _httpClient;

        HttpClientServices _httpClientServices;
        MistdoServices _mistdoServices;


        public CompaniesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
          _mistdoServices = new MistdoServices(_httpClient, "https://mistdo.dpr.gov.ng/");

            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        /*
     * Saving Tanks
     */
        [Authorize(Policy = "CompanyRoles")]
        public JsonResult SaveTanks(List<Tanks> Tanks, int AppID)
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {
                int total = 0;
                int newUpdate = 0;
                int newCount = 0;

                decimal converstion = 0;

                int i = 0;

                var result = "";


                var getNewTanks = Tanks.Where(x => x.Id == 0);

                if (getNewTanks.Count() > 0)
                {
                    newUpdate += getNewTanks.Count();
                }

                _helpersController.LogMessages(newUpdate + " Newly modified tanks for " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());

                var appstanks = _context.ApplicationTanks.Where(x => x.ApplicationId == AppID);

                if (appstanks.Count() > 0)
                {
                    _context.ApplicationTanks.RemoveRange(appstanks);
                    _context.SaveChanges();
                }

                foreach (var item in Tanks.ToList())
                {
                    i += 1;

                    var appProduct = _context.Products.Where(x => x.Id == item.ProductId );

                    
                    var checkTank = _context.Tanks.Where(x => x.Name.ToUpper().Trim() == item.Name.ToUpper().Trim() && x.FacilityId == item.FacilityId);

                    if (checkTank.Count() <= 0)
                    {
                        Tanks tanks = new Tanks()
                        {
                            FacilityId = item.FacilityId,
                            MaxCapacity = item.MaxCapacity,
                            ProductId = item.ProductId,
                            CreatedAt = DateTime.Now,
                            Name = item.Name.ToUpper(),
                            Position = item.Position,
                            Height=item.Height,
                            Diameter=item.Diameter,
                            FriendlyName=item.FriendlyName,
                            CompanyId=userID
                        };
                        _context.Tanks.Add(tanks);

                        ApplicationTanks appTanks = new ApplicationTanks()
                        {
                            FacilityId = item.FacilityId,
                            ApplicationId = AppID,
                            Capacity = Convert.ToDouble( item.MaxCapacity),
                            ProductId =(int) item.ProductId,
                            Date = DateTime.Now,
                            TankName = item.Name.ToUpper(),
                            TankId = tanks.Id,
                            CompanyId=userID
                        };

                        
                        _context.ApplicationTanks.Add(appTanks);

                        newCount += _context.SaveChanges();

                    }
                    else
                    {
                        checkTank.FirstOrDefault().FacilityId = item.FacilityId;
                        checkTank.FirstOrDefault().MaxCapacity = item.MaxCapacity;
                        checkTank.FirstOrDefault().ProductId = item.ProductId;
                        checkTank.FirstOrDefault().Name = item.Name.ToUpper();
                        checkTank.FirstOrDefault().Position = item.Position;
                        checkTank.FirstOrDefault().Height = item.Height;
                        checkTank.FirstOrDefault().Diameter = item.Diameter;
                        //checkTank.FirstOrDefault().up = DateTime.Now;

                        ApplicationTanks appTanks = new ApplicationTanks()
                        {
                            FacilityId = item.FacilityId,
                            ApplicationId = AppID,
                            Capacity = Convert.ToDouble(item.MaxCapacity),
                            ProductId = (int)item.ProductId,
                            Date = DateTime.Now,
                            TankName = item.Name.ToUpper(),
                            TankId = checkTank.FirstOrDefault().Id,
                            CompanyId = userID
                        };


                        _context.ApplicationTanks.Add(appTanks);
                        newCount += _context.SaveChanges();
                    }
                }

                if (newCount > 0)
                {
                    _helpersController.LogMessages("New tank created or modified for " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());

                    
                        var apps = _context.applications.Where(x => x.id == AppID && x.DeleteStatus != true);
                        apps.FirstOrDefault().UpdatedAt = DateTime.Now;
                    

                    if (_context.SaveChanges() > 0)
                    {
                        _helpersController.LogMessages(newUpdate + " Newly added/modified/non-modified tanks saved for " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());


                        if (apps.Count() > 0)
                        {
                            //check if application is LTO and applicant does not have mistdo
                            var mistdo = _context.MistdoStaff.Where(x => x.FacilityId == apps.FirstOrDefault().FacilityId && x.DeletedStatus == false).ToList();
                            var ph = _context.Phases.Where(a => a.id == apps.FirstOrDefault().PhaseId).FirstOrDefault();

                            if ((ph.ShortName == "LTO" || ph.ShortName == "TO"|| ph.ShortName == "LR") && mistdo.Count() < 2)// change to 5 later
                            {
                                apps.FirstOrDefault().status = GeneralClass.MistdoRequired;
                                apps.FirstOrDefault().UpdatedAt = DateTime.Now;

                                if (_context.SaveChanges() > 0)
                                {
                                    result = "1|" + generalClass.Encrypt(apps.FirstOrDefault().id.ToString());
                                    _helpersController.LogMessages("Tanks saved sucessfully for " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());
                                }
                                else
                                {
                                    result = "0|Something went wrong trying to update your application status. Please try again";
                                }
                            }
                            else
                            {
                                apps.FirstOrDefault().status = GeneralClass.PaymentPending;
                                apps.FirstOrDefault().UpdatedAt = DateTime.Now;

                                if (_context.SaveChanges() > 0)
                                {
                                    result = "2|" + generalClass.Encrypt(apps.FirstOrDefault().id.ToString());
                                    _helpersController.LogMessages("Tanks saved sucessfully for " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());
                                }
                                else
                                {
                                    result = "0|Something went wrong trying to update your application status. Please try again";
                                }
                            }
                        }

                        else
                        {
                            result = "0|Error, Application not found.";
                        }
                    }
                    else
                    {
                        result = "0|Something went wrong trying to save your added/modified/non-modified tanks, please try again later";
                    }
                }
                else
                {
                    result = "0|Error, Something went wrong trying to save your tanks. Please try again.";
                }

                _helpersController.LogMessages(result, _helpersController.getSessionEmail());
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }
        }


        #region Mistdo


        /*
         * Creating Mistdo Staff for a company
         * 
         * id => encrypted Application ID
         */
        public IActionResult Mistdo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                int appid = generalClass.DecryptIDs(id);

                if (appid == 0)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
                }
                else
                {
                    var apps = _context.applications.Where(x => x.id == appid && x.DeleteStatus != true);
                    ViewBag.Facility = _context.Facilities.ToList();

                    if (apps.Count() > 0)
                    {
                        var mistdo = _context.MistdoStaff.Where(x => x.FacilityId == apps.FirstOrDefault().FacilityId && x.CompanyId == apps.FirstOrDefault().company_id && x.DeletedStatus == false);

                        ViewData["ApplicationId"] = appid;
                        ViewData["FacilityId"] = apps.FirstOrDefault().FacilityId;

                        return View(mistdo.ToList());
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application was not found. Kindly contact support.") });
                    }
                }
            }
        }



        /*
         * Verifying and saving a mistdo staff
         */
        public JsonResult VerifyMistdoStaff(string CertNo, string FacilityId)
        {
            var result = "";
            int done = 0;

            var url = "home/verifymistdocertificate?certificateid=" + CertNo;

            try
            {
                var verify = _mistdoServices.VerifyStaff(url);

                if (verify != null) {

                    if (verify.Result.message == "Found")
                    {
                        var data = verify.Result.datas;

                        var check = _context.MistdoStaff.Where(x => x.CertificateNo == data.certificateNo && x.FacilityId.ToString() == FacilityId.Trim()  && x.DeletedStatus == false);

                        if (check.Count() > 0)
                        {
                            check.FirstOrDefault().IssuedDate = data.certificateIssue;
                            check.FirstOrDefault().ExpiryDate = data.certificateExpiry;
                            check.FirstOrDefault().MistdoServerId = data.mistdoId;
                            check.FirstOrDefault().FullName = data.fullname;
                            check.FirstOrDefault().PhoneNo = data.phoneNumber;
                            check.FirstOrDefault().Email = data.email;
                            check.FirstOrDefault().UpdatedAt = DateTime.Now;

                            done += _context.SaveChanges();
                        }
                        else
                        {
                            MistdoStaff mistdo = new MistdoStaff()
                            {
                                FullName = data.fullname,
                                PhoneNo = data.phoneNumber,
                                Email = data.email,
                                MistdoServerId = data.mistdoId,
                                FacilityId=int.Parse(FacilityId),
                                CertificateNo = data.certificateNo,
                                IssuedDate = data.certificateIssue,
                                ExpiryDate = data.certificateExpiry,
                                CompanyId = _helpersController.getSessionUserID(),
                                DeletedStatus = false,
                                CreatedAt = DateTime.Now
                            };

                            _context.MistdoStaff.Add(mistdo);

                            done += _context.SaveChanges();
                        }

                        if (done > 0)
                        {
                            result = "Saved";

                        }
                        else
                        {
                            result = "Something went wrong tying to save this staff, please try again later.";
                        }
                    }
                    else
                    {
                        result = verify.Result.message;
                    }
                }
                else
                {
                    result = null;
                } 
                

            }
            catch (Exception ex)
            {
                result = "Error : " + ex.Message + " " + ex.InnerException;
            }

            _helpersController.LogMessages("Result for saving mistdo staff " + result, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Removing a mistdo staff
         */
        [HttpPost]
        public JsonResult RemoveMistdoStaff(int id)
        {
            var result = "";
            try
            {

                var check = _context.MistdoStaff.Where(x => x.MistdoId == id && x.DeletedStatus == false);

                if (check.Count() > 0)
                {
                    check.FirstOrDefault().DeletedStatus = true;
                    check.FirstOrDefault().DeletedAt = DateTime.Now;

                    if (_context.SaveChanges() > 0)
                    {
                        result = "Deleted";
                    }
                    else
                    {
                        result = "Something went wrong trying to remove this record, please try again later";
                    }
                }
                else
                {
                    result = "Something went wrong trying to find this particular entry, please try again later.";
                }
            }
            catch (Exception ex)
            {
                result = "Error : " + ex.Message + " " + ex.InnerException;
            }

            _helpersController.LogMessages("Result for deleting mistdo staff " + result, _helpersController.getSessionEmail());

            return Json(result);
        }





        public JsonResult CheckMistdoStaff(string appid)
        {
            string result = "";

            try
            {
                int id = int.Parse(appid);
                var apps = _context.applications.Where(x => x.id == id && x.DeleteStatus == false);

                if (apps.Count() > 0)
                {
                    var check = _context.MistdoStaff.Where(x => x.CompanyId == _helpersController.getSessionUserID() && x.FacilityId== apps.FirstOrDefault().FacilityId && x.DeletedStatus == false && x.ExpiryDate.Value >= DateTime.Now.Date);

                if (check.Count() >= 5) // numbers of mistdo staff required for this portal  change to 5 later
                {
                    
                        apps.FirstOrDefault().status = GeneralClass.PaymentPending;
                        apps.FirstOrDefault().UpdatedAt = DateTime.Now;

                        if (_context.SaveChanges() > 0)
                        {
                            var fac = _context.Facilities.Where(a => a.Id == apps.FirstOrDefault().FacilityId).FirstOrDefault();

                            result = "1|" + generalClass.Encrypt(apps.FirstOrDefault().id.ToString());
                            _helpersController.LogMessages("Mistdo Staff added sucessfully for "+fac.Name + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());
                        }
                        else
                        {
                            result = "0|Something went wrong trying to update your application status. Please try again";
                        }
                    }
                    else
                    {
                        result = "0|Sorry, you must have at least 5 Mistdo certified staff for this company.";

                    }
                }
                else
                {
                    result = "0|Error, Application not found.";

                }
            }
            catch (Exception ex)
            {
                result = "0|Error : " + ex.Message + " " + ex.InnerException;
            }

            return Json(result);
        }


        #endregion

        public JsonResult DeleteApplication(string AppID)
        {
            string result = "";

            int appID = 0;

                appID = Convert.ToInt32(AppID);

                var apps = _context.applications.Where(x => x.id == appID && x.DeleteStatus != true);

                    if (apps.Count() > 0)
                    {
                    
                            //Now check tanks that are associated with this application
                            var aptanks= _context.Tanks.Where(a => a.FacilityId == apps.FirstOrDefault().FacilityId).ToList();
                            if(aptanks.Count() > 0)
                            {
                                foreach(var t in aptanks)
                                {
                                    if(t.Status != null)
                                    {
                                        var splitTD = t.Status.Contains("|") ? t.Status.Split('|')[1] : t.Status;
                                            // if (splitTD == apps.FirstOrDefault().id.ToString())lk check later
                                            if (t.Status.Contains("Processing"))
                                           {
                                            var atank = _context.ApplicationTanks.Where(a => a.TankId == t.Id && a.ApplicationId == appID).FirstOrDefault();
                                                if (atank != null) {
                                                //first delete from Appication tanks

                                                _context.ApplicationTanks.Remove(atank);
                                                    _context.SaveChanges();

                                               }
                                            
                                                //_context.Tanks.Remove(t);
                                                //_context.SaveChanges();
                                        }
                                    }
                                }
                            }
                     
                    apps.FirstOrDefault().DeleteStatus = true;
                    apps.FirstOrDefault().DeletedAt = DateTime.Now;
                    apps.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();
                    if (_context.SaveChanges() > 0)
                    {
                        result = "App Removed";
                    }
                    else
                    {
                            result = "Something went wrong trying to remove this application";
                        }
                    }
                    else
                    {
                        result = "This application was not found. Please try again";
                    }
            

            _helpersController.LogMessages("Delete application status : " + result + " Application ID " + appID, _helpersController.getSessionEmail());

            return Json(result);
        }

        public IActionResult Index(string updateType, int? appId)
        {
            if (generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName)) == "Error")
            {
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {

                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                string userID = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

               if (!string.IsNullOrEmpty(updateType))
                {
                    ViewBag.ActiveView = updateType;
                }
                var company = userEmail;

                if (company == "Error")
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
                }
                else
                {
                    CompanyDetail companyModels = new CompanyDetail();

                    var paramData = _restService.parameterData("compemail", company);
                    var response = _restService.Response("/api/company/{compemail}/{email}/{apiHash}", paramData, "GET", null); // GET

                    if (response.IsSuccessful == false)
                    {
                        companyModels = null;
                        ViewData["CompanyName"] = null;
                    }
                    else
                    {
                        companyModels = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);


                        string address = "";

                        if (companyModels != null)
                        {
                            address = companyModels.registered_Address_Id;
                        }

                        var paramDatas = _restService.parameterData("Id", address);
                        var responses = _restService.Response("/api/Address/ById/{Id}/{email}/{apiHash}", paramDatas); // GET

                        if (responses != null)
                        {
                            var com = JsonConvert.DeserializeObject<Address>(responses.Content);

                            if (com != null)
                            {
                                //var code = generalClass.GetStateShortName(com.stateName.ToUpper(), "00" + CODE);

                                // companyModels. = code;
                                companyModels.address_1 = com.address_1;
                                companyModels.address_2 = com.address_2;
                                companyModels.type = com.type;
                                companyModels.countryName = com.countryName;
                                companyModels.city = com.city;
                                if (com.stateName != null)
                                {
                                    companyModels.stateName = com.stateName.ToUpper();
                                }
                                companyModels.postal_Code = com.postal_Code;
                                companyModels.stateId = com.stateId;
                                companyModels.country_Id = com.country_Id;
                            }
                        }
                    }
                    _helpersController.LogMessages("Displaying full company's profile for admin...", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));
                    if (companyModels == null)
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, company profile was not found on ELPS. Please contact DPR.") });
                    }
                    else
                    {
                        return View(companyModels);
                    }

                }
            }
        }
        #region Company detail
        public IActionResult CompanyProfile(string id)
        {
            int userID = generalClass.DecryptIDs(id);

            if (userID==0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, company ID is not passed correctly. Please try again") });

            }
            ViewBag.compId = id;
            ViewBag.ActiveView = "Profile";
            return View();
        }
        //[Authorize(Policy = "CompanyRoles")]
        public IActionResult CompanyInformation(string message = null)
        {
            var msg = "";

            if (message != null)
            {
                msg = generalClass.Decrypt(message);
            }

            ViewData["Message"] = msg;

            return View();
        }
        #endregion
        public IActionResult Index2(string id, int? appId)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (userRole.Contains("Admin") || userRole.Contains("Staff"))
            {

                return RedirectToAction("All");
            }
            else
            {
                var comp = _context.companies.Where(a => a.CompanyEmail == userEmail).FirstOrDefault();
                var apps = _context.applications.Where(a => a.company_id == comp.id).ToList();


                #region Get From ELPS

                var paramDatas = _restService.parameterData("compemail", userEmail);
                var response = _restService.Response("/api/company/{compemail}/{email}/{apiHash}", paramDatas, "GET", null); // GET
               

              //  var CompanyModell = JsonConvert.DeserializeObject<com>(response.Content.ToString());

                var client = new WebClient();
      string output = client.DownloadString(ElpsServices._elpsBaseUrl + "/api/Companyfull/" + userEmail + "/" + ElpsServices._elpsAppEmail + "/" + ElpsServices.appHash);
                var CompanyModel = JsonConvert.DeserializeObject<LpgLicense.Models.CompanyModelAPI>(output);

                if (CompanyModel != null && CompanyModel.Company != null) 
                {

                   
                    List<countries> allCountry = _context.countries.OrderBy(c => c.name).ToList();
                    countries ctry = new countries();
                    if (CompanyModel.Company != null && CompanyModel.Company.Nationality != null)
                        ctry = allCountry.Find(C => C.name.ToLower() == CompanyModel.Company.Nationality.ToLower());
                    ViewBag.country = allCountry;
                    ViewBag.Nationality = new SelectList(allCountry, "Id", "Name", ctry);
                    if (!string.IsNullOrEmpty(id))
                    {
                        ViewBag.ActiveView = id;
                    }
                    ViewBag.AppId = appId;
                    if (TempData["status"] != null)
                    {
                        ViewBag.Msg = TempData["message"].ToString();
                        ViewBag.Type = TempData["status"].ToString();
                        TempData.Clear();
                    }
                    return View(CompanyModel);
                }
                else if (CompanyModel != null && CompanyModel.Company.Registered_Address_Id == null && CompanyModel.Company.Registered_Address_Id <= 0)
                {
                    return RedirectToAction("ContinueCompanyUpdate", new { id = id, appId = 1 });
                }

                else if (CompanyModel != null && CompanyModel.CompanyExpatriateQuotas == null || CompanyModel.CompanyExpatriateQuotas.Count == 0)
                {
                    return RedirectToAction("ContinueCompanyUpdate", new { id = id, appId = 1 });
                }
                return RedirectToAction("Dashboard", "Companies");

                #endregion
            }
        }
        public IActionResult Dashboard()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int userELPSID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionElpsID));
            
            var company = _context.companies.Where(a => a.id == userID).FirstOrDefault();

            if (company != null)
            {

                if (string.IsNullOrEmpty(company.contact_firstname)) 
                {
                }

                ViewBag.CompanyName = company.name;
                var cid = company.elps_id.GetValueOrDefault();
                var approveFac = _context.MarketingCompanies.Where(a => a.SponsorId == cid).ToList();
                if (approveFac.Count > 0)
                {
                    ViewBag.approveFac = approveFac;
                }
                var dashboardModel = new DashBoardModel();
                dashboardModel.CompanyId = company.id;
                var appl = (from u in _context.applications
                            join c in _context.Categories on u.category_id equals c.id
                            join p in _context.Phases on u.PhaseId equals p.id
                            where u.company_id == company.id && u.DeleteStatus!= true
                            select new {
                                status = u.status, categoryName=c.name, submitted=u.submitted, phaseName=p.name, dateAdded=u.date_added, dateSubmitted=u.CreatedAt
                            }).ToList();
             

                ViewBag.TotalFacility = _context.Facilities.Where(a => a.CompanyId == company.id && a.DeletedStatus != true).Count();




                if (appl.Count > 0)
                {
                    if (appl.Where(a => a.status.ToLower() == "payment pending").FirstOrDefault() != null)
                    {
                        #region Has atleast one Application that is yet to be Paid for
                        //PP Payment Pending
                        ViewBag.PPCount = "1";
                        ViewBag.Type = "warn";
                        #endregion
                    }
                    else if (appl.Where(a => a.status.ToLower() == "payment completed" && a.submitted == false).FirstOrDefault() != null)
                    {
                        #region Has atleast one Application that not been submitted
                        ViewBag.PCCount = "1";
                        ViewBag.Type = "info";
                        #endregion
                    }
                    
                    #region Application Counter for DashBoard
                    dashboardModel.TotalApplication = appl.Count;
                    dashboardModel.Approved = appl.Count(a => a.status.ToLower() == "approved");
                    dashboardModel.Processing = appl.Count(a => a.status.ToLower() == "processing");
                    dashboardModel.Rejected = appl.Count(a => a.status.ToLower() == "declined" || a.status.ToLower() == "rejected");
                    dashboardModel.Pending = appl.Count(a => a.status.ToLower() == "pending");
                    dashboardModel.PaymentPending = appl.Count(a => a.status.ToLower() == "payment pending");
                    dashboardModel.DocumentRequired = appl.Count(a => a.status.ToLower() == "documents required");
                    #region Expiring application
                    int expCount = 0;
                    int expired = 0;
                    //int id = appl.FirstOrDefault().Id;
                    List<permits> permits = _context.permits.Where(p => p.company_id == company.id).ToList();
                    foreach (var item in permits)
                    {
                        var check = item.date_expire.AddDays(-90);
                        var now = DateTime.Now;

                        if (item.date_expire < now && string.IsNullOrEmpty(item.is_renewed))
                        {
                            expired++;
                        }
                        else if (check <= now && item.date_expire > now)
                        {
                            expCount++;
                        }
                        
                    }
                    #endregion

                    //dashboardModel.Expiring = expiringApp.Count;
                    dashboardModel.CategoryA = appl.Count(a => a.categoryName.ToLower() == "categorya" && a.status.ToLower() == "approved");
                    dashboardModel.CategoryB = appl.Count(a => a.categoryName.ToLower() == "categoryb" && a.phaseName.ToLower() == "lto" && a.status.ToLower() == "approved");
                    dashboardModel.CategoryC = appl.Count(a => a.categoryName.ToLower() == "categoryc" && a.status.ToLower() == "approved");

                    dashboardModel.Expiring = expCount;
                    dashboardModel.Expired = expired;
                    dashboardModel.SubmittedNow = appl.Where(x=>x.submitted==true && DateTime.Now.AddMinutes(-30)< x.dateSubmitted).ToList().Count();
                    #endregion
                }
                else
                {
                    ViewBag.Msg = "You have not applied for any license on the Portal.";
                    ViewBag.Type = "info";
                }
                var mesg = (from u in _context.messages  
                                 where (u.company_id== userID || u.sender_id == userELPSID.ToString() && u.UserType.ToLower()=="company") select u).ToList().OrderByDescending(x=>x.date).Take(10);
                dashboardModel.Messages = mesg.ToList();
                ViewBag.Messages = mesg;
                //get company application inspection schedule
               var getMS = (from u in _context.MeetingSchedules.AsEnumerable()
                             join a in _context.applications.AsEnumerable() on u.ApplicationId equals a.id
                             where a.company_id == userID && u.Approved == true && u.Accepted == null
                             && u.ScheduleExpired!= true
                             && u.MeetingDate > DateTime.Now
                             select new MeetingSchedulesModel
                             {
                                 Id = u.Id,
                                 Reference = a.reference,
                                 MeetingDate = u.MeetingDate,
                                 Message = u.Message,
                                 ApplicationId = a.id
                             }).ToList();
                ViewBag.PendingSchedule = getMS;
                return View(dashboardModel);
            }
            else
            {
                return RedirectToAction("APIRegister", new { email = userEmail });
            }

        }
        //////[Authorize(Roles = "COMPANY")]
        public IActionResult AllCompanies()
        {
            if (generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName)) == "Error")
            {
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {
                var com = _context.companies;
                _helpersController.LogMessages("Displaying all companies...", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail)));
                return View(com.ToList());
            }
        }
        public IActionResult AllCompanyDoc(string id)
        {
            if (generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleName)) == "Error")
            {
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Straddle not found or not in correct format. Kindly contact support.") });
                }

                int CompID = 0;
                var comp_id = generalClass.Decrypt(id);

                if (comp_id == "Error")
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Straddle not found or not in correct format. Kindly contact support.") });
                }
                else
                {
                    List<PresentDocuments> presentDocuments = new List<PresentDocuments>();

                    CompID = Convert.ToInt32(comp_id);
                    var doc = _context.ApplicationDocuments;

                    var getCom = from c in _context.companies
                                 select new
                                 {
                                     CompanyName = c.name,
                                     CompanyElpsID = c.elps_id,
                                     CompanyID = c.id
                                 };

                    if (getCom.Count() > 0 && getCom.FirstOrDefault().CompanyElpsID != 0)
                    {
                        getCom = getCom.Where(x => x.CompanyID == CompID);

                        if (getCom.Count() > 0)
                        {
                            ViewData["CompanyName"] = "All available documents for " + getCom.FirstOrDefault()?.CompanyName;

                            _helpersController.LogMessages("Getting " + ViewData["CompanyName"].ToString(), generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

                            List<LpgLicense.Models.Document> companyDoc = generalClass.getCompanyDocuments(getCom.FirstOrDefault().CompanyElpsID.ToString());

                            if (companyDoc == null)
                            {
                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                            }
                            else
                            {
                                foreach (var appDoc in doc.ToList())
                                {
                                    if (appDoc.docType == "Company")
                                    {
                                        foreach (var cDoc in companyDoc)
                                        {
                                            if (cDoc.document_type_id == appDoc.ElpsDocTypeID.ToString())
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
                                }

                                _helpersController.LogMessages("Displaying company documents records for " + ViewData["CompanyName"].ToString(), generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail)));

                                presentDocuments = presentDocuments.GroupBy(x => x.TypeName).Select(c => c.FirstOrDefault()).OrderBy(x => x.TypeName).ToList();
                                return View(presentDocuments);
                            }
                        }
                        else
                        {
                            return View(presentDocuments);
                        }
                    }
                    else
                    {
                        return View(presentDocuments);
                    }
                }
            }
        }

        public IActionResult MySchedules()
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userELPSID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionElpsID));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
           
            if (userELPSID == 0)
            {
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {
                var sch = from s in _context.MeetingSchedules.AsEnumerable()
                          join a in _context.applications.AsEnumerable() on s.ApplicationId equals a.id
                          join phase in _context.Phases on a.PhaseId equals phase.id
                          join f in _context.Facilities.AsEnumerable() on a.FacilityId equals f.Id
                          join c in _context.companies.AsEnumerable() on a.company_id equals c.id
                          join ad in _context.addresses.AsEnumerable() on f.AddressId equals ad.id
                          join sf in _context.States_UT.AsEnumerable() on ad.StateId equals sf.State_id
                          where a.company_id == userID && a.DeleteStatus != true && s.Approved == true

                          select new MySchdule
                          {
                              AppRef=a.reference,
                              ScheduleID = s.Id,
                              Phase=phase.name,
                              FacilityID = f.Id,
                              FacilityName = f.Name,
                              FacilityAddress = ad.address_1 + ", " + sf.StateName + " (" + ad.city + ")",
                              ContactName = f.ContactName,
                              ContactPhone = f.ContactNumber,
                              ScheduleDate = s.MeetingDate.ToString(),
                              CompanyName = c.name,
                              CompanyID = c.id,
                              CustomerRespons = s.Accepted == true ? 1 : 0,
                              CustomerResponse = s.Accepted == true ? "Accepted" : s.Accepted == false ?"Declined": "Awaiting",
                              Accepted = s.Accepted,
                              StaffComment = s.Message,
                              SupervisorComment = s.FinalComment != null ? s.FinalComment : "Schedule Approved",
                              CustomerComment = s.DeclineReason != null ? s.DeclineReason : "Schedule Accepted",
                              //ScheduleType = s.Type,
                              ScheduleLocation = s.Address,
                              CreatedAt = s.Date,
                              UpdatedAt = s.UpdatedAt,
                          };

               

                _helpersController.LogMessages("Displaying company's schedule...", userEmail);
                
                return View(sch.ToList());
                
            }
        }

        public IActionResult MyExtraPayments()
        {
            var exPay = from e in _context.ApplicationExtraPayments.AsEnumerable()
                        join a in _context.applications.AsEnumerable() on e.ApplicationId equals a.id
                        join c in _context.companies.AsEnumerable() on a.company_id equals c.id
                        join p in _context.Phases.AsEnumerable() on a.PhaseId equals p.id
                        join ct in _context.Categories.AsEnumerable() on a.category_id equals ct.id
                        where c.id == _helpersController.getSessionUserID() && a.DeleteStatus != true
                        select new ExtraPaymentsModel
                        {
                            ApplicationId = a.id,
                            Reference = a.reference,
                            Type = e.Type.ToUpper(),
                            Category = c.name,
                            Status=e.Status,
                            Phase=p.name,
                            Date=e.Date,
                            Amount=e.Amount,
                            Comment=e.Comment,
                            RRR=e.RRR,
                            
                        };

            _helpersController.LogMessages("Displaying company's extra payment list...", _helpersController.getSessionEmail());

            return View(exPay.ToList());
        }


        // //[Authorize(Policy = "CompanyRoles")]
        public IActionResult DocumentsLibrary()
        {
            List<PresentDocuments> presentDocuments = new List<PresentDocuments>();

            var doc = _context.ApplicationDocuments;

            var getCom = _context.companies.Where(x => x.id == _helpersController.getSessionUserID());
            var getFac = _context.Facilities.Where(x => x.CompanyId == _helpersController.getSessionUserID());


            if (getCom.Count() > 0 && getFac.Count() > 0)
            {
                ViewData["CompanyName"] = "All available documents for " + getCom.FirstOrDefault().name;

                List<LpgLicense.Models.Document> companyDoc = generalClass.getCompanyDocuments(getCom.FirstOrDefault().elps_id.ToString());
                List<LpgLicense.Models.FacilityDocument> facilityDoc = generalClass.getFacilityDocuments(getFac.FirstOrDefault().Elps_Id.ToString());

                if (companyDoc == null || facilityDoc == null)
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
                }
                else
                {
                    foreach (var appDoc in doc.ToList())
                    {
                        if (appDoc.docType == "Company")
                        {
                            foreach (var cDoc in companyDoc)
                            {
                                if (cDoc.document_type_id == appDoc.ElpsDocTypeID.ToString())
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
                            foreach (var fDoc in facilityDoc)
                            {
                                if (fDoc.Document_Type_Id == appDoc.ElpsDocTypeID)
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
                        presentDocuments = presentDocuments.GroupBy(x => x.TypeName).Select(c => c.FirstOrDefault()).OrderBy(x => x.TypeName).ToList();

                    }
                }
            }
            else
            {
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                
                ViewData["CompanyName"] = "No available documents for "+ userName;
                //return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("No document found for this company") });
            }

            _helpersController.LogMessages("Displaying company's documents...", _helpersController.getSessionEmail());

            return View(presentDocuments);
        }


        public IActionResult LegalStuff(string id)
        {


            if (id == null || string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps... No company was found or passed") });
            }
            else
            {
                ViewData["cid"] = id;
                return View();
            }
        }
        public IActionResult Messages()
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userELPSID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionElpsID));

            if (userELPSID==0) 
            { 
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {
                var messages = _context.messages.Where(x => x.sender_id == userELPSID.ToString()).OrderByDescending(x => x.id);

                _helpersController.LogMessages("Displaying company's messages as list...", userEmail);
                return View(messages.ToList());
            }
        }

        /*
       * Viewing a single message for a company 
       * 
       * id  => encrypted company id,
       * option => encrypted message id
       */
        public IActionResult Message(string id, string option)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userELPSID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionElpsID));

            if (userELPSID == 0)
            {
                return RedirectToAction("ExpiredSession", "Account");

            }
            else
            {

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(option))
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Message not found or not in correct format. Kindly contact support.") });
                }

                int comp_id = 0;
                int msg_id = 0;

                var c_id = generalClass.Decrypt(id);
                var m_id = generalClass.Decrypt(option);

                if (c_id == "Error" || m_id == "Error")
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Message not found or not in correct format. Kindly contact support.") });
                }
                else
                {
                    comp_id = Convert.ToInt32(c_id);
                    msg_id = Convert.ToInt32(m_id);

                    var msg = _context.messages.Where(x => x.id == msg_id);
                     var app = _context.applications.Where(a => a.id== msg.FirstOrDefault().AppId).FirstOrDefault();
                     var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                    var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    var facState = address == null ? null : _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();

                    ViewBag.application = app;
                    ViewBag.Facility = facility.Name + "("+address.address_1+", "+address.city+" "+ facState.StateName +")";
                    msg.FirstOrDefault().read =1;
                    _context.SaveChanges();
                   
                    ViewData["MessageTitle"] = msg.FirstOrDefault().subject;

                    _helpersController.LogMessages("Displaying single company's message...", userEmail);
                    return View(msg.FirstOrDefault());
                }
            }
        }

        public IActionResult FullCompanyProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }

            var company = generalClass.Decrypt(id);

            if (company == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found or not in correct format. Kindly contact support.") });
            }
            else
            {
                var com = _context.companies.Where(x => x.elps_id.ToString() == company || x.id.ToString()==company).FirstOrDefault();

                ViewData["CompanyProfile"] = "Update Profile for " + com.name;

                CompanyDetail companyModels = new CompanyDetail();

                var paramData = _restService.parameterData("id", com.elps_id.ToString());
                var response = _restService.Response("/api/company/{id}/{email}/{apiHash}", paramData, "GET", null); // GET

                if (response.IsSuccessful == false)
                {
                    var comp = _context.companies.Where(x => x.elps_id == com.elps_id).FirstOrDefault();
                    companyModels.user_Id = comp.CompanyEmail;
                    companyModels.name = comp.name;
                    companyModels.id = (int)comp.elps_id;
                }
                else
                {
                    companyModels = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);
                }

                _helpersController.LogMessages("Displaying full company's profile for admin...", _helpersController.getSessionEmail());

                return View(companyModels);
            }
        }
        /*
        * Getting company profile information
        */

        public IActionResult GetCompanyProfile(string CompanyId)
        {
            var paramData = _restService.parameterData("id", CompanyId);
            var result = generalClass.RestResult("company/{id}", "GET", paramData, null); // GET
            return Json(result.Value.ToString());
        }





        /*
         * Updating full a company's profile
         */
        [HttpPost]

        public JsonResult UpdateCompanyProfile(jsonCompanyDetail companyDetail)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            try
            {
                string results = "";

                bool emailChange = false;

                var company = _context.companies.Where(x => x.CompanyEmail == companyDetail.user_Id);

                if (company.Count() > 0)
                {
                    if (company.FirstOrDefault().CompanyEmail == companyDetail.user_Id)
                    {
                        emailChange = false;
                    }
                    else
                    {
                        emailChange = true;
                    }
                }

                LpgLicense.Models.CompanyChangeModel companyChange = new LpgLicense.Models.CompanyChangeModel()
                {
                    Name = companyDetail.name,
                    RC_Number = companyDetail.rC_Number,
                    Business_Type = companyDetail.business_Type,
                    emailChange = emailChange,
                    CompanyId = companyDetail.id,
                    NewEmail = companyDetail.user_Id
                };

                CompanyInformationModel companyDetails = new CompanyInformationModel();
                companyDetails.company = companyDetail;

                var result = generalClass.RestResult("company/Edit", "PUT", null, companyDetails, "Company Updated"); // PUT

                var result2 = generalClass.RestResult("Accounts/ChangeEmail", "POST", null, companyChange, "Company Updated"); // PUT

                if (result2.Value.ToString() == "Company Updated")
                {
                    
                    if (company.Count() > 0)
                    {
                        company.FirstOrDefault().name = companyDetail.name;
                        company.FirstOrDefault().CompanyEmail = companyDetail.user_Id;
                        company.FirstOrDefault().UpdatedAt = DateTime.Now;
                        company.FirstOrDefault().contact_firstname = companyDetail.contact_FirstName;
                        company.FirstOrDefault().contact_lastname = companyDetail.contact_LastName;
                        company.FirstOrDefault().contact_phone = companyDetail.contact_Phone;
                        if (_context.SaveChanges() > 0)
                        {
                            results = "Company Updated";
                        }
                        else
                        {
                            results = "Company profile successfully updated on ELPS but not on PDJ DEPOT. Please try again later.";
                        }
                    }
                    else
                    {
                        results = "Company profile updated on ELPS but not on PDJ DEPOT. Please try again later.";
                    }
                }
                else
                {
                    results = result.Value.ToString();
                }

                _helpersController.LogMessages("Updating company's profile...", _helpersController.getSessionEmail());

                return Json(results);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }
        }



        /*
         * Createing a company's address
         */
        public IActionResult CreateCompanyAddress(string CompanyId, Address address)
        {
            List<Address> addresses = new List<Address>();
            addresses.Add(address);

            var paramData = _restService.parameterData("CompId", CompanyId);
            var result = generalClass.RestResult("Address/{CompId}", "POST", paramData, addresses, "Created Address"); // POST
            _helpersController.LogMessages("Creating new company's address....", _helpersController.getSessionEmail());
            return Json(result.Value);
        }


        /*
         * Updating a company's address
         */


        //[AllowAnonymous]
        public IActionResult UpdateCompanyAddress(Address address, List<Address> addresse)
        {
            addresse.Add(address);
            var result = generalClass.RestResult("Address", "PUT", null, addresse, "Address Updated"); // PUT
            _helpersController.LogMessages("Updating company's address...", _helpersController.getSessionEmail());
            
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var company = _context.companies.Where(x => x.id == CompanyID && x.DeleteStatus != true && x.ActiveStatus != false).FirstOrDefault();

            if (company != null && address!= null && address.id != 0)
            {
                if (company.registered_address_id == null || company.registered_address_id <= 0)
                {
                    var add = new addresses
                    {
                        elps_id = 0,
                        address_1 = address.address_1,
                        city = address.city,
                        country_id = 156,
                        LgaId = address.LGAId,
                        StateId = address.stateId,
                    };
                    _context.addresses.Add(add);
                  
                    if(  _context.SaveChanges() > 0)
                    {
                        company.registered_address_id = add.id;
                        _context.SaveChanges();
                    }


                }
                else
                {
                    var add = _context.addresses.Where(x => x.id == company.registered_address_id).FirstOrDefault();
                    if( add!=null)
                    {

                        add.address_1 = address.address_1;
                        add.city = address.city;
                        add.country_id = 156;
                        add.LgaId = address.LGAId;
                        add.StateId = address.stateId;
                        _context.SaveChanges();

                    }
                }

            }

            return Json(result.Value);
        }



        /*
         * Getting Directors Names
         */
        public IActionResult GetDirectorsNames(string CompanyId)
        {
            var paramData = _restService.parameterData("CompId", CompanyId);
            var result = generalClass.RestResult("Directors/{CompId}", "GET", paramData, null, null); // GET
            _helpersController.LogMessages("Getting company's directors name", _helpersController.getSessionEmail());
            return Json(result.Value);
        }



        /*
         * Creating company directors
         */
        //[HttpPost]
        public IActionResult CreateCompanyDirectors(string CompanyId, string telephone, string nationality, addresses ad, string firstName, string lastName)
        {
            Directors directors = new Directors()
            {
                address=new Address()
                {
                address_1 = ad.address_1,
                postal_Code = ad.postal_code,
                city = ad.city,
                country_Id = ad.country_id,
                stateId = ad.StateId
                },
                firstName = firstName,
                lastName = lastName,
                nationality = int.Parse(nationality),
                telephone = telephone,
                company_Id = int.Parse(CompanyId),
               
            };

            List<Directors> directorsList = new List<Directors>();
            directorsList.Add(directors);

            var paramData = _restService.parameterData("CompId", CompanyId);
            var result = generalClass.RestResult("Directors/{CompId}", "POST", paramData, directorsList, "Director Created"); // POST
            _helpersController.LogMessages("Creating company's directors...", _helpersController.getSessionEmail());
            return Json(result.Value);
        }


        /*
         * Retriving a list of a particular company directors
         */
        public IActionResult GetDirectors(string DirectorID)
        {
            var paramData = _restService.parameterData("Id", DirectorID);
            var result = generalClass.RestResult("Directors/ById/{Id}", "GET", paramData, null, null); // GET
            _helpersController.LogMessages("Getting single company's director details...", _helpersController.getSessionEmail());
            return Json(result.Value);
        }


        /*
         * Updating company's director information
         */
        public IActionResult UpdateCompanyDirectors(List<Directors> directors)
        {
            var result = generalClass.RestResult("Directors", "PUT", null, directors, "Director Updated"); // PUT
            _helpersController.LogMessages("Updating company's director...", _helpersController.getSessionEmail());
            return Json(result.Value);
        }






        public IActionResult AcceptLegal(string id)
        {

            if (id == null || string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps... This company was not found and cannot accept tearms and conditions. Please try again.") });
            }
            else
            {
                int cmid = Convert.ToInt32(generalClass.Decrypt(id));

                var company = (from c in _context.companies where c.id == cmid select c);

                if (company.Count() > 0)
                {
                    company.FirstOrDefault().isFirstTime = false;
                    company.FirstOrDefault().UpdatedAt = DateTime.UtcNow.AddHours(1);
                    company.FirstOrDefault().RoleId = 15;
                    string content = "Hello " + company.FirstOrDefault().name + ", Welcome to DEPOT Portal.";

                    if (_context.SaveChanges() > 0)
                    {
                        _helpersController.LogMessages("Company accepted legal conditions", company.FirstOrDefault().CompanyEmail);
                        return RedirectToAction("UserAuth", "Auth", new { email = company.FirstOrDefault().CompanyEmail});
                    }
                    else
                    {
                        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps... Something went wrong trying to accept your tearms and conditions. Please try again.") });
                    }
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Opps... This company was not found and cannot accept tearms and conditions. Please try again.") });
                }
            }
        }
    }
}




