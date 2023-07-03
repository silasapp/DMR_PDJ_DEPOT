using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;

using NewDepot.Helpers;
using NewDepot.Models;
using Microsoft.AspNetCore.Authorization;

namespace NewDepot.Controllers.Configurations
{

    public class WorkProccessesController : Controller
    {
        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();


        public WorkProccessesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        // GET: WorkProccesses
        public IActionResult Index()
        {

            var phs = _context.Phases.Where(p => p.DeleteStatus != true).ToList();
            ViewBag.Phases = phs;
          
            var loc = _context.Location.Where(p => p.DeleteStatus != true).ToList();
            ViewBag.Location = loc;
            var rol = _context.UserRoles.Where(p => p.DeleteStatus != true).ToList();
            ViewBag.Roles = rol;
            return View();
        }



        /*
         * Application process list
         */
        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult GetApplicationProcess(string id)
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

            var get = from ap in _context.WorkProccess.AsEnumerable()
                      //join sf in _context.Staff.AsEnumerable() on ap.CreatedBy equals sf.StaffID into Staff
                      join ct in _context.Categories.AsEnumerable() on ap.CategoryID equals ct.id
                      join ph in _context.Phases.AsEnumerable() on ap.PhaseID equals ph.id
                      join r in _context.UserRoles.AsEnumerable() on ap.RoleID equals r.Role_id into Role
                      join l in _context.Location.AsEnumerable() on ap.LocationID equals l.LocationID into Location
                      where (ap.DeleteStatus != true && Role.FirstOrDefault()?.DeleteStatus != true) 
                      && (Location.FirstOrDefault()?.DeleteStatus != true)
                      select new
                      {
                          ProcessID = ap.ProccessID,
                          PhaseID=ph.id,
                          PhaseName=ph.name,
                          RoleID = Role.FirstOrDefault()?.Role_id,
                          RoleName = Role.FirstOrDefault()?.RoleName,
                          LocationID = Location.FirstOrDefault()?.LocationID,
                          LocationName = Location.FirstOrDefault()?.LocationName,
                          Sort = ap.Sort,
                          canPush = ap.canPush == true ? "YES" : "NO",
                          canWork = ap.canWork == true ? "YES" : "NO",
                          canAccept = ap.canAccept == true ? "YES" : "NO",
                          canReject = ap.canReject == true ? "YES" : "NO",
                          canReport = ap.canReport == true ? "YES" : "NO",
                          canInspect = ap.canInspect == true ? "YES" : "NO",
                          canSchdule = ap.canSchdule == true ? "YES" : "NO",
                          CreatedAt = ap.CreatedAt.ToString(),
                          UpdatedAt = ap.UpdatedAt.ToString(),
                      };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    get =
                               sortColumn == "PhaseName" ? get.OrderByDescending(c => c.PhaseName).ThenBy(x => x.Sort) :
                               sortColumn == "roleName" ? get.OrderByDescending(c => c.RoleName).ThenBy(x => x.Sort) :
                               sortColumn == "locationName" ? get.OrderByDescending(c => c.LocationName).ThenBy(x => x.Sort) :
                               //sortColumn == "flowType" ? get.OrderByDescending(c => c.FlowType).ThenBy(x => x.Sort) :
                               sortColumn == "sort" ? get.OrderByDescending(c => c.Sort) :
                               sortColumn == "createdAt" ? get.OrderByDescending(c => c.CreatedAt).ThenBy(x => x.Sort) :

                               get.OrderByDescending(c => c.ProcessID + " " + sortColumnDir);
                }
                else
                {
                    get =
                               sortColumn == "PhaseName" ? get.OrderBy(c => c.PhaseName).ThenBy(x => x.Sort) :
                               sortColumn == "roleName" ? get.OrderBy(c => c.RoleName).ThenBy(x => x.Sort) :
                               sortColumn == "locationName" ? get.OrderBy(c => c.LocationName).ThenBy(x => x.Sort) :
                               sortColumn == "sort" ? get.OrderBy(c => c.Sort) :
                               sortColumn == "createdAt" ? get.OrderBy(c => c.CreatedAt).ThenBy(x => x.Sort) :

                               get.OrderBy(c => c.ProcessID + " " + sortColumnDir);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                get = get.Where(c => c.PhaseName.ToUpper().Contains(txtSearch.ToUpper()) || c.LocationName.ToUpper().Contains(txtSearch.ToUpper())  || c.RoleName.ToUpper().Contains(txtSearch.ToUpper()) || c.CreatedAt.ToUpper().Contains(txtSearch));
            }

            totalRecords = get.Count();
            var data = get.OrderBy(x => x.PhaseID).ThenBy(x => x.Sort).Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying application processes", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        /*
         * Getting process for editing
         */
        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult GetProcess(int processID)
        {
            var get = _context.WorkProccess.Where(x => x.ProccessID == processID && x.DeleteStatus != true);

            _helpersController.LogMessages("Displaying single application process. Application Process ID : " + processID, _helpersController.getSessionEmail());

            if (get.Count() > 0)
            {
       get.FirstOrDefault().RoleName = _context.UserRoles.Where(r => r.Role_id == get.FirstOrDefault().RoleID).FirstOrDefault().RoleName;
       get.FirstOrDefault().PhaseName = _context.Phases.Where(r => r.id == get.FirstOrDefault().PhaseID).FirstOrDefault().name;
       get.FirstOrDefault().LocationName = _context.Location.Where(r => r.LocationID == get.FirstOrDefault().LocationID).FirstOrDefault().LocationName;
                return Json(get.ToList());
            }
            else
            {
                return Json("The process was not found or have been deleted.");
            }
        }




