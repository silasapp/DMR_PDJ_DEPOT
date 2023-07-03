using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using NewDepot.Helpers;
using NewDepot.Controllers.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewDepot.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace NewDepot.Controllers.Application
{
    //[Authorize]
    public class SearchController : Controller
    {
        RestSharpServices _restService = new RestSharpServices();
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public SearchController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        public IActionResult Index(string q)
        {
            ViewBag.AppDoc = _context.ApplicationDocuments.ToList();

            //return View("Error");
            if (!string.IsNullOrEmpty(q))
            {
                ulong x;
                if (q.Length > 11 && ulong.TryParse(q, out x))
                {
                    #region Looking for Application
                    var result = FindApplication(q);
                    if (result != null)
                    {
                        var history = _context.application_desk_histories.Where(a => a.application_id == result.app.id).OrderByDescending(a => a.date);
                        ViewBag.History = history.Take(2);
                        ViewBag.SearchKey = q;
                        return View(result);

                    }
                    #endregion
                }
                else
                {
                    #region Looking for Permit
                    var result = FindPermit(q);
                    if (result != null)
                    {
                       return View("Permit", result);

                    }
                    else
                    {
                        var coy = FindCompany(q);
                        if (coy != null)
                            return RedirectToAction("FullCompanyProfile", "Companies", new { id = @generalClass.Encrypt(coy.id.ToString()) });
                    }
                    #endregion
                }
            }
            return View();
        }

        private companies FindCompany(string q)
        {
            q = q.ToLower();
            var coy = _context.companies.Where(a => a.name.ToLower().Contains(q) || a.CompanyEmail.ToLower().Contains(q)).FirstOrDefault();
            return coy;
        }

        public IActionResult FindFacility(int id)
        {
            var facs = _context.Facilities.Where(a => a.CompanyId == id && a.DeletedStatus != true).ToList();
            return Json(facs);
        }

        public MyApps FindApplication(string refno)
        {

            var app = (from u in _context.applications.AsEnumerable()
                       join phs in _context.Phases.AsEnumerable() on u.PhaseId equals phs.id
                       join c in _context.companies.AsEnumerable() on u.company_id equals c.id
                       //join perm in _context.permits.AsEnumerable() on u.id equals perm.application_id
                       join fac in _context.Facilities.AsEnumerable() on u.FacilityId equals fac.Id
                       join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                       join std in _context.States_UT.AsEnumerable() on ad.StateId equals std.State_id
                       where u.reference== refno
                       select new MyApps
                       {
                           //AppPermit = perm,
                           app = u,
                           comp = c,
                           PhaseName = phs.name,
                           Type = u.type,
                           OfficeName = _helpersController.GetApplicationOffice(u.id).FirstOrDefault().OfficeName.ToString(),
                           CurrentStaff = _context.Staff.Where(s => s.StaffID == u.current_desk).FirstOrDefault() == null ? "Company" : _context.Staff.Where(s => s.StaffID == u.current_desk).FirstOrDefault().StaffEmail,
                           ApplicationDocs = _context.SubmittedDocuments.Where(s => s.AppID == u.id && s.DeletedStatus != true).ToList(),
                           StateName = std.StateName
                       }).FirstOrDefault();
            if(app !=null)
            { 
            return app;
            }
            return null;
        }

        public MyApps FindPermit(string permitno)
        {
            var app = (from u in _context.applications.AsEnumerable()
                       join phs in _context.Phases.AsEnumerable() on u.PhaseId equals phs.id
                       join c in _context.companies.AsEnumerable() on u.company_id equals c.id
                       join perm in _context.permits.AsEnumerable() on u.id equals perm.application_id
                       join fac in _context.Facilities.AsEnumerable() on u.FacilityId equals fac.Id
                       join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                       join std in _context.States_UT.AsEnumerable() on ad.StateId equals std.State_id
                       where perm.permit_no.ToLower() == permitno.ToLower() && u.isLegacy!= true
                       select new MyApps
                       {
                           AppPermit = perm,
                           CheckApprovalType = (phs.ShortName == "NDT" || phs.ShortName == "LTO" || phs.ShortName == "LR" || phs.ShortName == "TO" || phs.ShortName == "RC") ? "Yes" : "",
                           ModifyType = _context.FacilityModifications.Where(a => a.ApplicationId == u.id).FirstOrDefault() != null ? _context.FacilityModifications.Where(a => a.ApplicationId == u.id).FirstOrDefault().Type : "",
                           app = u,
                           comp = c,
                           PhaseName = phs.name,
                           Type = u.type,
                           OfficeName = _helpersController.GetApplicationOffice(u.id).FirstOrDefault().OfficeName.ToString(),
                           CurrentStaff = _context.Staff.Where(s => s.StaffID == u.current_desk).FirstOrDefault() == null ? "Company" : _context.Staff.Where(s => s.StaffID == u.current_desk).FirstOrDefault().StaffEmail,
                           ApplicationDocs = _context.SubmittedDocuments.Where(s => s.AppID == u.id && s.DeletedStatus != true).ToList(),
                           StateName = std.StateName
                       }).FirstOrDefault();
            
           
            return app;
        }


        #region Legacy
        //[Authorize(Roles = "AllStaffRoles")]
        public IActionResult Legacy()
        {
            var country = _context.countries.Where(a => a.name.ToLower() == "nigeria".ToLower()).FirstOrDefault();
            var states = _context.States_UT.Where(a => a.Country_id == 156);
            ViewBag.States = states.OrderBy(a => a.StateName).ToList();

            var model = new MyApps();
            var found = (from u in _context.Legacies.AsEnumerable()
                         join comp in _context.companies.AsEnumerable() on u.CompId equals comp.id.ToString()

                         select new MyApps
                         {
                             legaciess = u,
                             CompanyName = comp.name,
                             CompanyDetails = comp.CompanyEmail,
                             StateName = u.City
                         });
            if (TempData["alert"] != null)
            {
                ViewBag.Alert = (AlertBox)TempData["alert"];
            }

            if (TempData["NotAdded"] != null)
            {
                ViewBag.NotAdded = (List<Legacies>)TempData["NotAdded"];
            }
            return View(model);
        }


        public int LegacyMethod(Legacies model, string cEmail, bool batch = false)
        {
            int val = 0;
            model.IsUsed = false;
            if (string.IsNullOrEmpty(model.CompId.ToString()))
            {
                model.CompId = " ";
            }
            model.LicenseNo = model.LicenseNo.Trim();
            var check = _context.Legacies.Where(a => a.LicenseNo.ToLower() == model.LicenseNo.ToLower()).FirstOrDefault();
            if (check != null)
            {
                //notAdded.Add(check);
                if (!batch)
                {
                    var alert = new AlertBox()
                    {
                        ButtonType = AlertType.Warning,
                        Message = "A license with the provided license number already exist on portal. Please check and try again.",
                        Title = "Existing License"
                    };

                    TempData["alert"] = alert;
                }
                else
                {
                    _helpersController.LogMessages("Duplicate Entry on Batch: " + model.LicenseNo);
                    val = 1;
                }
            }
            else
            {
                model.ATKTanks = model.ATKVol > 0 ? model.ATKTanks > 1 ? model.ATKTanks : 1 : 0;
                model.BaseOilTanks = model.BaseOilVol > 0 ? model.BaseOilTanks > 1 ? model.BaseOilTanks : 1 : 0;
                model.BitumenTanks = model.BitumenVol > 0 ? model.BitumenTanks > 1 ? model.BitumenTanks : 1 : 0;
                model.FuelOilTanks = model.FuelOilVol > 0 ? model.FuelOilTanks > 1 ? model.FuelOilTanks : 1 : 0;
                model.LubeOilGreaseTanks = model.LubeOilGreaseVol > 0 ? model.LubeOilGreaseTanks > 1 ? model.LubeOilGreaseTanks : 1 : 0;
                model.AGO_Tanks = model.AGOVol > 0 ? model.AGO_Tanks > 0 ? model.AGO_Tanks : 1 : 0;
                model.DPK_Tanks = model.DPKVol > 0 ? model.DPK_Tanks > 0 ? model.DPK_Tanks : 1 : 0;
                model.PMS_Tanks = model.PMSVol > 0 ? model.PMS_Tanks > 0 ? model.PMS_Tanks : 1 : 0;

                _context.Legacies.Add(model);
                _context.SaveChanges();
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                _helpersController.LogMessages("Legacy entered: " + model.LicenseNo, userEmail);

                #region Send Notification mail to Marketer
                if (!batch)
                {
                    AlertBox alert = null;

                    alert = new AlertBox()
                    {

                        ButtonType = AlertType.Success,
                        Message = "Legacy license added to portal successfully" + (!string.IsNullOrEmpty(cEmail) ? " and the marketer has been sent a mail (" + cEmail + ") with link to proceed with the application." : "."),
                        Title = "License Added"
                    };

                    TempData["alert"] = alert;
                }
                #endregion

            }
            return val;
        }

        [HttpPost]
        //[Authorize(Roles = "Admin, Support, Support2, ITAdmin")]
        public IActionResult SendMail(string email, string lno)
        {
            AlertBox alert = null;
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(lno))
            {
                var license = _context.Legacies.Where(a => a.LicenseNo.ToLower() == lno.ToLower()).FirstOrDefault();
                if (license != null)
                {
                    alert = new AlertBox()
                    {
                        ButtonType = AlertType.Success,
                        Message = "Marketer has been sent a mail (" + email + ") with link to proceed with the application",
                        Title = "Mail Sent"
                    };

                }
                else
                {
                    alert = new AlertBox()
                    {
                        ButtonType = AlertType.Failure,
                        Message = "The license number supplied does not match any record. Please check and try again.",
                        Title = "Not Found"
                    };
                }
            }
            else
            {
                alert = new AlertBox()
                {
                    ButtonType = AlertType.Failure,
                    Message = "One or more of the required field(s) is/are missing. Please check and try again",
                    Title = "Mail Not Sent"
                };
            }
            TempData["alert"] = alert;
            return RedirectToAction("Legacy");
        }


        [HttpPost]
        // [Authorize(Roles = "Admin, Staff")]
        public IActionResult FindLegacy(string q)
        {
            var found = (from u in _context.Legacies.AsEnumerable()
                         join comp in _context.companies.AsEnumerable() on u.CompId equals comp.id.ToString()
                         where comp.name.ToLower().Contains(q) || u.LicenseNo.ToLower().Contains(q)
                         select new MyApps
                         {
                             legaciess = u,
                             CompanyName = comp.name,
                             CompanyDetails = comp.CompanyEmail,
                             StateName = u.City
                         });

            return PartialView(found);
        }

        //[Authorize(Roles = "Admin, Support, Support2, ITAdmin")]
        public IActionResult EditLegacy(int id, string lno)
        {
            var found = _context.Legacies.Where(a => a.Id == id && a.LicenseNo.ToLower() == lno.ToLower()).FirstOrDefault();
            var states = _context.States_UT.Where(a => a.Country_id == 156);
            ViewBag.States = states.OrderBy(a => a.StateName).ToList();

            return PartialView(found);
        }

        // [Authorize(Roles = "Admin, Support, Support2, ITAdmin")]
        [HttpPost]
        public IActionResult EditLegacy(Legacies model)
        {
            if (ModelState.IsValid)
            {
                var legacy = _context.Legacies.Where(a => a.Id == model.Id).FirstOrDefault();

                if (legacy != null)
                {
                    legacy.LicenseNo = model.LicenseNo.ToUpper();
                    legacy.Issue_Date = model.Issue_Date.ToString();
                    legacy.Exp_Date = model.Exp_Date.ToString();
                    legacy.FacilityName = model.FacilityName;
                    legacy.FacilityAddress = model.FacilityAddress;
                    legacy.State = model.State;
                    legacy.LGA = model.LGA;
                    legacy.City = model.City;
                    legacy.LandMeters = model.LandMeters;
                    //IsPipeLine  =  = model.IsPipeLine;
                    legacy.IsHighTension = model.IsHighTension;
                    //IsHighWay  =  = model.IsHighWay;
                    legacy.ContactName = model.ContactName;
                    legacy.ContactPhone = model.ContactPhone;
                    legacy.CreatedAt = DateTime.Now;


                    _context.SaveChanges();
                    var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                    int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                    _helpersController.LogMessages("Legacy details modifies: " + model.LicenseNo, userEmail);

                }
            }
            else
            {
                TempData["alert"] = new AlertBox() { ButtonType = AlertType.Failure, Message = "One or more of the entries is/are invalid. Try again.", Title = "Legacy Not Updated" };
            }

            return RedirectToAction("Legacy");
        }

        public IActionResult FindUserApplication(string currentLicense)
        {
            var app = (from u in _context.applications.AsEnumerable()
                       join perm in _context.permits.AsEnumerable() on u.id equals perm.application_id
                       join fac in _context.Facilities.AsEnumerable() on u.FacilityId equals fac.Id
                       join ad in _context.addresses.AsEnumerable() on fac.AddressId equals ad.id
                       join std in _context.States_UT.AsEnumerable() on ad.StateId equals std.State_id
                       where u.current_Permit == currentLicense
                       select new MyApps
                       {
                           AppPermit = perm,
                           app = u,
                           StateName = std.StateName
                       });

            if (app != null)
            {
                return Json(app);
            }
            return Json(0);
        }
        #endregion


    }
}
//[HttpPost]
//[Authorize(Roles = "AllStaffRoles")]
//public ActionResult AddLegacy(Legacies model, string cEmail, HttpPostedFileBase file)
//{
//    UtilityHelper.LogMessages("We are here");
//    //var lgs= new List<Legacy>();
//    if (model == null || file != null)
//    {
//        AlertBox alert;
//        string filePath = "";
//        if (file != null)
//        {
//            UtilityHelper.LogMessages("File is not Null");
//            try
//            {
//                #region hide this Part for now


