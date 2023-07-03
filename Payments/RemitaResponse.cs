using Microsoft.Extensions.Configuration;
using NewDepot.Helpers;
using NewDepot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace NewDepot.Payments
{
    
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
            IConfiguration _configuration = (IConfiguration)new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json")
                           .AddEnvironmentVariables().Build(); 

            try
            {

                string hash_string = rrr.Trim() + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                string hash = PaymentRef.getHash(hash_string);
                string elpsurl = _configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString();

                var url = elpsurl+"/Payment/checkifpaid?id=r{rrr}";
                //https://elps.dpr.gov.ng/payment/checkifpaid?id=r330247144129
                NewRemitaResponse response = null;

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    string responseJson = client.DownloadString(url);
                    response = JsonConvert.DeserializeObject<NewRemitaResponse>(responseJson);

                    if (response != null && (response.status == "01" || response.status == "00"))
                    {
                        return response;

                    }

                }
                return response;

            }
            catch (Exception x)
            {
                
                //_.LogMessages(x.ToString());
                return null;
            }
        }
    }

    public static class RemitaSplitParams
    {

        static IConfiguration _configuration = (IConfiguration)new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json")
                       .AddEnvironmentVariables().Build();

        public static string MERCHANTID = _configuration.GetSection("AmountSetting").GetSection("merchantID").Value.ToString();
        public static string SERVICETYPE_GEN = _configuration.GetSection("AmountSetting").GetSection("servTyp_Gen").Value.ToString();
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