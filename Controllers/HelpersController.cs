using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using NewDepot.Controllers.Authentications;
using NewDepot.Helpers;
using NewDepot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using System.Security.Cryptography;
using LpgLicense.Models;
using System.Text;
using RPartner = LpgLicense.Models.RPartner;
using System.Net;
using Staff = NewDepot.Models.Staff;
using System.DrawingCore.Imaging;
using iTextSharp;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using NewDepot.Models.Stored_Procedures;
//using UnityEngine;

namespace NewDepot.Controllers
{
    [Authorize]
    public class HelpersController : Controller
    {

        IHttpContextAccessor _httpContextAccessor;
        public static IConfiguration _configuration;
        public Depot_DBContext _context;

        RestSharpServices restSharpServices = new RestSharpServices();
        GeneralClass generalClass = new GeneralClass();
        RestSharpServices _restService = new RestSharpServices();
        //ElpsServices elpsServices = new ElpsServices();

        public HelpersController(Depot_DBContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        /*
       * Setting facility products as a join string
       */
        public  string GetFacilityProducts(int facility_id)
        {
            string result = "";
            List<string> product = new List<string>();

            if (facility_id > 0)
            {
                var products = from fp in _context.Facilities.AsEnumerable()
                               join t in _context.Tanks.AsEnumerable() on fp.Id equals t.FacilityId
                               join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                               where fp.Id == facility_id && (t.Status == null || (t.Status.Contains("Approved")))
                               select new
                               {
                                   ProductName = p.Name
                               };

                if (products.Count() > 0)
                {
                    for (int p = 0; p < products.Count(); p++)
                    {
                        product.Add(products.ToList()[p].ProductName);
                    }

                    result = string.Join(", ", product.ToList());
                }
                else
                {
                    result = "No available product";
                }
            }
            else
            {
                result = "No facility product found.";
            }

            return result;
        }
        public string GetTanksCapacity(int facility_id)
        {
            string result = "";
            List<string> product = new List<string>();

            if (facility_id > 0)
            {
                var tanksCapacity = (from fp in _context.Facilities.AsEnumerable()
                                join t in _context.Tanks.AsEnumerable() on fp.Id equals t.FacilityId
                                join p in _context.Products.AsEnumerable() on t.ProductId equals p.Id
                                where fp.Id == facility_id && (t.Status == null || (t.Status.Contains("Approved")))
                                select t).ToList().Sum(x => x.Capacity);
                result = tanksCapacity.ToString();
            }
            return result;

        }

        public static Byte[] BitmapToBytes(System.DrawingCore.Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.DrawingCore.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        public decimal GetPricePerVolume(double volume)
        {

            decimal fixdAmt4istFixdLtrs = Convert.ToDecimal(_configuration.GetSection("AmountSetting").GetSection("fixdAmt4istFixdLtrs").Value.ToString());
            double istFixdLtrs = Convert.ToDouble(_configuration.GetSection("AmountSetting").GetSection("istFixdLtrs").Value.ToString());
            double incrmentalAfterFixdLtrs = Convert.ToDouble(_configuration.GetSection("AmountSetting").GetSection("incrmentalAfterFixdLtrs").Value.ToString());
            decimal amt4IncrmentalLtrs = Convert.ToDecimal(_configuration.GetSection("AmountSetting").GetSection("amt4IncrmentalLtrs").Value.ToString());


            decimal amtDue = fixdAmt4istFixdLtrs;//150000

            double volDiff = volume - istFixdLtrs;//7500000
            if (volDiff > 0)
            {
                var f5000LtrsInDiff = volDiff / incrmentalAfterFixdLtrs; var amt4XtraLitrs = Convert.ToDecimal(f5000LtrsInDiff) * amt4IncrmentalLtrs;//100
                amtDue += Convert.ToDecimal(amt4XtraLitrs);
            }
            return amtDue;
            //if (cate.ToLower() == "CategoryA".ToLower())
            //{
            //    //Category A
            //    FirstTerm = type.ToLower() == "new" ? 9000 : 5000;
            //    Difference = 2000;
            //    volDiff = 10000;
            //}
            //else
            //{
            //    //Category B
            //    FirstTerm = type.ToLower() == "new" ? 7000 : 5000;
            //    Difference = 1000;
            //    volDiff = 20000;
            //}

            //if (volume <= volDiff)
            //{
            //    return FirstTerm;
            //}
            //else
            //{
            //    int nth = volume % volDiff > 0 ? ((volume / volDiff) + 1) : (volume / volDiff);
            //    returnValue = FirstTerm + ((nth - 1) * Difference);
            //}

            //return returnValue;

        }

        public byte[] GenerateQRCode(string qrcodeText)
        {

            QRCodeGenerator qrg = new QRCodeGenerator();
            QRCodeData qrd = qrg.CreateQrCode(qrcodeText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrd);
            System.DrawingCore.Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return BitmapToBytes(qrCodeImage);


            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrcodeText, QRCodeGenerator.ECCLevel.Q);
            //QRCode qrCode = new QRCode(qrCodeData);
            //System.DrawingCore.Bitmap qrCodeImage = qrCode.GetGraphic(20);
            //var imageResult = BitmapToBytes(qrCodeImage);
            //return imageResult.ToString();

        }
        public string GetDatePad(string day)
        {
            string pad = "";

            int dy = Convert.ToInt32(day);
            switch (dy)
            {
                case 1:
                case 21:
                case 31:
                    pad = $"{dy}st";
                    break;
                case 2:
                case 22:
                    pad = $"{dy}nd";
                    break;
                case 3:
                case 23:
                    pad = $"{dy}rd";
                    break;
                default:
                    pad = $"{dy}th";
                    break;
            }

            return pad;
        }

        //public static string GenerateQRCode22(string qrcodeText)
        //{
        //var barcodeWriter = new BarcodeWriter();
        //barcodeWriter.Format = BarcodeFormat.QR_CODE;
        //var result = barcodeWriter.Write(qrcodeText);
        //string stringBase64 = "";

        //using (MemoryStream ms = new MemoryStream())
        //{
        //    result.Save(ms, ImageFormat.Jpeg);
        //    byte[] byteImage = ms.ToArray();
        //    stringBase64 = Convert.ToBase64String(byteImage);

        //}
        //return stringBase64;

        //}
        public bool GenerateNMDPRAID_Preview(int stateID, out string dprid)
        {
            //PPD-013/DT
            dprid = "PPD-";
            
              var stat = _context.States_UT.Where(a => a.State_id == stateID).FirstOrDefault();
                if (stat != null)
                {
                    string stc = stat.StateName.Substring(0,3);
                    //get the last company NMDPRA Id set for this

                    var lastComp = _context.companies.OrderByDescending(a => a.id).FirstOrDefault(a => !string.IsNullOrEmpty(a.DPR_Id));
                    if (lastComp != null)
                    {
                        var did = lastComp.DPR_Id.Split(new char[] { '-', '/' });
                        var tempId = $"PPD-{did[1]}/{stc}";
                        var compY = _context.companies.FirstOrDefault(a => a.DPR_Id.ToLower() == tempId.ToLower());
                        if (compY == null)
                        {
                            dprid = tempId;
                        }
                        else
                        {
                            dprid = $"PPD-{(int.Parse(did[1]) + 1).ToString("D3")}/{stc}";
                        }

                    }
                    else
                    {
                        var rnd = new Random(1);
                        int i = rnd.Next(1, 500);
                        var tmpId = $"PPD-{i.ToString("D3")}/{stc}";
                        companies comp = null;
                        while (comp != null)
                        {
                            comp = _context.companies.Where(a => a.DPR_Id.ToLower() == tmpId.ToLower()).FirstOrDefault();
                            i = rnd.Next(1, 500);
                            tmpId = $"PPD-{i.ToString("D3")}/{stc}";
                        }
                        dprid = tmpId;
                    }
                    return true;
                }


            return false;
        }
        public bool GenerateNMDPRAID(int stateID, out string dprid)
        {
            //PPD-013/DT
            dprid = "PPD-";
            
              var stat = _context.States_UT.Where(a => a.State_id == stateID).FirstOrDefault();
                if (stat != null)
                {
                    string stc = stat.StateName.Substring(0,3);
                    //get the last company NMDPRA Id set for this

                    var lastComp = _context.companies.OrderByDescending(a => a.id).FirstOrDefault(a => !string.IsNullOrEmpty(a.DPR_Id));
                    if (lastComp != null)
                    {
                        var did = lastComp.DPR_Id.Split(new char[] { '-', '/' });
                        var tempId = $"PPD-{did[1]}/{stc}";
                        var compY = _context.companies.FirstOrDefault(a => a.DPR_Id.ToLower() == tempId.ToLower());
                        if (compY == null)
                        {
                            dprid = tempId;
                        }
                        else
                        {
                            dprid = $"PPD-{(int.Parse(did[1]) + 1).ToString("D3")}/{stc}";
                        }

                    }
                    else
                    {
                        var rnd = new Random(1);
                        int i = rnd.Next(1, 500);
                        var tmpId = $"PPD-{i.ToString("D3")}/{stc}";
                        companies comp = null;
                        while (comp != null)
                        {
                            comp = _context.companies.Where(a => a.DPR_Id.ToLower() == tmpId.ToLower()).FirstOrDefault();
                            i = rnd.Next(1, 500);
                            tmpId = $"PPD-{i.ToString("D3")}/{stc}";
                        }
                        dprid = tmpId;
                    }
                    return true;
                }


            return false;
        }

        public IActionResult GetLga(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var Id = Convert.ToInt32(id);
                var lg = _context.Lgas.Where(a => a.StateId == Id).OrderBy(c => c.Name).ToList();
                return Json(lg);
            }
            return Content("State Id is required");
        }
        public string UploadFormImages(IFormFile file, string imgName, string userName, string Ip="null")
        {
            if (file != null)
            {
                string folder = DateTime.Now.Day.ToString("00") + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Year;

                var uid = Guid.NewGuid().ToString();
                if (file.Length > 0)
                {
                    string fileName = string.Format(imgName + Path.GetExtension(file.FileName));
                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploads = Path.Combine(up, "images/FormImages");

                    string filePath = "/images/FormImages/" + fileName.Replace(";", "");

                    using (var s = new FileStream(Path.Combine(uploads, fileName.Replace(";", "")),
                         FileMode.Create))
                    {
                        file.CopyTo(s);
                    }
                  
                    return string.Format(filePath);
                }

            }

            return string.Empty;
        }

        /*
 * Getting all roles for staff
 */
        public JsonResult GetStaffRoles(int roleID = 0, bool deletedStatus = false)
        {
            var _roles = (from z in _context.UserRoles where z.DeleteStatus == deletedStatus select z);
            _roles = roleID != 0 ? _roles.Where(z => z.Role_id == roleID) :
              _roles.Where(z => z.DeleteStatus == deletedStatus);

            return Json(_roles.ToList());
        }
        public JsonResult GetFieldOffices(int fieldID = 0, bool deletedStatus = false)
        {
            var _fields = (from z in _context.FieldOffices where z.DeleteStatus == deletedStatus select z);
            _fields = fieldID != 0 ? _fields.Where(z => z.FieldOffice_id == fieldID) :
              _fields.Where(z => z.DeleteStatus == deletedStatus);

            return Json(_fields.ToList());
        }

        /*
  * Getting all Location for staff
  */
        public JsonResult GetLocations(int locationID = 0, bool deletedStatus = false)
        {
            var _location = (from z in _context.Location where z.DeleteStatus == deletedStatus select z);
            _location = locationID != 0 ? _location.Where(z => z.LocationID == locationID) :
              _location.Where(z => z.DeleteStatus == deletedStatus);

            return Json(_location.ToList());
        }

        public MyApps SingleApplicationDetails(int id)
        {
            var apps = (from app in _context.applications.AsEnumerable()
                        join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                        join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                        join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                        join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                        join ad in _context.addresses on fac.AddressId equals ad.id
                        join sf in _context.States_UT.AsEnumerable() on ad.StateId equals sf.State_id
                        where app.id == id && app.DeleteStatus != true && c.DeleteStatus != true && app.submitted == true
                        select new MyApps
                        {
                            appID = app.id,
                            Reference = app.reference,
                            CategoryName = cat.name,
                            PhaseName = phs.name,
                            category_id = cat.id,
                            FacilityId = fac.Id,
                            PhaseId = phs.id,
                            AppPermits = _context.permits.Where(x => x.application_id == app.id).ToList(),
                            //AppPermit = app.current_Permit!=null? app.current_Permit: "",
                            Current_Permit = app.current_Permit!=null? app.current_Permit: "",
                            Address_1 = ad.address_1,
                            Status = app.status,
                            Date_Added = Convert.ToDateTime(app.date_added),
                            DateSubmitted = Convert.ToDateTime(app.CreatedAt),
                            Submitted = app.submitted,
                            CompanyName = c.name,
                            Company_Id=c.id,
                            StateName = sf.StateName,
                            City = ad.city,
                            LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                            FacilityName = fac.Name,
                            Year = app.year,
                            Yearr = app.year.ToString()
                        });


            return apps.FirstOrDefault();
        }
        public List<MyApps> ApplicationDetails()
        {
            //var fields = _storedProcedure.AppFieldZones.FromSqlRaw("EXECUTE dbo.Applications_Field_Zone").ToList();
            var app = _context.applications
                .Include("Companies")
                .Include("Phases")
                .Include("Facility")
                .Include("Facility.Addresses.states")
                .Where(a => a.DeleteStatus != true && a.company_id > 0 && a.isLegacy != true)
                .Select(x => new MyApps
                {

                    appID = x.id,
                    Reference = x.reference,
                    Submitted = x.submitted,
                    CompanyName = x.Companies.name,
                    PhaseName = x.Phases.name,
                    CompanyDetails = x.Companies.CompanyEmail,
                    LGA = x.Facility.Addresses.LgaId > 0 ? _context.Lgas.Where(l => l.Id == x.Facility.Addresses.LgaId).FirstOrDefault().Name : x.Facility.Addresses.city,
                    FacilityId = x.Facility.Id,
                    Company_Id = x.company_id,
                    FacilityName = x.Facility.Name,
                    category_id = x.category_id,
                    Address_1 = x.Facility.Addresses.address_1,
                    FacilityDetails = x.Facility.Name + " (" + x.Facility.Addresses.address_1 + ")",
                    Year = x.year,
                    CategoryName = x.Categories.name.ToLower().Contains("pay sanction") ? "Pay Sanction" : x.Phases.name.ToUpper(),
                    CategoryId = x.category_id,
                    PhaseId = x.PhaseId,
                    ShortName = x.Phases.ShortName,
                    Type = x.type.ToUpper(),
                    Status = x.status,
                    StateName = x.Facility.Addresses.states.StateName,
                    currentDeskID = _context.MyDesk.Where(b => b.AppId == x.id && b.HasWork != true).FirstOrDefault() != null ? _context.MyDesk.Where(b => b.AppId == x.id && b.HasWork != true).FirstOrDefault().StaffID : 0,
                    Date_Added = x.date_added,
                    days = x.CreatedAt != null ? DateTime.Now.Day - ((DateTime)x.CreatedAt).Day : DateTime.Now.Day - ((DateTime)x.date_added).Day,
                    dateString = x.CreatedAt != null ? x.CreatedAt.Value.Date.ToString("yyyy-MM-dd") : x.date_added.Date.ToString("yyyy-MM-dd"),
                    DateSubmitted = x.CreatedAt != null ? (DateTime)x.CreatedAt : (DateTime)x.date_added,
                    CreatedAt = x.CreatedAt != null ? (DateTime)x.CreatedAt : (DateTime)x.date_added
                }).ToList();

            return app;
        }


        public string[] LineSplitter(string textToSplit, int maxlength)
        {
            string[] returnValue = new string[2];
            //int maxLength = (maxlength != null && maxlength.Value > 0 ? maxlength.Value : 60);
            int xwords = 80;
            string[] lineSplitter = textToSplit.Split(' ');

        Operate:
            string holder = string.Empty;
            var take = lineSplitter.Take(xwords); //Taking xwords words from line1splitter
            foreach (var word in take) { holder += word + " "; }
            holder = holder.Trim(); // removing leading & trailing spaces

            if (holder.Length > maxlength)
            {
                xwords -= 1;
                goto Operate; //reduce word by 1 and go back
            }
            else
            {
                returnValue[0] = holder.Replace(",,", ",");
                var lin2Pre = textToSplit.Substring(holder.Length);
                returnValue[1] = !string.IsNullOrEmpty(lin2Pre) ? lin2Pre : "";
            }

            return returnValue;
        }

        public void SaveHistory(int appID, int StaffID, string StaffUsername, string status, string comment)
        {

            application_desk_histories AppDeskHistory = new application_desk_histories()
            {
                application_id = appID,
                UserName = StaffUsername,
                StaffID = StaffID,
                comment = comment,
                status = status,
                date = DateTime.UtcNow.AddHours(1)
            };

            _context.application_desk_histories.Add(AppDeskHistory);
            _context.SaveChanges();
        }

        /*
         * Deleteing a company's document and facility document
         */
        public JsonResult DeleteCompanyDocument(string CompElpsDocID, string DocType, string View)
        {
            var deleteURL = "";

            if (DocType == "Company")
            {
                deleteURL = "CompanyDocument";
            }
            else
            {
                deleteURL = "FacilityDocument";
            }


            var paramData = restSharpServices.parameterData("Id", CompElpsDocID);
            var result = generalClass.RestResult(deleteURL + "/Delete/{Id}", "DELETE", paramData, null, "Document Deleted", null); // DELETE

            if (result.Value.ToString() != "Network Error")
            {
                var getSubmitted = (from u in _context.SubmittedDocuments
                                    join b in _context.ApplicationDocuments on u.AppDocID equals b.AppDocID
                                    where u.CompElpsDocID == Convert.ToInt32(CompElpsDocID) && u.DeletedStatus != true
                                    select u).FirstOrDefault();
                if (getSubmitted != null )
                {
                    if(View == "Reupload")
                    getSubmitted.DocSource = null;
                    else
                    getSubmitted.DeletedStatus = true;
                    
                    _context.SaveChanges();
                }
            }
            return Json(result.Value);
        }

        public IActionResult DeleteAppDocument(int id, int appID)
        {
            var deleteURL = ""; 
            var getDoc = (from sb in _context.SubmittedDocuments
                          join ad in _context.ApplicationDocuments on sb.AppDocID equals ad.AppDocID
                          where sb.SubDocID == id && sb.AppID ==appID
                          select new { sb, ad }).FirstOrDefault();
            if (getDoc != null)
            {
                if (getDoc.ad.docType == "Company")
                {
                    deleteURL = "CompanyDocument";
                }
                else
                {
                    deleteURL = "FacilityDocument";
                }

                var paramData = restSharpServices.parameterData("Id", getDoc.sb.CompElpsDocID.ToString());
                var result = generalClass.RestResult(deleteURL + "/Delete/{Id}", "DELETE", paramData, null, "Document Deleted", null);

                  _context.SubmittedDocuments.Remove(getDoc.sb);
                  _context.SaveChanges();
                  return Json(result.Value);

            }
            return Json("An error occured");

        }

        public int GetProcessingDays(int appid)
        {
            int days = 0;
            var app = _context.applications.Where(x => x.id == appid).FirstOrDefault();

            var desk = _context.MyDesk.Where(x => x.AppId == app.id).AsEnumerable();

            if (app.status.Trim() == GeneralClass.Approved )
            {
                if (desk.Count()>0)
                {
                    days = app.CreatedAt != null ? Convert.ToInt32((desk.LastOrDefault().CreatedAt.Date - app.CreatedAt.Value.Date).TotalDays) : Convert.ToInt32((desk.LastOrDefault().CreatedAt.Date - app.date_added.Date).TotalDays);

                }
            }
            else if (app.status.Trim() == GeneralClass.Processing || app.status.Trim() == GeneralClass.Rejected)
            {
                days = app.CreatedAt!= null? Convert.ToInt32((DateTime.Now.Date - app.CreatedAt.Value.Date).TotalDays) : Convert.ToInt32((DateTime.Now.Date - app.date_added.Date).TotalDays);
            }

            return days;
        }



        public int ProcessingDays(DateTime landedOn, DateTime? processedOn, DateTime? submittedOn)
        {
            int days = 0;

            if (processedOn == null)
            {
                days = Convert.ToInt32((landedOn.Date - submittedOn?.Date)?.TotalDays);
            }
            else
            {
                days = Convert.ToInt32((processedOn?.Date - landedOn.Date)?.TotalDays);
            }
            return days;
        }


        public int getSessionOfficeID()
        {
            try
            {
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
                var getStaff = (from s in _context.Staff
                                join f in _context.FieldOffices on s.FieldOfficeID equals f.FieldOffice_id
                                where s.StaffID == userID
                                select s
                              ).FirstOrDefault();
                if (getStaff != null)
                {
                    return (int)getStaff.FieldOfficeID;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public int getSessionRoleID()
        {
            try
            {
                return Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionRoleID)));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public string getSessionEmail()
        {
            try
            {
                return generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionEmail));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int getSessionLogin()
        {
            try
            {
                int sessionLogin = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionLogin)));
                return sessionLogin;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int getSessionUserID()
        {
            try
            {
                return Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(AuthController.sessionUserID)));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<ApplicationOffice> GetLegacyApplicationOffice(int state_id, bool isHeadOffice)
        {
            List<ApplicationOffice> applicationOffices = new List<ApplicationOffice>();

            applicationOffices = (from s in _context.States_UT.AsEnumerable()
                                  join fo in _context.FieldOfficeStates.AsEnumerable() on s.State_id equals fo.StateId
                                  join of in _context.FieldOffices.AsEnumerable() on fo.FieldOffice_id equals of.FieldOffice_id
                                  join zf in _context.ZonalFieldOffice.AsEnumerable() on of.FieldOffice_id equals zf.FieldOffice_id
                                  join z in _context.ZonalOffice.AsEnumerable() on zf.Zone_id equals z.Zone_id
                                  select new ApplicationOffice
                                  {
                                      StateName = s.StateName,
                                      FieldOfficeId = of.FieldOffice_id,
                                      OfficeName = of.OfficeName,
                                      ZonalOffice = z.ZoneName,
                                      ZonalOfficeId = z.Zone_id,
                                      SateId = s.State_id
                                  }).ToList();

            if (isHeadOffice == true)
            {
                applicationOffices = applicationOffices.Where(x => x.OfficeName.Contains("HEAD OFFICE")).ToList();
            }
            else
            {

                if (state_id > 0)
                {
                    applicationOffices = applicationOffices.Where(x => x.SateId == state_id).ToList();
                }
                else
                {
                    applicationOffices = null;
                }
            }

            return applicationOffices;
        }



        public void LogMessages(string message, string user_id = null)
        {
            var auditTrail = new AuditTrail()
            {
                CreatedAt = DateTime.UtcNow,
                UserID = user_id,
                AuditAction = message
            };

            _context.AuditTrail.Add(auditTrail);
            _context.SaveChanges();

        }

        //} public void LogMessages(string message, string user_id = null)
        //{
        //    var auditTrail = new AuditLogs()
        //    {
        //        EventDateUTC = DateTime.UtcNow.AddHours(1),
        //        UserId = user_id,
        //        NewValue = message
        //    };

        //    _context.AuditLogs.Add(auditTrail);
        //    _context.SaveChanges();
        //}

        public RenewModel GetRenewalModel(Facilities fac, permits pm)
        {

            var atg = _context.ATGs.Where(a => a.FacilityId == fac.Id).FirstOrDefault();

            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));
            LogMessages("get model from Facility with Permit", CompanyID.ToString());
            var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();



