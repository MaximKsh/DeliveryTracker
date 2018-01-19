using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using DeliveryTrackerWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly IAccountService accountService;

        private readonly ISecurityManager securityManager;
        
        public AccountController(IAccountService accountService, ISecurityManager securityManager)
        {
            this.accountService = accountService;
            this.securityManager = securityManager;
        }
        
        // account/login
        // account/about
        // account/edit
        // account/change_password
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CodePassword codePassword)
        {
            var result = await this.accountService.LoginWithRegistrationAsync(codePassword);
            if (!result.Success)
            {
                return this.Unauthorized();
            }
            var token = this.securityManager.AcquireToken(result.Result.Item2);
            return this.Ok(new
            {
                User = result.Result.Item1,
                Token = token,
            });
        }
        
        [Authorize]
        [HttpGet("about")]
        public async Task<IActionResult> About()
        {
            var result = await this.accountService.GetAsync();
            if (!result.Success)
            {
                return this.StatusCode(401, result.Errors);
            }
            return this.Ok(result.Result);
        }
        
        [Authorize]
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] User newData)
        {
            var result = await this.accountService.EditAsync(newData);
            if (!result.Success)
            {
                return this.BadRequest(result.Errors);
            }
            return this.Ok(result.Result);
        }
        
        [Authorize]
        [HttpPost("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] OldNewPasswordViewModel oldNewPassword)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule("OldNewPassword", oldNewPassword)
                .AddNotNullRule("old", oldNewPassword.Old)
                .AddNotNullRule("new", oldNewPassword.New)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(validationResult.Error.ToOneElementList());
            }

            var result = await this.accountService.ChangePasswordAsync(
                oldNewPassword.Old.Password,
                oldNewPassword.New.Password);
            if (result.Success)
            {
                return this.Ok();
            }
            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(result.Errors);

        }
    }
}