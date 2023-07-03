using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewDepot.Models;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Helpers;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;

namespace NewDepot.Controllers
{
    //
    //
    //]
    public class ZoneStatesController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public ZoneStatesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        // GET: ZoneStates
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.ZoneStates.ToListAsync());
        }

        public JsonResult GetZoneStates()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getZoneState = from zs in _context.ZoneStates
                               join s in _context.States_UT on zs.State_id equals s.State_id
                               join z in _context.ZonalOffice on zs.Zone_id equals z.Zone_id
                               join c in _context.countries on s.Country_id equals c.id
                               where zs.DeleteStatus  != true && s.DeleteStatus  != true && z.DeleteStatus  != true                               select new
                               {
                                   ZoneStateID = zs.ZoneStates_id,
                                   CountryName = c.name,
                                   StateId = zs.State_id,
                                   Zone_id = zs.Zone_id,
                                   StateName = s.StateName,
                                   ZoneName = z.ZoneName,
                                   UpdatedAt = zs.UpdatedAt.ToString(),
                                   CreatedAt = zs.CreatedAt.ToString()
                               };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getZoneState = sortColumn == "countryName" ? getZoneState.OrderByDescending(s => s.CountryName) :
                                    sortColumn == "stateName" ? getZoneState.OrderByDescending(s => s.StateName) :
                                    sortColumn == "zoneName" ? getZoneState.OrderByDescending(s => s.ZoneName) :
                                    sortColumn == "updatedAt" ? getZoneState.OrderByDescending(s => s.UpdatedAt) :
                                    sortColumn == "createdAt" ? getZoneState.OrderByDescending(s => s.CreatedAt) :
                                    getZoneState.OrderByDescending(s => s.ZoneStateID + " " + sortColumnDir);
                }
                else
                {
                    getZoneState = sortColumn == "countryName" ? getZoneState.OrderBy(c => c.CountryName) :
                                   sortColumn == "stateName" ? getZoneState.OrderBy(s => s.StateName) :
                                   sortColumn == "zoneName" ? getZoneState.OrderBy(s => s.ZoneName) :
                                   sortColumn == "updatedAt" ? getZoneState.OrderBy(c => c.UpdatedAt) :
                                   sortColumn == "createdAt" ? getZoneState.OrderBy(c => c.CreatedAt) :
                                   getZoneState.OrderBy(c => c.ZoneStateID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getZoneState = getZoneState.Where(c => c.CountryName.Contains(txtSearch.ToUpper()) || c.StateName.Contains(txtSearch.ToUpper()) || c.ZoneName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getZoneState.Count();
            var data = getZoneState.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying zones states", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        // POST: ZoneStates/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateZoneState(int ZoneID, int StateID)
        {
            string response = "";

            var check = from zs in _context.ZoneStates
                        join s in _context.States_UT on zs.State_id equals s.State_id
                        join z in _context.ZonalOffice on zs.Zone_id equals z.Zone_id
                        where zs.Zone_id == ZoneID && zs.State_id == StateID && zs.DeleteStatus  != true
                        select new
                        {
                            s.StateName,
                            z.ZoneName
                        };

            if (check.Count() > 0)
            {
                response = check.FirstOrDefault().StateName + " and " + check.FirstOrDefault().ZoneName + " relationship already exits.";
            }
            else
            {
                ZoneStates _zoneStates = new ZoneStates()
                {
                    Zone_id = ZoneID,
                    State_id = StateID,
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.ZoneStates.Add(_zoneStates);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "ZoneState Created";
                }
                else
                {
                    response = "Something went wrong trying to create Zone and State relationship. Please try again.";
                }
            }

            helpers.LogMessages("Creating zones states. Status : " + response + " Zonal ID : " + ZoneID + " State ID : " + StateID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }




        // POST: ZoneStates/Edit/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditZoneState(int ZoneStateID, int ZoneID, int StateID)
        {
            string response = "";
            var getZoneState = from x in _context.ZoneStates where x.ZoneStates_id == ZoneStateID select x;

            if (getZoneState.FirstOrDefault().State_id == StateID && getZoneState.FirstOrDefault().Zone_id == ZoneID)
            {
                response = "This relationship already exits. Try a different one.";
            }
            else
            {
                getZoneState.FirstOrDefault().State_id = StateID;
                getZoneState.FirstOrDefault().Zone_id = ZoneID;
                getZoneState.FirstOrDefault().UpdatedAt = DateTime.Now;
                getZoneState.FirstOrDefault().DeleteStatus = false;

                int updated = await _context.SaveChangesAsync();

                if (updated > 0)
                {
                    response = "ZoneState Updated";
                }
                else
                {
                    response = "Nothing was updated.";
                }
            }

            helpers.LogMessages("Updating Zone States. Status : " + response + " Zonal ID : " + ZoneID + " State ID : " + StateID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }




        // POST: ZoneStates/Delete/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteZoneState(int ZoneStateID)
        {
            string response = "";

            var get = from c in _context.ZoneStates where c.ZoneStates_id == ZoneStateID select c;

            get.FirstOrDefault().DeletedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().DeleteStatus = true;
            get.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "ZoneState Deleted";
            }
            else
            {
                response = "Zone => State not deleted. Something went wrong trying to delete this entry.";
            }

            helpers.LogMessages("Deleting Zone States. Status : " + response + " ZonalStateID : " + ZoneStateID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        private bool ZoneStatesExists(int id)
        {
            return _context.ZoneStates.Any(e => e.ZoneStates_id == id);
        }
    }
}
