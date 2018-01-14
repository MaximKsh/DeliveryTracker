using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InstanceService : IInstanceService
    {
        #region sql

        private const string SqlInsertInstance = @"
insert into instances(id, name, creator_id)
values (@id, @name, @creator_id)
;";
        
        private const string SqlSetInstanceCreator = @"
update instances
set creator_id = @creator_id
where id = @id
;";
        
        private const string SqlGetInstance = @"
select
    id,
    name
    creator_id
from instances
where id = @id
;";
        
        #endregion
        
        #region fields
        
        private readonly IPostgresConnectionProvider cp;

        private readonly IAccountService accountService;
        
        private readonly ILogger<InstanceService> logger;
        
        #endregion
        
        #region constructor
        
        public InstanceService(
            IPostgresConnectionProvider cp,
            IAccountService accountService)
        {
            this.cp = cp;
            this.accountService = accountService;
        }
        
        #endregion

        #region public
        
        public async Task<ServiceResult<Instance>> CreateAsync(
            Instance instance, 
            User creatorInfo, 
            CodePassword codePassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (creatorInfo == null)
            {
                throw new ArgumentNullException(nameof(creatorInfo));
            }
            if (codePassword == null)
            {
                throw new ArgumentNullException(nameof(codePassword));
            }
            
            return await this.CreateInternalAsync(instance, creatorInfo, codePassword, oc);
        }

        public async Task<ServiceResult<Instance>> GetAsync(Guid instanceId, NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();

                using (var command = connWrapper.CreateCommand())
                {
                    var id = Guid.NewGuid();
                    command.CommandText = SqlGetInstance;
                    command.Parameters.Add(new NpgsqlParameter("id", instanceId));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<Instance>(reader.GetInstance());
                        }
                        return new ServiceResult<Instance>(ErrorFactory.InstanceNotFound());
                    }
                }
            }
        }
        
        #endregion
        
        #region private

        private async Task<ServiceResult<Instance>> CreateInternalAsync(
            Instance instance,
            User creatorInfo,
            CodePassword codePassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();

                using (var transaction = connWrapper.BeginTransaction())
                {
                    var instanceId = await InsertNewInstance(instance, connWrapper);
                    
                    var registrationResult = await this.accountService.RegisterAsync(
                        creatorInfo, 
                        codePassword,
                        connWrapper);
                    if (!registrationResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<Instance>(registrationResult.Errors);
                    }

                    var user = registrationResult.Result.Item1;
                    var credentials = registrationResult.Result.Item2;
                    await SetInstanceCreator(instanceId, user.Id, connWrapper);
                    transaction.Commit();
                }
            }
            return new ServiceResult<Instance>(ErrorFactory.ServerError());
        }
        
        private static async Task<Guid> InsertNewInstance(
            Instance instance,
            NpgsqlConnectionWrapper oc)
        {
            using (var command = oc.CreateCommand())
            {
                var id = Guid.NewGuid();
                command.CommandText = SqlInsertInstance;
                command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                command.Parameters.Add(new NpgsqlParameter("name", instance.Name));
                command.Parameters.Add(new NpgsqlParameter("creator_id", Guid.Empty));
                await command.ExecuteNonQueryAsync();
                return id;
            }
        }
        
        private static async Task SetInstanceCreator(
            Guid instanceId,
            Guid creatorId,
            NpgsqlConnectionWrapper oc)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlSetInstanceCreator;
                command.Parameters.Add(new NpgsqlParameter("id", instanceId));
                command.Parameters.Add(new NpgsqlParameter("creator_id", creatorId));
                await command.ExecuteNonQueryAsync();
            }
        }
        
        #endregion
    }
}