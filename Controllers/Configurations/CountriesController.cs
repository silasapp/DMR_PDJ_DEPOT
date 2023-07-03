using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewDepot.Models;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Helpers;
using Microsoft.AspNetCore.Http;
using NewDepot.HelpersClass;
using Microsoft.Extensions.Configuration;

namespace NewDepot.Controllers
{
    [Authorize]
    public class CountriesController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public CountriesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        // GET: Countries
        [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult Index()
        {
            return View();
        }


        public JsonResult GetCountries()
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

            var getCountries = from c in _context.Countries
                               where c.DeleteStatus == false
                               select new
                               {
                                   CountryId = c.CountryId,
                                   CountryName = c.CountryName,
                                   UpdatedAt = c.UpdatedAt.ToString(),
                                   CreatedAt = c.CreatedAt.ToString()
                               };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getCountries = sortColumn == "countryName" ? getCountries.OrderByDescending(c => c.CountryName) :
                               sortColumn == "updatedAt" ? getCountries.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getCountries.OrderByDescending(c => c.CreatedAt) :
                               getCountries.OrderByDescending(c => c.CountryId + " " + sortColumnDir);
                }
                else
                {
                    getCountries = sortColumn == "countryName" ? getCountries.OrderBy(c => c.CountryName) :
                               sortColumn == "updatedAt" ? getCountries.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getCountries.OrderBy(c => c.CreatedAt) :
                               getCountries.OrderBy(c => c.CountryId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getCountries = getCountries.Where(c => c.CountryName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getCountries.Count();
            var data = getCountries.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessage("Displaying all countries...", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }





        // POST: Countries/Create
        [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateCountry(string Country)
        {
            string response = "";

            var country = from c in _context.Countries
                          where c.CountryName == Country.ToUpper() && c.DeleteStatus == false
                          select c;

            if (country.Count() > 0)
            {
                response = "Country already exits, please enter another country.";
            }
            else
            {
                Countries con = new Countries()
                {
                    CountryName = Country.ToUpper(),
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.Countries.Add(con);
                int CountryCreated = await _context.SaveChangesAsync();

                if (CountryCreated > 0)
                {
                    response = "Country Created";
                }
                else
                {
                    response = "Something went wrong trying to create country. Please try again.";
                }
            }

            helpers.LogMessage("Creating new country. Status : " + response + " Country name : " + Country, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditCountry(int CountryId, string Country)
        {
            string response = "";
            var getCountry = from c in _context.Countries where c.CountryId == CountryId select c;

            getCountry.FirstOrDefault().CountryName = Country.ToUpper();
            getCountry.FirstOrDefault().UpdatedAt = DateTime.Now;
            getCountry.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Country Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessage("Updating country. Status : " + response + " New Country : " + Country, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var countries = await _context.Countries
                .FirstOrDefaultAsync(m => m.CountryId == id);
            if (countries == null)
            {
                return NotFound();
            }

            return View(countries);
        }



        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var getCountry = from c in _context.Countries where c.CountryId == id select c;

            getCountry.FirstOrDefault().DeletedAt = DateTime.Now;
            getCountry.FirstOrDefault().UpdatedAt = DateTime.Now;
            getCountry.FirstOrDefault().DeleteStatus = true;
            getCountry.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
               
                helpers.LogMessage("Removing country. Status : Success.  Country ID : " + id, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return BadRequest();
            }
           
        }



        private bool CountriesExists(int id)
        {
            return _context.Countries.Any(e => e.CountryId == id);
        }
    }
}
