using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Geopositioning;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Notifications;
using DeliveryTracker.References;
using DeliveryTracker.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DeliveryTracker.Views;
using DeliveryTrackerWeb.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DeliveryTrackerWeb
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class Startup
    {
        private readonly IConfiguration configuration;
        
        public Startup(IConfiguration configuration)
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
                .AddDeliveryTrackerCommon()
                .AddDeliveryTrackerDatabase()
                .AddDeliveryTrackerIdentification(this.configuration)
                .AddDeliveryTrackerGeopositioning()
                .AddDeliveryTrackerInstances()
                .AddDeliveryTrackerReferences()
                .AddDeliveryTrackerViews()
                .AddDeliveryTrackerTasks()
                .AddDeliveryTrackerNotifications()
                ;
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, ISettingsStorage settingsStorage)
        {
            settingsStorage
                .AddDeliveryTrackerDatabaseSettings(this.configuration)
                .AddDeliveryTrackerIdentificationSettings(this.configuration)
                .AddDeliveryTrackerInstancesSettings(this.configuration)
                .AddDeliveryTrackerNotificationSettings(this.configuration);
            
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseAuthentication();
            app.UseMiddleware<CheckSessionMiddleware>();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
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
                    p.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new DefaultNamingStrategy()
                        {
                            ProcessDictionaryKeys = true,
                        }
                    };
                    p.SerializerSettings.Converters.Add(new DictionaryObjectJsonConverter());
                    p.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    p.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    p.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    p.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    p.SerializerSettings.Formatting = Formatting.None;
                });
        }

    }
}