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
        public async Task<IActionResult> AddTask([FromBody] TaskViewModel addTaskViewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("number", addTaskViewModel.Number, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }
            
            var result = await this.taskService.AddTask(
                this.User.Identity.Name,
                addTaskViewModel);
            if (!result.Success)
            {
                var error = result.Errors.First();
                if (error.Code == ErrorCode.UserNotFound)
                {
                    return this.NotFound(error.ToErrorListViewModel());
                }
                if (error.Code == ErrorCode.UserNotInRole
                    || error.Code == ErrorCode.UserWithoutRole
                    || error.Code == ErrorCode.PerformerInAnotherInstance)
                {
                    return this.StatusCode(403, error.ToErrorListViewModel());
                }
                return this.BadRequest(error.ToErrorListViewModel());
            }
                
            await this.dbContext.SaveChangesAsync();
            return this.StatusCode(
                201,
                new TaskViewModel
                {
                    Id = result.Result.Id,
                    State = this.taskStateCache.GetById(result.Result.StateId).Alias,
                });
        }
        
        [HttpPost("cancel_task")]
        public async Task<IActionResult> CancelTask([FromBody] TaskViewModel taskInfo)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("id", taskInfo.Id, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error);
            }

            var taskResult = await this.taskService.CancelTaskByManager(
                this.User.Identity.Name,
                // Проверено ParametersValidator
                // ReSharper disable once PossibleInvalidOperationException
                taskInfo.Id.Value);
            
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
            return this.Ok(new TaskViewModel
            {
                Id = task.Id,
                State = this.taskStateCache.GetById(task.StateId).Alias,
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
                    new UserViewModel
                    {
                        Username = p.UserName,
                        Surname = p.Surname,
                        Name = p.Name,
                        PhoneNumber = p.PhoneNumber,
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
                    new TaskViewModel
                    {
                        Id = p.Id,
                        Number = p.Number,
                        ShippingDesc = p.ShippingDesc,
                        Details = p.Details,
                        Address = p.Address,
                        Author = new UserViewModel
                        {
                            Username = p.Author.UserName,
                            Name = p.Author.Name,
                            Surname = p.Author.Surname,
                            PhoneNumber = p.Author.PhoneNumber,
                        },
                        Performer = p.Performer != null 
                            ? new UserViewModel
                            {
                                Username = p.Performer.UserName,
                                Name = p.Performer.Name,
                                Surname = p.Performer.Surname,
                                PhoneNumber = p.Performer.PhoneNumber,
                            }
                            : null,
                        TaskDateTimeRange = new DateTimeRangeViewModel
                        {
                            From = p.DatetimeFrom,
                            To = p.DatetimeTo,
                        },
                        State = this.taskStateCache.GetById(p.StateId).Alias,
                        CreationDate = p.CreationDate,
                        SetPerformerDate = p.SetPerformerDate,
                        InWorkDate = p.InWorkDate,
                        CompletionDate = p.CompletionDate,
                    });

            return this.Ok(tasks);
        }
        
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks(int? limitParam, int? offsetParam)
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
                    new TaskViewModel
                    {
                        Id = p.Id,
                        Number = p.Number,
                        ShippingDesc = p.ShippingDesc,
                        Details = p.Details,
                        Address = p.Address,
                        Author = new UserViewModel
                        {
                            Username = p.Author.UserName,
                            Name = p.Author.Name,
                            Surname = p.Author.Surname,
                            PhoneNumber = p.Author.PhoneNumber,
                        },
                        Performer = p.Performer != null 
                            ? new UserViewModel
                            {
                                Username = p.Performer.UserName,
                                Name = p.Performer.Name,
                                Surname = p.Performer.Surname,
                                PhoneNumber = p.Performer.PhoneNumber,
                            }
                            : null,
                        TaskDateTimeRange = new DateTimeRangeViewModel
                        {
                            From = p.DatetimeFrom,
                            To = p.DatetimeTo,
                        },
                        State = this.taskStateCache.GetById(p.StateId).Alias,
                        CreationDate = p.CreationDate,
                        SetPerformerDate = p.SetPerformerDate,
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

            return this.Ok(new TaskViewModel
            {
                Id = task.Id,
                Number = task.Number,
                ShippingDesc = task.ShippingDesc,
                Details = task.Details,
                Address = task.Address,
                Author = new UserViewModel
                {
                    Username = task.Author.UserName,
                    Name = task.Author.Name,
                    Surname = task.Author.Surname,
                    PhoneNumber = task.Author.PhoneNumber,
                },
                Performer = task.Performer != null 
                    ? new UserViewModel
                    {
                        Username = task.Performer.UserName,
                        Name = task.Performer.Name,
                        Surname = task.Performer.Surname,
                        PhoneNumber = task.Performer.PhoneNumber,
                    }
                    : null,
                TaskDateTimeRange = new DateTimeRangeViewModel
                {
                    From = task.DatetimeFrom,
                    To = task.DatetimeTo,
                },
                State = this.taskStateCache.GetById(task.StateId).Alias,
                CreationDate = task.CreationDate,
                SetPerformerDate = task.SetPerformerDate,
                InWorkDate = task.InWorkDate,
                CompletionDate = task.CompletionDate,
            });
        }
        
        #endregion
        
    }
}