using System;
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

        private readonly IUserRepository userRepository;
        
        private readonly ISecurityManager securityManager;

        private readonly IPostgresConnectionProvider connectionProvider;

        private readonly ILogger<SessionController> logger;
        
        #endregion
        
        #region constuctor
        
        public SessionController(
            IUserRepository userRepository,
            ISecurityManager securityManager,
            ILogger<SessionController> logger, 
            IPostgresConnectionProvider connectionProvider)
        {
            this.userRepository = userRepository;
            this.securityManager = securityManager;
            this.logger = logger;
            this.connectionProvider = connectionProvider;
        }

        #endregion

        [HttpGet("cc")]
        public async Task<IActionResult> MethodC()
        {

            using (var conn = this.connectionProvider.Create().Connect())
            {
                using (var conn2 = conn?.Connect() ?? this.connectionProvider.Create().Connect())
                {
                    

                }

            }

            return this.Ok();
        }

        [HttpGet("aa")]
        public async Task<IActionResult> MethodA(string username, string password)
        {
            var newUser = this.userRepository.Create(new User
            {
                Id = Guid.NewGuid(),
                Code = username,
            });
            var credentials = this.securityManager.ValidatePassword(username, password);
            var token = this.securityManager.AcquireToken(credentials);
            return this.Ok(token);
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