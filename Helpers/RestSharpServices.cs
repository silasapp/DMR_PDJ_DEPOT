using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewDepot.Helpers
{
    public class RestSharpServices
    {

        public static RestClient _client;
        public RestRequest _request;
        public Method _method;
        public string _url;
        ElpsServices elpsServices = new ElpsServices();



        public RestSharpServices()
        {
            RestClient restClient = new RestClient(ElpsServices._elpsBaseUrl);
            restClient.Timeout = (60 * 1000);
            _client = restClient;
        }



        private Method restMethod(string methodType = null)
        {
            var method = methodType == "PUT" ? Method.PUT : methodType == "POST" ? Method.POST : methodType == "DELETE" ? Method.DELETE : Method.GET;
            return method;
        }


        private RestRequest ServiceRequest(string apiURL, string method = null)
        {
            _method = restMethod(method);
            var request = new RestRequest(apiURL, _method);
            return request;
        }


        private RestRequest AddParameters(string apiURL, string method = null, List<ParameterData> paramData = null)
        {
            var _request = ServiceRequest(apiURL, method);
            _request.AddUrlSegment("email", ElpsServices._elpsAppEmail);
            _request.AddUrlSegment("apiHash", ElpsServices.appHash);

            if (paramData != null)
            {
                foreach (var _paramData in paramData)
                {
                    _request.AddUrlSegment(_paramData.ParamKey, _paramData.ParamValue);
                }
            }


            return _request;
        }


        // For all request PUT, POST, GET 
        public IRestResponse Response(string apiURL, List<ParameterData> paramData = null, string method = null, object json = null, string ext = null)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (apiURL.Contains("roms.dpr.gov.ng") || apiURL.Contains("localhost:32000"))
            {
                _request = AddParameters(apiURL, method, paramData);
            }
            else
            {
                if (ext != null)
                {
                    _request = AddParameters(ElpsServices._elpsBaseUrl + apiURL + ext, method, paramData);
                }
                else
                {
                    _request = AddParameters(ElpsServices._elpsBaseUrl + apiURL, method, paramData);
                }
            }
            _request.RequestFormat = DataFormat.Json;

            if (json != null)
            {
                _request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(json), ParameterType.RequestBody);
            }
            IRestResponse restResponse = _client.Execute(_request);

            return restResponse;
        }

        
        public IRestResponse Response3(string apiURL, List<ParameterData> paramData = null, string method = null, IFormFile json = null, string ext = null, string filepath = null)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (ext != null)
            {
                // _request = AddParameters(ElpsServices._elpsBaseUrl + apiURL, method, paramData, json);
                _request = AddParameters(ElpsServices._elpsBaseUrl + apiURL + ext, method, paramData);
            }
            else
            {
                _request = AddParameters(ElpsServices._elpsBaseUrl + apiURL, method, paramData);
            }
            _request.RequestFormat = DataFormat.Json;
            if (json != null)
            {
                //    var stream = new MemoryStream(json);
                //    IFormFile file = new FormFile(stream, 0, json.Length, "name", "fileName");
                //    byte[] imageBytes;
                //    using (FileStream fs = File.Open(filepath, FileMode.Open))
                //    {
                //        imageBytes = new BinaryReader(fs).ReadBytes((int)fs.Length);
                //    }
                //    string image = Convert.ToBase64String(imageBytes);
                //dict.Add("IMAGE_DATA", image);
                _request.AddHeader("Authorization", "Authorization");
                _request.AddHeader("Content-Type", "multipart/form-data");
                _request.AddFile("Minutes", filepath);
                //_request.AddFile("UnitizationLetter", filepath, "application/json; charset=utf-8");

                //_request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(stream), ParameterType.RequestBody);
            }
            //if (json != null)
            //{
            //    /* _request.AddBody(json);*/
            //    _request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(json), ParameterType.RequestBody);

            //}
            IRestResponse restResponse = _client.Execute(_request);

            return restResponse;
        }


        // For all request PUT, POST, GET 
        public IRestResponse Response2(string apiURL, List<ParameterData> paramData = null, string method = null, byte[] json = null, string fp = null, string type = null)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _request = AddParameters2(ElpsServices._elpsBaseUrl + apiURL, method);

            _request.RequestFormat = DataFormat.Json;

            if (json != null)
            {
                var stream = new MemoryStream(json);
                IFormFile file = new FormFile(stream, 0, json.Length, "name", "fileName");
                byte[] imageBytes;
                using (FileStream fs = File.Open(fp, FileMode.Open))
                {
                    imageBytes = new BinaryReader(fs).ReadBytes((int)fs.Length);
                }
                string image = Convert.ToBase64String(imageBytes);
                //dict.Add("IMAGE_DATA", image);
                _request.AddHeader("Authorization", "Authorization");
                _request.AddHeader("Content-Type", "multipart/form-data");
                string fileNM = "";
                _request.AddFile(fileNM, fp);
            }
            IRestResponse restResponse = _client.Execute(_request);

            return restResponse;
        }

        private RestRequest AddParameters2(string apiURL, string method = null)
        {
            var _request = ServiceRequest(apiURL, method);

            return _request;
        }

        public string ErrorResponse(IRestResponse restResponse)
        {
            if (restResponse.ResponseStatus.ToString() == "Error")
            {
                return "A network related error has occured. Please try again";

                //return "A network related error has occured. Message : " + restResponse.ErrorException.Source.ToString() + " - " + restResponse.ErrorException.InnerException.Message.ToString() + " --- Error Code : " + restResponse.ErrorException.HResult;
            }
            else
            {
                return "A network related error has occured. Message : " + restResponse.ErrorException.Source.ToString() + " - " + restResponse.ErrorException.InnerException.Message.ToString() + " --- Error Code : " + restResponse.ErrorException.HResult;

            }
        }



        public List<ParameterData> parameterData(string key, string value)
        {
            var paramData = new List<ParameterData>();

            paramData.Add(new ParameterData
            {
                ParamKey = key,
                ParamValue = value
            });

            return paramData;
        }

    }
}
