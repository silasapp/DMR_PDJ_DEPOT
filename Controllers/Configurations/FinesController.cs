//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;

//using NewDepot.Helpers;
//using Microsoft.AspNetCore.Authorization;
//using NewDepot.Models;

//namespace NewDepot.Controllers.Configurations
//{
//    //[Authorize(Policy = "ProcessingStaffRoles")]
//    public class FinesController : Controller
//    {
//        private readonly Depot_DBContext _context;
//        IHttpContextAccessor _httpContextAccessor;
//        public IConfiguration _configuration;
//        HelpersController _helpersController;
//        GeneralClass generalClass = new GeneralClass();

//        public FinesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
//        {
//            _context = context;
//            _configuration = configuration;
//            _httpContextAccessor = httpContextAccessor;
//            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
//        }



        
//        public async Task<IActionResult> Index()
//        {
//            return View(await _context.Fines.ToListAsync());
//        }



//        public JsonResult GetFines()
//        {
//            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
//            var start = HttpContext.Request.Form["start"].FirstOrDefault();
//            var length = HttpContext.Request.Form["length"].FirstOrDefault();
//            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
//            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
//            var txtSearch = HttpContext.Request.Form["search[value]"][0];

//            int pageSize = length != null ? Convert.ToInt32(length) : 0;
//            int skip = start != null ? Convert.ToInt32(start) : 0;
//            int totalRecords = 0;

//            var getFines = from r in _context.Fines
//                              where r.DeleteStatus == false
//                              select new
//                              {
//                                  FineID = r.FineId,
//                                  FineName = r.FineName,
//                                  FineDescription = r.FineDescription,
//                                  FineAmount = "₦" + string.Format("{0:N}", r.Amount).ToString(),
//                                  ServiceCharge = r.ServiceCharge,
//                                  UpdatedAt = r.UpdatedAt.ToString(),
//                                  CreatedAt = r.CreatedAt.ToString(),
//                              };

//            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
//            {
//                if (sortColumnDir == "desc")
//                {
//                    getFines = sortColumn == "fineName" ? getFines.OrderByDescending(c => c.FineName) :
//                               sortColumn == "createdAt" ? getFines.OrderByDescending(c => c.CreatedAt) :
//                               getFines.OrderByDescending(c => c.FineID + " " + sortColumnDir);
//                }
//                else
//                {
//                    getFines = sortColumn == "fineName" ? getFines.OrderBy(c => c.FineName) :
//                               sortColumn == "createdAt" ? getFines.OrderBy(c => c.CreatedAt) :
//                               getFines.OrderBy(c => c.FineID);
//                }
//            }

//            if (!string.IsNullOrWhiteSpace(txtSearch))
//            {
//                getFines = getFines.Where(c => c.FineName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
//            }

//            totalRecords = getFines.Count();
//            var data = getFines.Skip(skip).Take(pageSize).ToList();

//            _helpersController.LogMessages("Displaying application fines.", _helpersController.getSessionEmail());

//            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });
//        }





      
//        public async Task<IActionResult> CreateFine(string FineName, string FineDescription, int FineAmount, int ServiceCharge)
//        {
//            string response = "";

//            var appstage = from a in _context.Fines
//                           where a.FineName == FineName.ToUpper() && a.DeleteStatus == false
//                           select a;

//            if (appstage.Count() > 0)
//            {
//                response = "Application fine already exits, please enter another fine.";
//            }
//            else
//            {
//                fines con = new Fines()
//                {
//                    FineName = FineName.ToUpper(),
//                    FineDescription = FineDescription,
//                    ServiceCharge = ServiceCharge,
//                    Amount = FineAmount,
//                    CreatedAt = DateTime.Now,
//                    DeleteStatus = false
//                };

//                _context.Fines.Add(con);
//                int Created = await _context.SaveChangesAsync();

//                if (Created > 0)
//                {
//                    response = "Fine Created";
//                }
//                else
//                {
//                    response = "Something went wrong trying to create this fine. Please try again.";
//                }
//            }

//            _helpersController.LogMessages("Creating application fine. Status : " + response + " Application Fine Name : " + FineName.ToUpper(), _helpersController.getSessionEmail());

//            return Json(response);
//        }





//        public async Task<IActionResult> EditFine(int FineID, string FineName, string FineDescription, int FineAmount, int ServiceCharge)
//        {
//            string response = "";
//            var getFine = from c in _context.Fines where c.FineId == FineID select c;

//            getFine.FirstOrDefault().FineName = FineName.ToUpper();
//            getFine.FirstOrDefault().ServiceCharge = ServiceCharge;
//            getFine.FirstOrDefault().FineDescription = FineDescription;
//            getFine.FirstOrDefault().Amount = FineAmount;
//            getFine.FirstOrDefault().UpdatedAt = DateTime.Now;
//            getFine.FirstOrDefault().DeleteStatus = false;

//            int updated = await _context.SaveChangesAsync();

//            if (updated > 0)
//            {
//                response = "Fine Updated";
//            }
//            else
//            {
//                response = "Nothing was updated.";
//            }

//            _helpersController.LogMessages("Updating application Fine. Status : " + response + " Application fine ID : " + FineID, _helpersController.getSessionEmail());

//            return Json(response);
//        }



//        public async Task<IActionResult> DeleteFine(int FineID)
//        {
//            string response = "";

//            var getFine = from c in _context.Fines where c.FineId == FineID select c;

//            getFine.FirstOrDefault().DeletedAt = DateTime.Now;
//            getFine.FirstOrDefault().UpdatedAt = DateTime.Now;
//            getFine.FirstOrDefault().DeleteStatus = true;
//            getFine.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();

//            int updated = await _context.SaveChangesAsync();

//            if (updated > 0)
//            {
//                response = "Fine Deleted";
//            }
//            else
//            {
//                response = "Application Fine not deleted. Something went wrong trying to delete this application fine.";
//            }

//            _helpersController.LogMessages("Deleting application fine. Status : " + response + " Application fine ID : " + FineID, _helpersController.getSessionEmail());

//            return Json(response);
//        }

//    }
//}