//                #endregion

//                ISheet sheet;
//                var fn = file.FileName.Split('.');
//                var fxt = fn[fn.Length - 1];
//                if (fxt == "xls")
//                {
//                    UtilityHelper.LogMessages("File is xls");
//                    HSSFWorkbook hssfwb = new HSSFWorkbook(file.InputStream); //HSSWorkBook object will read the Excel 97-2000 formats  
//                    sheet = hssfwb.GetSheetAt(0); //get first Excel sheet from workbook  
//                }
//                else
//                {
//                    //UtilityHelper.LogMessages("File is not xls");
//                    XSSFWorkbook hssfwb = new XSSFWorkbook(file.InputStream); //XSSFWorkBook will read 2007 Excel format  
//                    sheet = hssfwb.GetSheetAt(0); //get first Excel sheet from workbook   
//                }
//                var errors = 0;
//                var successes = 0;
//                StringBuilder sb = new StringBuilder();
//                Legacy leg = null;
//                DataFormatter dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);
//                UtilityHelper.LogMessages($"{sheet.LastRowNum} total number of rows");

//                for (int row = 1; row <= sheet.LastRowNum; row++) //Loop the records upto filled row  
//                {

//                    //leg = new Legacy();
//                    if (sheet.GetRow(row) != null) //null is when the row only contains empty cells   
//                    {
//                        //Here for sample , I just save the value in "value" field, Here you can write your custom logics...  
//                        var rw = sheet.GetRow(row);
//                        //get cell(2)Licesnse Number first
//                        //check if it already Exist
//                        var ln = rw.GetCell(2).StringCellValue;
//                        leg = _legacyRep.Where(a => a.LicenseNo.ToLower() == ln.ToLower()).FirstOrDefault();
//                        if (leg == null)
//                        {
//                            //UtilityHelper.LogMessages($"{ln} does not Exist");
//                            leg = new Legacy();
//                            leg.LicenseNo = ln;
//                            leg.CompID = rw.GetCell(1)?.StringCellValue;
//                            leg.CompName = rw.GetCell(3)?.StringCellValue;
//                            leg.CompAddress = rw.GetCell(4)?.StringCellValue;
//                            leg.FacilityAddress = rw.GetCell(5)?.StringCellValue;
//                            leg.LGA = rw.GetCell(6)?.StringCellValue;
//                            leg.State = rw.GetCell(7)?.StringCellValue;
//                            leg.AGOVol = Convert.ToDouble(rw.GetCell(10)?.NumericCellValue);
//                            leg.AGO_Tanks = Convert.ToInt32(rw.GetCell(11)?.NumericCellValue);
//                            leg.PMSVol = Convert.ToDouble(rw.GetCell(12)?.NumericCellValue);
//                            leg.PMS_Tanks = Convert.ToInt32(rw.GetCell(13)?.NumericCellValue);
//                            leg.DPKVol = Convert.ToDouble(rw.GetCell(14)?.NumericCellValue);
//                            leg.DPK_Tanks = Convert.ToInt32(rw.GetCell(15)?.NumericCellValue);
//                            leg.BitumenTanks = Convert.ToInt32(rw.GetCell(16)?.NumericCellValue);


