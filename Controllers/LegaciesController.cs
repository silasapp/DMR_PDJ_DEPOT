using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using NewDepot.Helpers;
using NewDepot.Controllers.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewDepot.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace NewDepot.Controllers.Application
{
    //[Authorize]
    public class LegaciesController : Controller
    {
        RestSharpServices _restService = new RestSharpServices();
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public LegaciesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        //[Authorize(Policy = "CompanyRoles")]
        public IActionResult Apply()
        {
            var user_email = _helpersController.getSessionUserID();

           
                var getType = from a in _context.Categories.AsEnumerable()
                          join p in _context.Phases.AsEnumerable() on a.id equals p.category_id
                          where (a.name.ToLower().Contains("new depot")) && a.DeleteStatus != true && p.DeleteStatus != true
                          orderby a.id ascending
                          select new CategoryType
                          {
                              CatTypeId = p.id,
                              CategoryName = p.name,
                              PhaseName = p.name,
                              Counter = a.id,
                          };

                var getProduct = from cp in _context.Products
                                 where cp.DeletedStatus != true
                                 select new Products
                                 {
                                     Name = cp.Name,
                                     Id = cp.Id
                                 };

                if (getType.Count() > 0)
                {
                    var companyID = _helpersController.getSessionUserID();

                        ViewData["ApplyDescription"] = getType.FirstOrDefault().CategoryName;
                        ViewBag.Products = getProduct.ToList();
                        ViewBag.CategoryTypes = getType.ToList();
                        
                        var states = _context.States_UT.Where(x=> x.Country_id== 156).ToList();
                        ViewBag.States = states;

                        _helpersController.LogMessages("Loading company information for legacy application" + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());

                        return View();
                   
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, no application stages was found for the selected application") });
                }
            }
        public IActionResult UploadLegacyDocument(string id) // application id
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong. Application not found. Kindly contact support.") });
            }

            int legacy_id = Convert.ToInt32(generalClass.Decrypt(id));
            var checkLeg = _context.Legacies.Where(x => x.Id == legacy_id && x.DeleteStatus != true).FirstOrDefault();
            var facility = _context.Facilities.Where(a => a.Name.ToLower().Trim() == checkLeg.FacilityName.ToLower().Trim()).FirstOrDefault(); //lk change to facilityID

            int facility_id = facility.Id;
            ViewData["PermitNo"] = checkLeg.LicenseNo;
            ViewData["LegacyID"] = checkLeg.Id;
            ViewData["FacilityID"] = facility.Id;


            var getDocuments = _context.ApplicationDocuments.AsEnumerable().Where(x => x.docType == "Company" && x.DocName.Contains("LEGACY") && x.DeleteStatus != true);

            var facDetails = from fac in _context.Facilities 
                             join company in _context.companies on fac.CompanyId equals company.id
                             from doc in _context.ApplicationDocuments
                      where doc.DocName.Contains("LEGACY") && doc.DeleteStatus != true  && fac.DeletedStatus != true && company.DeleteStatus != true 
                      && fac.Id == facility_id
                             select new
                             {
                                 FacilityName = fac.Name,
                                 LocalFacilityID = fac.Id,
                                 ElpsFacilityID = fac.Elps_Id,
                                 LocalCompanyID = company.id,
                                 ElpsCompanyID = company.elps_id,
                                 AppDocID = doc.AppDocID,
                                 EplsDocTypeID = doc.ElpsDocTypeID,
                                 DocName = doc.DocName,
                                 docType = doc.docType,
                        
                             };

            List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
            List<MissingDocument> missingDocuments = new List<MissingDocument>();;
            List<BothDocuments> bothDocuments = new List<BothDocuments>();

            if (facDetails.Count() > 0)
            {
                ViewData["FacilityName"] = facDetails.FirstOrDefault().FacilityName;
                ViewData["CompanyElpsID"] = facDetails.FirstOrDefault().ElpsCompanyID;
                ViewData["FacilityElpsID"] = facDetails.FirstOrDefault().ElpsFacilityID;

                List<LpgLicense.Models.FacilityDocument> facilityDoc = generalClass.getFacilityDocuments(facDetails.FirstOrDefault().ElpsFacilityID.ToString());

                foreach (var appDoc in facDetails.ToList())
                {

                    if (facilityDoc != null)
                    {
                        foreach (var fDoc in facilityDoc.Where(d => d.Document_Type_Id == appDoc.EplsDocTypeID))
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
                var result = facDetails.AsEnumerable().Where(x => !presentDocuments.AsEnumerable().Any(x2 => x2.LocalDocID == x.AppDocID));


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

                bothDocuments.Add(new BothDocuments
                {
                    missingDocuments = missingDocuments,
                    presentDocuments = presentDocuments,
                });
            }

            _helpersController.LogMessages("Displaying legacy document upload for " + ViewData["FacilityName"], generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return View(bothDocuments.ToList());
        }




        ////[Authorize(Policy = "CompanyRoles")]
        public IActionResult Create( Legacies legacy,  string state, List<int> Products)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {

                string result = "";

                // geting local DB state id for saving
                var statee = _context.States_UT.Where(x => x.State_id== Convert.ToInt32(state)).FirstOrDefault();

                var Issuedate = DateTime.Parse(legacy.Issue_Date.ToString().Trim());
                var Expirydate = DateTime.Parse(legacy.Exp_Date.ToString().Trim());
                string products = string.Join(",", Products.ToList());
                int facilityID = 0;
                if (Issuedate > GeneralClass.PortalDate)
                {
                    result = "Sorry, you cannot perform this operation. Ref : Legacy Error";
                }
                else
                {
                    var Lga = _context.Lgas.Where(l => l.Id == int.Parse(legacy.LGA))?.FirstOrDefault();

                    if (statee != null)
                    {
                        // check if permit has been entered before.
                        var checkLegacy = _context.Legacies.Where(x => x.LicenseNo.ToUpper() == legacy.LicenseNo.ToUpper() && x.Status!=null && x.DeleteStatus != true).FirstOrDefault();

                        if (checkLegacy != null)
                        {
                            result = "Sorry, this permit has already been entered before.";
                        }
                        else
                        {
                            //update legacy

                            var checkLeg = new Legacies();
                            checkLeg.CompName = userName;
                            checkLeg.CompId = userID.ToString();
                            checkLeg.LicenseNo = legacy.LicenseNo.ToUpper();
                            checkLeg.Issue_Date = Issuedate.ToString();
                            checkLeg.Exp_Date = Expirydate.ToString();
                            checkLeg.FacilityName = legacy.FacilityName;
                            checkLeg.FacilityAddress = legacy.FacilityAddress;
                            checkLeg.State = statee.StateName;
                            checkLeg.LGA = Lga.Name;
                            checkLeg.City = legacy.City;
                            checkLeg.AppType = legacy.AppType;
                            checkLeg.LandMeters = legacy.LandMeters;
                            checkLeg.IsPipeline = legacy.IsPipeline;
                            checkLeg.IsHighTension = legacy.IsHighTension;
                            checkLeg.IsHighway = legacy.IsHighway;
                            checkLeg.ContactName = legacy.ContactName;
                            checkLeg.ContactPhone = legacy.ContactPhone;
                            checkLeg.CreatedAt = DateTime.Now;
                            checkLeg.DeleteStatus = false;
                            checkLeg.Products = products;
                            checkLeg.IsUsed = false;
                                     _context.Legacies.Add(checkLeg);
                                    if (_context.SaveChanges() > 0)
                                    {

                                    int elspStateID = generalClass.GetStatesFromCountry(statee.State_id.ToString());

                                    //getting company elps id from loacal DB to save to elps facility
                                    var company = _context.companies.Where(x => x.id == Convert.ToInt16(checkLeg.CompId));

                                    var compId = company.FirstOrDefault().id;
                                    var compEmail = company.FirstOrDefault().CompanyEmail;
                                    var compName = company.FirstOrDefault().name;
                                    var compElpsID = company.FirstOrDefault().elps_id;

                                    // already exiting facility on local DB
                                    var facilityy = _context.Facilities.Where(x => x.Name.ToUpper() == checkLeg.FacilityName.ToUpper() && x.CompanyId.ToString() == checkLeg.CompId).FirstOrDefault();

                                    if (facilityy == null ) 
                                    { 
                                        int done = 0;

                                        Facilities _facilities = new Facilities()
                                        {
                                            CompanyId = Convert.ToInt16(checkLeg.CompId),
                                            Name = checkLeg.FacilityName,
                                            address_1 = checkLeg.FacilityAddress,
                                            StateName = checkLeg.State,
                                            LGA = checkLeg.LGA,
                                            city = checkLeg.City,
                                            Date = DateTime.Now,
                                            DeletedStatus = false,
                                            ContactName = checkLeg.ContactName,
                                            ContactNumber = checkLeg.ContactPhone
                                        };

                                        // saving facility to local DB
                                        _context.Facilities.Add(_facilities);
                                        done = _context.SaveChanges();

                                    ////save facility suitability information
                                    var suitInfo = new SuitabilityInspections()
                                    {
                                        SizeOfLand = legacy.LandMeters.ToString(),
                                        ISAlongPipeLine = legacy.IsPipeline,
                                        IsOnHighWay = legacy.IsHighway,
                                        IsUnderHighTension = legacy.IsHighTension,
                                        CompanyId = company.FirstOrDefault().id,
                                        FacilityId = _facilities.Id,
                                        ApplicationId = 0
                                    };
                                    _context.SuitabilityInspections.Add(suitInfo);
                                    done = _context.SaveChanges();

                                    var add = new addresses
                                        {
                                            elps_id = 0,
                                            address_1 = checkLeg.FacilityAddress,
                                            city = checkLeg.City,
                                            country_id = 156,
                                            LgaId = Lga.Id,
                                            StateId = _context.States_UT.Where(s => s.StateName == checkLeg.State).FirstOrDefault().State_id,
                                        };
                                        _context.addresses.Add(add);
                                        done = _context.SaveChanges();

                                        if (done > 0)
                                        {
                                            _facilities.AddressId = add.id;
                                            _context.SaveChanges();

                                            facilityID = _facilities.Id;

                                            LpgLicense.Models.Facility facility = new LpgLicense.Models.Facility()
                                            {
                                                Name = checkLeg.FacilityName,
                                                CompanyID = (int)company.FirstOrDefault().elps_id,
                                                StreetAddress = add.address_1,
                                                City = _facilities.city,
                                                FacilityType = "Depot",
                                                StateId = add.StateId,
                                                DateAdded = DateTime.Now,
                                                LGAId = (int)add.LgaId,

                                            };

                                            // saving new facility to elps
                                            var response2 = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facility);
                                            var res = JsonConvert.DeserializeObject<LpgLicense.Models.Facility>(response2.Content);

                                            // updating new epls facility id with local DB
                                            var updateFacility = _context.Facilities.Where(x => x.Id == facilityID).FirstOrDefault();
                                            int fac_saved = 0;
                                            if (res == null)
                                            {
                                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem has occured while trying to post your facilty details to ELPS. Please contact support.") });

                                            }
                                            else
                                            {
                                                updateFacility.Elps_Id = res.Id;
                                                fac_saved = _context.SaveChanges();
                                            }
                                            
                                            if (fac_saved > 0)
                                            {
                                                result = "Legacy Submitted|"+ generalClass.Encrypt(checkLeg.Id.ToString());
                                            }
                                            else
                                            {
                                                result = "Something went wrong trying to create your facility, please try again.";
                                            }
                                        }
                                        else
                                        {
                                            result = "Something went wrong trying to save this new facility from legacy application.";
                                        }
                                    }
                                    else 
                                    {
                                    //just update tables where applicable
                                        int done = 0;
                                    facilityID = facilityy.Id;

                                    facilityy.Name = checkLeg.FacilityName;
                                    facilityy.address_1 = checkLeg.FacilityAddress;
                                    facilityy.StateName = checkLeg.State;
                                    facilityy.LGA = Lga.Name;
                                    facilityy.city = checkLeg.City;
                                    facilityy.Date = DateTime.Now;
                                    facilityy.DeletedStatus = false;
                                    facilityy.ContactName = checkLeg.ContactName;
                                    facilityy.ContactNumber = checkLeg.ContactPhone;

                                    // saving facility to local DB
                                    done = _context.SaveChanges();

                                    var facAddress = _context.addresses.Where(a => a.id == facilityy.AddressId).FirstOrDefault();

                                    if (facAddress== null){
                                        var add = new addresses
                                        {
                                            elps_id = 0,
                                            address_1 = checkLeg.FacilityAddress,
                                            city = checkLeg.City,
                                            country_id = 156,
                                            LgaId = Lga.Id,
                                            StateId = _context.States_UT.Where(s => s.StateName == checkLeg.State).FirstOrDefault().State_id,
                                        };
                                        _context.addresses.Add(add);
                                        done = _context.SaveChanges();
                                        facilityy.AddressId = add.id;
                                        _context.SaveChanges();

                                    }
                                    else
                                    {
                                        //just update address table with new info
                                        facAddress.elps_id = 0;
                                        facAddress.address_1 = checkLeg.FacilityAddress;
                                        facAddress.city = checkLeg.City;
                                        facAddress.country_id = 156;
                                        facAddress.LgaId = Lga.Id;
                                        facAddress.StateId = _context.States_UT.Where(s => s.StateName == checkLeg.State).FirstOrDefault().State_id;
                                        done= _context.SaveChanges();

                                    }
                                    if (done > 0)
                                    {
                                        int fac_saved = 0;

                                        if (facilityy.Elps_Id == null)// has not been pushed to ELPS
                                        {
                                            LpgLicense.Models.Facility facility = new LpgLicense.Models.Facility()
                                            {
                                                Name = checkLeg.FacilityName,
                                                CompanyID = (int)company.FirstOrDefault().elps_id,
                                                StreetAddress = facAddress.address_1,
                                                City = facAddress.city,
                                                FacilityType = "Depot",
                                                StateId = facAddress.StateId,
                                                DateAdded = DateTime.Now,
                                                LGAId = Lga.Id,

                                            };

                                            // saving new facility to elps
                                            var response2 = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facility);
                                            var res = JsonConvert.DeserializeObject<LpgLicense.Models.Facility>(response2.Content);

                                            // updating new epls facility id with local DB
                                            var updateFacility = _context.Facilities.Where(x => x.Id == facilityID).FirstOrDefault();
                                            fac_saved = 0;
                                            if (res == null)
                                            {
                                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem has occured while trying to post your facilty details to ELPS. Please contact support.") });

                                            }
                                            else
                                            {
                                                updateFacility.Elps_Id = res.Id;
                                                fac_saved = _context.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            fac_saved = 1;
                                        }
                                        if (fac_saved > 0)
                                        {
                                            result = "Legacy Submitted|" + generalClass.Encrypt(checkLeg.Id.ToString());
                                        }
                                        else
                                        {
                                            result = "Something went wrong trying to create your facility, please try again.";
                                        }
                                    }
                                    else
                                    {
                                        result = "Something went wrong trying to save this new facility from legacy application.";
                                    }
                                }

                            }

                            else
                                {
                                result = "Something went wrong trying to save your legacy application. Please try again later";
                                }
                               
                        }
                    }
                    else
                    {
                        result = "Application state selected not found.";
                    }

                }

                _helpersController.LogMessages("Legacy application status : " + result + ". Legacy information : " + legacy.LicenseNo, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }


        }
        ////[Authorize(Roles = "COMPANY")]
       
        //[Authorize(Policy = "CompanyRoles")]
        public IActionResult ContinueLegacy(string id)
        {
            int legid = 0;

            var LegID = generalClass.Decrypt(id);

            if (LegID == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Legacy Application link not found. Kindly contact support.") });
            }
            else
            {
                legid = Convert.ToInt32(LegID);
                var getLeg = GetLegacy(legid);
                var get = getLeg.Where(x => x.Legacy.Id == legid);

                var gets = from l in _context.Legacies.AsEnumerable()
                           join cat in _context.Phases.AsEnumerable() on l.AppType equals cat.name
                           where l.Id == legid && cat.DeleteStatus != true
                           select new CategoryType
                           {
                               CatTypeId = cat.id,
                               CategoryName = cat.name
                           };


                var getCategoryProduct = from cp in _context.Products
                                         where cp.DeletedStatus != true
                                         select new
                                         {
                                             Name = cp.Name,
                                             ProductId = cp.Id
                                         };

                ViewData["ResubmitTitle"] = get.FirstOrDefault().Legacy.LicenseNo;

                var companyID = _helpersController.getSessionUserID();
                var facility = _context.Facilities.Where(a => a.Name.ToLower().Trim() == get.FirstOrDefault().Legacy.FacilityName.ToLower().Trim()).FirstOrDefault(); //lk change to facilityID


                    var bothDocuments = new List<BothDocuments>();

                    bothDocuments.Add(new BothDocuments
                    {   
                        legacyModels = get.ToList()
                    });

                    var getType = from a in _context.Categories.AsEnumerable()
                                  join p in _context.Phases.AsEnumerable() on a.id equals p.category_id
                                  where (a.name.ToLower().Contains("new depot") || p.ShortName == "NDT") && a.DeleteStatus != true && p.DeleteStatus != true
                                  orderby a.id ascending
                                  select new CategoryType
                                  {
                                      CatTypeId = p.id,
                                      CategoryName = p.name,
                                      PhaseName = p.name,
                                      Counter = a.id,
                                  };

                    ViewBag.Products = getCategoryProduct.ToList();
                    ViewBag.CategoryTypes = getType.ToList();
                    var states = _context.States_UT.Where(x => x.Country_id == 156).ToList();
                    ViewBag.States = states;
                    _helpersController.LogMessages("Loading company information, legacy information and document for continuation legacy application " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());

                    return View(bothDocuments.ToList());
                }
              
        }

        [Authorize(Policy = "CompanyRoles")]
        public IActionResult Continue(Legacies legacy, string state, List<int> Products)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {

                string result = "";

                // geting local DB state id for saving
                var statee = _context.States_UT.Where(x => x.State_id == Convert.ToInt32(state)).FirstOrDefault();
                var Lga = _context.Lgas.Where(l => l.Id == int.Parse(legacy.LGA))?.FirstOrDefault();

                var Issuedate = DateTime.Parse(legacy.Issue_Date.ToString().Trim());
                var Expirydate = DateTime.Parse(legacy.Exp_Date.ToString().Trim());
                string products = string.Join(",", Products.ToList());
                int facilityID = 0;
                if (Issuedate > GeneralClass.PortalDate)
                {
                    result = "Sorry, you cannot perform this operation. Ref : Legacy Error";
                }
                else
                {
                    if (statee != null)
                    {
                        // check if permit has been entered before.
                        var checkLeg = _context.Legacies.Where(x => x.Id == legacy.Id && x.DeleteStatus != true).FirstOrDefault();

                        if (checkLeg == null)
                        {
                            result = "Sorry, this legacy application can not be found, kindly apply afresh.";
                        }
                        else
                        {
                            //update legacy

                            checkLeg.CompId = userID.ToString();
                            checkLeg.LicenseNo = legacy.LicenseNo.ToUpper();
                            checkLeg.Issue_Date = Issuedate.ToString();
                            checkLeg.Exp_Date = Expirydate.ToString();
                            checkLeg.FacilityName = legacy.FacilityName;
                            checkLeg.FacilityAddress = legacy.FacilityAddress;
                            checkLeg.State = statee.StateName;
                            checkLeg.LGA = Lga.Name;
                            checkLeg.City = legacy.City;
                            checkLeg.AppType = legacy.AppType;
                            checkLeg.LandMeters = legacy.LandMeters;
                            checkLeg.IsPipeline = legacy.IsPipeline;
                            checkLeg.IsHighTension = legacy.IsHighTension;
                            checkLeg.IsHighway = legacy.IsHighway;
                            checkLeg.ContactName = legacy.ContactName;
                            checkLeg.ContactPhone = legacy.ContactPhone;
                            checkLeg.UpdatedAt = DateTime.Now;
                            checkLeg.DeleteStatus = false;
                            checkLeg.Products = products;
                            if (_context.SaveChanges() > 0)
                            {

                                int elspStateID = generalClass.GetStatesFromCountry(statee.State_id.ToString());

                                //getting company elps id from loacal DB to save to elps facility
                                var company = _context.companies.Where(x => x.id == Convert.ToInt16(checkLeg.CompId));

                                var compId = company.FirstOrDefault().id;
                                var compEmail = company.FirstOrDefault().CompanyEmail;
                                var compName = company.FirstOrDefault().name;
                                var compElpsID = company.FirstOrDefault().elps_id;

                                // already exiting facility on local DB
                                var facilityy = _context.Facilities.Where(x => x.Name.ToUpper() == checkLeg.FacilityName.ToUpper() && x.CompanyId.ToString() == checkLeg.CompId).FirstOrDefault();

                                if (facilityy == null)
                                {
                                    int done = 0;

                                    Facilities _facilities = new Facilities()
                                    {
                                        CompanyId = Convert.ToInt16(checkLeg.CompId),
                                        Name = checkLeg.FacilityName,
                                        address_1 = checkLeg.FacilityAddress,
                                        StateName = checkLeg.State,
                                        LGA = Lga.Name,
                                        city = checkLeg.City,
                                        Date = DateTime.Now,
                                        DeletedStatus = false,
                                        ContactName = checkLeg.ContactName,
                                        ContactNumber = checkLeg.ContactPhone
                                    };

                                    // saving facility to local DB
                                    _context.Facilities.Add(_facilities);
                                    done = _context.SaveChanges();

                                    var add = new addresses
                                    {
                                        elps_id = 0,
                                        address_1 = checkLeg.FacilityAddress,
                                        city = checkLeg.City,
                                        country_id = 156,
                                        LgaId = Lga.Id,
                                        StateId = _context.States_UT.Where(s => s.StateName == checkLeg.State).FirstOrDefault().State_id,
                                    };
                                    _context.addresses.Add(add);
                                    done = _context.SaveChanges();

                                    if (done > 0)
                                    {
                                        _facilities.AddressId = add.id;
                                        _context.SaveChanges();

                                        facilityID = _facilities.Id;

                                        LpgLicense.Models.Facility facility = new LpgLicense.Models.Facility()
                                        {
                                            Name = checkLeg.FacilityName,
                                            CompanyID = (int)company.FirstOrDefault().elps_id,
                                            StreetAddress = add.address_1,
                                            City = _facilities.city,
                                            FacilityType = "Depot",
                                            StateId = add.StateId,
                                            DateAdded = DateTime.Now,
                                            LGAId = Lga.Id,

                                        };

                                        // saving new facility to elps
                                        var response2 = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facility);
                                        var res = JsonConvert.DeserializeObject<LpgLicense.Models.Facility>(response2.Content);

                                        // updating new epls facility id with local DB
                                        var updateFacility = _context.Facilities.Where(x => x.Id == facilityID).FirstOrDefault();
                                        int fac_saved = 0;
                                        if (res == null)
                                        {
                                            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem has occured while trying to post your facilty details to ELPS. Please contact support.") });

                                        }
                                        else
                                        {
                                            updateFacility.Elps_Id = res.Id;
                                            fac_saved = _context.SaveChanges();
                                        }

                                        if (fac_saved > 0)
                                        {
                                            result = "Legacy Submitted|" + generalClass.Encrypt(checkLeg.Id.ToString());
                                        }
                                        else
                                        {
                                            result = "Something went wrong trying to create your facility, please try again.";
                                        }
                                    }
                                    else
                                    {
                                        result = "Something went wrong trying to save this new facility from legacy application.";
                                    }
                                }
                                else
                                {
                                    //just update tables where applicable
                                    int done = 0;
                                    facilityID = facilityy.Id;

                                    facilityy.Name = checkLeg.FacilityName;
                                    facilityy.address_1 = checkLeg.FacilityAddress;
                                    facilityy.StateName = checkLeg.State;
                                    facilityy.LGA = Lga.Name;
                                    facilityy.city = checkLeg.City;
                                    facilityy.Date = DateTime.Now;
                                    facilityy.DeletedStatus = false;
                                    facilityy.ContactName = checkLeg.ContactName;
                                    facilityy.ContactNumber = checkLeg.ContactPhone;

                                    // saving facility to local DB
                                    done += _context.SaveChanges();

                                    var facAddress = _context.addresses.Where(a => a.id == facilityy.AddressId).FirstOrDefault();
                                    if (facAddress == null)
                                    {
                                        var add = new addresses
                                        {
                                            elps_id = 0,
                                            address_1 = checkLeg.FacilityAddress,
                                            city = checkLeg.City,
                                            country_id = 156,
                                            LgaId = Lga.Id,
                                            StateId = _context.States_UT.Where(s => s.StateName == checkLeg.State).FirstOrDefault().State_id,
                                        };
                                        _context.addresses.Add(add);
                                        done += _context.SaveChanges();
                                        facilityy.AddressId = add.id;
                                        _context.SaveChanges();

                                    }
                                    else
                                    {
                                        //just update address table with new info
                                        facAddress.elps_id = 0;
                                        facAddress.address_1 = checkLeg.FacilityAddress;
                                        facAddress.city = checkLeg.City;
                                        facAddress.country_id = 156;
                                        facAddress.LgaId =Lga!=null? Lga.Id: facAddress.LgaId;
                                        facAddress.StateId = _context.States_UT.Where(s => s.StateName.ToLower() == checkLeg.State.ToLower()).FirstOrDefault().State_id;
                                        done += _context.SaveChanges();

                                    }
                                    if (done > 0)
                                    {
                                        int fac_saved = 0;

                                        if (facilityy.Elps_Id == null)// has not been pushed to ELPS
                                        {
                                            LpgLicense.Models.Facility facility = new LpgLicense.Models.Facility()
                                            {
                                                Name = checkLeg.FacilityName,
                                                CompanyID = (int)company.FirstOrDefault().elps_id,
                                                StreetAddress = facAddress.address_1,
                                                City = facAddress.city,
                                                FacilityType = "Depot",
                                                StateId = facAddress.StateId,
                                                DateAdded = DateTime.Now,
                                                LGAId = Lga.Id,

                                            };

                                            // saving new facility to elps
                                            var response2 = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facility);
                                            var res = JsonConvert.DeserializeObject<LpgLicense.Models.Facility>(response2.Content);

                                            // updating new epls facility id with local DB
                                            var updateFacility = _context.Facilities.Where(x => x.Id == facilityID).FirstOrDefault();
                                            fac_saved = 0;
                                            if (res == null)
                                            {
                                                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem has occured while trying to post your facilty details to ELPS. Please contact support.") });

                                            }
                                            else
                                            {
                                                updateFacility.Elps_Id = res.Id;
                                                fac_saved = _context.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            fac_saved = 1;
                                        }
                                        if (fac_saved > 0)
                                        {
                                            result = "Legacy Submitted|" + generalClass.Encrypt(checkLeg.Id.ToString());
                                        }
                                        else
                                        {
                                            result = "Something went wrong trying to create your facility, please try again.";
                                        }
                                    }
                                    else
                                    {
                                        result = "Something went wrong trying to save this new facility from legacy application.";
                                    }
                                }

                            }

                            else
                            {
                                result = "Something went wrong trying to save your legacy application. Please try again later";
                            }

                        }
                    }
                    else
                    {
                        result = "Application state selected not found.";
                    }

                }

                _helpersController.LogMessages("Legacy application status : " + result + ". Legacy information : " + legacy.LicenseNo, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }


        }

        public IActionResult EditLegacy(Legacies legacy, string state, List<int> Products)
        {
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            try
            {

                string result = "";

                // geting local DB state id for saving
                var statee = _context.States_UT.Where(x => x.State_id == Convert.ToInt32(state)).FirstOrDefault();
                //var Issuedate = DateTime.Parse(legacy.Issue_Date.ToString().Trim());
                //var Expirydate = DateTime.Parse(legacy.Exp_Date.ToString().Trim());
                //string products = string.Join(",", Products.ToList());
                //int facilityID = 0;

                //if (statee != null)
                //{
                    // check if permit has been entered before.
                    var checkLeg = _context.Legacies.Where(x => x.Id == legacy.Id && x.DeleteStatus != true).FirstOrDefault();

                    if (checkLeg == null)
                    {
                        result = "Sorry, this legacy application can not be found.";
                    }
                    else
                    {
                        //update legacy
                       
                        checkLeg.FacilityName = legacy.FacilityName;
                        checkLeg.FacilityAddress = legacy.FacilityAddress;
                        checkLeg.AppType = legacy.AppType;
                        checkLeg.IsUsed = legacy.IsUsed;
                        checkLeg.UpdatedAt = DateTime.Now;
                        if (userRole == GeneralClass.SUPER_ADMIN && legacy.LicenseNo!="undefined")
                        {
                            checkLeg.LicenseNo = legacy.LicenseNo.ToUpper();
                        }
                    //checkLeg.Issue_Date = Issuedate.ToString();
                    //checkLeg.Exp_Date = Expirydate.ToString();
                    // checkLeg.State = statee.StateName;
                    // checkLeg.LGA = legacy.LGA;
                    //checkLeg.City = legacy.City;
                    //checkLeg.Products = products;
                    int save = _context.SaveChanges();
                    if (save > 0)
                        {

                            //getting company elps id from loacal DB to save to elps facility
                            var company = _context.companies.Where(x => x.id == Convert.ToInt16(checkLeg.CompId));

                            // already exiting facility on local DB
                            var facility = _context.Facilities.Where(x => x.Name.ToUpper() == checkLeg.FacilityName.ToUpper()).FirstOrDefault();

                            if (facility != null)
                            {
                                facility.Name = checkLeg.FacilityName;
                                facility.address_1 = checkLeg.FacilityAddress;
                                _context.SaveChanges();
                            }

                            result = "Legacy Updated";

                        }
                        else
                        {
                            result = "Something went wrong trying to update this legacy information.";
                        }
                    }


                    return Json(result);
                //}
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please contact support");
            }


        }


        public IActionResult SubmitDocuments(int FacilityID,int LegacyID, List<SubmitDoc> legacyDocuments)
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();

            var result = "";
            try
            {
                var checkLeg = _context.Legacies.Where(x => x.Id == LegacyID && x.DeleteStatus != true);
                bool isHeadOffice = false;

                string RoleName = GeneralClass.SUPERVISOR;

                // geting local DB state id for saving
                var statee = _context.States_UT.Where(x => x.StateName == checkLeg.FirstOrDefault().State).FirstOrDefault();

                    if (statee != null)
                    {
                        
                            var getAppLocation = _helpersController.GetLegacyApplicationOffice(statee.State_id, isHeadOffice);

                            if (getAppLocation.Count() > 0)
                            {
                                var getStaff = from s in _context.Staff.AsEnumerable()
                                               join r in _context.UserRoles.AsEnumerable() on s.RoleID equals r.Role_id
                                               join l in _context.Location.AsEnumerable() on s.LocationID equals l.LocationID
                                               where l.LocationName == GeneralClass.HQ && r.RoleName == RoleName && s.DeleteStatus != true && s.ActiveStatus != false
                                               select new
                                               {
                                                   StaffId = s.StaffID,
                                                   StaffEmail = s.StaffEmail,
                                                   StaffName = s.LastName + " " + s.FirstName
                                               };

                                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));

                                if (getStaff.Count() > 0){

                                     checkLeg.FirstOrDefault().Status = GeneralClass.Processing;
                                     checkLeg.FirstOrDefault().DropStaffId = getStaff.FirstOrDefault().StaffId;
                                     checkLeg.FirstOrDefault().AppDocId = legacyDocuments.FirstOrDefault().LocalDocID;
                                     checkLeg.FirstOrDefault().DocSource = legacyDocuments.FirstOrDefault().DocSource;
                                     checkLeg.FirstOrDefault().CompElpsDocId = legacyDocuments.FirstOrDefault().CompElpsDocID;
                                     checkLeg.FirstOrDefault().UpdatedAt = DateTime.Now;
                                

                                    if (_context.SaveChanges() > 0)
                                    {
                                        string reslink = "/Legacies/MyLegacy";
                                        result = "1|" + reslink;

                                        string subj = "Legacy Application (" + checkLeg.FirstOrDefault().LicenseNo.ToUpper() + ") is on your desk";
                                        string cont = "A legacy application with permit number : " + checkLeg.FirstOrDefault().LicenseNo.ToUpper() + " has been submitted on your desk for verification.";
                                        var msg = _helpersController.SendEmailMessageSBJAsync(getStaff.FirstOrDefault().StaffEmail, getStaff.FirstOrDefault().StaffName, subj, cont, GeneralClass.STAFF_NOTIFY, null);

                                        string subject = "Legacy Application Submitted with Permit No : " + checkLeg.FirstOrDefault().LicenseNo.ToUpper();
                                        string content = "You have submitted your legacy application with permit number " + checkLeg.FirstOrDefault().LicenseNo.ToUpper() + " from processing on NMDPRA Depot portal. Kindly find application details below.";

                                        var company = _context.companies.Where(x => x.id == _helpersController.getSessionUserID());
                                        var email = _helpersController.SendEmailMessageSBJAsync(_helpersController.getSessionEmail(), company.FirstOrDefault().name, subject, content, GeneralClass.COMPANY_NOTIFY, null);

                                    }
                                    else
                                    {
                                        result = "0|Something went wrong trying to submit your legacy application. Please try again later";
                                    }
                                }
                                else
                                {
                                    result = "0|Something went wrong, processing staff not found. Please try again later or contact support.";
                                }
                            }
                            else
                            {
                                result = "0|Something went wrong trying to identify this application location, please contact support.";
                            }
                        
                    }
                    else
                    {
                        result = "0|Application state selected not found.";
                    }

                _helpersController.LogMessages("Legacy application status : " + result + ". Legacy information : " + checkLeg.FirstOrDefault().LicenseNo, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }

        }



        /*
        * Getting all legacy application for a particular company 
        * And getting a legacy by its id
        * 
        * id => NOT encrypted legacy id
        */
        public List<LegacyModel> GetLegacy(int id = 0)
        {
            var get = (from l in _context.Legacies
                      join s in _context.States_UT.AsEnumerable() on l.State equals s.StateName
                      join c in _context.companies.AsEnumerable() on Convert.ToInt32(l.CompId) equals c.id
                      join cat in _context.Phases.AsEnumerable() on l.AppType equals cat.name
                      where l.CompId == _helpersController.getSessionUserID().ToString() && l.DeleteStatus != true
                      select new LegacyModel
                      {
                          Legacy = l,
                          Company = c,
                          State = s,
                          Category = cat.name 
                      });
            if (id != 0)
            {
                get.Where(x => x.Legacy.Id == id);
            }


            return get.ToList();
        }

        //[Authorize(Policy = "CompanyRoles")]
        public IActionResult MyLegacy()
        {
            var get = GetLegacy();
            _helpersController.LogMessages("Displaying company's legacy for " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());
            return View(get.ToList());
        }



        //[Authorize(Policy = "CompanyRoles")]
        public JsonResult DeleteLegacy(string LegID)
        {
            try
            {
                string result = "";

                int leg_id = 0;

                var legID = generalClass.Decrypt(LegID);

                if (legID == "Error")
                {
                    result = "Legacy Application link not found. Kindly contact support.";
                }
                else
                {
                    leg_id = Convert.ToInt32(legID);

                    var get = _context.Legacies.Where(x => x.Id == leg_id);

                    if (get.Count() > 0)
                    {
                        get.FirstOrDefault().DeleteStatus = true;
                        get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();
                        get.FirstOrDefault().DeletedAt = DateTime.Now;

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Legacy Deleted";
                        }
                        else
                        {
                            result = "Something went wrong trying to delete your legacy licence. Please try again later.";
                        }
                    }
                    else
                    {
                        result = "Legacy application not found. Kindly contact support.";
                    }
                }

                _helpersController.LogMessages("Legacy application status : " + result + ". Legacy information ID : " + leg_id, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }

        }



        //[Authorize(Policy = "CompanyRoles")]
        public IActionResult Resubmit(string id)
        {
            int legid = 0;

            var LegID = generalClass.Decrypt(id);

            if (LegID == "Error")
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Legacy Application link not found. Kindly contact support.") });
            }
            else
            {
                legid = Convert.ToInt32(LegID);
                var getLeg = GetLegacy(legid);
                var get = getLeg.Where(x => x.Legacy.Id == legid);

                var gets = from l in _context.Legacies.AsEnumerable()
                            join cat in _context.Phases.AsEnumerable() on l.AppType equals cat.name
                            where l.Id == legid && cat.DeleteStatus != true 
                            select new CategoryType
                            {
                                CatTypeId = cat.id,
                                CategoryName =cat.name
                            };


                var getCategoryProduct = from cp in _context.Products
                                         where cp.DeletedStatus != true
                                         select new 
                                         {
                                             Name = cp.Name,
                                             ProductId = cp.Id
                                         };

                ViewData["ResubmitTitle"] = get.FirstOrDefault().Legacy.LicenseNo;

                var companyID = _helpersController.getSessionUserID();
                var facility = _context.Facilities.Where(a => a.Name.ToLower().Trim() == get.FirstOrDefault().Legacy.FacilityName.ToLower().Trim()).FirstOrDefault(); //lk change to facilityID

                var getCompany = _context.companies.AsEnumerable().Where(x => x.id == companyID && x.DeleteStatus != true);
                var getDocuments = _context.ApplicationDocuments.AsEnumerable().Where(x => x.docType == "Company" && x.DocName.Contains("LEGACY") && x.DeleteStatus != true);

                List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
                List<MissingDocument> missingDocuments = new List<MissingDocument>();
                List<BothDocuments> bothDocuments = new List<BothDocuments>();

                if (getCompany.Count() > 0 && getDocuments.Count() > 0 && gets.Count() > 0 && getCategoryProduct.Count() > 0)
                {
                    ViewData["CompanyElpsID"] = getCompany.FirstOrDefault().elps_id;
                    ViewData["FacilityElpsID"] = facility.Elps_Id;

                    List<LpgLicense.Models.FacilityDocument> facilityDoc = generalClass.getFacilityDocuments(facility.Elps_Id.ToString());


                    if (facilityDoc != null)
                    {

                        if (facilityDoc != null)
                        {
                            foreach (var fDoc in facilityDoc.Where(d => d.Document_Type_Id == getDocuments.FirstOrDefault().ElpsDocTypeID))
                            {
                                presentDocuments.Add(new PresentDocuments
                                {
                                    Present = true,
                                    FileName = fDoc.Name,
                                    Source = fDoc.Source,
                                    CompElpsDocID = fDoc.Id,
                                    DocTypeID = fDoc.Document_Type_Id,
                                    LocalDocID = getDocuments.FirstOrDefault().AppDocID,
                                    DocType = getDocuments.FirstOrDefault().docType,
                                    TypeName = getDocuments.FirstOrDefault().DocName

                                });
                            }
                        }
                    }
                    var result = getDocuments.AsEnumerable().Where(x => !presentDocuments.AsEnumerable().Any(x2 => x2.LocalDocID == x.AppDocID));

                    foreach (var r in result)
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
                        presentDocuments = presentDocuments,
                        legacyModels = get.ToList()
                    });

                    var getType = from a in _context.Categories.AsEnumerable()
                                  join p in _context.Phases.AsEnumerable() on a.id equals p.category_id
                                  where (a.name.ToLower().Contains("new depot") || p.ShortName == "NDT") && a.DeleteStatus != true && p.DeleteStatus != true
                                  orderby a.id ascending
                                  select new CategoryType
                                  {
                                      CatTypeId = p.id,
                                      CategoryName = p.name,
                                      PhaseName = p.name,
                                      Counter = a.id,
                                  };

                    ViewBag.Products = getCategoryProduct.ToList();
                    ViewBag.CategoryTypes = getType.ToList();
                    var states = _context.States_UT.Where(x => x.Country_id == 156).ToList();
                    ViewBag.States = states;
                    _helpersController.LogMessages("Loading company information, legacy information and document for resubmitting legacy application " + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());

                    return View(bothDocuments.ToList());
                }
                else
                {
                    return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying to load legacy application. Kindly contact support.") });
                }

            }
        }




        /*
         * Resubmitting a rejected legacy application
         */
        [Authorize(Policy = "CompanyRoles")]

        public IActionResult LegacyResubmit(Legacies Legacy, string State, int LegacyID, List<LegacyDocuments> legacyDocuments, List<int> Products)
        {
            try
            {
                string result = "";

                // geting local DB state id for saving
                var statee = _context.States_UT.Where(x => x.State_id == Convert.ToInt32(State)).FirstOrDefault();

                var Issuedate = DateTime.Parse(Legacy.Issue_Date.ToString().Trim());
                var Expirydate = DateTime.Parse(Legacy.Exp_Date.ToString().Trim());

                var get = _context.Legacies.Where(x => x.Id == LegacyID && x.DeleteStatus != true);

                string products = string.Join(",", Products.ToList());

                if (statee != null)
                {
                    if (get.Count() > 0)
                    {
                        var getStaff = _context.Staff.Where(x => x.StaffID == get.FirstOrDefault().DropStaffId);


                        get.FirstOrDefault().LicenseNo = Legacy.LicenseNo;
                        get.FirstOrDefault().Issue_Date = Issuedate.ToString();
                        get.FirstOrDefault().Exp_Date = Expirydate.ToString();
                        get.FirstOrDefault().State = statee.StateName;
                        get.FirstOrDefault().FacilityName = Legacy.FacilityName;
                        get.FirstOrDefault().FacilityAddress = Legacy.FacilityAddress;
                        //get.FirstOrDefault().Products = products;
                        get.FirstOrDefault().LGA = Legacy.LGA;
                        get.FirstOrDefault().City = Legacy.City;
                        get.FirstOrDefault().LandMeters = Legacy.LandMeters;
                        get.FirstOrDefault().IsPipeline = Legacy.IsPipeline;
                        get.FirstOrDefault().IsHighTension = Legacy.IsHighTension;
                        get.FirstOrDefault().IsHighway = Legacy.IsHighway;
                        get.FirstOrDefault().ContactName = Legacy.ContactName;
                        get.FirstOrDefault().ContactPhone = Legacy.ContactPhone;
                        get.FirstOrDefault().Status = GeneralClass.Processing;
                        get.FirstOrDefault().UpdatedAt = DateTime.Now;
                        get.FirstOrDefault().AppDocId = legacyDocuments.FirstOrDefault().LocalDocID;
                        get.FirstOrDefault().CompElpsDocId = legacyDocuments.FirstOrDefault().CompElpsDocID;
                        get.FirstOrDefault().DocSource = legacyDocuments.FirstOrDefault().DocSource;

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Legacy Resubmitted";

                            string subj = "Legacy Application (" + Legacy.LicenseNo.ToUpper() + ") Resubmitted";
                            string cont = "A legacy application with permit number : " + Legacy.LicenseNo.ToUpper() + " has been resubmitted on your desk for verification.";
                            var msg = _helpersController.SendEmailMessageSBJAsync(getStaff.FirstOrDefault().StaffEmail, getStaff.FirstOrDefault().FirstName, subj, cont, GeneralClass.STAFF_NOTIFY, null);

                            string subject = "Legacy Application Resubmitted with Permit No : " + Legacy.LicenseNo.ToUpper();
                            string content = "You have resubmitted your legacy application with permit number " + Legacy.LicenseNo.ToUpper() + " from processing on NMDPRA Depot portal. Kindly find application details below.";
                            var company = _context.companies.Where(x => x.id == _helpersController.getSessionUserID());
                            var email = _helpersController.SendEmailMessageSBJAsync(_helpersController.getSessionEmail(), company.FirstOrDefault().name, subject, content, GeneralClass.COMPANY_NOTIFY, null);

                        }
                        else
                        {
                            result = "Something went wrong trying to update your legacy application.";
                        }
                    }
                    else
                    {
                        result = "Application not found, or has been deleted.";
                    }
                }
                else
                {
                    result = "Application state not found.";
                }

                _helpersController.LogMessages("Legacy application resubmit status : " + result + ". Legacy information : " + Legacy.LicenseNo, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }
        }



        public List<LegacyModel> GetApplicationLegacy(int id, string option = null)
        {
            var get = from l in _context.Legacies.AsEnumerable()
                      where l.DeleteStatus != true
                      select new LegacyModel
                      {
                          
                          Legacy = l,
                          CompanyName = l.CompName?.ToUpper(),
                          Category = l.AppType?.ToUpper(),
                          Staff = _context.Staff.Where(x => x.StaffID == l.ApprovedBy)?.FirstOrDefault(),
                          CurrentDesk = _context.Staff.Where(x => x.StaffID == l.DropStaffId)?.FirstOrDefault()
                      };

         


            if (id == 0 && option == GeneralClass.Processing)
            {
                get = get.Where(x => x.Legacy.Status == GeneralClass.Processing && x.Legacy.DropStaffId == _helpersController.getSessionUserID()).ToList(); // Listing
            }
            else if (id != 0 && option == GeneralClass.Processing)
            {
                get = get.Where(x => x.Legacy.Id == id && x.Legacy.Status == GeneralClass.Processing && x.Legacy.DropStaffId == _helpersController.getSessionUserID()).ToList(); // View with Operations
            }
            else if (id != 0 && option == "ALL")
            {
                get = get.Where(x => x.Legacy.Id == id).ToList(); // View without Operatioins
            }
            else if (id != 0 && option == "Company")
            {
                get = get.Where(x => x.Legacy.Id == id).ToList(); // View without Operatioins
            }


            return get.ToList();
        }

        
        /*
        * Getting Legacy Application for processing and Viewing All
        */
        //[Authorize(Policy = "ProcessingStaffRoles")]
        public IActionResult LegacyApplications(int id, string option)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Legacy Application link not found. Kindly contact support.") });
            }

            var get = GetApplicationLegacy(id, option);
            ViewData["LegacyTitle"] = option + " Legacy Applications";
            ViewData["LegacyOption"] = option;

            var getType = from a in _context.Categories.AsEnumerable()
                          join p in _context.Phases.AsEnumerable() on a.id equals p.category_id
                          where (a.name.ToLower().Contains("new depot")) || p.ShortName =="NDT" && a.DeleteStatus != true && p.DeleteStatus != true
                          orderby a.id ascending
                          select new CategoryType
                          {
                              CatTypeId = p.id,
                              CategoryName = p.name,
                              PhaseName = p.name,
                              Counter = a.id,
                          };
            ViewBag.CategoryTypes = getType.ToList();

            _helpersController.LogMessages("Displaying legacy application for " + ViewData["LegacyOption"], _helpersController.getSessionEmail());

            return View(get);
        }




        [AllowAnonymous]
        public JsonResult LegacyAppsCount()
        {
            var leg = _context.Legacies.Where(x => (x.Status == GeneralClass.Processing) && x.DropStaffId == _helpersController.getSessionUserID() && x.DeleteStatus != true);
            return Json(leg.Count());
        }



        /*
       * Viewing Legacy Licence with control panel and non control panel
       * 
       * id => not encrypted legacy id
       * option => not encryted option (Processing or All)
       */
        //[Authorize(Policy = "ProcessingStaffRoles")]
        public IActionResult ViewLegacyApp(int id, string option)
        {
            if (string.IsNullOrWhiteSpace(option) || id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Legacy Application link not found. Kindly contact support.") });
            }

            var get = GetApplicationLegacy(id, option);

            //var productsList = get.FirstOrDefault().Legacy.Products.Split(",");


            //foreach (var p in productsList.ToList())
            //{
            //    var prod = _context.Products.Where(x => x.Id.ToString() == p);

            //    products = products + ", " + prod.FirstOrDefault().Name;
            //}

            //ViewData["Products"] = products;

            ViewData["LegacyViewTitle"] = "Legacy Applications For : " + get.FirstOrDefault().Legacy.LicenseNo;
            ViewData["LegacyOption"] = option;

            _helpersController.LogMessages("Displaying legacy application for " + ViewData["LegacyOption"], _helpersController.getSessionEmail());

            return View(get.ToList());
        }


        /*
         * Accepting Legacy licence
         */
        //[Authorize(Policy = "ProcessingStaffRoles")]
        public IActionResult AcceptLegacy(string LegacyID, string Comment, string State)
        {
            string result = "";

            int leg_id = 0;

            var legID = generalClass.Decrypt(LegacyID);

            if (legID == "Error")
            {
                result = "Legacy Application link not found. Kindly contact support.";
            }
            else
            {

                try
                {
                    leg_id = Convert.ToInt32(legID);

                    var get = _context.Legacies.Where(x => x.Id == leg_id && x.Status == GeneralClass.Processing && x.DeleteStatus != true);

                    if (get.Count() > 0)
                    {
                        var state = _context.States_UT.Where(s => s.StateName == get.FirstOrDefault().State).FirstOrDefault();
                        int elspStateID = generalClass.GetStatesFromCountry(state.State_id.ToString());

                        //getting company elps id from loacal DB to save to elps facility
                        var company = _context.companies.Where(x => x.id == Convert.ToInt16(get.FirstOrDefault().CompId));

                        var compId = company.FirstOrDefault().id;
                        var compEmail = company.FirstOrDefault().CompanyEmail;
                        var compName = company.FirstOrDefault().name;
                        var compElpsID = company.FirstOrDefault().elps_id;

                        // already exiting facility on local DB
                        var facility_count = _context.Facilities.Where(x => x.Name.ToUpper() == get.FirstOrDefault().FacilityName.ToUpper() && x.address_1.ToUpper() == get.FirstOrDefault().FacilityAddress.ToUpper() && x.StateName == get.FirstOrDefault().State && x.CompanyId == get.FirstOrDefault().Id).Count();

                        if (facility_count > 0)
                        {
                            result = "This facility has already been added.";
                        }
                        else
                        {
                            int done = 0;

                            Facilities _facilities = new Facilities()
                            {
                                CompanyId = Convert.ToInt16(get.FirstOrDefault().CompId),
                                Name = get.FirstOrDefault().FacilityName,
                                address_1 = get.FirstOrDefault().FacilityAddress,
                                StateName = get.FirstOrDefault().State,
                                LGA = get.FirstOrDefault().LGA,
                                city = get.FirstOrDefault().City,
                                Date = DateTime.Now,
                                DeletedStatus = false,
                                ContactName = get.FirstOrDefault().ContactName,
                                ContactNumber = get.FirstOrDefault().ContactPhone
                            };

                            // saving facility to local DB
                            _context.Facilities.Add(_facilities);
                            done = _context.SaveChanges();

                            //save facility suitability information
                            var suitInfo = new SuitabilityInspections()
                            {
                                SizeOfLand = get.FirstOrDefault().LandMeters.ToString(),
                                ISAlongPipeLine = get.FirstOrDefault().IsPipeline,
                                IsOnHighWay = get.FirstOrDefault().IsHighway,
                                IsUnderHighTension = get.FirstOrDefault().IsHighTension,
                                CompanyId = company.FirstOrDefault().id,
                                FacilityId = _facilities.Id,
                                ApplicationId = 0

                            };
                            _context.SuitabilityInspections.Add(suitInfo);
                            done = _context.SaveChanges();

                            var add = new addresses
                            {
                                elps_id = 0,
                                address_1 = get.FirstOrDefault().FacilityAddress,
                                city = get.FirstOrDefault().City,
                                country_id = 156,
                                LgaId = _context.Lgas.Where(l => l.Name.Contains(get.FirstOrDefault().LGA)).FirstOrDefault().Id,
                                StateId = _context.States_UT.Where(s => s.StateName == get.FirstOrDefault().State).FirstOrDefault().State_id,
                            };
                            _context.addresses.Add(add);
                            done = _context.SaveChanges();

                            if (done > 0)
                            {
                                _facilities.AddressId = add.id;
                                _context.SaveChanges();

                                int facilityID = _facilities.Id;

                                LpgLicense.Models.Facility facility = new LpgLicense.Models.Facility()
                                {
                                    Name = get.FirstOrDefault().FacilityName,
                                    CompanyID = (int)company.FirstOrDefault().elps_id,
                                    StreetAddress = add.address_1,
                                    City = _facilities.city,
                                    FacilityType = "Depot",
                                    StateId = add.StateId,
                                    DateAdded = DateTime.Now,
                                    LGAId = (int)add.LgaId,

                                };

                                // svaing new facility to elps
                                var response2 = _restService.Response("/api/Facility/Add/{email}/{apiHash}", null, "POST", facility);
                                var res = JsonConvert.DeserializeObject<LpgLicense.Models.Facility>(response2.Content);

                                // updating new epls facility id with local DB
                                var updateFacility = _context.Facilities.Where(x => x.Id == facilityID).FirstOrDefault();
                                updateFacility.Elps_Id = res.Id;
                                _context.SaveChanges();

                                result = "Facility created but application not created. Something went wrong";

                                var getType = (from a in _context.Categories.AsEnumerable()
                                              join p in _context.Phases.AsEnumerable() on a.id equals p.category_id
                                              where p.name==get.FirstOrDefault().AppType
                                              orderby a.id ascending
                                              select new CategoryType
                                              {
                                                  CatTypeId = a.id,
                                                  PhaseTypeId=p.id,
                                                  CategoryName = p.name,
                                                  PhaseName = p.name,
                                                  Counter = a.id,
                                              }).FirstOrDefault();


                                // creating application on local db
                                NewDepot.Models.applications _applications = new NewDepot.Models.applications()
                                {
                                    company_id = Convert.ToInt16(get.FirstOrDefault().CompId),
                                    FacilityId = facilityID,
                                    type="new",
                                    year=DateTime.Now.Year,
                                    category_id=getType.CategorId,
                                    PhaseId=getType.PhaseTypeId,
                                    status = GeneralClass.Approved,
                                    CreatedAt = DateTime.Now,
                                    current_Permit = get.FirstOrDefault().LicenseNo,
                                    date_modified = DateTime.Now,
                                    date_added = DateTime.Now,
                                    reference = generalClass.Generate_Application_Number(),
                                    UpdatedAt = DateTime.Now,
                                    submitted = true,
                                    DeleteStatus = false,
                                    AppProcessed = true,
                                    SupervisorProcessed = true,
                                    isLegacy = true
                                };

                                _context.applications.Add(_applications);
                                int app_saved = _context.SaveChanges();

                                if (app_saved > 0)
                                {
                                    NewDepot.Models.permits permits = new NewDepot.Models.permits()
                                    {
                                        application_id = _applications.id,
                                        permit_no = get.FirstOrDefault().LicenseNo,
                                        Printed = false,
                                        is_renewed = "false",
                                        date_issued = Convert.ToDateTime(get.FirstOrDefault().Issue_Date),
                                        date_expire = Convert.ToDateTime(get.FirstOrDefault().Exp_Date),
                                        company_id= Convert.ToInt16(get.FirstOrDefault().CompId),
                                        //facilityID = _applications.FacilityId,
                                        CreatedAt = DateTime.Now,
                                        ApprovedBy = get.FirstOrDefault().DropStaffId
                                    };

                                    _context.permits.Add(permits);

                                    var getLeg = _context.Legacies.Where(x => x.Id == leg_id);

                                    var permitNo = getLeg.FirstOrDefault().LicenseNo;

                                    getLeg.FirstOrDefault().Status = GeneralClass.Approved;
                                    getLeg.FirstOrDefault().IsUsed = false;
                                    getLeg.FirstOrDefault().UpdatedAt = DateTime.Now;
                                    getLeg.FirstOrDefault().ApprovedAt = DateTime.Now;
                                    getLeg.FirstOrDefault().Comment = Comment;
                                    getLeg.FirstOrDefault().ApprovedBy = _helpersController.getSessionUserID();

                                    int permitDone = _context.SaveChanges();

                                    if (permitDone > 0)
                                    {
                                        result = "Legacy Approved";
                                        string subject = "Legacy Application Approved with Permit No : " + permitNo.ToUpper();
                                        string content = "Your legacy application with permit number " + permitNo.ToUpper() + " has been approved on NMDPRA Depot portal. Kindly find application details below.";
                                        var email = _helpersController.SendEmailMessageSBJAsync(compEmail, compEmail, subject, content, GeneralClass.COMPANY_NOTIFY);

                                    }
                                    else
                                    {
                                        result = "Something went wrong trying to add legacy licence number.";
                                    }
                                }
                                else
                                {
                                    result = "Something went wrong trying to create your application, please try again.";
                                }
                            }
                            else
                            {
                                result = "Something went wrong trying to save this new facility from legacy application.";
                            }
                        }

                    }
                    else
                    {
                        result = "Application not found, or deleted, or already approved";
                    }

                    _helpersController.LogMessages("Legacy application status : " + result + ". Legacy information ID : " + leg_id, _helpersController.getSessionEmail());

                    return Json(result);
                }
                catch (Exception e)
                {
                    result = e.Message;
                    return Json(result);
                }
            }
            return Json(result);

        }


        /*
         * Rejecting Legacy licence
         */
        //[Authorize(Policy = "ProcessingStaffRoles")]
        public JsonResult RejectLegacy(string LegacyID, string Comment)
        {
            try
            {
                string result = "";

                int leg_id = 0;

                var legID = generalClass.Decrypt(LegacyID);

                if (legID == "Error")
                {
                    result = "Legacy application not found. Kindly contact support.";
                }
                else
                {
                    leg_id = Convert.ToInt32(legID);

                    var get = _context.Legacies.Where(x => x.Id == leg_id && x.DeleteStatus != true && x.Status == GeneralClass.Processing);

                    if (get.Count() > 0)
                    {
                        var permitNo = get.FirstOrDefault().LicenseNo;

                        var compid = Convert.ToInt16( get.FirstOrDefault().CompId );
                        get.FirstOrDefault().Status = GeneralClass.Rejected;
                        get.FirstOrDefault().Comment = Comment;
                        get.FirstOrDefault().UpdatedAt = DateTime.Now;

                        if (_context.SaveChanges() > 0)
                        {
                            result = "Legacy Rejected";

                            string subject = "Legacy Application Rejected with Permit No : " + permitNo;
                            string content = "Your legacy application with permit number " + permitNo + " has been rejected with comment (" + Comment + ").";

                            var company = _context.companies.Where(x => x.id == compid);

                            var email = _helpersController.SendEmailMessageSBJAsync(company.FirstOrDefault().CompanyEmail, company.FirstOrDefault().name, subject, content, GeneralClass.COMPANY_NOTIFY);

                        }
                        else
                        {
                            result = "Something went wrong trying to reject your legacy application.";
                        }
                    }
                    else
                    {
                        result = "Application not found, or deleted, or already approved";
                    }
                }

                _helpersController.LogMessages("Legacy application status : " + result + ". Legacy information ID : " + leg_id, _helpersController.getSessionEmail());

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Error : Exception - " + ex.Message + ". Please conntact support");
            }
        }




        /*
         * Viewing Legacy Licence with control panel and non control panel
         * 
         * id => not encrypted legacy id
         * option => not encryted option (Processing or All)
         */

        public IActionResult ViewLegacy(int id, string option)
        {
            if (string.IsNullOrWhiteSpace(option) || id == 0)
            {
                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Legacy Application link not found. Kindly contact support.") });
            }

            var get = GetApplicationLegacy(id, option);

            var productsList = get.FirstOrDefault().Legacy.Products.Split(",");

            var products = "";

            //foreach (var p in productsList.ToList())
            //{
            //    var prod = _context.Products.Where(x => x.Id.ToString() == p);

            //    products = products + ", " + prod.FirstOrDefault().Name;
            //}


            ViewData["LegacyViewTitle"] = "Legacy Applications For : " + get.FirstOrDefault().Legacy.LicenseNo;
            ViewData["LegacyOption"] = option;

            _helpersController.LogMessages("Displaying legacy application for " + ViewData["LegacyOption"], _helpersController.getSessionEmail());

            return View(get);
        }


    }
}

