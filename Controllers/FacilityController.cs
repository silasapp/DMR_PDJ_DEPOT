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
 

namespace NewDepot.Controllers
{
   //[Route("api")]
   //[ApiController]
    public class FacilityController : Controller
    {
        private readonly Depot_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;

        ElpsResponse elpsResponse = new ElpsResponse();

       Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;
        
        public FacilityController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }
       
        // GET: Facility
        public IActionResult Index(int? id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            if (id != null)
            {
                // Looking for specific Facility
            }
            else
            {
                if (userRole.Contains("COMPANY"))
                {
                    //Get all my Facilities

                    var facilities = _context.Facilities.Where(x =>x.CompanyId==userID && x.DeletedStatus != true).ToList();
                    facilities.ForEach(x =>
                    {
                        x.NoOfTanks = _context.Tanks.Where(a => a.FacilityId == x.Id).Count();
                        x.NoOfPumps = _context.Pumps.Where(a => a.FacilityId == x.Id).Count();

                        var add = (from ad in _context.addresses
                                   join sd in _context.States_UT on ad.StateId equals sd.State_id
                                   where ad.id == x.AddressId
                                   select new
                                   {
                                       address = ad.address_1 + "," + ad.city + " " + sd.StateName
                                   }).FirstOrDefault();

                        x.address_1 =  add ==null ? x.address_1 : add.address;
                    });

                    return View(facilities);
                }
                 
                else
                {
                    return RedirectToAction("AllFacilities");
                }
            }
            return View("Error");
        }

        public IActionResult AllFacilities()
        {
            var facilities = _context.Facilities.Where(x=>x.DeletedStatus!=true).ToList();
            var Companies = _context.companies.Where(x => x.DeleteStatus != true).ToList();
            ViewBag.Companies = Companies;

            facilities.ForEach(x =>
            {
                x.NoOfTanks = _context.Tanks.Where(a => a.FacilityId == x.Id).Count();
                x.NoOfPumps = _context.Pumps.Where(a => a.FacilityId == x.Id).Count();

                var add = (from ad in _context.addresses
                           join sd in _context.States_UT on ad.StateId equals sd.State_id
                           where ad.id==x.AddressId
                           select new
                           {
                               address=ad.address_1 +"," +ad.city+" "+sd.StateName
                           }).FirstOrDefault();
                x.address_1 = add!=null? add.address: x.address_1;
            });

            return View(facilities);
        }