//                            leg.BitumenVol = Convert.ToDouble(rw.GetCell(17)?.NumericCellValue);
//                            leg.ATKTanks = Convert.ToInt32(rw.GetCell(18)?.NumericCellValue);
//                            leg.ATKVol = Convert.ToDouble(rw.GetCell(19)?.NumericCellValue);
//                            leg.BaseOilTanks = Convert.ToInt32(rw.GetCell(20)?.NumericCellValue);
//                            leg.BaseOilVol = Convert.ToDouble(rw.GetCell(21)?.NumericCellValue);
//                            leg.LubeOilGreaseTanks = Convert.ToInt32(rw.GetCell(22)?.NumericCellValue);
//                            leg.LubeOilGreaseVol = Convert.ToInt32(rw.GetCell(23)?.NumericCellValue);

//                            leg.FuelOilTanks = Convert.ToInt32(rw.GetCell(24)?.NumericCellValue);
//                            leg.FuelOilVol = Convert.ToDouble(rw.GetCell(25)?.NumericCellValue);
//                            leg.Issue_Date = dataFormatter.FormatCellValue(rw.GetCell(26));
//                            leg.Exp_Date = dataFormatter.FormatCellValue(rw.GetCell(27));//.StringCellValue;
//                            leg.AppType = dataFormatter.FormatCellValue(rw.GetCell(28));//.StringCellValue;
//                            _legacyRep.Add(leg);
//                            errors++;
//                            // legList.Add(leg);
//                        }
//                        else
//                        {
//                            // UtilityHelper.LogMessages($"{ln} already Exist");
//                            sb.Append($"{ln} : already Exist || ");
//                        }