            RenewModel renew = new RenewModel()
            {
                Permit = pm,
                Company = comp,
                Facility = fac,
                Products = _context.Products.ToList(),
                Pumps = _context.Pumps.Where(a => a.FacilityId == fac.Id).ToList(),
                Tanks = _context.Tanks.Where(a => a.FacilityId == fac.Id /*&& (a.Status == null || a.Status == GeneralClass.Approved )*/ && a.Decommissioned!= true).ToList(),
                ATGParams = atg != null ? atg.Parameters.Split(';').ToList() : null
            };
            return renew;
        }
        public RenewModel GetRenewalModel(Legacies lg, Facilities fac = null)
        {
            var CompanyName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var CompanyEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int CompanyID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            var comp = _context.companies.Where(x => x.id == CompanyID).FirstOrDefault();
            //var getApp = _context.applications.Where(x => x.id == pm.application_id).FirstOrDefault();
            //var ps = _context.Phases.Where(a => a.id == getApp.PhaseId).FirstOrDefault();

            LogMessages("getting tanks from Legacy", CompanyName);
            var pm = new permits();
            var dtIsue = DateTime.Now;
            if (fac == null)
            {
                fac = new Facilities
                {
                    NoofDriveIn = 1,
                    NoOfDriveOut = 1

                };

            }
            var tnks = new List<Tanks>();
            if (lg != null)
            {
                var OwnerComp = _context.companies.Where(x => x.name.ToLower()==lg.CompName.ToLower() || x.CompanyCode == lg.CompId).FirstOrDefault();
                //pm.company_id = lg.CompId!= null && lg.CompId!= " "? int.Parse(lg?.CompId):int.Parse(OwnerComp?.id.ToString()  );
                pm.company_id = OwnerComp != null ? (int)OwnerComp?.id : 0;
                if (string.IsNullOrEmpty(lg?.Issue_Date))
                {
                    pm.date_issued = DateTime.TryParseExact(lg.Issue_Date, "d/M/yyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtIsue) ? dtIsue : dtIsue;

                }
                pm.permit_no = lg?.LicenseNo;
                pm.categoryName = lg?.AppType;

                if (lg.AGOVol > 0)
                {
                    if (lg.AGO_Tanks <= 0)
                    {
                        lg.AGO_Tanks = 1;
                    }
                    for (int i = 1; i <= lg.AGO_Tanks; i++)
                    {


                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.AGOVol / lg.AGO_Tanks).ToString(),
                            Name = $"AGO Tanks {i}"
                        });
                    }
                }
                if (lg.DPKVol > 0)
                {
                    if (lg.DPK_Tanks <= 0)
                    {
                        lg.DPK_Tanks = 1;
                    }
                    for (int i = 1; i <= lg.DPK_Tanks; i++)
                    {


                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.DPKVol / lg.DPK_Tanks).ToString(),
                            Name = $"DPK Tanks {i}"
                        });
                    }
                }
                if (lg.PMSVol > 0)
                {
                    if (lg.PMS_Tanks <= 0)
                    {
                        lg.PMS_Tanks = 1;
                    }
                    for (int i = 1; i <= lg.PMS_Tanks; i++)
                    {


                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.PMSVol / lg.PMS_Tanks).ToString(),
                            Name = $"PMS Tanks {i}"
                        });
                    }
                }
                if (lg.FuelOilVol > 0)
                {
                    if (lg.FuelOilTanks <= 0)
                    {
                        lg.FuelOilTanks = 1;
                    }
                    for (int i = 1; i <= lg.FuelOilTanks; i++)
                    {


                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.FuelOilVol / lg.FuelOilTanks).ToString(),
                            Name = $"Fuel Oil Tanks {i}"
                        });
                    }
                }
                if (lg.ATKVol > 0)
                {
                    if (lg.ATKTanks <= 0)
                    {
                        lg.ATKTanks = 1;
                    }
                    for (int i = 1; i <= lg.ATKTanks; i++)
                    {


                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.ATKVol / lg.ATKTanks).ToString(),
                            Name = $"ATK Tanks {i}"
                        });
                    }
                }
                if (lg.BaseOilVol > 0)
                {
                    if (lg.BaseOilTanks <= 0)
                    {
                        lg.BaseOilTanks = 1;
                    }
                    for (int i = 1; i <= lg.BaseOilTanks; i++)
                    {


                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.BaseOilVol / lg.BaseOilTanks).ToString(),
                            Name = $"Base Oil Tanks {i}"
                        });
                    }
                }
                if (lg.BitumenVol > 0)
                {
                    if (lg.BitumenTanks <= 0)
                    {
                        lg.BitumenTanks = 1;
                    }
                    for (int i = 1; i <= lg.BitumenTanks; i++)
                    {


                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.BitumenVol / lg.BitumenTanks).ToString(),
                            Name = $"Bitumen Tanks {i}"
                        });
                    }
                }

                if (lg.LubeOilGreaseVol > 0)
                {
                    if (lg.LubeOilGreaseTanks <= 0)
                    {
                        lg.LubeOilGreaseTanks = 1;
                    }
                    for (int i = 1; i <= lg.LubeOilGreaseTanks; i++)
                    {

                        tnks.Add(new Tanks
                        {
                            MaxCapacity = (lg.LubeOilGreaseVol / lg.LubeOilGreaseTanks).ToString(),
                            Name = $"Lube Oil/Grease Tanks {i}"
                        });
                    }
                }

            }
            var cn = lg == null ? "" : lg.CompName;
            LogMessages($"Number of Tanks:: {tnks.Count} for {cn} inside getting model");
            RenewModel renew = new RenewModel()
            {
                Permit = pm,
                Company = comp,
                Facility = fac,
                Products = _context.Products.ToList(),
                Tanks = tnks,
                Pumps = new List<Pumps>()

            };
            return renew;
        }


        //public List<Message> SaveStaffMessage(int AppID, string staffMail, string subject, string content, string StaffElpsID)
        //{

        //    StaffMessages messages = new StaffMessages()
        //    {
        //        AppID = AppID,
        //        Subject = subject,
        //        MesgContent = content,
        //        StaffELPSID = StaffElpsID,
        //        Seen = false,
        //        StaffEmail = staffMail,
        //        CreatedAt = DateTime.UtcNow.AddHours(1)
        //    };
        //    _context.StaffMessages.Add(messages);
        //    _context.SaveChanges();

        //    var msg = GetSMessage(messages.MessageID, messages.StaffELPSID);
        //    return msg;

        //}


        public List<AppMessage> SaveMessage(int AppID, int userID, string subject, string content, string userElpsID, string type)
        {

            messages messages = new messages()
            {
                company_id = type.Contains("ompany") ? userID : 0,
                UserID = userID,
                AppId = AppID,
                subject = subject,
                content = content,
                sender_id = userElpsID,
                read = 0,
                UserType = type,
                date = DateTime.UtcNow.AddHours(1)
            };
            _context.messages.Add(messages);
            _context.SaveChanges();

            var msg = GetMessage(messages.id, Convert.ToInt32(messages.UserID));
            return msg;

        }
        public int AddStaffToMeeting(int id, string[] staffEmail, string usrEmail, string ip)
        {
            try
            {
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                //Id is the ID of the Meeting/Inspection involved
                var ms = _context.MeetingSchedules.Where(a => a.Id == id).FirstOrDefault();
                if (ms != null)
                {


                    foreach (var item in staffEmail)
                    {
                        var staff = _context.Staff.Where(x => x.StaffEmail == item && x.DeleteStatus != true && x.ActiveStatus != false).FirstOrDefault();
                        var m = item;


                        var im = _context.InspectionMeetingAttendees.Where(a => a.StaffEmail.ToLower() == m.ToLower() && a.MeetingScheduleId == id).FirstOrDefault();
                        if (im == null)
                        {
                            im = new InspectionMeetingAttendees
                            {
                                Date = DateTime.Now,
                                MeetingScheduleId = id,
                                StaffEmail = m
                            };
                            _context.InspectionMeetingAttendees.Add(im);
                            _context.SaveChanges();

                            LogMessages(m + " added to inspection meeting schedule successfully", userID.ToString());
                        }
                    }
                    //Staff already opted In/Success
                    return 0;
                }
                //Schedule not Found
                return 1;
            }
            catch (Exception x)
            {
                //server Error
                return 2;
                //Log the error Message
            }
        }

        public List<AppMessage> GetSMessage(int msg_id, string seid)
        {
            List<AppMessage> messages = new List<AppMessage>();
            //var message = from m in _context.StaffMessages
            //              join a in _context.App on m.AppID equals a.App_Id
            //              join cm in _context.Party.AsEnumerable() on a.Party_Id equals cm.partyId
            //              join st in _context.AppStage.AsEnumerable() on (int)a.CurrentStageID equals st.AppStageID
            //              where m.StaffELPSID == seid && m.MessageID == msg_id
            //              select new AppMessage
            //              {
            //                  Subject = m.Subject,
            //                  Content = m.MesgContent,
            //                  RefNo = a == null ? "" : a.AppRefNO,
            //                  Stage = st == null ? "" : st.StageName,
            //                  Status = a == null ? "" : a.Status,
            //                  Seen = m.Seen,
            //                  PartyADetails = cm == null ? "" : cm.CompanyName + " (" + cm.Company_Address + ", " + cm.City + ", " + cm.StateName + ")",
            //                  AppLocation = a.AppLocation,
            //                  PartyBDetails = a == null ? "" : a.PartyBName
            //              };
            return messages;
        }


        public List<AppMessage> GetMessage(int msg_id, int seid)
        {

            var message = from m in _context.messages
                          join a in _context.applications on m.AppId equals a.id
                          join cm in _context.companies on a.company_id equals cm.id
                          join f in _context.Facilities on a.FacilityId equals f.Id
                          join ca in _context.Categories on a.category_id equals ca.id
                          join ph in _context.Phases on a.PhaseId equals ph.id
                          //join st in _context.AppStage.AsEnumerable() on (int)a.CurrentStageID equals st.AppStageID
                          where m.UserID == seid && m.id == msg_id
                          select new AppMessage
                          {
                              Subject = m.subject,
                              Content = m.content,
                              RefNo = a == null ? "" : a.reference,
                              //Stage = st == null ? "" : st.StageName,
                              Status = a == null ? "" : a.status,
                              Seen = m.read,
                              CompanyName = cm == null ? "" : cm.name,
                              FacilityName = f.Name,
                              CategoryName = ca.name,
                              PhaseName = ph.name,
                              //check lk

                              StatutoryLicenceFee = a.fee_payable,
                              ServiceCharge = a.service_charge,
                              TotalAmountDue = a.fee_payable + a.service_charge,
                              ApplicationPeriod = a.date_added,
                              DateSubmitted = a.date_added
                          };
            return message.ToList();
        }

        public async System.Threading.Tasks.Task<string> SendEmailMessageSBJAsync(string emailTo, string fullname, string subject, string content, string option, List<AppMessage> appMessages = null)
        {
            var result = "";
            var password = _configuration.GetSection("MailSetting").GetSection("mailPass").Value.ToString();
            var username = _configuration.GetSection("MailSetting").GetSection("UserName").Value.ToString();
            var emailFrom = _configuration.GetSection("MailSetting").GetSection("mailSender").Value.ToString();
            var Host = _configuration.GetSection("MailSetting").GetSection("mailHost").Value.ToString();
            var Port = Convert.ToInt16( _configuration.GetSection("MailSetting").GetSection("ServerPort").Value.ToString() );


            string msgBody = "";

            //if (option == GeneralClass.DEFAULT_MESSAGE)
            //{
            //    msgBody = MessageTemplate(subject, content, appMessages);
            //}
            if (option == GeneralClass.STAFF_NOTIFY)
            {
                msgBody = StaffMessageTemplateSBJ(subject, content, fullname);
            }
            else if (option == GeneralClass.COMPANY_NOTIFY)
            {
                msgBody = CompanyMessageTemplate(appMessages);
            }


            MailMessage _mail = new MailMessage();
            SmtpClient client = new SmtpClient(Host, Port);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(username, password);
            _mail.From = new MailAddress(emailFrom);
            _mail.To.Add(new MailAddress(emailTo, fullname));
            _mail.Subject = subject;
            _mail.IsBodyHtml = true;
            _mail.Body = msgBody;

            try
            {
                await client.SendMailAsync(_mail);
                result = "OK";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }


        public string SendEmailMessage(string email_to, string email_to_name, List<AppMessage> AppMessages, byte[] attach)
        {
            var result = "";
            var password = _configuration.GetSection("MailSetting").GetSection("mailPass").Value.ToString();
            var username = _configuration.GetSection("MailSetting").GetSection("UserName").Value.ToString();
            var emailFrom = _configuration.GetSection("MailSetting").GetSection("mailSender").Value.ToString();
            var Host = _configuration.GetSection("MailSetting").GetSection("mailHost").Value.ToString();
            var Port = Convert.ToInt16(_configuration.GetSection("MailSetting").GetSection("ServerPort").Value.ToString());

            var msgBody = CompanyMessageTemplate(AppMessages);

            MailMessage _mail = new MailMessage();
            SmtpClient client = new SmtpClient(Host, Port);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(username, password);
            _mail.From = new MailAddress(emailFrom);
            _mail.To.Add(new MailAddress(email_to, email_to_name));
            _mail.Subject = AppMessages.FirstOrDefault().Subject.ToString();
            _mail.IsBodyHtml = true;
            _mail.Body = msgBody;
            if (attach != null)
            {
                string name = "App Letter";
                Attachment at = new Attachment(new MemoryStream(attach), name);
                _mail.Attachments.Add(at);
            }
            //_mail.CC=
            try
            {
                client.Send(_mail);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string SendEmailMessage2Staff(string email_to, string email_to_name, List<AppMessage> AppMessages, byte[] attach)
        {
            var result = "";
            var password = _configuration.GetSection("MailSetting").GetSection("mailPass").Value.ToString();
            var username = _configuration.GetSection("MailSetting").GetSection("UserName").Value.ToString();
            var emailFrom = _configuration.GetSection("MailSetting").GetSection("mailSender").Value.ToString();
            var Host = _configuration.GetSection("MailSetting").GetSection("mailHost").Value.ToString();
            var Port = Convert.ToInt16(_configuration.GetSection("MailSetting").GetSection("ServerPort").Value.ToString());

            var msgBody = StaffMessageTemplate(AppMessages, email_to_name);

            MailMessage _mail = new MailMessage();
            SmtpClient client = new SmtpClient(Host, Port);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(username, password);
            _mail.From = new MailAddress(emailFrom);
            _mail.To.Add(new MailAddress(email_to, email_to_name));
            _mail.Subject = AppMessages.FirstOrDefault().Subject.ToString();
            _mail.IsBodyHtml = true;
            _mail.Body = msgBody;
            if (attach != null)
            {
                string name = "App Letter";
                Attachment at = new Attachment(new MemoryStream(attach), name);
                _mail.Attachments.Add(at);
            }
            //_mail.CC=
            try
            {

                client.Send(_mail);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string SendEmailMessageDEP(string email_to, string email_to_name, List<AppMessage> AppMessages, byte[] attach)
        {
            var result = "";
            var password = _configuration.GetSection("MailSetting").GetSection("mailPass").Value.ToString();
            var username = _configuration.GetSection("MailSetting").GetSection("UserName").Value.ToString();
            var emailFrom = _configuration.GetSection("MailSetting").GetSection("mailSender").Value.ToString();
            var Host = _configuration.GetSection("MailSetting").GetSection("mailHost").Value.ToString();
            var Port = Convert.ToInt16(_configuration.GetSection("MailSetting").GetSection("ServerPort").Value.ToString());

            var msgBody = CompanyMessageTemplate(AppMessages);

            MailMessage _mail = new MailMessage();
            SmtpClient client = new SmtpClient(Host, Port);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(username, password);
            _mail.From = new MailAddress(emailFrom);
            _mail.To.Add(new MailAddress(email_to, email_to_name));
            _mail.Subject = AppMessages.FirstOrDefault().Subject.ToString();
            _mail.IsBodyHtml = true;
            _mail.Body = msgBody;
            if (attach != null)
            {
                string name = "App Letter";
                Attachment at = new Attachment(new MemoryStream(attach), name);
                _mail.Attachments.Add(at);
            }
            //_mail.CC=
            try
            {

                client.Send(_mail);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public byte[] addByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }

        /*
       * Email HTML template
       */
      
        public string CompanyMessageTemplate(List<AppMessage> AppMessages)
        {
            var msg = AppMessages.FirstOrDefault();
            string body = "<div>";


            //body += "<div style='width: 800px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='/*~/images/nmdpra.png*/' alt='Logo'/></div>";

            body += "<div style='width: 800px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='~/images/nmdpra.png' alt='Logo'/></div>";
            body += "<div class='text-left' style='background-color: #ece8d4; width: 800px; min-height: 200px;'>";
            body += "<div style='padding: 10px 30px 30px 30px;'>";
            body += "<h5 style='text-align: center; font-weight: 300; padding-bottom: 10px; border-bottom: 1px solid #ddd;'>" + msg.Subject + "</h5>";
            body += "<p>Dear Sir/Madam,</p>";
            body += "<p style='line-height: 30px; text-align: justify;'>" + msg.Content + "</p>";

            body += "<table style = 'width: 100%;'><tbody>";
            body += "<tr><td style='width: 200px;'><strong>App Ref No:</strong></td><td> " + msg.RefNo + " </td></tr>";
            body += "<tr><td style='width: 200px;'><strong>Company Name:</strong></td><td> " + msg.CompanyName + " </td></tr>";
            body += "<tr><td><strong>Facility Name:</strong></td><td> " + msg.FacilityName + " </td></tr>";
            body += "<tr><td><strong>Statutory Licence Fee:</strong></td><td> " + msg.StatutoryLicenceFee + " </td></tr>";
            body += "<tr><td><strong>Service Charge:</strong></td><td> " + msg.ServiceCharge + " </td></tr>";
            body += "<tr><td><strong>Total Amount Due:</strong></td><td> " + msg.TotalAmountDue + " </td></tr>";
            body += "<tr><td><strong>Application Period:</strong></td><td> " + msg.ApplicationPeriod + " </td></tr>";
            body += "<tr><td><strong>Category & Phase:</strong></td><td> " + msg.CategoryName + '(' + msg.PhaseName + ')' + "</td></tr>";
            body += "<tr><td><strong>Date Submitted:</strong></td><td> " + msg.DateSubmitted + " </td></tr>";
            body += "<tr><td><strong>Status:</strong></td><td> " + msg.Status + " </td></tr>";
            body += "</tbody></table><br/>";

            body += "<p> </p>";
            body += "<p>Nigerian Midstream and Downstream Petroleum Regulatory Authority<br/> <small>(PDJ) </small></p> </div>";
            body += "<div style='padding:10px 0 10px; 10px; background-color:#888; color:#f9f9f9; width:800px;'> &copy; " + DateTime.UtcNow.AddHours(1).Year + " Nigerian Midstream and Downstream Petroleum Regulatory Authority &minus; NMDPRA Nigeria</div></div></div>";

            return body;
        }


        /*
         *Staff Email HTML template
         */
        public string StaffMessageTemplate(List<AppMessage> AppMessages, string StaffFN)
        {
            var msg = AppMessages.FirstOrDefault();
            string body = "<div>";
            body += "<div style='width: 800px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='~/images/nmdpra.png' alt='Logo'/></div>";
            body += "<div class='text-left' style='background-color: #ece8d4; width: 800px; min-height: 200px;'>";
            body += "<div style='padding: 10px 30px 30px 30px;'>";
            body += "<h5 style='text-align: center; font-weight: 300; padding-bottom: 10px; border-bottom: 1px solid #ddd;'>" + msg.Subject + "</h5>";
            body += "<p>Dear " + StaffFN + ",</p>";
            body += "<p style='line-height: 30px; text-align: justify;'>" + msg.Content + "</p>";
            body += "<table style = 'width: 100%;'><tbody>";
            body += "<tr><td style='width: 200px;'><strong>App Ref No:</strong></td><td> " + msg.RefNo + " </td></tr>";
            // body += "<tr><td><strong>Stage:</strong></td><td> " + msg.Stage + " </td></tr>";
            body += "</tbody></table><br/>";

            body += "<p>Nigerian Midstream and Downstream Petroleum Regulatory Authority<br/> <small>(PDJ) </small></p> </div>";
            body += "<div style='padding:10px 0 10px; 10px; background-color:#888; color:#f9f9f9; width:800px;'> &copy; " + DateTime.UtcNow.AddHours(1).Year + " Nigerian Midstream and Downstream Petroleum Regulatory Authority &minus; NMDPRA Nigeria</div></div></div>";

            return body;
        }
        public string StaffMessageTemplateSBJ(string subject, string content, string StaffFN)
        {
            string body = "<div>";
            body += "<div style='width: 800px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='~/images/nmdpra.png' alt='Logo'/></div>";
            body += "<div class='text-left' style='background-color: #ece8d4; width: 800px; min-height: 200px;'>";
            body += "<div style='padding: 10px 30px 30px 30px;'>";
            body += "<h5 style='text-align: center; font-weight: 300; padding-bottom: 10px; border-bottom: 1px solid #ddd;'>" + subject + "</h5>";
            body += "<p>Dear " + StaffFN + ",</p>";
            body += "<p style='line-height: 30px; text-align: justify;'>" + content + "</p>";
            body += "<table style = 'width: 100%;'><tbody>";
            body += "</tbody></table><br/>";

            body += "<p>Nigerian Midstream and Downstream Petroleum Regulatory Authority<br/> <small>(PDJ) </small></p> </div>";
            body += "<div style='padding:10px 0 10px; 10px; background-color:#888; color:#f9f9f9; width:800px;'> &copy; " + DateTime.UtcNow.AddHours(1).Year + " Nigerian Midstream and Downstream Petroleum Regulatory Authority &minus; NMDPRA Nigeria</div></div></div>";

            return body;
        }

       


        /*
         * Encrypting App ID and processing rule id
         * 
         */
        public JsonResult GetEncrypt(string App_id, string process_id)
        {
            var result = "";

            string AppID = generalClass.Encrypt(App_id.Trim());
            string ProcessID = generalClass.Encrypt(process_id);

            string dAppID = generalClass.Decrypt(AppID);
            string dProcessID = generalClass.Decrypt(ProcessID);

            result = AppID + "|" + ProcessID;

            return Json(result.Trim());
        }


        /*
       * Encrypting App Request ID and processing rule id
       * 
       */
        public JsonResult GetEncrypt2(string App_id, string process_id)
        {
            var result = "";

            string AppReqID = generalClass.Encrypt(App_id.Trim());
            string ProcessID = generalClass.Encrypt(process_id);

            string dAppReqID
                = generalClass.Decrypt(AppReqID);
            string dProcessID = generalClass.Decrypt(ProcessID);

            result = AppReqID + "|" + ProcessID;

            return Json(result.Trim());
        }



        /*
         * Updating App status to ELPS
         */
        public bool UpdateElpsApp(List<NewDepot.Models.applications> Apps)
        {
            bool result = false;

            var App = from a in _context.applications
                      join c in _context.companies on a.company_id equals c.id
                      where a.id == Apps.FirstOrDefault().id
                      && a.DeleteStatus != true
                      select new
                      {
                          a,
                          c
                      };

            if (App.Count() > 0)
            {
                //    var paramData = restSharpServices.parameterData("orderId", App.FirstOrDefault().a.AppRefNO);
                //    var elpsApp = restSharpServices.Response("/api/App/ByOrderId/{orderId}/{email}/{apiHash}", paramData, "GET");

                //    if (elpsApp.IsSuccessful == true)
                //    {
                //        var resp = JsonConvert.DeserializeObject<JObject>(elpsApp.Content);

                //        var values = new JObject();
                //        values.Add("orderId", App.FirstOrDefault().a.AppRefNO);
                //        values.Add("company_Id", App.FirstOrDefault().c.CompanyElpsID);
                //        values.Add("status", App.FirstOrDefault().a.Status);
                //        values.Add("date", DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                //        values.Add("categoryName", (string)resp.SelectToken("categoryName"));
                //        values.Add("licenseName", (string)resp.SelectToken("licenseName"));
                //        values.Add("licenseId", (string)resp.SelectToken("licenseId"));
                //        values.Add("id", (string)resp.SelectToken("id"));

                //        var updateApp = restSharpServices.Response("api/App/{email}/{apiHash}", null, "PUT", values);

                //        if (updateApp.IsSuccessful == true)
                //        {
                //            result = true;
                //        }
                //        else
                //        {
                //            result = false;
                //        }
                //    }
                //    else
                //    {
                //        result = false;
                //    }
            }
            else
            {
                result = false;
            }

            return result;
        }



        /*
         * Posting Approved permit to elps.
         */
        public bool PostPermitToElps(int permitID, string permitNO, string OrderID, int ElpsCompID, DateTime issuedDate, DateTime expiryDate, bool isRenew)
        {
            var values = new JObject();
            values.Add("permit_No", permitNO);
            values.Add("orderId", OrderID);
            values.Add("company_Id", ElpsCompID.ToString());
            values.Add("date_Issued", issuedDate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            values.Add("date_Expire", expiryDate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            values.Add("categoryName", "PDJ");
            values.Add("is_Renewed", (isRenew == true) ? "Yes" : "No");
            values.Add("licenseId", permitID.ToString());
            values.Add("id", 0);

            List<JObject> newDocs = new List<JObject>();

            var paramData = restSharpServices.parameterData("CompId", ElpsCompID.ToString());
            var savePermit = restSharpServices.Response("api/Permits/{CompId}/{email}/{apiHash}", paramData, "POST", values);

            if (savePermit.IsSuccessful)
            {
                JObject eplsPermit = JsonConvert.DeserializeObject<JObject>(savePermit.Content);

                return true;
            }
            else
            {
                return false;
            }
        }
        public JsonResult GetCompany(string CompanyName = null, int CompanyId = 0, string CompanyAddress = null, string CompanyEmail = null, string partyA = null, bool deleteStatus = false)
        {
            var comp = (from s in _context.companies
                        where s.name != CompanyName && s.DeleteStatus == deleteStatus
                        select new
                        {
                            companyName = s.name,
                            CompanyId = s.id,
                            Address = s.Address,
                            Email = s.CompanyEmail,
                            createAt = s.CreatedAt,
                            updatedAt = s.UpdatedAt,
                            deleteStatus = s.DeleteStatus,
                            deletedAt = s.DeletedAt,
                            deletedBy = s.DeletedBy,
                        });

            comp = CompanyName != null ? comp.Where(s => s.companyName == CompanyName.ToUpper()) :
                     CompanyId != 0 ? comp.Where(s => s.CompanyId == CompanyId) :
                     CompanyEmail != null ? comp.Where(s => s.Email == CompanyEmail.ToUpper()) :
                     CompanyAddress != null ? comp.Where(s => s.Address == CompanyAddress) :
                     comp.Where(s => s.deleteStatus == deleteStatus);

            return Json(comp.ToList());
        }

        /*
        * Getting all states with a particular country, state id, state name or country id
        */
        public JsonResult GetAllStatesFromCountry(int CountryID = 0, int StateID = 0, string StateName = null, string CountryName = null, bool deleteStatus = false)
        {
            var states = (from s in _context.States_UT
                          join c in _context.countries on s.Country_id  equals c.id
                          orderby s.StateName
                          select new
                          {
                              countryName = c.name,
                              countryID = c.id,
                              stateID = s.State_id,
                              stateName = s.StateName,
                              createAt = s.CreatedAt,
                              updatedAt = s.UpdatedAt,
                              deleteStatus = s.DeleteStatus,
                              deletedAt = s.DeletedAt,
                              deletedBy = s.DeletedBy,
                          });

            states = CountryName != null ? states.Where(s => s.countryName == CountryName) :
                     CountryID != 0 ? states.Where(s => s.countryID == CountryID) :
                     StateName != null ? states.Where(s => s.stateName == StateName) :
                     StateID != 0 ? states.Where(s => s.stateID == StateID) :
                     states.Where(s => s.deleteStatus == deleteStatus);

            return Json(states.ToList());
        }

        /*
 * Get All states by country id
 */
        public JsonResult GetStates(string CountryId)
        {
          
            var states = _context.States_UT.Where(x => CountryId == x.Country_id.ToString() && x.DeleteStatus!= true).ToList();
            if(states.Count() > 0)
            {
                return Json(states);
            }
            var paramData = restSharpServices.parameterData("Id", CountryId);
            var result = restSharpServices.Response("Address/states/{Id}", paramData, "GET", null, null); // GET
            return Json(result.Content);
        }
        public JsonResult GetProducts(int id)
        {
            var prods = _context.Products.ToList();
            
                return Json(prods);
            
        }
        /*
         * Getting all zones with ID or Name
         */
        public JsonResult GetAllZones(int zonalID = 0, string zonalName = null, bool deletedStatus = false)
        {
            var zones = (from z in _context.ZonalOffice where z.DeleteStatus != true select z);

            zones = zonalName != null ? zones.Where(z => z.ZoneName == zonalName.ToUpper()) :
                    zonalID != 0 ? zones.Where(z => z.Zone_id == zonalID) :
                    zones.Where(z => z.DeleteStatus == deletedStatus);

            return Json(zones.ToList());
        }



        /*
         * Getting all field office with ID or Name
         */
        //public JsonResult GetAllFieldOffice(int OfficeID = 0, string FieldOfficeName = null, bool deletedStatus = false)
        //{
        //    var _fieldOffice = (from z in _context. where z.DeleteStatus == deletedStatus select z);

        //    _fieldOffice = FieldOfficeName != null ? _fieldOffice.Where(z => z.OfficeName == FieldOfficeName.ToUpper()) :
        //            OfficeID != 0 ? _fieldOffice.Where(z => z.FieldOfficeID == OfficeID) :
        //            _fieldOffice.Where(z => z.DeleteStatus == deletedStatus);

        //    return Json(_fieldOffice.ToList());
        //}



        /*
        * Getting all company document types from elps to save specific
        */
        public Document GetDocumentDetail(string DocID)
        {
            Document documents = new Document();

            var paramData = restSharpServices.parameterData("id", DocID);
            var response = restSharpServices.Response("api/CompanyDocument/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful == false)
            {
                documents = null;
            }
            else
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content);
                documents = JsonConvert.DeserializeObject<Document>(response.Content);
            }
            return documents;
        }


        public JsonResult GetElpsDocumentsTypes()
        {
            var result = generalClass.RestResult("Documents/Types", "GET", null, null, null); // GET
        
            return Json(result.Value.ToString());
        }
        /*
       * Getting all company document types from elps to save specific
       */
        public JsonResult GetElpsDocumentsTypess()
        {
            var result = generalClass.RestResult2("Documents/Types", "GET", null, null, null); // GET
            if (!result.Value.ToString().ToLower().Contains("error"))
            {
                var r = JsonConvert.DeserializeObject<List<FacilityDoc>>(result.Value.ToString());
                return Json(r);
            }
            else
            {
                return Json(result.Value.ToString());
            }
        }
        /*
  * Getting the list of application processes
  */
        public List<WorkProccess> GetAppProcess(int AppPhaseID, int AppCategoryID, int ProcessID = 0, int Sort = 0)
        {
            var process = (from c in _context.WorkProccess
                           where c.PhaseID == AppPhaseID && c.CategoryID == AppCategoryID && c.DeleteStatus != true
                           select c);

            if (ProcessID != 0)
            {
                process = process.Where(x => x.ProccessID == ProcessID);
            }
            else if (Sort != 0)
            {
                process = process.Where(x => x.Sort == Sort);
            }
            else if (ProcessID != 0 && Sort != 0)
            {
                process = process.Where(x => x.ProccessID == ProcessID && x.Sort == Sort);
            }

            process = process.OrderBy(x => x.Sort);

            return process.ToList();
        }


        /*
       *  Application process
       */
  
        public int ApplicationDropStaff(int appid, int AppPhaseID, int AppCategoryID, int StateID, int Sort, string ModificationType=null)
               {
            List<DropStaff> dropStaffs = new List<DropStaff>();
            string rolename = "";
            if (string.IsNullOrWhiteSpace(AppPhaseID.ToString()) || string.IsNullOrWhiteSpace(AppCategoryID.ToString()))
            {
                dropStaffs = null;
            }
            else
            {
                var getProccess = _context.WorkProccess.Where(x => (x.PhaseID == AppPhaseID && x.CategoryID == AppCategoryID && x.Sort == (Sort + 1)) && x.DeleteStatus != true);

                if (getProccess.Count() > 0)
                {
                    var userRole = (from r in _context.UserRoles where r.Role_id == getProccess.FirstOrDefault().RoleID
                                    select r).FirstOrDefault();
                    rolename = userRole.RoleName;
                    // get staff role for the process;
                    var getSaff = (from s in _context.Staff.AsEnumerable()
                                   join r in _context.UserRoles.AsEnumerable() on s.RoleID equals r.Role_id
                                   join z in _context.ZonalFieldOffice.AsEnumerable() on s.FieldOfficeID equals z.FieldOffice_id
                                   where ((s.RoleID == getProccess.FirstOrDefault().RoleID && s.LocationID == getProccess.FirstOrDefault().LocationID
                                   && s.DeleteStatus != true && s.ActiveStatus == true))
                                   select new DropStaff
                                   {
                                       FieldOfficeId = (int)s.FieldOfficeID,
                                       StaffId = s.StaffID,
                                       Role = r.RoleName,
                                       Sort = getProccess.FirstOrDefault().Sort,
                                       ZonalOfficeId = z.Zone_id,
                                       ProcessId = getProccess.FirstOrDefault().ProccessID,
                                       DeskCount = _context.MyDesk.Where(x => x.StaffID == s.StaffID && x.HasWork != true).Count(),
                                      // Process = getProccess.FirstOrDefault().ac
                                   }).ToList();

                    if (getSaff.Count() > 0)
                    {
                        var minDeskCount = getSaff.Min(x => x.DeskCount);
                        var getLocation = _context.Location.Where(x => x.LocationID == getProccess.FirstOrDefault().LocationID && x.DeleteStatus != true);


                        // find the field office the application belongs to;
                        var fieldApp = this.GetApplicationOffice(appid);

                        if (fieldApp.Count() > 0)
                        {
                            if (getLocation.FirstOrDefault().LocationName == "HQ")
                            {
                                fieldApp = fieldApp.Where(x => x.OfficeName.Contains("HEAD")).ToList();

                                if (fieldApp.Count() <= 0)
                                {
                                    fieldApp = (from of in _context.FieldOffices.AsEnumerable()

                                                join zf in _context.ZonalFieldOffice.AsEnumerable() on of.FieldOffice_id equals zf.FieldOffice_id
                                                join z in _context.ZonalOffice.AsEnumerable() on zf.Zone_id equals z.Zone_id
                                                where of.OfficeName.Contains("HEAD")
                                                select new ApplicationOffice
                                                {
                                                    OfficeName = of.OfficeName,
                                                    FieldOfficeId = of.FieldOffice_id,
                                                    ZonalOffice = z.ZoneName,
                                                    ZonalOfficeId = z.Zone_id,
                                                }).ToList();
                                }
                            }
                            else
                            {
                                fieldApp = fieldApp.Where(x => !x.OfficeName.Contains("HEAD")).ToList();
                            }
                        }


                        if (fieldApp.Count() > 0)
                        {

                            if (getLocation.Count() > 0)
                            {
                                if (getLocation.FirstOrDefault().LocationName == "FO")
                                {
                                    dropStaffs = getSaff.Where(x => x.FieldOfficeId == fieldApp.FirstOrDefault().FieldOfficeId && x.Role == rolename).OrderByDescending(x => x.DeskCount).ToList();
                                }
                                else if (getLocation.FirstOrDefault().LocationName == "ZO")
                                {
                                    dropStaffs = getSaff.Where(x => x.Role == rolename && x.ZonalOfficeId == fieldApp.FirstOrDefault().ZonalOfficeId).OrderByDescending(x => x.DeskCount).ToList();
                                }
                                else if (getLocation.FirstOrDefault().LocationName == "HQ")
                                {
                                    dropStaffs = getSaff.Where(x => x.Role == rolename).OrderByDescending(x => x.DeskCount).ToList();
                                }
                                else
                                {
                                    dropStaffs = null;
                                }
                            }
                            else
                            {
                                dropStaffs = null;
                            }
                        }
                        else
                        {
                            dropStaffs = null;
                        }
                    }
                }
            }
            if (dropStaffs.FirstOrDefault() != null)
            {
                return dropStaffs.FirstOrDefault().StaffId;
            }
            else
            {

                return 0;
            }
        }


        //Get Application Field Office ID
        public List<ApplicationOffice> GetApplicationOffice(int appid)
        {
            List<ApplicationOffice> applicationOffices = new List<ApplicationOffice>();

            if (appid > 0)
            {
                applicationOffices = (from a in _context.applications.AsEnumerable()
                                      join f in _context.Facilities.AsEnumerable() on a.FacilityId equals f.Id
                                      join ad in _context.addresses on f.AddressId equals ad.id
                                      join s in _context.States_UT.AsEnumerable() on ad.StateId equals s.State_id
                                      join fo in _context.FieldOfficeStates.AsEnumerable() on s.State_id equals fo.StateId
                                      join of in _context.FieldOffices.AsEnumerable() on fo.FieldOffice_id equals of.FieldOffice_id
                                      join zf in _context.ZonalFieldOffice.AsEnumerable() on of.FieldOffice_id equals zf.FieldOffice_id
                                      join z in _context.ZonalOffice.AsEnumerable() on zf.Zone_id equals z.Zone_id
                                      where a.id == appid && a.DeleteStatus != true && a.isLegacy !=true
                                      select new ApplicationOffice
                                      {
                                          StateName = s.StateName,
                                          ZonalOrField= of.FieldType,
                                          FieldOfficeId = of.FieldOffice_id,
                                          OfficeName = of.OfficeName,
                                          ZonalOffice = z.ZoneName,
                                          ZonalOfficeId = z.Zone_id,
                                          SateId = s.State_id
                                      }).ToList();
            }
            else
            {
                applicationOffices = null;
            }

            return applicationOffices;
        }
        //public static ApplicationOffice GetApplicationOffices(int appid)
        //{
        //   ApplicationOffice applicationOffices = new ApplicationOffice();
        //    var _context = new Depot_DBContext();
        //    if (appid > 0)
        //    {
        //        applicationOffices = (from a in _context.applications
        //                              join f in _context.Facilities on a.FacilityId equals f.Id
        //                              join ad in _context.addresses on f.AddressId equals ad.id
        //                              join s in _context.States_UT on ad.StateId equals s.State_id
        //                              join fo in _context.FieldOfficeStates on s.State_id equals fo.StateId
        //                              join of in _context.FieldOffices on fo.FieldOffice_id equals of.FieldOffice_id
        //                              join zf in _context.ZonalFieldOffice on of.FieldOffice_id equals zf.FieldOffice_id
        //                              join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
        //                              where a.id == appid && a.DeleteStatus != true && a.isLegacy !=true
        //                              select new ApplicationOffice
        //                              {
        //                                  StateName = s.StateName,
        //                                  ZonalOrField= of.FieldType,
        //                                  FieldOfficeId = of.FieldOffice_id,
        //                                  OfficeName = of.OfficeName,
        //                                  ZonalOffice = z.ZoneName,
        //                                  ZonalOfficeId = z.Zone_id,
        //                                  SateId = s.State_id
        //                              }).FirstOrDefault();
        //    }
        //    else
        //    {
        //        applicationOffices = null;
        //    }

        //    return applicationOffices;
        //}




        public int ApplicationDropStaffy(int AppPhaseID, int AppCategoryID, int StateID, int Sort)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(AppPhaseID.ToString()) || string.IsNullOrWhiteSpace(AppCategoryID.ToString()))
            {
                result = 0;
            }
            else
            {
                var getProccess = _context.WorkProccess.Where(x => (x.PhaseID == AppPhaseID && x.CategoryID == AppCategoryID && x.Sort == (Sort +1)) && x.DeleteStatus != true);

                if (getProccess.Count() > 0)
                {
                    var getSaff = _context.Staff.Where(x => x.RoleID == getProccess.FirstOrDefault().RoleID && x.LocationID == getProccess.FirstOrDefault().LocationID && x.DeleteStatus != true && x.ActiveStatus == true);
                    var staffLocation = _context.Location.Where(x => x.LocationID == getProccess.FirstOrDefault().LocationID).FirstOrDefault();

                    if (getSaff.Count() > 0 && staffLocation!=null)
                    {

                        if (staffLocation.LocationName.ToLower().Contains("hq"))
                        {
                            result = getSaff.FirstOrDefault().StaffID;

                        }

                        else
                        {
                            List<Staff> AllStaff = new List<Staff>();
                            var state = _context.States_UT.Where(x => x.State_id == StateID);

                            List<int> staffFieldOffices = FieldOfficeStaff(state.FirstOrDefault().State_id);
                            
                           
                             foreach (var fieldOffice in staffFieldOffices)
                             {
                                foreach (var s in  getSaff.Where(x => x.FieldOfficeID == fieldOffice))
                                {
                                    AllStaff.Add(s);
                                        
                                }
                            }

                             if(AllStaff.Count > 0)
                            {
                                var getStaffDesk= (from u in AllStaff
                                                  select new
                                                  {
                                                      StaffID = u.StaffID,
                                                      DeskCount = _context.MyDesk.Where(c => c.StaffID == u.StaffID && c.HasWork != true).Count(),

                                                  }).ToList();
                                int min = getStaffDesk.Min(e => e.DeskCount);
                                var staffWithLowJob = getStaffDesk.Where(x => x.DeskCount == min).FirstOrDefault();

                                result = staffWithLowJob.StaffID;
                            }

                        }

                   
                    }
                    else
                    {
                        result = 0;
                    }
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }


        ////[Authorize(Policy = "AllStaffRoles")]
        public JsonResult GetStaffs(int stafid)
        {
            int staff_id = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionuserId")).Trim());
            var staff = _context.Staff.Where(x => x.DeleteStatus != true && x.ActiveStatus == true && x.StaffID != staff_id);
            //&& x.FieldOfficeID == getStaffFieldOfice.FirstOrDefault().FieldOfficeID);
            return Json(staff.ToList());
        }

        ////[Authorize(Policy = "AllStaffRoles")]
        public JsonResult GetApp(int id)
        {
            var App = _context.applications.Where(x => x.id == id && x.DeleteStatus != true);
            //&& x.FieldOfficeID == getStaffFieldOfice.FirstOrDefault().FieldOfficeID);
            return Json(App.ToList());
        }

        /*
        * Getting all facility document types from elps to save specific
        */
        


        /*
        * Getting all App Documents from local DataBase
        */
        public JsonResult GetAppDocs(int DocID = 0, bool deletedStatus = false)
        {
           
            return Json(DocID);
        }

        //public List<Company_Document> GetCompanyDocuments(int id, string type = "")
        //{
        //    var compDocs = new List<Company_Document>();
        //    using (LocalWebClient client = new LocalWebClient())
        //    {
        //        client.Headers.Add(HttpRequestHeader.Accept, "application/json");
        //        var Url = "";
        //        if (string.IsNullOrEmpty(type) || type.ToLower().Trim() == "company")
        //        {
        //            Url = ApiBaseUrl + "CompanyDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;
        //        }
        //        else
        //        {
        //            Url = ApiBaseUrl + "FacilityDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;
        //        }
        //        UtilityHelper.LogMessages(Url);
        //        var output = client.DownloadString(Url);
        //        compDocs = JsonConvert.DeserializeObject<List<Company_Document>>(output);
        //    }

        //    return compDocs;
        //}
        // Get Company Documents
        public List<Company_Document> GetCompanyDocuments(string companyID)
        {
            List<Company_Document> documents = new List<Company_Document>();

            var paramData = restSharpServices.parameterData("id", companyID);
            var response = restSharpServices.Response("/api/CompanyDocuments/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful != true)
            {
                documents = null;
            }
            else
            {
                documents = JsonConvert.DeserializeObject<List<Company_Document>>(response.Content);
            }
            return documents;
        }

        /*
         * Getting single company document
         */
        public Company_Document GetCompanyDocument(string compElpsDocID)
        {
            Company_Document documents = new Company_Document();

            var paramData = restSharpServices.parameterData("id", compElpsDocID);
            var response = restSharpServices.Response("/api/CompanyDocument/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful != true)
            {
                documents = null;
            }
            else
            {
                documents = JsonConvert.DeserializeObject<Company_Document>(response.Content);
            }
            return documents;
        }


        /*
         * Getting single Facility document
         */
        public company_documents getFacilityDocument(string FacilityDocID)
        {
            company_documents facilityDocuments = new company_documents();

            var paramData = restSharpServices.parameterData("id", FacilityDocID);
            var response = restSharpServices.Response("/api/FacilityFiles/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful != true)
            {
                facilityDocuments = null;
            }
            else
            {
                facilityDocuments = JsonConvert.DeserializeObject<company_documents>(response.Content);
            }

            return facilityDocuments;
        }



        // Get Facility Documents
        public List<FacilityDocument> getFacilityDocuments(string facilityID)
        {
            List<FacilityDocument> facilityDocuments = new List<FacilityDocument>();

            var paramData = restSharpServices.parameterData("id", facilityID);
            var response = restSharpServices.Response("/api/FacilityFiles/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful != true)
            {
                facilityDocuments = null;
            }
            else
            {
                facilityDocuments = JsonConvert.DeserializeObject<List<FacilityDocument>>(response.Content);
            }

            return facilityDocuments;
        }


        /*
         * Getting all App Stages
         */
        //public JsonResult GetAppStages(int StageID = 0, bool deletedStatus = false)
        //{
        //    var _stage = (from z in _context.AppStage where z.DeleteStatus == deletedStatus select z);
        //    _stage = StageID != 0 ? _stage.Where(z => z.AppStageID == StageID) :
        //      _stage.Where(z => z.DeleteStatus == deletedStatus);

        //    return Json(_stage.ToList());
        //}



        /*
        * Getting all Location for staff
        */

        /*
        * Getting all Location for staff
        */
       
        public JsonResult GetStafff()
        {
            int staff_id = getSessionUserID();
            var getStaffFieldOfice = _context.Staff.AsEnumerable().Where(x => x.StaffID == staff_id);
            var staff = _context.Staff.AsEnumerable().Where(x => x.DeleteStatus == false && x.ActiveStatus == true && x.StaffID != staff_id && x.FieldOfficeID == getStaffFieldOfice.FirstOrDefault()?.FieldOfficeID);
            return Json(staff.ToList());
        }


        /*
         * Updating application status to ELPS
         */
        public bool UpdateElpsApplication(List<applications> apps)
        {
            bool result = false;

            var app = from a in _context.applications
                      join c in _context.companies on a.company_id equals c.id
                      where a.id == apps.FirstOrDefault().id && a.DeleteStatus != true
                      select new
                      {
                          a,
                          c
                      };

            if (app.Count() > 0)
            {
                var paramData = restSharpServices.parameterData("orderId", app.FirstOrDefault().a.reference);
                var elpsApp = restSharpServices.Response("/api/Application/ByOrderId/{orderId}/{email}/{apiHash}", paramData, "GET");

                if (elpsApp.IsSuccessful == true)
                {
                    var resp = JsonConvert.DeserializeObject<JObject>(elpsApp.Content);

                    if (resp != null)
                    {
                        var values = new JObject();
                        values.Add("orderId", app.FirstOrDefault().a.reference);
                        values.Add("company_Id", app.FirstOrDefault().c.elps_id);
                        values.Add("status", app.FirstOrDefault().a.status);
                        values.Add("date", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                        values.Add("categoryName", (string)resp.SelectToken("categoryName"));
                        values.Add("licenseName", (string)resp.SelectToken("licenseName"));
                        values.Add("licenseId", (string)resp.SelectToken("licenseId"));
                        values.Add("id", (string)resp.SelectToken("id"));

                        var updateApp = restSharpServices.Response("api/Application/{email}/{apiHash}", null, "PUT", values);

                        if (updateApp.IsSuccessful == true)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }





        /*
         * Get All states by country id
         */
        //public JsonResult GetStates(string CountryId)
        //{
        //    var paramData = restSharpServices.parameterData("Id", CountryId);
        //    var result = generalClass.RestResult("Address/states/{Id}", "GET", paramData, null, null); // GET
        //    return Json(result.Value);
        //}
        //}



        /*
        * Getting all facility document types from elps to save specific
        */
        public JsonResult GetElpsFacDocumentsTypes()
        {
            var paramData = restSharpServices.parameterData("Type", "facility");
            var result = generalClass.RestResult("Documents/Facility", "GET", paramData, null, null, "/{Type}"); // GET
            return Json(result.Value);
        }
        public JsonResult GetElpsFacDocumentsTypess()
        {
            var paramData = restSharpServices.parameterData("Type", "facility");
            var result = generalClass.RestResult2("Documents/Facility", "GET", paramData, null, null, "/{Type}"); // GET
             //var r = JsonConvert.DeserializeObject<List<FacilityDoc>>(result.Value.ToString());
            if (!result.Value.ToString().ToLower().Contains("error"))
            {
                var r = JsonConvert.DeserializeObject<List<FacilityDoc>>(result.Value.ToString());
                return Json(r);
            }
            else
            {
                return Json(result.Value.ToString());
            }
        }

        /*
         * Getting Application State ID
         */
        public int GetApplicationState(int AppID)
        {
            int stateID = 0;

            var get = from a in _context.applications
                      join f in _context.Facilities on a.FacilityId equals f.Id
                      join ad in _context.addresses on f.AddressId equals ad.id
                      join s in _context.States_UT on ad.StateId equals s.State_id
                      where (a.id == AppID && f.DeletedStatus != true && s.DeleteStatus != true)
                      select s;

            if (get.Count() > 0)
            {
                stateID = get.FirstOrDefault().State_id;
            }
            else
            {
                stateID = 0;
            }

            return stateID;
        }


        /*
        * Deleteing a company's document and facility document
        */
        public IActionResult DeleteCompanyDocuments(string CompElpsDocID, string DocType)
        {
            var deleteURL = "";

            if (DocType == "Company")
            {
                deleteURL = "CompanyDocument";
            }
            else
            {
                deleteURL = "FacilityDocument";
            }

            var paramData = restSharpServices.parameterData("Id", CompElpsDocID);
            var result = generalClass.RestResult(deleteURL + "/Delete/{Id}", "DELETE", paramData, null, "Document Deleted", null); // DELETE
            return Json(result.Value);

        }





        /*
         * Get the registered address of a company by using the company's registered id
         */
        public JsonResult GetCompanyAddress(string address_id)
        {
            if (!string.IsNullOrWhiteSpace(address_id))
            {
                var paramData = restSharpServices.parameterData("Id", address_id);
                var result = generalClass.RestResult("Address/ById/{Id}", "GET", paramData, null, null); // GET
                return Json(result.Value);
            }
            else
            {
                return Json("Empty");
            }
        }
        public string getHash(string hashItem)
        {
            string hash = "";



            var data = Encoding.UTF8.GetBytes(hashItem);
            byte[] x;
            using (SHA512 shaM = new SHA512Managed())
            {
                x = shaM.ComputeHash(data);

            }
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in x)
                stringBuilder.AppendFormat("{0:X2}", b);

            hash = stringBuilder.ToString();


            return hash;
        }

        public applications RecordApplication(Phases ph, double tnkTotalVol, int tnkCount, int companyId, int facilityId, string appType, string PermitNo, string usrEmail, string ip, bool frmATC = false, decimal tkFee = 0)
        {
            #region MyRegion
            var fd = CalculateAppFee(ph, PermitNo, tnkTotalVol, tnkCount, frmATC, tkFee);
            if (ph.id == 11 || ph.id == 7)
            {
                appType = "renew";
            }
            PermitNo = ph.ShortName == "REG" ? "REG" : PermitNo;

            #endregion
            var app = new applications();
            app.category_id = ph.category_id;
            app.company_id = companyId;
            app.date_added = DateTime.Now;
            app.date_modified = DateTime.Now;
            app.CreatedAt = DateTime.Now;
            app.UpdatedAt = DateTime.Now;
            app.FacilityId = facilityId;// id;
            app.fee_payable = Math.Round(fd.Fee);
            app.PhaseId = ph.id;
            app.reference = generalClass.Generate_Application_Number();
            app.service_charge = (decimal)ph.ServiceCharge;
            app.TransferCost = tkFee;
            app.status = GeneralClass.PaymentPending;
            app.type = appType;
            app.year = DateTime.Today.Year;
            app.AllowPush = true;
            app.current_Permit = PermitNo;
            app.DeleteStatus = false;
            app.PaymentDescription = fd.FeeDescription;
            //applications after update
            app.Migrated = false;
            _context.applications.Add(app);
            _context.SaveChanges();

            //check if current permit is for legacy
            if (PermitNo != null)
            {
                var lg = _context.Legacies.Where(a => a.LicenseNo.Trim().ToLower() == PermitNo.Trim().ToLower()).FirstOrDefault();
                if (lg != null)
                {
                    lg.IsUsed = true;
                    _context.SaveChanges();
                }
            }

            //record form details for application
            var phaseForms = _context.Forms.Where(a => a.Deleted != true).ToList();
            var phaseForm = new Forms();
            foreach(var f in phaseForms)
            {
                var otherPhases = f.OtherPhases != null ? f.OtherPhases : null;
           
                if (f.PhaseId == ph.id)
                {
                    phaseForm = f;
                    break;
                   
                }
                else if (otherPhases != null)
                {

                    if (otherPhases.Contains(ph.id.ToString()) || f.PhaseId == ph.id)
                    {
                        phaseForm = f;
                        break;
                    }
                }
            }
            var appF = _context.ApplicationForms.Where(a => a.ApplicationId == app.id ).FirstOrDefault();
            if (appF == null && phaseForm != null)
                {
                    var af = new ApplicationForms();
                    af.Confirmed = false;
                    af.ApplicationId = app.id;
                    af.Date = DateTime.Now;
                    af.Filled = false;
                    af.DateModified = DateTime.Now;
                    af.FormId = phaseForm.Id.ToString();
                    af.DepartmentId = 00;
                    af.FormTitle = phaseForm.FriendlyName;
                _context.ApplicationForms.Add(af);
                _context.SaveChanges();
                }
        
            return app;
        }

        public FacilityAPIModel PushFacility(FacilityAPIModel model)
        {
            try
            {

                var param = JsonConvert.SerializeObject(model);
                var paramDatas = _restService.parameterData("fac", param);
                var output = _restService.Response("/api/Facility/Add/{email}/{apiHash}", paramDatas, "POST");
                var response = JsonConvert.DeserializeObject<FacilityAPIModel>(output.ToString());


                return response;

            }
            catch (Exception x)
            {
                LogMessages($"Push Facility to Elps:: {x.ToString()}");
                return null;
            }
            return null;

        }

        public PaymentFeeDescription CalculateAppFee(Phases ph, string PermitNo, double tnkTotalVol, int tnkCount, bool frmATC = false, decimal tkFee = 0, int currentYear = 0)
        {
            try
            {

                string feeDescription = "";
                if (ph.PriceByVolume)
                {
                    feeDescription += $" 0 - 7,500,000 ltrs = N150,000. Every 5000ltrs above this = N100. {Environment.NewLine}";
                }
                if (ph.ProcessingFeeByTank)
                {
                    feeDescription += $"Processing Fee: {ph.ProcessingFee.ToString("N2")}/Tank. {Environment.NewLine}";
                }
                int noOfYr = 0;
                int dt = currentYear== 0 ? DateTime.Now.Year : currentYear;
                if (!string.IsNullOrEmpty(PermitNo))
                {
                    if (ph.category_id == 1)
                    {
                        noOfYr = 1;
                    }
                    else
                    {

                        var p = PermitNo.Split('/');
                        if (p[0] == "NMDPRA" || p[0] == "DPR")
                        {
                            //Platform Permit, Lets look for it and get the Year issued
                            //NMDPRA/PDJ/18/N3080
                            if (ph.ShortName == "LTO" || ph.ShortName == "LR" || ph.ShortName == "TO")
                            {
                                //check if it's Year
                                int y = 0;
                                int a = ph.ShortName == "LR"? p.Length - 1 : p.Length - 2;
                                if (int.TryParse(p[a], out y))
                                {

                                    var x = ph.ShortName == "LR" ? y.ToString() : dt.ToString().Substring(0, 2) + y;
                                    int yr = 0;
                                    if (int.TryParse(x, out yr))
                                    {
                                        noOfYr = dt - yr;
                                    }

                                    if(noOfYr > 100 || noOfYr < 0)
                                    {
                                        noOfYr = dt - int.Parse(p[4]);
                                    }
                                }
                                else 
                                {
                                    var getPermitExpiry = _context.permits.Where(x => x.permit_no == PermitNo).FirstOrDefault();
                                    noOfYr = getPermitExpiry != null ? (dt - getPermitExpiry.date_expire.Year) : noOfYr;
                                }
                            }
                        }
                        else
                        {
                            int yr = 0;
                            if (int.TryParse(p[p.Length - 1], out yr))
                            {
                                noOfYr = dt - yr;
                            }
                        }
                        //DEP00085/2017
                    }
                }
                var fee = 0.00m;


                if (frmATC && ph.ShortName == "TO")//Take Over from ATC
                {
                    var st = Convert.ToDecimal(_configuration.GetSection("AmountSetting").GetSection("TOfrmATCStatutoryFee").Value.ToString());

                    fee += st;
                    feeDescription += $"Statutory Fee: N{st.ToString("N2")} {Environment.NewLine}";
                    var pr = Convert.ToDecimal(_configuration.GetSection("AmountSetting").GetSection("TOfrmATCProcessFee").Value.ToString()) * tnkCount;

                    fee += pr;
                    feeDescription += $"Processing Fee: N{pr.ToString("N2")} {Environment.NewLine}";
                }
                else
                {
                    if (ph.PriceByVolume)
                    {
                        var st = GetPricePerVolume(tnkTotalVol);
                        fee += st;
                        feeDescription += $"Statutory Fee: N{st.ToString("N2")}  {Environment.NewLine}";
                    }
                    else
                    {
                        fee += (decimal)ph.Price;
                        feeDescription += $"Statutory Fee: N{ph.Price.Value.ToString("N2")} {Environment.NewLine}";
                    }
                    if (ph.ProcessingFeeByTank)
                    {
                        var pr = ph.ProcessingFee * tnkCount;
                        fee += pr;
                        feeDescription += $"Processing Fee: N{pr.ToString("N2")} {Environment.NewLine}";

                    }
                    else
                    {
                        fee += ph.ProcessingFee;
                        feeDescription += $"Processing Fee: N{ph.ProcessingFee.ToString("N2")} {Environment.NewLine}";

                    }
                    if (ph.SanctionFee != null && ph.SanctionFee.GetValueOrDefault() > 0)
                    {
                        fee += ph.SanctionFee.GetValueOrDefault();
                        string sanctPay = $"For {ph.name}";

                        feeDescription += $"Sanction Fee: N{ph.SanctionFee.GetValueOrDefault().ToString("N2")}, {sanctPay}  {Environment.NewLine}";

                    }
                }

                if (noOfYr <= 0)
                {
                    noOfYr = 1;
                }
                else
                {
                    if (noOfYr > 1)
                    {
                        feeDescription += $"Being Payment for : {noOfYr}Yrs {Environment.NewLine}";
                    }
                }
                if (frmATC)
                {
                    feeDescription += $" | From ATC";
                }
                if (ph.id == 3 || ph.id == 11 || ph.id == 5)
                {
                    noOfYr = 1;
                }
                fee *= noOfYr;

                if (ph.ShortName.Equals("TO"))
                {
                    var appfee = Convert.ToDouble(_configuration.GetSection("AmountSetting").GetSection("TOApprovalFee").Value.ToString());
                    var applicationfee = Convert.ToDecimal(_configuration.GetSection("AmountSetting").GetSection("TOApplicationFee").Value.ToString());


                    var approvalfee = 0.05 * (double)tkFee > appfee ? 0.05 * (double)tkFee : appfee;
                    fee += (decimal)approvalfee;
                    feeDescription += $"Approval (Transaction) Fee: N{approvalfee.ToString("N2")} {Environment.NewLine}";

                    fee += applicationfee;
                    feeDescription += $"Application Fee: N{applicationfee.ToString("N2")} {Environment.NewLine}";
                }
                return new PaymentFeeDescription { FeeDescription = feeDescription, Fee = Math.Ceiling(fee), NoOfYr = noOfYr };

            }
            catch (Exception x)
            {
                LogMessages($"Calculate Amount:: {x.ToString()}");
                throw;
            }

        }


        /*
          * Get field office staff based on their state id
          * 
          * StateID => Not encrypted state id
          * 
          */
        /*
                 * Get field office staff based on their state id
                 * 
                 * StateID => Not encrypted state id
                 * 
                 */
        public List<int> FieldOfficeStaff(int StateID)
        {
            var getZone = _context.ZoneStates.Where(x => x.State_id == StateID && x.DeleteStatus != true);
            List<int> staffFieldOffices = new List<int>();

            if (getZone.Count() > 0)
            {
                var getFieldOffice = _context.ZonalFieldOffice.Where(x => x.DeleteStatus != true);

                foreach (var f in getFieldOffice.ToList())
                {
                    foreach (var z in getZone.ToList())
                    {
                        if (z.Zone_id == f.Zone_id)
                        {
                            staffFieldOffices.Add(f.FieldOffice_id);
                        }
                    }
                }
            }
            else
            {
                staffFieldOffices.Add(0);
            }
            return staffFieldOffices;
        }





        /*
         * Finding all role based staff to push application to
         */
        public List<StaffPushApps> GetPushStaff(int staff_id)
        {
            List<StaffPushApps> staff = new List<StaffPushApps>();

            var role_type = from s in _context.Staff
                            join r in _context.UserRoles on s.RoleID equals r.Role_id
                            where s.StaffID == staff_id && s.ActiveStatus == true && s.DeleteStatus != true
                            select new
                            {
                                StaffID = s.StaffID,
                                OfficeID = s.FieldOfficeID,
                                RoleID = s.RoleID,
                                RoleName = r.RoleName.Trim(),
                                Location = s.LocationID,
                                Field = s.FieldOfficeID
                            };

            if (role_type.Count() > 0)
            {
                if (role_type.FirstOrDefault().RoleName.Trim() == GeneralClass.ADPDJ)
                {
                    staff = GetPushStaffs(GeneralClass.SECTION_HEAD, (int)role_type.FirstOrDefault().OfficeID, (int)role_type.FirstOrDefault().Location);
                }
                else if (role_type.FirstOrDefault().RoleName.Trim() == GeneralClass.SECTION_HEAD)
                {
                    staff = GetPushStaffs(GeneralClass.TEAMLEAD, (int)role_type.FirstOrDefault().OfficeID, (int)role_type.FirstOrDefault().Location);
                }
                else
                {
                    // not in use
                    staff = GetPushStaffs(GeneralClass.TEAMLEAD, (int)role_type.FirstOrDefault().OfficeID, (int)role_type.FirstOrDefault().Location);
                }
            }

            return staff;
        }



        /*
         * Getting all staffs in the same role
         * 
         * role_type => the type of role for that user
         * staff_field_offic_id => the field office of that staff
         * location => the location that staff is found in
         * 
         */
        public List<StaffPushApps> GetPushStaffs(string role_type, int staff_field_offic_id, int Location)
        {
            List<StaffPushApps> staffs = new List<StaffPushApps>();

            int zoneID = GetZonesFromFieldOffice(staff_field_offic_id);
            List<int> fieldOffices = GetFieldOfficesFrromZones(zoneID);

            var getStaff = from s in _context.Staff
                           join r in _context.UserRoles on s.RoleID equals r.Role_id
                           where
                           r.RoleName == role_type &&
                           s.LocationID == Location && s.ActiveStatus == true && s.DeleteStatus != true
                           select new StaffPushApps
                           {
                               StaffId = s.StaffID,
                               LastName = s.LastName,
                               FirstName = s.FirstName,
                               Email = s.StaffEmail,
                               FieldOffice = (int)s.FieldOfficeID,
                               DeskCount = _context.MyDesk.Where(x => x.StaffID == s.StaffID && x.HasWork != true).Count()
                           };

            if (getStaff.Count() > 0)
            {
                foreach (var s in getStaff)
                {
                    foreach (var fid in fieldOffices)
                    {
                        if (s.FieldOffice == fid)
                        {
                            staffs.Add(s);
                        }
                    }
                }
            }
            return staffs;
        }





        /*
         * Getting all field office from zones
         * 
         */
        public List<int> GetFieldOfficesFrromZones(int zonalID)
        {
            List<int> _fieldOffice = new List<int>();

            //var fieldOffice = from zf in _context.ZonalFieldOffice
            //                  join z in _context.ZonalOffice on zf.ZoneId equals z.ZoneId
            //                  join f in _context.FieldOffices on zf.FieldOfficeId equals f.FieldOfficeId
            //                  where zf.ZoneId == zonalID && zf.DeleteStatus != true && z.DeleteStatus != true && f.DeleteStatus != true
            //                  select zf;

            //if (fieldOffice.Count() > 0)
            //{
            //    foreach (var f in fieldOffice)
            //    {
            //        _fieldOffice.Add(f.FieldOfficeId);
            //    }
            //}
            return _fieldOffice;
        }





        /*
           * Getting all zones from field office
           * 
           */
        public int GetZonesFromFieldOffice(int field_office)
        {
            int zone = 0;
            var zones = from zf in _context.ZonalFieldOffice
                        join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                        join f in _context.FieldOffices on zf.FieldOffice_id equals f.FieldOffice_id
                        where zf.FieldOffice_id == field_office && zf.DeleteStatus != true && z.DeleteStatus != true && f.DeleteStatus != true
                        select zf;

            if (zones.Count() > 0)
            {
                zone = zones.FirstOrDefault().Zone_id;
            }
            return zone;
        }


        public void SaveHistory(int appid, int staffid, string status, string comment)
        {
            var getStaff = (from u in _context.Staff where u.StaffID == staffid select u).FirstOrDefault();

            application_desk_histories appDeskHistory = new application_desk_histories()
            {
                UserName = getStaff.StaffEmail,
                application_id = appid,
                StaffID = staffid,
                comment = comment,
                status = status,
                date = DateTime.Now
            };

            _context.application_desk_histories.Add(appDeskHistory);
            _context.SaveChanges();
        }



        public string MessageTemplate(string subject, string content)
        {
            string body = "<div>";
            body += "<div style='width: 700px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='~/images/nmdpra.png' alt='Logo'/></div>";
            body += "<div class='text-left' style='background-color: #ece8d4; width: 700px; min-height: 200px;'>";
            body += "<div style='padding: 10px 30px 30px 30px;'>";
            body += "<h5 style='text-align: center; font-weight: 300; padding-bottom: 10px; border-bottom: 1px solid #ddd;'>" + subject.ToUpper() + "</h5>";
            body += "<p>Dear Sir/Madam,</p>";
            body += "<p style='line-height: 30px; text-align: justify;'>" + content + "</p>";
            body += "<br>";
            body += "<p>Kindly go to <a href='https://depot.dpr.gov.ng/'>DEPOT Portal (CLICK HERE)</a> link and process application on your desk. </p>";
            body += "<p>Nigerian Midstream and Downstream Petroleum Regulatory Authority<br/> <small> </small></p> </div>";
            body += "<div style='padding:10px 0 10px; 10px; background-color:#888; color:#f9f9f9; width:700px;'> &copy; " + DateTime.Now.Year + " Nigerian Midstream and Downstream Petroleum Regulatory Authority &minus; NMDPRA Nigeria</div></div></div>";
            return body;
        }




        //public string StaffMessageTemplate(string subject, string content)
        //{
        //    string body = "<div>";
        //    body += "<div style='width: 700px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='~/images/nmdpra.png' alt='Logo'/></div>";
        //    body += "<div class='text-left' style='background-color: #ece8d4; width: 700px; min-height: 200px;'>";
        //    body += "<div style='padding: 10px 30px 30px 30px;'>";
        //    body += "<h5 style='text-align: center; font-weight: 300; padding-bottom: 10px; border-bottom: 1px solid #ddd;'>" + subject + "</h5>";
        //    body += "<p>Dear Sir/Madam,</p>";
        //    body += "<p style='line-height: 30px; text-align: justify;'>" + content + "</p>";
        //    body += "<br>";
        //    body += "<p>Kindly go to <a href='https://depot.dpr.gov.ng/'>DEPOT Portal (CLICK HERE)</a> link and process application on your desk. </p>";
        //    body += "<p>Nigerian Midstream and Downstream Petroleum Regulatory Authority<br/> <small> </small></p> </div>";
        //    body += "<div style='padding:10px 0 10px; 10px; background-color:#888; color:#f9f9f9; width:700px;'> &copy; " + DateTime.Now.Year + " Nigerian Midstream and Downstream Petroleum Regulatory Authority &minus; NMDPRA Nigeria</div></div></div>";
        //    return body;
        //}




        public string GenerateReceiptNo(double Amount, long Id)
        {
            string S = "1";
            string BK = "01";
            if (Amount < 250000)
            {
                BK = "02";
            }
            string MM = DateTime.Now.Month.ToString("00");
            string YY = DateTime.Now.Year.ToString().Substring(2, 2);
            string XXXX = Id.ToString("000000");
            return string.Format("{0}{1}{2}{3}{4}", S, BK, MM, YY, "0" + XXXX);
        }

        public string GenerateReceiptNo(double Amount, long Id, DateTime datepaid)
        {
            string S = "1";
            string BK = "01";
            if (Amount < 250000)
            {
                BK = "02";
            }
            string MM = datepaid.Month.ToString("00");
            string YY = datepaid.Year.ToString().Substring(2, 2);
            string XXXX = Id.ToString("000000");
            return string.Format("{0}{1}{2}{3}{4}", S, BK, MM, YY, "0" + XXXX);
        }

        public string ReferenceCode()
        {
            //generate 12 digit numbers
            var bytes = new byte[8];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            ulong random = BitConverter.ToUInt64(bytes, 0) % 1000000000000;
            return String.Format("{0:D12}", random);
        }



        /*
       * Generating permit numbers with different parameters
       */
        public string GenerateReferenceNumber(string types)
        {
            string PermitNumber = "";

            int i = 1;
            int num = 0;
            var year = DateTime.Now.Year;
            string type = "";
            type = "Depot";
            //check

            var seq = _context.applications.OrderByDescending(x => x.id);

            if (seq.Count() > 0)
            {
                if (year > seq.FirstOrDefault().year)
                {
                    num += i;
                    PermitNumber = "NMDPRA/UMR/RM/" + type + "/" + year + "/" + num.ToString("D4");
                }
                else
                {
                    num = (int)seq.FirstOrDefault().id + i;
                    PermitNumber = "NMDPRA/UMR/RM/" + type + "/" + year + "/" + num.ToString("D4");

                }
            }
            else
            {
                PermitNumber = "NMDPRA/UMR/RM/" + type + "/" + year + "/" + i.ToString("D4");
            }

            return PermitNumber;

        }




        //Next Approval section
        public string Assign(int appId,int deskId,int processId, string UserName, string actionComment,string staffToPushTo, int fieldOffice = 0)
        {
            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var getCurrentStaff = _context.Staff.Where(x => x.StaffID == userID).FirstOrDefault();

                var getDesk = _context.MyDesk.Where(x => x.DeskID == deskId).FirstOrDefault();
                if (getDesk == null)
                {
                    return "Invalid Application";
                }

                var application = _context.applications.Where(a => a.id == getDesk.AppId).FirstOrDefault();
                var phase = _context.Phases.Where(a => a.id == application.PhaseId).FirstOrDefault();
                var facility = _context.Facilities.Where(a => a.Id == application.FacilityId).FirstOrDefault();
                var company = _context.companies.Where(c => c.id == application.company_id).FirstOrDefault();
                var facilityAddress = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();

                //Check if processing is still pending on a desk
                var checkPendingDesk = _context.MyDesk.Where(C => C.HasWork != true  && C.AppId==application.id && C.StaffID!= userID).FirstOrDefault();
                if (checkPendingDesk != null)
                {
                    var getStaff = (from u in _context.Staff where u.StaffID == checkPendingDesk.StaffID select u).FirstOrDefault();
                    return string.Format("Application is still pending on user {0} desk", getStaff.StaffEmail);
                }

                //check if application is depot modification

                var modificationType = phase.ShortName != "DM" ? "": _context.FacilityModifications.Where(x => x.ApplicationId == appId).FirstOrDefault()?.Type?.ToLower();

                var AllProcess = (from ap in _context.WorkProccess
                                 where ap.PhaseID == application.PhaseId && ap.DeleteStatus!=true
                                select ap);
                 var nextProcess = (from ap in _context.WorkProccess
                                    where  ap.PhaseID == application.PhaseId && ap.Sort == (getDesk.Sort+1) && ap.DeleteStatus != true
                                    select ap).FirstOrDefault();


                if (nextProcess == null)
                {
                    application.status = GeneralClass.Approved;
                    application.current_desk = null;
                    _context.SaveChanges();

                    #region Update App Status on ELPS if Approved 

                    if (application.PhaseId == 6)
                    {
                        //Take Over
                        //get the facility, check if the companyId has been updated, else update and also push to elps
                        // to complete proper change of Ownership
                        var fac = _context.Facilities.Where(a => a.Id == application.FacilityId).FirstOrDefault();
                        if (fac != null && fac.CompanyId != application.company_id)
                        {

                            fac.CompanyId = application.company_id;
                            _context.SaveChanges();
                        }
                    }

                    var appAPI = new ApplicationAPIModel();
                    appAPI.Status = application.status;
                    appAPI.OrderId = application.reference;
                    appAPI.CompanyId = (int)company.elps_id;
                    appAPI.licenseName = phase.name;

                    var param = JsonConvert.SerializeObject(appAPI);
                    var paramDatas = _restService.parameterData("app", param);
                    var outputL = _restService.Response("/api/Application/{email}/{apiHash}", paramDatas, "PUT");
                    //var respApp = JsonConvert.DeserializeObject<ApplicationAPIModel>(outputL.ToString());
                    if (outputL.IsSuccessful)
                    {
                        #region Log in History
                        var hist = new application_desk_histories();
                        hist.application_id = appId;
                        hist.date = DateTime.Now;
                        hist.comment = "Application Approved and License/Permit Issued";
                        hist.UserName = userEmail;
                        hist.status = "Final Approval";
                        _context.SaveChanges();

                        #endregion

                        return "Processing Complete";
                    }
                    else
                    {
                     return null;
                    }
                }
                else
                {
                    Staff nextStaff = null;

                    //check if pusher specifies who to push application to
                    if (staffToPushTo != null)
                    {
                        nextStaff = _context.Staff.Where(a => a.DeleteStatus != true && a.ActiveStatus != false && (a.StaffID.ToString() == staffToPushTo || a.StaffEmail == staffToPushTo)).FirstOrDefault();
                        
                        //Now check if selected staff is the next processor
                        var checkNextProcess =AllProcess.OrderByDescending(x => x.Sort).Where(x => x.RoleID == nextStaff.RoleID && x.Sort == getDesk.Sort + 1).FirstOrDefault();
                       
                        
                        if(checkNextProcess == null) //This indicates that the staff selected to push application to is not the next processor, so find the next processor
                        {
                            nextStaff = (from stf in _context.Staff
                                         join r in _context.UserRoles on stf.RoleID equals r.Role_id
                                         where stf.DeleteStatus != true && stf.ActiveStatus != false
                                         && r.Role_id == nextProcess.RoleID && stf.FieldOfficeID == getCurrentStaff.FieldOfficeID
                                         select stf).FirstOrDefault();
                        }

                    }
                    else
                    {

                        string Location = "";
                        var getLocation = _context.Location.Where(x => x.LocationID == nextProcess.LocationID).FirstOrDefault();
                        if (getLocation != null)
                        {
                            Location = getLocation.LocationName;
                        }
                        
                        //check if sender is supervisor in HQ
                        if (userRole == GeneralClass.SUPERVISOR && Location.ToLower().Contains("hq"))
                        {
                            
                            nextStaff = (from stf in _context.Staff
                                      join r in _context.UserRoles on stf.RoleID equals r.Role_id
                                      where stf.DeleteStatus != true && stf.ActiveStatus != false
                                      && r.RoleName == GeneralClass.ADPDJ
                                      select stf).FirstOrDefault();
                            nextProcess = AllProcess.OrderByDescending(x => x.Sort).Where(x => x.RoleID == nextStaff.RoleID).FirstOrDefault();
                        }
                        else
                        {
                            //int branchId = GetBranch(facilityAddress.StateId, Location);
                            var getCurrentStaffFD = _context.FieldOffices.Where(x => x.FieldOffice_id == getCurrentStaff.FieldOfficeID).FirstOrDefault();
                            var getNextProcessRole = _context.UserRoles.Where(r => r.Role_id == nextProcess.RoleID).FirstOrDefault();

                            if (Location.ToLower().Contains("hq"))
                            {

                                    var getStaff = from s in _context.Staff
                                                   join r in _context.UserRoles on s.RoleID equals r.Role_id
                                                   where s.RoleID == getNextProcessRole.Role_id &&
                                                   s.LocationID == nextProcess.LocationID && s.ActiveStatus!= false && s.DeleteStatus != true
                                                   select new StaffPushApps
                                                   {
                                                       Staff = s,
                                                       StaffId = s.StaffID,
                                                       LastName = s.LastName,
                                                       FirstName = s.FirstName,
                                                       Email = s.StaffEmail,
                                                       FieldOffice = (int)s.FieldOfficeID,
                                                       DeskCount = _context.MyDesk.Where(x => x.StaffID == s.StaffID && x.HasWork != true).Count()
                                                   };

                                    var minDeskCount = getStaff.ToList().Min(x => x.DeskCount);
                                    nextStaff = getStaff.Where(a => a.DeskCount == minDeskCount).FirstOrDefault().Staff;
                                
                            }
                            else
                            {
                                

                                    var getStaff = from s in _context.Staff
                                                   join r in _context.UserRoles on s.RoleID equals r.Role_id
                                                   where s.RoleID == getNextProcessRole.Role_id &&
                                                   s.FieldOfficeID == getCurrentStaffFD.FieldOffice_id 
                                                   && s.ActiveStatus != false && s.DeleteStatus != true
                                                   select new StaffPushApps
                                                   {
                                                       Staff = s,
                                                       StaffId = s.StaffID,
                                                       LastName = s.LastName,
                                                       FirstName = s.FirstName,
                                                       Email = s.StaffEmail,
                                                       FieldOffice = (int)s.FieldOfficeID,
                                                       DeskCount = _context.MyDesk.Where(x => x.StaffID == s.StaffID && x.HasWork != true).Count()
                                                   };

                                    var minDeskCount = getStaff.ToList().Min(x => x.DeskCount);
                                    nextStaff = getStaff.Where(a => a.DeskCount == minDeskCount).FirstOrDefault().Staff;
                                
                            }
                            
                        }
                    }
                    if (nextStaff == null)
                    {
                        return "not ok";
                    }

                    //save staff info to mydesk
                    var newdesk = new MyDesk()
                    {
                        StaffID = nextStaff.StaffID,
                        FromStaffID = userID,
                        HasWork = false,
                        HasPushed = false,
                        AppId = application.id,
                        Sort = nextProcess.Sort,
                        ProcessID = nextProcess.ProccessID,
                        CreatedAt = DateTime.Now,
                        Comment = actionComment
                    };
                    _context.MyDesk.Add(newdesk);
                    _context.SaveChanges();

                    #region Log in History
                    //send dashboard notification to the sender and receiver
                    string subject = "Push for Application With Ref: " + application.reference;
                    string content = userEmail + " pushed application to your desk for processing.";
                    
                    string content2 = "You pushed application to " + nextStaff.StaffEmail + "'s desk for processing.";
                    var emailMsg = SaveMessage(application.id, userID, subject, content2, getCurrentStaff.StaffElpsID, "Staff");
                    //var sendEmail = SendEmailMessage2Staff(userEmail, userName, emailMsg, null);

                    var emailMsg2 = SaveMessage(application.id, nextStaff.StaffID, subject, content, nextStaff.StaffElpsID, "Staff");
                    //var sendEmail2 = SendEmailMessage2Staff(nextStaff.StaffEmail, nextStaff.FirstName, emailMsg, null);
                    //savehistory
                    if (actionComment == null)
                    {
                        SaveHistory(application.id, userID, userEmail, GeneralClass.Move, userEmail + " pushed application to " + nextStaff.StaffEmail + " for processing.");
                    }
                    else
                    {
                        SaveHistory(application.id, userID, userEmail, GeneralClass.Move, userEmail + " passed application to " + nextStaff.StaffEmail + "( comment => " + newdesk.Comment + ") for approval");
                    }
                    #endregion

                    application.current_desk = nextStaff.StaffID;
                    _context.SaveChanges();
                    //|| application.PhaseId == 3
                    //check for all the Applications that starts from the Field Office
                    //|| application.PhaseId == 5  || application.PhaseId == 9 || application.PhaseId == 11

                    if ((application.type.ToLower() == "renew") && nextProcess.Sort == 1)
                    {
                        // 5 for Facility Modification; 9 for Modification without approval; 
                        // 11 for Recalibration of tanks
                        var requiredStaff = (from m in _context.Staff
                                             join a in _context.UserRoles on m.RoleID equals a.Role_id
                                             where m.ActiveStatus == true && m.DeleteStatus != true && a.RoleName.ToLower() == "opscon" || a.RoleName.ToLower() == "adops"
                                             select m).ToList();

                        //Push applications to opscon,AdOps,
                        NotifyZNFD(application.id, facilityAddress.StateId, requiredStaff, UserName);
                    }

                    LogMessages(userEmail + " Assigned Application To " + nextStaff.StaffEmail + " For Processing ", userEmail);
                    return "Ok";
                }
            }
            catch (Exception x)
            {
                return "not ok";
            }
        }

        public bool NotifyZNFD(int id, int stateId, List<Staff> opscons, string username)
        {
            // Get Opscon to handle

            #region get the Staff to be added
            //Get all Zones
            var output = _restService.Response("/api/Branch/ZoneMapping/{email}/{apiHash}", null, null);
            if (output.IsSuccessful == true)
            {
                var zonesmapping = JsonConvert.DeserializeObject<List<zoneModel>>(output.Content.ToString());

                List<Staff> selectedOps = new List<Staff>();
                zoneModel selectedZone = null;
                var selectedZone2 = (from u in zonesmapping
                                join z in _context.branches on u.branchId equals z.id
                                join st in _context.States_UT on u.country_id equals st.Country_id
                                where st.State_id == stateId && z.branchCode.ToLower() != "hq"
                                select new zoneModel
                                {
                                    stateId = st.State_id,
                                    country_id = st.Country_id,
                                    branchId = z.id,
                                    code = z.branchCode

                                }).FirstOrDefault();
                //var zones = from zf in _context.ZonalFieldOffice
                //            join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                //            join f in _context.FieldOffices on zf.FieldOffice_id equals f.FieldOffice_id
                //            where zf.FieldOffice_id == field_office && zf.DeleteStatus != true && z.DeleteStatus != true && f.DeleteStatus != true
                //            select zf;

                selectedZone = (from zf in _context.ZonalFieldOffice
                                    join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                                join f in _context.FieldOffices on zf.FieldOffice_id equals f.FieldOffice_id
                                join zs in _context.ZoneStates on z.Zone_id equals zs.Zone_id
                                    join s in _context.States_UT on zs.State_id equals s.State_id
                                    join c in _context.countries on s.Country_id equals c.id
                                    where zf.DeleteStatus != true && f.DeleteStatus != true 
                                    && z.DeleteStatus != true && s.DeleteStatus != true &&
                                    s.State_id == stateId && (!(f.OfficeName.ToLower().Contains( "head"))) &&
                                    zs.DeleteStatus != true
                                 select new zoneModel
                                 {
                                     stateId = s.State_id,
                                     country_id = s.Country_id,
                                     branchId = f.FieldOffice_id,
                                     code = f.OfficeName

                                 }).FirstOrDefault();




                if (selectedZone == null)
                {
                    //Loop thru the states in each zone for possible selection
                    selectedZone = zonesmapping.Where(a => a.CoveredStates.Select(s => s.StateId).Contains(stateId)).FirstOrDefault();
                    if (selectedZone != null)
                    {

                        var getFieldZonalOffice = (from zf in _context.ZonalFieldOffice
                                                              join z in _context.ZonalOffice on zf.Zone_id equals z.Zone_id
                                                              join f in _context.FieldOffices on zf.FieldOffice_id equals f.FieldOffice_id
                                                              join zs in _context.ZoneStates on z.Zone_id equals zs.Zone_id
                                                              where zs.State_id ==stateId && zf.DeleteStatus != true
                                                              && z.DeleteStatus != true && f.DeleteStatus != true
                                                              select f).FirstOrDefault(); 
                        selectedOps = opscons.Where(a => a.FieldOfficeID == getFieldZonalOffice.FieldOffice_id).ToList();

                    }
                }
                else
                {
                    selectedOps = opscons.Where(a => a.FieldOfficeID == selectedZone.branchId).ToList();

                }

                #endregion

                if (selectedOps != null && selectedOps.Count > 0 && selectedZone != null)
                {
                    foreach (var ops in selectedOps)
                    {


                        var checkJoint = _context.JointAccounts.Where(a => a.ApplicationId == id && a.Opscon == ops.StaffEmail).FirstOrDefault();
                        if (checkJoint == null)
                        {

                            var joint = new JointAccounts();
                            joint.ApplicationId = id;
                            joint.DateAdded = DateTime.Now;
                            joint.OperationsCompleted = false;
                            joint.Opscon = ops.StaffEmail;
                            joint.Assigned = false;
                            _context.JointAccounts.Add(joint);
                            _context.SaveChanges();
                        }
                    }
                    //check mail
                    #region Send Mail to Notify Opscon and Supervisor
                    var body = "";
                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    string file = up + @"\\Templates\" + "InternalMemo.txt";
                    using (var sr = new StreamReader(file))
                    {

                        body = sr.ReadToEnd();
                    }

                    var subject = "New Application on Depot Portal";
                    var app = _context.applications.Where(a => a.id == id).FirstOrDefault();
                    var phs = _context.Phases.Where(a => a.id == app.PhaseId).FirstOrDefault();
                    var facility = _context.Facilities.Where(x => x.Id == app.FacilityId).FirstOrDefault();
                    var companyName = _context.companies.Where(x => x.id == app.company_id).FirstOrDefault();

                    foreach (var ops in selectedOps)
                    {
                        var stf = _context.Staff.Where(a => a.StaffEmail == ops.StaffEmail).FirstOrDefault();
                        var type = (app.type.ToLower() == "new" ? "New Depot License" : "Depot License Renewal").ToString() + "(" + phs.name + ")";
                        var msg = $"A new application on NMDPRA Depot Portal for " + type + " has been submitted on the portal and you are hereby notified for Joint Operation towards the issuing of the License/Approval. <p>Details of the Application is as follow:</p>";
                        msg += "<table class'table'>" +
                            $"<tr><td>Application Reference</td><td><a href='{ElpsServices._elpsBaseUrl}/Process/ViewApplication/" + app.id + "'>" + app.reference + "</a></td></tr>" +
                            $"<tr><td>Application Company</td><td><a href='{ElpsServices._elpsBaseUrl}/Company/Detail/" + app.company_id + "'>" + companyName.name + "</a></td></tr>" +
                            $"<tr><td>Facility</td><td><a href='{ElpsServices._elpsBaseUrl}/Facility/ViewFacility/" + app.FacilityId + "'>" + facility.Name + "(" + facility.address_1 + ")</a></td></tr>" +
                            "<tr><td>Facility Address</td><td>" + facility.address_1 + "</td></tr>" +
                            "</table><br /><br /><p>You will be notified on the progress and action required by you as the application process progresses.</p>";
                        //var msgBody = string.Format(body, subject, "", ops.FirstName, msg);

                        // MailHelper.SendEmail(ops.UserEmail, subject, msgBody);
                        var mailBase = body;

                        //var msgBody = string.Format(item mailBase, subject, "", staff.FirstName, item);
                        var emailMsg = SaveMessage(app.id, ops.StaffID, subject, msg, ops.StaffElpsID, "Staff");
                        var sendEmail = SendEmailMessage2Staff(ops.StaffEmail, ops.FirstName, emailMsg, null);

                    }
                    #endregion

                    return true;
                }
            }
            return false;
        }

        public MeetingVenue GetMeetingVenue(int Id, string Owner)
        {
            MeetingVenue venue = new MeetingVenue();
            if (Owner.ToLower() == "client")
            {
                var facility = _context.Facilities.Where(f => f.Id == Id).FirstOrDefault();
                var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                var state = _context.States_UT.Where(a => a.State_id == address.StateId).FirstOrDefault();

                venue.Address = address.address_1 + " " + address.city + ", " + state.StateName+".";
                venue.Title = "Applicant Facility";
            }
            else
            {
                //Branch branch = _branchRep.FindBy(C => C.Id == Id).FirstOrDefault();
                #region Get Branch from ELPS
                FieldOffices branch = new FieldOffices();  //_branchRep.GetAll().ToList();
                var mkem = ElpsServices._elpsAppEmail;
                var clientUrl = ElpsServices._elpsBaseUrl;
                var hash = ElpsServices.appHash;

                var client = new WebClient();
                string output = client.DownloadString(clientUrl + "Branch/" + Id + "/" + mkem + "/" + hash);
                //branch = JsonConvert.DeserializeObject<FieldOffices>(output);
                branch = _context.FieldOffices.Where(x => x.FieldOffice_id == Id).FirstOrDefault();
                #endregion
                venue.Title = branch.OfficeName;
                venue.Address = branch.OfficeName;
            }
            return venue;
        }

        public List<MeetingVenue> GetAllVenue(int facStateId, string facStateName)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            List<MeetingVenue> meetingVenues = new List<MeetingVenue>();
            meetingVenues.Add(new MeetingVenue { Id = 0, Title = "Applicant's Facility" });

            #region Get All Branches from ELPS
            List<BranchModel> branches = new List<BranchModel>();  //_branchRep.GetAll().ToList();

            var client = new WebClient();
            string output = client.DownloadString(ElpsServices._elpsBaseUrl + "/api/Branch/All/" + ElpsServices._elpsAppEmail + "/" + ElpsServices.appHash);
            branches = JsonConvert.DeserializeObject<List<BranchModel>>(output);
            #endregion
            var me = _context.Staff.Where(a => a.StaffEmail == userEmail).FirstOrDefault();
            var myLocation = _context.Location.Where(x => x.LocationID == me.LocationID).FirstOrDefault();

            if (myLocation.LocationName.ToLower().Contains("hq"))
            {
                //Pick HQ
                var hq = branches.Where(a => a.name.ToLower() == "Head Quaters".ToLower() || a.name.ToLower() == "head office").FirstOrDefault();
                meetingVenues.Add(new MeetingVenue { Id = hq.id, Title = hq.name });

            }
            else { 
            var hq = branches.Where(a => a.name.ToLower() == "Head Quaters".ToLower() || a.name.ToLower() == "head office").FirstOrDefault();
            meetingVenues.Add(new MeetingVenue { Id = hq.id, Title = hq.name });
                //Pick My Branch
                if (hq.id != me.FieldOfficeID)
                {
                   // var myBranch = branches.Where(a => a.id == me.FieldOfficeID).FirstOrDefault();
                    //meetingVenues.Add(new MeetingVenue { Id = myBranch.id, Title = myBranch.name });

                    var myOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == me.FieldOfficeID).FirstOrDefault();
                    meetingVenues.Add(new MeetingVenue { Id = myOffice.FieldOffice_id, Title = myOffice.OfficeName });

                }

            }
            //Pick the Branch of the State where the Facility is Located
            var fb = branches.Where(a => a.stateName.ToLower() == facStateName.ToLower()).FirstOrDefault();
            if (fb != null)
            {
                meetingVenues.Add(new MeetingVenue { Id = fb.id, Title = $"NMDPRA Office - ({fb.name} {fb.stateName})" });
            }
            return meetingVenues;
        }
        public List<MeetingVenue> GetOfflineVenue(int facStateId, string facStateName)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

            List<MeetingVenue> meetingVenues = new List<MeetingVenue>();
            meetingVenues.Add(new MeetingVenue { Id = 0, Title = "Applicant's Facility" });

            var branches = _context.FieldOffices;
            var me = _context.Staff.Where(a => a.StaffEmail == userEmail).FirstOrDefault();
            var myLocation = _context.Location.Where(x => x.LocationID == me.LocationID).FirstOrDefault();

            if (myLocation.LocationName.ToLower().Contains("hq"))
            {
                //Pick HQ
                var hq = branches.Where(a => a.OfficeName.ToLower().Contains("head") || a.OfficeName.ToLower().Contains("hq")).FirstOrDefault();
                meetingVenues.Add(new MeetingVenue { Id = hq.FieldOffice_id, Title = $"NMDPRA Office - ({hq.OfficeName} {hq.OfficeAddress})" });

            }
            else { 
            var hq = branches.Where(a => a.OfficeName.ToLower().Contains("head") || a.OfficeName.ToLower().Contains("hq")).FirstOrDefault();
                meetingVenues.Add(new MeetingVenue { Id = hq.FieldOffice_id, Title = $"NMDPRA Office - ({hq.OfficeName} {hq.OfficeAddress})" });
                //Pick My Branch
                if (hq.FieldOffice_id != me.FieldOfficeID)
                {
                    var myOffice = _context.FieldOffices.Where(x => x.FieldOffice_id == me.FieldOfficeID).FirstOrDefault();
                    meetingVenues.Add(new MeetingVenue { Id = myOffice.FieldOffice_id, Title = $"NMDPRA Office - ({myOffice.OfficeName} {myOffice.OfficeAddress})" });

                }

            }
            //Pick the Branch of the State where the Facility is Located
            var fb = branches.Where(a => a.OfficeName.ToLower() == facStateName.ToLower()).FirstOrDefault();
            if (fb != null)
            {
                meetingVenues.Add(new MeetingVenue { Id = fb.FieldOffice_id, Title = $"NMDPRA Office - ({fb.OfficeName} {fb.OfficeAddress})" });
            }
            return meetingVenues;
        }
        public List<LpgLicense.Models.RPartner> BuildPartners(applications application, RemitaSplit rmSplit, string extra, decimal amount = 0, decimal TransferCostAmount = 0)

        {

            string ServiceTypeID = _configuration.GetSection("AmountSetting").GetSection("ServiceTypeID").Value.ToString();

            string beneficiaryName = _configuration.GetSection("RemitaSplit").GetSection("AccName_1").Value.ToString();

            string bankCode = _configuration.GetSection("RemitaSplit").GetSection("bankCode").Value.ToString();

            string beneficiaryAccount = _configuration.GetSection("RemitaSplit").GetSection("Acc_1").Value.ToString();

            string deductFeeFrom = _configuration.GetSection("RemitaSplit").GetSection("AccDeduct_1").Value.ToString();
         
            string deductIGRFeeFrom = _configuration.GetSection("RemitaSplit").GetSection("AccDeduct_2").Value.ToString();

            var IGRAccount = _configuration.GetSection("RemitaSplit").GetSection("IGRAccount").Value.ToString();

            var IGRBankCode = _configuration.GetSection("RemitaSplit").GetSection("IGRBankCode").Value.ToString();

            var TargetBankCode = _configuration.GetSection("RemitaSplit").GetSection("TargetBankCode").Value.ToString();
            
            var TargetBankAccount = _configuration.GetSection("RemitaSplit").GetSection("TargetAccount").Value.ToString();

            double fivepercent = 0.00; double IGRtenpercent = 0.00;


            #region build partners share



            double amountToShare = 0;

            double rmAmt = 0;   //RM

            double fgAmt = 0;   //FG

            double dprAmt = 0;  //NMDPRA

            double bmAmt = 0;   //Brandone

            double rm1 = 0;

            double rm2 = 0;



            if (application.PhaseId == 1)

            {

                rm1 = 46.25;

                rmSplit.serviceTypeId = ServiceTypeID;

                amountToShare = Convert.ToDouble(application.service_charge) - rm1;

                fgAmt = Convert.ToDouble(application.fee_payable - (application.fee_payable * 0.01m)); // 1% of Statutory

                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;

                bmAmt = amountToShare * 0.9;

                rmAmt = rm1 + rm2;

                dprAmt = (amountToShare * 0.1) + (rmAmt);

            }

            else if (application.PhaseId == 2)

            {

                rmSplit.serviceTypeId = ServiceTypeID;

                rm1 = 157.50;

                amountToShare = Convert.ToDouble(application.service_charge) - rm1;// ap.service_charge - rm1;

                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory

                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;

                bmAmt = amountToShare * 0.9;    // 90% of Service Charge

                rmAmt = rm1 + rm2;

                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge



                //amountToShare = (app.service_charge - 265 - SmsCharge);

            }

            else if (application.PhaseId == 3)

            {

                rmSplit.serviceTypeId = ServiceTypeID;



                rm1 = 350.00;

                amountToShare = Convert.ToDouble(application.service_charge) - rm1;

                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory

                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;

                bmAmt = amountToShare * 0.9;    // 90% of Service Charge

                rmAmt = rm1 + rm2;

                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge

                //amountToShare = (app.service_charge - 550 - SmsCharge);

            }

            else if (application.PhaseId == 4)

            {

                rmSplit.serviceTypeId = ServiceTypeID;



                rm1 = 157.50;

                amountToShare = Convert.ToDouble(application.service_charge) - rm1;

                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory

                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;

                bmAmt = amountToShare * 0.9;    // 90% of Service Charge

                rmAmt = rm1 + rm2;

                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge



                //amountToShare = (app.service_charge - 265 - SmsCharge);

            }

            else if (application.PhaseId == 5)

            {

                rmSplit.serviceTypeId = ServiceTypeID;

                rm1 = 350.00;

                amountToShare = Convert.ToDouble(application.service_charge) - rm1;

                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory

                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;

                bmAmt = amountToShare * 0.9;    // 90% of Service Charge

                rmAmt = rm1 + rm2;

                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge



            }

            else if (application.PhaseId == 6)

            {

                rmSplit.serviceTypeId = ServiceTypeID;



                rm1 = 350.00;

                amountToShare = Convert.ToDouble(application.service_charge) - rm1;

                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory

                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;

                bmAmt = amountToShare * 0.9;    // 90% of Service Charge

                rmAmt = rm1 + rm2;

                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge



            }

            else if (application.PhaseId == 7)

            {

                rmSplit.serviceTypeId = ServiceTypeID;



                rm1 = 0;

                amountToShare = Convert.ToDouble(application.service_charge) - rm1;

                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory

                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;

                bmAmt = amountToShare * 0.9;    // 90% of Service Charge

                rmAmt = rm1 + rm2;

                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge



            }



            rmSplit.serviceTypeId = ServiceTypeID;

            var rp = new List<RPartner>();

            var fp = application.fee_payable.ToString().Split('.');


            if (application.PhaseId == 6 && extra != "yes") //Take Over

            {

                //IGR 5% Payment implementation

                double TFCost = Convert.ToDouble(application.TransferCost);

                fivepercent = (5.00 / 100) * TFCost;

                fivepercent = fivepercent >= 1000000.00 ? fivepercent : 1000000.00;

                //Now get FG Amount

                var FGAmount = amount > 0 ? (amount - Convert.ToDecimal(fivepercent)).ToString() : (application.fee_payable - Convert.ToDecimal(fivepercent)).ToString();

                //Get IGR Amount
            
                IGRtenpercent = (0.05 * fivepercent) + double.Parse(rmSplit.ServiceCharge); //10% of IGR fee
                rmSplit.IGRFee = fivepercent.ToString();

                fivepercent = fivepercent + double.Parse(rmSplit.ServiceCharge) - IGRtenpercent;

                double totalIGRFee = fivepercent + Convert.ToDouble(IGRtenpercent);
                double newIGRFee = (0.9) * totalIGRFee;
                double newTgtFee = totalIGRFee - newIGRFee;
     
                rp.Add(new RPartner

                {

                    lineItemsId = "1",

                    beneficiaryName = beneficiaryName,

                    bankCode = bankCode,

                    beneficiaryAccount = beneficiaryAccount,

                    beneficiaryAmount = FGAmount,

                    deductFeeFrom = deductFeeFrom

                });


                rp.Add(new RPartner

                {

                    lineItemsId = "2",

                    beneficiaryName = "Beneficiary 2",

                    beneficiaryAccount = IGRAccount,

                    bankCode = IGRBankCode,

                    beneficiaryAmount = newIGRFee.ToString(),

                    deductFeeFrom = deductIGRFeeFrom

                });

                rp.Add(new RPartner

                {

                    lineItemsId = "3",

                    beneficiaryName = "Beneficiary 3",

                    beneficiaryAccount = TargetBankAccount,

                    bankCode = TargetBankCode,

                    beneficiaryAmount = newTgtFee.ToString(),

                    deductFeeFrom = "2"

                });

                #endregion
            }

            else //extra payment build partner

            {
                #region Take Over Extra Payment
                if (application.PhaseId == 6 && TransferCostAmount > 0) //TakeOver Extra Payment
                {
                    //IGR 5% Payment implementation

                    double TFCost = Convert.ToDouble(TransferCostAmount);

                    fivepercent = (5.00 / 100) * TFCost;

                    //Get what was initially posted to IGR
                    var trimIGRFee = application.PaymentDescription.ToLower().Contains("igr fee") ? application.PaymentDescription.Substring(application.PaymentDescription.LastIndexOf("IGR Fee:")) : null;
                    int IGRFee = string.IsNullOrEmpty(trimIGRFee) ? 0 : Convert.ToInt32(trimIGRFee.Split('N')[1].TrimEnd(Environment.NewLine.ToCharArray()));

                    //Now remove initial IGR fee from calculated five percent to get balance
                    fivepercent = fivepercent - IGRFee;

                    double checkfivepercent = fivepercent < Convert.ToDouble(amount) ? Convert.ToDouble(amount) - fivepercent : 0;

                    //Now get five value after check
                    fivepercent = checkfivepercent > 0 ? fivepercent : Convert.ToDouble(amount);

                    //Now get FG Amount 
                    var FGAmount = checkfivepercent > 0 ? (amount - Convert.ToDecimal(fivepercent)).ToString() : "0";

                    //Now get 5% of IGR fivepercent
                    IGRtenpercent = (0.05 * fivepercent) * 2 ; //10% of IGR fee

                    var ServiceCharge = 0.05 * fivepercent;

                    rmSplit.ServiceCharge = ServiceCharge.ToString();

                    //Get IGR Amount
                    fivepercent = (0.05 * fivepercent) + fivepercent - IGRtenpercent;

                    
                   
                    rmSplit.IGRFee = fivepercent.ToString();
                    rmSplit.totalAmount = (amount + int.Parse(rmSplit.ServiceCharge)).ToString();
                    rmSplit.AmountDue = (amount + int.Parse(rmSplit.ServiceCharge)).ToString();

                    double totalIGRFee = fivepercent + Convert.ToDouble(IGRtenpercent);
                    double newIGRFee = (0.9) * totalIGRFee;
                    double newTgtFee = totalIGRFee - newIGRFee;

                    rp.Add(new RPartner

                    {

                        lineItemsId = "1",

                        beneficiaryName = "Beneficiary 1",

                        beneficiaryAccount = IGRAccount,

                        bankCode = IGRBankCode,

                        beneficiaryAmount = newIGRFee.ToString(),

                        deductFeeFrom = "0"

                    });

                    rp.Add(new RPartner

                    {

                        lineItemsId = "2",

                        beneficiaryName = "Beneficiary 2",

                        beneficiaryAccount = TargetBankAccount,

                        bankCode = TargetBankCode,

                        beneficiaryAmount = newTgtFee.ToString(),

                        deductFeeFrom = "1"

                    });

                    if( FGAmount!="0" && FGAmount !="0.00")
                    {

                        rp.Add(new RPartner

                        {

                            lineItemsId = "3",

                            beneficiaryName = beneficiaryName,

                            bankCode = bankCode,

                            beneficiaryAccount = beneficiaryAccount,

                            beneficiaryAmount = FGAmount,

                            deductFeeFrom = "2"

                        });
                    }
                }
                #endregion
                else // Not take-over extra fee
                {
                    rp.Add(new RPartner

                    {

                        lineItemsId = "1",

                        beneficiaryName = beneficiaryName,

                        bankCode = bankCode,

                        beneficiaryAccount = beneficiaryAccount,

                        beneficiaryAmount = amount > 0 ? amount.ToString() : (application.fee_payable + application.service_charge).ToString(),

                        deductFeeFrom = deductFeeFrom


                });
                }
            }
            #endregion

            return rp;

        }



        public int GetBranch(int stateId, string ProcessLocationCode)
        {
            //UtilityHelper.LogMessages($"StateId: {stateId}");
            ProcessLocationCode = ProcessLocationCode.ToLower();
            var output = _restService.Response("/api/Branch/ZoneMapping/{email}/{apiHash}", null, null);

            var zonesmapping = JsonConvert.DeserializeObject<List<zoneModel>>(output.ToString());
            zoneModel selectedZone = null;
            selectedZone = (from u in zonesmapping
                            join z in _context.branches on u.branchId equals z.id
                            //join s in _context.countries on u.country_id equals s.id
                            join st in _context.States_UT on u.country_id equals st.Country_id
                            where st.State_id == stateId && z.branchCode.ToLower() != "hq"
                            select new zoneModel
                            {
                                stateId = st.State_id,
                                country_id = st.Country_id,
                                branchId = z.id,
                                code = z.branchCode

                            }).FirstOrDefault();

            if (ProcessLocationCode == "zn") // || ProcessLocationCode.ToLower() == "fd")
            {
                if (selectedZone == null)
                    return 0;

                return selectedZone.branchId;
            }
            else if (ProcessLocationCode == "fd" || ProcessLocationCode == "znfd")
            {
                try
                {
                    var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Select(s => s.StateId).Contains(stateId)).FirstOrDefault();
                    if (zn == null)
                    {
                        //No zone covers the supplied state, check if the zone itself is the FD
                        zn = zonesmapping.Where(a => a.stateId == stateId && a.code.ToLower() != "hq").FirstOrDefault();

                        return zn.branchId;
                    }
                    var fdId = zn.CoveredFieldOffices.Where(a => a.StateId == stateId).FirstOrDefault().StateId;
                    
                    if (ProcessLocationCode == "znfd")
                    {
                        fdId = zn.branchId;
                    }
                    return fdId;
                }
                catch (Exception)
                {
                    return 0;
                }

            }

            else if (ProcessLocationCode == "hq")
            {
                selectedZone = zonesmapping.Where(a => a.code.ToLower() == ProcessLocationCode).FirstOrDefault();

                if (selectedZone == null)
                    return 0;
                return selectedZone.branchId;
            }
            return 0;

        }

        public string GetBranchName(string stateName, string ProcessLocationCode)
        {
            ProcessLocationCode = ProcessLocationCode.ToLower();
            //Get all Zones
            var output = _restService.Response("/api/Branch/ZoneMapping/{email}/{apiHash}", null, null);
            if (output.ContentType == null)
            {
                return null;
            }
            var zonesmapping = JsonConvert.DeserializeObject<List<zoneModel>>(output.Content.ToString());

            List<UserBranches> selectedOps = new List<UserBranches>();
            zoneModel selectedZone = null;
            selectedZone = (from u in zonesmapping.AsEnumerable()
                                //join z in _context.branches on u.branchId equals z.id
                            join z in zonesmapping.AsEnumerable() on u.branchId equals z.branchId
                            join st in _context.States_UT on u.country_id equals st.Country_id
                            where st.StateName.ToLower() == stateName.ToLower() && z.code.ToLower() != "hq"
                            select new zoneModel
                            {
                                stateId = st.State_id,
                                country_id = st.Country_id,
                                branchId = z.branchId,
                                code = z.code,
                                branchName = z.name,
                                StateName = st.StateName
                            }).FirstOrDefault();

            if (ProcessLocationCode == "zn") 
            {
                selectedZone = zonesmapping.Where(a => a.StateName == stateName && a.code.ToLower() != "hq").FirstOrDefault();
                if (selectedZone == null)
                    return "";
                return selectedZone.branchName;
            }
            else if (ProcessLocationCode == "fd")
            {
                try
                {
                    var st = _context.States_UT.Where(x => x.StateName.ToLower() == stateName.ToLower()).FirstOrDefault();
                    var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Any(x => x.StateId== st.State_id)).FirstOrDefault();
                    if (zn == null)
                    {
                        //No zone covers the supplied state, check if the zone itself is the FD
                        zn = zonesmapping.Where(a => a.StateName == stateName && a.code.ToLower() != "hq").FirstOrDefault();
                        return zn?.branchName + "|ZN";
                    }
                    return zn?.CoveredFieldOffices.FirstOrDefault(x => x.StateId== st.State_id).Name;
                }
                catch (Exception x)
                {
                    LogMessages($"{stateName} did not return any value and error occured {x.Message.ToString()}");
                    return "";
                }

            }
            else if (ProcessLocationCode == "hq")
            {
                selectedZone = zonesmapping.Where(a => a.code.ToLower() == ProcessLocationCode).FirstOrDefault();

                if (selectedZone == null)
                    return "";
                return selectedZone.branchName;
            }
            return "";

        }
        //Application Prosessing
        #region Application Prosessing
        public string Assign(int Id, string UserName, string Ip, int userBranchId = 0, int sortOrder = 0)
        {

            try
            {
                var application = _context.applications.Where(a => a.id == Id).FirstOrDefault();
                var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                var companyName = _context.companies.Where(x => x.id == application.company_id).FirstOrDefault();
                var app = _context.MyDesk.Where(a => a.AppId == Id).OrderBy(a => a.Sort).ToList();

                var staff = _context.Staff.Where(x => x.StaffEmail == UserName).FirstOrDefault();
                if (staff == null)
                {
                    return "Staff doesn't exist!"; // throw (new Exception());

                }
                if (app.Count() <= 0)
                    return "Invalid Application!"; // throw (new Exception());
                var checker = app.FirstOrDefault();

                var getWorkFlow = _context.WorkProccess.Where(x => x.PhaseID == application.PhaseId).OrderBy(C => C.Sort).ToList();

                var getCurrentWF = _context.WorkProccess.Where(x => x.PhaseID == application.PhaseId && x.Sort == sortOrder + 1).FirstOrDefault();

                //Check if processing is still pending on a desk
                MyDesk processing = _context.MyDesk.Where(ap => ap.HasWork != true).FirstOrDefault();


                if (processing != null)
                {
                    string currentStaffEmail = _context.Staff.Where(x => x.StaffID == processing.StaffID).FirstOrDefault().StaffEmail;
                    return string.Format("Application is still pending on user {0} desk", currentStaffEmail); //throw new Exception();
                }
                if (getCurrentWF == null)
                {
                    application.status = GeneralClass.Approved;
                    application.current_desk = null;
                    _context.SaveChanges();
                    #region Update App Status on ELPS if Approved 

                    if (application.PhaseId == 6)
                    {
                        //Take Over
                        //get the facility, check if the companyId has been updated, else update and also push to elps
                        // to complete proper change of Ownership
                        var fac = _context.Facilities.Where(a => a.Id == application.FacilityId).FirstOrDefault();
                        if (fac != null && fac.CompanyId != application.company_id)
                        {

                            fac.CompanyId = application.company_id;
                            _context.SaveChanges();
                        }
                    }


                    using (WebClient clientL = new WebClient())
                    {
                        clientL.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        var appAPI = new ApplicationAPIModel();
                        appAPI.Status = application.status;
                        appAPI.OrderId = application.reference;

                        var param = JsonConvert.SerializeObject(appAPI);


                        var paramDatas = _restService.parameterData("model", param);
                        var responsee = _restService.Response("/api/Application/{model}/{email}/{apiHash}", paramDatas, "PUT");

                        var respApp = JsonConvert.DeserializeObject<ApplicationAPIModel>(responsee.ToString());
                    }
                    //}
                    #endregion

                    //Log in History
                    string comment = "Application Approved and License/Permit Issued";

                    SaveHistory(application.id, staff.StaffID, UserName, GeneralClass.FinalApproval, comment);


                    return "Application Processing Completed";
                }

                var getNextProcessingStaff = GetPushStaff(staff.StaffID);

                if (getNextProcessingStaff != null)
                {
                    string Location = _context.FieldOffices.Where(x => x.FieldOffice_id == getNextProcessingStaff.FirstOrDefault().FieldOffice).FirstOrDefault().OfficeName;

                    var facilityAddress = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    
                    string comment = "Application assigned to staff.";
                    SaveHistory(application.id, staff.StaffID, UserName, GeneralClass.Move, comment);

                    //Update applicaion table
                    application.current_desk = getNextProcessingStaff.FirstOrDefault().StaffId;
                    _context.SaveChanges();


                    if ((application.type.ToLower() == "renew") && getCurrentWF.Sort == 1)
                    {
                        // 5 for Facility Modification; 9 for Modification without approval; 
                        // 11 for Recalibration of tanks

                        LogMessages("Push and notify OPSCON and ADOPS.");

                        var requiredStaff = (from u in _context.Staff
                                             join r in _context.UserRoles on u.RoleID equals r.Role_id
                                             where r.RoleName.ToLower() == "opscon" || r.RoleName.ToLower() == "adops"
                                             && u.DeleteStatus != true && u.ActiveStatus != false
                                             select u).ToList();

                        //Push applications to opscon,AdOps,
                        NotifyZNFD(application.id, facilityAddress.StateId, requiredStaff, UserName);
                    }

                    return "Ok";
                }
                else
                {
                    return "false";

                }
            }
            catch (Exception x)
            {
                LogMessages("From Assign Next Staff- AppId {Id} User {UserName} ::: {x.ToString()}");
                return "not ok";
            }
        }

        //public bool NotifyZNFD(int id, int stateId, List<vUserBranch> opscons, string username, string ip)
        //{
        //    // Get Opscon to handle
        //    //UtilityHelper.LogMessages($"Yes, we are here to add to notify opscon");

        //    #region get the Staff to be added
        //    //Get all Zones
        //    var client = new WebClient();
        //    string output = client.DownloadString(ELPSAPIHelper.ApiBaseUrl + "Branch/ZoneMapping/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash);
        //    var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);

        //    List<vUserBranch> selectedOps = new List<vUserBranch>();
        //    vZone selectedZone = null;
        //    selectedZone = zonesmapping.Where(a => a.StateId == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
        //    if (selectedZone == null)
        //    {
        //        //Loop thru the states in each zone for possible selection
        //        selectedZone = zonesmapping.Where(a => a.CoveredStates.Select(s => s.StateId).Contains(stateId)).FirstOrDefault();
        //        if (selectedZone != null)
        //        {
        //            //Found one zone.
        //            selectedOps = opscons.Where(a => a.BranchId == selectedZone.BranchId).ToList();

        //        }
        //    }
        //    else
        //    {
        //        selectedOps = opscons.Where(a => a.BranchId == selectedZone.BranchId).ToList();

        //    }

        //    #endregion

        //    if (selectedOps != null && selectedOps.Count > 0 && selectedZone != null)
        //    {
        //        foreach (var ops in selectedOps)
        //        {


        //            var checkJoint = _jointRep.Where(a => a.ApplicationId == id && a.Opscon == ops.UserEmail).FirstOrDefault();
        //            if (checkJoint == null)
        //            {

        //                var joint = new JointAccount();
        //                joint.ApplicationId = id;
        //                joint.DateAdded = DateTime.Now;
        //                joint.OperationsCompleted = false;
        //                joint.Opscon = ops.UserEmail;
        //                joint.Assigned = false;
        //                _jointRep.Add(joint);
        //                _jointRep.Save(username, ip);
        //            }
        //        }

        //        #region Send Mail to Notify Opscon and Supervisor
        //        var body = "";
        //        using (var sr = new StreamReader(HttpContext.Current.Server.MapPath(@"\\App_Data\\Templates\") + "InternalMemo.txt"))
        //        {
        //            body = sr.ReadToEnd();
        //        }
        //        var subject = "New Application on Depot Portal";
        //        var context = new NMDPRA_DepotContext();
        //        var vApp = context.vApplications.Where(a => a.Id == id).FirstOrDefault();
        //        foreach (var ops in selectedOps)
        //        {
        //            var stf = context.Staffs.Where(a => a.UserId == ops.UserEmail).FirstOrDefault();
        //            //var man = _staffRepo.Where(a => a.UserId == manager.UserEmail).FirstOrDefault();
        //            var type = (vApp.Type.ToLower() == "new" ? "New Depot License" : "Depot License Renewal").ToString() + "(" + vApp.PhaseName + ")";
        //            var msg = $"A new application on NMDPRA Depot Portal for " + type + " has been submitted on the portal and you are hereby notified for Joint Operation towards the issuing of the License/Approval. <p>Details of the Application is as follow:</p>";
        //            msg += "<table class'table'>" +
        //                $"<tr><td>Application Reference</td><td><a href='{elpsBaseURL}/Process/ViewApplication/" + vapp.id + "'>" + vApp.Reference + "</a></td></tr>" +
        //                $"<tr><td>Application Company</td><td><a href='{elpsBaseURL}/Company/Detail/" + vApp.Company_Id + "'>" + vApp.CompanyName + "</a></td></tr>" +
        //                $"<tr><td>Facility</td><td><a href='{elpsBaseURL}/Facility/ViewFacility/" + vApp.FacilityId + "'>" + vApp.FacilityName + "(" + vApp.FacilityAddress() + ")</a></td></tr>" +
        //                "<tr><td>Facility Address</td><td>" + vApp.FacilityFullAddress() + "</td></tr>" +
        //                "</table><br /><br /><p>You will be notified on the progress and Action required by you as the application process progresses.</p>";
        //            var msgBody = string.Format(body, subject, "", ops.FirstName, msg);

        //            MailHelper.SendEmail(ops.UserEmail, subject, msgBody);
        //        }
        //        #endregion

        //        return true;
        //    }
        //    return false;
        //}

        //public int GetBranch(int stateId, string ProcessLocationCode)
        //{
        //    //UtilityHelper.LogMessages($"StateId: {stateId}");
        //    ProcessLocationCode = ProcessLocationCode.ToLower();
        //    var client = new WebClient();
        //    string output = client.DownloadString(ELPSAPIHelper.ApiBaseUrl + "Branch/ZoneMapping/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash);
        //    var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);
        //    vZone selectedZone = null; //
        //                               // UtilityHelper.LogMessages($"Zones: {JsonConvert.SerializeObject(zonesmapping)}");
        //    if (ProcessLocationCode == "zn") // || ProcessLocationCode.ToLower() == "fd")
        //    {
        //        selectedZone = zonesmapping.Where(a => a.StateId == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
        //        if (selectedZone == null)
        //            return 0;
        //        //UtilityHelper.LogMessages($"From first Zones: {JsonConvert.SerializeObject(selectedZone)}");

        //        return selectedZone.BranchId;
        //    }
        //    else if (ProcessLocationCode == "fd" || ProcessLocationCode == "znfd")
        //    {
        //        try
        //        {
        //            var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Select(s => s.StateId).Contains(stateId)).FirstOrDefault();
        //            if (zn == null)
        //            {
        //                //No zone covers the supplied state, check if the zone itself is the FD
        //                zn = zonesmapping.Where(a => a.StateId == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
        //                //UtilityHelper.LogMessages($"From FD Inner: {JsonConvert.SerializeObject(zn)}");

        //                return zn.BranchId;
        //            }
        //            var fdId = zn.CoveredFieldOffices.Where(a => a.StateId == stateId).FirstOrDefault().Id;
        //            //UtilityHelper.LogMessages($"From FD : {JsonConvert.SerializeObject(zn)}");
        //            if (ProcessLocationCode == "znfd")
        //            {
        //                // UtilityHelper.LogMessages("znfd");
        //                fdId = zn.BranchId;
        //            }
        //            return fdId;
        //        }
        //        catch (Exception)
        //        {
        //            return 0;
        //        }

        //    }

        //    else if (ProcessLocationCode == "hq")
        //    {
        //        selectedZone = zonesmapping.Where(a => a.Code.ToLower() == ProcessLocationCode).FirstOrDefault();

        //        if (selectedZone == null)
        //            return 0;
        //        return selectedZone.BranchId;
        //    }
        //    return 0;

        //}

        //public static string GetBranchName(string stateId, string ProcessLocationCode)
        //{
        //    // UtilityHelper.LogMessages($"State name :{stateId}");
        //    ProcessLocationCode = ProcessLocationCode.ToLower();
        //    var client = new WebClient();
        //    string output = client.DownloadString(ELPSAPIHelper.ApiBaseUrl + "Branch/ZoneMapping/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash);
        //    //UtilityHelper.LogMessages(output);
        //    var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);
        //    vZone selectedZone = null; //
        //    if (ProcessLocationCode == "zn") // || ProcessLocationCode.ToLower() == "fd")
        //    {
        //        selectedZone = zonesmapping.Where(a => a.StateName == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
        //        if (selectedZone == null)
        //            return "";
        //        return selectedZone.BranchName;
        //    }
        //    else if (ProcessLocationCode == "fd")
        //    {
        //        try
        //        {
        //            var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Any(x => x.StateName.Contains(stateId))).FirstOrDefault();
        //            //var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Select(s => s.StateName).Contains(stateId)).FirstOrDefault();
        //            if (zn == null)
        //            {
        //                // UtilityHelper.LogMessages($"{stateId} did not return any value");
        //                //No zone covers the supplied state, check if the zone itself is the FD
        //                zn = zonesmapping.Where(a => a.StateName == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
        //                return zn?.BranchName + "|ZN";
        //            }
        //            //UtilityHelper.LogMessages(JsonConvert.SerializeObject(zn));
        //            //var fdId = zn.CoveredFieldOffices.Where(a => a.StateName == stateId).FirstOrDefault().Name;
        //            return zn?.CoveredFieldOffices.FirstOrDefault(x => x.StateName.Contains(stateId)).Name;
        //        }
        //        catch (Exception x)
        //        {
        //            UtilityHelper.LogMessages($"{stateId} did not return any value and error occured {x.ToString()}");


        //            return "";
        //        }

        //    }
        //    else if (ProcessLocationCode == "hq")
        //    {
        //        selectedZone = zonesmapping.Where(a => a.Code.ToLower() == ProcessLocationCode).FirstOrDefault();

        //        if (selectedZone == null)
        //            return "";
        //        return selectedZone.BranchName;
        //    }
        //    return "";

        //}

        #endregion


        #region PermitGenerator

        public string CreatePermit(applications app, Facilities fac, string r = "")
        {
            try
            {
                #region Create The Permit
                var getAppPhase = _context.Phases.Where(p => p.id == app.PhaseId).FirstOrDefault();
                var getAppCat = _context.Categories.Where(p => p.id == app.category_id).FirstOrDefault();
                var company = _context.companies.Where(p => p.id == app.company_id).FirstOrDefault();

                var newLicenseValue = _configuration.GetSection("AmountSetting").GetSection("NewLicenseValue").Value.ToString();
                var suitExpiry = _configuration.GetSection("AmountSetting").GetSection("suitabilityExpiry").Value.ToString();
                var atcExpiry = _configuration.GetSection("AmountSetting").GetSection("atcExpiry").Value.ToString();

                var useNewPermitNo = newLicenseValue;
                string pm = "";
                var elps_Permit = new PermitAPIModel();
                string letterTemplate = "";
                var tenure = 0;
                permits permit = new permits();

                permit.application_id = app.id;
                permit.company_id = app.company_id;
                permit.date_issued = DateTime.Now;
                bool TOfrmATC = false;
                if (getAppPhase.name.ToLower() == "take over" && app.PaymentDescription.Contains("From ATC"))
                {
                    TOfrmATC = true;
                }
                if (getAppPhase.name.ToLower() == "Suitability Inspection".ToLower())
                {
                    letterTemplate = "SuitabilityLetter.txt";
                    tenure = Convert.ToInt16(suitExpiry);
                    permit.date_expire = DateTime.Now.AddDays(5000);
                }
                else if (getAppPhase.name.ToLower() == "Approval To Construct".ToLower())
                {
                    letterTemplate = "ATCLetter.txt";
                    tenure = Convert.ToInt16(atcExpiry);
                    permit.date_expire = DateTime.Now.AddDays(tenure);
                }
                else if (getAppPhase.name.ToLower() == "Regularization".ToLower())
                {
                    letterTemplate = "RegularizationLetter.txt";
                    tenure = Convert.ToInt16(atcExpiry);
                    permit.date_expire = DateTime.Now.AddDays(tenure);
                }
                else if (getAppPhase.name.ToLower() == "license to operate" || getAppPhase.name.ToLower() == "license renewal" || getAppPhase.name.ToLower() == "take over")
                {
                    letterTemplate = "NDTs.txt";
                    //permit.date_expire = new DateTime(app.CreatedAt, 12, 31);// DateTime.Now.AddYears(1); // DateTime.Parse("12/31/" + DateTime.Now.Year);
                    permit.date_expire = new DateTime(app.year, 12, 31);
                    //use current year for expiration jare.
                    permit.date_expire = new DateTime(DateTime.Now.Year, 12, 31);

                    int noOfYr = 0;
                    //int dt = DateTime.Now.Year;
                    if (!string.IsNullOrEmpty(app.current_Permit))
                    {
                        var p = app.current_Permit.Split('/');
                        if (p[0] == "NMDPRA" || p[0] == "DPR")
                        {
                            //Platform Permit, Lets look for it and get the Year issued
                            //NMDPRA/PDJ/18/N3080
                            var x = app.year.ToString().Substring(0, 2) + p[p.Length - 2];
                            int yr = 0;
                            if (int.TryParse(x, out yr))
                            {
                                noOfYr = app.year - yr;
                            }
                        }
                        else
                        {
                            int yr = 0;
                            if (int.TryParse(p[p.Length - 1], out yr))
                            {
                                noOfYr = app.year - yr;
                            }
                        }
                        //DEP00085/2017
                    }


                    if (noOfYr == 0)
                    {
                        //permit.date_expire = permit.date_expire.AddYears(1);
                        permit.date_expire = permit.date_expire;
                    }
                    if (permit.date_expire.Year - DateTime.Now.Year < 0)
                    {
                       //permit.date_expire = new DateTime(DateTime.Now.Year, 12, 31).AddYears(5).AddHours(-3);
                        permit.date_expire = new DateTime(DateTime.Now.Year, 12, 31).AddYears(1).AddHours(-3);
                    }

                }
                else if (getAppPhase.name.ToLower() == "Calibration/Integrity Tests(NDTs)".ToLower())
                {
                    letterTemplate = "NDTs.txt";
                    //var currentMonth = UtilityHelper.CurrentTime.Month;
                    //var rem = 12 - currentMonth;
                    permit.date_expire = new DateTime(app.date_added.Year, 1, 1).AddYears(5).AddHours(-3);// DateTime.Now.AddYears(1); // DateTime.Parse("12/31/" + DateTime.Now.Year);
                }
                else
                {
                    letterTemplate = "NDTs.txt";
                    if (app.PhaseId == 11)
                    {
                        permit.date_expire = new DateTime(DateTime.Today.Year, 1, 1).AddYears(5).AddHours(-3);
                    }
                    else
                    {
                        permit.date_expire = new DateTime(app.date_added.Year, 12, 31); 
                        int noOfYr = 0;
                        if (!string.IsNullOrEmpty(app.current_Permit))
                        {
                            var p = app.current_Permit.Split('/');
                            if (p[0] == "NMDPRA" || p[0] == "DPR")
                            {
                                //Platform Permit, Lets look for it and get the Year issued
                                //NMDPRA/PDJ/18/N3080
                                var x = app.year.ToString().Substring(0, 2) + p[p.Length - 2];
                                int yr = 0;
                                if (int.TryParse(x, out yr))
                                {
                                    noOfYr = app.year - yr;
                                }
                            }
                            else
                            {
                                int yr = 0;
                                if (int.TryParse(p[p.Length - 1], out yr))
                                {
                                    noOfYr = app.year - yr;
                                }
                            }
                            //DEP00085/2017
                        }


                        if (noOfYr == 0)
                        {
                            permit.date_expire = permit.date_expire.AddYears(1);
                        }
                    }
                }

                permit.categoryName = getAppCat.name + "(" + getAppPhase.name + ")";

                var application = _context.applications.Where(a => a.id == app.id).FirstOrDefault();
                if (permit.date_issued > permit.date_expire)
                {
                    permit.date_expire = new DateTime(permit.date_issued.Year, 12, 31);
                }
                if (r == "ge")
                {
                    if (application.CreatedAt.Value.Month >= 9)
                    {
                        permit.date_expire = new DateTime(application.year + 1, 12, 31).AddHours(23);
                    }
                    else
                    {
                        permit.date_expire = new DateTime(application.year, 12, 31).AddHours(23);
                    }
                }

                //if (!string.IsNullOrEmpty(application.current_Permit))            Old Method
                if (!string.IsNullOrEmpty(application.type) && application.type.Trim().ToLower() == "renew")
                {   
                    //Renew Permit
                    if (useNewPermitNo == "Yes" && permit.date_expire > new DateTime(2019, 12, 31).AddDays(1))
                    {
                        permit.permit_no = GenerateNewPermitNo(app.id, "R", fac.CategoryCode, permit.date_expire.Year.ToString(), app.PhaseId);
                    }
                    else
                    {
                        permit.permit_no = GeneratePermitNo(app.id, "R", permit.date_expire, app.category_id, app.PhaseId);
                    }
                    elps_Permit.Is_Renewed = "renew";

                    //Update old permit to Completed status
                    if (!string.IsNullOrEmpty(application.current_Permit))
                    {
                        var oldPermit = _context.permits.Where(a => a.permit_no.ToLower() == application.current_Permit.ToLower()).FirstOrDefault();
                        if (oldPermit != null)
                        {
                            oldPermit.is_renewed = "Completed";
                            _context.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (fac.IdentificationCode == null)
                    {
                        //generate facility identification code
                        var getCode = getFacilityCategoryFromROMS(Convert.ToInt32(company.elps_id), company.CompanyEmail);
                    }
                    //New Permit
                    if (useNewPermitNo == "Yes" && permit.date_expire > new DateTime(2019, 12, 31).AddDays(1))
                    {
                      permit.permit_no = GenerateNewPermitNo(app.id, "N", fac.CategoryCode, permit.date_expire.Year.ToString(), app.PhaseId);

                    }
                    else
                     {
                     permit.permit_no = GeneratePermitNo(app.id, "N", permit.date_expire, app.category_id, app.PhaseId);
                     }
                    elps_Permit.Is_Renewed = "new";
                    
                }


                _context.permits.Add(permit);
                _context.SaveChanges();

                //Push Permit to ELPS
                elps_Permit.CategoryName = getAppCat.name;
                elps_Permit.Company_Id = _context.companies.Where(c => c.id == application.company_id).FirstOrDefault().elps_id.GetValueOrDefault();
                elps_Permit.Date_Expire = permit.date_expire;
                elps_Permit.Date_Issued = permit.date_issued;
                elps_Permit.OrderId = application.reference;
                elps_Permit.Permit_No = permit.permit_no;
                elps_Permit.Id = permit.id;
                int elpsPermitId = 0;
                
                if (!PostPermit(elps_Permit, elpsPermitId))
                {
                    throw new ArgumentException("Error Pushing Permit to ELPS!");
                }


                var facilityPermit = new FacilityPermit
                {
                    IsRenewed= (getAppPhase.ShortName!= "LR" && getAppPhase.ShortName != "RC")? false:true,
                    FacilityID = fac.Id,
                    ElpsID = (int)fac.Elps_Id,
                    ApplicationID = app.id,
                    Type = app.type,
                    FacilityName = fac.Name,
                    CompanyName = company.name,
                    CompanyID = company.id,
                    PhaseName = getAppPhase.name,
                    CategoryID = getAppCat.id,
                    PermitNo = permit.permit_no,
                    DateIssued = permit.date_issued,
                    DateExpired = permit.date_expire
                };
                _context.FacilityPermit.Add(facilityPermit); _context.SaveChanges();
                #endregion
                pm = permit.permit_no;
                #region send Mail
                if (r != "re" && r != "ge")
                {
                    var date = DateTime.Now;
                    var dt = date.Day.ToString() + date.Month.ToString() + date.Year.ToString();
                    var sn = string.Format("NMDPRA/PDJ/{0}/{1}", dt, application.company_id);
                    var body = "";
                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string file = up + @"\\Templates\" + letterTemplate;
                    using (var sr = new StreamReader(file))
                    {

                        body = sr.ReadToEnd();
                    }

                    var topCaption = "";
                    string subject = ApprovalSubject(out topCaption, app.PhaseId, TOfrmATC);

                    var msgBody = "";

                    if (getAppPhase.name.ToLower() == "Approval To Construct".ToLower())
                    {
                        msgBody = string.Format(body, subject, company.name, permit.permit_no, permit.date_issued.ToShortDateString(), permit.date_expire.ToShortDateString(), fac.Name + " (" + fac.address_1 + ")", DateTime.Now.Year, tenure + " days");
                    }
                    else
                    {
                        topCaption += "<br />Details of the Facility is as follow:";
                        var tenureStr = getAppPhase.name.ToLower() == "suitability" ? tenure + " days" : "NA";//not Available Yet
                        msgBody = string.Format(body, subject, company.name, permit.permit_no, permit.date_issued.ToShortDateString(), permit.date_expire.ToShortDateString(), fac.Name + " (" + fac.address_1 + ")", DateTime.Now.Year, tenureStr, topCaption);
                    }

                    var emailMsg = SaveMessage(app.id, app.company_id, subject, msgBody, company.elps_id.ToString(), "Company");
                    var sendEmail = SendEmailMessage(company.CompanyEmail, company.name, emailMsg, null);

                }

                #endregion

                return pm;
            }
            catch (Exception x)
            {
                LogMessages(x.ToString());
                return x.ToString();// return "Error";
            }
        }

        private string ApprovalSubject(out string note, int phaseId = 0, bool toFrmATC = false)
        {
            note = "Congratulations! ";

            if (phaseId == 1)
            {
                note += "A new suitability letter has been generated in favour of your facility.";
                return "Suitability Letter for New Facility";
            }
            else if (phaseId == 2)
            {
                note += "A new depot approval - \"Approval To Construct (ATC)\" approved in favour of your facility.";
                return "Approval To Construct Approved";
            }
            else if (phaseId == 3)
            {
                note += "A new depot approval - \"Calibration/Integrity Tests(NDTs)\" approved in favour of your facility.";
                return "Calibration/Integrity Tests(NDTs) Approved";
            }
            else if (phaseId == 5 || phaseId == 9)
            { //Facility Modification and Modification without Approval
                note += "A new depot approval - \"Depot Modification\" approved in favour of your facility.";
                return "Depot Modification Approved";
            }//

            else if (phaseId == 8)
            {
                note += "A new depot approval - \" (ATC)\"Regularization approved in favour of your facility.";
                return "Regularization Approved";
            }
            else if (phaseId == 6)
            {
                if (toFrmATC)
                {
                    note += "A new depot approval - \"Approval To Construct (ATC) from Take Over\" approved in favour of your facility.";
                    return "Approval To Construct Approved (TO)";
                }
                else
                {
                    note += "A new depot license - \"Take Over\" for \"License To Operate (LTO)\" approved in favour of your facility.";
                    return "Take Over Approved for \"License To Operate (LTO)\"";


                }
            }
            else if (phaseId == 11)
            {
                note += "A new depot approval - \"Calibration/Integrity Tests(NDTs)\" approved in favour of your facility.";
                return "Re-Calibration Approved";

            }
            else if (phaseId == 10)
            {
                note += "A new depot approval - \"Selling Above Ex-Deport Price\"  Sanction Payment Accepted";
                return "Selling Above Ex-Deport Price Sanction Payment Accepted";

            }
            else
            {
                note += "A new depot license - \"License To Operate (LTO)\" approved in favour of your facility.";
                return "License To Operate Approved";
            }
        }

        private string GeneratePermitNo(int appId, string status, DateTime expYear, int catId = 0, int phaseId = 0, bool tofrmATC = false)
        {
            string no = "NMDPRA/PDJ/";
            string touse = string.Empty;

            Random rnd = new Random();

        generate:
            //00001 - 99999
            int digits = rnd.Next(10001, 99999);

            //if (catId == 2)
            //{
            switch (phaseId)
            {
                case 1:
                    no += "SUI/";
                    break;
                case 2:
                    no += "ATC/";
                    break;
                case 8:
                    no += "REG/";
                    break;
                case 3:
                case 11:
                    no += "PLT/";
                    break;
                case 4:
                case 7:
                    no += "LTO/";
                    break;
                case 5:
                case 9:
                    no += "MD/";
                    break;
                case 6:

                    no += "TO/";

                    break;
                case 10:
                    no += "SA/";
                    break;
                default:
                    break;
            }
            if (string.IsNullOrEmpty(status) && status.ToLower() == "r")
                no += expYear.Year.ToString().Substring(2, 2) + "/R{0}";
            else//
                no += expYear.Year.ToString().Substring(2, 2) + "/N{0}";

            touse = string.Format(no, digits.ToString("00000"));

            var check = _context.permits.Where(p => p.permit_no.ToLower() == touse.ToLower()).FirstOrDefault();
            //Check if the NO is not existing
            if (check == null)
            {
                return touse;
            }
            else
                goto generate;
        }


        private string GenerateNewPermitNo(int appId, string status, string facCat, string year, int phaseId = 0)
        {
            //string no = "NMDPRA/PDJ/";
            string no = $"NMDPRA/";
            bool approval = false;
            string touse = string.Empty;
            switch (phaseId)
            {
                case 1:
                    no += "SUI";
                    approval = true;
                    break;
                case 2:
                    no += "ATC";
                    approval = true;
                    break;
                case 8:
                    no += "REG";
                    approval = true;
                    break;
                case 3:
                case 11:
                    no += "PLT";
                    approval = true;
                    break;
                case 4:
                case 6:
                case 7:
                    no += "";
                    break;
                case 5:
                case 9:
                    no += "MD";
                    approval = true;
                    break;
                case 10:
                    no += "SA";
                    approval = true;
                    break;
                default:
                    break;
            }
            if (!approval)
            {
                no += $"{status.ToUpper()}/{facCat.ToUpper()}";
            }
            Random rnd = new Random();
        generate:
            //00001 - 99999

            // var lastPermit = _permit.GetAll().OrderByDescending(a=>a.date_expire).FirstOrDefault();

            int digits = rnd.Next(1, 999);
            no += $"/{digits.ToString("000")}/{year}";
            touse = no;// string.Format(no, digits.ToString("000"));

            var check = _context.permits.Where(p => p.permit_no.ToLower() == touse.ToLower()).FirstOrDefault();
            //Check if the NO is not existing
            if (check == null)
            {
                return touse;
            }
            else
                goto generate;
        }
        public string GenerateNewPermitNo_Preview(int appId, string status, string facCat, string year, int phaseId = 0)
        {
            //string no = "NMDPRA/PDJ/";
            string no = $"PRE_NMDPRA/";
            bool approval = false;
            string touse = string.Empty;
            switch (phaseId)
            {
                case 1:
                    no += "SUI";
                    approval = true;
                    break;
                case 2:
                    no += "ATC";
                    approval = true;
                    break;
                case 8:
                    no += "REG";
                    approval = true;
                    break;
                case 3:
                case 11:
                    no += "PLT";
                    approval = true;
                    break;
                case 4:
                case 6:
                case 7:
                    no += "";
                    break;
                case 5:
                case 9:
                    no += "MD";
                    approval = true;
                    break;
                case 10:
                    no += "SA";
                    approval = true;
                    break;
                default:
                    break;
            }
            if (!approval)
            {
                no += $"{status.ToUpper()}/{facCat.ToUpper()}";
            }
            Random rnd = new Random();
        
            int digits = rnd.Next(1, 999);
            no += $"/{digits.ToString("000")}/{year}";
            touse = no;
            
            return touse;
       
        }

        //#endregion

        #region Permit
        /*
         * Posting approved permit to elps.
         */
        public bool PostPermit(PermitAPIModel model, int elpsid)
        {
            var values = new JObject();
            values.Add("permit_No", model.Permit_No);
            values.Add("orderId", model.OrderId);
            values.Add("company_Id", model.Company_Id.ToString());
            values.Add("date_Issued", model.Date_Issued.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            values.Add("date_Expire", model.Date_Expire.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            values.Add("categoryName", model.CategoryName);
            values.Add("is_Renewed", (model.Is_Renewed == "true") ? "Yes" : "No");
            values.Add("licenseId", model.Id.ToString());
            values.Add("id", 0);

            List<JObject> newDocs = new List<JObject>();

            var paramData = restSharpServices.parameterData("CompId", model.Company_Id.ToString());
            var savePermit = restSharpServices.Response("api/Permits/{CompId}/{email}/{apiHash}", paramData, "POST", values);

            if (savePermit.IsSuccessful)
            {
                JObject eplsPermit = JsonConvert.DeserializeObject<JObject>(savePermit.Content);

                var permit = _context.permits.Where(x => x.id == model.Id);

                if (permit.Count() > 0)
                {
                    permit.FirstOrDefault().elps_id = (int)eplsPermit.SelectToken("id");
                    _context.SaveChanges();
                    return true;
                }
                else
                {

                    var vApp = _context.applications.Where(a => a.reference == model.OrderId).FirstOrDefault();
                    if (vApp != null)
                    {
                        var fac = _context.Facilities.Where(a => a.Id == vApp.FacilityId).FirstOrDefault();
                        CreatePermit(vApp, fac, "re");
                        
                            var permity = _context.permits.Where(x => x.application_id == vApp.id);

                            if (permity.Count() > 0)
                            {
                                permity.FirstOrDefault().elps_id = (int)eplsPermit.SelectToken("id");
                                _context.SaveChanges();
                                return true;
                            }
                        
                    }
                    return true;

                }
            }
            else
            {
                return false;
            }
        }


        #endregion


        public static async Task<string> postExternalTest(string api, object values)
        {
            using (var client = new HttpClient())
            {
                var IGR_URL = _configuration.GetSection("RemitaSplit").GetSection("IGR_URL").Value.ToString();
                var bearer = _configuration.GetSection("RemitaSplit").GetSection("Bearer").Value.ToString();

                var baseUri = IGR_URL + "/api/";
                client.BaseAddress = new Uri(baseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new
               MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new
               AuthenticationHeaderValue("Bearer", bearer.ToString());
                string output = JsonConvert.SerializeObject(values);
                var response = await client.PostAsync(api,
                new StringContent(output, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return responseJson;
                }
                return null;
            }
        }


        public string PostReferenceToIGR(string baseUrl, string requestUri, object payload)
        {
            try
            {
                var bearer = _configuration.GetSection("RemitaSplit").GetSection("Bearer").Value.ToString();

                var resp = Send(baseUrl, new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"),
                    Headers = { Authorization =
                        new AuthenticationHeaderValue("Bearer", bearer.ToString())}
                }).Result;

                if (resp.IsSuccessStatusCode)
                    return resp.Content.ReadAsStringAsync().Result;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }
        public static async Task<HttpResponseMessage> Send(string url, HttpRequestMessage message)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                return await client.SendAsync(message);
            }
        }






        #region Build PDF
        public string DMLetter(DM_ATCLetter ATCMessage, string tnkCount, string tnkWords)
        {
            var msg = ATCMessage;
            string body = "<div>";
            string authorityOrDepartment = "department";
            string organization = "Department of Petroleum Resources";
            string organizationAcronym = "DPR";

            if(ATCMessage.DateApproved >= GeneralClass.DPR_ChangeDate)
            {
                authorityOrDepartment = "authority";
                organization = "Nigerian Midstream and Downstream Petroleum Regulatory Authority";
                organizationAcronym = "NMDPRA";

            }
            body += "<p><u>RE: APPROVAL TO MODIFY PETROLEUM PRODUCTS STORAGE DEPOT</u></p>";
            body += "<p style='text-align: justify;'>We refer to your application of " + msg.DateApplied + " for Approval To Modify a bulk storage depot facility at " + msg.FacilityAddress + "&nbsp; and the subsequent site inspection visit carried out on " + msg.ScheduleDate + ".</p>";
            if (ATCMessage.ModifyType == "ATC")
            {
                body += "<p style='text-align: justify;'>In view of the satisfactory report from the visit and the consent obtained from other appropriate Government Agencies, I am pleased to inform you that you are hereby granted &nbsp;<b> Approval to Construct</b> your " + tnkCount + " (" + tnkWords + ") additional storage tank(s) with the following product type/capacities:</p></div>";
            }
            if (ATCMessage.ModifyType.Contains("Decommission"))
            {
                body = "<p><u>RE: APPROVAL TO DECOMMISSION PETROLEUM PRODUCTS STORAGE TANK(S)</u></p>";
                body += "<p style='text-align: justify;'>We refer to your application of " + msg.DateApplied + " for Approval To Modify a bulk storage depot facility at &nbsp;" + msg.FacilityAddress + " &nbsp; and the subsequent site inspection visit carried out on " + msg.ScheduleDate + ".</p>";
                body += "<p style='text-align: justify;'>In view of the satisfactory report from the from the facility inspection, I am pleased to inform you that you are hereby granted &nbsp;<b> Approval to Decommission</b> your " + tnkCount + " (" + tnkWords + ") storage tank(s) with the following product type / capacities: <p>";
            }
            else
            {
                body += "<p style='text-align: justify;'>In view of the satisfactory report from the visit and the consent obtained from other appropriate Government Agencies, I am pleased to inform you that you are hereby granted &nbsp;<b> Approval to Modify</b> your " + tnkCount + " (" + tnkWords + ") storage tank(s) with the following product type/capacities:</p></div>";
            }
            body += "<table style='width:100%;border-spacing:0;margin-top:25px;'>";
            body += "<tr><th>S/N</th><th>TANK</th><th>PRODUCT</th><th>DIAMETER(meters)</th><th>HEIGHT(meters)</th><th>CAPACITY(litres)</th><th>REMARK</th></tr>";
            body += msg.TanksText;
            body += "</tbody></table>";

            if (ATCMessage.ModifyType.Contains("Decommission"))
            {
                body += "<p>Please note the following; </p>";
                body += "<p>No reconstruction of decommissioned storage tank shall be carried without approval from The Authority.</p>";
                body += "<p>In addition to the above, The Authority representatives will be nominated to monitor the decommissioning work and to ensure that all standards are maintained.</p>";
                body += "<p>You are therefore requested to strictly adhere to the above as the issuance of an updated operating license for the facility will be dependent on total compliance.</p>";
            }
            else {
                body += $"<div><ol class='contentTable'><li style='page -break-inside:avoid'>The construction shall be in line with the approved drawings submitted to the {authorityOrDepartment} and in accordance with relevant provisions of Petroleum Regulations; </li>";
                body += "<li style='page -break-inside:avoid'>All impact mitigation/monitoring in the Environmental Evaluation Report (E.E.R) as indicated in the Environmental Management Plan are to be comprehensively implemented during the construction and operation phases.</li> ";
                body += $"<li style='page -break-inside:avoid'>You shall officially inform the {authorityOrDepartment} of each milestone, particularly pipeline laying, tank/pipeline hydro - tests, and calibration exercises.</li> ";
                #region  ATK, PMS, Bitumen
                if (msg.TanksText.ToLower().Contains("pms"))
                {
                    body += "<li style='page -break-inside:avoid'> The PMS storage tank MUST be fitted with internal floating blanket</li> ";
                }
                if (msg.TanksText.ToLower().Contains("atk")) {
                    body += "<li style='page -break-inside:avoid'> All ATK tanks MUST be filled with an internal floating suction system.</li> ";
                    body += "<li style='page -break-inside:avoid'> The ATK Storage tanks and piping network MUST be expoxy coated internally.</li> ";
                    body += "<li style='page -break-inside:avoid'> All ATK receipt and discharge points MUST have comprehensive filtration systems.</li> ";
                }
                if (msg.TanksText.ToLower().Contains("bitumen")) {

                    body += "<li style='page -break-inside:avoid'> The Bitumen storage tanks MUST be properly lagged.</li> ";
                    body += "<li style='page -break-inside:avoid'> All Bitumen pipelines MUST be insulated with appropriate thermal insulating materials.</li> ";
                }
                #endregion

                body += $"<li style='page -break-inside:avoid'> At the completion of the project, a detailed operational safety case shall be carried out by a {organizationAcronym} Accredited Consultant.</li> ";
                body += $"<li style='page -break-inside:avoid'> You shall inform the {authorityOrDepartment} on completion of the construction work for the pre-commissioning inspection prior to the issuance of operating licence.</li> </ol>";


                body += $"<p style='text-align: justify;'>  In addition to the above, {organizationAcronym} representatives will be nominated to monitor the construction work and to ensure that all standards are maintained. You are therefore requested to strictly adhere to the above as the issuance of an updated operating license for the facility will be dependent on total compliance. </p> ";
                body += "</div>";
            }

            return body;
        }
        public string ATCLetter(DM_ATCLetter ATCMessage)
        {
            var msg = ATCMessage;
            string body = "<div>";

            string authorityOrDepartment = "department";
            string organization = "Department of Petroleum Resources";
            string organizationAcronym = "DPR";

            if (ATCMessage.DateApproved >= GeneralClass.DPR_ChangeDate)
            {
                authorityOrDepartment = "authority";
                organization = "Nigerian Midstream and Downstream Petroleum Regulatory Authority";
                organizationAcronym = "NMDPRA";

            }

            body += "<p><u>RE: APPLICATION FOR APPROVAL TO CONSTRUCT PETROLEUM PRODUCTS STORAGE DEPOT</u></p>";
            body += "<p style='text-align: justify; line-height: 1.4em;'>We refer to your application of " + msg.DateApplied + " for <b>Approval To Construct</b> a bulk storage depot facility at " + msg.FacilityAddress + " and the subsequent site inspection visit carried out on " + msg.ScheduleDate + ".</p>";
            body += "<p style='text-align: justify; line-height: 1.4em;'>In view of the satisfactory report from the visit and the consent obtained from other appropriate Government Agencies, I am pleased to inform you that you are hereby granted &nbsp;<b> Approval to Construct</b> a Bulk Storage Depot Facility with the following capacities:</p>";
            body += "<table style='width:100%;border-spacing:0;margin-top:25px;'>";
            body += "<tr><th>S/N</th><th>PRODUCT</th><th>DIAMETER(meters)</th><th>HEIGHT(meters)</th><th>CAPACITY(litres)</th></tr>";
            body += msg.TanksText;
            body += "</tbody></table><div><br/>";
            body += $"<p style='text-align: justify;'>The construction of the facility shall be carried out under the following conditions; </p>";
            body += $"<ol><li style='page -break-inside:avoid'> The construction shall be in line with the approved drawings submitted to the {authorityOrDepartment} and in accordance with relevant provisions of Petroleum Regulations.</li> ";
            body += "<li style='page -break-inside:avoid'> All impact mitigation/monitoring in the Environmental Evaluation Report (E.E.R) as indicated in the Environmental Management Plan are to be comprehensively implemented during the construction and operation phases.</li> ";
            body += $"<li style='page -break-inside:avoid'> You shall officially inform the {authorityOrDepartment} of each milestone, particularly pipeline laying, tank/pipeline hydro-tests and calibration exercises.</li> ";
            #region  ATK, PMS, Bitumen
            if (msg.TanksText.ToLower().Contains("pms"))
            {
                body += "<li style='page -break-inside:avoid'> The PMS storage tank MUST be fitted with internal floating blanket.</li> ";
            }
            if (msg.TanksText.ToLower().Contains("atk")) { 
                body += "<li style='page -break-inside:avoid'> All ATK tanks MUST be filled with an internal floating suction system.</li> ";
                body += "<li style='page -break-inside:avoid'> The ATK Storage tanks and piping network MUST be expoxy coated internally.</li> ";
                body += "<li style='page -break-inside:avoid'> All ATK receipt and discharge points MUST have comprehensive filtration systems.</li> ";
            }     
            if (msg.TanksText.ToLower().Contains("bitumen")){

                body += "<li style='page -break-inside:avoid'> The Bitumen storage tanks MUST be properly lagged.</li> ";
                body += "<li style='page -break-inside:avoid'> All Bitumen pipelines MUST be insulated with appropriate thermal insulating materials.</li> ";
                body += "<li style='page -break-inside:avoid'> All cargo pipelines MUST be fitted with adequate Pressure and flow monitoring devices.</li> ";
            }
            #endregion

            body += $"<li style='page -break-inside:avoid'> At the completion of the project, a detailed operational safety case shall be carried out by a {authorityOrDepartment}-Accredited Consultant.</li> ";
            body += $"<li style='page -break-inside:avoid'> You shall inform the {authorityOrDepartment} on completion of the construction work for the pre-commissioning inspection prior to the issuance of operating license.</li> </ol>";


            body += $"<p style='text-align: justify;'>   In addition to the above, {organizationAcronym} representatives wiil be nominated to monitor the construction work and to ensure that all standards are maintained. </p> ";
            body += "<p style='text-align: justify;'>You are therefore requested to strictly adhere to the above as the issuance of an operating license for the facility will be dependent on total compliance. </p> ";
            body += "</div>";



            return body;
        }

        public string RenderViewToString(string viewPath)
        {
            var file = new FileInfo(viewPath);
            using (StreamReader streamReader = file.OpenText())
            {
                string viewLine = "";
                var result = string.Empty;
                while ((viewLine = streamReader.ReadLine()) != null)
                {
                    result = result + viewLine;
                }
                return result;
            }
        }

        //public byte[] GetPDF(string param1, string param2)
        //{

        //    byte[] bPDF = null;
        //    try
        //    {
        //        HTML = HTML.Replace("{{parameter form the html file}}", param1);
        //        HTML = HTML.Replace("{{parameter form the html file}}", param2);

        //        MemoryStream ms = new MemoryStream();
        //        TextReader txtReader = new StringReader(HTML.ToString());


        //        var image = @"C:\Templates\avis.jpg";
        //        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(image);
        //        jpg.SpacingBefore = 10f;
        //        jpg.ScaleToFit(50, 50);
        //        jpg.Alignment = iTextSharp.text.Image.ALIGN_RIGHT;



        //        doc.Open();
        //        doc.Add(jpg);
        //        using (var cssMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csstemp)))
        //        {
        //            using (var htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(pHTML)))
        //            {
        //                XMLWorkerHelper.GetInstance().ParseXHtml(oPdfWriter, doc, htmlMemoryStream, cssMemoryStream);
        //            }
        //        }

        //        doc.Close();

        //        bPDF = ms.ToArray();

        //        return bPDF;
        //    }
        //    catch (Exception ex)
        //    {
        //        var mess = ex.StackTrace;
        //        var mess2 = ex.Message;
        //    }

        //    return bPDF;


        //}
        #endregion
        //public ApplicationAPIModel updateApplication(string status, string reference)
        //{
        //    ApplicationAPIModel appAPI;

        //    using (WebClient client = new WebClient())
        //    {
        //        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        //        appAPI = new ApplicationAPIModel();
        //        appAPI.Status = status;
        //        appAPI.OrderId = reference;

        //        var param = JsonConvert.SerializeObject(appAPI);
        //        var url = _elpsBaseUrl;
        //        url += "Application/" + _elpsAppEmail + "/" + _elpsAppKey;

        //        var output = client.UploadString(url, "PUT", param);

        //        appAPI = JsonConvert.DeserializeObject<ApplicationAPIModel>(output);

        //    }
        //    return appAPI;

        //}


        #endregion



        #region Facility Identification Code Generation
        [AllowAnonymous]
        public string GetFacilityIdentificationCode(int facilityID)
        {
            var facilityCode = ""; string defaultYOP = "18";
            var facility = _context.Facilities.Where(a => a.Id == facilityID).FirstOrDefault();
            
            //get facility address, LGA and state
            var facDetails = (from a in _context.applications 
                            join c in _context.companies on a.company_id equals c.id
                            join f in _context.Facilities on a.FacilityId equals f.Id
                            join ad in _context.addresses on f.AddressId equals ad.id
                            join st in _context.States_UT on ad.StateId equals st.State_id
                            where f.Id == facilityID && a.DeleteStatus!= true
                            select new
                            {
                                CompanyName= c.name,
                                CompanyEmail =c.CompanyEmail,
                                CompanyElpsID=c.elps_id,
                                StateName= st.StateName,
                                StateCode= st.Code,
                                LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault() : new Lgas(),
                                LGA_ID= ad.LgaId,
                            }).FirstOrDefault();
           

            if (facDetails != null)
            {
                facilityCode = facDetails.StateCode.Trim(); //State code

                //get facility category from ROMS
                var facilityCategory = getFacilityCategoryFromROMS(Convert.ToInt32(facDetails.CompanyElpsID), facDetails.CompanyEmail);
                if (facilityCategory == "0" || facilityCategory.ToLower().Contains("error"))
                    return "Error: An error occured while getting the facility category from ROMS portal.";
                else
                    facilityCode = facilityCode + "/" + facilityCategory; //Category code
                   facility.CategoryCode = facilityCategory;

                //get LGA code
                if (facDetails.LGA != null)
                    facilityCode =facilityCode+ "/"+ facDetails.LGA.LGA_Code?.Trim() ; //LGA code
                else
                    return "Error: LGA details could not be found for this facility";


                //get number of depots in the facility state

                var getDepotsCount = (from f in _context.Facilities
                                       where f.IdentificationCode!= null && f.IdentificationCode.StartsWith(facDetails.StateCode)
                                       && f.DeletedStatus != true 
                                       select new
                                       {
                                           f,
                                       }).ToList().GroupBy(x=> x.f.IdentificationCode).Select(y => y.FirstOrDefault()).ToList();


                facilityCode = facilityCode + "/" + (getDepotsCount.Count() + 1).ToString("D3"); //Depots Count


                //get MatlabCode
                var matlab = MatlabCode(facDetails.CompanyName[0], 3);

                if (matlab != null)
                {
                    facilityCode = facilityCode + "/" + matlab.ToUpper(); //matlab code
                    var facilityCodes = (from fc in _context.Facilities
                                         join c in _context.companies on fc.CompanyId equals c.id
                                         where fc.DeletedStatus != true && fc.IdentificationCode != null
                                         && fc.IdentificationCode.Contains(matlab.ToUpper())
                                         select fc).FirstOrDefault();

                    while (facilityCodes != null)
                    {
                        matlab = MatlabCode(facDetails.CompanyName[0], 3);
                        facilityCodes = (from fc in _context.Facilities
                                         join c in _context.companies on fc.CompanyId equals c.id
                                         where fc.DeletedStatus != true && fc.IdentificationCode != null
                                         && fc.IdentificationCode.Contains(matlab.ToUpper())
                                         select fc).FirstOrDefault();
                    }

                }
                else
                    return "Error: An error occured while trying to generate the matlab random code for this facility";

                //year of operation
                facilityCode = facilityCode + "/" + defaultYOP; // first operation year
                facility.IdentificationCode = facilityCode;
                _context.SaveChanges();
            }
            return facilityCode;

        }

        [AllowAnonymous]
        public string getFacilityCategoryFromROMS(int companyELPSID, string companyEmail)
        {
            var ROMS_URL = _configuration.GetSection("ROMS").GetSection("URL").Value.ToString();
            var ROMS_HashKey = _configuration.GetSection("ROMS").GetSection("HashKey").Value.ToString();
            var ROMS_EmailSetting = _configuration.GetSection("ROMS").GetSection("EmailSetting").Value.ToString();
            var ROMS_KeyConnect = _configuration.GetSection("ROMS").GetSection("CodeConnect").Value.ToString();
            List<ROMSFacilityModel> allStationStates = new List<ROMSFacilityModel>();

            var url = ROMS_URL+ "GetFacilityList?companyEmail="+companyEmail+"&email="+ ROMS_EmailSetting + "&code="+ ROMS_KeyConnect;
            var res = _restService.Response(url, null, "GET", null);
            if (res != null && res.StatusCode != System.Net.HttpStatusCode.InternalServerError)
            {
                if (!res.Content.Contains("Company not found"))
                {

                    allStationStates = (List<ROMSFacilityModel>)Newtonsoft.Json.JsonConvert.DeserializeObject(res.Content.ToString(), typeof(List<ROMSFacilityModel>));
                }
            }
            else
                return "error";

            if(allStationStates!= null && allStationStates.Count > 0)
            {
                var stateZone = (from st in allStationStates
                                 join sts in _context.States_UT on st.Address.State.ToLower() equals sts.StateName.ToLower()
                                 join gp in _context.GeoPoliticalStates on sts.State_id equals gp.StateId
                                 join gz in _context.GeoPoliticalZone on gp.GeoId equals gz.GeoId
                                 where gp.DeletedStatus != true && gz.DeletedStatus != true
                                 select gz).GroupBy(x=> x.GeoId).Select(y=> y.FirstOrDefault()).ToList();
                int zonesCount = stateZone.Count();

                if (zonesCount > 0 && zonesCount < 3)
                    return "C";
                if (zonesCount > 3 && zonesCount < 6)
                    return "B";
                if (zonesCount > 6)
                    return "A";
                else
                    return "Error: Facilities state zones can not be found.";
            }
            else
            {
                return "D";
            }

            return "ok";

        }

        private static Random random = new Random();

        public static string MatlabCode(char companyFLetter,int length = 3)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var randomCode = Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray();
            string newRC = new string(randomCode);

             return companyFLetter.ToString() + newRC;
        }


        //res.Content = "[{ 'LicenseNumber':'BR75903/2022','FacilityId':'DPR/FAC/001659'}]"; 
        //""UniqueId":"KD0035","LicenseCode":"KD2MKA0035SZYA17","UniqueIdOnELPS":"DPR/ELPS/F/001661","FacilityName":"SHARON PETROLEUM NIGERIA LIMITED","DriveIns":1,"DriveOuts":1,"TankCount":6,"PumpCount":11,"Status":"Independent","FacilityType":"Station","Company":{ "Name":"SHARON PETROLEUM NIGERIA LIMITED","ContactName":"EHIZOGIE solomon","email":"sharonpetroleumltd@gmail.com","Phone":"08033141231","Address":{ "StreetAddress":"KM 1, ABUJA JUNCTION, GONI GORA ","City":"KADUNA","State":"Kaduna","LGA":null} },"Address":{ "StreetAddress":"PLOT 2, KACHIA ROAD,","City":"","State":"Kaduna","LGA":"Kaduna South"},"Tanks":null,"Pumps":null}];
        //var url = ROMS_URL+ "Facility/AllStationsForDepots?companyELPSID="+companyELPSID
        //+"&companyEmail="+companyEmail+"&depothashKey="+ROMS_HashKey;

        #endregion

        public JsonResult GetLicense(int id)
        {

            try
            {
                var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
                var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
                var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
                int userID = generalClass.DecryptIDs(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserID));

                var application = _context.applications.Where(a => a.id == id).FirstOrDefault();

                if (application != null)
                {

                    var permit = _context.permits.Where(a => a.application_id == application.id).FirstOrDefault();
                    var facility = _context.Facilities.Where(x => x.Id == application.FacilityId).FirstOrDefault();
                    var address = _context.addresses.Where(a => a.id == facility.AddressId).FirstOrDefault();
                    var state = _context.States_UT.Where(x => x.State_id == address.StateId).FirstOrDefault();
                    var comp = _context.companies.Where(c => c.id == permit.company_id).FirstOrDefault();
                    var compAdd = _context.addresses.Where(a => a.id == comp.registered_address_id).FirstOrDefault();
                    var compState = _context.States_UT.Where(x => x.State_id == 0 ).FirstOrDefault();

                    if (compAdd != null)
                    {
                        compState = _context.States_UT.Where(x => x.State_id == compAdd.StateId).FirstOrDefault();
                    }
                    
                    var category = _context.Categories.Where(ct => ct.id == application.category_id).FirstOrDefault();
                    var Phase = _context.Phases.Where(p => p.id == application.PhaseId).FirstOrDefault();
                    var sch = _context.MeetingSchedules.Where(a => a.Accepted == true && a.Approved == true && a.ApplicationId == application.id).OrderByDescending(a => a.Date).FirstOrDefault();

                    var vAppProc = _context.MyDesk.Where(a => a.AppId == application.id).OrderByDescending(a => a.Sort).FirstOrDefault();
                    int approver = vAppProc.StaffID;
                    var me = _context.Staff.Where(a => a.StaffID == approver).FirstOrDefault();
                    var atnks = _context.ApplicationTanks.Where(a => a.FacilityId == application.FacilityId && a.ApplicationId == application.id).ToList();
                    var tanks = _context.Tanks.Where(a => a.FacilityId == application.FacilityId && !a.Decommissioned).ToList();


                    FieldOffices brch = new FieldOffices();
                    bool isZ;
                    //var brch = elpsCaller.GetBranch(myUB.BranchId);

                    brch = _context.FieldOffices.Where(x => x.FieldOffice_id == me.FieldOfficeID).FirstOrDefault();

                    string position = "";
                    if (Phase.ShortName.ToUpper() == "SI")
                    {
                        position = "AD";
                    }
                    else
                    {
                        position = "Director";
                    }


                    //Tank count figure and word
                    string tnkCountInWords = "";
                    int tankChangeCount = 0;
                    string DMType = "";
                    var signature = _context.Signatories.Where(a => a.Position == position && a.StartDate >= permit.date_issued && (a.EndDate == null || a.EndDate <= permit.date_issued)).FirstOrDefault();

                    var newSignatory = (from st in _context.Staff
                                        join r in _context.UserRoles on st.RoleID equals r.Role_id
                                        where st.DeleteStatus != true && st.ActiveStatus != false
                                        && ((Phase.IssueType.ToLower() == "approval" && r.RoleName == GeneralClass.ED) || (Phase.IssueType.ToLower() != "approval" && r.RoleName == GeneralClass.AUTHORITY))
                                        && r.DeleteStatus != true
                                        select st).FirstOrDefault();


                    string signatur = "";
                    string nam = "";
                    string signatur_n = "";
                    string nam_n = "";
                    if (signature != null)
                    {
                        signatur = string.IsNullOrEmpty(signature.Signature) ? "" : signature.Signature;
                        nam = string.IsNullOrEmpty(signature.Name) ? "" : signature.Name;
                    }

                    //New Signature
                    if (newSignatory != null)
                    {
                        signatur_n = string.IsNullOrEmpty(newSignatory.SignaturePath) ? "" : newSignatory.SignaturePath;
                        nam_n = string.IsNullOrEmpty(newSignatory.SignatureName) ? "" : newSignatory.SignatureName;
                    }

                    var up = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var signPath = Path.Combine(up, "OldDepotStyle/Content/Signatures/DG.png");

                    var phcode = new[] { "SI" };
                    var getIssueDateView = permit.date_issued < GeneralClass.DPR_ChangeDate ? "Old View" : "New View";

                    var model = new SuitabilityLetterModel
                    {

                        PrintView = getIssueDateView,
                        RefNo = permit.permit_no,
                        CompanyName = comp.name,
                        Address = compAdd.address_1.ToUpper(),
                        City = compAdd.city,
                        State = compState != null ? compState.StateName : "",
                        Date = GetDatePad(permit.date_issued.ToString("dd")) + permit.date_issued.ToString(" MMMM ") + permit.date_issued.ToString("yyy "),// CalendarHelper.ShortDate(permit.date_issued); //.ToLongDateString();
                        DateApproved = permit.date_issued,                                                                                                                     // CalendarHelper.ShortDate(permit.date_issued); //.ToLongDateString();
                                                                                                                                                                               //FacilityAddress = address.FacilityAddress(),
                        FacilityAddress = address.address_1,
                        DateApplied = application.CreatedAt != null ? application.CreatedAt.Value.ToLongDateString() : application.date_added.ToLongDateString(),
                        Signature = signPath.Trim(),
                        SignedBy = me.ToString(),
                        Signature_N = signatur_n.Trim(),
                        SignedBy_N = newSignatory.FirstName + " " + newSignatory.LastName.ToString(),
                        Office = brch.OfficeName.ToUpper(),
                        FacStateName = state.StateName,
                        IsZopscon = false,
                        PhaseShortName = Phase.ShortName.ToUpper()
                    };
                    var body = "";

                    LogMessages(model == null ? "Returning empty model" : "USERRR: (" + Phase.ShortName + ") " + model.RefNo);
                    string phaseShortName = "";
                    
                    #region check application/facility tanks to determine template to be used
                    string ATK = ""; string Bit = ""; string PMS = "";

                    if (atnks.Count() > 0)
                    {
                        foreach (var at in atnks)
                        {
                            var ProductName = _context.Products.Where(p => p.Id == at.ProductId).FirstOrDefault()?.Name;
                            if (ProductName != null)
                            {

                                if (ProductName.ToLower().Contains("bitumen"))
                                {
                                    Bit = "Yes";
                                }
                                if (ProductName.ToLower().Contains("atk"))
                                {
                                    ATK = "Yes";
                                }
                                if (ProductName.ToLower().Contains("pms"))
                                {
                                    PMS = "Yes";
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var at in tanks)
                        {
                            var ProductName = _context.Products.Where(p => p.Id == at.ProductId).FirstOrDefault()?.Name;
                            if (ProductName != null)
                            {
                                if (ProductName.ToLower().Contains("bitumen"))
                                {
                                    Bit = "Yes";
                                }
                                if (ProductName.ToLower().Contains("atk"))
                                {
                                    ATK = "Yes";
                                }
                                if (ProductName.ToLower().Contains("pms"))
                                {
                                    PMS = "Yes";
                                }
                            }
                        }
                    }


                    #endregion
                    switch (Phase.ShortName.ToUpper())
                    {
                        case "SI":
                            {
                                phaseShortName = "SI";
                                break;
                            }
                        case "REG":
                            {
                                phaseShortName = "REG";
                                break;
                            }
                        case "ATC":
                        case "CWA":
                            {

                                //Now check if products contains ATK, Bitumen or Both

                                phaseShortName = "ATC";

                                break;
                            }

                        case "NDT":
                        case "RC":
                            {
                                phaseShortName = "NDT";
                                break;
                            }
                        case "DM":
                            {
                                phaseShortName = "DM";

                                break;
                            }

                        case "UWA":
                            {
                                phaseShortName = "DM";
                                break;
                            }
                        case "SAP":
                            {
                                phaseShortName = "SAP";
                                break;
                            }

                    }

                    var file = Path.Combine(up, "Templates/ApprovalTemplate/" + phaseShortName + ".txt");
                    using (var sr = new StreamReader(file.Trim()))
                    {

                        body = sr.ReadToEnd();
                    }

                    string stn = "";
                    var appOffice = GetApplicationOffice(application.id).FirstOrDefault();
                    //string zonalOrFieldID = appOffice.ZonalOrField.ToString()=="FD"? "Field| "+ appOffice.OfficeName : "Zonal| "+ appOffice.OfficeName;
                    string zonalOrFieldID = appOffice.OfficeName;
                    if (zonalOrFieldID == null)
                    {
                        return Json("zone/field office details not found");

                    }

                    else
                    {
                        stn = appOffice.OfficeName;
                        model.FacilityZonalOrFeildOffice = zonalOrFieldID;
                    }
                    switch (Phase.ShortName.ToUpper())
                    {
                        case "SI":
                            {
                                // only one data: DateApplied
                                var suit = _context.SuitabilityInspections.Where(a => a.ApplicationId == application.id).FirstOrDefault();
                                if (suit != null)
                                {
                                    model.SizeOfLand = suit.SizeOfLand;
                                    model.ZonalOrFeild = stn;
                                }

                                model.Body = string.Format(body, model.DateApplied, stn);
                                break;
                            }
                        case "REG":
                            {
                                // only one data: DateApplied
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                if (atnks.Count > 0)
                                {
                                    foreach (var item in atnks)
                                    {
                                        var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity.ToString("N0")}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }
                                else
                                {
                                    foreach (var item in tanks)
                                    {

                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }

                                tnks = sb.ToString();

                                model.Body = string.Format(body, model.FacilityAddress, model.FacilityZonalOrFeildOffice, model.DateApplied, sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString(), tnks);
                                break;
                            }

                        case "ATC":
                        case "CWA":
                            {
                                // only one data: DateApplied
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                if (atnks.Count > 0)
                                {
                                    foreach (var item in atnks)
                                    {
                                        var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity.ToString("N0")}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }
                                else
                                {
                                    foreach (var item in tanks)
                                    {

                                        var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td></tr>";
                                        sb.Append(td);
                                        c++;
                                    }
                                }

                                tnks = sb.ToString();

                                var letterdate = sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString();
                                var ATCLet = new DM_ATCLetter();
                                ATCLet.FacilityAddress = model.FacilityAddress;
                                ATCLet.DateApplied = model.DateApplied;
                                ATCLet.ScheduleDate = letterdate;
                                ATCLet.DateApproved = _context.permits.Where(x => x.application_id == application.id).FirstOrDefault().date_issued;
                                ATCLet.StateName = state.StateName == "Delta State" ? "Warri" : state.StateName;
                                ATCLet.TanksText = tnks;

                                var getbody = ATCLetter(ATCLet);

                                model.Body = string.Format(getbody);
                                break;
                            }
                        case "NDT":
                        case "RC":
                            {
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                //Calibration and re-calibration
                                if (Phase.ShortName.ToUpper() == "NDT")
                                {


                                    if (atnks.Count > 0)
                                    {
                                        foreach (var item in atnks)
                                        {
                                            var it = _context.Tanks.Where(a => a.Id == item.TankId).FirstOrDefault();
                                            var product = _context.Products.Where(p => p.Id == it.ProductId).FirstOrDefault();

                                            var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.TankName}</td><td>{product.Name}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{item.Capacity}</td><td>NA</td></tr>";
                                            sb.Append(td);
                                            c++;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item in tanks)
                                        {
                                            var td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{item.Name}</td><td>{item.ProductName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>NA</td></tr>";
                                            sb.Append(td);
                                            c++;
                                        }
                                    }

                                    tnks = sb.ToString();
                                }
                                else
                                {
                                    var tnksm = _context.FacilityModifications.Where(a => a.ApplicationId == application.id).ToList();
                                    foreach (var item in tnksm)
                                    {
                                        var td = "";
                                        var aptnk = _context.ApplicationTanks.Where(a => a.FacilityId == item.FacilityId && a.ApplicationId == item.FacilityId).ToList();
                                        aptnk.ForEach(atnk =>
                                        {
                                            var it = _context.Tanks.Where(a => a.Id == atnk.TankId).FirstOrDefault();
                                            var product = _context.Products.Where(p => p.Id == atnk.ProductId).FirstOrDefault();
                                            td = $"<tr style=\"text-align:center\"><td>{c}</td><td>{it.Name}</td><td>{product.Name}</td><td>{it.Diameter}</td><td>{it.Height}</td><td>{it.MaxCapacity}</td><td>{it.ModifyType}</td></tr>";

                                        });
                                        sb.Append(td);
                                        c++;
                                    }
                                    tnks = sb.ToString();
                                }
                                model.Body = string.Format(body, model.FacilityAddress, model.DateApplied, sch == null ? model.DateApplied : sch.ApprovedDate.GetValueOrDefault().ToLongDateString(), model.CompanyName, tnks, stn);//, state.StateName

                                break;

                            }
                        case "DM":
                        case "UWA":
                            {
                                string tnks = "";
                                var sb = new StringBuilder();
                                var c = 1;
                                int cnt = 0;
                                string td = "";
                                string prevProd = "";
                                string activ = "Active";
                                var facMod = _context.FacilityModifications.Where(a => a.ApplicationId == application.id).FirstOrDefault();

                                if (facMod != null)
                                {

                                    prevProd = facMod.PrevProduct;
                                    if (facMod.Type == "Conversion")
                                    {
                                        phaseShortName = "DMC";

                                    }
                                    if (facMod.Type.Contains("Inclusion"))
                                    {
                                        phaseShortName = "DMI";
                                    }
                                    DMType = facMod.Type;
                                    foreach (var item in tanks.OrderBy(a => a.Name))
                                    {
                                        activ = "Active";
                                        var productName = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault().Name;

                                        var tm = atnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                        if (tm != null)
                                        {
                                            tankChangeCount += 1;
                                            if (facMod.Type == "ATC" || facMod.Type.Contains("Inclusion"))
                                            {
                                                activ = "";

                                            }
                                            if (facMod.Type.Contains("Decommission"))
                                            {
                                                activ = "Inactive";
                                            }
                                            if (facMod.Type == "Conversion")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.MaxCapacity}</td><td>{activ}(Converted)</td></tr>";

                                            }
                                            else if (facMod.Type == "Inclusion")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.MaxCapacity}</td><td>{activ}({facMod.Type})</td></tr>";

                                            }
                                            else
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}({facMod.Type})</td></tr>";
                                            }
                                        }
                                        else
                                        {
                                            if (phaseShortName == "DMC")
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>";


                                            }
                                            else
                                            {
                                                td = phaseShortName == "DM" ? $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>"
                                                                     : $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{productName}</td><td>{int.Parse(item.MaxCapacity).ToString("#,###,###")}</td><td>{activ}</td></tr>";

                                            }
                                        }

                                        sb.Append(td);
                                        c++;
                                    }


                                }
                                else
                                {
                                    var modificationTnks = _context.FacilityTankModifications.Where(a => a.ApplicationId == application.id).ToList();
                                    if (modificationTnks.Count > 0)
                                    {
                                        var _tm = modificationTnks.Where(a => a.Type == "Convert" || a.Type.Contains("Inclusion")).FirstOrDefault();
                                        if (_tm != null)
                                        {
                                            phaseShortName = "DMC";
                                        }
                                        foreach (var item in tanks.OrderBy(a => a.Name))
                                        {
                                            var product = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();

                                            var tm = modificationTnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                            if (tm != null)
                                            {
                                                if (facMod.Type == "ATC" || facMod.Type.Contains("Inclusion"))
                                                {
                                                    activ = "";
                                                }
                                                if (tm.Type.Contains("Decommission"))
                                                {
                                                    activ = "Inactive";
                                                }
                                                else if (tm.Type.Contains("Conver"))
                                                {
                                                    tm.Type = "Converted";
                                                    var tks = _context.Tanks.Where(a => a.Id == tm.TankId).FirstOrDefault();

                                                    prevProd += string.IsNullOrEmpty(prevProd) ? tm.PrevProduct + " to " + product.Name : " and " + tm.PrevProduct + " to " + product.Name;

                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.MaxCapacity}</td><td>{activ}({tm.Type})</td></tr>";

                                                }
                                                else
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}({tm.ModifyType})</td></tr>";

                                                }
                                            }
                                            else
                                            {
                                                if (phaseShortName == "DMC")
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>


                                                }
                                                else
                                                {
                                                    td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>


                                                }
                                            }
                                            sb.Append(td);
                                            c++;
                                        }

                                    }
                                    else
                                    {
                                        foreach (var item in tanks)
                                        {
                                            var product = _context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();
                                            var tm = atnks.Where(a => a.TankId == item.Id).FirstOrDefault();
                                            if (tm != null)
                                            {

                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>

                                            }
                                            else
                                            {
                                                td = $"<tr style=\"text-align:center;border: 1px solid black;padding: 15px;height: 40px;\"><td>{c}</td><td>{item.Name}</td><td>{product.Name}</td><td>{item.Diameter}</td><td>{item.Height}</td><td>{item.MaxCapacity}</td><td>{activ}</td></tr>";//<td>{item.ModifyType}</td>

                                            }
                                            sb.Append(td);
                                            c++;
                                        }
                                    }
                                }
                                tnks = sb.ToString();
                                double fontSize = 1.2;
                                if (atnks.Count <= 3)
                                {
                                    fontSize = 1.4;
                                }

                                var pth = Path.Combine(up, "Templates/ApprovalTemplate");

                                using (var sr = new StreamReader(pth + "/" + phaseShortName + ".txt"))
                                {

                                    body = sr.ReadToEnd();
                                }
                                tnkCountInWords = GeneralClass.NumWords(Convert.ToDouble(tankChangeCount));

                                if (phaseShortName == "DMC")
                                {

                                    model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, stn, model.CompanyName, prevProd, tnks, application.date_added.Year + 1);//, state.StateName

                                }
                                else if (phaseShortName == "DMI")
                                {
                                    model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, model.CompanyName, tnks, fontSize, phaseShortName.Equals("DMI") ? $",{address?.city}, {state?.StateName}".ToUpper() : "", tankChangeCount, tnkCountInWords, state.StateName, application.date_added.Year + 1);

                                }
                                else //DM Letter
                                {
                                    var letterdate = sch == null ? model.DateApplied : sch.MeetingDate.ToLongDateString();
                                    var DMLet = new DM_ATCLetter();
                                    DMLet.CompanyName = model.CompanyName;
                                    DMLet.FacilityAddress = model.FacilityAddress;
                                    DMLet.DateApplied = model.DateApplied;
                                    DMLet.ScheduleDate = letterdate;
                                    DMLet.DateApproved = _context.permits.Where(x => x.application_id == application.id).FirstOrDefault().date_issued;
                                    DMLet.StateName = state.StateName == "Delta State" ? "Warri" : state.StateName;
                                    DMLet.TanksText = tnks;
                                    DMLet.ModifyType = DMType;
                                    var getbody = DMLetter(DMLet, tankChangeCount.ToString(), tnkCountInWords);
                                    model.Body = string.Format(getbody);
                                    //model.Body = string.Format(body, model.FacilityAddress.ToUpper(), model.DateApplied, "Monday, January 25, 2021", model.CompanyName, tnks, fontSize, phaseShortName.Equals("DM") ? $",{address?.city}, {state?.StateName}".ToUpper() : "", tanks.Count, tnkCountInWords);//, state.StateName
                                }
                                break;
                                //break;
                            }
                        case "SAP":
                            {
                                model.Body = string.Format(body, model.FacilityAddress);
                                break;
                            }
                        default:
                            break;
                    }

                    LogMessages(phaseShortName + " approval generated for application with ref:" + application.reference, userEmail);
                    return Json(model);
                }
                return Json("application not found");
            }
            catch (Exception ex)
            {

                LogMessages($"Error while loading approval view: {ex.Message.ToString()}");
                return Json(ex?.Message + " " + ex?.InnerException);
            }
        }

    }
}