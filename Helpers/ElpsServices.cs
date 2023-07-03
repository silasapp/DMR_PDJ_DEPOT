using LpgLicense.Models;
using NewDepot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NewDepot.Helpers
{
    public class ElpsServices
    {

        public static string _elpsBaseUrl;
        public static string _elpsAppKey;
        public static string _elpsAppEmail;
        public static string appHash;
        public static string public_key;
        public static string conString;

        /*
         * Intializing AppHash for passing.
         */
        public ElpsServices()
        {
            appHash = this.GenerateSHA512(_elpsAppEmail.Trim() + _elpsAppKey.Trim());
        }



        public bool CodeCheck(string email, string code)
        {
            var StringCode = public_key.ToUpper().Trim() + "." + email.ToUpper().Trim() + "." + _elpsAppKey.ToUpper().Trim();

            var hashCode = this.GenerateSHA512(StringCode);

            if (hashCode == code)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public string GenerateSHA512(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();

            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        //#region document section

        //  public List<Company_Document> GetCompanyDocuments(int id, string type = "")
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
        //            id = 11;
        //            Url = ApiBaseUrl + "FacilityDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;
        //        }
        //        UtilityHelper.LogMessages(Url);
        //        var output = client.DownloadString(Url);
        //        compDocs = JsonConvert.DeserializeObject<List<Company_Document>>(output);
        //    }

        //    return compDocs;
        //}

        //public List<files> GetCompanyDocuments(int id)
        //{
        //    var compDocs = new List<files>();
        //    using (LocalWebClient client = new LocalWebClient())
        //    {
        //        client.Headers.Add(HttpRequestHeader.Accept, "application/json");
        //        var Url = ApiBaseUrl + "CompanyDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;

        //        var output = client.DownloadString(Url);
        //        compDocs = JsonConvert.DeserializeObject<List<files>>(output);
        //    }

        //    return compDocs;
        //}
        //public .files GetCompanyDocument(int id, string type = "")
        //{
        //    var compDocs = new .files();
        //    using (LocalWebClient client = new LocalWebClient())
        //    {
        //        client.Headers.Add(HttpRequestHeader.Accept, "application/json");
        //        var Url = "";
        //        if (!string.IsNullOrEmpty(type) && type.ToLower() == "facility")
        //        {
        //            Url = ApiBaseUrl + "FacilityDocuments/" + id + "/" + ApiEmail + "/" + ApiHash;
        //        }
        //        else
        //        {
        //            Url = ApiBaseUrl + "CompanyDocument/" + id + "/" + ApiEmail + "/" + ApiHash;
        //        }
        //        try
        //        {
        //            var output = client.DownloadString(Url);
        //            compDocs = JsonConvert.DeserializeObject<files>(output);

        //        }
        //        catch (Exception)
        //        {


        //        }
        //    }

        //    return compDocs;
        //}


        ////public List<Document_Type> GetDocumentTypes()
        ////{
        ////    var compDocs = new List<Document_Type>();
        ////    using (LocalWebClient client = new LocalWebClient())
        ////    {
        ////        client.Headers.Add(HttpRequestHeader.Accept, "application/json");
        ////        var Url = ApiBaseUrl + "Documents/Types/" + ApiEmail + "/" + ApiHash;

        ////        var output = client.DownloadString(Url);
        ////        compDocs = JsonConvert.DeserializeObject<List<Document_Type>>(output);
        ////    }

        ////    return compDocs;
        ////}

        //public List<Document_Type> GetDocumentTypes(string type = "")
        //{
        //    var compDocs = new List<Document_Type>();
        //    using (LocalWebClient client = new LocalWebClient())
        //    {
        //        client.Headers.Add(HttpRequestHeader.Accept, "application/json");
        //        string Url = "";
        //        if (string.IsNullOrEmpty(type))
        //        {
        //            Url = ApiBaseUrl + "Documents/Types/" + ApiEmail + "/" + ApiHash;
        //        }
        //        else
        //        {
        //            Url = ApiBaseUrl + "Documents/Facility/" + ApiEmail + "/" + ApiHash + "/" + type;
        //        }

        //        var output = client.DownloadString(Url);
        //        compDocs = JsonConvert.DeserializeObject<List<Document_Type>>(output);
        //    }

        //    return compDocs;
        //}

        //#endregion

        //#region Permit
        //public bool PushPermit(PermitAPIModel model, out int elpsid)
        //{
        //    elpsid = 0;
        //    using (LocalWebClient client = new LocalWebClient())
        //    {
        //        try
        //        {
        //            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        //            var Url = ApiBaseUrl + "Permits/" + model.Company_Id + "/" + ApiEmail + "/" + ApiHash;
        //            var mod = new JavaScriptSerializer().Serialize(model);

        //            var output = client.UploadString(Url, "POST", mod);
        //            var response = JsonConvert.DeserializeObject<PermitAPIModel>(output);

        //            if (response != null && !string.IsNullOrEmpty(response.permit_no))
        //            {
        //                elpsid = response.Id;
        //                return true;
        //            }
        //            else
        //                return false;
        //        }
        //        catch (Exception)
        //        {
        //            return false;
        //        }
        //    }
        //}

        //public FacilityAPIModel PushFacility(FacilityAPIModel model)
        //{
        //    //elpsid = 0;
        //    using (LocalWebClient client = new LocalWebClient())
        //    {
        //        try
        //        {
        //            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

        //            var param = JsonConvert.SerializeObject(model);
        //            var url = ApiBaseUrl;
        //            url += "Facility/Add/" +ApiEmail + "/" + ApiHash;
        //            //var Url = ApiBaseUrl + "Permits/" + model.Company_Id + "/" + ApiEmail + "/" + ApiHash;
        //            //var mod = new JavaScriptSerializer().Serialize(model);
        //            UtilityHelper.LogMessages($"Push Facility to Elps:: {param} :: {url}");
        //            var output = client.UploadString(url, "POST", param);

        //            //var response = JsonConvert.DeserializeObject<PermitAPIModel>(output);
        //            var response = JsonConvert.DeserializeObject<FacilityAPIModel>(output);
        //            UtilityHelper.LogMessages($"Push Facility to Elps:: {response}");
        //            return response;
        //            //if (response != null && !string.IsNullOrEmpty(response.permit_no))
        //            //{
        //            //    elpsid = response.Id;
        //            //    return true;
        //            //}
        //            //else
        //            //    return false;
        //        }
        //        catch (Exception x)
        //        {
        //            UtilityHelper.LogMessages($"Push Facility to Elps:: {x.ToString()}");
        //            return null;
        //        }
        //        return null;
        //    }
        //}
        //#endregion

        //public ApplicationAPIModel updateApplication(string status, string reference)
        //{
        //    ApplicationAPIModel appAPI;

        //    using (LocalWebClient client = new LocalWebClient())
        //    {
        //        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        //        appAPI = new ApplicationAPIModel();
        //        appAPI.Status = status;
        //        appAPI.OrderId = reference;

        //        var param = new JavaScriptSerializer().Serialize(appAPI);
        //        var url = ELPSAPIHelper.ApiBaseUrl;
        //        url += "Application/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash;

        //        var output = client.UploadString(url, "PUT", param);

        //        appAPI = JsonConvert.DeserializeObject<ApplicationAPIModel>(output);

        //    }
        //    return appAPI;

        //}

        //public List<vZone> GetZones()
        //{
        //    var client = new WebClient();
        //    string output = client.DownloadString(ApiBaseUrl + "Branch/AllZones/" + ApiEmail + "/" + ApiHash);
        //    var zn = JsonConvert.DeserializeObject<List<vZone>>(output);

        //    return zn;
        //}

        //public List<vZoneState> GetZoneStates(int id)
        //{
        //    var client = new WebClient();
        //    string output = client.DownloadString(ApiBaseUrl + "Branch/StatesInZone/" + id + "/" + ApiEmail + "/" + ApiHash);
        //    var zn = JsonConvert.DeserializeObject<List<vZoneState>>(output);

        //    return zn;
        //}

        //public List<vZone> GetZoneMapping()
        //{
        //    var client = new WebClient();
        //    string output = client.DownloadString(ApiBaseUrl + "Branch/ZoneMapping/" + ApiEmail + "/" + ApiHash);
        //    var zonesmapping = JsonConvert.DeserializeObject<List<vZone>>(output);

        //    return zonesmapping;
        //}

        //public vBranch GetBranch(int id)
        //{
        //    var client = new WebClient();
        //    string output = client.DownloadString(ApiBaseUrl + "Branch/" + id + "/" + ApiEmail + "/" + ApiHash);
        //    var zonesmapping = JsonConvert.DeserializeObject<vBranch>(output);

        //    return zonesmapping;
        //}

        //public vZone GetMyZone(vBranch branch)
        //{
        //    var _zones = GetZoneMapping();
        //    var zones = _zones.Where(a => a.Code.ToLower() != "HQ".ToLower()).ToList();
        //    UtilityHelper.LogMessages("Branch ID: " + branch.Id + "; Branch: " + branch.Name);
        //    var myZone = zones.Where(a => a.BranchId == branch.Id).FirstOrDefault();

        //    if (myZone == null)
        //    {
        //        //Try inner branches
        //        myZone = zones.Where(a => a.CoveredFieldOffices.Select(s => s.StateId).Contains(branch.StateId) || a.CoveredFieldOffices.Select(s => s.StateName).Contains(branch.StateName.ToLower())).FirstOrDefault();
        //    }
        //    UtilityHelper.LogMessages("Zonal Office = " + myZone == null ? "No Zone found" : myZone.Name);
        //    return myZone;
        //}

        //public vZone GetMyZone(int branchId, out vBranch branch, out bool isZopscon)
        //{
        //    try
        //    {
        //        var brch = GetBranch(branchId);
        //        var _zones = GetZoneMapping();
        //        var zones = _zones.Where(a => a.Code.ToLower() != "HQ".ToLower()).ToList();

        //        vZone myZone = zones.Where(a => a.BranchId == branchId).FirstOrDefault();
        //        if (myZone == null || string.IsNullOrEmpty(myZone.Name))
        //        {
        //            //Try inner branches
        //            myZone = zones.Where(a => a.CoveredFieldOffices.Select(s => s.StateId).Contains(brch.StateId) || a.CoveredFieldOffices.Select(s => s.Id.ToString()).Contains(brch.Id.ToString())).FirstOrDefault();
        //            if (myZone != null && !string.IsNullOrEmpty(myZone.Name))
        //            {
        //                UtilityHelper.LogMessages("Found Zone: " + myZone.Name);
        //            }
        //            isZopscon = false;
        //        }
        //        else
        //        {
        //            UtilityHelper.LogMessages("Out of the LOOP: " + myZone == null || string.IsNullOrEmpty(myZone.Name) ? "Branch not Zone" : "Branch is Zone (" + myZone.Name + ")");
        //            isZopscon = true;
        //        }

        //        branch = brch;
        //        return myZone;
        //    }
        //    catch (Exception ex)
        //    {
        //        UtilityHelper.LogMessages("Error Occured: " + ex.Message);
        //        isZopscon = false;
        //        branch = null;
        //        return null;
        //    }
        //}



      

    }
}
