using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Database;
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
        
        private static readonly string AuthHeader = HttpRequestHeader.Authorization.ToString();
        
        private readonly IAccountService accountService;

        private readonly ISecurityManager securityManager;

        private readonly IPostgresConnectionProvider cp;

        private readonly IUserCredentialsAccessor accessor;

        private readonly IDeviceManager deviceManager;
        
        public AccountController(
            IAccountService accountService, 
            ISecurityManager securityManager,
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor,
            IDeviceManager deviceManager)
        {
            this.accountService = accountService;
            this.securityManager = securityManager;
            this.cp = cp;
            this.accessor = accessor;
            this.deviceManager = deviceManager;
        }
        
        // account/check
        // account/login
        // account/refresh
        // account/about
        // account/edit
        // account/change_password
        // account/update_device

        [HttpGet("check")]
        [Authorize]
        public IActionResult Check()
        {
            // Проверка валидности токена.
            // Если токен невалиден, вернется 403.
            return this.Ok();
        }
        
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

            using (var cw = this.cp.Create().Connect())
            using (var transact = cw.BeginTransaction())
            {
                var result = await this.accountService.LoginWithRegistrationAsync(codePassword, cw);
                if (!result.Success)
                {
                    transact.Rollback();
                    return this.Forbid();
                }
                var sessionResult = await this.securityManager.NewSessionAsync(result.Result.Credentials, cw);
                if (!sessionResult.Success)
                {
                    transact.Rollback();
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, new AccountResponse(sessionResult.Errors));
                }

                var session = sessionResult.Result;
                var statusCode = result.Result.Registered
                    ? (int)HttpStatusCode.Created
                    : (int)HttpStatusCode.OK;
                transact.Commit();
                return this.StatusCode(statusCode, new AccountResponse
                {
                    User = result.Result.User,
                    Token = session.SessionToken,
                    RefreshToken = session.RefreshToken,
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] AccountRequest request)
        {
            // Несмотря на то, что нет [Authorize] нужен токен
            // Не нужна проверка токена целиком для запуска метода.
            // Однако наличие в DI корректных UserCredentials необходимо
            if (!this.Request.Headers.ContainsKey(AuthHeader))
            {
                return this.Unauthorized();
            }
            
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule(nameof(request.RefreshToken), request.RefreshToken)
                .Validate();
            if (!this.Request.Headers.ContainsKey(AuthHeader))
            {
                return this.Forbid();
            }    
            
            if (!validationResult.Success)
            {
                return this.BadRequest(new AccountResponse(validationResult.Error));
            }
            
            var result = await this.securityManager.RefreshSessionAsync(request.RefreshToken);
            if (!result.Success)
            {
                return this.Forbid();
            }

            var session = result.Result;

            var accountGetResult = await this.accountService.GetAsync();
            if (!accountGetResult.Success)
            {
                return this.Forbid();
            }
            
            return this.Ok( new AccountResponse
            {
                User = accountGetResult.Result.User,
                Token = session.SessionToken,
                RefreshToken = session.RefreshToken,
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
                User = result.Result.User
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
                User = result.Result.User
            });
        }
        
        [Authorize]
        [HttpPost("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] AccountRequest request)
        {
            var oldPassword = request?.CodePassword;
            var newPassword = request?.NewCodePassword;
            
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request.CodePassword), oldPassword)
                .AddNotNullRule(nameof(request.NewCodePassword), newPassword)
                .Validate();
            
            if (!validationResult.Success)
            {
                return this.BadRequest(new AccountResponse(validationResult.Error));
            }

            var result = await this.accountService.ChangePasswordAsync(
                oldPassword?.Password,
                newPassword?.Password);
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

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var creds = this.accessor.GetUserCredentials();
            var result = await this.securityManager.LogoutAsync(creds.Id);
            if (result.Success)
            {
                return this.Ok();
            }

            return this.Forbid();
        }

        [Authorize]
        [HttpPost("update_device")]
        public async Task<IActionResult> UpdateDevice([FromBody] AccountRequest request)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request), request)
                .AddNotNullRule($"{nameof(request)}.{nameof(request.Device)}", request.Device)
                .Validate();
            
            if (!validationResult.Success)
            {
                return this.BadRequest(new AccountResponse(validationResult.Error));
            }

            var result = await this.deviceManager.UpdateUserDeviceAsync(request.Device);
            if (result.Success)
            {
                return this.Ok(new AccountResponse());
            }

            return this.BadRequest(new AccountResponse(result.Errors));
        }
        
    }
}