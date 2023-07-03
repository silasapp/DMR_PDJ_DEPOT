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
//    public class RerouteReminder : BackgroundService, IDisposable
//    {
//        GeneralClass generalClass = new GeneralClass();
//        IHttpContextAccessor _httpContextAccessor;
//        public IConfiguration _configuration;
//        HelpersController _helpersController;
//        private readonly ILogger<RerouteReminder> _logger;
//        private readonly IServiceScopeFactory _scopeFactory;

//        public RerouteReminder(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<RerouteReminder> logger, IServiceScopeFactory scopeFactory)
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
//                _logger.LogDebug("Application reroute reminder Service is running in background");

//                while (!stoppingToken.IsCancellationRequested)
//                {
//                    using (var scope = _scopeFactory.CreateScope())
//                    {
//                        var _context = scope.ServiceProvider.GetRequiredService<AGS_DBContext>();

//                        _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

//                        var get = (from d in _context.MyDesk.AsEnumerable()
//                                   join a in _context.Applications.AsEnumerable() on d.AppId equals a.AppId
//                                   join s in _context.Staff.AsEnumerable() on d.StaffId equals s.StaffId
//                                   where d.HasWork == false && DateTime.Now.Date >= d.CreatedAt.Date.AddDays(12) && a.IsLegacy == false 
//                                   select new
//                                   {
//                                       s.StaffEmail,
//                                       s.LastName,
//                                       s.FirstName,
//                                       s.StaffId,
//                                       a.AppRefNo,
//                                       s.FieldOfficeId
//                                   }).ToList();

//                        var getRem = get.GroupBy(x => x.StaffId).Select(x => x.FirstOrDefault()).ToList();

//                        if (getRem.Count() > 0)
//                        {
//                            for (int a = 0; a < getRem.Count(); a++)
//                            {
//                                var getSup = from s in _context.Staff.AsEnumerable()
//                                             join r in _context.UserRoles.AsEnumerable() on s.RoleId equals r.RoleId
//                                             where s.FieldOfficeId == getRem.ToList()[a].FieldOfficeId && (r.RoleName == GeneralClass.FIELD_SUPERVISOR || r.RoleName == GeneralClass.HQ_SUPERVISOR)
//                                             select new
//                                             {
//                                                 s.StaffEmail,
//                                                 s.LastName,
//                                                 s.FirstName,
//                                                 s.StaffId,
//                                             };

//                                if (getSup.Count() > 0)
//                                {
//                                    string subj = "-- REROUTE APPLICATION ON STAFF DESK --";
//                                    string cont =  getRem.ToList()[a].LastName + " " + getRem.ToList()[a].FirstName + " has some application(s) on his/her desk that needs to be rerouted because it has stayed more than 2 weeks on desk. Kindly go to Desk => My Location Desk and move the apps to another staff in the same role with the previous staff...";
//                                    var msg = _helpersController.SendEmailMessageAsync(getSup.FirstOrDefault().StaffEmail, getSup.FirstOrDefault().LastName + " " + getSup.FirstOrDefault().FirstName, subj, cont, GeneralClass.STAFF_NOTIFY, null);
//                                }
//                            }
//                        }
//                    }

//                    await Task.Delay(TimeSpan.FromDays(12), stoppingToken);
//                }

//                _logger.LogDebug("Application reroute reminder Service has stopped in background");
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//    }
//}
