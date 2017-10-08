using System;
using System.Threading.Tasks;
using DeliveryTracker.Helpers;
using DeliveryTracker.Services;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Controllers
{
    [Route("api/session")]
    public class SessionController: Controller
    {
        #region fields
        
        private readonly AccountService accountService;

        private readonly ILogger<SessionController> logger;
        
        #endregion
        
        #region constuctor
        
        public SessionController(
            AccountService accountService, 
            ILogger<SessionController> logger)
        {
            this.accountService = accountService;
            this.logger = logger;
        }

        #endregion
        
        #region actions
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CredentialsViewModel credentials)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }

            var loginResult = await this.accountService.Login(
                credentials.UserName, 
                credentials.Password,
                credentials.Role);
            if (!loginResult.Success)
            {
                return this.Unauthorized();
            }
            return this.Ok(loginResult.Result);
        }

        [Authorize]
        [HttpGet("check")]
        public async Task<IActionResult> CheckSession()
        {
            var currentUserResult = await this.accountService.FindUser(this.User.Identity.Name);
            if (!currentUserResult.Success)
            {
                return this.Unauthorized();
            }
            var user = currentUserResult.Result;
            
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return this.Unauthorized();
            }
            var role = roleResult.Result;
            
            return this.Ok(new UserInfoViewModel
            {
                DisplayableName = user.DisplayableName,
                Group = user.Group.DisplayableName,
                Role = role,
                UserName = user.UserName,
            });
        }
        
        #endregion
        
    }
}