        /*
         * Creating application process...
         */
        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult CreateProcess(WorkProccess Proccess)
        {
            string result = "";

            var check = _context.WorkProccess.Where(x => x.PhaseID == Proccess.PhaseID && x.RoleID == Proccess.RoleID && x.Sort == Proccess.Sort && x.DeleteStatus != true);

            if (check.Count() > 0)
            {
                result = "This application process already exits. Try a different process.";
            }
            else
            {
                var phs = _context.Phases.Where(x => x.id == Proccess.PhaseID).FirstOrDefault();

                WorkProccess nproc = new WorkProccess();
                nproc.Sort =    Proccess.Sort;
                nproc.PhaseID =    Proccess.PhaseID;
                nproc.LocationID = Proccess.LocationID;
                nproc.RoleID =     Proccess.RoleID;
                nproc.CategoryID = (int)phs?.category_id;
                nproc.PhaseName = phs?.name;
                nproc.RoleName = _context.UserRoles.Where(x=> x.Role_id == Proccess.RoleID).FirstOrDefault()?.RoleName;
                nproc.LocationName = _context.Location.Where(x=> x.LocationID == Proccess.LocationID).FirstOrDefault()?.LocationName;
                nproc.CreatedAt =  DateTime.Now;
                nproc.CreatedBy =  _helpersController.getSessionUserID();

                _context.WorkProccess.Add(nproc);
                int done = _context.SaveChanges();

                if (done > 0)
                {
                    result = "Process Created";
                }
                else
                {
                    result = "Process not created. Something went wrong trying to create this process.";
                }
            }

            _helpersController.LogMessages("Creating application process. Status : " + result, _helpersController.getSessionEmail());

            return Json(result);
        }




        /*
         * Creating application proccess clone
         */
        //////[Authorize(Policy = "ConfigurationRoles")]




        /*
         * Editing Application process
         * 
         */
        ////[Authorize(Policy = "ConfigurationRoles")]
        public IActionResult EditProcess(int processID, WorkProccess Proccess)
        {
            string result = "";
            var check = _context.WorkProccess.Where(x => x.ProccessID == processID && x.DeleteStatus != true);

            if (check.Count() > 0)
            {
                var phs = _context.Phases.Where(x => x.id == Proccess.PhaseID).FirstOrDefault();

                check.FirstOrDefault().UpdatedAt = DateTime.Now;
                check.FirstOrDefault().UpdatedBy =  _helpersController.getSessionUserID();
                check.FirstOrDefault().PhaseID = Proccess.PhaseID;
                check.FirstOrDefault().Sort = Proccess.Sort;
                check.FirstOrDefault().RoleID = Proccess.RoleID;
                check.FirstOrDefault().LocationID = Proccess.LocationID;
                check.FirstOrDefault().CategoryID = (int)phs?.category_id;
                check.FirstOrDefault().PhaseName = phs?.name;
                check.FirstOrDefault().RoleName = _context.UserRoles.Where(x => x.Role_id == Proccess.RoleID).FirstOrDefault()?.RoleName;
                check.FirstOrDefault().LocationName = _context.Location.Where(x => x.LocationID == Proccess.LocationID).FirstOrDefault()?.LocationName;
               

                int done = _context.SaveChanges();

                if (done > 0)
                {
                    result = "Process Updated";
                }
                else
                {
                    result = "Nothing was updated.";
                }
            }
            else
            {
                result = "This application process was not found or have been deleted.";
            }

            _helpersController.LogMessages("Updating application process. Status : " + result + " Application Process ID : " + processID, _helpersController.getSessionEmail());

            return Json(result);
        }


        /*
         * Removing application process
         */
        ////[Authorize(Policy = "ConfigurationRoles")]
        public IActionResult DeleteProcess(int processID)
        {
            string response = "";

            var check = from s in _context.WorkProccess where s.ProccessID == processID && s.DeleteStatus != true select s;

            if (check.Count() > 0)
            {
                check.FirstOrDefault().DeleteStatus = true;
                check.FirstOrDefault().DeletedBy =  _helpersController.getSessionUserID();
                check.FirstOrDefault().DeletedAt = DateTime.Now;

                int done = _context.SaveChanges();

                if (done > 0)
                {
                    response = "Process Removed";
                }
                else
                {
                    response = "Something went wrong trying to remove this process. Try again.";
                }
            }
            else
            {
                response = "This Application process was not found.";
            }

            _helpersController.LogMessages("Deleting application process. Status : " + response + " Application Process ID : " + processID, _helpersController.getSessionEmail());

            return Json(response);
        }

    }
}
