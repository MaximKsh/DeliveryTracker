using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.Roles;
using DeliveryTracker.TaskStates;
using DeliveryTracker.Validation;
using Microsoft.EntityFrameworkCore;

namespace DeliveryTracker.Services
{
    public class TaskService
    {
        #region fields

        private readonly TaskStateCache taskStateCache;
        
        private readonly DeliveryTrackerDbContext dbContext;

        private readonly AccountService accountService;

        private readonly RoleCache roleCache;
        
        #endregion
        
        #region constructor
        
        public TaskService(
            TaskStateCache taskStateCache,
            DeliveryTrackerDbContext dbContext, 
            AccountService accountService,
            RoleCache roleCache)
        {
            this.taskStateCache = taskStateCache;
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.roleCache = roleCache;
        }
        
        #endregion
        
        #region public

        /// <summary>
        /// Добавить задание с указанными параметрами.
        /// </summary>
        /// <param name="senderUserName"></param>
        /// <param name="caption"></param>
        /// <param name="content"></param>
        /// <param name="performerUserName"></param>
        /// <param name="deadlineDate"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> AddTask(
            string senderUserName,
            string caption,
            string content,
            string performerUserName,
            DateTime? deadlineDate)
        {
            var senderResult = await this.accountService.FindUser(senderUserName);
            if (!senderResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotFound(senderUserName));
            }
            var sender = senderResult.Result;

            UserModel performer = null;
            if (performerUserName != null)
            {
                var performerResult = await this.accountService.FindUser(performerUserName);
                if (!performerResult.Success)
                {
                    return new ServiceResult<TaskModel>(
                        null,
                        ErrorFactory.UserNotFound(performerUserName));
                }
                performer = performerResult.Result;
            }

