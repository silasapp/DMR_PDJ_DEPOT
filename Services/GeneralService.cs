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
using NewDepot.Controllers.UsersManagement;
using Microsoft.AspNetCore.Mvc;

namespace NewDepot.Services
{
    public class GeneralService : BackgroundService, IDisposable
    {
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        private readonly ILogger<GeneralService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        RestSharpServices _restService = new RestSharpServices();

        public GeneralService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<GeneralService> logger, IServiceScopeFactory scopeFactory)
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
                _logger.LogDebug("General service is running in background");

                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var _context = scope.ServiceProvider.GetRequiredService<Depot_DBContext>();

                    _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);

                        var getSch = (from s in _context.MeetingSchedules.AsEnumerable()
                                      join mg in _context.ManagerScheduleMeetings.AsEnumerable() on s.Id equals mg.ScheduleId
                                      where (DateTime.Now > s.MeetingDate || DateTime.Now > s.Date.AddDays(3)) && s.ScheduleExpired == null
                                      select s).ToList();

                        int i = 0;

                        if (getSch.Count() > 0)
                        {
                            getSch.ForEach( sh=> {

                                sh.ScheduleExpired = true;
                                sh.UpdatedAt = DateTime.Now;
                                i = _context.SaveChanges();
                            });
                        }

                        var get = from o in _context.OutOfOffice.AsEnumerable()
                                  where o.Approved== true && o.Status == GeneralClass._WAITING && o.DeletedStatus == false
                                  select o;


                        foreach (var a in get.ToList())
                        {
                            if (a.DateFrom < DateTime.Now)
                            {
                                var update = _context.OutOfOffice.Where(x => x.OutID == a.OutID);
                                update.FirstOrDefault().Status = GeneralClass._STARTED;
                                update.FirstOrDefault().UpdatedAt = DateTime.Now;
                               i=  _context.SaveChanges();
                            }
                        }
                        await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
                    }
                

                _logger.LogDebug("General service has stopped in background");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
