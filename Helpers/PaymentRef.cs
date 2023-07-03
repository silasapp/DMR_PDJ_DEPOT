using System;
using System.Collections.Generic;
using NewDepot.Models;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using NewDepot.Controllers;
using Microsoft.AspNetCore.Http;
using LpgLicense.Models;
using Application = NewDepot.Models.applications;
using System.Security.Cryptography;
using RPartner = NewDepot.Models.RPartner;
using Microsoft.AspNetCore.Authorization;

namespace NewDepot.Helpers
{
    [Authorize]
    public static class PaymentRef 
    {

        static IConfiguration _configuration = (IConfiguration)new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory()) // requires Microsoft.Extensions.Configuration.Json
                           .AddJsonFile("appsettings.json") // requires Microsoft.Extensions.Configuration.Json
                           .AddEnvironmentVariables().Build(); // requires Microsoft.Extensions.Configuration.EnvironmentVariables
            static string CatPhase_Suit = _configuration.GetSection("AmountSetting").GetSection("CatPhase_Suit").Value.ToString();
            static string CatPhase_ATC = _configuration.GetSection("AmountSetting").GetSection("CatPhase_ATC").Value.ToString();
            static string CatPhase_LTO = _configuration.GetSection("AmountSetting").GetSection("CatPhase_LTO").Value.ToString();
            static string CatPhase_TankTest = _configuration.GetSection("AmountSetting").GetSection("CatPhase_TankTest").Value.ToString();
            static string CatPhase_To = _configuration.GetSection("AmountSetting").GetSection("CatPhase_To").Value.ToString();
            static string CatPhase_CatA = _configuration.GetSection("AmountSetting").GetSection("CatPhase_CatA").Value.ToString();
            static string CatPhase_CatB = _configuration.GetSection("AmountSetting").GetSection("CatPhase_CatB").Value.ToString();
        
            public static string RefrenceCode()
        {
           //generate 12 digit numbers
            var bytes = new byte[8];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            ulong random = BitConverter.ToUInt64(bytes, 0) % 1000000000000;
            return  String.Format("{0:D12}", random);
        }

        public static string getHash(string hashItem)
        {
            string hash = "";



            var data = Encoding.UTF8.GetBytes(hashItem);
            byte[] x;
            using (SHA512 shaM = new SHA512Managed())
            {
                 x = shaM.ComputeHash(data);
                
            }
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in x)
                stringBuilder.AppendFormat("{0:X2}", b);

            hash = stringBuilder.ToString();
            

