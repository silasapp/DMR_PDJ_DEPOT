using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Rotativa.AspNetCore;
using NewDepot.Models.Stored_Procedures;
using Westwind.AspNetCore.LiveReload;
using Microsoft.AspNetCore.Authentication.Cookies;
using NewDepot.Helpers;
using NewDepot.Services;
using NewDepot.Models;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace NewDepot
{
    public class Startup
    {
        public 
            Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Depot_DBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Depot_DBConnectionString")));
            services.AddDbContext<StoredProcedure>(options => options.UseSqlServer(Configuration.GetConnectionString("Depot_DBConnectionString")));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.LoginPath = new PathString("/Account/AccessDenied");
               });

            services.ConfigureApplicationCookie(opts => opts.LoginPath = "/");


            ElpsServices._elpsAppEmail = Configuration.GetSection("ElpsKeys").GetSection("elpsAppEmail").Value.ToString();
            ElpsServices._elpsBaseUrl = Configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString();
            ElpsServices.public_key = Configuration.GetSection("ElpsKeys").GetSection("PK").Value.ToString();
            ElpsServices._elpsAppKey = Configuration.GetSection("ElpsKeys").GetSection("elpsSecretKey").Value.ToString();
            ElpsServices.conString = Configuration.GetSection("ElpsKeys").GetSection("conString").Value.ToString();

            services.AddDistributedMemoryCache();

            services.AddSession();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(12);//You can set Time   
            });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReportingRoles",
                     policy => policy.RequireRole(GeneralClass.ED,GeneralClass.AUTHORITY,GeneralClass.HDS, GeneralClass.OOD, GeneralClass.ADPDJ, GeneralClass.SUPERVISOR, GeneralClass.ADMIN, GeneralClass.SUPPORT, GeneralClass.SUPER_ADMIN, GeneralClass.ICT_ADMIN, GeneralClass.IT_ADMIN));
                
                options.AddPolicy("AdminRoles",
                     policy => policy.RequireRole(GeneralClass.ADMIN, GeneralClass.HDS, GeneralClass.SUPPORT, GeneralClass.SUPER_ADMIN, GeneralClass.ICT_ADMIN, GeneralClass.IT_ADMIN));


                options.AddPolicy("ProcessingStaffRoles",
                     policy => policy.RequireRole(GeneralClass.ED, GeneralClass.AUTHORITY, GeneralClass.OOD, GeneralClass.HDS, GeneralClass.ADPDJ, GeneralClass.SUPERVISOR, GeneralClass.INSPECTOR, GeneralClass.OPSCON, GeneralClass.ADOPS, GeneralClass.TEAMLEAD));


                options.AddPolicy("AllStaffRoles",
                     policy => policy.RequireRole(GeneralClass.ED_STA,GeneralClass.ED, GeneralClass.ACE_STA, GeneralClass.AUTHORITY, GeneralClass.HDS, GeneralClass.OOD, GeneralClass.ADPDJ, GeneralClass.SUPERVISOR, GeneralClass.INSPECTOR,  GeneralClass.OPSCON, GeneralClass.ADOPS, GeneralClass.TEAMLEAD, GeneralClass.ADMIN, GeneralClass.ICT_ADMIN, GeneralClass.IT_ADMIN, GeneralClass.SUPER_ADMIN, GeneralClass.SUPPORT));


                options.AddPolicy("CompanyRoles",
                    policy => policy.RequireRole(GeneralClass.COMPANY));


                options.AddPolicy("ConfigurationRoles",
                   policy => policy.RequireRole(GeneralClass.SUPER_ADMIN, GeneralClass.SUPPORT, GeneralClass.HDS, GeneralClass.IT_ADMIN, GeneralClass.ICT_ADMIN, GeneralClass.ADMIN, GeneralClass.OOD, GeneralClass.ADPDJ));


                options.AddPolicy("HeadOfficeStaffRoles",
                    policy => policy.RequireRole(GeneralClass.ED, GeneralClass.AUTHORITY,GeneralClass.HDS, GeneralClass.SUPPORT, GeneralClass.INSPECTOR, GeneralClass.OOD, GeneralClass.SUPER_ADMIN, GeneralClass.IT_ADMIN, GeneralClass.ADMIN, GeneralClass.ICT_ADMIN, GeneralClass.SUPERVISOR, GeneralClass.ADPDJ));


                options.AddPolicy("PermitRoles",
                    policy => policy.RequireRole(GeneralClass.ED, GeneralClass.AUTHORITY, GeneralClass.OPSCON , GeneralClass.OOD, GeneralClass.SUPPORT, GeneralClass.HDS,  GeneralClass.SUPER_ADMIN, GeneralClass.IT_ADMIN, GeneralClass.ADMIN, GeneralClass.COMPANY, GeneralClass.ICT_ADMIN, GeneralClass.ADMIN, GeneralClass.SUPERVISOR, GeneralClass.ADPDJ, GeneralClass.OPSCON, GeneralClass.TEAMLEAD, GeneralClass.INSPECTOR, GeneralClass.ADOPS));


                options.AddPolicy("FieldOfficeStaffRoles",
                   policy => policy.RequireRole(GeneralClass.SUPER_ADMIN, GeneralClass.SUPPORT, GeneralClass.IT_ADMIN, GeneralClass.ADMIN, GeneralClass.OOD , GeneralClass.TEAMLEAD, GeneralClass.OPSCON, GeneralClass.ADOPS));

            });

             services.AddRazorPages().AddRazorRuntimeCompilation();
              services.AddMvc().AddRazorRuntimeCompilation();

            services.AddHttpContextAccessor();
            services.AddHttpClient();


            services.AddMvc(options => options.EnableEndpointRouting = false)
                           .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);


        }

        //Scaffold-DbContext -Connection name=Depot_DBConnectionString -OutputDir Models -context Depot_DBContext -UseDatabaseNames -Project NewDepot Microsoft.EntityFrameworkCore.SqlServer -force
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }
            app.Use(async (context, next) =>
            {
                context.Response.Redirect("https://depotonline.nmdpra.gov.ng/");
                return;
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(env.ContentRootPath+"/wwwroot")
            });
            app.UseSession();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}/{option?}/{option2?}/{option3?}/{option4?}/{option5?}/{option6?}");

                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "{controller}/{action}/{id?}"
                    );
            });

            RotativaConfiguration.Setup(env.ContentRootPath, "wwwroot/Rotativa");

        }
    }
}



