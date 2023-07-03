using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Helpers;
using Microsoft.AspNetCore.Http;
using NewDepot.Helpers;
using Microsoft.Extensions.Configuration;
using NewDepot.Models;
using Microsoft.EntityFrameworkCore;

namespace NewDepot.Controllers
{
    [Authorize]
    public class ConfigurationsController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public ConfigurationsController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        // //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        // GET: ApplicationDocuments

        #region Application Documents
        public IActionResult ApplicationDocuments()
        {
            return View();
        }


        public JsonResult GetAppDoc()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getAppDoc = from r in _context.ApplicationDocuments
                            where r.DeleteStatus == false
                            select new
                            {
                                AppDocID = r.AppDocID,
                                AppDocElpsID = r.ElpsDocTypeID,
                                DocName = r.DocName,
                                DocPhase = r.docType,
                                UpdatedAt = r.UpdatedAt.ToString(),
                                CreatedAt = r.CreatedAt.ToString()
                            };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getAppDoc = sortColumn == "docName" ? getAppDoc.OrderByDescending(c => c.DocName) :
                               sortColumn == "updatedAt" ? getAppDoc.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppDoc.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "docPhase" ? getAppDoc.OrderByDescending(c => c.DocPhase) :
                               getAppDoc.OrderByDescending(c => c.AppDocID + " " + sortColumnup);
                }
                else
                {
                    getAppDoc = sortColumn == "docName" ? getAppDoc.OrderBy(c => c.DocName) :
                               sortColumn == "updatedAt" ? getAppDoc.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppDoc.OrderBy(c => c.CreatedAt) :
                               sortColumn == "docPhase" ? getAppDoc.OrderBy(c => c.DocPhase) :
                               getAppDoc.OrderBy(c => c.AppDocID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getAppDoc = getAppDoc.Where(c => c.DocName.Contains(txtSearch.ToUpper()) || c.DocPhase.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getAppDoc.Count();
            var data = getAppDoc.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying application documents", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        // POST: ApplicationDocuments/Create
        public async Task<IActionResult> CreateAppDoc(int AppDocElpsID, string AppDocName, string AppDocPhase)
        {
            string response = "";

            var appCategory = from a in _context.ApplicationDocuments
                           where (a.DocName == AppDocName.ToUpper() && a.ElpsDocTypeID == AppDocElpsID && a.docType == AppDocPhase && a.DeleteStatus == false)
                           select a;

            if (appCategory.Count() > 0)
            {
                response = "Application document name already exits, please enter another name.";
            }
            else
            {
                ApplicationDocuments con = new ApplicationDocuments()
                {
                    DocName = AppDocName.ToUpper(),
                    docType = AppDocPhase,
                    ElpsDocTypeID = AppDocElpsID,
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.ApplicationDocuments.Add(con);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "AppDoc Created";
                }
                else
                {
                    response = "Something went wrong trying to create this App Doc. Please try again.";
                }
            }

            helpers.LogMessages("Creating application documents. Status : " + response, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }




        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        // POST: ApplicationDocuments/Edit/5
        public async Task<IActionResult> EditAppDoc(int AppDocID, string AppDocName, string AppDocPhase)
        {
            string response = "";
            var getAppDoc = from c in _context.ApplicationDocuments where c.AppDocID == AppDocID select c;

            getAppDoc.FirstOrDefault().DocName = AppDocName.ToUpper();
            getAppDoc.FirstOrDefault().docType = AppDocPhase;
            getAppDoc.FirstOrDefault().UpdatedAt = DateTime.Now;
            getAppDoc.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "AppDoc Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating application documents. Status : " + response + " Application Document ID : " + AppDocID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
            return Json(response);
        }





        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        // POST: ApplicationDocuments/Delete/5
        public async Task<IActionResult> DeleteAppDoc(int AppDocID)
        {
            string response = "";

            var getAppDoc = from c in _context.ApplicationDocuments where c.AppDocID == AppDocID select c;

            getAppDoc.FirstOrDefault().DeletedAt = DateTime.Now;
            getAppDoc.FirstOrDefault().UpdatedAt = DateTime.Now;
            getAppDoc.FirstOrDefault().DeleteStatus = true;
            getAppDoc.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "AppDoc Deleted";
            }
            else
            {
                response = "Application document not deleted. Something went wrong trying to delete this application document.";
            }

            helpers.LogMessages("Deleting application documents. Status : " + response + " Application Document ID : " + AppDocID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }

        private bool ApplicationDocumentsExists(int id)
        {
            return _context.ApplicationDocuments.Any(e => e.AppDocID == id);
        }
        #endregion
        #region Application Process

        // GET: Application Proccesses
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> ApplicationProccess()
        {
            return View(await _context.WorkProccess.ToListAsync());
        }



        /*
         * Application process list
         */
        public JsonResult GetApplicationProcess()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var get = from ap in _context.WorkProccess
                      join sf in _context.Staff on ap.CreatedBy equals sf.StaffID into Staff
                      join sfu in _context.Staff on ap.UpdatedBy equals sfu.StaffID into StaffU
                      join ph in _context.Phases on ap.PhaseID equals ph.id into Phase
                      join ct in _context.Categories on ap.CategoryID equals ct.id into Category
                      join r in _context.UserRoles on ap.RoleID equals r.Role_id into Role
                      join l in _context.Location on ap.LocationID equals l.LocationID into Location
                      where ((ap.DeleteStatus == false) && (Phase.FirstOrDefault().Deleted == false)
                      //&& (Category.FirstOrDefault().de == false)
                      && (Role.FirstOrDefault().DeleteStatus == false) && (Location.FirstOrDefault().DeleteStatus == false)
                      && (Staff.FirstOrDefault().DeleteStatus == false))
                      select new
                      {
                          ProcessID = ap.ProccessID,
                          PhaseID = Phase.FirstOrDefault().id,
                          PhaseName = Phase.FirstOrDefault().name,
                          CategoryID = Category.FirstOrDefault().id,
                          CategoryName = Category.FirstOrDefault().name,
                          RoleID = Role.FirstOrDefault().Role_id,
                          RoleName = Role.FirstOrDefault().RoleName,
                          LocationID = Location.FirstOrDefault().LocationID,
                          LocationName = Location.FirstOrDefault().LocationName,
                          Sort = ap.Sort,
                          CanPush = ap.canPush == true ? "YES" : "NO",
                          CanWork = ap.canWork == true ? "YES" : "NO",
                          CanAccept = ap.canAccept == true ? "YES" : "NO",
                          CanReject = ap.canReject == true ? "YES" : "NO",
                          CanReport = ap.canReport == true ? "YES" : "NO",
                          CanInspect = ap.canInspect == true ? "YES" : "NO",
                          CanSchdule = ap.canSchdule == true ? "YES" : "NO",
                          CreatedAt = ap.CreatedAt.ToString(),
                          CreatedBy = Staff.FirstOrDefault().FirstName + " " + Staff.FirstOrDefault().LastName,
                          UpdatedAt = ap.UpdatedAt.ToString(),
                          UpdatedBy = StaffU.FirstOrDefault().FirstName + " " + StaffU.FirstOrDefault().LastName,

                      };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    get = sortColumn == "PhaseName" ? get.OrderByDescending(c => c.PhaseName) :
                               sortColumn == "CategoryName" ? get.OrderByDescending(c => c.CategoryName) :
                               sortColumn == "roleName" ? get.OrderByDescending(c => c.RoleName) :
                               sortColumn == "locationName" ? get.OrderByDescending(c => c.LocationName) :
                               sortColumn == "sort" ? get.OrderByDescending(c => c.Sort) :
                               sortColumn == "createdAt" ? get.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "updatedAt" ? get.OrderByDescending(c => c.UpdatedAt) :
                               get.OrderByDescending(c => c.ProcessID + " " + sortColumnup);
                }
                else
                {
                    get = sortColumn == "PhaseName" ? get.OrderBy(c => c.PhaseName) :
                               sortColumn == "CategoryName" ? get.OrderBy(c => c.CategoryName) :
                               sortColumn == "roleName" ? get.OrderBy(c => c.RoleName) :
                               sortColumn == "locationName" ? get.OrderBy(c => c.LocationName) :
                               sortColumn == "sort" ? get.OrderBy(c => c.Sort) :
                               sortColumn == "createdAt" ? get.OrderBy(c => c.CreatedAt) :
                               sortColumn == "updatedAt" ? get.OrderBy(c => c.UpdatedAt) :
                               get.OrderBy(c => c.ProcessID + " " + sortColumnup);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                get = get.Where(c => c.PhaseName.Contains(txtSearch.ToUpper()) || c.CategoryName.Contains(txtSearch.ToUpper()) || c.LocationName.Contains(txtSearch.ToUpper()) || c.RoleName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = get.Count();
            var data = get.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying application processes", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        /*
         * Getting process for editing
         */
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public JsonResult GetProcess(int processID)
        {
            var get = _context.WorkProccess.Where(x => x.ProccessID == processID && x.DeleteStatus == false);

            helpers.LogMessages("Displaying single application process. Application Process ID : " + processID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            if (get.Count() > 0)
            {
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
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult CreateProcess(WorkProccess Process)
        {
            string result = "";

            var check = _context.WorkProccess.Where(x => x.PhaseID== Process.PhaseID && x.CategoryID == Process.CategoryID && x.RoleID == Process.RoleID
            && x.LocationID == Process.LocationID && x.Sort == Process.Sort && x.DeleteStatus == false);

            if (check.Count() > 0)
            {
                result = "This Application process already exits. Try a different process.";
            }
            else
            {
                Process.CreatedAt = DateTime.Now;
                Process.CreatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

                _context.WorkProccess.Add(Process);
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

            helpers.LogMessages("Creating application process. Status : " + result, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(result);
        }



        /*
         * Editing Application process
         * 
         */
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult EditProcess(int processID, WorkProccess Process)
        {
            string result = "";
            var check = _context.WorkProccess.Where(x => x.ProccessID == processID && x.DeleteStatus == false);

            if (check.Count() > 0)
            {
                check.FirstOrDefault().UpdatedAt = DateTime.Now;
                check.FirstOrDefault().UpdatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));
                check.FirstOrDefault().PhaseID = Process.PhaseID;
                check.FirstOrDefault().CategoryID = Process.CategoryID;
                check.FirstOrDefault().Sort = Process.Sort;
                check.FirstOrDefault().RoleID = Process.RoleID;
                check.FirstOrDefault().LocationID = Process.LocationID;
                check.FirstOrDefault().canWork = Process.canWork;
                check.FirstOrDefault().canInspect = Process.canInspect;
                check.FirstOrDefault().canPush = Process.canPush;
                check.FirstOrDefault().canReject = Process.canReject;
                check.FirstOrDefault().canReport = Process.canReport;
                check.FirstOrDefault().canSchdule = Process.canSchdule;
                check.FirstOrDefault().canAccept = Process.canAccept;

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

            helpers.LogMessages("Updating application process. Status : " + result + " Application Process ID : " + processID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(result);
        }


        /*
         * Removing application process
         */
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult DeleteProcess(int processID)
        {
            string response = "";

            var check = from s in _context.WorkProccess where s.ProccessID == processID && s.DeleteStatus == false select s;

            if (check.Count() > 0)
            {
                check.FirstOrDefault().DeleteStatus = true;
                check.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));
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

            helpers.LogMessages("Deleting application process. Status : " + response + " Application Process ID : " + processID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }
        #endregion

        #region Application Category
        // GET: Categoriess
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetAppCategorys()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getAppCategory = from r in _context.Categories
                              where r.DeleteStatus != true
                              select new
                              {
                                  CategoryId = r.id,
                                  CategoryName = r.name,
                                  ShortName = r.FriendlyName,
                                  CategoryAmount = r.Price,
                                  ServiceCharge = r.ServiceCharge,
                                  UpdatedAt = r.UpdatedAt.ToString(),
                                  CreatedAt = r.CreatedAt.ToString(),
                              };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getAppCategory = sortColumn == "stageName" ? getAppCategory.OrderByDescending(c => c.CategoryName) :
                               sortColumn == "updatedAt" ? getAppCategory.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "shortName" ? getAppCategory.OrderByDescending(c => c.ShortName) :
                               sortColumn == "stageAmount" ? getAppCategory.OrderByDescending(c => c.CategoryAmount) :
                               sortColumn == "createdAt" ? getAppCategory.OrderByDescending(c => c.CreatedAt) :
                               getAppCategory.OrderByDescending(c => c.CategoryId + " " + sortColumnup);
                }
                else
                {
                    getAppCategory = sortColumn == "stageName" ? getAppCategory.OrderBy(c => c.CategoryName) :
                               sortColumn == "updatedAt" ? getAppCategory.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "shortName" ? getAppCategory.OrderBy(c => c.ShortName) :
                               sortColumn == "stageAmount" ? getAppCategory.OrderBy(c => c.CategoryAmount) :
                               sortColumn == "createdAt" ? getAppCategory.OrderBy(c => c.CreatedAt) :
                               getAppCategory.OrderBy(c => c.CategoryId);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getAppCategory = getAppCategory.Where(c => c.CategoryName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getAppCategory.Count();
            var data = getAppCategory.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying application categories.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });
        }





        // POST: Categoriess/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateAppCategory(string AppCategoryName, string AppShortName, int CategoryAmount, int ServiceCharge)
        {
            string response = "";

            var appstage = from a in _context.Categories
                           where a.name == AppCategoryName.ToUpper() && a.DeleteStatus == false
                           select a;

            if (appstage.Count() > 0)
            {
                response = "Application stage already exits, please enter another stage.";
            }
            else
            {
                Categories con = new Categories()
                {
                    name = AppCategoryName.ToUpper(),
                    FriendlyName = AppShortName.ToUpper(),
                    ServiceCharge = ServiceCharge,
                    Price = CategoryAmount,
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.Categories.Add(con);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "AppCategory Created";
                }
                else
                {
                    response = "Something went wrong trying to create this App Category. Please try again.";
                }
            }

            helpers.LogMessages("Creating application stage. Status : " + response + " Application stage Name : " + AppCategoryName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }





        // POST: Categoriess/Edit/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditAppCategory(int AppCategoryID, string AppCategoryName, string AppShortName, int CategoryAmount, int ServiceCharge)
        {
            string response = "";
            var getAppCategory = from c in _context.Categories where c.id == AppCategoryID select c;

            getAppCategory.FirstOrDefault().name = AppCategoryName.ToUpper();
            getAppCategory.FirstOrDefault().ServiceCharge = ServiceCharge;
            getAppCategory.FirstOrDefault().FriendlyName = AppShortName;
            getAppCategory.FirstOrDefault().Price = CategoryAmount;
            getAppCategory.FirstOrDefault().UpdatedAt = DateTime.Now;
            getAppCategory.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "AppCategory Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating application stage. Status : " + response + " Application Category ID : " + AppCategoryID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        // POST: Categoriess/Delete/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteAppCategory(int AppCategoryID)
        {
            string response = "";

            var getAppCategory = from c in _context.Categories where c.id == AppCategoryID select c;

            getAppCategory.FirstOrDefault().DeletedAt = DateTime.Now;
            getAppCategory.FirstOrDefault().UpdatedAt = DateTime.Now;
            getAppCategory.FirstOrDefault().DeleteStatus = true;
            getAppCategory.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "AppCategory Deleted";
            }
            else
            {
                response = "Application stage not deleted. Something went wrong trying to delete this application stage.";
            }

            helpers.LogMessages("Deleting application stage. Status : " + response + " Application stage ID : " + AppCategoryID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }
        #endregion
        #region Application Phases
        // GET: ApplicationPhases
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> ApplicationPhases()
        {
            return View(await _context.Phases.ToListAsync());
        }

        public JsonResult GetAppPhase()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getAppPhase = from r in _context.Phases
                             where r.DeleteStatus == false
                             select new
                             {
                                 AppPhaseID = r.id,
                                 PhaseName = r.name,
                                 UpdatedAt = r.UpdatedAt.ToString(),
                                 CreatedAt = r.CreatedAt.ToString()
                             };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getAppPhase = sortColumn == "typeName" ? getAppPhase.OrderByDescending(c => c.PhaseName) :
                               sortColumn == "updatedAt" ? getAppPhase.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppPhase.OrderByDescending(c => c.CreatedAt) :
                               getAppPhase.OrderByDescending(c => c.AppPhaseID + " " + sortColumnup);
                }
                else
                {
                    getAppPhase = sortColumn == "typeName" ? getAppPhase.OrderBy(c => c.PhaseName) :
                               sortColumn == "updatedAt" ? getAppPhase.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppPhase.OrderBy(c => c.CreatedAt) :
                               getAppPhase.OrderBy(c => c.AppPhaseID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getAppPhase = getAppPhase.Where(c => c.PhaseName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getAppPhase.Count();
            var data = getAppPhase.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying application types.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }





        // POST: ApplicationPhases/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateAppPhase(string AppPhaseName)
        {
            string response = "";
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            var getAppPhase = from a in _context.Phases
                             where a.name == AppPhaseName.ToUpper() && a.DeleteStatus == false
                             select a;

            if (getAppPhase.Count() > 0)
            {
                response = "Application type already exits, please enter another type.";
            }
            else
            {
                Phases _appPhase = new Phases()
                {
                    name = AppPhaseName.ToUpper(),
                    CreatedAt = DateTime.Now,
                    CreatedBy=userID,
                    DeleteStatus = false
                };

                _context.Phases.Add(_appPhase);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "AppPhase Created";
                }
                else
                {
                    response = "Something went wrong trying to create this App Phase. Please try again.";
                }
            }

            helpers.LogMessages("Creating application types. Status : " + response + " Application type name : " + AppPhaseName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        // POST: ApplicationPhases/Edit/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditAppPhase(int AppPhaseID, string AppPhaseName)
        {
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            string response = "";
            var getAppPhase = from c in _context.Phases where c.id == AppPhaseID select c;

            getAppPhase.FirstOrDefault().name = AppPhaseName.ToUpper();
            getAppPhase.FirstOrDefault().UpdatedAt = DateTime.Now;
            getAppPhase.FirstOrDefault().DeleteStatus = false;
            getAppPhase.FirstOrDefault().UpdatedBy = userID;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "AppPhase Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating application types. Status : " + response + " Application types ID : " + AppPhaseID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }




        // POST: ApplicationPhases/Delete/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteAppPhase(int AppPhaseID)
        {
            string response = "";
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));

            var getAppPhase = from c in _context.Phases where c.id == AppPhaseID select c;

            getAppPhase.FirstOrDefault().DeletedAt = DateTime.Now;
            getAppPhase.FirstOrDefault().UpdatedAt = DateTime.Now;
            getAppPhase.FirstOrDefault().DeleteStatus = true;
            getAppPhase.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "AppPhase Deleted";
            }
            else
            {
                response = "Application type not deleted. Something went wrong trying to delete this application type.";
            }

            helpers.LogMessages("Deleting application type. Status : " + response + " Application type ID : " + AppPhaseID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }
        #endregion


        #region FieldOffices
        // GET: FieldOffices
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> FieldOffices()
        {
            return View(await _context.FieldOffices.ToListAsync());
        }
        public JsonResult GetFieldOffice()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
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
                                     UpdatedAt = c.UpdatedAt.ToString(),
                                     CreatedAt = c.CreatedAt.ToString()
                                 };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getFieldOffice = sortColumn == "officeName" ? getFieldOffice.OrderByDescending(c => c.OfficeName) :
                               sortColumn == "updatedAt" ? getFieldOffice.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getFieldOffice.OrderByDescending(c => c.CreatedAt) :
                               getFieldOffice.OrderByDescending(c => c.FieldOfficeID + " " + sortColumnup);
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

            helpers.LogMessages("Displaying all field officess...", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        // POST: FieldOffices/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateFieldOffice(string OfficeName)
        {
            string response = "";

            var country = from s in _context.FieldOffices
                          where s.OfficeName == OfficeName.ToUpper() && s.DeleteStatus == false
                          select s;

            if (country.Count() > 0)
            {
                response = "Field office already exits, please enter another field office.";
            }
            else
            {
                FieldOffices _fieldOffice = new FieldOffices()
                {
                    OfficeName = OfficeName.ToUpper(),
                    CreatedAt = DateTime.Now,
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

            helpers.LogMessages("Creating new field office. Status : " + response + " field office name : " + OfficeName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);

        }

        // GET: FieldOffices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fieldOffices = await _context.FieldOffices.FindAsync(id);
            if (fieldOffices == null)
            {
                return NotFound();
            }
            return View(fieldOffices);
        }

        /*
         * Edit field Office
         */
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditFieldOffice(string OfficeName, int FieldOfficeId)
        {
            string response = "";

            var getFieldOffice = from c in _context.FieldOffices where c.FieldOffice_id == FieldOfficeId select c;

            getFieldOffice.FirstOrDefault().OfficeName = OfficeName.ToUpper();
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

            helpers.LogMessages("Updating field office. Status : " + response + " field office ID: " + FieldOfficeId, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }


        // GET: FieldOffices/Delete/
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteOffice(int FieldOfficeID)
        {
            string response = "";

            var getState = from c in _context.FieldOffices where c.FieldOffice_id == FieldOfficeID select c;

            getState.FirstOrDefault().DeletedAt = DateTime.Now;
            getState.FirstOrDefault().UpdatedAt = DateTime.Now;
            getState.FirstOrDefault().DeleteStatus = true;
            getState.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Office Deleted";
            }
            else
            {
                response = "Office not deleted. Something went wrong trying to delete this Field Office.";
            }

            helpers.LogMessages("Deleting field office. Status : " + response + " field office ID : " + FieldOfficeID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }


        // POST: FieldOffices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fieldOffices = await _context.FieldOffices.FindAsync(id);
            _context.FieldOffices.Remove(fieldOffices);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FieldOfficesExists(int id)
        {
            return _context.FieldOffices.Any(e => e.FieldOffice_id == id);
        }
        #endregion

        #region Locations
        // GET: Locations
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> Locations()
        {
            return View(await _context.Location.ToListAsync());
        }


        // Get all locations
        public JsonResult GetLocations()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getCountries = from c in _context.Location
                               join s in _context.Staff on c.CreatedBy equals s.StaffID into staff
                               join ss in _context.Staff on c.UpdatedBy equals ss.StaffID into staffs
                               where c.DeleteStatus == false
                               select new
                               {
                                   LocationId = c.LocationID,
                                   LocationName = c.LocationName,
                                   UpdatedAt = c.UpdatedAt.ToString(),
                                   CreatedAt = c.CreatedAt.ToString(),
                                   CreatedBy = staff.FirstOrDefault().FirstName + " " + staff.FirstOrDefault().LastName,
                                   UpdatedBy = staffs.FirstOrDefault().FirstName + " " + staffs.FirstOrDefault().LastName,
                               };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getCountries = sortColumn == "locationName" ? getCountries.OrderByDescending(c => c.LocationName) :
                               sortColumn == "updatedAt" ? getCountries.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getCountries.OrderByDescending(c => c.CreatedAt) :
                               getCountries.OrderByDescending(c => c.LocationId + " " + sortColumnup);
                }
                else
                {
                    getCountries = sortColumn == "locationName" ? getCountries.OrderBy(c => c.LocationName) :
                               sortColumn == "updatedAt" ? getCountries.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getCountries.OrderBy(c => c.CreatedAt) :
                               getCountries.OrderBy(c => c.LocationId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getCountries = getCountries.Where(c => c.LocationName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getCountries.Count();
            var data = getCountries.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying all locations", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        //create Locatioin
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
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
                NewDepot.Models.Location loc = new NewDepot.Models.Location()
                {
                    LocationName = Location.ToUpper(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID"))),
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

            helpers.LogMessages("Creating new location. Status : " + response + " Location name : " + Location, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditLocation(int LocationId, string Location)
        {
            string response = "";
            var get = from c in _context.Location where c.LocationID == LocationId select c;

            get.FirstOrDefault().LocationName = Location.ToUpper();
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));
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

            helpers.LogMessages("Updating Location. Status : " + response + " Location ID : " + LocationId, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }

        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> RemoveLocation(int LocationID)
        {
            string response = "";

            var get = from c in _context.Location where c.LocationID == LocationID select c;

            get.FirstOrDefault().DeletedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedAt = DateTime.Now;
            get.FirstOrDefault().UpdatedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));
            get.FirstOrDefault().DeleteStatus = true;
            get.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Location Removed";
            }
            else
            {
                response = "Location not deleted. Something went wrong trying to delete this Location.";
            }

            helpers.LogMessages("Deleting Location. Status : " + response + " Location ID : " + LocationID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);

        }


        private bool LocationExists(int id)
        {
            return _context.Location.Any(e => e.LocationID == id);
        }
        #endregion

        #region ZonalFieldOffices
        // GET: ZonalFieldOffices
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> ZonalFieldOffices()
        {
            return View(await _context.ZonalFieldOffice.ToListAsync());
        }

        public JsonResult GetZonalFieldOffice()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
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
                                where (zf.DeleteStatus == false && f.DeleteStatus == false && z.DeleteStatus == false && s.DeleteStatus == false && 
                                //c.DeleteStatus == false &&
                                zs.DeleteStatus == false)

                                select new
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
                                })
                               .GroupBy(i => new { i.OfficeName })
                               .Select(i => new
                               {
                                   ZonalFieldOfficeID = i.FirstOrDefault().ZonalFieldOfficeID,
                                   CountryID = i.FirstOrDefault().CountryID,
                                   StateId = i.FirstOrDefault().StateId,
                                   ZoneId = i.FirstOrDefault().ZoneId,
                                   FieldOfficeID = i.FirstOrDefault().FieldOfficeID,
                                   CountryName = i.FirstOrDefault().CountryName,
                                   StateName = i.FirstOrDefault().StateName,
                                   ZoneName = i.FirstOrDefault().ZoneName,
                                   OfficeName = i.FirstOrDefault().OfficeName,
                                   UpdatedAt = i.FirstOrDefault().UpdatedAt.ToString(),
                                   CreatedAt = i.FirstOrDefault().CreatedAt.ToString()
                               });


            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getZoneState = sortColumn == "countryName" ? getZoneState.OrderByDescending(s => s.CountryName) :
                                    sortColumn == "stateName" ? getZoneState.OrderByDescending(s => s.StateName) :
                                    sortColumn == "zoneName" ? getZoneState.OrderByDescending(s => s.ZoneName) :
                                    sortColumn == "officeName" ? getZoneState.OrderByDescending(s => s.OfficeName) :
                                    sortColumn == "updatedAt" ? getZoneState.OrderByDescending(s => s.UpdatedAt) :
                                    sortColumn == "createdAt" ? getZoneState.OrderByDescending(s => s.CreatedAt) :
                                    getZoneState.OrderByDescending(s => s.ZonalFieldOfficeID + " " + sortColumnup);
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

            helpers.LogMessages("Displaying all zonal field office.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        // POST: ZonalFieldOffices/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateZonalFieldOffice(int ZoneID, int FieldOfficeID)
        {
            string response = "";

            var check = from zs in _context.ZonalFieldOffice
                        join f in _context.FieldOffices on zs.FieldOffice_id equals f.FieldOffice_id
                        join z in _context.ZonalOffice on zs.Zone_id equals z.Zone_id
                        where zs.Zone_id == ZoneID && zs.FieldOffice_id == FieldOfficeID && zs.DeleteStatus == false
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
            helpers.LogMessages("Creating new zonal field office. Status : " + response + " zonal office ID : " + ZoneID + " field office ID : " + FieldOfficeID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }

        // POST: ZonalFieldOffices/Edit/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
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

            helpers.LogMessages("Updating zonal field office. Status : " + response + " Zonal Office ID : " + ZoneID + " field office ID : " + FieldOfficeID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        // POST: ZonalFieldOffices/Delete/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteZonalFieldOffice(int ZonalFieldOfficeID)
        {
            string response = "";

            var getZonalFieldOffice = from c in _context.ZonalFieldOffice where c.FieldOffice_id == ZonalFieldOfficeID select c;

            getZonalFieldOffice.FirstOrDefault().DeletedAt = DateTime.Now;
            getZonalFieldOffice.FirstOrDefault().UpdatedAt = DateTime.Now;
            getZonalFieldOffice.FirstOrDefault().DeleteStatus = true;
            getZonalFieldOffice.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "ZonalFieldOffice Deleted";
            }
            else
            {
                response = "Zone => Field Office not deleted. Something went wrong trying to delete this entry.";
            }

            helpers.LogMessages("Deleting zonal field office. Status : " + response + " Zonal field Office ID : " + ZonalFieldOfficeID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }

        private bool ZonalFieldOfficeExists(int id)
        {
            return _context.ZonalFieldOffice.Any(e => e.ZoneFieldOffice_id == id);
        }

        #endregion
        #region States
        // GET: States
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult AllStates()
        {
            return View();
        }

        public JsonResult GetStates()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getStates = from s in _context.States_UT
                            join c in _context.countries on s.Country_id equals c.id
                            where s.DeleteStatus == false //&& c.DeleteStatus == false
                            select new
                            {
                                CountryId = c.id,
                                StateId = s.State_id,
                                CountryName = c.name,
                                StateName = s.StateName,
                                UpdatedAt = s.UpdatedAt.ToString(),
                                CreatedAt = s.CreatedAt.ToString()
                            };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getStates = sortColumn == "countryName" ? getStates.OrderByDescending(s => s.CountryName) :
                               sortColumn == "stateName" ? getStates.OrderByDescending(s => s.StateName) :
                               sortColumn == "updatedAt" ? getStates.OrderByDescending(s => s.UpdatedAt) :
                               sortColumn == "createdAt" ? getStates.OrderByDescending(s => s.CreatedAt) :
                               getStates.OrderByDescending(s => s.StateId + " " + sortColumnup);
                }
                else
                {
                    getStates = sortColumn == "countryName" ? getStates.OrderBy(c => c.CountryName) :
                                sortColumn == "stateName" ? getStates.OrderBy(s => s.StateName) :
                               sortColumn == "updatedAt" ? getStates.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getStates.OrderBy(c => c.CreatedAt) :
                               getStates.OrderBy(c => c.StateId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getStates = getStates.Where(c => c.CountryName.Contains(txtSearch.ToUpper()) || c.StateName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getStates.Count();
            var data = getStates.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying all States", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));


            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        // POST: States/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateState(int CountryID, string StateName)
        {
            string response = "";

            var country = from s in _context.States_UT
                          where s.Country_id == CountryID && s.StateName == StateName.ToUpper() && s.DeleteStatus == false
                          select s;

            if (country.Count() > 0)
            {
                response = "State already exits, please enter another state.";
            }
            else
            {
                States_UT states = new States_UT()
                {
                    Country_id = CountryID,
                    StateName = StateName.ToUpper(),
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.States_UT.Add(states);
                int StateCreated = await _context.SaveChangesAsync();

                if (StateCreated > 0)
                {
                    response = "State Created";
                }
                else
                {
                    response = "Something went wrong trying to create state. Please try again.";
                }
            }
            helpers.LogMessages("Creating new State. Status : " + response + " Country ID : " + CountryID + "State Name : " + StateName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }

        // GET: States/Edit/5
        public async Task<IActionResult> EditState(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var states = await _context.States_UT.FindAsync(id);
            if (states == null)
            {
                return NotFound();
            }
            return View(states);
        }

        /*
         * edit state and country
         */
        [HttpPost]
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditState(string StateName, int StateID, int CountryID)
        {
            string response = "";
            var getState = from x in _context.States_UT where x.State_id == StateID select x;

            getState.FirstOrDefault().StateName = StateName.ToUpper();
            getState.FirstOrDefault().Country_id = CountryID;
            getState.FirstOrDefault().UpdatedAt = DateTime.Now;
            getState.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "State Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating State. Status : " + response + " State ID : " + StateID + " Country ID : " + CountryID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));


            return Json(response);
        }



        // Removing a state
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteState(int StateID)
        {
            string response = "";

            var getState = from c in _context.States_UT where c.State_id == StateID select c;

            getState.FirstOrDefault().DeletedAt = DateTime.Now;
            getState.FirstOrDefault().UpdatedAt = DateTime.Now;
            getState.FirstOrDefault().DeleteStatus = true;
            getState.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "State Deleted";
            }
            else
            {
                response = "State not deleted. Something went wrong trying to delete this state.";
            }

            helpers.LogMessages("Deleting State. Status : " + response + " State ID : " + StateID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }



        private bool StatesExists(int id)
        {
            return _context.States_UT.Any(e => e.State_id == id);
        }
        #endregion
        //#region Application Phase With Category
        //// GET: AppTypeWithStage
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //public IActionResult Index()
        //{
        //    return View();
        //}


        //public JsonResult GetTypeStage()
        //{
        //    var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
        //    var start = HttpContext.Request.Form["start"].FirstOrDefault();
        //    var length = HttpContext.Request.Form["length"].FirstOrDefault();
        //    var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
        //    var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
        //    var txtSearch = HttpContext.Request.Form["search[value]"][0];

        //    int pageSize = length != null ? Convert.ToInt32(length) : 0;
        //    int skip = start != null ? Convert.ToInt32(start) : 0;
        //    int totalRecords = 0;

        //    var get = from ts in _context.AppTypeStage
        //              join s in _context.ApplicationStage on ts.AppStageId equals s.AppStageId
        //              join t in _context.ApplicationType on ts.AppTypeId equals t.AppTypeId
        //              where ts.DeleteStatus == false && s.DeleteStatus == false && t.DeleteStatus == false
        //              select new
        //              {
        //                  TypeStageID = ts.TypeStageId,
        //                  TypeID = t.AppTypeId,
        //                  TypeName = t.TypeName,
        //                  StageID = s.AppStageId,
        //                  Counter = ts.Counter,
        //                  StageName = s.StageName,
        //                  ShortName = s.ShortName,
        //                  ServiceCharge = s.ServiceCharge,
        //                  StageAmount = s.Amount,
        //                  UpdatedAt = ts.UpdatedAt.ToString(),
        //                  CreatedAt = ts.CreatedAt.ToString()
        //              };

        //    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
        //    {
        //        if (sortColumnup == "desc")
        //        {
        //            get = sortColumn == "stageName" ? get.OrderByDescending(s => s.StageName) :
        //                            sortColumn == "typeName" ? get.OrderByDescending(t => t.TypeName) :
        //                            sortColumn == "updatedAt" ? get.OrderByDescending(s => s.UpdatedAt) :
        //                            sortColumn == "counter" ? get.OrderByDescending(s => s.Counter) :
        //                            sortColumn == "shortName" ? get.OrderByDescending(s => s.ShortName) :
        //                            sortColumn == "stageAmount" ? get.OrderByDescending(s => s.StageAmount) :

        //                            sortColumn == "createdAt" ? get.OrderByDescending(s => s.CreatedAt) :
        //                            get.OrderByDescending(ts => ts.TypeStageID + " " + sortColumnup);
        //        }
        //        else
        //        {
        //            get = sortColumn == "stageName" ? get.OrderBy(s => s.StageName) :
        //                           sortColumn == "typeName" ? get.OrderBy(t => t.TypeName) :
        //                           sortColumn == "counter" ? get.OrderBy(t => t.Counter) :
        //                           sortColumn == "shortName" ? get.OrderBy(t => t.ShortName) :
        //                           sortColumn == "stageAmount" ? get.OrderBy(t => t.StageAmount) :
        //                           sortColumn == "updatedAt" ? get.OrderBy(c => c.UpdatedAt) :
        //                           sortColumn == "createdAt" ? get.OrderBy(c => c.CreatedAt) :
        //                           get.OrderBy(ts => ts.TypeStageID);
        //        }
        //    }

        //    if (!string.IsNullOrWhiteSpace(txtSearch))
        //    {
        //        get = get.Where(c => c.StageName.Contains(txtSearch.ToUpper()) || c.TypeName.Contains(txtSearch.ToUpper()));
        //    }

        //    totalRecords = get.Count();
        //    var data = get.Skip(skip).Take(pageSize).ToList();

        //    helpers.LogMessages("Displaying all application stage and type", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));


        //    return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        //}


        //// Creating Type and Stage 
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //public async Task<IActionResult> CreateTypeStage(int TypeID, int StageID, int Counter)
        //{
        //    string response = "";

        //    var check = from ts in _context.AppTypeStage
        //                join t in _context.ApplicationType on ts.AppTypeId equals t.AppTypeId
        //                join s in _context.ApplicationStage on ts.AppStageId equals s.AppStageId
        //                where ts.AppTypeId == TypeID && ts.AppStageId == StageID && ts.DeleteStatus == false
        //                select new
        //                {
        //                    s.StageName,
        //                    t.TypeName
        //                };

        //    if (check.Count() > 0)
        //    {
        //        response = check.FirstOrDefault().StageName + " and " + check.FirstOrDefault().TypeName + " relationship already exits.";
        //    }
        //    else
        //    {
        //        AppTypeStage _typeStage = new AppTypeStage()
        //        {
        //            AppTypeId = TypeID,
        //            AppStageId = StageID,
        //            Counter = Counter,
        //            CreatedAt = DateTime.Now,
        //            DeleteStatus = false
        //        };

        //        _context.AppTypeStage.Add(_typeStage);
        //        int Created = await _context.SaveChangesAsync();

        //        if (Created > 0)
        //        {
        //            response = "TypeState Created";
        //        }
        //        else
        //        {
        //            response = "Something went wrong trying to create Type and Stage relationship. Please try again.";
        //        }
        //    }

        //    helpers.LogMessages("Creating new application stage and type. Status : " + response + " Application type ID : " + TypeID + "Application stage ID : " + StageID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

        //    return Json(response);
        //}


        //// Eidt Type Stage 
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //public async Task<IActionResult> EditTypeStage(int TypeStageID, int TypeID, int StageID, int Counter)
        //{
        //    string response = "";
        //    var check = from x in _context.AppTypeStage where x.TypeStageId == TypeStageID select x;

        //    if (check.FirstOrDefault().AppTypeId == TypeID && check.FirstOrDefault().AppStageId == StageID && check.FirstOrDefault().Counter == Counter)
        //    {
        //        response = "This relationship already exits. Try a different one.";
        //    }
        //    else
        //    {
        //        check.FirstOrDefault().AppTypeId = TypeID;
        //        check.FirstOrDefault().AppStageId = StageID;
        //        check.FirstOrDefault().Counter = Counter;
        //        check.FirstOrDefault().UpdatedAt = DateTime.Now;
        //        check.FirstOrDefault().DeleteStatus = false;

        //        int updated = await _context.SaveChangesAsync();

        //        if (updated > 0)
        //        {
        //            response = "TypeStage Updated";
        //        }
        //        else
        //        {
        //            response = "Nothing was updated.";
        //        }
        //    }

        //    helpers.LogMessages("Updating application stage and type. Status : " + response + " Application type ID : " + TypeID + "Application stage ID : " + StageID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

        //    return Json(response);
        //}


        //// Delete Type Stage
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //public async Task<IActionResult> DeleteTypeStage(int TypeStageID)
        //{
        //    string response = "";

        //    var get = from c in _context.AppTypeStage where c.TypeStageId == TypeStageID select c;

        //    get.FirstOrDefault().DeletedAt = DateTime.Now;
        //    get.FirstOrDefault().UpdatedAt = DateTime.Now;
        //    get.FirstOrDefault().DeleteStatus = true;
        //    get.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

        //    int updated = await _context.SaveChangesAsync();

        //    if (updated > 0)
        //    {
        //        response = "TypeStage Deleted";
        //    }
        //    else
        //    {
        //        response = "Type => Stage not deleted. Something went wrong trying to delete this entry.";
        //    }

        //    helpers.LogMessages("Deleting application stage and type. Status : " + response + " Application typeStage ID : " + TypeStageID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

        //    return Json(response);
        //}

        //#endregion



        //#region Application Phase Documents
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //// GET: AppStageDocuments
        //public IActionResult Index()
        //{
        //    return View();
        //}
        //public JsonResult GetStageDoc()
        //{
        //    var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
        //    var start = HttpContext.Request.Form["start"].FirstOrDefault();
        //    var length = HttpContext.Request.Form["length"].FirstOrDefault();
        //    var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
        //    var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
        //    var txtSearch = HttpContext.Request.Form["search[value]"][0];

        //    int pageSize = length != null ? Convert.ToInt32(length) : 0;
        //    int skip = start != null ? Convert.ToInt32(start) : 0;
        //    int totalRecords = 0;

        //    var get = from ts in _context.AppStageDocuments
        //              join s in _context.ApplicationStage on ts.AppStageId equals s.AppStageId
        //              join d in _context.ApplicationDocuments on ts.AppDocId equals d.AppDocId
        //              where ts.DeleteStatus == false && s.DeleteStatus == false && d.DeleteStatus == false
        //              select new
        //              {
        //                  StageDocID = ts.StageDocId,
        //                  DocID = d.AppDocId,
        //                  DocName = d.DocName,
        //                  StageID = s.AppStageId,
        //                  StageName = s.StageName,
        //                  DocType = d.DocType,
        //                  UpdatedAt = ts.UpdatedAt.ToString(),
        //                  CreatedAt = ts.CreatedAt.ToString()
        //              };

        //    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
        //    {
        //        if (sortColumnup == "desc")
        //        {
        //            get = sortColumn == "stageName" ? get.OrderByDescending(s => s.StageName) :
        //                            sortColumn == "docName" ? get.OrderByDescending(t => t.DocName) :
        //                            sortColumn == "docType" ? get.OrderByDescending(t => t.DocType) :
        //                            sortColumn == "updatedAt" ? get.OrderByDescending(s => s.UpdatedAt) :
        //                            sortColumn == "createdAt" ? get.OrderByDescending(s => s.CreatedAt) :
        //                            get.OrderByDescending(ts => ts.StageDocID + " " + sortColumnup);
        //        }
        //        else
        //        {
        //            get = sortColumn == "stageName" ? get.OrderBy(s => s.StageName) :
        //                           sortColumn == "docName" ? get.OrderBy(t => t.DocName) :
        //                           sortColumn == "docType" ? get.OrderBy(t => t.DocType) :
        //                           sortColumn == "updatedAt" ? get.OrderBy(c => c.UpdatedAt) :
        //                           sortColumn == "createdAt" ? get.OrderBy(c => c.CreatedAt) :
        //                           get.OrderBy(ts => ts.StageDocID);
        //        }
        //    }

        //    if (!string.IsNullOrWhiteSpace(txtSearch))
        //    {
        //        get = get.Where(c => c.StageName.Contains(txtSearch.ToUpper()) || c.DocName.Contains(txtSearch.ToUpper()));
        //    }

        //    totalRecords = get.Count();
        //    var data = get.Skip(skip).Take(pageSize).ToList();

        //    helpers.LogMessages("Displaying all application stage documents.", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

        //    return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        //}
        //// Creating Type and Stage 
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //public async Task<IActionResult> CreateStageDocuments(int DocID, int StageID)
        //{
        //    string response = "";

        //    var check = from ts in _context.AppStageDocuments
        //                join t in _context.ApplicationDocuments on ts.AppDocId equals t.AppDocId
        //                join s in _context.ApplicationStage on ts.AppStageId equals s.AppStageId
        //                where ts.AppDocId == DocID && ts.AppStageId == StageID && ts.DeleteStatus == false
        //                select new
        //                {
        //                    s.StageName,
        //                    t.DocName
        //                };

        //    if (check.Count() > 0)
        //    {
        //        response = check.FirstOrDefault().StageName + " and " + check.FirstOrDefault().DocName + " relationship already exits.";
        //    }
        //    else
        //    {
        //        AppStageDocuments _stageDoc = new AppStageDocuments()
        //        {
        //            AppDocId = DocID,
        //            AppStageId = StageID,
        //            CreatedAt = DateTime.Now,
        //            DeleteStatus = false
        //        };

        //        _context.AppStageDocuments.Add(_stageDoc);
        //        int Created = await _context.SaveChangesAsync();

        //        if (Created > 0)
        //        {
        //            response = "StageDoc Created";
        //        }
        //        else
        //        {
        //            response = "Something went wrong trying to create Document and Stage relationship. Please try again.";
        //        }
        //    }

        //    helpers.LogMessages("Creating new application stage documents. Status : " + response + " Application Document ID : " + DocID + "Application stage ID : " + StageID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

        //    return Json(response);
        //}


        //// Eidt Type Stage 
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //public async Task<IActionResult> EditStageDocuments(int StageDocID, int DocID, int StageID)
        //{
        //    string response = "";
        //    var check = from x in _context.AppStageDocuments where x.StageDocId == StageDocID select x;

        //    if (check.FirstOrDefault().AppDocId == DocID && check.FirstOrDefault().AppStageId == StageID)
        //    {
        //        response = "This relationship already exits. Try a different one.";
        //    }
        //    else
        //    {
        //        check.FirstOrDefault().AppDocId = DocID;
        //        check.FirstOrDefault().AppStageId = StageID;
        //        check.FirstOrDefault().UpdatedAt = DateTime.Now;
        //        check.FirstOrDefault().DeleteStatus = false;

        //        int updated = await _context.SaveChangesAsync();

        //        if (updated > 0)
        //        {
        //            response = "StageDoc Updated";
        //        }
        //        else
        //        {
        //            response = "Nothing was updated.";
        //        }
        //    }
        //    helpers.LogMessages("Updating application stage documents. Status : " + response + " Application Document ID : " + DocID + "Application stage ID : " + StageID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

        //    return Json(response);
        //}


        //// Delete Type Stage
        ////[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        //public async Task<IActionResult> DeleteStageDocument(int StageDocID)
        //{
        //    string response = "";

        //    var get = from c in _context.AppStageDocuments where c.StageDocId == StageDocID select c;

        //    get.FirstOrDefault().DeletedAt = DateTime.Now;
        //    get.FirstOrDefault().UpdatedAt = DateTime.Now;
        //    get.FirstOrDefault().DeleteStatus = true;
        //    get.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

        //    int updated = await _context.SaveChangesAsync();

        //    if (updated > 0)
        //    {
        //        response = "StageDoc Deleted";
        //    }
        //    else
        //    {
        //        response = "Doc => Stage not deleted. Something went wrong trying to delete this entry.";
        //    }

        //    helpers.LogMessages("Deleting application stage documents. Status : " + response + " Application Stage Document ID : " + StageDocID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

        //    return Json(response);
        //}

        //#endregion
    }
}





