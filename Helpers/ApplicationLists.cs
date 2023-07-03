using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using NewDepot.Models;
using LpgLicense.Models;
using NewDepot.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using NewDepot.Controllers;

namespace NewDepot.Helpers
{        
    public class ApplicationHelper { 
   GeneralClass generalClass = new GeneralClass();

    SubmittedDocuments _appDocRep;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        private readonly Depot_DBContext _context; IHttpContextAccessor _httpContextAccessor;
        RestSharpServices _restService = new RestSharpServices();
        ElpsResponse elpsResponse = new ElpsResponse();
        Helpers.Authentications auth = new Helpers.Authentications();
        public ApplicationHelper(Depot_DBContext context, SubmittedDocuments appDocRep, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _appDocRep = appDocRep; _context = context;
            _configuration = configuration; 
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

        }


        public List<Document_Type> GetApplicationFiles(applications ap, int compElpsId, int facElpsId = 0)
        {



            var rqDocs = new List<Document_Type>();
            var ps = _context.Phases.Where(a => a.id == ap.PhaseId).FirstOrDefault();

            //All Company Document types & Facility Doc Types
            var allDocs = generalClass.GetDocumentTypes();
            var allFacDocs = generalClass.GetElpsFacDocumentsTypes();

            //get company Documents & company facility document
            var compDocs = generalClass.GetCompanyDocuments(compElpsId.ToString());
            var compFacDocs = new List<Company_Document>();

            //check for network error
            if (allDocs==null|| allFacDocs==null || compDocs==null)
            {
               rqDocs= null;
                return (rqDocs);
            }
            else
            {
                //get phase required documents
                var phasRqDocs = _context.ApplicationDocuments.Where(a => a.PhaseId == ap.PhaseId).ToList();
                //get phase required Facility documents
                var phasFacRqDocs = _context.PhaseFacilityDocuments.Where(a => a.PhaseId == ap.PhaseId).ToList();
                // get category required documents
                var catRqDocs = _context.document_type_categories.Where(a => a.category_id == ps.category_id).ToList();

                #region get Phase Level Documents

                foreach (var item in phasRqDocs)
                {
                    rqDocs.AddRange(ProcessRequiredDocs(item.ElpsDocTypeID, ap.id, compDocs, allDocs));
                }

                #endregion

                #region Facility Level Document
                foreach (var item in phasFacRqDocs)
                {
                    rqDocs.AddRange(ProcessFacRequiredDocs(item.document_type_id, ap.id, compFacDocs, allFacDocs));
                }
                #endregion

                #region Get Category Level Documents
                foreach (var item in catRqDocs)
                {
                    rqDocs.AddRange(ProcessRequiredDocs(item.document_type_id, ap.id, compDocs, allDocs));

                }
                #endregion

                #region Check if there are any other required doc for the application

                var extraDocs = _context.application_documents.Where(a => a.application_id == ap.id).ToList();
                foreach (var item in extraDocs)
                {
                    rqDocs.AddRange(ProcessRequiredDocs(item.document_type_id, ap.id, compDocs, allDocs));
                }
                var getAdditionalDoc = allFacDocs;
                List<int> ReqDocIDs = rqDocs.Select(x => x.Document_Id).Distinct().ToList();

                getAdditionalDoc = allFacDocs.Where(x => !rqDocs.Any(p => p.Id == x.Id)).ToList();

                foreach (var item in getAdditionalDoc)
                {
                    rqDocs.AddRange(ProcessAdditionalRequiredDocs(item.Document_Id, item.Document_Name, ap.id, compDocs, getAdditionalDoc));

                }
                #endregion

                return rqDocs;
            }
        }
        public List<Document_Type> GetAppFiles(applications ap, int compElpsId, int facElpsId = 0)
        {


            //bad var allFacDocs = _helpersController.GetElpsFacDocumentsTypes();
            var allFacDocs = generalClass.GetElpsFacDocumentsTypes();

            var compFacDocs = _helpersController.getFacilityDocuments(facElpsId.ToString());
            if (allFacDocs == null || compFacDocs == null)
            {
                return null;
            }
            else
            {

                var subdocs2 = (from sb in _context.SubmittedDocuments
                                join ad in _context.ApplicationDocuments on sb.AppDocID equals ad.AppDocID
                                where sb.AppID == ap.id
                                select new ApplicationDocuments
                                {
                                    ElpsDocTypeID = ad.ElpsDocTypeID
                                }).ToList();

                var docs = compFacDocs.Where(x => subdocs2.Any(y => y.ElpsDocTypeID == x.Document_Type_Id)).ToList();

                if (docs == null)
                {
                    return null;
                }
                else { 
                  
                    var doc = new List<Document_Type>();

                    foreach (var item in docs)
                    {
                        //check if doc name is not null in appdoc and appphasedoc table
                        var allAppDoc = _context.ApplicationDocuments.Where(x => x.ElpsDocTypeID == item.Document_Type_Id).FirstOrDefault();
                        var allPhaseDoc = _context.AppPhaseDocuments.Where(x => x.AppDocID == item.Document_Type_Id).FirstOrDefault();
                        if (allAppDoc != null)
                        {
                            allAppDoc.DocName = item.Name;
                            _context.SaveChanges();
                        }

                        var facdoc = allFacDocs.Where(x => x.Id == item.Document_Type_Id).FirstOrDefault();
                        string name = facdoc.documentTypeName != null ? facdoc.documentTypeName : "facdoc.documentTypeName";
                        doc.Add(new Document_Type
                        {
                            Name = name,
                            Selected = true,
                            Type = "Facility",
                            Document_Id = item.Id,
                            CoyFileId = item.Id,
                            Source = item.Source,
                            ParentSelected = true,
                            Id = item.Document_Type_Id
                        });
                    }

                    var phasedocs = _context.PhaseFacilityDocuments.Where(x => x.PhaseId == ap.PhaseId).ToList();
                    if (phasedocs.Where(x => doc.Any(y => y.Document_Id == x.document_type_id)) != null)
                    {
                        var sdoc = compFacDocs.Where(x => phasedocs.Any(y => y.document_type_id == x.Document_Type_Id))
                            .OrderByDescending(x => x.Date_Added).GroupBy(x => x.Document_Type_Id).Select(x => x.FirstOrDefault()).ToList();
                        if (sdoc != null)
                        {
                            foreach (var item in sdoc)
                            {
                                var f = subdocs2.FirstOrDefault(x => x.ElpsDocTypeID == item.Document_Type_Id);
                                if (f == null)
                                {
                                    var facdoc = allFacDocs.Where(x => x.Id == item.Document_Type_Id).FirstOrDefault();
                                    string name = facdoc.Name != null ? facdoc.Name : "facdoc.documentTypeName";

                                    doc.Add(new Document_Type
                                    {
                                        Name = name,
                                        Selected = true,
                                        Type = "Facility",
                                        Document_Id = item.Document_Type_Id,
                                        CoyFileId = item.Id,
                                        Source = item.Source,
                                        ParentSelected = true,
                                        Id = item.Document_Type_Id
                                    });
                                }
                            }
                        }
                    }

                    return doc.Where(x => phasedocs.Any(z => z.document_type_id == x.Id)).GroupBy(y => y.Id).Select(x => x.FirstOrDefault()).ToList();
                }
            }
            }

