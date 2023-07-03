using System;
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
    public class FieldOfficesController : Controller
    {
        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();



        public FieldOfficesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        // GET: FieldOffices
        public async Task<IActionResult> Index()
        {
            return View(await _context.FieldOffices.ToListAsync());
        }



        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult GetFieldOffice()
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

            var getFieldOffice = from c in _context.FieldOffices
                                 where c.DeleteStatus == false
                                 select new
                                 {
                                     FieldOfficeID = c.FieldOffice_id,
                                     OfficeName = c.OfficeName,
                                     //Address = c.OfficeAddress,
                                     UpdatedAt = c.UpdatedAt.ToString(),
                                     CreatedAt = c.CreatedAt.ToString()
                                 };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getFieldOffice = sortColumn == "officeName" ? getFieldOffice.OrderByDescending(c => c.OfficeName) :
                               sortColumn == "updatedAt" ? getFieldOffice.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getFieldOffice.OrderByDescending(c => c.CreatedAt) :
                               getFieldOffice.OrderByDescending(c => c.FieldOfficeID + " " + sortColumnDir);
                }
                else
                {
                    getFieldOffice = sortColumn == "officeName" ? getFieldOffice.OrderBy(c => c.OfficeName) :
                               sortColumn == "updatedAt" ? getFieldOffice.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getFieldOffice.OrderBy(c => c.CreatedAt) :
                               getFieldOffice.OrderBy(c => c.FieldOfficeID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getFieldOffice = getFieldOffice.Where(c => c.OfficeName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getFieldOffice.Count();
            var data = getFieldOffice.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all field officess...", _helpersController.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        // POST: FieldOffices/Create
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> CreateFieldOffice(string OfficeName, string OfficeAddress)
        {
            string response = "";

            var country = from s in _context.FieldOffices
                          where s.OfficeName == OfficeName.ToUpper() && s.DeleteStatus == false
                          select s;

            if (country.Count() > 0)
            {
                response = "Field Office already exits, please enter another Field Office.";
            }
            else
            {
                FieldOffices _fieldOffice = new FieldOffices()
                {
                    OfficeName = OfficeName.ToUpper(),
                    CreatedAt = DateTime.Now,
                    //OfficeAddress = OfficeAddress.ToUpper(),
                    DeleteStatus = false
                };

                _context.FieldOffices.Add(_fieldOffice);
                int OfficeCreated = await _context.SaveChangesAsync();

                if (OfficeCreated > 0)
                {
                    response = "Office Created";
                }
                else
                {
                    response = "Something went wrong trying to create this field office. Please try again.";
                }
            }

            _helpersController.LogMessages("Creating new field office. Status : " + response + " field office name : " + OfficeName, _helpersController.getSessionEmail());

            return Json(response);

        }





        /*
         * Edit field Office
         */
        [Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> EditFieldOffice(string OfficeName, int FieldOfficeId, string OfficeAddress)
        {
            string response = "";

            var getFieldOffice = from c in _context.FieldOffices where c.FieldOffice_id == FieldOfficeId select c;

            getFieldOffice.FirstOrDefault().OfficeName = OfficeName.ToUpper();
            //getFieldOffice.FirstOrDefault().OfficeAddress = OfficeAddress.ToUpper();
            getFieldOffice.FirstOrDefault().UpdatedAt = DateTime.Now;
            getFieldOffice.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Office Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            _helpersController.LogMessages("Updating field office. Status : " + response + " field office ID: " + FieldOfficeId, _helpersController.getSessionEmail());

            return Json(response);
        }




        // GET: FieldOffices/Delete/
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> DeleteOffice(int FieldOfficeID)
        {
            string response = "";

            var getState = from c in _context.FieldOffices where c.FieldOffice_id == FieldOfficeID select c;

            getState.FirstOrDefault().DeletedAt = DateTime.Now;
            getState.FirstOrDefault().UpdatedAt = DateTime.Now;
            getState.FirstOrDefault().DeleteStatus = true;
            getState.FirstOrDefault().DeletedBy =  _helpersController.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Office Deleted";
            }
            else
            {
                response = "Office not deleted. Something went wrong trying to delete this Field Office.";
            }

            _helpersController.LogMessages("Deleting field office. Status : " + response + " field office ID : " + FieldOfficeID, _helpersController.getSessionEmail());

            return Json(response);
        }

    }
}
