using System;
using System.Threading.Tasks;
using DeliveryTracker.Auth;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.Roles;
using DeliveryTracker.Services;
using DeliveryTracker.TaskStates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker
{
    public class TestStartup
    {
        private readonly IConfiguration configuration;
        
        public TestStartup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            this.configuration = builder.Build();
        }
        

        public void ConfigureServices(IServiceCollection services)
        {
            /*services.AddDbContext<DeliveryTrackerDbContext>(options =>
            {
                options.UseInMemoryDatabase("DeliveryTrackerTestDB");
                options.ConfigureWarnings(warningOpts =>
                {
                    warningOpts.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                });
            });*/

            
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

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}