        //public List<Document_Type> GetAppFiles(applications ap, int compElpsId, int facElpsId = 0)
        //{
        //    var allFacDocs = generalClass.GetElpsFacDocumentsTypes();

        //    var compFacDocs =generalClass.getFacilityDocuments(facElpsId.ToString());

        //    var subdocs = _context.application_documents.Where(x => x.application_id == ap.id).ToList();
        //    var docs = compFacDocs.Where(x => subdocs.Any(y => y.document_type_id == x.document_type_id)).ToList();
        //    var doc = new List<Document_Type>();
        //    foreach (var item in docs)
        //    {
        //        var f = allFacDocs.FirstOrDefault(x => x.Id == item.document_type_id);
        //        doc.Add(new Document_Type
        //        {
        //            Name = f?.Name,
        //            Selected = true,
        //            Type = "Facility",
        //            Document_Id = item.Id,
        //            CoyFileId = item.Id,
        //            Source = item.Source,
        //            ParentSelected = true,
        //            Id = item.document_type_id
        //        });
        //    }

        //    var phasedocs = _context.PhaseFacilityDocuments.Where(x => x.PhaseId == ap.PhaseId).ToList();
        //    if (phasedocs.Where(x => doc.Any(y => y.Document_Id == x.document_type_id)) != null)
        //    {
        //        var sdoc = compFacDocs.Where(x => phasedocs.Any(y => y.document_type_id == x.document_type_id))
        //            .OrderByDescending(x => x.Date_Added).GroupBy(x => x.document_type_id).Select(x => x.FirstOrDefault()).ToList();
        //        if (sdoc != null)
        //        {
        //            foreach (var item in sdoc)
        //            {
        //                var f = subdocs.FirstOrDefault(x => x.document_type_id == item.document_type_id);
        //                if (f == null)
        //                {
        //                    doc.Add(new Document_Type
        //                    {
        //                        Name = allFacDocs.FirstOrDefault(x => x.Id == item.document_type_id)?.Name,
        //                        Selected = true,
        //                        Type = "Facility",
        //                        Document_Id = item.Id,
        //                        CoyFileId = item.Id,
        //                        Source = item.Source,
        //                        ParentSelected = true,
        //                        Id = item.document_type_id
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return doc.Where(x => phasedocs.Any(z => z.document_type_id == x.Id)).GroupBy(y => y.Id).Select(x => x.FirstOrDefault()).ToList();
        //}