//public IActionResult Apply()
//{
//    var user_email = _helpersController.getSessionUserID();


//    var getType = from a in _context.Categories.AsEnumerable()
//                  join p in _context.Phases.AsEnumerable() on a.id equals p.category_id
//                  where (a.name.ToLower().Contains("new depot") || p.ShortName == "NDT") && a.DeleteStatus != true && p.DeleteStatus != true
//                  orderby a.id ascending
//                  select new CategoryType
//                  {
//                      CatTypeId = p.id,
//                      CategoryName = p.name,
//                      PhaseName = p.name,
//                      Counter = a.id,
//                  };

//    var getProduct = from cp in _context.Products
//                     where cp.DeletedStatus != true
//                     select new Products
//                     {
//                         Name = cp.Name,
//                         Id = cp.Id
//                     };

//    if (getType.Count() > 0)
//    {
//        var companyID = _helpersController.getSessionUserID();

//        var getCompany = _context.companies.Where(x => x.id == companyID && x.DeleteStatus != true);
//        var getDocuments = _context.ApplicationDocuments.AsEnumerable().Where(x => x.docType == "Company" && x.DocName.Contains("LEGACY") && x.DeleteStatus != true);

//        List<PresentDocuments> presentDocuments = new List<PresentDocuments>();
//        List<MissingDocument> missingDocuments = new List<MissingDocument>();
//        List<BothDocuments> bothDocuments = new List<BothDocuments>();

