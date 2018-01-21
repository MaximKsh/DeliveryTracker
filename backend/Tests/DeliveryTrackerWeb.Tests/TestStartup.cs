using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
using DeliveryTracker.Views;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace DeliveryTrackerWeb.Tests
{
    public class TestStartup
    {
        private readonly IConfiguration configuration;
        
        public TestStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            Configure4xx(services);
            ConfigureJson(services);

            services
                .TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services
                .AddDeliveryTrackerDatabase()
                .AddDeliveryTrackerIdentification(this.configuration)
                .AddDeliveryTrackerInstances(this.configuration)
                .AddDeliveryTrackerReferences()
                .AddDeliveryTrackerViews()
                ;
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseExceptionHandler(conf =>
            {
                conf.Run(async context =>
                {
                    // Логирование исключения уже производится в ExceptionHandler-е
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var responseString = JsonConvert
                        .SerializeObject(ErrorFactory.ServerError());
                    await context.Response.WriteAsync(responseString).ConfigureAwait(false);
                });
            });
            app.UseAuthentication();
            app.UseMvc();
        }

        // ReSharper disable once InconsistentNaming
        private static void Configure4xx(IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });
        }

        private static void ConfigureJson(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(
                p =>
                {
                    p.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    p.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                    p.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    p.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    p.SerializerSettings.Formatting = Formatting.None;
                });
        }
    }
}