using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using NewDepot.Helpers;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Models;

namespace NewDepot.Controllers.Configurations
{

    ////[Authorize(Policy = "ConfigurationRoles")]
    public class ZoneFieldOfficesController : Controller
    {
        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();

        public ZoneFieldOfficesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        // GET: ZonalFieldOffices
        
        public async Task<IActionResult> Index()
        {
            return View(await _context.ZonalFieldOffice.ToListAsync());
        }



        
        public JsonResult GetZoneFieldOffice()
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

            var getZoneState = (from zf in _context.ZonalFieldOffice
                                join f in _context.FieldOffices on zf.FieldOffice_id equals f.FieldOffice_id
                                join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                                join zs in _context.ZoneStates on z.Zone_id equals zs.Zone_id
                                join s in _context.States_UT on zs.State_id equals s.State_id
                                join c in _context.countries on s.Country_id equals c.id
                                where (zf.DeleteStatus  != true && f.DeleteStatus  != true && z.DeleteStatus  != true && s.DeleteStatus  != true
                                )select new
                                {
                                    ZonalFieldOfficeID = zf.ZoneFieldOffice_id,
                                    CountryID = c.id,
                                    StateId = s.State_id,
                                    ZoneId = zf.Zone_id,
                                    FieldOfficeID = f.FieldOffice_id,
                                    CountryName = c.name,
                                    StateName = s.StateName,
                                    ZoneName = z.ZoneName,
                                    OfficeName = f.OfficeName,
                                    UpdatedAt = zf.UpdatedAt.ToString(),
                                    CreatedAt = zf.CreatedAt.ToString()
                                });


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getZoneState = sortColumn == "countryName" ? getZoneState.OrderByDescending(s => s.CountryName) :
                                    sortColumn == "stateName" ? getZoneState.OrderByDescending(s => s.StateName) :
                                    sortColumn == "zoneName" ? getZoneState.OrderByDescending(s => s.ZoneName) :
                                    sortColumn == "officeName" ? getZoneState.OrderByDescending(s => s.OfficeName) :
                                    sortColumn == "updatedAt" ? getZoneState.OrderByDescending(s => s.UpdatedAt) :
                                    sortColumn == "createdAt" ? getZoneState.OrderByDescending(s => s.CreatedAt) :
                                    getZoneState.OrderByDescending(s => s.ZonalFieldOfficeID + " " + sortColumnDir);
                }
                else
                {
                    getZoneState = sortColumn == "countryName" ? getZoneState.OrderBy(c => c.CountryName) :
                                   sortColumn == "stateName" ? getZoneState.OrderBy(s => s.StateName) :
                                   sortColumn == "officeName" ? getZoneState.OrderBy(s => s.OfficeName) :
                                   sortColumn == "zoneName" ? getZoneState.OrderBy(s => s.ZoneName) :
                                   sortColumn == "updatedAt" ? getZoneState.OrderBy(c => c.UpdatedAt) :
                                   sortColumn == "createdAt" ? getZoneState.OrderBy(c => c.CreatedAt) :
                                   getZoneState.OrderBy(c => c.ZonalFieldOfficeID);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getZoneState = getZoneState.Where(c => c.CountryName.Contains(txtSearch.ToUpper()) || c.StateName.Contains(txtSearch.ToUpper()) || c.ZoneName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch) || c.OfficeName.Contains(txtSearch.ToUpper()));
            }

            totalRecords = getZoneState.Count();
            var data = getZoneState.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all zonal field office.", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        // POST: ZonalFieldOffices/Create
        
        public async Task<IActionResult> CreateZonalFieldOffice(int ZoneID, int FieldOfficeID)
        {
            string response = "";

            var check = from zs in _context.ZonalFieldOffice
                        join f in _context.FieldOffices on zs.FieldOffice_id equals f.FieldOffice_id
                        join z in _context.ZonalOffice on zs.Zone_id equals z.Zone_id
                        where zs.Zone_id == ZoneID && zs.FieldOffice_id == FieldOfficeID && zs.DeleteStatus  != true
                        select new
                        {
                            f.OfficeName,
                            z.ZoneName
                        };

            if (check.Count() > 0)
            {
                response = check.FirstOrDefault().OfficeName + " and " + check.FirstOrDefault().ZoneName + " relationship already exits.";
            }
            else
            {
                ZonalFieldOffice _ZonalFieldOffice = new ZonalFieldOffice()
                {
                    Zone_id = ZoneID,
                    FieldOffice_id = FieldOfficeID,
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.ZonalFieldOffice.Add(_ZonalFieldOffice);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "ZonalFieldOffice Created";
                }
                else
                {
                    response = "Something went wrong trying to create Zone and Field Office relationship. Please try again.";
                }
            }
            _helpersController.LogMessages("Creating new zonal field office. Status : " + response + " zonal office ID : " + ZoneID + " field office ID : " + FieldOfficeID, _helpersController.getSessionEmail());

            return Json(response);
        }





        // POST: ZonalFieldOffices/Edit/5
        
        public async Task<IActionResult> EditZonalFieldOffice(int ZoneID, int FieldOfficeID, int ZonalFieldOfficeID)
        {
            string response = "";
            var getZonalFieldOffice = from x in _context.ZonalFieldOffice where x.ZoneFieldOffice_id == ZonalFieldOfficeID select x;

            if (getZonalFieldOffice.FirstOrDefault().FieldOffice_id == FieldOfficeID && getZonalFieldOffice.FirstOrDefault().Zone_id == ZoneID)
            {
                response = "This relationship already exits. Try a different one.";
            }
            else
            {
                getZonalFieldOffice.FirstOrDefault().FieldOffice_id = FieldOfficeID;
                getZonalFieldOffice.FirstOrDefault().Zone_id = ZoneID;
                getZonalFieldOffice.FirstOrDefault().UpdatedAt = DateTime.Now;
                getZonalFieldOffice.FirstOrDefault().DeleteStatus = false;

                int updated = await _context.SaveChangesAsync();

                if (updated > 0)
                {
                    response = "ZonalFieldOffice Updated";
                }
                else
                {
                    response = "Nothing was updated.";
                }
            }

            _helpersController.LogMessages("Updating zonal field office. Status : " + response + " Zonal Office ID : " + ZoneID + " field office ID : " + FieldOfficeID, _helpersController.getSessionEmail());

            return Json(response);
        }



        // POST: ZonalFieldOffices/Delete/5
        
        public async Task<IActionResult> DeleteZonalFieldOffice(int ZonalFieldOfficeID)
        {
            string response = "";

            var getZonalFieldOffice = from c in _context.ZonalFieldOffice where c.FieldOffice_id == ZonalFieldOfficeID select c;

            getZonalFieldOffice.FirstOrDefault().DeletedAt = DateTime.Now;
            getZonalFieldOffice.FirstOrDefault().UpdatedAt = DateTime.Now;
            getZonalFieldOffice.FirstOrDefault().DeleteStatus = true;
            getZonalFieldOffice.FirstOrDefault().DeletedBy =  _helpersController.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "ZonalFieldOffice Deleted";
            }
            else
            {
                response = "Zone => Field Office not deleted. Something went wrong trying to delete this entry.";
            }

            _helpersController.LogMessages("Deleting zonal field office. Status : " + response + " Zonal field Office ID : " + ZonalFieldOfficeID, _helpersController.getSessionEmail());

            return Json(response);
        }

    }

   
}
