using System;
using System.Threading.Tasks;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Tasks;
using DeliveryTracker.Validation;
using DeliveryTrackerWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/tasks")]
    public class TaskController : Controller
    {
        #region fields

        private readonly ITaskService taskService;

        private readonly IPostgresConnectionProvider cp;

        private readonly IUserCredentialsAccessor accessor;
        
        #endregion

        #region constructor

        public TaskController(
            ITaskService taskService,
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor)
        {
            this.taskService = taskService;
            this.cp = cp;
            this.accessor = accessor;
        }

        #endregion
        
        // tasks/create
        // tasks/get
        // tasks/edit
        // tasks/change_state
        // tasks/delete
        
        [HttpPost("create")]
        [Authorize(AuthorizationPolicies.CreatorOrManager)]
        public async Task<IActionResult> Create([FromBody] TaskRequest request)
        {
            var taskInfo = request?.TaskInfo;
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request), request)
                .AddNotNullRule(nameof(taskInfo), taskInfo)
                .AddNotNullOrWhitespaceRule($"{nameof(taskInfo)}.{nameof(taskInfo.TaskNumber)}", taskInfo?.TaskNumber)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new TaskResponse(validationResult.Error));
            }

            using (var connWrapper = this.cp.Create().Connect())
            {
                using (var transaction = connWrapper.BeginTransaction())
                {
                    var result = await this.taskService.CreateAsync(taskInfo, connWrapper);

                    if (!result.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(new TaskResponse(result.Errors));
                    }

                    var task = result.Result;
                    var packResult = await this.taskService.PackTaskAsync(task, connWrapper);
                    if (!packResult.Success)
                    {
                        transaction.Rollback();
                        return this.BadRequest(new TaskResponse(packResult.Errors));
                    }
                    transaction.Commit();
                    return this.Ok(new TaskResponse { TaskPackage = packResult.Result });
                }
            }

        }
        
        [HttpGet("get")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var validationResult = new ParametersValidator()
                .AddNotEmptyGuidRule(nameof(id), id)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new TaskResponse(validationResult.Error));
            }

            using (var connWrapper = this.cp.Create().Connect())
            {
                var result = await this.taskService.GetTaskAsync(id, connWrapper);

                if (!result.Success)
                {
                    return this.BadRequest(new TaskResponse(result.Errors));
                }

                var task = result.Result;
                var packResult = await this.taskService.PackTaskAsync(task, connWrapper);
                if (!packResult.Success)
                {
                    return this.BadRequest(new TaskResponse(packResult.Errors));
                }

                return this.Ok(new TaskResponse {TaskPackage = packResult.Result});
            }
        }
        
        [HttpPost("edit")]
        [Authorize(AuthorizationPolicies.CreatorOrManager)]
        public async Task<IActionResult> Edit([FromBody] TaskRequest request)
        {
            var taskInfo = request?.TaskInfo;
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request), request)
                .AddNotNullRule(nameof(taskInfo), taskInfo)
                .AddNotEmptyGuidRule($"{nameof(taskInfo)}.{nameof(taskInfo.Id)}", taskInfo?.Id ?? Guid.Empty)
                .Validate();
            if (!validationResult.Success
                || taskInfo == null)
            {
                return this.BadRequest(new TaskResponse(validationResult.Error));
            }

            var credentials = this.accessor.GetUserCredentials();
            taskInfo.InstanceId = credentials.InstanceId;
            
            using (var connWrapper = this.cp.Create().Connect())
            {
                using (var transact = connWrapper.BeginTransaction())
                {
                    var editResult = await this.taskService.EditTaskAsync(taskInfo, connWrapper);

                    if (!editResult.Success)
                    {
                        transact.Rollback();
                        return this.BadRequest(new TaskResponse(editResult.Errors));
                    }

                    var task = editResult.Result;
                    var packResult = await this.taskService.PackTaskAsync(task, connWrapper);
                    if (!packResult.Success)
                    {
                        transact.Rollback();
                        return this.BadRequest(new TaskResponse(packResult.Errors));
                    }

                    transact.Commit();
                    return this.Ok(new TaskResponse {TaskPackage = packResult.Result});
                }
            }
        }
        
        [HttpPost("change_state")]
        [Authorize]
        public async Task<IActionResult> ChangeState([FromBody] TaskRequest request)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request), request)
                .AddNotEmptyGuidRule($"{nameof(request)}.{nameof(request.Id)}", request.Id)
                .AddNotEmptyGuidRule($"{nameof(request)}.{nameof(request.TransitionId)}", request.TransitionId)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new TaskResponse(validationResult.Error));
            }

            using (var connWrapper = this.cp.Create().Connect())
            {
                using (var transact = connWrapper.BeginTransaction())
                {
                    var transitResult = await this.taskService.TransitAsync(request.Id, request.TransitionId, connWrapper);

                    if (!transitResult.Success)
                    {
                        transact.Rollback();
                        return this.BadRequest(new TaskResponse(transitResult.Errors));
                    }

                    var task = transitResult.Result;
                    var packResult = await this.taskService.PackTaskAsync(task, connWrapper);
                    if (!packResult.Success)
                    {
                        transact.Rollback();
                        return this.BadRequest(new TaskResponse(packResult.Errors));
                    }

                    transact.Commit();
                    return this.Ok(new TaskResponse {TaskPackage = packResult.Result});
                }
            }
        }
    }
}