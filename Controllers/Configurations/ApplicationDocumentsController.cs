using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewDepot.Helpers;
using NewDepot.Controllers.Configurations;
using NewDepot.Models;

namespace NewDepot.Controllers
{
    //[Authorize]
    public class ApplicationDocumentsController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public ApplicationDocumentsController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



        // GET: ApplicationDocuments
        ////[Authorize(Policy = "ConfigurationRoles")]
        public IActionResult Index()
        {
            var cat = _context.Phases.Where(p => p.DeleteStatus != true).ToList();
            ViewBag.Phases = cat;
            return View();
        }



        ////[Authorize(Policy = "ConfigurationRoles")]
        public JsonResult GetAppDoc()
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

            var getAppDoc = (from r in _context.ApplicationDocuments
                             where r.DeleteStatus != true
                             select new
                             {
                                 AppDocID = r.AppDocID,
                                 AppDocElpsID = r.ElpsDocTypeID,
                                 PhaseName = _context.Phases.Where(c => c.id == r.PhaseId).FirstOrDefault() == null ? "Not Applicable" : _context.Phases.Where(c => c.id == r.PhaseId).FirstOrDefault().name,
                                 DocName = r.DocName,
                                 DocType = r.docType,
                                 UpdatedAt = r.UpdatedAt.ToString(),
                                 CreatedAt = r.CreatedAt.ToString()
                             });/*.GroupBy(x=> x.PhaseName).ToList()*/

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getAppDoc = sortColumn == "docName" ? getAppDoc.OrderByDescending(c => c.DocName) :
                                sortColumn == "phaseName" ? getAppDoc.OrderByDescending(c => c.PhaseName) :
                               sortColumn == "updatedAt" ? getAppDoc.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppDoc.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "docType" ? getAppDoc.OrderByDescending(c => c.DocType) :
                               getAppDoc.OrderByDescending(c => c.AppDocID + " " + sortColumnDir);
                }
                else
                {
                    getAppDoc = sortColumn == "docName" ? getAppDoc.OrderBy(c => c.DocName) :
                               sortColumn == "phaseName" ? getAppDoc.OrderByDescending(c => c.PhaseName) :

                               sortColumn == "updatedAt" ? getAppDoc.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getAppDoc.OrderBy(c => c.CreatedAt) :
                               sortColumn == "docType" ? getAppDoc.OrderBy(c => c.DocType) :
                               getAppDoc.OrderBy(c => c.AppDocID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getAppDoc = getAppDoc.Where(c => c.PhaseName.Contains(txtSearch.ToUpper()) ||c.DocName.Contains(txtSearch.ToUpper()) || c.DocType.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getAppDoc.Count();
            var data = getAppDoc.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying application documents", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        ////[Authorize(Policy = "ConfigurationRoles")]
        // POST: ApplicationDocuments/Create
        public async Task<IActionResult> CreateAppDoc(int AppDocElpsID, string AppDocName, string AppDocType, string PhaseName)
        {
            string response = "";


            var getPhase = (from a in _context.Phases
                             where a.name == PhaseName || a.id.ToString() == PhaseName && a.DeleteStatus != true
                             select a).FirstOrDefault();

            var appstage = from a in _context.ApplicationDocuments
                           where (a.DocName == AppDocName.ToUpper() && a.ElpsDocTypeID == AppDocElpsID && a.docType == AppDocType && a.DeleteStatus != true)
                           select a;

            if (getPhase != null)
            {
                appstage = from a in _context.ApplicationDocuments
                               where (a.PhaseId == getPhase.id && a.DocName == AppDocName.ToUpper() && a.ElpsDocTypeID == AppDocElpsID && a.docType == AppDocType && a.DeleteStatus != true)
                               select a;
            }
            if (appstage.Count() > 0)
            {
                response = getPhase != null ? "Application document name already exist for this phase, kindly select another document.": "Application document name already exist, kindly enter another name.";
            }
            else
            {
                ApplicationDocuments con = new ApplicationDocuments()
                {
                    DocName = AppDocName.ToUpper(),
                    docType = AppDocType,
                    PhaseId = getPhase != null ? getPhase.id: 0,
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

            _helpersController.LogMessages("Creating application documents. Status : " + response, _helpersController.getSessionEmail());

            return Json(response);
        }


        ////[Authorize(Policy = "ConfigurationRoles")]
        // POST: ApplicationDocuments/Edit/5
        public async Task<IActionResult> EditAppDoc(int AppDocID, string AppDocName, string AppDocType, string PhaseName)
        {
            string response = "";

            var getPhase = (from a in _context.Phases
                            where a.name == PhaseName || a.id.ToString() == PhaseName && a.DeleteStatus != true
                            select a).FirstOrDefault();

            var getAppDoc = from c in _context.ApplicationDocuments where c.AppDocID == AppDocID select c;

            getAppDoc.FirstOrDefault().DocName = AppDocName.ToUpper();
            getAppDoc.FirstOrDefault().PhaseId = getPhase!=null? getPhase.id: 0;
            getAppDoc.FirstOrDefault().docType = AppDocType;
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

            _helpersController.LogMessages("Updating application documents. Status : " + response + " Application Document ID : " + AppDocID, _helpersController.getSessionEmail());
            return Json(response);
        }



        ////[Authorize(Policy = "ConfigurationRoles")]
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

            _helpersController.LogMessages("Deleting application documents. Status : " + response + " Application Document ID : " + AppDocID, _helpersController.getSessionEmail());

            return Json(response);
        }

        private bool ApplicationDocumentsExists(int id)
        {
            return _context.ApplicationDocuments.Any(e => e.AppDocID == id);
        }
    }
}
