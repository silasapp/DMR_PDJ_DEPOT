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
using System.Data.Sql;
using System.Transactions;
using Rotativa.AspNetCore;
using System.Text;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
//using NewDepot.Controllers;
using Microsoft.AspNetCore.Http;
using LpgLicense.Models;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Controllers.Authentications;
using NewDepot.Controllers;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NewDepot.Controllers
{
    
    public class MigrationsController : Controller
    {
        RestSharpServices _restService = new RestSharpServices();
        public IConfiguration _configuration;
        IHttpContextAccessor _httpContextAccessor;
        private readonly Depot_DBContext _context;
        SubmittedDocuments _appDocRep;
        ElpsResponse elpsResponse = new ElpsResponse();
        ApplicationHelper appHelper;
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        HelpersController _helpersController;

        public MigrationsController(
            Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            appHelper = new ApplicationHelper(_context, _appDocRep, _httpContextAccessor, _configuration);
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }

        //Company 
        public IActionResult ApplicationErrorMigration()
        {

            int i = 0;
            var getAllApps = (from ap in _context.applications
                              join his in _context.application_desk_histories on ap.id equals his.application_id
                              where ap.status == GeneralClass.Processing || ap.status == GeneralClass.PaymentCompleted
                              select ap).ToList();
            getAllApps.ForEach(x =>
            {
                //check
                var allhist = (from u in _context.application_desk_histories where u.application_id == x.id select u).OrderByDescending(x=>x.id).ToList();
                var checkhist = (from u in _context.application_desk_histories where u.application_id == x.id && u.status.ToLower() == "final approval" select u).FirstOrDefault();
                if(checkhist != null)
                {
                    x.status = GeneralClass.Approved;
                    x.current_desk = null;

                    if(allhist.FirstOrDefault().comment.ToLower() == "application landed on staff desk")
                    {
                        _context.application_desk_histories.Remove(allhist.FirstOrDefault());
                    }
                    //check my desk
                    var checkdesk = (from u in _context.MyDesk where u.AppId == x.id && u.HasWork != true select u).FirstOrDefault();
                    if(checkdesk != null)
                    {
                        _context.MyDesk.Remove(checkdesk);

                    }

                }


                i = _context.SaveChanges();
            });

            return Json(new { success = true, Message = i + " number of records have been updated successfully" });

        }
        public IActionResult CompanyMigration()
        {

            int i = 0;
            var getAllCompanies = (from c in _context.companies
                                   where c.CompanyEmail == null
                                   select c).ToList();
            getAllCompanies.ForEach(x =>
            {
                x.CompanyEmail = x.user_id;
                x.ActiveStatus = true;
                x.isFirstTime = false;
                x.CreatedAt = DateTime.Now;
                x.DeleteStatus = false;
                x.LocationId = 0;
                x.RoleId = 15; //Company Role ID

                var companyAddress = (from ad in _context.addresses where ad.id == x.operational_address_id select ad).FirstOrDefault();
                if (companyAddress != null)
                {
                    x.Address = companyAddress?.address_1.Length > 100 ? companyAddress?.address_1.Substring(0, 99): companyAddress?.address_1;
                    x.City = companyAddress?.city;
                   
                    //var stateName = _context.States_UT.Where(s => s.State_id == companyAddress.StateId).FirstOrDefault() != null ? _context.States_UT.Where(s => s.State_id == companyAddress.StateId).FirstOrDefault().StateName.ToUpper() : null;
                    //x.StateName = stateName?.Length > 15 ? stateName?.Substring(0, 14) : stateName;

                 }
                i = _context.SaveChanges();
            });

            return Json(new { success = true, Message = i + " number of records have been updated successfully" });

        }
        // Document Submission Migration 
        public IActionResult DocumentMigrationFor(int id)
        {

            int i = 0; string Source = ""; string Name = ""; string DocElpsId = "";

            var subDoc = _context.application_documents.ToList();
            var getAllApp = (from doc in subDoc.AsEnumerable()
                             join ap in _context.applications.AsEnumerable() on doc.application_id equals ap.id
                             where doc.application_id == id
                             select new
                             {
                                 doc,
                                 ap

                             }
                       ).ToList();

            getAllApp.ForEach(x =>
            {
                var fac = _context.Facilities.Where(a => a.Id == x.ap.FacilityId).FirstOrDefault();

                if (fac != null)
                {

                    LpgLicense.Models.Document DI = _helpersController.GetDocumentDetail(x.doc.document_id.ToString());
                    if (DI != null)
                    {
                        Source = DI.source;
                        Name = DI?.documentTypeName;
                        DocElpsId = x.doc.document_id.ToString();
                        

                    var doc = (from dc in _context.ApplicationDocuments where dc.ElpsDocTypeID == x.doc.document_type_id || dc.DocName.ToUpper() == Name.ToLower() select dc).FirstOrDefault();

                    if (doc != null)
                    {
                        var subDoc = (from sb in _context.SubmittedDocuments where sb.AppID == x.ap.id && sb.AppDocID == doc.AppDocID select sb).FirstOrDefault();

                        if (subDoc == null)
                        {

                            var sbdoc = new SubmittedDocuments()
                            {
                                AppID = x.ap.id,
                                AppDocID = doc.AppDocID,
                                DocSource = Source.Replace("democontent", "content"),
                                DocumentName = Name.ToUpper(),
                                DeletedStatus = false,
                                CompElpsDocID = Convert.ToInt32(DocElpsId),
                                CreatedAt = DateTime.Now
                            };
                            _context.SubmittedDocuments.Add(sbdoc);
                            i = _context.SaveChanges();
                        }
                            else
                            {
                                subDoc.AppID = x.ap.id;
                                subDoc.AppDocID = doc.AppDocID;
                                subDoc.DocSource = Source.Replace("democontent", "content");
                                subDoc.DocumentName = Name.ToUpper();
                                subDoc.DeletedStatus = false;
                                subDoc.CompElpsDocID = Convert.ToInt32(DocElpsId);
                            }
                        }
                    else
                    {

                        ApplicationDocuments con = new ApplicationDocuments()
                        {
                            DocName = Name.ToUpper(),
                            docType = "Facility",
                            PhaseId = 0,
                            ElpsDocTypeID = x.doc.document_type_id,
                            CreatedAt = DateTime.Now,
                            DeleteStatus = false
                        };

                        _context.ApplicationDocuments.Add(con);
                        int Created = _context.SaveChanges();

                        if (Created > 0)
                        {
                            var subDoc = (from sb in _context.SubmittedDocuments where sb.AppID == x.ap.id && sb.AppDocID == con.AppDocID select sb).FirstOrDefault();

                            if (subDoc == null)
                            {
                                var sbdoc = new SubmittedDocuments()
                                {
                                    AppID = x.ap.id,
                                    AppDocID = con.AppDocID,
                                    DocSource = Source.Replace("democontent", "content"),
                                    DocumentName = Name.ToUpper(),
                                    DeletedStatus = false,
                                    CompElpsDocID = Convert.ToInt32(DocElpsId),
                                    CreatedAt = DateTime.Now
                                };
                                _context.SubmittedDocuments.Add(sbdoc);
                                i = _context.SaveChanges();
                                }
                                else
                                {
                                    subDoc.AppID = x.ap.id;
                                    subDoc.AppDocID = con.AppDocID;
                                    subDoc.DocSource = Source.Replace("democontent", "content");
                                    subDoc.DocumentName = Name.ToUpper();
                                    subDoc.DeletedStatus = false;
                                    subDoc.CompElpsDocID = Convert.ToInt32(DocElpsId);
                                }
                                i = _context.SaveChanges();
                            }
                        
                      
                    }

                }
            }
                    
                
            });
            return Json(new { success = true, Message = i + " number of application document records have been migrated successfully" });

        }
        // Application Migration 
        public IActionResult ApplicationMigration()
        {

            int i = 0;
            var getAllApp = (from ap in _context.applications
                             join fac in _context.Facilities on ap.FacilityId equals fac.Id
                             join comp in _context.companies on ap.company_id equals comp.id
                            
                             select ap
                           ).ToList();


            getAllApp.ForEach(x =>
            {
                x.Migrated = true;
                x.CreatedAt = x.date_added;
                x.UpdatedAt = x.date_modified;
                x.DeleteStatus = x.company_id <=0 ?true: false;

                i = _context.SaveChanges();

            });

            return Json(new { success = true, Message = i + " number of application records have been updated successfully" });

        } 
        // Application Desk Migration 
        // Application Migration 
        public IActionResult StaffMigration()
        {

            int i = 0;

            int sort = 0;
            var process = new WorkProccess();
            var getAllStaff = (from ub in _context.UserBranches
                               join sf in _context.Staffs on ub.UserEmail.ToLower() equals sf.Email.ToLower()
                               select new {ub,sf}
                              ).ToList();


            //Migrate Role
            
                var getAllApp = (from ap in _context.applications
                             join proc in _context.application_Processings on ap.id equals proc.ApplicationId
                             where ap.company_id > 0 && proc.processor>0

                             select new { ap, proc }
                          ).OrderByDescending(x=> x.proc.Id).ToList();

            //var getAllStaff_NA = _context.Staff.AsEnumerable().Any(x2 => x2.StaffEmail.ToLower() == x.sf.Email.ToLower())).ToList();

            var getAllApp_NA = getAllApp.Where(x => x.proc.Processed != true && x.proc.Assigned == true).ToList();

            var branche = _restService.Response("/api/Branch/All/{email}/{apiHash}/", null, "GET");
            var branches = JsonConvert.DeserializeObject<List<vBranch>>(branche.Content);
            if (branches != null)
            {
                var getAllStaff_NA = (from u in getAllApp_NA
                                      join x in getAllStaff on u.proc.processor equals x.ub.Id
                                      from st in _context.Staff
                                      where x.sf.Email.ToLower() != st.StaffEmail.ToLower()
                                      select x
                                   ).ToList();

                    getAllStaff.ForEach(x =>
                  {

                    //Staff Role
                    var workRole = (from w in _context.WorkRoles where w.Id == x.ub.RoleId select w).FirstOrDefault();
                      int SRoleID = x.ub.RoleId;
                      if (workRole != null)
                      {
                          var userRole = (from w in _context.UserRoles where w.RoleName.ToLower() == workRole.Name.ToLower() select w).FirstOrDefault();
                          SRoleID = userRole != null ? userRole.Role_id : SRoleID;
                      }
                      var branch = branches.Where(a => a.Id == x.ub.BranchId).FirstOrDefault();


                      var staff = (from stf in _context.Staff where stf.StaffEmail.ToLower() == x.sf.Email.ToLower() select stf).FirstOrDefault();
                      if (staff != null)
                      {
                        //staff.StaffElpsID = x.sf.UserId;
                        staff.CreatedAt = DateTime.Now;
                          staff.ActiveStatus = x.ub.Active == false ? false : true;

                          string location = branch.Name.ToLower().Contains("head office") ? "HQ" : "FO";
                          var loc = (from l in _context.Location where l.LocationName == location select l).FirstOrDefault();
                          staff.LocationID = loc?.LocationID;
                          staff.RoleID = SRoleID;

                          var getStaffBranch = (from u in _context.FieldOffices where u.OfficeName.ToLower() == branch.Name.ToLower() select u).FirstOrDefault();
                          if (getStaffBranch != null)
                          {
                              staff.FieldOfficeID = getStaffBranch.FieldOffice_id;
                          }
                          i = _context.SaveChanges();
                        //Check for applications with current desk as staff
                        //Check for applications with current desk as staff
                        foreach (var app in getAllApp.Where(p => p.proc.processor == x.ub.Id))
                          {
                             var OldPhaseRouting=(from u in _context.PhaseRoutings where
                                                  u.PhaseId==app.ap.PhaseId && u.ProcessingRule_Id==app.proc.ProcessingRule_Id  select u).FirstOrDefault();
                              //First get application phase ID
                              if (OldPhaseRouting != null)//5 is depot modification
                              {

                                  sort = (int)OldPhaseRouting.SortOrder;
                                  process = _context.WorkProccess.Where(x => x.Sort == sort && x.PhaseID == app.ap.PhaseId && x.DeleteStatus != true).FirstOrDefault();

                                  sort = process == null && sort > 7 ? 1 : sort;
                                  process = _context.WorkProccess.Where(x => x.Sort == sort && x.PhaseID == app.ap.PhaseId && x.DeleteStatus != true).FirstOrDefault();

                                  var checkdesk = (from u in _context.MyDesk where u.StaffID == staff.StaffID && u.AppId == app.ap.id && u.Sort == sort && u.ProcessID == process.ProccessID select u).FirstOrDefault();

                                      if (checkdesk == null)
                                      {


                                          //now create app process in mydesk table
                                          MyDesk drop = new MyDesk()
                                          {
                                              ProcessID = process.ProccessID,
                                              Sort = sort,
                                              AppId = app.ap.id,
                                              StaffID = staff.StaffID,
                                              FromStaffID = 0,
                                              HasWork = app.proc.Processed == true ? true : false,
                                              HasPushed = true,

                                          };
                                          if (app.proc.Date_Assigned != null)
                                              drop.CreatedAt = Convert.ToDateTime(app.proc?.Date_Assigned);
                                          else if (app.ap.CreatedAt != null)
                                              drop.CreatedAt = Convert.ToDateTime(app.ap.CreatedAt);
                                          else
                                              drop.CreatedAt = Convert.ToDateTime(app.ap.date_added);

                                          if (app.proc.DateProcessed != null)
                                          {
                                              drop.UpdatedAt = Convert.ToDateTime(app.proc?.DateProcessed);
                                          }
                                          _context.MyDesk.Add(drop);
                                          i = _context.SaveChanges();
                                      }
                                      else
                                      {
                                          checkdesk.ProcessID = process.ProccessID;
                                          checkdesk.Sort = sort;
                                          checkdesk.FromStaffID = 0;
                                          checkdesk.HasWork = app.proc.Processed == true ? true : false;
                                          checkdesk.HasPushed = true;
                                          if (app.proc.Date_Assigned != null)
                                              checkdesk.CreatedAt = Convert.ToDateTime(app.proc?.Date_Assigned);
                                          else if (app.ap.CreatedAt != null)
                                              checkdesk.CreatedAt = Convert.ToDateTime(app.ap.CreatedAt);
                                          else
                                              checkdesk.CreatedAt = Convert.ToDateTime(app.ap.date_added);

                                          if (app.proc.DateProcessed != null)
                                          {
                                              checkdesk.UpdatedAt = Convert.ToDateTime(app.proc?.DateProcessed);
                                          }
                                          i = _context.SaveChanges();

                                      }
                              
                              }

                        }
                        i = _context.SaveChanges();
                      }

                    //create staff newly
                    else
                      {

                          string location = branch.Name.ToLower().Contains("head office") ? "HQ" : "FO";
                          var loc = (from l in _context.Location where l.LocationName == location select l).FirstOrDefault();
                          branch.Name = branch.Name.ToLower() == "head office" ? "abuja head office" : branch.Name.ToLower();
                          branch.Name = branch.Name.ToLower() == "bauch field office" ? "bauchi field office" : branch.Name.ToLower();
                          var getStaffBranch = (from u in _context.FieldOffices where branch.Name.ToLower().Trim().Contains(u.OfficeName.ToLower().Trim()) select u).FirstOrDefault();

                          if (getStaffBranch != null)
                          {
                              Models.Staff staf = new Models.Staff()
                              {
                                  StaffElpsID = x.sf.UserId.Trim(),

                                  RoleID = SRoleID,
                                  StaffEmail = x.ub.UserEmail,
                                  FirstName = x.sf.FirstName,
                                  LastName = x.sf.LastName,
                                  CreatedAt = DateTime.Now,
                                  ActiveStatus = true,
                                  DeleteStatus = false,
                                  Theme = "Light",
                                  LocationID = loc?.LocationID,
                                  FieldOfficeID = getStaffBranch.FieldOffice_id,

                                  //SignaturePath = "path",
                                  //SignatureName = newFileName,
                                  CreatedBy = 0
                              };

                              _context.Staff.Add(staf);
                              int saved = _context.SaveChanges();


                              i = _context.SaveChanges();
                              var processingApp = (from ap in _context.application_Processings
                                                   join app in _context.applications on ap.ApplicationId equals app.id
                                                   where ap.processor == x.ub.Id
                                                   select new
                                                   {
                                                       app = app,
                                                       proc = ap
                                                   }).ToList();

                              //Check for applications with current desk as staff
                              foreach (var app in processingApp)
                              {

                                  var OldPhaseRouting = (from u in _context.PhaseRoutings
                                                         where u.PhaseId == app.app.PhaseId && u.ProcessingRule_Id == app.proc.ProcessingRule_Id
                                                         select u).FirstOrDefault();
                                  //First get application phase ID
                                  if (OldPhaseRouting != null)//5 is depot modification
                                  {

                                      sort = (int)OldPhaseRouting.SortOrder;
                                      process = _context.WorkProccess.Where(x => x.Sort == sort && x.PhaseID == app.app.PhaseId && x.DeleteStatus != true).FirstOrDefault();

                                      sort = process == null && sort > 7 ? 1 : sort;

                                      process = _context.WorkProccess.Where(x => x.Sort == sort && x.PhaseID == app.app.PhaseId && x.DeleteStatus != true).FirstOrDefault();
                                      var checkdesk = (from u in _context.MyDesk where u.StaffID == staff.StaffID && u.AppId == app.app.id && u.Sort == sort && u.ProcessID == process.ProccessID select u).FirstOrDefault();

                                          if (checkdesk == null)
                                          {


                                              //now create app process in mydesk table
                                              MyDesk drop = new MyDesk()
                                              {
                                                  ProcessID = process.ProccessID,
                                                  Sort = sort,
                                                  AppId = app.app.id,
                                                  StaffID = staff.StaffID,
                                                  FromStaffID = 0,
                                                  HasWork = app.proc.Processed == true ? true : false,
                                                  HasPushed = true,

                                              };
                                              if (app.proc.Date_Assigned != null)
                                                  drop.CreatedAt = Convert.ToDateTime(app.proc?.Date_Assigned);
                                              else if (app.app.CreatedAt != null)
                                                  drop.CreatedAt = Convert.ToDateTime(app.app.CreatedAt);
                                              else
                                                  drop.CreatedAt = Convert.ToDateTime(app.app.date_added);

                                              if (app.proc.DateProcessed != null)
                                              {
                                                  drop.UpdatedAt = Convert.ToDateTime(app.proc?.DateProcessed);
                                              }
                                              _context.MyDesk.Add(drop);
                                              i = _context.SaveChanges();
                                          }
                                          else
                                          {
                                              checkdesk.ProcessID = process.ProccessID;
                                              checkdesk.Sort = sort;
                                              checkdesk.FromStaffID = 0;
                                              checkdesk.HasWork = app.proc.Processed == true ? true : false;
                                              checkdesk.HasPushed = true;
                                              if (app.proc.Date_Assigned != null)
                                                  checkdesk.CreatedAt = Convert.ToDateTime(app.proc?.Date_Assigned);
                                              else if (app.app.CreatedAt != null)
                                                  checkdesk.CreatedAt = Convert.ToDateTime(app.app.CreatedAt);
                                              else
                                                  checkdesk.CreatedAt = Convert.ToDateTime(app.app.date_added);

                                              if (app.proc.DateProcessed != null)
                                              {
                                                  checkdesk.UpdatedAt = Convert.ToDateTime(app.proc?.DateProcessed);
                                              }
                                              i = _context.SaveChanges();

                                          }
                                      }
                                                                   }
                          

                              i = _context.SaveChanges();

                          }
                          else
                          {
                              _helpersController.LogMessages("no branch office was found for " + x.sf.Email);

                          }
                      }

                  });
                
            }
            else
            {
                return Json(new { success = false, Message = "An error occured while trying to get branches from ELPS" }); ;

            }
            return Json(new { success = true, Message = i + " number of staff and app process records have been updated successfully" });

        }

        // Phase Migration 
        public IActionResult PhaseMigration()
        {

            int i = 0;
            var getAllApp = (from ap in _context.Phases
                             select ap
                           ).ToList();


            getAllApp.ForEach(x =>
            {
                x.FlowType = x.id == 3 || x.id == 7 || x.id == 11 ? "Field_HQ": "HQ Only";
                x.CreatedAt = DateTime.Now;
                x.UpdatedAt = DateTime.Now;
                x.DeleteStatus =  false;

                i = _context.SaveChanges();

            });

            return Json(new { success = true, Message = i + " number of phase records have been updated successfully" });

        }
        // Phase Migration 

        public IActionResult FacilityMigration()
        {

            int i = 0;
            var getAllApp = (from ap in _context.Facilities
                             select ap
                           ).ToList();


            getAllApp.ForEach(x =>
            {
                x.DeletedStatus =  false;

                i = _context.SaveChanges();

            });

            return Json(new { success = true, Message = i + " number of facility records have been updated successfully" });

        }

    }
}