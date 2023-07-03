//using Newtonsoft.Json;
//using NewDepot.Models;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Net;
//using System.Web;
//using System.Web.Script.Serialization;
//using NewDepot.Models;

//namespace NewDepot.Helpers
//{
//    public class ELPSAPIHelper
//    {
//        #region ELPS API Params
//        public static string ApiEmail { get { return ConfigurationManager.AppSettings["mkEm"]; } }
//        public static string ApiKey { get { return ConfigurationManager.AppSettings["mk"]; } }
//        public static string PublicKey { get { return ConfigurationManager.AppSettings["pk"]; } }
//        public static string ApiHash { get { return PaymentRef.getHash(ApiEmail + ApiKey); } }
//        public static string ApiBaseUrl { get { return ConfigurationManager.AppSettings["mkUrl"]; } }
//        public static string ApiPayUrl { get { return ConfigurationManager.AppSettings["elpsPaymentUrl"]; } }

//        #endregion

//        #region Compny Document

         
//        public List<Company_Document> GetCompanyDocuments(int id, string type = "")
//        {
//            var compDocs = new List<Company_Document>();
//            using (LocalWebClient client = new LocalWebClient())
//            {
//                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
//                var Url = "";
//                if (string.IsNullOrEmpty(type) || type.ToLower().Trim() == "company")
//                {
//                    Url = ApiBaseUrl + "CompanyDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;
//                }
//                else
//                {
//                    Url = ApiBaseUrl + "FacilityDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;
//                }
//                UtilityHelper.LogMessages(Url);
//                var output = client.DownloadString(Url);
//                compDocs = JsonConvert.DeserializeObject<List<Company_Document>>(output);
//            }

//            return compDocs;
//        }

//        public List<files> GetCompanyDocuments(int id)
//        {
//            var compDocs = new List<files>();
//            using (LocalWebClient client = new LocalWebClient())
//            {
//                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
//                var Url = ApiBaseUrl + "CompanyDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;

//                var output = client.DownloadString(Url);
//                compDocs = JsonConvert.DeserializeObject<List<files>>(output);
//            }

//            return compDocs;
//        }
//        public .files GetCompanyDocument(int id, string type = "")
//        {
//            var compDocs = new .files();
//            using (LocalWebClient client = new LocalWebClient())
//            {
//                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
//                var Url = "";
//                if (!string.IsNullOrEmpty(type) && type.ToLower() == "facility")
//                {
//                    Url = ApiBaseUrl + "FacilityDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;
//                }
//                else
//                {
//                    Url = ApiBaseUrl + "CompanyDocument/" + id + "/" + ApiEmail + "/" + ApiHash;
//                }
//                try
//                {
//                    var output = client.DownloadString(Url);
//                    compDocs = JsonConvert.DeserializeObject<files>(output);

//                }
//                catch (Exception)
//                {


//                }
//            }

//            return compDocs;
//        }


//        //public List<Document_Type> GetDocumentTypes()
//        //{
//        //    var compDocs = new List<Document_Type>();
//        //    using (LocalWebClient client = new LocalWebClient())
//        //    {
//        //        client.Headers.Add(HttpRequestHeader.Accept, "application/json");
//        //        var Url = ApiBaseUrl + "Documents/Types/" + ApiEmail + "/" + ApiHash;

//        //        var output = client.DownloadString(Url);
//        //        compDocs = JsonConvert.DeserializeObject<List<Document_Type>>(output);
//        //    }

//        //    return compDocs;
//        //}

//        public List<Document_Type> GetDocumentTypes(string type = "")
//        {
//            var compDocs = new List<Document_Type>();
//            using (LocalWebClient client = new LocalWebClient())
//            {
//                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
//                string Url = "";
//                if (string.IsNullOrEmpty(type))
//                {
//                    Url = ApiBaseUrl + "Documents/Types/" + ApiEmail + "/" + ApiHash;
//                }
//                else
//                {
//                    Url = ApiBaseUrl + "Documents/Facility/" + ApiEmail + "/" + ApiHash + "/" + type;
//                }

//                var output = client.DownloadString(Url);
//                compDocs = JsonConvert.DeserializeObject<List<Document_Type>>(output);
//            }

//            return compDocs;
//        }

//        #endregion

//        #region Permit
//        public bool PushPermit(PermitAPIModel model, out int elpsid)
//        {
//            elpsid = 0;
//            using (LocalWebClient client = new LocalWebClient())
//            {
//                try
//                {
//                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
//                    var Url = ApiBaseUrl + "Permits/" + model.Company_Id + "/" + ApiEmail + "/" + ApiHash;
//                    var mod = new JavaScriptSerializer().Serialize(model);

//                    var output = client.UploadString(Url, "POST", mod);
//                    var response = JsonConvert.DeserializeObject<PermitAPIModel>(output);

//                    if (response != null && !string.IsNullOrEmpty(response.permit_no))
//                    {
//                        elpsid = response.Id;
//                        return true;
//                    }
//                    else
//                        return false;
//                }
//                catch (Exception)
//                {
//                    return false;
//                }
//            }
//        }

//        public FacilityAPIModel PushFacility(FacilityAPIModel model)
//        {
//            //elpsid = 0;
//            using (LocalWebClient client = new LocalWebClient())
//            {
//                try
//                {
//                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

