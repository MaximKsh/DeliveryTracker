using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using DeliveryTrackerWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/instance")]
    public sealed class InstanceController: Controller
    {
        #region fields
        
        private readonly IInstanceService instanceService;

        private readonly ISecurityManager securityManager;
        
        private readonly ILogger<InstanceController> logger;
        
        #endregion
        
        #region constructor
        
        public InstanceController(
            IInstanceService instanceService, 
            ISecurityManager securityManager,
            ILogger<InstanceController> logger)
        {
            this.instanceService = instanceService;
            this.securityManager = securityManager;
            this.logger = logger;
        }

        #endregion 
        
        #region actions
        
        // instance/create
        // instance/settings
        // ...

        // invitation/create
        // invitation/get
        // invitation/delete
        
        // user/get
        // user/edit
        // user/delete
        
        // task/create
        // task/get
        // task/edit
        // task/change_state
        // task/delete
        
        // account/login
        // account/about
        // account/edit
        // account/change_password
        
        // reference/types
        // reference/{type}/create
        // reference/{type}/edit
        // reference/{type}/get
        // reference/{type}/delete
        
        // view/groups
        // view/{groupName}/views
        // view/{groupName}/{viewName}

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] InstanceRequest request)
        {
            var instance = request.Instance;
            var creator = request.Creator;
            var creatorDevice = request.CreatorDevice;
            var codePassword = request.CodePassword;
            
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request.Instance), instance)
                .AddNotNullRule(nameof(request.Creator), creator)
                .AddNotNullRule(nameof(request.CreatorDevice), creatorDevice)
                .AddNotNullRule(nameof(request.CodePassword), codePassword)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new InstanceResponse(validationResult.Error));
            }
            var result = await this.instanceService.CreateAsync(
                instance.Name, 
                creator,
                creatorDevice,
                codePassword);
            if(!result.Success)
            {
                return this.BadRequest(new InstanceResponse(result.Errors));
            }

            var sessionResult = await this.securityManager.NewSessionAsync(result.Result.Credentials);
            if (!sessionResult.Success)
            {
                return this.StatusCode((int)HttpStatusCode.Unauthorized, new AccountResponse(sessionResult.Errors));
            }

            var session = sessionResult.Result;
            return this.StatusCode((int)HttpStatusCode.Created, new InstanceResponse
            {
                Instance = result.Result.Instance,
                Creator = result.Result.User,
                Token = session.SessionToken,
                RefreshToken = session.RefreshToken,
            });
        }

        [Authorize]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await this.instanceService.GetAsync();
            return result.Success
                ? (IActionResult)this.Ok(new InstanceResponse { Instance = result.Result.Instance} )
                : this.BadRequest(new InstanceResponse(result.Errors));
        }
        
        
        #endregion
    }

}