        private List<Document_Type> ProcessRequiredDocs(int docTypId, int appId, List<Company_Document> compDocs, List<Document_Type> allDocs)
        {
            List<Document_Type> toreturn = new List<Document_Type>();
            //Get Application Documents
            var appDocs = _context.SubmittedDocuments.Where(a => a.AppID == appId).ToList();
            var appDocs2 = _context.application_documents.Where(a => a.application_id == appId).ToList();
            var app = _context.applications.Where(ap => ap.id == appId).FirstOrDefault();
            var company = _context.companies.Where(comp => comp.id == app.company_id).FirstOrDefault();
            var doc = compDocs.FirstOrDefault(a => a.Document_Type_Id == docTypId && !a.Archived);
            var dt = allDocs.Where(a => a.Id == docTypId).FirstOrDefault();
            #region
            if (doc == null)
            {
                // If company does not have the doc, pick from allDocs repository
                if (dt != null)
                {
                    dt.Type = "Company";
                    toreturn.Add(dt);
                }
            }
            #endregion
            #region
            else
            {
                // If company have the doc
                {
                    //check if it's already on Application Documents
                    var appDoc = appDocs.Where(a => a.AppDocID == docTypId).FirstOrDefault();

                        if (appDoc == null)
                        {
                            //Available on Company docs but not attached yet, so Attach it
                            appDoc = new SubmittedDocuments()
                            {
                                AppID = appId,
                                AppDocID = doc.Document_Type_Id,
                                //Document_Type_Id = docTypId
                                DocSource = doc.Source,
                                CompElpsDocID = company.elps_id,
                                CreatedAt = DateTime.Now,
                                DeletedStatus = false
                            };
                            _context.SubmittedDocuments.Add(appDoc);
                            _context.SaveChanges();
                        }
                         if (appDocs2.Count == 0)
                        {
                        //Available on Company docs but not attached yet, so Attach it
                        var appDocAdd = new application_documents()
                            {
                                application_id = appId,
                                document_id = doc.id,
                                document_type_id= doc.Document_Type_Id,
                                Status=true,
                                
                                //Document_Type_Id = docTypId
                                UniqueId= doc.UniqueId
                            };
                            _context.application_documents.Add(appDocAdd);
                            _context.SaveChanges();
                        }
                        if (appDoc.AppDocID != doc.Document_Type_Id)
                        {
                            //Available on Company doc and attached to app but it has been changed so update the attachement
                            appDoc.AppDocID = doc.Document_Type_Id;
                            appDoc.DocSource = doc.Source;
                            appDoc.UpdatedAt = DateTime.Now;
                            _context.SaveChanges();
                        //Available on Company doc and attached to app but it has been changed so update the attachement
                        var appDoc2 = appDocs2.Where(a => a.document_type_id == docTypId).FirstOrDefault();
                        if (appDoc != null)
                        {
                            appDoc2.application_id = appId;
                            appDoc2.document_id = docTypId;
                            appDoc2.document_type_id = doc.Document_Type_Id;
                            appDoc2.Status = true;
                            _context.SaveChanges();

                        }

                    }
                    string name = doc.documentTypeName != null ? doc.documentTypeName : "documentTypeName";

                    dt.Type = "Company";
                        dt.Selected = true;
                        dt.Document_Id = doc.Document_Type_Id;
                        dt.Name = name;
                        dt.CoyFileId = doc.id;
                        dt.Source = doc.Source;
                        dt.ParentSelected = doc.Status;
                        toreturn.Add(dt);
                    

                }
            }
            #endregion

            return toreturn;
        }
        private List<Document_Type> ProcessAdditionalRequiredDocs(int docTypId, string docTypName, int appId, List<Company_Document> compDocs, List<Document_Type> allDocs)
        {
            List<Document_Type> toreturn = new List<Document_Type>();
            //Get Application Documents
            var appDocs = _context.SubmittedDocuments.Where(a => a.AppID == appId).ToList();
            var appDocs2 = _context.application_documents.Where(a => a.application_id == appId).ToList();

            var doc = compDocs.Where(a => a.Document_Type_Id == docTypId && !a.Archived).FirstOrDefault();
            //var dt = context.document_types.Where(a => a.Id == item.document_type_id).FirstOrDefault();
            
            var dt = allDocs.Where(a => a.Id == docTypId).FirstOrDefault();
            #region
            if (doc == null)
            {
                //Coy does not have the doc, pick from allDocs
                if (dt != null)
                {
                    string name = doc.documentTypeName != null ? doc.documentTypeName : "documentTypeName";

                    dt.Type = "Company";
                    dt.Name = name;
                    dt.isAdditional = true;
                    
                        toreturn.Add(dt);
                    
                }
            }
            #endregion
            #region
            else
            {
                //Coy has the doc
                if (dt != null)
                {
                    //check if it's already on Application Documents
                    var appDoc = appDocs.Where(a => a.AppDocID == docTypId).FirstOrDefault();
                    //var appDoc = _appDocRep.Where(a => a.Application_Id == ap.id && a.document_type_id == item.document_type_id).FirstOrDefault();
                    if (appDoc == null)
                    {
                        //Available on Company docs but not attached yet, so Attach it
                        appDoc = new SubmittedDocuments()
                        {
                            AppID = appId,
                            AppDocID = doc.Document_Type_Id,
                            //Document_Type_Id = docTypId
                            DocSource=doc.Source,
                            CompElpsDocID= doc.id,
                            CreatedAt =DateTime.Now,
                            DeletedStatus=false
                        };
                        _context.SubmittedDocuments.Add(appDoc);
                        _context.SaveChanges();


                    }
                     if (appDocs2.Count == 0)
                    {
                        //Available on Company docs but not attached yet, so Attach it
                        var appDocAdd = new application_documents()
                        {
                            application_id = appId,
                            document_id = doc.id,
                            document_type_id = doc.Document_Type_Id,
                            Status = true,
                            //Document_Type_Id = docTypId
                            UniqueId = doc.UniqueId
                        };
                        _context.application_documents.Add(appDocAdd);
                        _context.SaveChanges();
                    }

                    if (appDoc.AppDocID != doc.Document_Type_Id)
                    {
                        //Available on Company doc and attached to app but it has been changed so update the attachement
                        appDoc.AppDocID = doc.Document_Type_Id;
                        appDoc.DocSource = doc.Source;
                        appDoc.UpdatedAt = DateTime.Now;
                        _context.SaveChanges();

                        //Available on Company doc and attached to app but it has been changed so update the attachement
                        var appDoc2 = appDocs2.Where(a => a.document_type_id == docTypId).FirstOrDefault();
                        if (appDoc != null)
                        {
                            appDoc2.application_id = appId;
                            appDoc2.document_id = doc.id;
                            appDoc2.document_type_id = doc.Document_Type_Id;
                            appDoc2.Status = true;
                            _context.SaveChanges();

                        }

                    }
                    string name = doc.documentTypeName != null ? doc.documentTypeName : "documentTypeName";

                    dt.Type = "Company";
                    dt.Selected = true;
                    dt.Document_Id = doc.Document_Type_Id;
                    dt.Name = name;
                    dt.CoyFileId = doc.id;
                    dt.Source = doc.Source;
                    dt.isAdditional = true;
                    dt.ParentSelected = doc.Status;
                    toreturn.Add(dt);
                }
            }
            #endregion

            return toreturn;
        }
        private List<Document_Type> ProcessFacRequiredDocs(int docTypId, int appId, List<Company_Document> facDocs, List<Document_Type> allDocs)
        {
            List<Document_Type> toreturn = new List<Document_Type>();
            //Get Application Documents
            var appDocs = _context.SubmittedDocuments.Where(a => a.AppID == appId).ToList();
            var appDocs2 = _context.application_documents.Where(a => a.application_id == appId).ToList();

            var doc = facDocs == null || facDocs.Count() <= 0 ? null : facDocs.FirstOrDefault(a => a.Document_Type_Id == docTypId && !a.Archived);
            var dt = allDocs.Where(a => a.Id == docTypId).FirstOrDefault();

            #region
            if (doc == null)
            {
                //Coy does not have the doc, pick from allDocs
                if (dt != null)
                {
                    dt.Type = "Facility";
                    toreturn.Add(dt);
                }
            }
            #endregion
            #region
            else
            {
                //Company have the doc
                if (dt != null)
                {
                    //check if it's already on Application Documents
                    var appDoc = appDocs.Where(a => a.AppDocID == docTypId).FirstOrDefault();
                    if (appDoc == null)
                    {
                        //Available on Company docs but not attached yet, so Attach it
                        appDoc = new SubmittedDocuments()
                        {
                            AppID = appId,
                            AppDocID = doc.Document_Type_Id,
                            DocSource = doc.Source,
                            CompElpsDocID = doc.id,
                            CreatedAt = DateTime.Now,
                            DeletedStatus = false
                        };
                        _context.SubmittedDocuments.Add(appDoc);
                        _context.SaveChanges();
                    }
                     if (appDocs2.Count == 0)
                    {
                        //Available on Company docs but not attached yet, so Attach it
                        var appDocAdd = new application_documents()
                        {
                            application_id = appId,
                            document_id = doc.id,
                            document_type_id = doc.Document_Type_Id,
                            Status = true,
                            //Document_Type_Id = docTypId
                            UniqueId = doc.UniqueId
                        };
                        _context.application_documents.Add(appDocAdd);
                        _context.SaveChanges();
                    }

                     if (appDoc.AppDocID != doc.Document_Type_Id)
                    {
                        //Available on Company doc and attached to app but it has been changed so update the attachement
                        appDoc.AppDocID = doc.Document_Type_Id;
                        appDoc.DocSource = doc.Source;
                        appDoc.UpdatedAt = DateTime.Now;
                        _context.SaveChanges();
                        //Available on Company doc and attached to app but it has been changed so update the attachement
                        var appDoc2 = appDocs2.Where(a => a.document_type_id == docTypId).FirstOrDefault();
                        if (appDoc != null)
                        {
                            appDoc2.application_id = appId;
                            appDoc2.document_id = doc.id;
                            appDoc2.document_type_id = doc.Document_Type_Id;
                            appDoc2.Status = true;
                            _context.SaveChanges();

                        }

                    }
                    string name = doc.documentTypeName != null ? doc.documentTypeName : "documentTypeName";
                    dt.Type = "Company";
                    dt.Selected = true;
                    dt.Document_Id = doc.Document_Type_Id;
                    dt.CoyFileId = doc.id;
                    dt.Name = name;
                    dt.Source = doc.Source;
                    dt.isAdditional = true;
                    dt.ParentSelected = doc.Status;
                    toreturn.Add(dt);
                }
            }
            #endregion

            return toreturn;
        }


    }


