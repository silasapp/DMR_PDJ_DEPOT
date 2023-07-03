using Microsoft.AspNetCore.Mvc;

using NewDepot.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Models;

namespace NewDepot.Controllers.Configurations
{
    public class ApplicationCategoriesController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public ApplicationCategoriesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        ////[Authorize(Policy = "ConfigurationRoles")]
        public IActionResult Index()
        {
            return View();
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult GetAppCategory()
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

            var getAppType = from r in _context.Categories
                             where r.DeleteStatus != true
                             select new
                             {
                                 CatId = r.id,
                                 CategoryName = r.name,
                                 FriendlyName = r.FriendlyName,
                                 UpdatedAt = r.UpdatedAt.ToString(),
                                 CreatedAt = r.CreatedAt.ToString()
                             };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getAppType = sortColumn == "categoryName" ? getAppType.OrderByDescending(c => c.CategoryName) :
                               sortColumn == "friendlyName" ? getAppType.OrderByDescending(c => c.FriendlyName) :
                               sortColumn == "updatedAt" ? getAppType.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppType.OrderByDescending(c => c.CreatedAt) :
                               getAppType.OrderByDescending(c => c.CatId + " " + sortColumnDir);
                }
                else
                {
                    getAppType = sortColumn == "categoryName" ? getAppType.OrderBy(c => c.CategoryName) :
                               sortColumn == "friendlyName" ? getAppType.OrderByDescending(c => c.FriendlyName) :
                               sortColumn == "updatedAt" ? getAppType.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppType.OrderBy(c => c.CreatedAt) :
                               getAppType.OrderBy(c => c.CatId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getAppType = getAppType.Where(c => c.CategoryName.Contains(txtSearch.ToUpper()) || c.FriendlyName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getAppType.Count();
            var data = getAppType.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying application category.", _helpersController.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        // POST: CreateCategory/Create
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> CreateCategory(string CategoryName, string FriendlyName)
        {
            string response = "";

            var getAppType = from a in _context.Categories
                             where a.name == CategoryName.ToUpper() && a.DeleteStatus != true
                             select a;

            if (getAppType.Count() > 0)
            {
                response = "Application category already exist.";
            }
            else
            {
               Categories Category  = new Categories ()
                {
                    name = CategoryName.ToUpper(),
                    FriendlyName=FriendlyName,
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.Categories.Add(Category);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "Category Created";
                }
                else
                {
                    response = "Something went wrong trying to create this App Category. Please try again.";
                }
            }

            _helpersController.LogMessages("Creating application Category. Status : " + response + " Application Category name : " + CategoryName, _helpersController.getSessionEmail());

            return Json(response);
        }



        // POST: ApplicationTypes/Edit/5
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> EditCategory(int CatId, string CategoryName, string FriendlyName)
        {
            string response = "";
            var get = from c in _context.Categories where c.id == CatId select c;

            get.FirstOrDefault().name = CategoryName;
            get.FirstOrDefault().FriendlyName = FriendlyName;
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Category Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            _helpersController.LogMessages("Updating application Category. Status : " + response + " Application Category ID : " + CatId, _helpersController.getSessionEmail());

            return Json(response);
        }




        // POST: ApplicationTypes/Delete/5
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> DeleteAppCategory(int CatId)
        {
            string response = "";

            var get = from c in _context.Categories where c.id == CatId select c;

            //checked if category has phase that has not been deleted
            var phase = _context.Phases.Where(u => u.category_id == CatId && u.DeleteStatus != true).FirstOrDefault();
            if (phase == null)
            {

                get.FirstOrDefault().DeletedAt = DateTime.Now;
                get.FirstOrDefault().UpdatedAt = DateTime.Now;
                get.FirstOrDefault().DeleteStatus = true;
                get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();

                int updated = await _context.SaveChangesAsync();

                if (updated > 0)
                {
                    response = "Category Deleted";
                }
                else
                {
                    response = "Application Category not deleted. Something went wrong trying to delete this application Category.";
                }
            }
            else
            {
                response = "Application category has phase(s) that are operational, hence deletion can not be done.";
            }

            _helpersController.LogMessages("Deleting application Category. Status : " + response + " Application Category ID : " + CatId, _helpersController.getSessionEmail());

            return Json(response);
        }




    }
}
