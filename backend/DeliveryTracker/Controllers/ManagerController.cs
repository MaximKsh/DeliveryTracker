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
    
    [Authorize(Policy = AuthPolicies.Manager)]
    [Route("api/manager")]
    public class ManagerController: Controller
    {
        #region fields

        private readonly TaskService taskService;

        private readonly DeliveryTrackerDbContext dbContext;

        private readonly TaskStateCache taskStateCache;

        private readonly PerformerService performerService;

        private readonly ILogger<ManagerController> logger;
        
        #endregion
        
        #region constructors
        
        public ManagerController(
            TaskService taskService,
            DeliveryTrackerDbContext dbContext,
            TaskStateCache taskStateCache, 
            PerformerService performerService,
            ILogger<ManagerController> logger)
        {
            this.taskService = taskService;
            this.dbContext = dbContext;
            this.taskStateCache = taskStateCache;
            this.performerService = performerService;
            this.logger = logger;
        }
        
        #endregion

        #region actions

        
        [HttpPost("add_task")]
        public async Task<IActionResult> AddTask([FromBody] AddTaskViewModel addTaskViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }

            var result = await this.taskService.AddTask(
                this.User.Identity.Name,
                addTaskViewModel.Caption,
                addTaskViewModel.Content, 
                addTaskViewModel.PerformerUserName, 
                addTaskViewModel.DeadlineDate);
            if (!result.Success)
            {
                var error = result.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserNotInRole
                    || error.Code == ErrorCode.UserWithoutRole
                    || error.Code == ErrorCode.PerformerInAnotherGroup)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }
                
            await this.dbContext.SaveChangesAsync();
            return this.StatusCode(
                201,
                new TaskInfoViewModel
                {
                    Id = result.Result.Id,
                    Caption = result.Result.Caption,
                    TaskState = this.taskStateCache.GetById(result.Result.StateId).Alias,
                });
        }
        
        [HttpPost("cancel_task")]
        public async Task<IActionResult> CancelTask([FromBody] TaskInfoViewModel taskInfo)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }

            var taskResult = await this.taskService.CancelTaskByManager(
                this.User.Identity.Name,
                taskInfo.Id);
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
        
        [HttpGet("available_performers")]
        public async Task<IActionResult> GetAvailablePerformers(int? limitParam, int? offsetParam)
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

            var availablePerformersResult =
                await this.performerService.GetAvailablePerformers(this.User.Identity.Name, limit, offset);
            if (!availablePerformersResult.Success)
            {
                // Обработаем первую ошибку
                var error = availablePerformersResult.Errors.First();
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

            var users = availablePerformersResult.Result
                .Select(p =>
                    new UserInfoViewModel
                    {
                        UserName = p.UserName,
                        DisplayableName = p.DisplayableName,
                        Position = new GeopositionViewModel
                        {
                            Latitude = p.Latitude ?? 0.0,
                            Longitude = p.Longitude ?? 0.0,
                        }
                    });

            return this.Ok(users);
        }

        [HttpGet("my_tasks")]
        public async Task<IActionResult> GetMyTasks(int? limitParam, int? offsetParam)
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
                await this.taskService.GetMyTasks(this.User.Identity.Name, limit, offset);
            if (!myTasksResult.Success)
            {
                // Обработаем первую ошибку
                var error = myTasksResult.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserWithoutRole)
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
                if (error.Code == ErrorCode.TaskIsForbidden
                    || error.Code == ErrorCode.UserWithoutRole)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
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