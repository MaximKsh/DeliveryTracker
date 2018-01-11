using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.DbModels;
using DeliveryTracker.Instances;
using DeliveryTracker.Services;
using DeliveryTracker.Tasks;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using DeliveryTrackerWeb.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryTrackerWeb.Controllers
{
    [Authorize(Policy = AuthPolicies.Performer)]
    [Route("api/performer")]
    public class PerformerController: Controller
    {
        #region fields
        
        private readonly PerformerService performerService;

        private readonly TaskService taskService;
        
        private readonly PushMessageService pushMessageService;

        private readonly AccountService accountService;
        
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
            PushMessageService pushMessageService,
            AccountService accountService,
            ILogger<PerformerController> logger)
        {
            this.performerService = performerService;
            this.dbContext = dbContext;
            this.taskStateCache = taskStateCache;
            this.taskService = taskService;
            this.pushMessageService = pushMessageService;
            this.accountService = accountService;
            this.logger = logger;
        }
        
        #endregion 
        
        #region fields
        
        [HttpPost("update_position")]
        public async Task<IActionResult> UpdatePosition([FromBody] GeopositionViewModel newPosition)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("newPosition", newPosition, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
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
        
        [HttpPost("reserve_task")]
        public async Task<IActionResult> ReserveTask([FromBody] TaskViewModel taskInfo)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("id", taskInfo?.Id, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
            }
            
            var taskResult = await this.taskService.ReserveTask(
                // ReSharper disable once PossibleInvalidOperationException
                // ReSharper disable once PossibleNullReferenceException
                taskInfo.Id.Value,
                this.User.Identity.Name);
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

        [HttpPost("take_task_to_work")]
        public async Task<IActionResult> TakeTaskToWork([FromBody] TaskViewModel taskInfo)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("taskInfo", taskInfo, p => p != null)
                .AddRule("id", taskInfo?.Id, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
            }
            
            var taskResult = await this.taskService.TakeTaskToWork(
                // ReSharper disable once PossibleInvalidOperationException
                // ReSharper disable once PossibleNullReferenceException
                taskInfo.Id.Value,
                this.User.Identity.Name);
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

        [HttpPost("complete_task")]
        public async Task<IActionResult> CompleteTask([FromBody] TaskViewModel taskInfo)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("taskInfo", taskInfo, p => p != null)
                .AddRule("id", taskInfo?.Id, p => p != null)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
            }
            
            var taskResult = await this.taskService.CompleteTaskByPerformer(
                this.User.Identity.Name,
                // ReSharper disable once PossibleInvalidOperationException
                // ReSharper disable once PossibleNullReferenceException
                taskInfo.Id.Value,
                taskInfo.State);
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

        [HttpGet("undistributed_tasks")]
        public async Task<IActionResult> GetUndistributedTasks(int? limitParam, int? offsetParam)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("limit", limitParam, p => p == null || p > 0)
                .AddRule("offset", offsetParam, p => p == null || p >= 0)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
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
        
        [HttpGet("my_tasks")]
        public async Task<IActionResult> GetMyTasks(int? limitParam, int? offsetParam)
        {
            var validateQueryParametersResult = new ParametersValidator()
                .AddRule("limit", limitParam, p => p == null || p <= 0)
                .AddRule("offset", offsetParam, p => p == null || p < 0)
                .Validate();
            if (!validateQueryParametersResult.Success)
            {
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
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
                return this.BadRequest(validateQueryParametersResult.Error.ToErrorListViewModel());
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
                if (error.Code == ErrorCode.UserWithoutRole)
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