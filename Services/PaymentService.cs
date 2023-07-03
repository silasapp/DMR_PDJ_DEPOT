using NewDepot.Controllers.Configurations;
using NewDepot.Helpers;
using NewDepot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NewDepot.Controllers;

namespace NewDepot.Services
{
    public class PaymentService : BackgroundService, IDisposable
    {
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        private readonly ILogger<PaymentService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        RestSharpServices _restService = new RestSharpServices();


        public PaymentService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<PaymentService> logger, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogDebug("Payment Service is running in background");

                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var _context = scope.ServiceProvider.GetRequiredService<Depot_DBContext>();

                    _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

                        var getPay = from p in _context.invoices.AsEnumerable()
                                     join a in _context.applications.AsEnumerable() on p.application_id equals a.id
                                     join r in _context.remita_transactions.AsEnumerable() on a.reference equals r.order_id
                                     where a.isLegacy != true && a.status == GeneralClass.PaymentPending && r.RRR != null && !r.RRR.Contains("DPR")
                                     && a.company_id >0 && a.DeleteStatus!=true
                                     select new
                                     {
                                         rrr = r.RRR,
                                         AppId = a.id,
                                         CompanyId = a.company_id,
                                         AppRefNo = a.reference
                                     };

                        if (getPay.Count() > 0)
                        {
                            for (int p = 0; p < getPay.Count(); p++)
                            {
                                var res = _restService.Response("/Payment/checkifpaid?id=r" + getPay.ToList()[p].rrr, null, "GET", null);

                                if (res!= null &&  res.StatusCode != System.Net.HttpStatusCode.InternalServerError && res.IsSuccessful == true)
                                {
                                    var resp = JsonConvert.DeserializeObject<JObject>(res.Content);

                                    if (resp != null && res.Content != null)
                                    {
                                     if ((resp.GetValue("message").ToString().ToLower() == "approved" && resp.GetValue("status").ToString() == "01") || (resp.GetValue("message").ToString().ToLower() == "successful" && resp.GetValue("status").ToString() == "00"))
                                        {
                                            var appid = getPay.ToList()[p].AppId;
                                            var companyid = getPay.ToList()[p].CompanyId;
                                            var refNo = getPay.ToList()[p].AppRefNo;
                                            var rrr = getPay.ToList()[p].rrr;

                                            var getApp = _context.applications.Where(x => x.id == appid);
                                            var getTrans = _context.invoices.Where(x => x.application_id == appid);

                                            getApp.FirstOrDefault().UpdatedAt = DateTime.Now;
                                            getApp.FirstOrDefault().status = GeneralClass.PaymentCompleted;
                                            getTrans.FirstOrDefault().status = "Paid";
                                            getTrans.FirstOrDefault().date_paid = Convert.ToDateTime(resp.GetValue("transactiontime").ToString());

                                            if (_context.SaveChanges() > 0)
                                            {
                                                var company = _context.companies.Where(x => x.id == companyid).FirstOrDefault();

                                                var getApps = _context.applications.Where(x => x.id == appid);
                                                var category = _context.Phases.Where(p => p.id == getApp.FirstOrDefault().PhaseId).FirstOrDefault();

                                                // Saving Messages
                                                string subject = category.name + " Application Payment made with Ref : " + refNo;
                                                string content = "You have made payment for your application (" + category.name  + ") with reference number " + refNo + ". Kindly go to your dashboard to complete application submission on the NMDPRA Depot portal. Find application details below:";
                                                if (company != null)
                                                {
                                                var emailMsg = _helpersController.SaveMessage(appid, (int)company.id, subject, content, company.elps_id.ToString(), "Company");
                                                var sendEmail = _helpersController.SendEmailMessage(company.CompanyEmail, company.name, emailMsg, null);

                                                _helpersController.UpdateElpsApplication(getApps.ToList());
                                                 _helpersController.LogMessages("Application payment made successfully. RRR => " + rrr, company.CompanyEmail);
                                                } 
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    }
                

                _logger.LogDebug("Payment Service has stopped in background");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
