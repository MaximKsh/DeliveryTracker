using System;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GroupService
    {
        #region fields
        
        private readonly DeliveryTrackerDbContext dbContext;

        private readonly AccountService accountService;
        
        #endregion
        
        #region constructor
        
        public GroupService(
            DeliveryTrackerDbContext dbContext,
            AccountService accountService)
        {
            this.dbContext = dbContext;
            this.accountService = accountService;
        }
        
        #endregion

        #region public
        
        /// <summary>
        /// Создать группу.
        /// </summary>
        /// <param name="name">Имя новой группы</param>
        /// <returns></returns>
        public ServiceResult<GroupModel> CreateGroup(string name )
        {
            var group = new GroupModel
            {
                Id = Guid.NewGuid(),
                DisplayableName = name,
            };
            var creatingResult = this.dbContext.Groups.Add(group);
            
            return new ServiceResult<GroupModel>(creatingResult.Entity);
        }

        /// <summary>
        /// Установить создателя группы.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="creatorId"></param>
        public ServiceResult<GroupModel> SetCreator(GroupModel group, Guid creatorId)
        {
            if (group.CreatorId.HasValue)
            {
                return new ServiceResult<GroupModel>(
                    group,
                    ErrorFactory.GroupAlreadyHasCreator(group.DisplayableName));
            }
            
            group.CreatorId = creatorId;
            var entityEntry = this.dbContext.Groups.Update(group);
            return new ServiceResult<GroupModel>(entityEntry.Entity);
        }
        
        #endregion
    }
}