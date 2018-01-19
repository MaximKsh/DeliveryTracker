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
        public async Task<IActionResult> Create([FromBody] CreateInstanceViewModel createInstanceViewModel)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule("CreateInstanceViewModel", createInstanceViewModel)
                .AddNotNullOrWhitespaceRule("InstanceName", createInstanceViewModel.InstanceName)
                .AddNotNullRule("Creator", createInstanceViewModel.Creator)
                .AddNotNullRule("CreatorCodePassword", createInstanceViewModel.CreatorCodePassword)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(validationResult.Error.ToOneElementList());
            }
            var result = await this.instanceService.CreateAsync(
                createInstanceViewModel.InstanceName, 
                createInstanceViewModel.Creator,
                createInstanceViewModel.CreatorCodePassword);
            if(!result.Success)
            {
                return this.BadRequest(result.Errors);
            }

            var token = this.securityManager.AcquireToken(result.Result.Item3);
            return this.Ok(new
            {
                Instance = result.Result.Item1,
                Creator = result.Result.Item2,
                Token = token,
            });
        }

        [Authorize]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await this.instanceService.GetAsync();
            return result.Success
                ? (IActionResult)this.Ok(result.Result)
                : this.BadRequest(result.Errors);
        }
        
        
        #endregion
    }

}