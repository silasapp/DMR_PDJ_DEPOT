using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NewDepot.Models;
using NewDepot.Helpers;
using System.IO;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
//using NewDepot.Payments;

using System.Transactions;
using Rotativa;
using System.Text;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using NewDepot.Controllers;
using Microsoft.AspNetCore.Http;
using LpgLicense.Models;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Controllers.Authentications;
using static NewDepot.Models.Payment;
using Microsoft.AspNetCore.Hosting.Server;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Options;
using Application = NewDepot.Models.applications;
using Microsoft.EntityFrameworkCore;
using RemitaResponse = NewDepot.Models.Payment.RemitaResponse;
using NewDepot.ViewModels;
using ApplicationHistory = NewDepot.Models.application_desk_histories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NewDepot.Controllers
{
    [Authorize]
    public class CompanyDocumentController : Controller
    {
        SubmittedDocuments _appDocRep;
        RestSharpServices _restService = new RestSharpServices();
        public IConfiguration _configuration;
        IHttpContextAccessor _httpContextAccessor;
        private readonly Depot_DBContext _context;

        ElpsResponse elpsResponse = new ElpsResponse();
        ApplicationHelper appHelper;
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;

        public CompanyDocumentController(
            Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

            //newly added

        }

        public int otherId { get { return Convert.ToInt16(_configuration.GetSection("AmountSetting").GetSection("otherDocId").Value.ToString()); } }


        // GET: CompanyDocument
        [Route("CompanyDocument/{id:int?}")]
        public ActionResult Index(int? id, string docUrl, string docHeader)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            if (id != null && id > 0)
            {
                var doc = _helpersController.GetCompanyDocument(id.GetValueOrDefault().ToString());
                if (doc == null)
                {
                    TempData["status"] = "fail";
                    TempData["message"] = "Document you are looking for does not exist. Please try again";
                    return View("Error");
                }
                if (doc != null && doc.Source.ToLower().EndsWith(".pdf"))
                {
                    doc.Source = doc.Source.StartsWith("http") ? doc.Source:_configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString() + doc.Source;
                    return Redirect(doc.Source);
                    //return File(compDoc.Source, "application/pdf");
                }
                doc.Source = doc.Source.StartsWith("http") ? doc.Source:_configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString() + doc.Source;
                return View("DocDetails", doc);
            }
            else if (!string.IsNullOrEmpty(docUrl) && !string.IsNullOrEmpty(docHeader))
            {
                ViewBag.DocUrl = docUrl;
                ViewBag.DocHeader = docHeader;
                return View("DocDetails");
            }

            var comp = _context.companies.Where(a => a.CompanyEmail == userEmail).FirstOrDefault();
            if (comp != null)
            {
                ViewBag.CompanyName = comp.name;
                var compDocs = _helpersController.GetCompanyDocuments(comp.elps_id.GetValueOrDefault().ToString());
                if(compDocs == null || compDocs.Count() <= 0)
                {
                    //Nothing was returned from ELPS, Look inside Local
                   var subDocs = _context.SubmittedDocuments.Where(a => a.CompElpsDocID == comp.elps_id && !a.DeletedStatus).ToList();
                    return View(subDocs);
                }
                else
                {
                    foreach (var d in compDocs)
                    {
                        d.Source = d.Source.StartsWith("http") ? d.Source :_configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value + d.Source;
                    }
                }

                //var compDocs = _context.company_documents.Where(a => a.CompanyId == comp.Id && !a.Archived).ToList();
                return View(compDocs);
            }
            else
            {
                TempData["status"] = "fail";
                TempData["message"] = "Please Login with your Company Credentials to Continue";
                return View(new List<company_documents>());
            }
        }

        //public FileResult DisplayPDFDocument(int docId, string docUrl)
        public ActionResult DisplayPDFDocument(int docId, string docUrl)
        {
            if (!string.IsNullOrEmpty(docUrl))
            {
                //return File(docUrl, "application/pdf");
                if (!docUrl.StartsWith("http"))
                {
                    docUrl=_configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value + docUrl;
                }
                return Redirect(docUrl);
            }
            else
            {

                var compDoc = _helpersController.GetCompanyDocument(docId.ToString());
                if (compDoc != null && compDoc.Source.ToLower().EndsWith(".pdf"))
                {
                    compDoc.Source = compDoc.Source = compDoc.Source.StartsWith("http") ? compDoc.Source :_configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value + compDoc.Source;
                    return Redirect(compDoc.Source);
                }
                return null;
            }
        }

        public ActionResult Reload(int id, int appId, string tag)
        {
            ViewBag.Target = tag;
            var app = _context.applications.Where(a => a.id == appId).FirstOrDefault();
            var coy = _context.companies.Where(a => a.id == app.company_id).FirstOrDefault();
            var company = _helpersController.GetCompanyDocument(id.ToString());
            if (company != null)
            {
                ViewBag.ApplicationId = appId;
                if(company.Document_Type_Id == otherId)
                {
                    company.Document_Name = company.Document_Name;
                }
                return View(company);
            }
            else
            {
                ViewBag.FileMessage = "Cannot find the specified Document";
                return View();
            }
        }

        public ActionResult DocumentUpload(int applicationId)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var comp = _context.companies.Where(a => a.id == userID).FirstOrDefault();
            if (comp != null)
            {
                var docs = _context.document_types.ToList();
                ViewBag.Documents = new SelectList(docs, "Id", "Name");
                ViewBag.ApplicationId = applicationId;
                ViewBag.CompanyName = comp.name;
                ViewBag.CompanyId = comp.elps_id.GetValueOrDefault();
                var compDocs = _helpersController.GetCompanyDocuments(comp.elps_id.GetValueOrDefault().ToString());
                return View(compDocs);
            }
            else
            {
                TempData["status"] = "fail";
                TempData["message"] = "Please Login with your Company Credentials to Continue";
                return View(new List<company_documents>());
            }
        }

        public ActionResult ExtraDoc(string id, string compId, string index)
        {
            var docs = _context.document_types.ToList();
            ViewBag.Documents = new SelectList(docs, "Id", "Name");

            ViewBag.CompanyId = Convert.ToInt16(compId.ToString());
            ViewBag.ApplicationId = id;
            ViewBag.Index = Convert.ToInt16(index) + 1;
            return View();
        }


        //[HttpPost] Adeola
        //public ActionResult UploadFile(int appid, int docTypId, int compId, string docName, string uid, IEnumerable<IFormFile> docs)//, string attach, string remove)
        //{
        //    var comp = _context.companies.Where(c => c.id == compId).FirstOrDefault();
        //    if (comp == null)
        //    {
        //        return Json(0);
        //    }
        //    else
        //    {
        //        docName = (docName.ToLower() == "undefined" ? "" : docName);
        //        var fileId = _helpersController.UploadCompDoc(docs.FirstOrDefault(), comp.name, docTypId, comp.id, userEmail, Request.Host.Value.ToString(), docName, uid);

        //        string result = string.Empty;

        //        #region Update Application Document
        //        var checkDoc = new application_documents();
        //        if (docTypId != otherId)
        //            checkDoc = _context.application_documents.Where(a => a.document_type_id == docTypId && a.application_id == appid).FirstOrDefault();
        //        else
        //            checkDoc = _context.application_documents.Where(a => a.document_type_id == docTypId && a.application_id == appid && a.UniqueId == uid).FirstOrDefault();
        //        //Edit App Doc if already existing

        //        if (checkDoc != null)
        //        {
        //            checkDoc.document_id = fileId;
        //            _context.SaveChanges();
        //        }
                
        //        #endregion

        //        return Json(docs.Select(x => new { name = x.FileName, fileid = fileId, msg = result }));
        //    }
        //}

        private bool AttachDocuments(int appid, int doctypid, int docid)
        {
            application_documents appDoc = new application_documents();
            appDoc.application_id = appid;
            appDoc.document_id = docid;
            appDoc.document_type_id = doctypid;

            _context.application_documents.Add(appDoc);
            _context.SaveChanges();

            return true;
        }

        // //[Authorize(Roles = "Dept_Manager,Admin,Dept_Checker,Staff,Dept_Approver")]
        public ActionResult Docs(int Id, string view)
        {
            var comp = _context.companies.Where(a => a.id == Id).FirstOrDefault();
            ViewBag.CompanyName = comp.name;
            //var compDocs = _context.company_documents.Where(a => a.CompanyId == Id).ToList();

            var compDocs = _helpersController.GetCompanyDocuments(comp.elps_id.GetValueOrDefault().ToString());
            if (compDocs == null || compDocs.Count() <= 0)
            {
                //Nothing was returned from ELPS, Look inside Local
               var submittedDocs = _context.SubmittedDocuments.Where(a => a.CompElpsDocID == comp.elps_id && a.DeletedStatus!=true).ToList();
                return View(submittedDocs);
            }
            else
            {
                foreach (var d in compDocs)
                {
                    d.Source = d.Source.StartsWith("http")?d.Source:_configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value + d.Source;
                }
            }

            if (!string.IsNullOrEmpty(view))
                return View(view, compDocs);
            else
                return View(compDocs);
        }

        public ActionResult Delete(int id)
        {
            var doc = _context.company_documents.Where(a => a.id == id).FirstOrDefault();
            if (doc != null)
            {
                return PartialView(doc);
            }
            //lk
            return View("Error");
        }

        //[HttpPost]
        //public ActionResult Delete(company_documents model)
        //{
        //    // Deleting a file should archive the file and not delete
        //    var file = _context.company_documents.Where(a => a.id == model.id).FirstOrDefault();
        //    if (file != null)
        //    {
        //        file.archived = true;
        //        _context.SaveChanges();
        //    }

        //    return RedirectToAction("index", new { id = 0 });
        //}

        //public ActionResult TotalDelete(int id, bool facDoc = false)
        //{
        //    var compDoc = new company_documents();
        //    if (facDoc)
        //    {
        //        compDoc = _helpersController.getFacilityDocument(id);
        //    }
        //    else
        //    {
        //        compDoc = _helpersController.GetCompanyDocument(id);
        //        if (compDoc != null)
        //        {
        //            return PartialView(compDoc);
        //        }
        //        return View("Error");
        //    }
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult TotalDelete(string toDelId, string DocType=null)
        //{
        //    var deleteURL = "";

        //    if (DocType == "Company")
        //    {
        //        deleteURL = "CompanyDocument";
        //    }
        //    else
        //    {
        //        deleteURL = "FacilityDocument";
        //    }

        //    var paramData = _restService.parameterData("Id", toDelId);
        //    var result = generalClass.RestResult(deleteURL + "/Delete/{Id}", "DELETE", paramData, null, "Document Deleted", null); // DELETE

        //    //del local
        //    var subdocs = _context.SubmittedDocuments.Where(x => x.CompElpsDocID == Convert.ToInt32(toDelId)).FirstOrDefault();
        //    var appdocs = _context.application_documents.Where(x => x.document_id== Convert.ToInt32(toDelId)).FirstOrDefault();

        //    if (result.Value.ToString() != "Network Error") {
        //        if (subdocs != null)
        //        {
        //            _context.SubmittedDocuments.Remove(subdocs);
        //            _context.SaveChanges();
        //        }
        //        if (appdocs != null)
        //        {
        //            _context.application_documents.Remove(appdocs);
        //            _context.SaveChanges();
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Errorr", "Home", new { message = generalClass.Encrypt("Sorry, a network error has occured. Please try again.") });

        //    }

        //    return Json(result.Value);

        //}

    }
}