//        if (getCompany.Count() > 0 && getDocuments.Count() > 0)
//        {
//            ViewData["CompanyElpsID"] = getCompany.FirstOrDefault().elps_id;

//            List<LpgLicense.Models.Document> companyDoc = generalClass.getCompanyDocuments(getCompany.FirstOrDefault().elps_id.ToString());
//            if (companyDoc == null)
//            {
//                return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("A network related problem. Please connect to a network.") });
//            }


//            foreach (var cDoc in companyDoc)
//            {
//                if (cDoc.document_type_id == getDocuments.FirstOrDefault().ElpsDocTypeID.ToString())
//                {
//                    //now check if facility is existing for any facility legacy before now
//                    var checkLeg = _context.Legacies.Where(x => x.CompElpsDocId == cDoc.id && x.DeleteStatus != true).FirstOrDefault();

//                    if (checkLeg == null)
//                    {

//                        presentDocuments.Add(new PresentDocuments
//                        {
//                            Present = true,
//                            FileName = cDoc.fileName,
//                            Source = cDoc.source,
//                            CompElpsDocID = cDoc.id,
//                            DocTypeID = Convert.ToInt32(cDoc.document_type_id),
//                            LocalDocID = getDocuments.FirstOrDefault().AppDocID,
//                            DocType = getDocuments.FirstOrDefault().docType,
//                            TypeName = cDoc.documentTypeName
//                        });
//                    }
//                }
//            }

