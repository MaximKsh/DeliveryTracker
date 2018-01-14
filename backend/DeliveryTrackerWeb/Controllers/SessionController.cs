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

        private readonly IInvitationManager invitationManager;
        
        private readonly ISecurityManager securityManager;

        private readonly IPostgresConnectionProvider connectionProvider;

        private readonly IInstanceService instanceService;
        
        private readonly ILogger<SessionController> logger;
        
        #endregion
        
        #region constuctor
        
        public SessionController(
            IUserManager userManager,
            ISecurityManager securityManager,
            IInvitationManager invitationManager,
            ILogger<SessionController> logger, 
            IPostgresConnectionProvider connectionProvider, 
            IInstanceService instanceService)
        {
            this.userManager = userManager;
            this.securityManager = securityManager;
            this.invitationManager = invitationManager;
            this.logger = logger;
            this.connectionProvider = connectionProvider;
            this.instanceService = instanceService;
        }

        #endregion

        [HttpGet("cc")]
        public async Task<IActionResult> MethodC()
        {
            var user = new User()
            {
                Surname = "asf",
                Name = "sdf",
                Role = "CreatorRole",
                InstanceId = Guid.Parse("e8d34e3b-d58a-4686-8131-b479e70ba9e4"),
            };
            var pass = new CodePassword()
            {
                Code = "aaaaa",
                Password = "abc",
            };
            var result = await this.invitationManager.DeleteAsync("Rspf");
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