//                    var param = JsonConvert.SerializeObject(model);
//                    var url = ApiBaseUrl;
//                    url += "Facility/Add/" +ApiEmail + "/" + ApiHash;
//                    //var Url = ApiBaseUrl + "Permits/" + model.Company_Id + "/" + ApiEmail + "/" + ApiHash;
//                    //var mod = new JavaScriptSerializer().Serialize(model);
//                    UtilityHelper.LogMessages($"Push Facility to Elps:: {param} :: {url}");
//                    var output = client.UploadString(url, "POST", param);

//                    //var response = JsonConvert.DeserializeObject<PermitAPIModel>(output);
//                    var response = JsonConvert.DeserializeObject<FacilityAPIModel>(output);
//                    UtilityHelper.LogMessages($"Push Facility to Elps:: {response}");
//                    return response;
//                    //if (response != null && !string.IsNullOrEmpty(response.permit_no))
//                    //{
//                    //    elpsid = response.Id;
//                    //    return true;
//                    //}
//                    //else
//                    //    return false;
//                }
//                catch (Exception x)
//                {
//                    UtilityHelper.LogMessages($"Push Facility to Elps:: {x.ToString()}");
//                    return null;
//                }
//                return null;
//            }
//        }
//        #endregion

//        public ApplicationAPIModel updateApplication(string status, string reference)
//        {
//            ApplicationAPIModel appAPI;

//            using (LocalWebClient client = new LocalWebClient())
//            {
//                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
//                appAPI = new ApplicationAPIModel();
//                appAPI.Status = status;
//                appAPI.OrderId = reference;

//                var param = new JavaScriptSerializer().Serialize(appAPI);
//                var url = ELPSAPIHelper.ApiBaseUrl;
//                url += "Application/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash;

//                var output = client.UploadString(url, "PUT", param);

//                appAPI = JsonConvert.DeserializeObject<ApplicationAPIModel>(output);

//            }
//            return appAPI;

//        }

//        public List<vZone> GetZones()
//        {
//            var client = new WebClient();
//            string output = client.DownloadString(ApiBaseUrl + "Branch/AllZones/" + ApiEmail + "/" + ApiHash);
//            var zn = JsonConvert.DeserializeObject<List<vZone>>(output);

//            return zn;
//        }

//        public List<vZoneState> GetZoneStates(int id)
//        {
//            var client = new WebClient();
//            string output = client.DownloadString(ApiBaseUrl + "Branch/StatesInZone/" + id + "/" + ApiEmail + "/" + ApiHash);
//            var zn = JsonConvert.DeserializeObject<List<vZoneState>>(output);

//            return zn;
//        }

//        public List<vZone> GetZoneMapping()
//        {
//            var client = new WebClient();
//            string output = client.DownloadString(ApiBaseUrl + "Branch/ZoneMapping/" + ApiEmail + "/" + ApiHash);
//            var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);

//            return zonesmapping;
//        }
        
//        public vBranch GetBranch(int id)
//        {
//            var client = new WebClient();
//            string output = client.DownloadString(ApiBaseUrl + "Branch/" + id + "/" + ApiEmail + "/" + ApiHash);
//            var zonesmapping = JsonConvert.DeserializeObject<vBranch>(output);

//            return zonesmapping;
//        }

//        public vZone GetMyZone(vBranch branch)
//        {
//            var _zones = GetZoneMapping();
//            var zones = _zones.Where(a => a.Code.ToLower() != "HQ".ToLower()).ToList();
//            UtilityHelper.LogMessages("Branch ID: " + branch.Id + "; Branch: " + branch.Name);
//            var myZone = zones.Where(a => a.BranchId == branch.Id).FirstOrDefault();

//            if (myZone == null)
//            {
//                //Try inner branches
//                myZone = zones.Where(a => a.CoveredFieldOffices.Select(s => s.StateId).Contains(branch.StateId) || a.CoveredFieldOffices.Select(s => s.StateName).Contains(branch.StateName.ToLower())).FirstOrDefault();
//            }
//            UtilityHelper.LogMessages("Zonal Office = " + myZone == null ? "No Zone found" : myZone.Name);
//            return myZone;
//        }

//        public vZone GetMyZone(int branchId, out vBranch branch, out bool isZopscon)
//        {
//            try
//            {
//                var brch = GetBranch(branchId);
//                var _zones = GetZoneMapping();
//                var zones = _zones.Where(a => a.Code.ToLower() != "HQ".ToLower()).ToList();

//                vZone myZone = zones.Where(a => a.BranchId == branchId).FirstOrDefault();
//                if (myZone == null || string.IsNullOrEmpty(myZone.Name))
//                {
//                    //Try inner branches
//                    myZone = zones.Where(a => a.CoveredFieldOffices.Select(s => s.StateId).Contains(brch.StateId) || a.CoveredFieldOffices.Select(s => s.Id.ToString()).Contains(brch.Id.ToString())).FirstOrDefault();
//                    if (myZone != null && !string.IsNullOrEmpty(myZone.Name))
//                    {
//                        UtilityHelper.LogMessages("Found Zone: " + myZone.Name);
//                    }
//                    isZopscon = false;
//                }
//                else
//                {
//                    UtilityHelper.LogMessages("Out of the LOOP: " + myZone == null || string.IsNullOrEmpty(myZone.Name) ? "Branch not Zone" : "Branch is Zone (" + myZone.Name + ")");
//                    isZopscon = true;
//                }

//                branch = brch;
//                return myZone;
//            }
//            catch (Exception ex)
//            {
//                UtilityHelper.LogMessages("Error Occured: " + ex.Message);
//                isZopscon = false;
//                branch = null;
//                return null;
//            }
//        }


//    }
//}