namespace DeliveryTracker.Tasks
{
    public class TaskService
    {
        /*
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
        /// <param name="authorName"></param>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> AddTask(
            string authorName,
            TaskViewModel taskInfo)
        {
            var authorResult = await this.accountService.FindUser(authorName);
            if (!authorResult.Success)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.UserNotFound(authorName));
            }
            var sender = authorResult.Result;

            UserModel performer = null;
            if (taskInfo?.Performer?.Username != null)
            {
                var performerResult = await this.accountService.FindUser(taskInfo.Performer.Username);
                if (!performerResult.Success)
                {
                    return new ServiceResult<TaskModel>(ErrorFactory.UserNotFound(taskInfo.Performer.Username));
                }
                performer = performerResult.Result;
            }

            return await this.AddTask(sender, taskInfo, performer);
        }

        /// <summary>
        /// Добавить задание с указанными параметрами.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskInfo"></param>
        /// <param name="performer"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> AddTask(
            UserModel sender,
            TaskViewModel taskInfo,
            UserModel performer)
        {
            // Проверяем что переданный менеджер действительно менеджер
            var managerRoleResult = await this.accountService.GetUserRole(sender);
            if (!managerRoleResult.Success)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.UserWithoutRole(sender.UserName));
            }
            if (managerRoleResult.Result != this.roleCache.Manager.Name
                && managerRoleResult.Result != this.roleCache.Creator.Name)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.UserNotInRole(
                        sender.UserName, 
                        $"{this.roleCache.Creator.Name}, {this.roleCache.Manager.Name}"));
            }
            
            if (performer != null)
            {
                // Менеджер и исполнитель должны быть в одном инстансе
                if (sender.InstanceId != performer.InstanceId)
                {
                    return new ServiceResult<TaskModel>(ErrorFactory.PerformerInAnotherInstance());
                }
                // Проверяем что переданный исполнитель действительно исполнитель
                var performerRoleResult = await this.accountService.GetUserRole(performer);
                if (!performerRoleResult.Success)
                {
                    return new ServiceResult<TaskModel>(ErrorFactory.UserWithoutRole(performer.UserName));
                }
                if (performerRoleResult.Result != this.roleCache.Performer.Name)
                {
                    return new ServiceResult<TaskModel>(
                        ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
                }
            }
            var newStateId = performer != null
                ? this.taskStateCache.NewState.Id
                : this.taskStateCache.NewUndistributedState.Id;
            
            var newTask = new TaskModel
            {
                Id = Guid.NewGuid(),
                Number = taskInfo.Number ?? "",
                ShippingDesc = taskInfo.ShippingDesc,
                Details = taskInfo.Details,
                Address = taskInfo.Address,
                DatetimeFrom = taskInfo.TaskDateTimeRange?.From,
                DatetimeTo = taskInfo.TaskDateTimeRange?.To,
                StateId = newStateId,
                InstanceId = sender.InstanceId,
                AuthorId = sender.Id,
                PerformerId = performer?.Id,
                CreationDate = DateTime.UtcNow,
                SetPerformerDate = performer != null ? (DateTime?)DateTime.UtcNow : null,
            };
            var addTaskResult = this.dbContext.Tasks.Add(newTask);
            return new ServiceResult<TaskModel>(addTaskResult.Entity);
        }

        /// <summary>
        /// Зарезервировать задание за пользователем
        /// Переход NewUndistributed -> New
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="performerName"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> ReserveTask(
            Guid taskId,
            string performerName)
        {
            var currentUserResult = await this.accountService.FindUser(performerName);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.UserNotFound(performerName));
            }
            var user = currentUserResult.Result;
            
            var taskModel = await this.dbContext.Tasks.FindAsync(taskId);
            if (taskModel == null)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.TaskNotFound(taskId));
            }
            return await this.ReserveTask(taskModel, user);
        }

        /// <summary>
        /// Зарезервировать задание за пользователем
        /// Переход NewUndistributed -> New
        /// </summary>
        /// <param name="task"></param>
        /// <param name="performer"></param>
        /// <returns></returns>
        public async Task<ServiceResult<TaskModel>> ReserveTask(
            TaskModel task,
            UserModel performer)
        {
            if (task.InstanceId != performer.InstanceId)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.TaskIsForbidden());
            }
            var roleResult = await this.accountService.GetUserRole(performer);
            if (!roleResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
            }
            var role = roleResult.Result;
            if (role != this.roleCache.Performer.Name)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
            }
            
            if (!this.taskStateCache.TryGetById(task.StateId, out var oldState))
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.IncorrectTaskState(null, this.taskStateCache.NewUndistributedState));
            }
            if (oldState != this.taskStateCache.NewUndistributedState)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.IncorrectTaskStateTransition(this.taskStateCache.NewUndistributedState, oldState));
            }
            
            
            task.StateId = this.taskStateCache.NewState.Id;
            task.PerformerId = performer.Id;
            task.SetPerformerDate = DateTime.UtcNow;
            var result = this.dbContext.Tasks.Update(task);
            return new ServiceResult<TaskModel>(result.Entity);
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
                return new ServiceResult<TaskModel>(ErrorFactory.UserNotFound(performerName));
            }
            var user = currentUserResult.Result;
            
            var taskModel = await this.dbContext.Tasks.FindAsync(taskId);
            if (taskModel == null)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.TaskNotFound(taskId));
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
            if (task.InstanceId != performer.InstanceId
                || task.PerformerId != performer.Id)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.TaskIsForbidden());
            }
            var roleResult = await this.accountService.GetUserRole(performer);
            if (!roleResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
            }
            var role = roleResult.Result;
            if (role != this.roleCache.Performer.Name)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.UserNotInRole(performer.UserName, this.roleCache.Performer.Name));
            }
            
            if (!this.taskStateCache.TryGetById(task.StateId, out var oldState))
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.IncorrectTaskState(null, this.taskStateCache.NewState));
            }
            if (oldState != this.taskStateCache.NewState)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.IncorrectTaskStateTransition(this.taskStateCache.NewState, oldState));
            }
            
            
            task.StateId = this.taskStateCache.InWorkState.Id;
            task.PerformerId = performer.Id;
            task.InWorkDate = DateTime.UtcNow;
            var result = this.dbContext.Tasks.Update(task);
            return new ServiceResult<TaskModel>(result.Entity);
           
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
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            
            var task = await this.dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.TaskNotFound(taskId));
            }
            
            if (!this.taskStateCache.TryGetByAlias(newStateAlias, out var newState))
            {
                return new ServiceResult<TaskModel>(
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
                    ErrorFactory.IncorrectTaskState(
                        taskState, 
                        this.taskStateCache.PerformedState, 
                        this.taskStateCache.CancelledState));
            }
            
            if (!this.taskStateCache.TryGetById(task.StateId, out var oldState)
                || oldState != this.taskStateCache.InWorkState)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.IncorrectTaskStateTransition(taskState, oldState));
            }
            
            // Роль проверять необязательно, достаточно учесть тот факт,
            // что исполнителем может стоять только пользователь в роли исполнителя.
            if (task.PerformerId != user.Id)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.TaskIsForbidden());
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
                return new ServiceResult<TaskModel>(ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            
            var task = await this.dbContext.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.TaskNotFound(taskId));
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
            if (task.AuthorId != user.Id)
            {
                return new ServiceResult<TaskModel>(ErrorFactory.TaskIsForbidden());
            }
            
            if (!this.taskStateCache.TryGetById(task.StateId, out var oldState)
                || ( oldState != this.taskStateCache.NewUndistributedState 
                     && oldState != this.taskStateCache.NewState
                     && oldState != this.taskStateCache.InWorkState))
            {
                return new ServiceResult<TaskModel>(
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
            int limit,
            int offset)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(ErrorFactory.UserNotFound(username));
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
            int limit,
            int offset)
        {
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(ErrorFactory.UserWithoutRole(user.UserName));
            }
            var role = roleResult.Result;
            if (role == this.roleCache.Manager.Name
                || role == this.roleCache.Creator.Name)
            {
                return await this.GetMyTasksForManager(user, limit, offset);
            }
            if (role == this.roleCache.Performer.Name)
            {
                return await this.GetMyTasksForPerformer(user, limit, offset);
            }
            return new ServiceResult<List<TaskModel>>(ErrorFactory.UserWithoutRole(user.UserName));
        }
        
        /// <summary>
        /// Получить все задания.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<TaskModel>>> GetTasks(
            string username,
            int limit,
            int offset)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            return await this.GetTasks(user, limit, offset);
        }
        
        /// <summary>
        /// Получить все задания.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<TaskModel>>> GetTasks(
            UserModel user,
            int limit,
            int offset)
        {
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(ErrorFactory.UserWithoutRole(user.UserName));
            }
            var role = roleResult.Result;
            if (role == this.roleCache.Manager.Name
                || role == this.roleCache.Creator.Name)
            {
                return await this.GetTasksForManager(user, limit, offset);
            }
            if (role == this.roleCache.Performer.Name)
            {
                return new ServiceResult<List<TaskModel>>(ErrorFactory.AccessDenied());
            }
            return new ServiceResult<List<TaskModel>>(ErrorFactory.UserWithoutRole(user.UserName));
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
            int limit, 
            int offset)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<TaskModel>>(ErrorFactory.UserNotFound(username));
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
            int limit,
            int offset)
        {
            if (await this.accountService.IsInRole(user, this.roleCache.Performer.Name))
            {
                return await this.GetUndistributedTasksInternal(user, limit, offset);
            }
            return new ServiceResult<List<TaskModel>>(
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
         
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.UserWithoutRole(user.UserName));
            }
            var role = roleResult.Result;
            if (role == this.roleCache.Manager.Name
                || role == this.roleCache.Creator.Name)
            {
                return await this.GetTaskForManager(user, taskId);
            }
            if (role == this.roleCache.Performer.Name)
            {
                return await this.GetTaskForPerformer(user, taskId);
            }
            return new ServiceResult<TaskModel>(
                ErrorFactory.UserWithoutRole(user.UserName));
            
        }
        
        #endregion
        
        
        #region private

        private async Task<ServiceResult<List<TaskModel>>> GetTasksForManager(
            UserModel user,
            int limit,
            int offset)
        {
            IQueryable<TaskModel> tasks = this.dbContext.Tasks
                .Where(p => p.InstanceId == user.InstanceId)
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Author)
                .Include(p => p.Performer)
                .OrderBy(p => p.CreationDate);
            
            return new ServiceResult<List<TaskModel>>(await tasks.ToListAsync());
        }
        
        private async Task<ServiceResult<List<TaskModel>>> GetMyTasksForManager(
            UserModel user,
            int limit,
            int offset)
        {
            IQueryable<TaskModel> tasks = this.dbContext.Tasks
                .Where(p => p.InstanceId == user.InstanceId && p.AuthorId == user.Id)
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Author)
                .Include(p => p.Performer)
                .OrderBy(p => p.CreationDate);
            
            return new ServiceResult<List<TaskModel>>(await tasks.ToListAsync());
        }
        
        private async Task<ServiceResult<List<TaskModel>>> GetMyTasksForPerformer(
            UserModel user,
            int limit,
            int offset)
        {
            IQueryable<TaskModel> tasks = this.dbContext.Tasks
                .Where(p => p.InstanceId == user.InstanceId && p.PerformerId == user.Id)
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Author)
                .Include(p => p.Performer)
                .OrderBy(p => p.CreationDate);
            
            return new ServiceResult<List<TaskModel>>(await tasks.ToListAsync());
        }
        
        private async Task<ServiceResult<List<TaskModel>>> GetUndistributedTasksInternal(
            UserModel user,
            int limit,
            int offset)
        {
            IQueryable<TaskModel> tasks = this.dbContext.Tasks
                .Where(p => p.InstanceId == user.InstanceId
                            && p.StateId == this.taskStateCache.NewUndistributedState.Id)
                .Skip(offset)
                .Take(limit)
                .Include(p => p.Author)
                .Include(p => p.Performer)
                .OrderBy(p => p.CreationDate);
            
            return new ServiceResult<List<TaskModel>>(await tasks.ToListAsync());
        }


        private async Task<ServiceResult<TaskModel>> GetTaskForManager(
            UserModel user,
            Guid taskId)
        {
            var task = await this.dbContext.Tasks
                .Include(p => p.Author)
                .Include(p => p.Performer)
                .FirstOrDefaultAsync(p => p.Id == taskId && p.InstanceId == user.InstanceId);

            if (task == null)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.TaskNotFound(taskId));
            }
            if (task.AuthorId != user.Id)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.TaskIsForbidden());
            }
            
            return new ServiceResult<TaskModel>(task);
        }
        
        private async Task<ServiceResult<TaskModel>> GetTaskForPerformer(
            UserModel user,
            Guid taskId)
        {
            var task = await this.dbContext.Tasks
                .Include(p => p.Author)
                .Include(p => p.Performer)
                .FirstOrDefaultAsync(p => p.Id == taskId && p.InstanceId == user.InstanceId);

            if (task == null)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.TaskNotFound(taskId));
            }
            if (task.PerformerId.HasValue 
                && task.PerformerId != user.Id)
            {
                return new ServiceResult<TaskModel>(
                    ErrorFactory.TaskIsForbidden());
            }
            
            return new ServiceResult<TaskModel>(task);
        }
        
        #endregion
        */
    }
}