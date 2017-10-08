using System;
using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Auth;
using DeliveryTracker.Db;
using DeliveryTracker.Helpers;
using DeliveryTracker.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DeliveryTracker.Roles;
using DeliveryTracker.Services;
using DeliveryTracker.TaskStates;
using DeliveryTracker.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeliveryTracker
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

            var connectionString = this.configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DeliveryTrackerDbContext>(
                options => options.UseNpgsql(connectionString));
 
            services.AddIdentity<UserModel, RoleModel>()
                .AddEntityFrameworkStores<DeliveryTrackerDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<UserModel, RoleModel, DeliveryTrackerDbContext, Guid>>()
                .AddRoleStore<RoleStore<RoleModel, DeliveryTrackerDbContext, Guid>>();

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
            
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = AuthPolicies.DefaultPolicy;
                options.AddPolicy(AuthPolicies.Creator, AuthPolicies.CreatorPolicy);
                options.AddPolicy(AuthPolicies.Manager, AuthPolicies.ManagerPolicy);
                options.AddPolicy(AuthPolicies.CreatorOrManager, AuthPolicies.CreatorOrManagerPolicy);
                options.AddPolicy(AuthPolicies.Performer, AuthPolicies.PerformerPolicy);
            });
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            // укзывает, будет ли валидироваться издатель при валидации токена
                            ValidateIssuer = true,
                            // строка, представляющая издателя
                            ValidIssuer = AuthHelper.Issuer,
 
                            // будет ли валидироваться потребитель токена
                            ValidateAudience = true,
                            // установка потребителя токена
                            ValidAudience = AuthHelper.Audience,
                            // будет ли валидироваться время существования
                            ValidateLifetime = true,
 
                            // установка ключа безопасности
                            IssuerSigningKey = AuthHelper.GetSymmetricSecurityKey(),
                            // валидация ключа безопасности
                            ValidateIssuerSigningKey = true,
                        };
                    });

            services.AddMvc();

            services
                .AddDeliveryTrackerServices()
                .AddDeliveryTrackerRoles()
                .AddDeliveryTrackerTaskStates();
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
    }
}
