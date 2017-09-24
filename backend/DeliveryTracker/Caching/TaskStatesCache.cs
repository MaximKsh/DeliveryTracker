using System;
using System.Linq;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Caching
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TaskStatesCache
    {
        #region constants

        private const int CacheEntryExpirationMinutes = 60;

        private const string NewStateAlias = "TaskState_New";
        private const string NewStateCaption = "LocMe: новое задание";
        private static readonly string NewStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStatesCache).FullName, NewStateAlias);
        
        private const string InWorkStateAlias = "TaskState_InWork";
        private const string InWorkStateCaption = "LocMe: в работе";
        private static readonly string InWorkStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStatesCache).FullName, InWorkStateAlias);
        

        private const string PerformedStateAlias = "TaskState_Performed";
        private const string PerformedStateCaption = "LocMe: выполнено";
        private static readonly string PerformedStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStatesCache).FullName, PerformedStateAlias);

        private const string CancelledStateAlias = "TaskState_Cancelled";
        private const string CancelledStateCaption = "LocMe: отменено";
        private static readonly string CancelledStateCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(TaskStatesCache).FullName, CancelledStateAlias);

        #endregion

        #region fields

        private readonly DeliveryTrackerDbContext dbContext;

        private readonly ILogger<TaskStatesCache> logger;

        private readonly IMemoryCache memoryCache;
        
        #endregion

        #region constuctor

        public TaskStatesCache(
            DeliveryTrackerDbContext dbContext,
            ILogger<TaskStatesCache> logger,
            IMemoryCache memoryCache)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        #endregion

        #region properties

        /// <summary>
        /// Состояние задания "новое".
        /// </summary>
        public TaskStateModel NewState => 
            this.GetState(NewStateAlias, NewStateCaption, NewStateCacheKey);

        /// <summary>
        /// Состояния задания "выполняется".
        /// </summary>
        public TaskStateModel InWorkState => 
            this.GetState(InWorkStateAlias, InWorkStateCaption, InWorkStateCacheKey);

        /// <summary>
        /// Состояние задания "выполнено".
        /// </summary>
        public TaskStateModel PerformedState => 
            this.GetState(PerformedStateAlias, PerformedStateCaption, PerformedStateCacheKey);

        /// <summary>
        /// Состояние задания "отменено".
        /// </summary>
        public TaskStateModel CancelledState =>
            this.GetState(CancelledStateAlias, CancelledStateCaption, CancelledStateCacheKey);

        #endregion

        #region private

        private TaskStateModel GetState(string alias, string caption, string cacheKey)
        {
            if (this.memoryCache.TryGetValue(cacheKey, out var taskStateObj)
                && taskStateObj is TaskStateModel taskStateCached)
            {
                return taskStateCached;
            }

            var taskState = this.dbContext.TaskStates.FirstOrDefault(p => p.Alias == alias) 
                            ?? this.CreateNewState(alias, caption);
            this.memoryCache.Set(
                cacheKey,
                taskState,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheEntryExpirationMinutes)));
                
            this.logger.LogTrace($"Task state {alias}: cache updated.");
            return taskState;
        }

        private TaskStateModel CreateNewState(string alias, string caption)
        {
            var taskState = new TaskStateModel
            {
                Id = Guid.NewGuid(),
                Alias = alias,
                Caption = caption,
            };
            taskState = this.dbContext.TaskStates.Add(taskState).Entity;
            this.dbContext.SaveChanges();
            this.logger.LogTrace($"Task state {alias} added in database.");
            return taskState;
        }
        
        #endregion
    }
}