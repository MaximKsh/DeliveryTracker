using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Auth;
using DeliveryTracker.Db;
using DeliveryTracker.Helpers;
using DeliveryTracker.Services;
using DeliveryTracker.TaskStates;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Controllers
{
    [Authorize(Policy = AuthPolicies.Performer)]
    [Route("api/performer")]
    public class PerformerController: Controller
    {
        #region fields
        
        private readonly PerformerService performerService;

        private readonly TaskService taskService;
        
        private readonly DeliveryTrackerDbContext dbContext;

        private readonly TaskStateCache taskStateCache;

        private readonly ILogger<PerformerController> logger;
        
        #endregion
        
        #region constructor
        
        public PerformerController(
            PerformerService performerService,
            DeliveryTrackerDbContext dbContext, 
            TaskStateCache taskStateCache,
            TaskService taskService, 
            ILogger<PerformerController> logger)
        {
            this.performerService = performerService;
            this.dbContext = dbContext;
            this.taskStateCache = taskStateCache;
            this.taskService = taskService;
            this.logger = logger;
        }
        
        #endregion 
        
        #region fields
        
        [HttpPost("update_position")]
        public async Task<IActionResult> UpdatePosition([FromBody] GeopositionViewModel newPosition)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }

            var updateResult = await this.performerService.UpdatePosition(
                this.User.Identity.Name,
                newPosition.Longitude,
                newPosition.Latitude);

            if (!updateResult.Success)
            {
                // Обработаем первую ошибку
                var error = updateResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }
            await this.dbContext.SaveChangesAsync();
            return this.Ok();
        }

        [HttpPost("inactive")]
        public async Task<IActionResult> Inactive()
        {
            var inactiveResult = await this.performerService.SetInactive(this.User.Identity.Name);

            if (!inactiveResult.Success)
            {
                // Обработаем первую ошибку
                var error = inactiveResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }

            await this.dbContext.SaveChangesAsync();
            return this.Ok();
        }

        [HttpPost("take_task_to_work")]
        public async Task<IActionResult> TakeTaskToWork([FromBody] TaskInfoViewModel taskInfo)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            
            var taskResult = await this.taskService.TakeTaskToWork(taskInfo.Id, this.User.Identity.Name);
            if (!taskResult.Success)
            {
                // Обработаем первую ошибку
                var error = taskResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound
                    || error.Code == ErrorCode.TaskNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserNotInRole
                    || error.Code == ErrorCode.TaskIsForbidden)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }
            var task = taskResult.Result;

            await this.dbContext.SaveChangesAsync();
            return this.Ok(new TaskInfoViewModel
            {
                Id = task.Id,
                Caption = task.Caption,
                TaskState = this.taskStateCache.GetById(task.StateId).Alias,
            });
        }

        [HttpPost("complete_task")]
        public async Task<IActionResult> CompleteTask([FromBody] TaskInfoViewModel taskInfo)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }

            var taskResult = await this.taskService.CompleteTaskByPerformer(
                this.User.Identity.Name,
                taskInfo.Id,
                taskInfo.TaskState);
            if (!taskResult.Success)
            {
                // Обработаем первую ошибку
                var error = taskResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound
                    || error.Code == ErrorCode.TaskNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserNotInRole
                    || error.Code == ErrorCode.TaskIsForbidden)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }
            var task = taskResult.Result;

            await this.dbContext.SaveChangesAsync();
            return this.Ok(new TaskInfoViewModel
            {
                Id = task.Id,
                Caption = task.Caption,
                TaskState = this.taskStateCache.GetById(task.StateId).Alias,
            });
        }

        [HttpGet("undistributed_tasks")]
        public async Task<IActionResult> GetUndistributedTasks(int? limitParam, int? offsetParam)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("limit", limitParam, p => p == null || p > 0)
                .AddRule("offset", offsetParam, p => p == null || p >= 0)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            var limit = limitParam ?? 100;
            var offset = offsetParam ?? 0;

            var myTasksResult =
                await this.taskService.GetUndistributedTasks(this.User.Identity.Name, limit, offset);
            if (!myTasksResult.Success)
            {
                // Обработаем первую ошибку
                var error = myTasksResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserNotInRole)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }

            var tasks = myTasksResult.Result
                .Select(p =>
                    new TaskPreviewViewModel
                    {
                        Id = p.Id,
                        TaskState = this.taskStateCache.GetById(p.StateId).Alias,
                        Caption = p.Caption,
                        ContentPreview = p.Content.Length > 50
                            ? p.Content.Substring(0, 50)
                            : p.Content,
                        SenderUserName = p.Sender.UserName,
                        SenderDisplayableName = p.Sender.DisplayableName,
                        PerformerUserName = p.Performer?.UserName,
                        PerformerDisplayableName = p.Performer?.DisplayableName,
                        CreationDate = p.CreationDate,
                        Deadline = p.Deadline,
                        InWorkDate = p.InWorkDate,
                        CompletionDate = p.CompletionDate,
                    });

            return this.Ok(tasks);
        }
        
        [HttpGet("my_tasks")]
        public async Task<IActionResult> GetMyTasks(int? limitParam, int? offsetParam)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("limit", limitParam, p => p == null || p <= 0)
                .AddRule("offset", offsetParam, p => p == null || p < 0)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            var limit = limitParam ?? 100;
            var offset = offsetParam ?? 0;

            var myTasksResult =
                await this.taskService.GetMyTasks(this.User.Identity.Name, limit, offset);
            if (!myTasksResult.Success)
            {
                // Обработаем первую ошибку
                var error = myTasksResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserNotInRole)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }

            var tasks = myTasksResult.Result
                .Select(p =>
                    new TaskPreviewViewModel
                    {
                        Id = p.Id,
                        TaskState = this.taskStateCache.GetById(p.StateId).Alias,
                        Caption = p.Caption,
                        ContentPreview = p.Content.Length > 50
                            ? p.Content.Substring(0, 50)
                            : p.Content,
                        SenderUserName = p.Sender.UserName,
                        SenderDisplayableName = p.Sender.DisplayableName,
                        PerformerUserName = p.Performer?.UserName,
                        PerformerDisplayableName = p.Performer?.DisplayableName,
                        CreationDate = p.CreationDate,
                        Deadline = p.Deadline,
                        InWorkDate = p.InWorkDate,
                        CompletionDate = p.CompletionDate,
                    });

            return this.Ok(tasks);
        }
        
        [HttpGet("task/{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("id", id, p => p != Guid.Empty)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }

            var taskResult =
                await this.taskService.GetTask(this.User.Identity.Name, id);
            if (!taskResult.Success)
            {
                // Обработаем первую ошибку
                var error = taskResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound
                    || error.Code == ErrorCode.TaskNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }
            var task = taskResult.Result;

            return this.Ok(new TaskDetailsViewModel
            {
                Id = task.Id,
                TaskState = this.taskStateCache.GetById(task.StateId).Alias,
                Caption = task.Caption,
                Content = task.Content,
                SenderUserName = task.Sender.UserName,
                SenderDisplayableName = task.Sender.DisplayableName,
                PerformerUserName = task.Performer?.UserName,
                PerformerDisplayableName = task.Performer?.DisplayableName,
                CreationDate = task.CreationDate,
                Deadline = task.Deadline,
                InWorkDate = task.InWorkDate,
                CompletionDate = task.CompletionDate,
            });
        }
        
        #endregion
        
    }
}