//                        //.GetCell(1).StringCellValue;

//                        //var a = value.GetCell(0);
//                        //var b = a.StringCellValue;
//                        //var c = a.NumericCellValue;

//                    }
//                }

//                _legacyRep.Save(User.Identity.Name, Request.UserHostAddress);
//                alert = new AlertBox()
//                {
//                    ButtonType = AlertType.Success,
//                    Message = successes + " Legacy Licenses added to portal successfully. " + sb.ToString() + (errors > 0 ? "There are " + errors + " Licences already existing and not added. "
//                    : sb.ToString()),
//                    Title = "Licenses Added"
//                };
//                ViewBag.alert = alert;
//                TempData["alert"] = alert;
//                //for (int i = 0; i < dt.Rows.Count; i++)
//                //{
//                //    var item = dt.Rows[i];
//                //    var legacy = new Legacy()
//                //    {
//                //        CompName = item["Company"].ToString(),
//                //        CompID = item["CompID"].ToString(),
//                //        FacilityAddress = item["Facility Address"].ToString(),
//                //        LicenseNo = item["License No"].ToString(),
//                //        State = item["State"].ToString(),
//                //        LGA = item["LGA"].ToString(),
//                //        Issue_Date = item["Issue Date"].ToString(),
//                //        AppType = item["AppType"].ToString(),
//                //         CompAddress=item["CompAddress"].ToString(),