        public IActionResult TopFacilities()
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {
                var myCompany = _context.companies.Where(a => a.id == userID).FirstOrDefault();
                if (myCompany != null)
                {
                    var myTopFac =(from f in _context.Facilities 
                                   where f.CompanyId == myCompany.id && f.DeletedStatus!= true
                                   select new
                                   {
                                       date=f.Date,
                                       name=f.Name,
                                       id=generalClass.Encrypt(f.Id.ToString())
                                   }).OrderByDescending(a => a.date).Take(5);
                    return Json(myTopFac);
                }
                return Json(0);
            }
            catch (Exception)
            {
                return Json(0);
            }
        }

        public IActionResult ViewFacility(string id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            int facID = generalClass.DecryptIDs(id);
            if ( facID == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, facility ID is not passed correctly. Please try again") });

            }

            var facility = _context.Facilities.Where(a => a.Id == facID).FirstOrDefault(); 
            if (facility != null)
            {
                var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                var LGA = _context.Lgas.Where(l => l.Id == address.LgaId).FirstOrDefault();
                var facState = address == null ? null : _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();

                string LGAa = LGA !=null ? LGA.Name: null + "," + facState+".";
                ViewBag.FacAddress = address.address_1 + address.city + LGAa + "," + facState.StateName +".";




                var dt = DateTime.Now.Date;
                var pms = (from p in _context.permits 
                           join a in _context.applications on p.application_id equals a.id
                           join f in _context.Facilities on a.FacilityId equals f.Id
                           where f.Id == facID /*&& p.date_issued >= dt*/ select p).ToList();
                //jadded
                facility.Tanks  = (from t in _context.Tanks.AsEnumerable()
                                            join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                                            where t.FacilityId == facility.Id 
                                            //&& (t.Status == null || (t.Status.Contains("Approved")))
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
                                                ModifyType = t.ModifyType,
                                                Status = t.Status
                                                
                                            }).ToList();

                facility.ApplicationTanks =(from t in _context.Tanks
                                            join at in _context.ApplicationTanks on t.Id equals at.TankId
                                            where t.FacilityId == facility.Id && t.DeletedStatus!=true 
                                            && t.Status.Contains("Processing")
                                            select new TankModel
                                            {
                                                Id =t.Id,
                                                Diameter = t.Diameter,
                                                Height = t.Height,
                                                MaxCapacity = t.MaxCapacity,
                                                Name = at.TankName,
                                                ProductName = _context.Products.Where(x => x.Id == at.ProductId).FirstOrDefault().Name,
                                                Status = t.Status

                                            }).ToList();
                facility.Pumps = _context.Pumps.Where(a => a.FacilityId == facility.Id).ToList();

                #region Loading Luggage
                #region Suitability
                var suit = _context.SuitabilityInspections.Where(a => a.FacilityId == facility.Id).FirstOrDefault();
                if (suit != null)
                {
                    ViewBag.Suitability = suit;
                   
                    var suitApp = _context.applications.Where(a => a.id == suit.ApplicationId).FirstOrDefault();
                    if (suitApp != null)
                    {

                    }
                }
                #endregion
                ViewBag.Luggage = pms;
                #endregion

            }
            else
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, this facility wasn't found. Please contact support") });

            }
            if (userRole.Contains("COMPANY"))
                return View(facility);
            else
                return View("_ViewFacility", facility);
        }

        public IActionResult CompanyFacilities(int id, string view)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var coy = _context.companies.Where(a => a.id == id).FirstOrDefault();
            if (coy != null)
            {
                ViewBag.CompanyName = coy.name;
                var facilities = _context.Facilities.Where(a => a.CompanyId == id).ToList();

                foreach (var fac in facilities)
                {
                    //fac.Tanks = _context.Tanks.Where(a => a.FacilityId == fac.Id).ToList();
                }

                if (!string.IsNullOrEmpty(view))
                    return View(view, facilities);
                return View(facilities);
            }
            else
            {
                return View("AppError");
            }
        }
        public JsonResult GetFacilityTanks(int id)
        {
            var tanks = _context.Tanks.Where(x=> x.FacilityId == id).ToList();
            return Json(tanks);

        }

        public JsonResult EditTank(int TankID, string TankName,string Capacity, string ProductName, string Height, string Diameter, string Status)
        {
            try
            {
                string result = "";

                    var get = _context.Tanks.Where(x => x.Id == TankID).FirstOrDefault();

                    if (get != null)
                    {
                        get.Name = TankName.TrimEnd().ToUpper();
                        get.Name =_context.Products.Where(x=> x.Id.ToString() == ProductName ).FirstOrDefault()?.Name;
                        get.ProductId =int.Parse(ProductName);
                        get.MaxCapacity = !string.IsNullOrEmpty(Capacity) ? Capacity: get.MaxCapacity;
                        get.Status = Status.Trim();
                        get.Height = double.Parse(Height);
                        get.Diameter = double.Parse(Diameter);
                        get.UpdatedAt = DateTime.Now;

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Tank Edited";
                        }
                        else
                        {
                            result = "Something went wrong trying to edit this tank";
                        }
                    }
                    else
                    {
                        result = "Something went wrong trying to find this tank.";
                    }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please contact support");
            }
        }      
        [Authorize(Policy = "AdminRoles")]
        public JsonResult EditPump(int PumpID, string PumpName, int TankID)
        {
            try
            {
                string result = "";

                    var get = _context.Pumps.Where(x => x.Id == PumpID).FirstOrDefault();

                    if (get != null)
                    {
                        get.Name = PumpName.TrimEnd().ToUpper();
                        get.TankId = TankID;

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Pump Edited";
                        }
                        else
                        {
                            result = "Something went wrong trying to edit this Pump";
                        }
                    }
                    else
                    {
                        result = "Something went wrong trying to find this Pump.";
                    }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please contact support");
            }
        }
        
        [Authorize(Policy = "AdminRoles")]
        public IActionResult DeleteTank(int id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            List<Tanks> _tankRepo = new List<Tanks>();
            var getApp = new applications();
            int i = 0;
            var tnk = _context.Tanks.Where(a => a.Id == id).FirstOrDefault();
            if (tnk != null)
            {
                var pps = _context.Pumps.Where(a => a.TankId == tnk.Id).ToList();
                foreach (var item in pps)
                {
                    if (item != null)
                    {
                        getApp = _context.applications.Where(x => x.FacilityId == pps.FirstOrDefault().FacilityId).FirstOrDefault();
                        pps.Remove(item);
                        _helpersController.SaveHistory(getApp.id, userID, "Tank deletion", "Tank" + pps.FirstOrDefault().Name + " deleted");
                       i= _context.SaveChanges();
                    }
                }
                if( i > 0)
                return RedirectToAction("ViewFacility", new { id = generalClass.Encrypt(tnk.FacilityId.ToString()) });
            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, the tank can not be found.") });
        }

        [Authorize(Policy = "AdminRoles")]
        public IActionResult DeletePump(int id)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            List<Pumps> _pumpRepo = new List<Pumps>();

            var pnp = _context.Pumps.Where(a => a.Id == id).FirstOrDefault();
            if (pnp != null)
            {
                var getApp = _context.applications.Where(x => x.FacilityId == pnp.FacilityId).FirstOrDefault();

                _pumpRepo.Remove(pnp);
                _helpersController.SaveHistory(getApp.id, userID, "Pump deletion", "Pump " + pnp.Name + " deleted");
                _context.SaveChanges();

                return RedirectToAction("ViewFacility", new { id = generalClass.Encrypt( pnp.FacilityId.ToString()) });
            }
            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, the pump/loading arm can not be found.") });
        }

        [AllowAnonymous, Route("Application/Forms/{code}/{hash}")]
        public JsonResult Application(string code, string hash)
        {
            string elpsSecretKey = _configuration.GetSection("ElpsKeys").GetSection("AppSecKey").Value.ToString();
            var hsh = PaymentRef.getHash(code + elpsSecretKey);

            if (hsh == hash)
            {
                var AppForms =(from u in _context.ApplicationForms 
                               join app in _context.applications on u.ApplicationId equals app.id
                               join fac in _context.Facilities on app.FacilityId equals fac.Id
                               join comp in _context.companies on app.company_id equals comp.id
                               select new {
                            u.Id, u.Filled, fac.Name, u.ApplicationId, app.company_id,
                            comp.name, u.FormId }).ToList();

                return Json(AppForms);
            }
            return Json(new { status = "Sorry, hash key did not match" });
        }
        [AllowAnonymous, Route("Application/Form/{id}/{hash}")]
        public JsonResult Application(int id, string hash)
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            string elpsSecretKey = _configuration.GetSection("ElpsKeys").GetSection("AppSecKey").Value.ToString();
            var hsh = PaymentRef.getHash(id.ToString() + elpsSecretKey);

            if (hsh == hash)
            {
                var AppForms = (from u in _context.ApplicationForms
                                join app in _context.applications on u.ApplicationId equals app.id
                                join fac in _context.Facilities on app.FacilityId equals fac.Id
                                join comp in _context.companies on app.company_id equals comp.id
                                select new
                                {
                                    u.Filled,
                                    u.Id,
                                    fac.Name,
                                    u.ApplicationId,
                                    app.company_id,
                                    comp.name,
                                    u.FormId
                                }).ToList();

                return Json(AppForms);
            }
            return Json(new { status = "Hash did not match" });
        }
        [HttpPost, AllowAnonymous, Route("Application/submitForm/{id}/{hash}")]
        //public IActionResult SubmitForm(int id, string hash, AppFormApiSubmitModel model)
        //{
        //    var respObj = new List<object>();
        //    try
        //    {

        //    //UtilityHelper.LogMessages(id + hash); 
        //    var hsh = PaymentRef.getHash(id + elpsSecretKey);
        //    //return Json(hsh);
        //    if (hsh == hash)
        //    {
        //        //get all the Facility that belongs the Companies
        //        var appForm = _appFormRep.FindBy(a => a.Id == id).FirstOrDefault();


        //        //var appForm = _appFormRep.FindBy(a => a.Id.ToString() == appFormId).FirstOrDefault();
        //        if (appForm != null)
        //        {
        //            // Id = Id.ToLower();
        //            List<Fields> formField = _FieldsRep.FindBy(a => a.FormId == id).ToList();

        //            Guid groupId = Guid.Empty;
        //            if (appForm.ValGroupId != null && appForm.ValGroupId != Guid.Empty)
        //            {
        //                groupId = appForm.ValGroupId.GetValueOrDefault();
        //            }
        //            else
        //            {
        //                groupId = Guid.NewGuid();
        //            }


        //            var listOfFldValue = new List<FieldValue>();
        //            foreach (var item in formField)
        //            {

        //                var formParameter = model.FieldAndValue.FirstOrDefault(a => a.FieldName.ToLower() == item.Label.ToLower());
        //                if (formParameter != null)
        //                {
        //                    var fieldV = _fieldValRep.FindBy(a => a.FieldId == item.Id && a.GroupId == groupId).FirstOrDefault();

        //                    if (fieldV == null)
        //                    {
        //                        fieldV = new FieldValue()
        //                        {
        //                            GroupId = groupId,
        //                            FieldId = item.Id,
        //                            Value = formParameter.Value
        //                        };
        //                        listOfFldValue.Add(fieldV);
        //                        _fieldValRep.Add(fieldV);
        //                    }
        //                    else
        //                    {
        //                        fieldV.Value = formParameter.Value;
        //                        listOfFldValue.Add(fieldV);
        //                        _fieldValRep.Edit(fieldV);
        //                    }

        //                    respObj.Add($"FieldName: {formParameter.FieldName}, was Saved correctly");

        //                }
        //                else
        //                {
        //                    respObj.Add($"We could not find any form item with this FieldName: {formParameter.FieldName}, please check again and resend");
        //                }

        //            }
        //            _fieldValRep.Save(userEmail, Request.UserHostAddress);

        //            #region Finishing
        //            //var reason = formCollection["reason"];
        //            //var recommend = formCollection["recommend"].ToLower() == "yes" ? true : false; // : null;;
        //            appForm.Filled = true;
        //            appForm.DateModified = UtilityHelper.CurrentTime;
        //                appForm.Reasons = model.Reasons;// reason;
        //                appForm.Recommend = model.Recommend;// recommend;
        //                appForm.ExtraReport1 = model.ExtraReport1;// ExtraReport1;
        //                appForm.ExtraReport2 = model.ExtraReport2;// ExtraReport2;
        //            appForm.ValGroupId = groupId;
        //            //get the staff that filled the form
        //            //var usr = userEmail;
        //            var stf = _vUsrBranchRep.FindBy(a => a.UserEmail.ToLower() == model.InspectorEmail).FirstOrDefault();
        //            if (stf != null)
        //            {
        //                appForm.StaffName = $"{stf.FirstName} {stf.LastName} ({model.InspectorEmail})";
        //            }
        //            //appForm.FieldAndValue = listOfFldValue;
        //            //return Json(appForm);
        //            _appFormRep.Edit(appForm);
        //            _appFormRep.Save(userEmail, Request.UserHostAddress);

        //            #region Log History
        //            var hist = new Application_Desk_History();
        //            hist.Application_Id = appForm.ApplicationId;
        //            hist.Date = UtilityHelper.CurrentTime;
        //            hist.Comment = "Application Form filled & submitted";
        //            hist.UserName = userEmail;
        //            hist.Status = "FilledForm";
        //            _appDHisRep.Add(hist);
        //            _appDHisRep.Save(userEmail, Request.UserHostAddress);
        //                #endregion
        //                return Json(new { status=1, message = respObj });
        //            #endregion
        //            //return RedirectToAction("Index", new { Id = Id, msg = "pass", companyId = companyId, applicationId = applicationId });
        //        }
        //        else
        //        {
        //            respObj.Add($"The Application form with the Id:{id} could not be found, please cross check and try again.");
        //        }

              
        //    }
        //    else
        //    {
        //        respObj.Add($"Your Hash, did not match, please make sure to salt your api key with form id befor computing the hash with SHA512");
        //    }


        //    }
        //    catch (Exception x)
        //    {
        //        UtilityHelper.LogMessages(x.ToString());
        //        respObj.Add($"some Error occured while processing your request, try again or contact the Admin");

        //    }

        //    return Json(new { status = 500, message = respObj });

        //}
        
        public IActionResult Updatee()
        {
            var facilities = generalClass.FacWithApplication().OrderBy(a=>a.fac.Name).ToList();

            return View(facilities);
        }
        public IActionResult Update()
        {
            var facilities = generalClass.FacWithApplication().OrderBy(a=>a.fac.Name).ToList();

            return View(facilities);
        }

        public IActionResult UpdateForm(int id)
        {
            var ap = (from facil in _context.Facilities.AsEnumerable()
                      join c in _context.companies.AsEnumerable() on facil.CompanyId equals c.id
                      join ad in _context.addresses.AsEnumerable() on facil.AddressId equals ad.id
                      join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                      where facil.DeletedStatus != true && facil.Id == id && c.DeleteStatus != true
                      select new MyApps
                      {
                          fac=facil,
                          FacilityId = facil.Id,
                          Current_Permit = "",
                          Address_1 = ad.address_1,
                          CompanyDetails = c.name,
                          City=ad.city,
                          CategoryCode=c.CompanyCode,
                          StateName = sd.StateName,
                          FacilityDetails = facil.Name,
                      }).FirstOrDefault();

            return View("UpdateForm", ap);
        }

        [HttpPost]
        public IActionResult Update(int id,string facName, string address, string city, string state,string facCode,string compCode, string cateCode, string FirstOperationYear)
        {
            var facility = _context.Facilities.Where(a => a.Id == id).FirstOrDefault();
            if (facility!=null)
            {
                facility.IdentificationCode = facCode;
                facility.CategoryCode = cateCode;
                facility.Name = facName;
                facility.address_1 = address;
                facility.city = city;
                facility.StateName = state;

                int fop = 0;
                int.TryParse(FirstOperationYear, out fop);
                if (fop<=0)
                {
                    fop = Convert.ToInt32(facCode.Substring(facCode.Length - 2));
                }
                facility.FirstOperationYear = fop;
                _context.SaveChanges();

                var facAddress = _context.addresses.Where(x => x.id == facility.AddressId).FirstOrDefault();
                if(facAddress!= null)
                {
                    var stat = _context.States_UT.Where(x => x.StateName.ToLower() == state.Trim().ToLower()).FirstOrDefault();
                    facAddress.address_1= address;
                    facAddress.city = city;

                    if (stat != null)
                        facAddress.StateId = stat.State_id;

                    _context.SaveChanges();
                }
                var comp = _context.companies.Where(a => a.id == facility.CompanyId).FirstOrDefault();
                if (comp!=null)
                {
                    if (!string.IsNullOrEmpty(compCode))
                    {
                        comp.CompanyCode = compCode;
                        _context.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Update");
        }
    }
}