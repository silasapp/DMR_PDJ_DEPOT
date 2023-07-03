using NewDepot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewDepot.Helpers
{
    public class HttpClientServices
    {

        public static string _baseUrl = "https://roms.dpr.gov.ng/GetFacility?id=";
        public static string _email = "admin@ags.dpr.gov.ng";
        public static string _sk = "9c16e2e2-ce4f-402e-92d7-2558dc1c6d22";
        

        private readonly HttpClient _httpClient;

        public ElpsServices elpsServices = new ElpsServices();

        public HttpClientServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }



        public string GenerateCode()
        {
            var code = elpsServices.GenerateSHA512(_email + "" + _sk);
            return code;
        }



        public string URL(string LicenseNumber, string full)
        {
            string code = GenerateCode();
            var url = _baseUrl + LicenseNumber + "&full=" + full + "&email=" + _email + "&code=" + code;
            return url;
        }



        private async Task<JObject> GetResults(string LicenceNO, string full)
        {
            string url = URL(LicenceNO, full);

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode == true)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<JObject>(responseBody.ToString());
                return responseObject;
            }
            else
            {
                return null;
            }

        }




        public async Task<bool> GetSuccess(string LicenseNO, string full)
        {
            var successObject = await GetResults(LicenseNO, full);
            bool result = false;

            if (successObject == null)
            {
                result = false;
            }
            else
            {
                var success = successObject.SelectToken("LicenseNumber").ToString();

                if (success == LicenseNO)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }




        //public async Task<Item> GetFacilityItems(string LicenseNO, string full)
        //{
        //    var itemObject = await GetResults(LicenseNO, full);

        //    Item item = null;

        //    if (itemObject == null)
        //    {
        //        item = null;
        //    }
        //    else
        //    {
        //        var _address = itemObject.SelectToken("Address");
        //        var _company = itemObject.SelectToken("Company");
        //        var _facility = itemObject.SelectToken("FacilityName");
        //        var _facilityId = itemObject.SelectToken("UniqueIdOnELPS").ToString().Split("/");

        //        item = new Item
        //        {
        //            FacilityId = _facilityId[3].ToString(),
        //            Address = _address.SelectToken("StreetAddress").ToString(),
        //            City = _address.SelectToken("City").ToString(),
        //            Lga = _address.SelectToken("LGA").ToString(),
        //            State = _address.SelectToken("State").ToString(),
        //            FacilityName =_facility.ToString(),
        //            ContactName = _company.SelectToken("ContactName").ToString(),
        //            Phone = _company.SelectToken("Phone").ToString()

        //        };
        //    }
        //    return item;
        //}

        
    }
}