            return await this.AddTask(sender, caption, content, performer, deadlineDate);
        }
        
        /// <summary>
        /// Добавить задание с указанными параметрами.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="caption"></param>
        /// <param name="content"></param>
        /// <param name="performer"></param>
        /// <param name="deadlineDate"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> AddTask(
            UserModel sender,
            string caption,
            string content,
            UserModel performer,
            DateTime? deadlineDate)
        {
            // Проверяем что переданный менеджер действительно менеджер
            var managerRoleResult = await this.accountService.GetUserRole(sender);
            if (!managerRoleResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserWithoutRole(sender.UserName));
            }
            if (managerRoleResult.Result != this.roleCache.Manager.Name)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotInRole(sender.UserName, this.roleCache.Manager.Name));
            }
            
            if (performer != null)
            {
                // Менеджер и исполнитель должны быть в одной группе
                if (sender.GroupId != performer.GroupId)
                {
                    return new ServiceResult<TaskModel>(
                        null,
                        ErrorFactory.PerformerInAnotherGroup());
                }
                // Проверяем что переданный исполнитель действительно исполнитель
                var performerRoleResult = await this.accountService.GetUserRole(performer);
                if (!performerRoleResult.Success)
                {
                    return new ServiceResult<TaskModel>(
                        null,
                        ErrorFactory.UserWithoutRole(performer.UserName));
                }
                if (performerRoleResult.Result != this.roleCache.Performer.Name)
                {
                    return new ServiceResult<TaskModel>(
                        null,
                        ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
                }
            }
            var newStateId = performer != null
                ? this.taskStateCache.NewState.Id
                : this.taskStateCache.NewUndistributedState.Id;
            
            var newTask = new TaskModel
            {
                Id = Guid.NewGuid(),
                StateId = newStateId,
                GroupId = sender.GroupId,
                Caption = caption,
                Content = content,
                SenderId = sender.Id,
                PerformerId = performer?.Id,
                CreationDate = DateTime.UtcNow,
                Deadline = deadlineDate,
            };
            var addTaskResult = this.dbContext.Tasks.Add(newTask);
            return new ServiceResult<TaskModel>(addTaskResult.Entity);
        }

        /// <summary>
        /// Взять задание в работу.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="performerName"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> TakeTaskToWork(
            Guid taskId,
            string performerName)
        {
            var currentUserResult = await this.accountService.FindUser(performerName);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotFound(performerName));
            }
            var user = currentUserResult.Result;
            
            var taskModel = await this.dbContext.Tasks.FindAsync(taskId);
            if (taskModel == null)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskNotFound(taskId));
            }
            
            return await this.TakeTaskToWork(taskModel, user);
        }

        /// <summary>
        /// Взять задание в работу.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="performer"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> TakeTaskToWork(
            TaskModel task,
            UserModel performer)
        {
            var roleResult = await this.accountService.GetUserRole(performer);
            if (!roleResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
            }
            var role = roleResult.Result;
            if (role != this.roleCache.Performer.Name)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
            }
            
            if (!this.taskStateCache.TryGetById(task.StateId, out var oldState))
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.IncorrectTaskState(
                        null,
                        this.taskStateCache.NewUndistributedState,
                        this.taskStateCache.NewState));
            }
            if (task.GroupId != performer.GroupId)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskIsForbidden());
            }
            

            if (oldState == this.taskStateCache.NewUndistributedState)
            {
                task.StateId = this.taskStateCache.InWorkState.Id;
                task.PerformerId = performer.Id;
                task.InWorkDate = DateTime.UtcNow;
                var result = this.dbContext.Tasks.Update(task);
                return new ServiceResult<TaskModel>(result.Entity);
            }
            if (oldState == this.taskStateCache.NewState)
            {
                if (task.PerformerId == performer.Id)
                {
                    task.StateId = this.taskStateCache.InWorkState.Id;
                    task.PerformerId = performer.Id;
                    task.InWorkDate = DateTime.UtcNow;
                    var result = this.dbContext.Tasks.Update(task);
                    return new ServiceResult<TaskModel>(result.Entity);
                }
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskIsForbidden());
            }
            return new ServiceResult<TaskModel>(
                null,
                ErrorFactory.IncorrectTaskStateTransition(this.taskStateCache.InWorkState, oldState));
           
        }

        /// <summary>
        /// Завершение задания исполнителем.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="taskId"></param>
        /// <param name="newStateAlias"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> CompleteTaskByPerformer(
            string username,
            Guid taskId,
            string newStateAlias)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            
            var task = await this.dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskNotFound(taskId));
            }
            
            if (!this.taskStateCache.TryGetByAlias(newStateAlias, out var newState))
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.IncorrectTaskState(
                        null,
                        this.taskStateCache.PerformedState,
                        this.taskStateCache.CancelledState));
            }
            return this.CompleteTaskByPerformer(user, task, newState);
        }
        
        /// <summary>
        /// Завершение задания исполнителем.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="task"></param>
        /// <param name="taskState"></param>
        /// <returns></returns>
        public ServiceResult<TaskModel> CompleteTaskByPerformer(
            UserModel user,
            TaskModel task,
            TaskStateModel taskState)
        {
            if (taskState != this.taskStateCache.PerformedState
                && taskState != this.taskStateCache.CancelledState)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.IncorrectTaskState(
                        taskState, 
                        this.taskStateCache.PerformedState, 
                        this.taskStateCache.CancelledState));
            }
            
            if (!this.taskStateCache.TryGetById(task.StateId, out var oldState)
                || oldState != this.taskStateCache.InWorkState)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.IncorrectTaskStateTransition(taskState, oldState));
            }
            
            // Роль проверять необязательно, достаточно учесть тот факт,
            // что исполнителем может стоять только пользователь в роли исполнителя.
            if (task.PerformerId != user.Id)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskIsForbidden());
            }
            
            task.StateId = taskState.Id;
            task.CompletionDate = DateTime.UtcNow;
            var result = this.dbContext.Tasks.Update(task);
            return new ServiceResult<TaskModel>(result.Entity);
        }

        /// <summary>
        /// Отменить задание автором задания.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> CancelTaskByManager(
            string username,
            Guid taskId)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            
            var task = await this.dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskNotFound(taskId));
            }
            return this.CancelTaskByManager(user, task);
        }

        /// <summary>
        /// Завершение задания исполнителем.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="task"></param>
        /// <returns>Модель задания</returns>
        public ServiceResult<TaskModel> CancelTaskByManager(
            UserModel user,
            TaskModel task)
        {
            // Роль проверять необязательно, достаточно учесть тот факт,
            // что автором может стоять только человек в роли менеджера.
            if (task.SenderId != user.Id)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskIsForbidden());
            }
            
            if (!this.taskStateCache.TryGetById(task.StateId, out var oldState)
                || ( oldState != this.taskStateCache.NewUndistributedState 
                     && oldState != this.taskStateCache.NewState
                     && oldState != this.taskStateCache.InWorkState))
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.IncorrectTaskStateTransition(
                        this.taskStateCache.CancelledByManagerState, 
                        oldState));
            }
            
            task.StateId = this.taskStateCache.CancelledByManagerState.Id;
            task.CompletionDate = DateTime.UtcNow;
            var result = this.dbContext.Tasks.Update(task);
            return new ServiceResult<TaskModel>(result.Entity);
        }

        /// <summary>
        /// Получить задания, отправленные или исполняемые пользователем(в зависимости от его роли).
        /// </summary>
        /// <param name="username"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<TaskModel>>> GetMyTasks(
            string username,
            int offset,
            int limit)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            return await this.GetMyTasks(user, limit, offset);
        }
        
        /// <summary>
        /// Получить задания, отправленные или исполняемые пользователем(в зависимости от его роли).
        /// </summary>
        /// <param name="user"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<TaskModel>>> GetMyTasks(
            UserModel user,
            int offset,
            int limit)
        {
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(
                    null,
                    ErrorFactory.UserWithoutRole(user.UserName));
            }
            var role = roleResult.Result;
            if (role == this.roleCache.Manager.Name)
            {
                return await this.GetMyTasksForManager(user, offset, limit);
            }
            if (role == this.roleCache.Performer.Name)
            {
                return await this.GetMyTasksForPerformer(user, offset, limit);
            }
            return new ServiceResult<List<TaskModel>>(
                null,
                ErrorFactory.UserWithoutRole(user.UserName));
        }
        
        /// <summary>
        /// Получить нераспределенные задания для исполнителя.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<TaskModel>>> GetUndistributedTasks(
            string username,
            int offset,
            int limit)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            return await this.GetUndistributedTasks(user, limit, offset);
        }
        
        /// <summary>
        /// Получить нераспределенные задания для исполнителя.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<TaskModel>>> GetUndistributedTasks(
            UserModel user,
            int offset,
            int limit)
        {
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(
                    null,
                    ErrorFactory.UserWithoutRole(user.UserName));
            }
            var role = roleResult.Result;
            if (role == this.roleCache.Performer.Name)
            {
                return await this.GetUndistributedTasksInternal(user, offset, limit);
            }
            return new ServiceResult<List<TaskModel>>(
                null,
                ErrorFactory.UserNotInRole(user.UserName, this.roleCache.Performer.Name));
        }

        /// <summary>
        /// Получить задание по его ID.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> GetTask(
            string username,
            Guid taskId)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            return await this.GetTask(user, taskId);
        }
        
        /// <summary>
        /// Получить задание по его ID.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> GetTask(
            UserModel user,
            Guid taskId)
        {
            var task = await this.dbContext.Tasks
                .Include(p => p.Sender)
                .Include(p => p.Performer)
                .FirstOrDefaultAsync(p => p.Id == taskId && p.GroupId == user.GroupId);

            if (task == null)
            {
                return new ServiceResult<TaskModel>(
                    null,
                    ErrorFactory.TaskNotFound(taskId));
            }
            return new ServiceResult<TaskModel>(task);
        }
        
        #endregion
        
        
        #region private

        private async Task<ServiceResult<List<TaskModel>>> GetMyTasksForManager(
            UserModel user,
            int offset,
            int limit)
        {
            IQueryable<TaskModel> tasks = this.dbContext.Tasks
                .Where(p => p.GroupId == user.GroupId && p.SenderId == user.Id)
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Sender)
                .Include(p => p.Performer)
                .OrderBy(p => p.CreationDate);
            
            return new ServiceResult<List<TaskModel>>(await tasks.ToListAsync());
        }
        
        private async Task<ServiceResult<List<TaskModel>>> GetMyTasksForPerformer(
            UserModel user,
            int offset,
            int limit)
        {
            IQueryable<TaskModel> tasks = this.dbContext.Tasks
                .Where(p => p.GroupId == user.GroupId && p.PerformerId == user.Id)
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Sender)
                .Include(p => p.Performer)
                .OrderBy(p => p.CreationDate);
            
            return new ServiceResult<List<TaskModel>>(await tasks.ToListAsync());
        }
        
        private async Task<ServiceResult<List<TaskModel>>> GetUndistributedTasksInternal(
            UserModel user,
            int offset,
            int limit)
        {
            IQueryable<TaskModel> tasks = this.dbContext.Tasks
                .Where(p => p.GroupId == user.GroupId
                            && p.StateId == this.taskStateCache.NewUndistributedState.Id)
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Sender)
                .Include(p => p.Performer)
                .OrderBy(p => p.CreationDate);
            
            return new ServiceResult<List<TaskModel>>(await tasks.ToListAsync());
        }
        
        #endregion
    }
}