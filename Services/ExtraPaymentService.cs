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
    public class ExtraPaymentService : BackgroundService, IDisposable
    {
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        private readonly ILogger<ExtraPaymentService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        RestSharpServices _restService = new RestSharpServices();


        public ExtraPaymentService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<ExtraPaymentService> logger, IServiceScopeFactory scopeFactory)
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
                _logger.LogDebug("Extra payment service is running in background");
                using var scope = _scopeFactory.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<Depot_DBContext>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

                    var getPay = from p in _context.ApplicationExtraPayments.AsEnumerable()
                                 join a in _context.applications.AsEnumerable() on p.ApplicationId equals a.id
                                 where a.isLegacy != true && p.Status == GeneralClass.PaymentPending && p.RRR != null && !p.RRR.Contains("DPR")
                                 select new
                                 {
                                     rrr = p.RRR,
                                     AppId = p.ApplicationId,
                                     CompanyId = a.company_id,
                                     AppRefNo = a.reference
                                 };

                    if (getPay.Count() > 0)
                    {
                        for (int p = 0; p < getPay.ToList().Count(); p++)
                        {
                            var res = _restService.Response("/Payment/checkifpaid?id=r" + getPay.ToList()[p].rrr, null, "GET", null);

                            if (res != null && res.StatusCode != System.Net.HttpStatusCode.InternalServerError && res.IsSuccessful == true)
                            {
                                var resp = JsonConvert.DeserializeObject<JObject>(res.Content.ToString());

                                if (resp != null && res.Content != null)
                                {
                                    if ((resp.GetValue("message").ToString().ToLower() == "approved" && resp.GetValue("status").ToString() == "01") || (resp.GetValue("message").ToString().ToLower() == "successful" && resp.GetValue("status").ToString() == "00"))
                                    {
                                        var appid = getPay.ToList()[p].AppId;
                                        var companyid = getPay.ToList()[p].CompanyId;
                                        var refNo = getPay.ToList()[p].AppRefNo;
                                        var rrr = getPay.ToList()[p].rrr;

                                        var getApp = _context.applications.Where(x => x.id == appid);
                                        var getTrans = _context.ApplicationExtraPayments.Where(x => x.ApplicationId == appid && x.RRR == rrr);

                                        getApp.FirstOrDefault().UpdatedAt = DateTime.Now;

                                        getTrans.FirstOrDefault().Status = GeneralClass.PaymentCompleted;
                                        //getTrans.FirstOrDefault().PayTypeStatus = "Service";
                                        getTrans.FirstOrDefault().DatePaid = resp.GetValue("transactiontime").ToString();

                                        if (_context.SaveChanges() > 0)
                                        {

                                            var company = (from a in _context.applications
                                                           join c in _context.companies on a.company_id equals c.id
                                                           where a.DeleteStatus == false && c.DeleteStatus == false && a.id == appid
                                                           select new
                                                           {
                                                               c,
                                                               a
                                                           }).FirstOrDefault();

                                            var category = _context.Phases.Where(p => p.id == company.a.PhaseId).FirstOrDefault();

                                            // Saving Messages
                                            string subject = "Application Extra Payment Success : " + company.a.reference;
                                            string content = "Extra payment made successfully for your application (" + category.name + ") with reference number " + company.a.reference + " and payment RRR " + rrr + " for processing on NMDPRA DEPOT portal. Kindly find application details below.";
                                            var staff = _context.Staff.Where(x => x.StaffEmail.ToLower() == getTrans.FirstOrDefault().UserName.ToLower()).FirstOrDefault();

                                            if (staff != null)
                                            {

                                                _helpersController.SaveHistory(appid, staff.StaffID, "Payment", "Extra payment for application paid successfully with Reference RRR : " + rrr);
                                            }
                                            var emailMsg = _helpersController.SaveMessage(company.a.id, (int)company.c.id, subject, content, company.c.elps_id.ToString(), "Company");
                                            var sendEmail = _helpersController.SendEmailMessage(company.c.CompanyEmail, company.c.name, emailMsg, null);

                                            _helpersController.LogMessages("Extra payment for application paid successfully with reference RRR : " + rrr, company.c.CompanyEmail);

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
