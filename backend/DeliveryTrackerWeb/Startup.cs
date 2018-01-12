﻿using System;
using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.DbModels;
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
using DeliveryTrackerWeb.Auth;
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

            var connectionString = this.configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DeliveryTrackerDbContext>(
                options => options.UseNpgsql(connectionString));
 
            services.AddIdentity<UserModel, RoleModel>(o =>
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
            
            this.ConfigureAuthorization(services);

            services.AddMvc().AddJsonOptions(
                p =>
                {
                    p.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    p.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    p.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    p.SerializerSettings.Formatting = Formatting.None;
                });

            
            
            services
                .AddDeliveryTrackerServices()
                .AddDeliveryTrackerViews()
                
                .AddDeliveryTrackerAuth();
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

        private void ConfigureAuthorization(IServiceCollection services)
        {
            var authInfo = new AuthInfo(
                this.configuration.GetValue<string>("AuthInfo:Key", null) ?? throw new NullReferenceException("specify secret key"),
                this.configuration.GetValue<string>("AuthInfo:Issuer", null) ?? throw new NullReferenceException("specify issuer"),
                this.configuration.GetValue<string>("AuthInfo:Audience", null) ?? throw new NullReferenceException("specify audience"),
                this.configuration.GetValue("AuthInfo:Lifetime", 1),
                this.configuration.GetValue("AuthInfo:ClockCkew", 1),
                this.configuration.GetValue("AuthInfo:RequireHttps", true));
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = authInfo.RequireHttps;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // укзывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представляющая издателя
                        ValidIssuer = authInfo.Issuer,
 
                        // будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        // установка потребителя токена
                        ValidAudience = authInfo.Audience,
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(authInfo.ClockCkew),
 
                        // установка ключа безопасности
                        IssuerSigningKey = authInfo.GetSymmetricSecurityKey(),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };
                });

            services.AddSingleton(authInfo);
        }
    }
}