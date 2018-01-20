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
        public async Task<IActionResult> Login([FromBody] AccountRequest request)
        {
            var codePassword = request.CodePassword;
            
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request.CodePassword), request.CodePassword)
                .Validate();
            
            if (!validationResult.Success)
            {
                return this.BadRequest(new AccountResponse(validationResult.Error));
            }
            
            var result = await this.accountService.LoginWithRegistrationAsync(codePassword);
            if (!result.Success)
            {
                return this.Unauthorized();
            }
            var token = this.securityManager.AcquireToken(result.Result.Item2);
            return this.Ok(new AccountResponse
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
                return this.Unauthorized();
            }
            return this.Ok(new AccountResponse
            {
                User = result.Result
            });
        }
        
        [Authorize]
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] AccountRequest request)
        {
            var newData = request.User;
            
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request.User), request.User)
                .Validate();
            
            if (!validationResult.Success)
            {
                return this.BadRequest(new AccountResponse(validationResult.Error));
            }
            
            var result = await this.accountService.EditAsync(newData);
            if (!result.Success)
            {
                return this.BadRequest(new AccountResponse(result.Errors));
            }
            return this.Ok(new AccountResponse
            {
                User = result.Result
            });
        }
        
        [Authorize]
        [HttpPost("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] AccountRequest request)
        {
            var oldPassword = request.CodePassword;
            var newPassword = request.NewCodePassword;
            
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request.CodePassword), oldPassword)
                .AddNotNullRule(nameof(request.NewCodePassword), newPassword)
                .Validate();
            
            if (!validationResult.Success)
            {
                return this.BadRequest(new AccountResponse(validationResult.Error));
            }

            var result = await this.accountService.ChangePasswordAsync(
                oldPassword.Password,
                newPassword.Password);
            if (result.Success)
            {
                return this.Ok();
            }
            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(new AccountResponse(result.Errors));
        }
    }
}