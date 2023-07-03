using LpgLicense.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NewDepot.Helpers;
using NewDepot.Models;
using System;
using System.Linq;
using System.Security.Claims;
using NewDepot.Controllers.Authentications;
using NewDepot.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.IO;
using System.Net.Http.Headers;

namespace NewDepot.Controllers
{
    public class FormController : Controller
    {
        private readonly Depot_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;

        ElpsResponse elpsResponse = new ElpsResponse();

        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;

        public FormController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



       // GET: /Forms/
            public IActionResult Index(string Id, string companyId, string applicationId, string msg = "")
              {
            Id = Id.ToLower();
            Forms frm = _context.Forms.Where(a => a.TableName.ToLower() == Id).FirstOrDefault();
            var formFields = new List<Fields>();
            List<Fields> formField = _context.Fields.Where(C => C.FormId == frm.Id).ToList();
            _helpersController.LogMessages(frm.FriendlyName + " has " + formField.Count() + " fields");
            foreach (var item in formField)
            {
                if (item.DataType == "hidden" && item.Label == "CompanyId")
                {
                    item.OptionValue = companyId;
                    formFields.Add(item);
                }
                else if (item.DataType == "hidden" && item.Label == "ApplicationId")
                {
                    item.OptionValue = applicationId;
                    formFields.Add(item);
                }
                else
                {
                    formFields.Add(item);
                }
            }
            if (!string.IsNullOrEmpty(msg))
            {
                if (msg.ToLower() == "pass")
                {
                    ViewBag.Message = "Your Entry was Submitted Successfully! Thank you.";
                }
            }

            _helpersController.LogMessages("After iteration, returning " + formFields.Count() + " fields");
            ViewBag.Forms = frm;
            return View(formFields);
        }

        [AllowAnonymous, HttpPost]
        public IActionResult Index(IFormCollection formCollection, List<IFormFile> Images, string Id, string companyId, string applicationId)
        {
            //id= Friendly Name of the Forms
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            Id = Id.ToLower();
            Forms frm = _context.Forms.Where(C => C.FriendlyName.ToLower() == Id).FirstOrDefault();

            List<Fields> formField = _context.Fields.Where(C => C.FormId == frm.Id).ToList();
            Guid groupId = Guid.NewGuid();
            List<IFormFile> files = new List<IFormFile>();

            if (Images != null)
            {
                foreach (IFormFile x in Images)
                {
                    files.Add(x);
                }
            }


            foreach (var item in formField)
            {
                if (item.DataType.ToLower() == "table" && !string.IsNullOrEmpty(item.OptionValue))
                {
                    string value = "";
                    List<string> options = item.OptionValue.Split('|').ToList();
                    var _cols = options[0].Trim();
                    var _rows = options[1].Trim();
                    var cols = _cols.Split('=')[1].Split(';');
                    var rows = _rows.Split('=')[1].Split(';');
                    if (rows[0].StartsWith("-1"))
                    {
                        // Dynamic
                        for (int i = 0; i < rows.Count(); i++)
                        {
                            for (int j = 0; j < cols.Count(); j++) //Starts from 0
                            {
                                var label = item.Id + "_R" + i + "_C" + j;
                                string formParameter = formCollection[label];
                                var v = label + ":" + formParameter;
                                var vv = string.IsNullOrEmpty(value) ? v : "\\" + v;
                                value += vv;
                            }
                            value += (i + 1) == rows.Count() ? "" : "|";
                        }
                    }
                    else
                    {
                        // Normal
                        for (int i = 0; i < rows.Count(); i++)
                        {
                            for (int j = 1; j < cols.Count(); j++)  //Starts from 1
                            {
                                var label = item.Id + "_R" + i + "_C" + j;
                                string formParameter = formCollection[label];
                                var v = label + ":" + formParameter;
                                var vv = string.IsNullOrEmpty(value) ? v : "\\" + v;
                                value += vv;
                            }
                            value += (i + 1) == rows.Count() ? "" : "|";
                        }
                    }

                    FieldValues fieldV = new FieldValues();

                    fieldV.Value = value;
                    fieldV.FieldId = item.Id;
                    fieldV.GroupId = groupId;
                    _context.FieldValues.Add(fieldV);

                }
                else if (item.DataType.ToLower() == "image" && files.Count > 0)
                {
                    List<string> options = item.OptionValue.Split(';').ToList();
                    var value = string.Empty;

                    var max = Convert.ToInt16(options[0].Split('=')[1]);
                    for (int i = 0; i < max; i++)
                    {
                        var pre = "img_" + item.Id + "_" + i;
                        var file = files;
                        if (file != null)
                        {
                            //lk
                            //var source = ImageHelper.UploadFormsImages(file, pre, userEmail, Request.UserHostAddress);
                            var source = "formImage";
                            value += source + ";";
                        }
                    }

                    FieldValues fieldV = new FieldValues();
                    fieldV.Value = value.EndsWith(";") ? value.Substring(0, value.Length - 1) : value;
                    fieldV.FieldId = item.Id;
                    fieldV.GroupId = groupId;
                    _context.FieldValues.Add(fieldV);

                }
                else
                {
                    string formParameter = formCollection[item.Label];
                    FieldValues fieldV = new FieldValues();

                    fieldV.Value = formParameter;
                    fieldV.FieldId = item.Id;
                    fieldV.GroupId = groupId;

                    _context.FieldValues.Add(fieldV);
                }
            }
            _context.SaveChanges();
            _helpersController.LogMessages("Add fields for form with name:"+ frm.Title, userEmail);

            //log messages
            return RedirectToAction("Index", new { Id = Id, msg = "pass", companyId = companyId, applicationId = applicationId });
        }