    public static class ApplicationLists
    {
       public static List<ProcessingLocation> GetLocation()
       {
           List<ProcessingLocation> locations = new List<ProcessingLocation>();
           locations.Add(new ProcessingLocation{Code="FD", Display="Field Office"});
           locations.Add(new ProcessingLocation{Code="ZN", Display="Zonal Office"});
           locations.Add(new ProcessingLocation{Code="HQ", Display="HeappDoc office"});
           return locations;
       }
    }

    public class ProcessingLocation
    {
        public string Code { get; set; }
        public string Display { get; set; }
    }

    public class ApplicationState
    {
        public int ApplicationId { get; set; }
        
        public string Stage { get; set; }
        //public int SubStage { get; set; }
        public string Message { get; set; }
        public Company _comp { get; set; }
        public addresses CompAddress { get; set; }
        private int CompDirs { get; set; }
        private int CompStaff { get; set; }

        applications _vAppRep;

        public ApplicationState(Company comp, addresses addresses, int applicationId, int compDir, int compStaff)
        {
            ApplicationId = applicationId;
            _comp = comp;
            CompStaff = compStaff;
            CompDirs = compDir;
            CompAddress = addresses;
            GetStage();
        }
        public ApplicationState(int applicationId, applications vAppRep)
        {
            this.ApplicationId = applicationId;
            _vAppRep = vAppRep;
        }

