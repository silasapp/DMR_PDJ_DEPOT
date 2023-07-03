//using Newtonsoft.Json;
//using NMDPRA_Depot.Domain.Abstract;
//using NewDepot.Models;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Net;
//using System.Transactions;
//using System.Web;
//using System.Web.Script.Serialization;
//using NewDepot.Models;
//using System.IO;

//namespace NewDepot.Helpers
//{
//    public class NextApproval
//    {
//        IapplicationsProcessingRuleRepository _vAppProcRep;
//        IApplication_ProcessingRepository _appProcRep;
//        IApplicationRepository _appRep;
//        IFacilityRepository _facilityRep;
//        ICompanyRepository _coyRep;
//        IAddressRepository _addRep;
//        IStaffRepository _StaffRep;
//        IApplication_Desk_HistoryRepository _appHist;
//        IJointAccountRepository _jointRep;
//        string selfBaseUrl = ConfigurationManager.AppSettings["selfBaseUrl"];
//        //IapplicationsAddressRepository _vAppliAddresRep;
//        //IUserBranchRepository _userBranchRep;
//        //IBranchRepository _branchRep;
//        //I.filesRepository _vCompFileRep;
//        //ICategoryRepository _catRep;
//        //IDocument_TypeRepository _docType;
//        //IDocument_Type_CategoryRepository _docTypeCatRep;
//        //IApplication_DocumentRepository _appDoc;
//        //IapplicationsDocumentRepository _vAppDoc;
//        //IDocument_Type_ApplicationRepository _documentApplicationRep;
//        //IZoneRepository _zoneRep;

//        #region Inuse
//        public NextApproval(IapplicationsProcessingRuleRepository vAppProc, IApplication_ProcessingRepository appProcRep,
//            IApplicationRepository appRep, IFacilityRepository facilityRep, ICompanyRepository coyRep, IAddressRepository addRep,
//            IStaffRepository StaffRep, IApplication_Desk_HistoryRepository appHist, IJointAccountRepository jointRep)
//        {
//            _vAppProcRep = vAppProc;
//            _appProcRep = appProcRep;
//            _appRep = appRep;
//            _facilityRep = facilityRep;
//            _coyRep = coyRep;
//            _addRep = addRep;
//            _StaffRep = StaffRep;
//            _appHist = appHist;
//            _jointRep = jointRep;
//        }

//        public string Assign(int Id, string UserName, string Ip, int userBranchId = 0)
//        {
//            try
//            {

//                //UtilityHelper.LogMessages($"we got to assign staff");

//                var appl = _appRep.FindBy(a => a.Id == Id).FirstOrDefault();
//                //if (appl.PhaseId==3 || appl.PhaseId==11)
//                //{
//                //    return "Ok";
//                //}
//                var context = new NMDPRA_DepotContext();
//                var app = _vAppProcRep.FindBy(C => C.ApplicationId == Id).ToList();
//                if (app.Count() <= 0)
//                    return "Invalid Application!"; // throw (new Exception());
//                var checker = app.FirstOrDefault();
//                List<Application_Processing> ApplicationsSteps = _appProcRep.FindBy(C => C.ApplicationId == checker.ApplicationId).OrderBy(C => C.sortOrder).ToList();
//                //Check if processing is still pending on a desk
//                Application_Processing processing = ApplicationsSteps.Find(C => C.Assigned == true && C.Processed == false);
//                if (processing != null)
//                    return string.Format("Application is still pending on user {0} approval", processing.Processor); //throw new Exception();
//                Application_Processing nextProcess = ApplicationsSteps.Find(C => C.Assigned == false);
//                if (nextProcess == null)
//                {
//                    appl.Status = "Approved";
//                    appl.Current_Desk = null;
//                    _appRep.Edit(appl);
//                    _appRep.Save(UserName, Ip);

//                    #region Update App Status on ELPS if Approved 
//                    //if (appl.PhaseId != 7)
//                    //{
//                    if (appl.PhaseId == 6)
//                    {
//                        //Take Over
//                        //get the facility, check if the companyId has been updated, else update and also push to elps
//                        // to complete proper change of Ownership
//                        var fac = _facilityRep.FindBy(a => a.Id == appl.FacilityId).FirstOrDefault();
//                        if (fac != null && fac.CompanyId != appl.Company_Id)
//                        {

//                            fac.CompanyId = appl.Company_Id;
//                            _facilityRep.Edit(fac);
//                            _facilityRep.Save(UserName, Ip);
//                        }
//                    }


