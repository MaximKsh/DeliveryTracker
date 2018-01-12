using System;
using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.DbModels;
using DeliveryTracker.Identification;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DeliveryTracker.Services;
using DeliveryTracker.Validation;
using DeliveryTracker.Views;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;

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
        

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<DeliveryTrackerDbContext>(options =>
            //    options.UseInMemoryDatabase("TestDB"));
            /*services.AddIdentity<UserModel, RoleModel>(o =>
                {
                    o.Password.RequiredLength = 1;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequireDigit = false;
                    o.Password.RequiredUniqueChars = 0;

                })
                .AddEntityFrameworkStores<DeliveryTrackerDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<UserModel, RoleModel, DeliveryTrackerDbContext, Guid>>()
                .AddRoleStore<RoleStore<RoleModel, DeliveryTrackerDbContext, Guid>>();
            */
            //var connectionString = this.configuration.GetConnectionString("DefaultConnection");
            //services.AddDbContext<DeliveryTrackerDbContext>(
            //    options => options.UseNpgsql(connectionString));
 
            Configure4xx(services);
            ConfigureJson(services);

            services
                .AddDeliveryTrackerDatabase()
                .AddDeliveryTrackerIdentifiaction(this.configuration)
                ;
        }

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
                        .SerializeObject(ErrorFactory.ServerError().ToErrorListViewModel());
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
                    p.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    p.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    p.SerializerSettings.Formatting = Formatting.None;
                });
        }

    }
}