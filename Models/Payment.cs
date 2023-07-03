using LpgLicense.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewDepot.Controllers;
using NewDepot.Helpers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewDepot.Models
{
    public class Payment {

        public static RestSharpServices _restService = new RestSharpServices();
        public static IConfiguration _configuration;
        public static IHttpContextAccessor _httpContextAccessor;
        public readonly Depot_DBContext _context;

        Helpers.Authentications auth = new Helpers.Authentications();
        public static GeneralClass generalClass = new GeneralClass();
        public static HelpersController _helpersController;
       
        public Payment(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }
        public class RemitaResponse
        {
            public string statusmessage { get; set; }
            public string merchantId { get; set; }
            public string status { get; set; }
            public string RRR { get; set; }
            public string Amount { get; set; }
            public string transactiontime { get; set; }
            public string orderId { get; set; }


        }

        public class PrePaymentResponse
        {
            public string StatusMessage { get; set; }
            public string AppId { get; set; }
            public string Status { get; set; }
            public string RRR { get; set; }
            public string Transactiontime { get; set; }
            public string TransactionId { get; set; }
        }

        public class NewRemitaResponse
        {
            public string orderId { get; set; }
            public string RRR { get; set; }
            public string status { get; set; }
            public string message { get; set; }
            public string transactiontime { get; set; }
            public string amount { get; set; }
          
            public static NewRemitaResponse CheckRRRPayment(string rrr)
            {
               try
            {

                    

                   string MERCHANTID = _configuration.GetSection("AmountSetting").GetSection("merchantID").Value.ToString();
                   string APIKEY = _configuration.GetSection("AmountSetting").GetSection("rKey").Value.ToString();
                    string hash_string = rrr.Trim() + APIKEY + MERCHANTID;
                    string hash = generalClass.Encrypt(hash_string);

                    var paramDatas = _restService.parameterData("id", rrr);
                    var responsee = _restService.Response("/Payment/checkifpaid/{id}/{email}/{apiHash}", paramDatas, "POST");

                
              NewRemitaResponse response = JsonConvert.DeserializeObject<NewRemitaResponse>(responsee.ToString());

                        if (response != null && ((response.message.ToString().ToLower() == "approved" && response.status.ToString() == "00")||(response.message.ToString().ToLower() == "successful" && response.status.ToString() == "00")))

                        {
                            return response;

                        }
                    return response;

                }

                catch (Exception x)
                {
          _helpersController.LogMessages(x.ToString());
                    return null;
                }
            }
        }

        public static class RemitaSplitParams
        {
            public static string SERVICETYPE_GEN = _configuration.GetSection("AmountSetting").GetSection("servTyp_Gen").Value.ToString();
            public static string MERCHANTID = _configuration.GetSection("AmountSetting").GetSection("merchantID").Value.ToString();
            public static string SERVICETYPE_MAJ = _configuration.GetSection("AmountSetting").GetSection("servTyp_Maj").Value.ToString();
            public static string SERVICETYPE_SPE = _configuration.GetSection("AmountSetting").GetSection("servTyp_Spe").Value.ToString();
            public static string APIKEY = _configuration.GetSection("AmountSetting").GetSection("rKey").Value.ToString();
            public static string GATEWAYURL = _configuration.GetSection("AmountSetting").GetSection("RemitaSplitUrl").Value.ToString();
            public static string CHECKSTATUS_ORDERID = _configuration.GetSection("AmountSetting").GetSection("RemitaStatus_OrderID").Value.ToString();
            public static string CHECKSTATUS_RRR = _configuration.GetSection("AmountSetting").GetSection("RemitaStatus_RRR").Value.ToString();
            public static string RESPONSEURL = _configuration.GetSection("AmountSetting").GetSection("RemitaPaymentCallback").Value.ToString();
            public static string RRRGATEWAYPAYMENTURL = _configuration.GetSection("AmountSetting").GetSection("RemitaRRRGateway").Value.ToString();

        }

    }
}