//                    using (WebClient clientL = new WebClient())
//                    {
//                        clientL.Headers.Add(HttpRequestHeader.ContentType, "application/json");
//                        var appAPI = new ApplicationAPIModel();
//                        appAPI.Status = appl.Status;
//                        appAPI.OrderId = appl.Reference;

//                        var param = new JavaScriptSerializer().Serialize(appAPI);
//                        //var param = "{\"orderId\":" + app.Reference + ",\"status\":\"" + app.Status + "\"}";

//                        var urlL = ELPSAPIHelper.ApiBaseUrl;
//                        urlL += "Application/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash;

//                        var outputL = clientL.UploadString(urlL, "PUT", param);

//                        var respApp = JsonConvert.DeserializeObject<ApplicationAPIModel>(outputL);
//                    }
//                    //}
//                    #endregion

//                    #region Log in History
//                    var hist = new Application_Desk_History();
//                    hist.Application_Id = Id;
//                    hist.Date = UtilityHelper.CurrentTime;
//                    hist.Comment = "Application Approved and License/Permit Issued";
//                    hist.UserName = UserName;
//                    hist.Status = "Final Approval";
//                    _appHist.Add(hist);
//                    _appHist.Save(UserName, Ip);
//                    #endregion

//                    return "Processing Complete";
//                }

//                string Location = nextProcess.ProcessingLocation;
//                var application = _appRep.FindBy(a => a.Id == Id).FirstOrDefault();
//                var facility = _facilityRep.FindBy(a => a.Id == application.FacilityId).FirstOrDefault();
//                var coyLocal = _coyRep.FindBy(c => c.Id == application.Company_Id).FirstOrDefault();

//                var facilityAddress = _addRep.FindBy(a => a.Id == facility.AddressId).FirstOrDefault();
//                int branchId = GetBranch(facilityAddress.StateId, Location);
//                var nextappstep = app.OrderBy(a => a.sortOrder).Where(a => a.Assigned == false).FirstOrDefault();

//               // UtilityHelper.LogMessages($"BranchId: {branchId} and next App Step: {JsonConvert.SerializeObject(nextappstep)}, facility Address: {JsonConvert.SerializeObject(facilityAddress)}");
//                Staff userBranch = null;
//                if (userBranchId > 0)
//                {
//                    userBranch = _StaffRep.FindBy(a => a.Id == userBranchId).FirstOrDefault();
//                }
//                else
//                {
//                    userBranch = _StaffRep.FindBy(C => C.BranchId == branchId && C.DepartmentId == nextappstep.DepartmentId && C.RoleId == nextappstep.RoleId && C.Active == true).OrderBy(C => C.DeskCount).FirstOrDefault();

//                }
//                if (userBranch==null)
//                {
//                   // UtilityHelper.LogMessages($"we couldnt find staff here: {facilityAddress.City}");
//                    //get the zone which this Branch belongs to.
//                    branchId = GetBranch(facilityAddress.StateId, "znfd");
//                    userBranch = _StaffRep.FindBy(C => C.BranchId == branchId && C.DepartmentId == nextappstep.DepartmentId && C.RoleId == nextappstep.RoleId && C.Active == true).OrderBy(C => C.DeskCount).FirstOrDefault();

//                   // UtilityHelper.LogMessages($"we checked zone and: {JsonConvert.SerializeObject(userBranch)}");

//                }
//                nextProcess.Processor = userBranch.Id;
//                nextProcess.Assigned = true;
//                nextProcess.Dateprocessed = UtilityHelper.CurrentTime;
//                _appProcRep.Edit(nextProcess);
//                _appProcRep.Save(UserName, Ip);

//                #region Log in History
//                var history = new Application_Desk_History();
//                history.Application_Id = Id;
//                history.Date = UtilityHelper.CurrentTime;
//                history.Comment = "Application landed on Staff Desk";
//                history.UserName = userBranch.UserEmail;
//                history.Status = "Move";
//                _appHist.Add(history);
//                _appHist.Save(UserName, Ip);
//                #endregion

//                application.Current_Desk = nextProcess.Id;
//                _appRep.Edit(application);
//                _appRep.Save(UserName, Ip);

//                //|| application.PhaseId == 3
//                //check for all the Applications that starts from the Field Office
//                //|| application.PhaseId == 5  || application.PhaseId == 9 || application.PhaseId == 11

//                if ((application.Type.ToLower() == "renew") && nextappstep.sortOrder == 1)
//                {
//                    // 5 for Facility Modification; 9 for Modification without approval; 
//                    // 11 for Recalibration of tanks

