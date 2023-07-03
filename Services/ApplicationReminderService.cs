//using LNG.Controllers.Configurations;
//using LNG.Helpers;
//using LNG.Models.DB;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace LNG.Models
//{
//    public class ApplicationReminderService : BackgroundService, IDisposable
//    {
//        GeneralClass generalClass = new GeneralClass();
//        IHttpContextAccessor _httpContextAccessor;
//        public IConfiguration _configuration;
//        HelpersController _helpersController;
//        private readonly ILogger<ApplicationReminderService> _logger;
//        private readonly IServiceScopeFactory _scopeFactory;

//        public ApplicationReminderService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<ApplicationReminderService> logger, IServiceScopeFactory scopeFactory)
//        {
//            _configuration = configuration;
//            _httpContextAccessor = httpContextAccessor;
//            _logger = logger;
//            _scopeFactory = scopeFactory;
//        }



//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            try
//            {
//                _logger.LogDebug("Application reminder Service is running in background");

//                while (!stoppingToken.IsCancellationRequested)
//                {
//                    using (var scope = _scopeFactory.CreateScope())
//                    {
//                        var _context = scope.ServiceProvider.GetRequiredService<AGS_DBContext>();

//                        _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

//                        var get = (from d in _context.MyDesk.AsEnumerable()
//                                   join a in _context.Applications.AsEnumerable() on d.AppId equals a.AppId
//                                   join s in _context.Staff.AsEnumerable() on d.StaffId equals s.StaffId
//                                   where d.HasWork == false && a.DeleteStatus == false && a.Status == GeneralClass.Processing && DateTime.Now.Date >= d.CreatedAt.Date.AddDays(4)
//                                   select new
//                                   {
//                                       a.AppId,
//                                       s.StaffEmail,
//                                       s.LastName,
//                                       s.FirstName,
//                                       s.StaffId,
//                                       a.AppRefNo
//                                   }).ToList();

//                        var getRem = get.GroupBy(x => x.StaffId).Select(x => x.FirstOrDefault()).ToList();

//                        if (getRem.Count() > 0)
//                        {
//                            for (int a = 0; a < getRem.Count(); a++)
//                            {
//                                string subj = "AGS Reminder -- Applications On Your Desk";
//                                string cont = "Here is a reminder for you... You are getting this reminder because you have have some application(s) on your desk that needs to be processed, please act accordingly.";

//                                var msg = _helpersController.SendEmailMessageAsync(getRem.ToList()[a].StaffEmail, getRem.ToList()[a].LastName + " " + getRem.ToList()[a].FirstName, subj, cont, GeneralClass.STAFF_NOTIFY, null);
//                            }
//                        }
//                    }

//                    await Task.Delay(TimeSpan.FromDays(4), stoppingToken);
//                }

//                _logger.LogDebug("Application reminder Service has stopped in background");
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }
//    }
//}
