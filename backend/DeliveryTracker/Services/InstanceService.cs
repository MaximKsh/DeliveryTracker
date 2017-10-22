using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.Roles;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;

namespace DeliveryTracker.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InstanceService
    {
        #region fields
        
        private readonly DeliveryTrackerDbContext dbContext;

        private readonly AccountService accountService;
        
        private readonly RoleCache roleCache;
        
        #endregion
        
        #region constructor
        
        public InstanceService(
            DeliveryTrackerDbContext dbContext,
            AccountService accountService, 
            RoleCache roleCache)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
            this.roleCache = roleCache;
        }
        
        #endregion

        #region public

        /// <summary>
        /// Создать группу.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public ServiceResult<InstanceModel> CreateInstance(InstanceViewModel instance)
        {
            var group = new InstanceModel
            {
                Id = Guid.NewGuid(),
                InstanceName = instance.InstanceName,
            };
            var creatingResult = this.dbContext.Instances.Add(group);
            
            return new ServiceResult<InstanceModel>(creatingResult.Entity);
        }

        /// <summary>
        /// Установить создателя инстанса.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="creatorId"></param>
        public ServiceResult<InstanceModel> SetCreator(InstanceModel instance, Guid creatorId)
        {
            if (instance.CreatorId.HasValue)
            {
                return new ServiceResult<InstanceModel>(
                    instance,
                    ErrorFactory.InstanceAlreadyHasCreator(instance.InstanceName));
            }
            
            instance.CreatorId = creatorId;
            var entityEntry = this.dbContext.Instances.Update(instance);
            return new ServiceResult<InstanceModel>(entityEntry.Entity);
        }

        /// <summary>
        /// Получить список исполнетелей в инстансе.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<UserModel>>> GetPerformers(
            string username,
            int limit,
            int offset)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<UserModel>>(ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            return this.GetPerformers(user, limit, offset);
        }

        /// <summary>
        /// Получить список исполнителей в инстансе.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public ServiceResult<List<UserModel>> GetPerformers(
            UserModel user,
            int limit,
            int offset)
        {
            return this.GetUsers(user.InstanceId, this.roleCache.Performer.Id, limit, offset);
        }
        
        /// <summary>
        /// Получить управляющих инстанса.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ServiceResult<List<UserModel>>> GetManagers(
            string username,
            int limit,
            int offset)
        {
            var currentUserResult = await this.accountService.FindUser(username);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<List<UserModel>>(
                    ErrorFactory.UserNotFound(username));
            }
            var user = currentUserResult.Result;
            return this.GetManagers(user, limit, offset);
        }

        /// <summary>
        /// Получить управляющих инстанса.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public ServiceResult<List<UserModel>> GetManagers(
            UserModel user,
            int limit,
            int offset)
        {
            return this.GetUsers(user.InstanceId, this.roleCache.Manager.Id, limit, offset);
        }

        /// <summary>
        /// Отметить исполнителя удаленным.
        /// </summary>
        /// <param name="currentUsername"></param>
        /// <param name="deletingUsername"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> DeletePerformer(
            string currentUsername,
            string deletingUsername)
        {
            var currentUserResult = await this.accountService.FindUser(currentUsername);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<UserModel>(ErrorFactory.UserNotFound(currentUsername));
            }
            var currentUser = currentUserResult.Result;
            if (!await this.accountService.IsInRole(currentUser, this.roleCache.Creator, this.roleCache.Manager))
            {
                return new ServiceResult<UserModel>(ErrorFactory.AccessDenied());
            }
            var deletingUserResult = await this.accountService.FindUser(deletingUsername);
            if (!deletingUserResult.Success)
            {
                return new ServiceResult<UserModel>(ErrorFactory.UserNotFound(deletingUsername));
            }
            var deletingUser = deletingUserResult.Result;
            if (!await this.accountService.IsInRole(deletingUser, this.roleCache.Performer))
            {
                return new ServiceResult<UserModel>(ErrorFactory.AccessDenied());
            }
            var result = await this.accountService.MarkUserAsDeleted(deletingUser);
            return result.Success
                ? new ServiceResult<UserModel>(result.Result)
                : new ServiceResult<UserModel>(result.Errors);
        }
        
        /// <summary>
        /// Отметить менеджера удаленным.
        /// </summary>
        /// <param name="currentUsername"></param>
        /// <param name="deletingUsername"></param>
        /// <returns></returns>
        public async Task<ServiceResult<UserModel>> DeleteManager(
            string currentUsername,
            string deletingUsername)
        {
            var currentUserResult = await this.accountService.FindUser(currentUsername);
            if (!currentUserResult.Success)
            {
                return new ServiceResult<UserModel>(ErrorFactory.UserNotFound(currentUsername));
            }
            var currentUser = currentUserResult.Result;
            if (!await this.accountService.IsInRole(currentUser, this.roleCache.Creator))
            {
                return new ServiceResult<UserModel>(ErrorFactory.AccessDenied());
            }
            var deletingUserResult = await this.accountService.FindUser(deletingUsername);
            if (!deletingUserResult.Success)
            {
                return new ServiceResult<UserModel>(ErrorFactory.UserNotFound(deletingUsername));
            }
            var deletingUser = deletingUserResult.Result;
            if (!await this.accountService.IsInRole(deletingUser, this.roleCache.Manager))
            {
                return new ServiceResult<UserModel>(ErrorFactory.AccessDenied());
            }
            var result = await this.accountService.MarkUserAsDeleted(deletingUser);
            return result.Success
                ? new ServiceResult<UserModel>(result.Result)
                : new ServiceResult<UserModel>(result.Errors);
        }
        
        #endregion
        
        #region private

        private ServiceResult<List<UserModel>> GetUsers(
            Guid instanceId,
            Guid roleId,
            int limit,
            int offset)
        {
            IQueryable<UserModel> users = this.dbContext.Users
                .Join(this.dbContext.UserRoles, user => user.Id, ru => ru.UserId, (user, ru) => new {user, ru})
                .Join(this.dbContext.Roles, arg => arg.ru.RoleId, role => role.Id, (arg, role) => new {arg.user, role})
                .Where(p => p.user.InstanceId == instanceId && p.role.Id == roleId)
                .Skip(offset)
                .Take(limit)
                .Select(p => p.user);

            return new ServiceResult<List<UserModel>>(users.ToList());
        }

        #endregion
    }
}