//                    UtilityHelper.LogMessages($"Yes about to notify.");

//                    var requiredStaff = _StaffRep.FindBy(C => C.Active && C.Role.ToLower() == "opscon" || C.Role.ToLower() == "adops").ToList();

//                    //Push applications to opscon,AdOps,
//                    NotifyZNFD(application.Id, facilityAddress.StateId, requiredStaff, UserName, Ip);
//                }

//                //trans.Complete();
//                return "Ok";
//                //  return View();

//            }
//            catch (Exception x)
//            {
//                UtilityHelper.LogMessages($"From Assign Next Staff- AppId {Id} User {UserName} ::: {x.ToString()}");
//                //trans.Dispose();
//                return "not ok";
//            }
//        }
        
//        public bool NotifyZNFD(int id, int stateId, List<Staff> opscons, string username, string ip)
//        {
//            // Get Opscon to handle
//            //UtilityHelper.LogMessages($"Yes, we are here to add to notify opscon");

//            #region get the Staff to be added
//            //Get all Zones
//            var client = new WebClient();
//            string output = client.DownloadString(ELPSAPIHelper.ApiBaseUrl + "Branch/ZoneMapping/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash);
//            var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);

//            List<Staff> selectedOps = new List<Staff>();
//            vZone selectedZone = null;
//            selectedZone = zonesmapping.Where(a => a.StateId == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
//            if (selectedZone == null)
//            {
//                //Loop thru the states in each zone for possible selection
//                selectedZone = zonesmapping.Where(a => a.CoveredStates.Select(s => s.StateId).Contains(stateId)).FirstOrDefault();
//                if (selectedZone != null)
//                {
//                    //Found one zone.
//                    selectedOps = opscons.Where(a => a.BranchId == selectedZone.BranchId).ToList();

//                }
//            }
//            else
//            {
//                selectedOps = opscons.Where(a => a.BranchId == selectedZone.BranchId).ToList();

//            }

//            #endregion
           
//            if (selectedOps != null && selectedOps.Count > 0 && selectedZone != null)
//            {
//                foreach (var ops in selectedOps)
//                {


//                    var checkJoint = _jointRep.FindBy(a => a.ApplicationId == id && a.Opscon == ops.UserEmail).FirstOrDefault();
//                    if (checkJoint == null)
//                    {

//                        var joint = new JointAccount();
//                        joint.ApplicationId = id;
//                        joint.DateAdded = DateTime.Now;
//                        joint.OperationsCompleted = false;
//                        joint.Opscon = ops.UserEmail;
//                        joint.Assigned = false;
//                        _jointRep.Add(joint);
//                        _jointRep.Save(username, ip);
//                    }
//                }

//                #region Send Mail to Notify Opscon and Supervisor
//                var body = "";
//                using (var sr = new StreamReader(HttpContext.Current.Server.MapPath(@"\\App_Data\\Templates\") + "InternalMemo.txt"))
//                {
//                    body = sr.ReadToEnd();
//                }
//                var subject = "New Application on Depot Portal";
//                var context = new NMDPRA_DepotContext();
//                var vApp = context.applicationss.Where(a => a.Id == id).FirstOrDefault();
//                foreach (var ops in selectedOps)
//                {
//                    var stf = context.Staffs.Where(a => a.UserId == ops.UserEmail).FirstOrDefault();
//                    //var man = _staffRepo.FindBy(a => a.UserId == manager.UserEmail).FirstOrDefault();
//                    var type = (vApp.Type.ToLower() == "new" ? "New Depot License" : "Depot License Renewal").ToString() + "(" + vApp.PhaseName + ")";
//                    var msg = $"A new application on NMDPRA Depot Portal for " + type + " has been submitted on the portal and you are hereby notified for Joint Operation towards the issuing of the License/Approval. <p>Details of the Application is as follow:</p>";
//                    msg += "<table class'table'>" +
//                        $"<tr><td>Application Reference</td><td><a href='{selfBaseUrl}/Process/ViewApplication/" + vApp.Id + "'>" + vApp.Reference + "</a></td></tr>" +
//                        $"<tr><td>Application Company</td><td><a href='{selfBaseUrl}/Company/Detail/" + vApp.Company_Id + "'>" + vApp.CompanyName + "</a></td></tr>" +
//                        $"<tr><td>Facility</td><td><a href='{selfBaseUrl}/Facility/ViewFacility/" + vApp.FacilityId + "'>" + vApp.FacilityName + "(" + vApp.FacilityAddress() + ")</a></td></tr>" +
//                        "<tr><td>Facility Address</td><td>" + vApp.FacilityFullAddress() + "</td></tr>" +
//                        "</table><br /><br /><p>You will be notified on the progress and Action required by you as the application process progresses.</p>";
//                    var msgBody = string.Format(body, subject, "", ops.FirstName, msg);

