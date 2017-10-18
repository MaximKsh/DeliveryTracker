using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.Roles;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Services
{
    public class PerformerService
    {
        private readonly DeliveryTrackerDbContext dbContext;

        private readonly AccountService accountService;

        private readonly RoleCache roleCache;
        
        public PerformerService(
            DeliveryTrackerDbContext dbContext, 
            AccountService accountService,
            RoleCache roleCache)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.roleCache = roleCache;
        }

        #region public
        
        /// <summary>
        /// Обновить позицию исполнителя. 
        /// Если до этого позиция не была указана, то установка положения
        /// считается переводом в активное состояние
        /// </summary>
        /// <param name="username">Имя(уникальное) исполнителя</param>
        /// <param name="longitude">долгота</param>
        /// <param name="latitude">широта</param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> UpdatePosition(
            string username,
            double longitude, 
            double latitude)
        {
            var userResult = await this.accountService.FindUser(username, false);
            if (!userResult.Success)
            {
                return userResult;
            }
            var user = userResult.Result;
            return this.UpdateCoordinatesInternal(user, longitude, latitude);
        }
        
        /// <summary>
        /// Обновить позицию исполнителя. 
        /// Если до этого позиция не была указана, то установка положения
        /// считается переводом в активное состояние
        /// </summary>
        /// <param name="user">Модель пользователя</param>
        /// <param name="longitude">долгота</param>
        /// <param name="latitude">широта</param>
        /// <returns></returns>
        public ServiceResult<UserModel> UpdatePosition(
            UserModel user, 
            double longitude, 
            double latitude)
        {
            return this.UpdateCoordinatesInternal(user, longitude, latitude);
        }
        
        /// <summary>
        /// Перевести исполнителя в состояние "не в работе"
        /// </summary>
        /// <param name="username">Имя(уникальное) исполнителя</param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> SetInactive(string username)
        {
            var userResult = await this.accountService.FindUser(username, false);
            if (!userResult.Success)
            {
                return userResult;
            }
            var user = userResult.Result;
            return this.UpdateCoordinatesInternal(user, null, null);
        }
        
        /// <summary>
        /// Перевести исполнителя в состояние "не в работе"
        /// </summary>
        /// <param name="user">Модель пользователя</param>
        /// <returns></returns>
        public ServiceResult<UserModel> SetInactive(UserModel user)
        {
            return this.UpdateCoordinatesInternal(user, null, null);
        }

        /// <summary>
        /// Получить доступных исполнителей для менеджера.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<UserModel>>> GetAvailablePerformers(
            string username,
            int offset,
            int limit)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<UserModel>>(
                    null,
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            return await this.GetAvailablePerformers(user, limit, offset);
        }

        /// <summary>
        /// Получить доступных исполнителей для менеджера.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<UserModel>>> GetAvailablePerformers(
            UserModel user,
            int offset,
            int limit)
        {
            var roleResult = await this.accountService.GetUserRole(user);
            if (!roleResult.Success)
            {
                return new ServiceResult<List<UserModel>>(
                    null,
                    ErrorFactory.UserNotInRole(user.UserName, this.roleCache.Manager.Name));
            }
            var role = roleResult.Result;
            if (role != this.roleCache.Manager.Name)
            {
                return new ServiceResult<List<UserModel>>(
                    null,
                    ErrorFactory.UserNotInRole(user.UserName, this.roleCache.Manager.Name));
            }
            
            IQueryable<UserModel> users = this.dbContext.Users
                .Where(p => p.InstanceId == user.InstanceId && p.Longitude != null && p.Latitude != null)
                .Skip(offset)
                .Take(limit);
            
            return new ServiceResult<List<UserModel>>(users.ToList());
        }

        #endregion
        
        #region private

        private ServiceResult<UserModel> UpdateCoordinatesInternal(
            UserModel user,
            double? longitude,
            double? latitude)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            user.Longitude = longitude;
            user.Latitude = latitude;
            user.LastTimePositionUpdated = DateTime.UtcNow;
            var result = this.dbContext.Users.Update(user);
            return new ServiceResult<UserModel>(result.Entity);
        }
        
        #endregion
    }
}