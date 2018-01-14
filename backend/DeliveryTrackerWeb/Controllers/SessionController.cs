using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.DbModels;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/session")]
    public class SessionController: Controller
    {
        #region fields

        private readonly IUserManager userManager;
        
        private readonly IRoleManager roleManager;
        private readonly ISecurityManager securityManager;

        private readonly IPostgresConnectionProvider connectionProvider;

        private readonly ILogger<SessionController> logger;
        
        #endregion
        
        #region constuctor
        
        public SessionController(
            IUserManager userManager,
            IRoleManager roleManager,
            ISecurityManager securityManager,
            ILogger<SessionController> logger, 
            IPostgresConnectionProvider connectionProvider)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.securityManager = securityManager;
            this.logger = logger;
            this.connectionProvider = connectionProvider;
        }

        #endregion

        [HttpGet("cc")]
        public async Task<IActionResult> MethodC()
        {
            var roles = new List<Role>();
            roles.Add(new Role(DefaultRoles.CreatorRole, DefaultRoles.CreatorRoleName));
            roles.Add(new Role(DefaultRoles.ManagerRole, DefaultRoles.ManagerRoleName));

            var user = new User()
            {
                Id = Guid.NewGuid(),
                Code = "abc",
                Position = new Geoposition()
                {
                    Latitude = 1,
                    Longitude = 2,
                },
                Roles = roles.AsReadOnly(),
            };
            var user2 = new User();
            var userSerialized = user.Serialize();
            user2.Deserialize(userSerialized);
            return this.Ok();
        }

        [HttpGet("aa")]
        public async Task<IActionResult> MethodA(string username, string password)
        {
           
            return this.Ok();
        }

        [Authorize]
        [HttpGet("bb")]
        public async Task<IActionResult> MethodB()
        {
            
            return this.Ok(new
            {
                Code = this.User.Claims.First(p => p.Type.Equals(DeliveryTrackerClaims.InstanceId)).Value,
            });
        }

    }
}