//                    MailHelper.SendEmail(ops.UserEmail, subject, msgBody);
//                }
//                #endregion

//                return true;
//            }
//            return false;
//        }

//        public int GetBranch(int stateId, string ProcessLocationCode)
//        {
//            //UtilityHelper.LogMessages($"StateId: {stateId}");
//            ProcessLocationCode = ProcessLocationCode.ToLower();
//            var client = new WebClient();
//            string output = client.DownloadString(ELPSAPIHelper.ApiBaseUrl + "Branch/ZoneMapping/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash);
//            var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);
//            vZone selectedZone = null; //
//           // UtilityHelper.LogMessages($"Zones: {JsonConvert.SerializeObject(zonesmapping)}");
//            if (ProcessLocationCode == "zn") // || ProcessLocationCode.ToLower() == "fd")
//            {
//                selectedZone = zonesmapping.Where(a => a.StateId == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
//                if (selectedZone == null)
//                    return 0;
//                //UtilityHelper.LogMessages($"From first Zones: {JsonConvert.SerializeObject(selectedZone)}");

//                return selectedZone.BranchId;
//            }
//            else if (ProcessLocationCode == "fd" || ProcessLocationCode == "znfd")
//            {
//                try
//                {
//                    var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Select(s => s.StateId).Contains(stateId)).FirstOrDefault();
//                    if (zn == null)
//                    {
//                        //No zone covers the supplied state, check if the zone itself is the FD
//                        zn = zonesmapping.Where(a => a.StateId == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
//                        //UtilityHelper.LogMessages($"From FD Inner: {JsonConvert.SerializeObject(zn)}");

//                        return zn.BranchId;
//                    }
//                    var fdId = zn.CoveredFieldOffices.Where(a => a.StateId == stateId).FirstOrDefault().Id;
//                    //UtilityHelper.LogMessages($"From FD : {JsonConvert.SerializeObject(zn)}");
//                    if (ProcessLocationCode == "znfd")
//                    {
//                       // UtilityHelper.LogMessages("znfd");
//                        fdId = zn.BranchId;
//                    }
//                    return fdId;
//                }
//                catch (Exception)
//                {
//                    return 0;
//                }

//            }
            
//            else if (ProcessLocationCode == "hq")
//            {
//                selectedZone = zonesmapping.Where(a => a.Code.ToLower() == ProcessLocationCode).FirstOrDefault();

//                if (selectedZone == null)
//                    return 0;
//                return selectedZone.BranchId;
//            }
//            return 0;

//        }
//        #endregion

//        public static string GetBranchName(string stateId, string ProcessLocationCode)
//        {
//           // UtilityHelper.LogMessages($"State name :{stateId}");
//            ProcessLocationCode = ProcessLocationCode.ToLower();
//            var client = new WebClient();
//            string output = client.DownloadString(ELPSAPIHelper.ApiBaseUrl + "Branch/ZoneMapping/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash);
//            //UtilityHelper.LogMessages(output);
//            var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);
//            vZone selectedZone = null; //
//            if (ProcessLocationCode == "zn") // || ProcessLocationCode.ToLower() == "fd")
//            {
//                selectedZone = zonesmapping.Where(a => a.StateName == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
//                if (selectedZone == null)
//                    return "";
//                return selectedZone.BranchName;
//            }
//            else if (ProcessLocationCode == "fd")
//            {
//                try
//                {
//                    var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Any(x => x.StateName.Contains(stateId))).FirstOrDefault();
//                    //var zn = zonesmapping.Where(a => a.CoveredFieldOffices.Select(s => s.StateName).Contains(stateId)).FirstOrDefault();
//                    if (zn == null)
//                    {
//                       // UtilityHelper.LogMessages($"{stateId} did not return any value");
//                        //No zone covers the supplied state, check if the zone itself is the FD
//                        zn = zonesmapping.Where(a => a.StateName == stateId && a.Code.ToLower() != "hq").FirstOrDefault();
//                        return zn?.BranchName + "|ZN";
//                    }
//                    //UtilityHelper.LogMessages(JsonConvert.SerializeObject(zn));
//                    //var fdId = zn.CoveredFieldOffices.Where(a => a.StateName == stateId).FirstOrDefault().Name;
//                    return zn?.CoveredFieldOffices.FirstOrDefault(x => x.StateName.Contains(stateId)).Name;
//                }
//                catch (Exception x)
//                {
//                   UtilityHelper.LogMessages($"{stateId} did not return any value and error occured {x.ToString()}");


