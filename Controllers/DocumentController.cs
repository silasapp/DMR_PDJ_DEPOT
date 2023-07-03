//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using NewDepot.Models;
//using NMDPRA_Depot.Domain.Abstract;
//using NewDepot.Helpers;

//namespace NewDepot.Controllers
//{
//    public class DocumentController : Controller
//    {
//        //
//        // GET: /Document/

//        IDocument_TypeRepository _documentTypeRep;

//        ELPSAPIHelper elpsHelper;

//        public DocumentController(IDocument_TypeRepository documentType)
//        {
//            _documentTypeRep = documentType;

//            elpsHelper = new ELPSAPIHelper();
//        }

//        public ActionResult Index()
//        {
//            List<Document_Type> documents = elpsHelper.GetDocumentTypes(); // _documentTypeRep.GetAll().ToList();
//            if (TempData["Message"] != null)
//            {
//                ViewBag.Message = TempData["Message"].ToString();
//                ViewBag.MsgType = TempData["status"].ToString();
//            }
//            return View(documents);
//        }

//        public ActionResult Create(int? id)
//        {
//            Document_Type model = new Document_Type();
//            if (id != null)
//            {
//                //Editing the Document
//                model = _documentTypeRep.FindBy(d => d.Id == id).FirstOrDefault();
                
//            }
//            return View(model);
//        }

//        [HttpPost]
//        public ActionResult Create(Document_Type doc)
//        {
//            try
//            {
//                if (doc.Id > 0)
//                {
//                    //Edit
//                    Document_Type document = _documentTypeRep.FindBy(C => C.Id == doc.Id).FirstOrDefault();
//                    var name = document.Name;
//                    document.Name = doc.Name;

//                    _documentTypeRep.Edit(document);
//                    _documentTypeRep.Save(userEmail, Request.UserHostAddress);
//                    TempData["status"] = "pass";
//                    TempData["Message"] = "Document type '" + name + "' was Modified to '" + doc.Name + "' successfully.";
//                }
//                else
//                {
//                    Document_Type document = _documentTypeRep.FindBy(C => C.Name.ToLower() == doc.Name.ToLower()).FirstOrDefault();
//                    if (document == null)
//                    {
//                        _documentTypeRep.Add(doc);
//                        _documentTypeRep.Save(userEmail, Request.UserHostAddress);
//                        TempData["status"] = "pass";
//                        TempData["Message"] = "Document type '" + doc.Name + "' was created successfully.";
//                    }
//                    else
//                        throw new Exception("Document Type already existing");
//                }
//                //TempData["status"] = "warn";
//                //TempData["Message"] = "Please Complete the required fields properly to proceed";
//            }
//            catch(Exception ex)
//            {
//                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
//                TempData["status"] = "fail";
//                TempData["Message"] = "An Error occured! The Document type was not created because " + ex.Message;
//            }
//            return RedirectToAction("Index");
//        }

//        public ActionResult Delete(int id)
//        {
//            Document_Type document = _documentTypeRep.FindBy(c => c.Id == id).FirstOrDefault();
//            if (document != null)
//            {
//                return View(document);
//            }
//            return View("AdPageNotFound");
//        }

//        [HttpPost]
//        public ActionResult Delete(Document_Type model)
//        {
//            try
//            {
//                Document_Type document = _documentTypeRep.FindBy(c => c.Id == model.Id).FirstOrDefault();
//                if (document != null)
//                {
//                    _documentTypeRep.Delete(document);
//                    _documentTypeRep.Save(userEmail, Request.UserHostAddress);
//                    TempData["status"] = "pass";
//                    TempData["Message"] = "The Document type was successfully deleted";
//                    return RedirectToAction("Index");
//                }
//                return View();
//            }
//            catch(Exception ex)
//            {
//                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
//                TempData["status"] = "fail";
//                TempData["Message"] = "The Document type was not deleted";
//                return RedirectToAction("Index");
//            }
//        }
//    }
//}