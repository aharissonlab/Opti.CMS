using EPiServer.Authorization;
using EPiServer.Cms.Shell;
using EPiServer.Cms.TinyMce.Core;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Personalization.VisitorGroups;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Geta.NotFoundHandler.Infrastructure.Configuration;
using Geta.NotFoundHandler.Infrastructure.Initialization;
using Geta.NotFoundHandler.Optimizely.Infrastructure.Configuration;
using Geta.NotFoundHandler.Optimizely.Infrastructure.Initialization;
using Geta.Optimizely.Sitemaps;
using Opti.CMS.Extensions;

namespace Opti.CMS;

public class Startup(
    IWebHostEnvironment webHostingEnvironment,
    IConfiguration configuration)
{
    readonly IWebHostEnvironment _webHostingEnvironment = webHostingEnvironment;
    readonly IConfiguration _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data"));

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            // .AddFind()
            .AddCms()
            .AddAlloy()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>();

        services.Configure<VisitorGroupOptions>(o => o.EnableSession = true);

        //services.Configure<HeadlessModeOptions>(options => options.HeadlessModeEnabled = true);

        //services.Configure<TinyMceConfiguration>(tinyMceConfiguration =>
        //{
        //    var defaultConfiguration = tinyMceConfiguration
        //                                .Default()
        //                                .AddEpiserverSupport();
        //});

        // Required by Wangkanai.Detection
        services.AddDetection();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services
            .AddNotFoundHandler(options =>
                options.UseSqlServer(_configuration.GetConnectionString("EPiServerDB")),
                policy => policy.RequireRole(Roles.WebAdmins));

        // Add Optimizely not found handler
        services.AddOptimizelyNotFoundHandler(options => options.AutomaticRedirectsEnabled = true);

        //Add Geta Optimizely sitemaps
        services.AddSitemaps(options =>
        {
            //options.EnableLanguageDropDownInAdmin = true;
            //options.EnableRealtimeCaching = true;
            //options.EnableRealtimeSitemap = false;
        },
        policy => policy.RequireRole(Roles.WebAdmins));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        // app.UseNotFoundHandler();
        app.UseOptimizelyNotFoundHandler();
        // Required by Wangkanai.Detection
        app.UseDetection();
        app.UseSession();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapContent();
        });
    }
}