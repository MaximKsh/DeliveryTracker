using System;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InstanceService
    {
        #region fields
        
        private readonly DeliveryTrackerDbContext dbContext;

        private readonly AccountService accountService;
        
        #endregion
        
        #region constructor
        
        public InstanceService(
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
        public ServiceResult<InstanceModel> CreateInstance(string name )
        {
            var group = new InstanceModel
            {
                Id = Guid.NewGuid(),
                DisplayableName = name,
            };
            var creatingResult = this.dbContext.Groups.Add(group);
            
            return new ServiceResult<InstanceModel>(creatingResult.Entity);
        }

        /// <summary>
        /// Установить создателя группы.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="creatorId"></param>
        public ServiceResult<InstanceModel> SetCreator(InstanceModel instance, Guid creatorId)
        {
            if (instance.CreatorId.HasValue)
            {
                return new ServiceResult<InstanceModel>(
                    instance,
                    ErrorFactory.InstanceAlreadyHasCreator(instance.DisplayableName));
            }
            
            instance.CreatorId = creatorId;
            var entityEntry = this.dbContext.Groups.Update(instance);
            return new ServiceResult<InstanceModel>(entityEntry.Entity);
        }
        
        #endregion
    }
}