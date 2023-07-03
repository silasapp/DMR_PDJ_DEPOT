using NewDepot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewDepot.Helpers
{
    public class MistdoServices
    {
        
        public static string _baseUrl = "";
        private readonly HttpClient _httpClient;
        public ElpsServices elpsServices = new ElpsServices();



        public MistdoServices(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }


        private string URL(string prefix_and_others)
        {
            string url = "";
            url = _baseUrl + prefix_and_others;
            return url;
        }


        private async Task<JObject> GetResults(string prefix_and_others)
        {
            string url = URL(prefix_and_others);


            var client = new HttpClient();

            //HttpResponseMessage response = await _httpClient.GetAsync(url);
            // if (response.IsSuccessStatusCode == true)

            client.BaseAddress = new Uri(_baseUrl);
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
             
                var responseBody = await response.Content.ReadAsStringAsync();
                var resultObject = JsonConvert.DeserializeObject<JObject>(responseBody.ToString());
                return resultObject;
            }
            else
            {
                return null;
            }
        }

      
        public async Task<MistdoModel> VerifyStaff(string prefix_and_others)
        {
            string result = "";

            var itemObject = await GetResults(prefix_and_others);

            data fac = new data();

            MistdoModel mm = new MistdoModel();

            if (itemObject == null)
            {
                mm = null;
            }
            else
            {
                var success = Convert.ToBoolean(itemObject.SelectToken("success"));

                if (success == true)
                {
                    var msg = itemObject.SelectToken("message").ToString().Trim();

                    if (msg == "Certificate Valid")
                    {
                        var d = itemObject.SelectToken("data");


                        fac = new data
                        {
                            fullname = d?.SelectToken("fullName").ToString().Trim(),
                            phoneNumber = d?.SelectToken("phoneNumber").ToString(),
                            email = d?.SelectToken("email").ToString(),
                            certificateNo = d?.SelectToken("certificateNo").ToString(),
                            mistdoId = d?.SelectToken("mistdoId").ToString(),
                            certificateIssue = DateTime.ParseExact(d?.SelectToken("certificateIssue").ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date,
                            certificateExpiry = DateTime.ParseExact(d?.SelectToken("certificateExpiry").ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date,
                        };
                        result = "Found";

                    }
                    else if (msg == "Certificate Expired")
                    {
                        result = "This certificate is expired, try a different one.";
                    }
                    else
                    {
                        result = msg;
                    }
                }
                else
                {
                    result = itemObject.SelectToken("message").ToString().Trim();
                }

                mm = new MistdoModel()
                {
                    success = success,
                    message = result,
                    datas = fac
                };
            }

            return mm;
        }
  



    }
}

//var client = new HttpClient();

//client.BaseAddress = new Uri(mistdoBase);
//var response = client.GetAsync($"/home/verifymistdocertificate?certificateid={cert}").Result;
//if (response.StatusCode == HttpStatusCode.OK)
//{
//    var content = response.Content.ReadAsStringAsync().Result;
//    return JsonConvert.DeserializeObject<MISTDOResponse>(content);
//}