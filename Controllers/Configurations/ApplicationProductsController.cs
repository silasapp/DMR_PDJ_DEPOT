using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using NewDepot.Helpers;
using NewDepot.Models;
using Microsoft.AspNetCore.Authorization;

namespace NewDepot.Controllers.Configurations
{
    public class ApplicationProductsController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public ApplicationProductsController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



        // GET: AppProducts
        ////[Authorize(Policy = "ConfigurationRoles")]
        public IActionResult Index()
        {

            return View();
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult GetProduct()
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

            var get = from p in _context.Products
                      where p.DeletedStatus!= true
                      select new
                      {
                          ProductID = p.Id,
                          ProductName = p.Name,
                          ShortName = p.FriendlyName,
                          UpdatedAt = p.UpdatedAt.ToString(),
                         CreatedAt = p.CreatedAt.ToString()
                      };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    get = sortColumn ==  "productName" ? get.OrderByDescending(t => t.ProductName) :
                                    sortColumn == "updatedAt" ? get.OrderByDescending(s => s.UpdatedAt) :
                                    sortColumn == "createdAt" ? get.OrderByDescending(s => s.CreatedAt) :
                                    get.OrderByDescending(ts => ts.ProductID + " " + sortColumnDir);
                }
                else
                {
                    get = sortColumn == "productName" ? get.OrderBy(t => t.ProductName) :
                                   sortColumn == "updatedAt" ? get.OrderBy(c => c.UpdatedAt) :
                                   sortColumn == "createdAt" ? get.OrderBy(c => c.CreatedAt) :
                                   get.OrderBy(ts => ts.ProductID);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                get = get.Where(c => c.ShortName.Contains(txtSearch.ToUpper()) || c.ProductName.Contains(txtSearch.ToUpper()));
            }

            totalRecords = get.Count();
            var data = get.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all application and product", _helpersController.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> CreateProduct(string Name, string FriendlyName)
        {
            string response = "";

            var check = from ts in _context.Products
                        where ts.Name.ToLower()==Name.ToLower() || ts.FriendlyName.ToLower() == FriendlyName.ToLower()
                        select new
                        {
                            ts.Name
                        };

            if (check.Count() > 0)
            {
                response = check.FirstOrDefault().Name + " product already exits.";
            }
            else
            {
                Products products = new Products()
                {
                    Name=Name,
                    FriendlyName=FriendlyName,
                    CreatedAt = DateTime.Now,
                    DeletedStatus = false
                };

                _context.Products.Add(products);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "Created";
                }
                else
                {
                    response = "Something went wrong trying to create Product. Please try again.";
                }
            }

            _helpersController.LogMessages("Creating new product. Status : " + response + " Application product name : " + Name, _helpersController.getSessionEmail());

            return Json(response);
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> EditProduct(int ProductId, string Name, string FriendlyName)
        {
            string response = "";
            var check = from x in _context.Products where x.Id == ProductId select x;

            if (check.Count() > 0)
            {  
                
                check.FirstOrDefault().Name = Name;
                check.FirstOrDefault().FriendlyName = FriendlyName;
                check.FirstOrDefault().UpdatedAt = DateTime.Now;
                check.FirstOrDefault().DeletedStatus = false;

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

            _helpersController.LogMessages("Updating application product. Status : " + response + " Application product ID : " + ProductId, _helpersController.getSessionEmail());

            return Json(response);
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> DeleteProduct(int ProductID)
        {
            string response = "";

            var get = from c in _context.Products where c.Id == ProductID select c;

            get.FirstOrDefault().DeletedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().DeletedStatus = true;
            get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Deleted";
            }
            else
            {
                response = "Product => not deleted. Something went wrong trying to delete this entry.";
            }

            _helpersController.LogMessages("Deleting product. Status : " + response + " Application Product ID : " + ProductID, _helpersController.getSessionEmail());

            return Json(response);
        }



    }
}
