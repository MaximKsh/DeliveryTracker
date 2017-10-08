using System;
using System.Linq;
using DeliveryTracker.Db;
using DeliveryTracker.Helpers;
using DeliveryTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.TaskStates
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TaskStateCache
    {
        #region constants

        private const int CacheEntryExpirationMinutes = 60;

        private const string NewUndistributedStateAlias = TaskStateInfo.NewUndistributedState;
        private static readonly string NewUndistributedStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStateCache).FullName, NewUndistributedStateAlias);
        
        private const string NewStateAlias = TaskStateInfo.NewState;
        private static readonly string NewStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStateCache).FullName, NewStateAlias);
        
        private const string InWorkStateAlias = TaskStateInfo.InWorkState;
        private static readonly string InWorkStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStateCache).FullName, InWorkStateAlias);

        private const string PerformedStateAlias = TaskStateInfo.PerformedState;
        private static readonly string PerformedStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStateCache).FullName, PerformedStateAlias);

        private const string CancelledStateAlias = TaskStateInfo.CancelledState;
        private static readonly string CancelledStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStateCache).FullName, CancelledStateAlias);

        private const string CancelledByManagerStateAlias = TaskStateInfo.CancelledByManagerState;
        private static readonly string CancelledByManagerStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStateCache).FullName, CancelledByManagerStateAlias);
        
        #endregion

        #region fields

        private readonly DeliveryTrackerDbContext dbContext;

        private readonly ILogger<TaskStateCache> logger;

        private readonly IMemoryCache memoryCache;
        
        #endregion

        #region constuctor

        public TaskStateCache(
            DeliveryTrackerDbContext dbContext,
            ILogger<TaskStateCache> logger,
            IMemoryCache memoryCache)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        #endregion

        #region properties

        /// <summary>
        /// Состояние задания "новое нераспределено".
        /// </summary>
        public TaskStateModel NewUndistributedState => 
            this.GetState(NewUndistributedStateAlias, NewUndistributedStateCacheKey);
        
        /// <summary>
        /// Состояние задания "новое".
        /// </summary>
        public TaskStateModel NewState => 
            this.GetState(NewStateAlias, NewStateCacheKey);

        /// <summary>
        /// Состояния задания "выполняется".
        /// </summary>
        public TaskStateModel InWorkState => 
            this.GetState(InWorkStateAlias, InWorkStateCacheKey);

        /// <summary>
        /// Состояние задания "выполнено".
        /// </summary>
        public TaskStateModel PerformedState => 
            this.GetState(PerformedStateAlias, PerformedStateCacheKey);

        /// <summary>
        /// Состояние задания "отменено".
        /// </summary>
        public TaskStateModel CancelledState =>
            this.GetState(CancelledStateAlias, CancelledStateCacheKey);

        /// <summary>
        /// Состояние задания "отменено управляющим".
        /// </summary>
        public TaskStateModel CancelledByManagerState =>
            this.GetState(CancelledByManagerStateAlias, CancelledByManagerStateCacheKey);
        
        #endregion

        #region public 

        /// <summary>
        /// Получить модель состояния по ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TaskStateModel GetById(Guid id)
        {
            return this.TryGetById(id, out var taskState) 
                ? taskState
                : throw new ArgumentOutOfRangeException(nameof(id));
        }

        /// <summary>
        /// Получить модель состояния задания по ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskState"></param>
        /// <returns></returns>
        public bool TryGetById(Guid id, out TaskStateModel taskState)
        {
            taskState = null;
            if (id == this.NewUndistributedState.Id)
            {
                taskState = this.NewUndistributedState;
            }
            if (id == this.NewState.Id)
            {
                taskState = this.NewState;
            }
            if (id == this.InWorkState.Id)
            {
                taskState = this.InWorkState;
            }
            if (id == this.PerformedState.Id)
            {
                taskState = this.PerformedState;
            }
            if (id == this.CancelledState.Id)
            {
                taskState = this.CancelledState;
            }
            if (id == this.CancelledByManagerState.Id)
            {
                taskState = this.CancelledByManagerState;
            }
            return taskState != null;
        }
        
        /// <summary>
        /// Получить модель состояния задания по алиасу.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TaskStateModel GetByAlias(string alias)
        {
            return this.TryGetByAlias(alias, out var taskState) 
                ? taskState
                : throw new ArgumentOutOfRangeException(nameof(alias));
        }

        /// <summary>
        /// Получить модель состояния по алиасу
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool TryGetByAlias(string alias, out TaskStateModel model)
        {
            switch (alias)
            {
                case NewUndistributedStateAlias:
                    model = this.NewUndistributedState;
                    return true;
                case NewStateAlias:
                    model = this.NewState;
                    return true;
                case InWorkStateAlias:
                    model = this.InWorkState;
                    return true;
                case PerformedStateAlias:
                    model = this.PerformedState;
                    return true;
                case CancelledStateAlias:
                    model = this.CancelledState;
                    return true;
                case CancelledByManagerStateAlias:
                    model = this.CancelledByManagerState;
                    return true;
                default:
                    model = null;
                    return false;
            }
        }
        
        #endregion
        
        #region private

        private TaskStateModel GetState(string alias, string cacheKey)
        {
            if (this.memoryCache.TryGetValue(cacheKey, out var taskStateObj)
                && taskStateObj is TaskStateModel taskStateCached)
            {
                return taskStateCached;
            }

            var taskState = this.dbContext.TaskStates.FirstOrDefault(p => p.Alias == alias) 
                            ?? this.CreateNewState(alias);
            this.memoryCache.Set(
                cacheKey,
                taskState,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheEntryExpirationMinutes)));
                
            this.logger.LogTrace($"Task state '{alias}': cache updated.");
            return taskState;
        }

        private TaskStateModel CreateNewState(string alias)
        {
            var taskState = new TaskStateModel
            {
                Id = Guid.NewGuid(),
                Alias = alias,
            };
            taskState = this.dbContext.TaskStates.Add(taskState).Entity;
            this.dbContext.SaveChanges();
            this.logger.LogTrace($"Task state '{alias}' added in database.");
            return taskState;
        }
        
        #endregion
    }
}