//                //    };
//                //    var email = !string.IsNullOrEmpty(item["Email"].ToString()) ? item["Email"].ToString() : "";

//                //    if (!string.IsNullOrEmpty(legacy.LicenseNo) && !string.IsNullOrEmpty(legacy.CompName) && !string.IsNullOrEmpty(legacy.State))
//                //        errors += LegacyMethod(legacy, email);
//                //}

//                //successes = dt.Rows.Count - errors;
//                //alert = new AlertBox()
//                //{
//                //    ButtonType = AlertType.Success,
//                //    Message = successes + " Legacy Licenses added to portal successfully. " + (errors > 0 ? "There are " + errors + " Licences already existing and not added." : ""),
//                //    Title = "Licenses Added"
//                //};
//                //System.IO.File.Delete(filePath);
//            }
//            catch (Exception ex)
//            {
//                return Content(ex.ToString());
//                if (System.IO.File.Exists(filePath))
//                {
//                    System.IO.File.Delete(filePath);
//                }
//                var msg = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message;

//                alert = new AlertBox()
//                {
//                    ButtonType = AlertType.Failure,
//                    Message = "An error occurred! >>> " + msg,
//                    Title = "License not Added"
//                };
//            }
//        }
//        else
//        {
//            alert = new AlertBox()
//            {
//                ButtonType = AlertType.Failure,
//                Message = "No file attached for upload. Upload file again!",
//                Title = "License not Added"
//            };
//        }
//        TempData["alert"] = alert;
//    }
//    else
//    {
//        LegacyMethod(model, cEmail);
//    }

