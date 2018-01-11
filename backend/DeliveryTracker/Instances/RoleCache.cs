using System;
using System.Linq;
using DeliveryTracker.Common;
using DeliveryTracker.DbModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Instances
{
    public class RoleCache
    {
        #region constants

        private const int CacheEntryExpirationMinutes = 60;
        
        private const string CreatorName = RoleAlias.Creator;
        private static readonly string CreatorRoleCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(RoleCache).FullName, CreatorName);

        private const string ManagerName = RoleAlias.Manager;
        private static readonly string ManagerRoleCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(RoleCache).FullName, ManagerName);

        private const string PerformerName = RoleAlias.Performer;
        private static readonly string PerformerRoleCacheKey = 
            string.Format(CacheHelper.CacheKeyFormat, typeof(RoleCache).FullName, PerformerName);

        #endregion

        #region fields

        private readonly RoleManager<RoleModel> roleManager;

        private readonly ILogger<RoleCache> logger;

        private readonly IMemoryCache memoryCache;
        
        #endregion

        #region constuctor

        public RoleCache(
            RoleManager<RoleModel> roleManager, 
            ILogger<RoleCache> logger,
            IMemoryCache memoryCache)
        {
            this.roleManager = roleManager;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        #endregion

        #region properties

        /// <summary>
        /// Роль "Создатель"
        /// </summary>
        public RoleModel Creator => this.GetRole(CreatorName, CreatorRoleCacheKey);

        /// <summary>
        /// Роль "Управляющий"
        /// </summary>
        public RoleModel Manager => this.GetRole(ManagerName, ManagerRoleCacheKey);

        /// <summary>
        /// Роль "Исполнитель"
        /// </summary>
        public RoleModel Performer => this.GetRole(PerformerName, PerformerRoleCacheKey);

        #endregion

        #region private

        private RoleModel GetRole(string name, string cacheKey)
        {
            if (this.memoryCache.TryGetValue(cacheKey, out var roleObj)
                && roleObj is RoleModel roleModelCached)
            {
                return roleModelCached;
            }

            var role = this.roleManager.Roles.FirstOrDefault(p => p.Name == name) ?? this.CreateNewRole(name);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheEntryExpirationMinutes));
            this.memoryCache.Set(cacheKey, role, cacheEntryOptions);
                
            this.logger.LogTrace($"Role {name}: cache updated.");
            return role;
        }

        private RoleModel CreateNewRole(string name)
        {
            var role = new RoleModel
            {
                Id = Guid.NewGuid(),
                Name = name
            };
            this.roleManager.CreateAsync(role).Wait();
            role = this.roleManager.Roles.FirstOrDefault(p => p.Name == name);
            this.logger.LogTrace($"Role {name} added in database.");
            return role;
        }
        
        #endregion
    }
}