//            var result = getDocuments.AsEnumerable().Where(x => !presentDocuments.Any(x2 => x2.LocalDocID == x.AppDocID));

//            foreach (var r in result)
//            {
//                missingDocuments.Add(new MissingDocument
//                {
//                    Present = false,
//                    DocTypeID = r.ElpsDocTypeID,
//                    LocalDocID = r.AppDocID,
//                    DocType = r.docType,
//                    TypeName = r.DocName
//                });
//            }

//            bothDocuments.Add(new BothDocuments
//            {
//                missingDocuments = missingDocuments,
//                presentDocuments = presentDocuments,
//            });

//            ViewData["ApplyDescription"] = getType.FirstOrDefault().CategoryName;
//            ViewBag.Products = getProduct.ToList();
//            ViewBag.CategoryTypes = getType.ToList();

//            var states = _context.States_UT.Where(x => x.Country_id == 156).ToList();
//            ViewBag.States = states;

//            _helpersController.LogMessages("Loading company information and document for legacy application" + _helpersController.getSessionEmail(), _helpersController.getSessionEmail());

//            return View(bothDocuments.ToList());
//        }
//        else
//        {
//            return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong trying to load legacy application. Kindly contact support.") });
//        }

//    }
//    else
//    {
//        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Something went wrong, no application stages was found for the selected application") });
//    }
//}
