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

    public class FieldOfficeStatesController : Controller
    {
        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();


        public FieldOfficeStatesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        // GET: ZoneStates
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.FieldOffices.ToListAsync());
        }



        // field office to state
        ////[Authorize(Policy = "ConfigurationRoles")]
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
            var getZoneState = (from zf in _context.ZonalFieldOffice
                                join f in _context.FieldOffices on zf.FieldOffice_id equals f.FieldOffice_id
                                join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                                join zs in _context.ZoneStates on z.Zone_id equals zs.Zone_id
                                join s in _context.States_UT on zs.State_id equals s.State_id
                                join c in _context.countries on s.Country_id equals c.id
                                where (zf.DeleteStatus == false && f.DeleteStatus == false && z.DeleteStatus == false && s.DeleteStatus == false
                                )
                                select new
                                {
                                   OfficeStateID = zs.State_id,
                                   CountryName = c.name,
                                   StateId = zs.State_id,
                                   OfficeId = zf.FieldOffice_id,
                                   StateName = s.StateName,
                                   OfficeName = f.OfficeName,
                                   UpdatedAt = f.UpdatedAt.ToString(),
                                   CreatedAt = f.CreatedAt.ToString()
                               });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getZoneState = sortColumn == "countryName" ? getZoneState.OrderByDescending(s => s.CountryName) :
                                    sortColumn == "stateName" ? getZoneState.OrderByDescending(s => s.StateName) :
                                    sortColumn == "officeName" ? getZoneState.OrderByDescending(s => s.OfficeName) :
                                    sortColumn == "updatedAt" ? getZoneState.OrderByDescending(s => s.UpdatedAt) :
                                    sortColumn == "createdAt" ? getZoneState.OrderByDescending(s => s.CreatedAt) :
                                    getZoneState.OrderByDescending(s => s.OfficeStateID + " " + sortColumnDir);
                }
                else
                {
                    getZoneState = sortColumn == "countryName" ? getZoneState.OrderBy(c => c.CountryName) :
                                   sortColumn == "stateName" ? getZoneState.OrderBy(s => s.StateName) :
                                   sortColumn == "officeName" ? getZoneState.OrderBy(s => s.OfficeName) :
                                   sortColumn == "updatedAt" ? getZoneState.OrderBy(c => c.UpdatedAt) :
                                   sortColumn == "createdAt" ? getZoneState.OrderBy(c => c.CreatedAt) :
                                   getZoneState.OrderBy(c => c.OfficeStateID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getZoneState = getZoneState.Where(c => c.CountryName.Contains(txtSearch.ToUpper()) || c.StateName.Contains(txtSearch.ToUpper()) || c.OfficeName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getZoneState.Count();
            var data = getZoneState.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying office states", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        // POST: ZoneStates/Create
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> CreateZoneState(int FieldOfficeID, int StateID)
        {
            string response = "";
            var check =  (from zf in _context.ZonalFieldOffice
                                            join f in _context.FieldOffices on zf.FieldOffice_id equals f.FieldOffice_id
                                            join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                                            join zs in _context.ZoneStates on z.Zone_id equals zs.Zone_id
                                            join s in _context.States_UT on zs.State_id equals s.State_id
                                            join c in _context.countries on s.Country_id equals c.id
                                              select new
                                            {
                                                s.StateName,
                                                f.OfficeName
                                            });

            if (check.Count() > 0)
            {
                response = check.FirstOrDefault().StateName + " and " + check.FirstOrDefault().OfficeName + " relationship already exits.";
            }
            else
            {
                ZoneStates _zoneStates = new ZoneStates()
                {
                    State_id = StateID,
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.ZoneStates.Add(_zoneStates);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "Created";
                }
                else
                {
                    response = "Something went wrong trying to create fiel office and State relationship. Please try again.";
                }
            }

            _helpersController.LogMessages("Creating field office states. Status : " + response + " Field Office ID : " + FieldOfficeID + " State ID : " + StateID, _helpersController.getSessionEmail());

            return Json(response);
        }




        // POST: ZoneStates/Edit/5
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> EditZoneState(int FieldOfficeStateID, int FieldOfficeID, int StateID)
        {
            string response = "";
            var getZoneState = from x in _context.ZoneStates where x.State_id == StateID select x;

            if (getZoneState.FirstOrDefault().State_id == StateID /*&& getZoneState.FirstOrDefault().FieldOfficeId == FieldOfficeID*/)
            {
                response = "This relationship already exits. Try a different one.";
            }
            else
            {
                getZoneState.FirstOrDefault().State_id = StateID;
               // getZoneState.FirstOrDefault().FieldOfficeId = FieldOfficeID;
                //getZoneState.FirstOrDefault().UpdatedAt = DateTime.Now;
                getZoneState.FirstOrDefault().DeleteStatus = false;

                int updated = await _context.SaveChangesAsync();

                if (updated > 0)
                {
                    response = "Updated";
                }
                else
                {
                    response = "Nothing was updated.";
                }
            }

            _helpersController.LogMessages("Updating field office States. Status : " + response + " field office ID : " + FieldOfficeID + " State ID : " + StateID, _helpersController.getSessionEmail());

            return Json(response);
        }




        // POST: ZoneStates/Delete/5
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> DeleteZoneState(int FieldOfficeStateID)
        {
            string response = "";

            //var get = from c in _context.zoneState where c.FieldOfficeStatesId == FieldOfficeStateID select c;

            //get.FirstOrDefault().DeletedAt = DateTime.Now;
            //get.FirstOrDefault().UpdatedAt = DateTime.Now;
            //get.FirstOrDefault().DeleteStatus = true;
            //get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Deleted";
            }
            else
            {
                response = "Field Office => State not deleted. Something went wrong trying to delete this entry.";
            }

            _helpersController.LogMessages("Deleting field office States. Status : " + response + " FieldOfficeStateID : " + FieldOfficeStateID, _helpersController.getSessionEmail());

            return Json(response);
        }


    }
}
