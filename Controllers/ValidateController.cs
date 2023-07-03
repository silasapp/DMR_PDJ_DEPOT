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

namespace NMDPRA_Depot.WebUI.Controllers
{
    //[Authorize(Roles ="Admin")]
    public class ValidateController : Controller
    {

        RestSharpServices _restService = new RestSharpServices();
        public IConfiguration _configuration;
        IHttpContextAccessor _httpContextAccessor;
        private readonly Depot_DBContext _context;
        ElpsResponse elpsResponse = new ElpsResponse();
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;

        public ValidateController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }
            //
            // GET: /Validate/
            public IActionResult Index()
        {
            if (TempData["message"] != null && TempData["msgType"] != null)
            {
                ViewBag.Msg = TempData["message"].ToString();
                ViewBag.MsgType = TempData["msgType"].ToString();
                TempData.Clear();
            }

            return View();
        }

        [HttpPost]
        public IActionResult SearchResult(string permitNo)
        {
            if (!string.IsNullOrEmpty(permitNo))
            {
                var permit = _context.permits.Where(P => P.permit_no.ToLower() == permitNo.ToLower()).FirstOrDefault();

                if (permit != null)
                {
                    if(permit.date_expire < DateTime.Now)
                    {
                        ViewBag.Msg = "The Requested Permit Has Expired";
                        ViewBag.MsgType = "warn";
                    }
                    else
                    {
                        ViewBag.Msg = "The Requested Permit Is Valid";
                        ViewBag.MsgType = "pass";
                    }
                    var app = (from p in _context.permits.AsEnumerable()
                               join a in _context.applications.AsEnumerable() on p.application_id equals a.id
                               join f in _context.Facilities.AsEnumerable() on a.FacilityId equals f.Id
                               join c in _context.companies.AsEnumerable() on a.company_id equals c.id
                               join ad in _context.addresses.AsEnumerable() on f.AddressId equals ad.id
                               join sd in _context.States_UT.AsEnumerable() on ad.StateId equals sd.State_id
                               join ca in _context.Categories.AsEnumerable() on a.category_id equals ca.id
                               join at in _context.Phases.AsEnumerable() on a.PhaseId equals at.id
                               where p.permit_no.ToLower() == permitNo.ToLower()
                               select new MyApps
                               {
                                   ModifyType = _context.FacilityModifications.Where(b => b.ApplicationId == a.id).FirstOrDefault() != null ? _context.FacilityModifications.Where(b => b.ApplicationId == a.id).FirstOrDefault().Type : "",
                                   app = a,
                                   AppPermit=p,
                                   fac=f,
                                   CompanyName = c.name,
                                   PhaseName = at.name,
                                   CompanyDetails = c.CompanyEmail,
                                   LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                                   Address_1=ad.address_1,
                                   FacilityId = f.Id,
                                   Company_Id = c.id,
                                   FacilityName=f.Name,
                                   FacilityDetails = f.Name + " (" + ad.address_1 + ")",
                                   CategoryName = at.name.ToUpper(),
                                   category_id=at.id,
                                    StateName=sd.StateName
                               });
                   
                   return View(app.FirstOrDefault());
                }
                else
                {

                    var lg_old = (from l in _context.Legacies.AsEnumerable()
                               join  c in _context.companies.AsEnumerable() on l.CompId equals c?.DPR_Id?.ToString()
                               where l.LicenseNo.ToLower() == permitNo.ToLower()
                               select new MyApps
                               {
                                   legaciess = l,
                                   CompanyName = c.name,
                                   CompanyDetails = c.CompanyEmail,
                                   LGA=l?.LGA,
                                   Address_1 = l?.FacilityAddress,
                                   Company_Id = c.id,
                                   FacilityDetails = l?.FacilityName,
                                   StateName = l?.State
                               });

                    var lg_new = (from l in _context.Legacies.AsEnumerable()
                               join  c in _context.companies.AsEnumerable() on l.CompId equals c.id.ToString()
                               where l.LicenseNo.ToLower() == permitNo.ToLower()
                               select new MyApps
                               {
                                   legaciess = l,
                                   fac = _context.Facilities.Where(x=> x.Name.ToLower() == l.FacilityName.ToLower() )?.FirstOrDefault(),
                                   CompanyName = c.name,
                                   CompanyDetails = c.CompanyEmail,
                                   LGA=l?.LGA,
                                   Address_1 = l?.FacilityAddress,
                                   Company_Id = c.id,
                                   FacilityDetails = l?.FacilityName,
                                   StateName = l?.State
                               });
                  
                    if (lg_old.Count() > 0)
                    {

                        ViewBag.legacy = lg_old;

                        TempData["message"] = "Requested Permit is a Valid Legacy Permit";
                        TempData["msgType"] = "pass";
                        return View(lg_old.FirstOrDefault());
                    }
                    if (lg_new.Count() > 0)
                    {

                        ViewBag.legacy = lg_new;

                        TempData["message"] = "Requested Permit is a Valid Legacy Permit";
                        TempData["msgType"] = "pass";
                        return View(lg_new.FirstOrDefault());
                    }
                    TempData["message"] = "Permit with the specified Permit Number not found. Enter another number and try again";
                    TempData["msgType"] = "fail";
                }
            }
            else
            {
                TempData["message"] = "Permit Number cannot be empty. Please enter the correct number and try again";
                TempData["msgType"] = "fail";
            }

            return RedirectToAction("Index");
        }

       
    }
}