//                    return "";
//                }

//            }
//            else if (ProcessLocationCode == "hq")
//            {
//                selectedZone = zonesmapping.Where(a => a.Code.ToLower() == ProcessLocationCode).FirstOrDefault();

//                if (selectedZone == null)
//                    return "";
//                return selectedZone.BranchName;
//            }
//            return "";

//        }

//        #region Archive
//        ///// <summary>
//        ///// List of Required Documents remaining for the supplied Application
//        ///// </summary>
//        ///// <param name="id"></param>
//        ///// <returns></returns>
//        //public List<Document_Type> DocsRemaining(int id)
//        //{
//        //    var appDocs = _vAppDoc.FindBy(a => a.Application_Id == id).ToList();        //Docs already attached to app

//        //    List<Document_Type> remainingfiles = new List<Document_Type>();
//        //    List<Document_Type> requiredFiles = ApplicationDocuments(id);   //From (Cat/Serv/Spec/app special) docs
//        //    var othId = Convert.ToInt16(ConfigurationManager.AppSettings["OtherDocId"].ToString());
//        //    if (appDocs.Count > 0)
//        //    {
//        //        foreach (var item in requiredFiles)
//        //        {
//        //            if (item.Id != othId)
//        //            {
//        //                var file = appDocs.Where(d => (d.Document_Type_Id == item.Id && d.Status.GetValueOrDefault() == true)).FirstOrDefault();
//        //                if (file == null)
//        //                {
//        //                    remainingfiles.Add(item);
//        //                }
//        //            }
//        //            else
//        //            {
//        //                //compare uniqueids
//        //                var file = appDocs.Where(d => (d.Document_Type_Id == item.Id && d.Status.GetValueOrDefault() == true && d.UniqueId == item.UniqueId)).FirstOrDefault();
//        //                //if (!string.IsNullOrEmpty(item.UniqueId) && item.Id == othId)
//        //                //{
//        //                //    remainingfiles.Add(item);
//        //                //}
//        //                if (file == null)
//        //                {
//        //                    remainingfiles.Add(item);
//        //                }
//        //            }
//        //        }
//        //        return remainingfiles;
//        //    }
//        //    else
//        //    {
//        //        return requiredFiles;
//        //    }
//        //}

//        //public List<Document_Type> DocsRemaining(List<Company_Document> CoyDocs, string appid) //(List<applicationsDocument> AppDocs, string appid)
//        //{
//        //    //var appDocs = _vAppDoc.FindBy(a => a.Application_Id == id).ToList();        //Docs already attached to app
//        //    int id = int.Parse(appid);
//        //    List<Document_Type> remainingfiles = new List<Document_Type>();
//        //    List<Document_Type> requiredFiles = ApplicationDocuments(id);   //From (Cat/Serv/Spec/app special) docs
//        //    var othId = Convert.ToInt16(ConfigurationManager.AppSettings["OtherDocId"].ToString());
//        //    if (CoyDocs.Count > 0)
//        //    {
//        //        foreach (var item in requiredFiles)
//        //        {
//        //            if (item.Id != othId)
//        //            {
//        //                var file = CoyDocs.Where(d => d.Document_Type_Id == item.Id && d.Status).FirstOrDefault();
//        //                if (file == null)
//        //                {
//        //                    remainingfiles.Add(item);
//        //                }
//        //            }
//        //            else
//        //            {
//        //                //compare uniqueids
//        //                var file = CoyDocs.Where(d => d.Document_Type_Id == item.Id && d.Status && d.UniqueId == item.UniqueId).FirstOrDefault();
//        //                //if (!string.IsNullOrEmpty(item.UniqueId) && item.Id == othId)
//        //                //{
//        //                //    remainingfiles.Add(item);
//        //                //}
//        //                if (file == null)
//        //                {
//        //                    remainingfiles.Add(item);
//        //                }
//        //            }
//        //        }
//        //        return remainingfiles;
//        //    }
//        //    else
//        //    {
//        //        return requiredFiles;
//        //    }
//        //}

//        //public int NoOfDocsRemaining(int id)
//        //{
//        //    return DocsRemaining(id).Count();
//        //}