//    TempData["NotAdded"] = notAdded;

//    return RedirectToAction("Legacy");
//}

//[Authorize(Roles = "Staff, Admin, ITAdmin")]
//public ActionResult DownloadCompanyExcel()
//{
//    try
//    {

//        var comps = _vCompRep.GetAll().ToList();
//        HSSFWorkbook workbook = new HSSFWorkbook();
//        //define cell style

//        HSSFFont myHeaderFont = (HSSFFont)workbook.CreateFont();
//        myHeaderFont.FontHeightInPoints = 14;
//        myHeaderFont.FontName = "Tahoma";
//        myHeaderFont.IsBold = true;
//        HSSFFont myFont = (HSSFFont)workbook.CreateFont();
//        myFont.FontHeightInPoints = 12;
//        myFont.FontName = "Tahoma";


//        // Defining a border
//        HSSFCellStyle borderedCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
//        borderedCellStyle.SetFont(myFont);
//        borderedCellStyle.BorderLeft = BorderStyle.Medium;
//        borderedCellStyle.BorderTop = BorderStyle.Medium;
//        borderedCellStyle.BorderRight = BorderStyle.Medium;
//        borderedCellStyle.BorderBottom = BorderStyle.Medium;
//        borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;

//        // Defining a border
//        HSSFCellStyle hBorderedCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
//        hBorderedCellStyle.SetFont(myHeaderFont);
//        hBorderedCellStyle.BorderLeft = BorderStyle.Thick;
//        hBorderedCellStyle.BorderTop = BorderStyle.Thick;
//        hBorderedCellStyle.BorderRight = BorderStyle.Thick;
//        hBorderedCellStyle.BorderBottom = BorderStyle.Thick;
//        hBorderedCellStyle.VerticalAlignment = VerticalAlignment.Center;