        private void GetStage()
        {
            //Stage 1: Category, Services & Specification selection
            //Stage 2: Application Review and Payment
            //Stage 3: Company Infomation
            //          3a: Company Details
            //          3b: Company Address
            //          3c: Company Directors
            //          3b: Company KeyStaff
            //Stage 4: Application Documents

            if (_comp != null && (string.IsNullOrEmpty(_comp.Contact_FirstName) || string.IsNullOrEmpty(_comp.Contact_LastName)))
            {
                // This means Company profile has not been filled
                Message = "Company Profile not completed. Please complete your company profile information to continue your application";
                Stage = "3a";
            }
            else if (CompAddress == null && CompAddress.StateId <= 0)
            {
                // No Address have been registered for this company OR The address is not properly completed
                Message = "No address found on your application or the Address is not completed properly. Please add your company address information to continue your application";
                Stage = "3b";
            }
            else if (CompDirs <= 0)
            {
                // No Directors have been registered for this company
                Message = "No Directors added to company profile. Please add at least one Director to your company to continue your application";
                Stage = "3c";
            }
            else
            {
                Message = "";
                Stage = "4";
                // Else: i.e. All Profile Filled, move to App Documents
            }

        }

    }

    public struct ApplicationStageResponse
    {
        public string stage { get; set; }
        public string Message { get; set; }
    }
   
    public static class AppDocConverter
    {
        public static SubmittedDocuments TovAppDoc(companies comp, files file, SubmittedDocuments adoc)
        {
            var toreturn = new SubmittedDocuments();
            toreturn.SubDocID = adoc.SubDocID;
            toreturn.AppDocID = adoc.AppDocID;
            toreturn.AppID = adoc.AppID;
            toreturn.CompElpsDocID = comp.elps_id;
            //toreturn.name = file.DocumentTypeName;
            toreturn.DocumentName = file.name;
            // toreturn.FileName = file.FileName;
            toreturn.DocSource = "httpcheck";
                //toreturn.Source.StartsWith("http")?toreturn.Source: configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value + file.source;
            //toreturn.Status = file.Status;
            //toreturn. = file.UniqueId;
            return toreturn;
        }
        
    }

    public static class ApplicationDocFilter
    {
        public static List<SubmittedDocuments> Filter(List<SubmittedDocuments> list)
        {
            var toreturn = new List<SubmittedDocuments>();
            foreach (var d in list)
            {
                var check = toreturn.Where(a => a.AppDocID == d.AppDocID && a.DeletedStatus != true).FirstOrDefault();

                if (check == null)
                    toreturn.Add(d);
            }

            return toreturn;
        }

    }


}