using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewDepot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 
namespace NewDepot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseIISIntegration();
      
                })
            .ConfigureServices(services => 
            {
                //services.AddHostedService<ExtraPaymentService>();
                services.AddHostedService<PaymentService>();
                services.AddHostedService<GeneralService>();
            });
    }
}