//        ISheet Sheet = workbook.CreateSheet("Depot Company");
//        //Creat The Headers of the excel
//        IRow HeaderRow = Sheet.CreateRow(2);

//        //Create The Actual Cells
//        CreateCell(HeaderRow, 0, "Name", hBorderedCellStyle);
//        CreateCell(HeaderRow, 1, "CAC Number", hBorderedCellStyle);
//        CreateCell(HeaderRow, 2, "TIN Number", hBorderedCellStyle);
//        CreateCell(HeaderRow, 3, "Contact Person", hBorderedCellStyle);
//        CreateCell(HeaderRow, 4, "Contact Phone", hBorderedCellStyle);
//        CreateCell(HeaderRow, 5, "Contact Email", hBorderedCellStyle);
//        CreateCell(HeaderRow, 6, "Street", hBorderedCellStyle);
//        CreateCell(HeaderRow, 7, "City", hBorderedCellStyle);
//        CreateCell(HeaderRow, 8, "State", hBorderedCellStyle);
//        CreateCell(HeaderRow, 9, "Country", hBorderedCellStyle);
//        // This Where the Data row starts from
//        // This Where the Data row starts from
//        int RowIndex = 3;

//        //Iteration through some collection
//        foreach (var company in comps)
//        {
//            //Creating the CurrentDataRow
//            IRow CurrentRow = Sheet.CreateRow(RowIndex);
//            CreateCell(CurrentRow, 0, company.Name, borderedCellStyle);
//            CreateCell(CurrentRow, 1, company.RC_Number, borderedCellStyle);
//            CreateCell(CurrentRow, 2, company.Tin_Number, borderedCellStyle);
//            CreateCell(CurrentRow, 3, $"{company.Contact_FirstName} {company.Contact_LastName}", borderedCellStyle);
//            CreateCell(CurrentRow, 4, company.Contact_Phone, borderedCellStyle);
//            CreateCell(CurrentRow, 5, company.User_Id, borderedCellStyle);
//            CreateCell(CurrentRow, 6, company.address_1, borderedCellStyle);
//            CreateCell(CurrentRow, 7, company.City, borderedCellStyle);
//            CreateCell(CurrentRow, 8, company.StateName, borderedCellStyle);
//            CreateCell(CurrentRow, 9, company.CountryName, borderedCellStyle);
//            // This will be used to calculate the merge area

//            RowIndex++;
//        }

//        // Auto sized all the affected columns
//        int lastColumNum = Sheet.GetRow(2).LastCellNum;
//        for (int i = 0; i <= lastColumNum; i++)
//        {
//            Sheet.AutoSizeColumn(i);
//            GC.Collect();
//        }
//        // Write Excel to disk 
//        string filePath = Server.MapPath("~/App_Data/DepotCompanyList.xls");
//        using (var fileData = new FileStream(filePath, FileMode.Create))
//        {

//            workbook.Write(fileData);
//        }
//        var memoryStream = new MemoryStream();
//        //using (var strm= new MemoryStream())
//        //{
//        //    workbook.Write(strm);
//        //  return File(strm, "application/vnd.ms-excel");
//        //}

//        using (var fileStream = new FileStream(filePath, FileMode.Open))
//        {
//            fileStream.CopyTo(memoryStream);
//        }
//        memoryStream.Position = 0;
//        return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DepotCompanyList.xls");


//    }
//    catch (Exception x)
//    {

//        return Content(x.ToString());
//    }
//}

//private void CreateCell(IRow CurrentRow, int CellIndex, string Value, HSSFCellStyle Style)
//{
//    ICell Cell = CurrentRow.CreateCell(CellIndex);
//    Cell.SetCellValue(Value);
//    Cell.CellStyle = Style;
//}