//        ///// <summary>
//        ///// Gets a List of all Documents required for the Supplied Application
//        ///// </summary>
//        ///// <param name="Id"></param>
//        ///// <returns></returns>
//        //public List<Document_Type> ApplicationDocuments(int Id)
//        //{
//        //    Application app = _appRep.FindBy(C => C.Id == Id).FirstOrDefault();
//        //    if (app == null)
//        //        return null;

//        //    Category cat = _catRep.FindBy(C => C.Id == app.Category_Id).FirstOrDefault();
//        //    //List<Application_Service> applicationServices = _appServRep.FindBy(C => C.Application_Id == Id).ToList();
//        //    //List<Application_Job_Specification> applicationSpecification = _appJobSpecRep.FindBy(C => C.Application_Id == Id).ToList();

//        //    List<Document_Type> allDocuments = _docType.GetAll().ToList();
//        //    List<Document_Type> returnedDocuments = new List<Document_Type>();
//        //    List<Document_Type_Category> cateDoc = _docTypeCatRep.FindBy(C => C.Category_Id == cat.Id).ToList();
//        //    //List<Document_Type_Service> serviDoc = _doc_serviceRep.GetAll().ToList();
//        //    //List<Document_Type_Job_Specification> speciDoc = _doc_specRep.GetAll().ToList();

//        //    //add all selected category documents

//        //    foreach (var item in cateDoc)
//        //    {
//        //        if (returnedDocuments.Count() <= 0 || returnedDocuments.Find(C => C.Id == item.Document_Type_Id) == null)
//        //        {
//        //            Document_Type docuToAdd = allDocuments.Find(C => C.Id == item.Document_Type_Id);
//        //            if (docuToAdd != null)
//        //                returnedDocuments.Add(docuToAdd);
//        //        }
//        //    }

//        //    // get documents in selected services
//        //    //foreach (var item in applicationServices)
//        //    //{
//        //    //    //List<Document_Type_Service> relatedServiceDoc = serviDoc.FindAll(C => C.Service_Id == item.Service_Id);

//        //    //    //foreach (var documentService in relatedServiceDoc)
//        //    //    //{
//        //    //    //    if (returnedDocuments.Find(C => C.Id == documentService.Document_Type_Id) == null)
//        //    //    //    {
//        //    //    //        Document_Type docuToAdd = allDocuments.Find(C => C.Id == documentService.Document_Type_Id);
//        //    //    //        returnedDocuments.Add(docuToAdd);
//        //    //    //    }
//        //    //    //}
//        //    //}

//        //    // get documents in selected Specifications

//        //    //foreach (var item in applicationSpecification)
//        //    //{
//        //    //    //List<Document_Type_Job_Specification> relatedSpecDoc = speciDoc.FindAll(C => C.Job_Specification_Id == item.Job_Specification_Id);

//        //    //    foreach (var docementSpec in relatedSpecDoc)
//        //    //    {
//        //    //        if (returnedDocuments.Find(C => C.Id == docementSpec.Document_Type_Id) == null)
//        //    //        {
//        //    //            Document_Type docuToAdd = allDocuments.Find(C => C.Id == docementSpec.Document_Type_Id);
//        //    //            returnedDocuments.Add(docuToAdd);
//        //    //        }
//        //    //    }
//        //    //}

//        //    //get documents requested for a specific Application
//        //    List<Document_Type_Application> ApplicationDocuments = _documentApplicationRep.FindBy(C => C.ApplicationId == app.Id).ToList();
//        //    var othId = Convert.ToInt16(ConfigurationManager.AppSettings["otherDocId"].ToString());
//        //    foreach (var apdoc in ApplicationDocuments)
//        //    {
//        //        Document_Type docuToAdd = new Document_Type();
//        //        if (apdoc.DocumentTypeId == othId)
//        //        {
//        //            docuToAdd.UniqueId = apdoc.UniqueId;
//        //            docuToAdd.Selected = false;
//        //            docuToAdd.ParentSelected = false;
//        //            docuToAdd.Id = apdoc.DocumentTypeId;
//        //            docuToAdd.Name = "Other Document";
//        //        }
//        //        else
//        //        {
//        //            docuToAdd = allDocuments.Where(a => a.Id == apdoc.DocumentTypeId).FirstOrDefault();
//        //        }
//        //        returnedDocuments.Add(docuToAdd);
//        //    }
//        //    _documentApplicationRep.Save("System", "System");
//        //    return returnedDocuments;
//        //}
//        #endregion
//    }
//}