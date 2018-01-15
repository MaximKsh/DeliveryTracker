using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/session")]
    public class SessionController: Controller
    {
        #region fields

        private readonly IUserManager userManager;

        private readonly IInvitationService invitationService;
        
        private readonly ISecurityManager securityManager;

        private readonly IPostgresConnectionProvider connectionProvider;

        private readonly IInstanceService instanceService;
        
        private readonly ILogger<SessionController> logger;
        
        #endregion
        
        #region constuctor
        
        public SessionController(
            IUserManager userManager,
            ISecurityManager securityManager,
            IInvitationService invitationService,
            ILogger<SessionController> logger, 
            IPostgresConnectionProvider connectionProvider, 
            IInstanceService instanceService)
        {
            this.userManager = userManager;
            this.securityManager = securityManager;
            this.invitationService = invitationService;
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
            var result = await this.invitationService.DeleteAsync("Rspf");
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