            return hash;
        }

        public static List<RPartner> BuildPartners(Application application, RemitaSplit rmSplit,decimal amount=0)
        {
                  IConfiguration _configuration = (IConfiguration)new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory()) // requires Microsoft.Extensions.Configuration.Json
                   .AddJsonFile("appsettings.json") // requires Microsoft.Extensions.Configuration.Json
                   .AddEnvironmentVariables().Build(); // requires Microsoft.Extensions.Configuration.EnvironmentVariables


            #region build partners share

            double amountToShare = 0;
            //int SmsCharge = 0;
            double rmAmt = 0;   //RM
            double fgAmt = 0;   //FG
            double dprAmt = 0;  //NMDPRA
            double bmAmt = 0;   //Brandone
            double rm1 = 0;
            double rm2 = 0;

            if (application.PhaseId == 1)
            {
                rm1 = 46.25;
                rmSplit.serviceTypeId = CatPhase_Suit;
                amountToShare = Convert.ToDouble(application.service_charge) - rm1;
                fgAmt = Convert.ToDouble(application.fee_payable - (application.fee_payable * 0.01m)); // 1% of Statutory
                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;
                bmAmt = amountToShare * 0.9;
                rmAmt = rm1 + rm2;
                dprAmt = (amountToShare * 0.1) + (rmAmt);
            }
            else if (application.PhaseId == 2)
            {
                rmSplit.serviceTypeId = CatPhase_ATC;

                rm1 = 157.50;
                amountToShare = Convert.ToDouble(application.service_charge) - rm1;// ap.service_charge - rm1;
                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory
                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;
                bmAmt = amountToShare * 0.9;    // 90% of Service Charge
                rmAmt = rm1 + rm2;
                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge

                //amountToShare = (app.service_charge - 265 - SmsCharge);
            }
            else if (application.PhaseId == 3)
            {
                rmSplit.serviceTypeId = CatPhase_TankTest;

                rm1 = 350.00;
                amountToShare = Convert.ToDouble(application.service_charge) - rm1;
                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory
                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;
                bmAmt = amountToShare * 0.9;    // 90% of Service Charge
                rmAmt = rm1 + rm2;
                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge
                                                          //amountToShare = (app.service_charge - 550 - SmsCharge);
            }
            else if (application.PhaseId == 4)
            {
                rmSplit.serviceTypeId = CatPhase_LTO;

                rm1 = 157.50;
                amountToShare = Convert.ToDouble(application.service_charge) - rm1;
                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory
                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;
                bmAmt = amountToShare * 0.9;    // 90% of Service Charge
                rmAmt = rm1 + rm2;
                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge

                //amountToShare = (app.service_charge - 265 - SmsCharge);
            }
            else if (application.PhaseId == 5)
            {
                rmSplit.serviceTypeId = CatPhase_LTO;

                rm1 = 350.00;
                amountToShare = Convert.ToDouble(application.service_charge) - rm1;
                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory
                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;
                bmAmt = amountToShare * 0.9;    // 90% of Service Charge
                rmAmt = rm1 + rm2;
                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge

            }
            else if (application.PhaseId == 6)
            {
                rmSplit.serviceTypeId = CatPhase_LTO;

                rm1 = 350.00;
                amountToShare = Convert.ToDouble(application.service_charge) - rm1;
                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory
                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;
                bmAmt = amountToShare * 0.9;    // 90% of Service Charge
                rmAmt = rm1 + rm2;
                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge

            }
            else if (application.PhaseId == 7)
            {
                rmSplit.serviceTypeId = CatPhase_LTO;

                rm1 = 0;
                amountToShare = Convert.ToDouble(application.service_charge) - rm1;
                fgAmt = Convert.ToDouble(application.fee_payable) - (Convert.ToDouble(application.fee_payable) * 0.01); // 1% of Statutory
                rm2 = Convert.ToDouble(application.fee_payable) - fgAmt;
                bmAmt = amountToShare * 0.9;    // 90% of Service Charge
                rmAmt = rm1 + rm2;
                dprAmt = (amountToShare * 0.1) + (rmAmt); // 10% of Service Charge + Remita charge

            }


            rmSplit.serviceTypeId = CatPhase_LTO;

            //rmAmt = app.service_charge - amountToShare;
            // For Remita
            var rp = new List<RPartner>();
            

            var fp = application.fee_payable.ToString().Split('.');
            rp.Add(new RPartner
            { 
                lineItemsId = "1",
                beneficiaryName = _configuration.GetSection("AmountSetting").GetSection("AccName_1").Value.ToString(),
                bankCode = _configuration.GetSection("AmountSetting").GetSection("bankCode").Value.ToString(),
                beneficiaryAccount = _configuration.GetSection("AmountSetting").GetSection("Acc_1").Value.ToString(),
                beneficiaryAmount = amount>0?amount.ToString(): (application.fee_payable + application.service_charge).ToString(),// (fgAmt + dprAmt + bmAmt).ToString(),
                deductFeeFrom = _configuration.GetSection("AmountSetting").GetSection("AccDeduct_1").Value.ToString()
        });
            #endregion

            return rp;
        }
    }

    public class ApplicationStatus
    {
        public static readonly string PaymentPending = "Payment Pending";
        public static readonly string PaymentCompleted = "Payment Completed";
        public static readonly string Processing = "Processing";
        public static readonly string Rejected = "Rejected";
        public static readonly string Approved = "Approved";
    }

    
}