        public IActionResult List()
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            List<Forms> allFormss = _context.Forms.ToList();
            if (TempData["alert"] != null)
            {
                ViewBag.Alert = (AlertBox)TempData["alert"];
            }
            _helpersController.LogMessages("Displaying all forms", userEmail);

            var getPhaseForm = (from u in _context.Forms.AsEnumerable()
                                join p in _context.Phases.AsEnumerable() on u.PhaseId equals p.id
                                select p).ToList();

            var phases = _context.Phases.ToList().Where(p => !getPhaseForm.Any(p2 => p2.id == p.id));
            var phases2 = phases.Where(p => getPhaseForm.All(p2 => p2.id != p.id));

            ViewBag.Phases = phases2;

            return View(allFormss);
        }

        public IActionResult FilledForms()
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            List<Forms> allFormss = _context.Forms.ToList();
            _helpersController.LogMessages("Displaying filled forms", userEmail);

            return View(allFormss);

        }

        public IActionResult CreateForms()
        {
            Forms frm = new Forms();
            var getPhaseForm = (from u in _context.Forms.AsEnumerable()
                              join p in _context.Phases.AsEnumerable() on u.PhaseId equals p.id
                              select p).ToList();
            
            var phases= _context.Phases.ToList().Where(p => ! getPhaseForm.Any(p2 => p2.id == p.id));
            var phases2 = phases.Where(p => getPhaseForm.All(p2 => p2.id != p.id));

            ViewBag.Phases = phases2;


            return View(frm);
        }

        public IActionResult AddForm(string phase, string formname)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            string result = "";
            if (formname!= "" && phase != "")
            {
                var form = new Forms();
                form.TableName = formname;  
                form.FriendlyName= formname;
                form.IsPublished = false;
                form.Deleted = false;
                form.PhaseId =Convert.ToInt16(phase);
                form.CreatedOnDate = DateTime.Now;
                _context.Forms.Add(form);
                _context.SaveChanges();
                _helpersController.LogMessages("Creating new form for " + form.FriendlyName, userEmail);
                result = "Success";
            }
            else
            {
                result = "Form or phase name value was not provided";
            }
            return Json(result);
        }

        public IActionResult ManageForm(int Id)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            Forms frm = _context.Forms.Where(C => C.Id == Id).FirstOrDefault();
            if (frm == null)
            {
                TempData["alert"] = new AlertBox()
                {
                    Title = "Cannot Manage!!!",
                    ButtonType = AlertType.Failure,
                    Message = "The form does not Exist. Please check and try again!"
                };
                return RedirectToAction("List");
            }
            var getFields = _context.Fields.Where(f => f.FormId == frm.Id).ToList();
            List<FieldValues> formValues = _context.FieldValues.Where(C => C.FormId == Id).ToList();
            if (formValues.Count() <= 0)
            {
                var CompanyId = _context.Fields.Where(a => a.FormId == frm.Id && a.Label == "CompanyId" && a.DataType == "hidden").FirstOrDefault();
                var ApplicationId = _context.Fields.Where(a => a.FormId == frm.Id && a.Label == "ApplicationId" && a.DataType == "hidden").FirstOrDefault();
                //if not existing, create for each form
                if (CompanyId == null)
                {
                    var fd = new Fields();
                    fd.DataType = "hidden";
                    fd.CreatedByUserId = userEmail;
                    fd.CreatedOnDate = DateTime.Now;
                    fd.FormId = frm.Id;
                    fd.Label = "CompanyId";
                    fd.LastModifiedByUserId = userEmail;
                    fd.lastModifiedOnDate =DateTime.Now;
                    fd.SortOrder = 1;
                    fd.IsRequired = true;

                    _context.Fields.Add(fd);
                }
                if (ApplicationId == null)
                {
                    var fd = new Fields();
                    fd.DataType = "hidden";
                    fd.CreatedByUserId = userEmail;
                    fd.CreatedOnDate = DateTime.Now;
                    fd.FormId = frm.Id;
                    fd.Label = "ApplicationId";
                    fd.LastModifiedByUserId = userEmail;
                    fd.lastModifiedOnDate = DateTime.Now;
                    fd.SortOrder = 2;
                    fd.IsRequired = true;

                    _context.Fields.Add(fd);
                }
                _context.SaveChanges();

                ViewBag.FormsName = frm.FriendlyName;
                Fields field = new Fields();
                field.FormId = Id;
                List<DataType> allTypes = GetTypes;
                ViewBag.types = new SelectList(allTypes.Where(a => a.Key != "hidden"), "Key", "Key");

                return View(getFields);
            }
            else
            {
                ViewBag.Alert = new AlertBox()
                {
                    Title = "Cannot Manage!!!",
                    ButtonType = AlertType.Warning,
                    Message = "You cannot modify this form again as it has already been filled by a user."
                };
            }
            _helpersController.LogMessages("Managing form for " + frm.Title, userEmail);

            return View(getFields);
        }

        [HttpPost]
        public IActionResult CreateField(Fields field, bool edit = false)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            if (edit)
            {
                var ff = _context.Fields.Where(a => a.Id == field.Id).FirstOrDefault();
                ff.Label = field.Label;
                ff.IsRequired = field.IsRequired;
                ff.LastModifiedByUserId = userEmail;
                ff.lastModifiedOnDate =  DateTime.Now;
                ff.OptionValue = field.OptionValue;
                ff.DataType = field.DataType;
                ff.Description = field.Description;
                ff.DisplayLabel = field.DisplayLabel;

                _context.SaveChanges();
                return RedirectToAction("ManageForm", new { Id = field.FormId });
            }
            var others = _context.Fields.Where(f => f.FormId == field.FormId).OrderBy(a => a.SortOrder).ToList();
            var lastOrder = others[others.Count() - 1].SortOrder;

            field.LastModifiedByUserId = userEmail;
            field.lastModifiedOnDate = DateTime.Now;
            field.CreatedByUserId = userEmail;
            field.CreatedOnDate = DateTime.Now;
            field.SortOrder = lastOrder + 1;
            _context.Fields.Add(field);
            _context.SaveChanges();

            Forms frm = _context.Forms.Where(C => C.Id == field.FormId).FirstOrDefault();

            _helpersController.LogMessages("Adding fields for " + frm.FriendlyName, userEmail);

            return RedirectToAction("ManageForm", new { Id = field.FormId });
        }

        public IActionResult EditField(int id)
        {
            var field = _context.Fields.Where(a => a.Id == id).FirstOrDefault();
            List<DataType> allTypes = GetTypes;
            ViewBag.types = new SelectList(allTypes.Where(a => a.Key != "hidden"), "Key", "Key");
            return PartialView(field);
        }

        public IActionResult Delete(int id)
        {
            var form = _context.Forms.Where(a => a.Id == id).FirstOrDefault();
            return View(form);
        }

        [HttpPost]
        public IActionResult Delete(Forms model)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var toDel = _context.Forms.Where(a => a.Id == model.Id).FirstOrDefault();
            if (toDel != null)
            {
                toDel.Deleted = true;
                _context.SaveChanges();

                _helpersController.LogMessages("Deleting fields for " + toDel.FriendlyName, userEmail);

                TempData["Message"] = "Forms " + toDel.FriendlyName + " deleted successfully!";
                TempData["MsgType"] = "pass";
            }
            else
            {
                TempData["Message"] = "Forms " + toDel.FriendlyName + " cannot be deleted, Please try again.";
                TempData["MsgType"] = "fail";
            }
            return RedirectToAction("List");
        }

        public IActionResult Publish(int id)
        {
            var form = _context.Forms.Where(a => a.Id == id).FirstOrDefault();
            return View(form);
        }

        [HttpPost]
        public IActionResult Publish(Forms model)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var toEdit = _context.Forms.Where(a => a.Id == model.Id).FirstOrDefault();
            if (toEdit != null)
            {
                toEdit.IsPublished = true;
                _context.SaveChanges();
                _helpersController.LogMessages("Publishing form with name: " + toEdit.FriendlyName, userEmail);

                TempData["Message"] = "Forms " + toEdit.FriendlyName + " Published successfully!";
                TempData["MsgType"] = "pass";
            }
            else
            {
                TempData["Message"] = "Forms " + toEdit.FriendlyName + " cannot be Published, Please try again.";
                TempData["MsgType"] = "fail";
            }
            return RedirectToAction("List");
        }


        public IActionResult FormsField(int Id)
        {
            List<Fields> fields = _context.Fields.Where(C => C.FormId == Id).ToList();
            return PartialView(fields);
        }

        public IActionResult DisplayForms(int Id)
        {

            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            List<Fields> formValues = _context.Fields.Where(C => C.FormId == Id).ToList();
            ViewBag.FormsName = _context.Forms.Where(f => f.Id == Id).Select(f => f.TableName).FirstOrDefault();
            List<string> headers = new List<string>();
            List<string> values = new List<string>();

            foreach (var item in formValues)
            {
                if (!headers.Contains(item.Label))
                {
                    headers.Add(item.Label);
                }

                values.Add(item.Label);
            }
            Forms frm = _context.Forms.Where(C => C.Id == Id).FirstOrDefault();

            _helpersController.LogMessages("Displaying all fields for " + frm.FriendlyName, userEmail);

            TempForms tpf = new TempForms { HeaderCount = headers.Count, Headers = headers, Values = values };
            return View(tpf);

        }

        List<DataType> GetTypes
        {
            get
            {
                List<DataType> types = new List<DataType>();
                types.Add(new DataType { Key = "One line Text", Value = "TextBox" });
                types.Add(new DataType { Key = "Date", Value = "Date" });
                types.Add(new DataType { Key = "MultiLine Text", Value = "TextArea" });
                types.Add(new DataType { Key = "Options", Value = "DropDown" });
                types.Add(new DataType { Key = "hidden", Value = "hidden" });
                types.Add(new DataType { Key = "CheckBox", Value = "checkbox" });
                types.Add(new DataType { Key = "Header", Value = "label" });
                types.Add(new DataType { Key = "Description", Value = "label" });
                types.Add(new DataType { Key = "Table", Value = "Table" });
                types.Add(new DataType { Key = "Image", Value = "Image" });

                return types;
            }

        }

        List<ValidationTypes> GetValidation
        {
            get
            {
                List<ValidationTypes> types = new List<ValidationTypes>();
                types.Add(new ValidationTypes { Name = "Email", Validation = "TextBox" });


                return types;
            }

        }

    }

    class DataType
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    class ValidationTypes
    {
        public string Validation { get; set; }
        public string Name { get; set; }
    }

    public class TempForms
    {
        public List<string> Headers { get; set; }

        public List<string> Values { get; set; }

        public int HeaderCount { get; set; }

        public int RowCount
        {
            get { return Headers.Count / Values.Count; }
        }

    }

}

