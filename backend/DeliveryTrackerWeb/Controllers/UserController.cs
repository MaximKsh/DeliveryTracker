using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTrackerWeb.Controllers
{
    [Authorize]
    [Route("api/user")]
    public class UserController: Controller
    {
        
        #region fields

        private readonly IUserManager userManager;
        
        private readonly IUserService userService;

        private readonly IUserCredentialsAccessor accessor;
        
        private readonly ILogger<UserController> logger;
        
        #endregion
        
        #region constructor
        
        public UserController(
            IUserManager userManager,
            IUserService userService,
            IUserCredentialsAccessor accessor,
            ILogger<UserController> logger)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.accessor = accessor;
            this.logger = logger;
        }

        #endregion 

        
        #region actions
        
        // user/get
        // user/edit
        // user/delete
        
        [HttpGet("get")]
        public async Task<IActionResult> Get(Guid? id, string code)
        {
            if (id.HasValue)
            {
                var result = await this.userService.GetAsync(id.Value);
                return result.Success
                    ? (IActionResult) this.Ok(result.Result)
                    : this.BadRequest(result.Errors);
            }
            if (!string.IsNullOrWhiteSpace(code))
            {
                var result = await this.userService.GetAsync(code);
                return result.Success
                    ? (IActionResult) this.Ok(result.Result)
                    : this.BadRequest(result.Errors);   
            }
            return this.BadRequest(ErrorFactory.ValidationError(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(nameof(id), null),
                new KeyValuePair<string, object>(nameof(code), null),
            }));
        }
        
        [Authorize(Policy = AuthorizationPolicies.CreatorOrManager)]
        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] User userInfo)
        {
            var result = await this.userService.EditAsync(userInfo);
            if (result.Success)
            {
                return this.Ok(result.Result);
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(result.Errors);
        }
        
        [Authorize(Policy = AuthorizationPolicies.CreatorOrManager)]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(Guid? id, string code)
        {
            Guid validId;
            if (id.HasValue)
            {
                validId = id.Value;
            }
            if (!string.IsNullOrWhiteSpace(code))
            {
                var credentials = this.accessor.UserCredentials;
                var idByCode = await this.userManager.GetIdAsync(code, credentials.InstanceId);
                if (!idByCode.HasValue)
                {
                    return this.BadRequest(ErrorFactory.UserNotFound(code));
                }

                validId = idByCode.Value;
            }
            else
            {
                return this.BadRequest(ErrorFactory.ValidationError(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>(nameof(id), null),
                    new KeyValuePair<string, object>(nameof(code), null),
                }));
            }
            var result = await this.userService.DeleteAsync(validId);
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
        
        #endregion
    }
}