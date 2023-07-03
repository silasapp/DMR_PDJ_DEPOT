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
    public class ApplicationPhasesController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public ApplicationPhasesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        ////[Authorize(Policy = "ConfigurationRoles")]
        public IActionResult Index()
        {

            var cat = _context.Categories.Where(p => p.DeleteStatus!= true).ToList();
            ViewBag.Categories = cat;
        

            var loc = _context.Location.Where(p => p.DeleteStatus != true).ToList();
            ViewBag.Location = loc;
            return View();
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult GetAppPhases()
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

            var getAppType = from r in _context.Phases
                             join c in _context.Categories on r.category_id equals c.id
                             where r.DeleteStatus != true
                             select new
                             {
                                 PhsId = r.id,
                                 PhaseName = r.name,
                                ShortName = r.ShortName,
                                CreatedAt = r.CreatedAt.ToString(),
                                UpdatedAt = r.UpdatedAt.ToString(),
                                IssueType = r.IssueType,
                                FlowType = r.FlowType,
                                CategoryName = c.name ,
                                Description = r.Description,
                                ServiceCharge = r.ServiceCharge,
                                Price = r.Price,
                                ProcessingFee = r.ProcessingFee,
                                ProcessingFeeByTank = r.ProcessingFeeByTank == true ? "Yes" : "No",
                                PriceByVolume = r.PriceByVolume == true ? "Yes" : "No",
                                
                             };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getAppType = sortColumn == "phaseName" ? getAppType.OrderByDescending(c => c.PhaseName) :
                               sortColumn == "shortName" ? getAppType.OrderByDescending(c => c.ShortName) :
                               sortColumn == "price" ? getAppType.OrderByDescending(c => c.Price) :
                               sortColumn == "updatedAt" ? getAppType.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppType.OrderByDescending(c => c.CreatedAt) :
                               getAppType.OrderByDescending(c => c.PhsId + " " + sortColumnDir);
                }
                else
                {
                    getAppType = sortColumn == "phaseName" ? getAppType.OrderBy(c => c.PhaseName) :
                               sortColumn == "shortName" ? getAppType.OrderByDescending(c => c.ShortName) :
                               sortColumn == "price" ? getAppType.OrderByDescending(c => c.Price) :
                               sortColumn == "updatedAt" ? getAppType.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppType.OrderBy(c => c.CreatedAt) :
                               getAppType.OrderBy(c => c.PhsId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getAppType = getAppType.Where(c => c.PhaseName.Contains(txtSearch)||c.FlowType.Contains(txtSearch)|| c.CategoryName.Contains(txtSearch)|| c.CategoryName.Contains(txtSearch)|| c.CategoryName.Contains(txtSearch) || c.ShortName.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getAppType.Count();
            var data = getAppType.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying application phase.", _helpersController.getSessionEmail());


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        // POST: CreatePhase/Create
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> CreatePhase(string CategoryName, string PhaseName, string ShortName, string Price, string ServiceCharge, string Description, string PriceByVolume, string ProcessingFee, string ProcessingFeeByTank, string SanctionFee, string IssueType, string FlowType)
        {
            string response = "";

            var getAppType = from a in _context.Phases
                             where a.name.ToLower() == PhaseName.ToLower() && a.category_id.ToString() == CategoryName && a.DeleteStatus != true
                             select a;

            if (getAppType.Count() > 0)
            {
                response = "Application phase already exist under selected category, please enter another type.";
            }
            else
            {
                try {
                    Phases Phase = new Phases()
                    {
                        name = PhaseName,
                        ShortName = ShortName,
                        IssueType = IssueType,
                        FlowType = FlowType,
                        category_id = Convert.ToInt16(CategoryName),
                        Description = Description,
                        ServiceCharge = Convert.ToInt32(ServiceCharge),
                        Price = Convert.ToInt64(Price),
                        ProcessingFee = Convert.ToInt64(ProcessingFee),
                        ProcessingFeeByTank = ProcessingFeeByTank == "Yes" ? true : false,
                        PriceByVolume = PriceByVolume == "Yes" ? true : false,
                        
                        CreatedAt = DateTime.Now,
                        DeleteStatus = false
                    };
                    if (SanctionFee != "") {
                        Phase.SanctionFee = Convert.ToDecimal(SanctionFee);
                    }

                    _context.Phases.Add(Phase);
                    int Created = await _context.SaveChangesAsync();

                    if (Created > 0)
                    {
                        response = "Phase Created";
                    }
                    else
                    {
                        response = "Something went wrong trying to create this App Phase. Please try again.";
                    }
                }
                catch(Exception e)
                {
                    response = e.Message;
                }
                }

            _helpersController.LogMessages("Creating application Phase. Status : " + response + " Application Phase name : " + PhaseName, _helpersController.getSessionEmail());

            return Json(response);
        }

        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> EditPhase(int PhsId, string PhaseName, string CategoryName,string ServiceCharge, string ShortName, string Price, string Description, string PriceByVolume, string ProcessingFee, string ProcessingFeeByTank, string SanctionFee, string IssueType, string FlowType)

        {
            string response = "";
            var get = from c in _context.Phases where c.id == PhsId select c;
            var getCategory = from c in _context.Categories where c.id.ToString() == CategoryName || c.name == CategoryName select c;

            get.FirstOrDefault().name = PhaseName;
            get.FirstOrDefault().ShortName = ShortName;
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().DeleteStatus = false;
            get.FirstOrDefault().IssueType = IssueType;
            get.FirstOrDefault().FlowType = FlowType;
            get.FirstOrDefault().category_id = getCategory.FirstOrDefault().id;
            get.FirstOrDefault().Description = Description;
            get.FirstOrDefault().ServiceCharge = Convert.ToInt32(ServiceCharge);
            get.FirstOrDefault().Price = Convert.ToInt64(Price);
            get.FirstOrDefault().ProcessingFee = Convert.ToInt64(ProcessingFee);
            get.FirstOrDefault().ProcessingFeeByTank = ProcessingFeeByTank == "Yes" ? true : false;
            get.FirstOrDefault().PriceByVolume = PriceByVolume == "Yes" ? true : false;

            if (SanctionFee != "")
            {
                get.FirstOrDefault().SanctionFee = Convert.ToDecimal(SanctionFee);
            }

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Phase Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            _helpersController.LogMessages("Updating application Phase. Status : " + response + " Application Phase ID : " + PhsId, _helpersController.getSessionEmail());

            return Json(response);
        }




        // POST: ApplicationTypes/Delete/5
        ////[Authorize(Policy = "ConfigurationRoles")]
        public async Task<IActionResult> DeleteAppPhase(int PhsId)
        {
            string response = "";

            var get = from c in _context.Phases where c.id == PhsId select c;

            if (get != null)
            {

                get.FirstOrDefault().DeletedAt = DateTime.Now;
                get.FirstOrDefault().UpdatedAt = DateTime.Now;
                get.FirstOrDefault().DeleteStatus = true;
                get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();

                int updated = await _context.SaveChangesAsync();

                if (updated > 0)
                {
                    response = "Phase Deleted";
                }
                else
                {
                    response = "Application Phase not deleted. Something went wrong trying to delete this application Phase.";
                }
            }
            else
            {
                response = "Application phase id was not passed correctly.";
            }

            _helpersController.LogMessages("Deleting application Phase. Status : " + response + " Application Phase ID : " + PhsId, _helpersController.getSessionEmail());

            return Json(response);
        }




    }
}
