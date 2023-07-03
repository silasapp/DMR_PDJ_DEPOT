using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using NewDepot.Helpers;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Models;

namespace NewDepot.Controllers.Configurations
{

    ////[Authorize(Policy = "ConfigurationRoles")]
    public class LocationsController : Controller
    {
        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();

        public LocationsController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



        // GET: Locations
       
        public IActionResult Index()
        {
            return View();
        }




        // Get all locations
        
        public JsonResult GetLocations()
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

            var getLocations = from c in _context.Location
                               where c.DeleteStatus == false

                               select new
                               {
                                   LocationId = c.LocationID,
                                   LocationName = c.LocationName,
                                   UpdatedAt = c.UpdatedAt.ToString(),
                                   CreatedAt = c.CreatedAt.ToString(),
                               };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getLocations = sortColumn == "locationName" ? getLocations.OrderByDescending(c => c.LocationName) :
                               sortColumn == "updatedAt" ? getLocations.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getLocations.OrderByDescending(c => c.CreatedAt) :
                               getLocations.OrderByDescending(c => c.LocationId + " " + sortColumnDir);
                }
                else
                {
                    getLocations = sortColumn == "locationName" ? getLocations.OrderBy(c => c.LocationName) :
                               sortColumn == "updatedAt" ? getLocations.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getLocations.OrderBy(c => c.CreatedAt) :
                               getLocations.OrderBy(c => c.LocationId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getLocations = getLocations.Where(c => c.LocationName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getLocations.Count();
            var data = getLocations.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all locations", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        //create Locatioin
      
        public async Task<IActionResult> CreateLocation(string Location)
        {
            string response = "";

            var country = from c in _context.Location
                          where c.LocationName == Location.ToUpper() && c.DeleteStatus == false
                          select c;

            if (country.Count() > 0)
            {
                response = "Location already exits, please enter another location.";
            }
            else
            {
                Location loc = new Location()
                {
                    LocationName = Location.ToUpper(),
                    CreatedAt = DateTime.Now,
                    CreatedBy =  _helpersController.getSessionUserID(),
                    DeleteStatus = false
                };

                _context.Location.Add(loc);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "Location Created";
                }
                else
                {
                    response = "Something went wrong trying to create location. Please try again.";
                }
            }

            _helpersController.LogMessages("Creating new location. Status : " + response + " Location name : " + Location, _helpersController.getSessionEmail());

            return Json(response);
        }



        
        public async Task<IActionResult> EditLocation(int LocationId, string Location)
        {
            string response = "";
            var get = from c in _context.Location where c.LocationID == LocationId select c;

            get.FirstOrDefault().LocationName = Location.ToUpper();
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedBy =  _helpersController.getSessionUserID();
            get.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Location Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            _helpersController.LogMessages("Updating Location. Status : " + response + " Location ID : " + LocationId, _helpersController.getSessionEmail());

            return Json(response);
        }

        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> RemoveLocation(int LocationID)
        {
            string response = "";

            var get = from c in _context.Location where c.LocationID == LocationID select c;

            get.FirstOrDefault().DeletedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedBy =  _helpersController.getSessionUserID();
            get.FirstOrDefault().DeleteStatus = true;
            get.FirstOrDefault().DeletedBy =  _helpersController.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Location Removed";
            }
            else
            {
                response = "Location not deleted. Something went wrong trying to delete this Location.";
            }

            _helpersController.LogMessages("Deleting Location. Status : " + response + " Location ID : " + LocationID, _helpersController.getSessionEmail());

            return Json(response);

        }



    }
}
