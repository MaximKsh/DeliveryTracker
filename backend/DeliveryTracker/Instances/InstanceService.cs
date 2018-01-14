using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InstanceService : IInstanceService
    {
        #region sql

        private static readonly string SqlInsertInstance = $@"
insert into instances({InstanceHelper.GetInstanceColumns()})
values ({InstanceHelper.GetInstanceColumns("@")})
;";

        private const string SqlSetInstanceCreator = @"
update instances
set creator_id = @creator_id
where id = @id
;";

        private static readonly string SqlGetInstance = $@"
select
{InstanceHelper.GetInstanceColumns()}
from instances
where id = @id
;";
        
        #endregion
        
        #region fields
        
        private readonly IPostgresConnectionProvider cp;

        private readonly IAccountService accountService;

        private readonly IInvitationManager invitationManager;
        
        private readonly ILogger<InstanceService> logger;
        
        #endregion
        
        #region constructor
        
        public InstanceService(
            IPostgresConnectionProvider cp,
            IInvitationManager invitationManager,
            IAccountService accountService)
        {
            this.cp = cp;
            this.invitationManager = invitationManager;
            this.accountService = accountService;
        }
        
        #endregion

        #region public

        public async Task<ServiceResult<Tuple<Instance, User, UserCredentials>>> CreateAsync(
            string instanceName, 
            User creatorInfo, 
            CodePassword codePassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddRule("InstanceName", instanceName, x => x != null && !string.IsNullOrWhiteSpace(x))
                .AddRule("creatorInfo.Surname", creatorInfo?.Surname, x => x != null && !string.IsNullOrWhiteSpace(x))
                .AddRule("creatorInfo.Name", creatorInfo?.Name, x => x != null && !string.IsNullOrWhiteSpace(x))
                .AddRule("creatorInfo.PhoneNumber", creatorInfo?.PhoneNumber, x => x != null && !string.IsNullOrWhiteSpace(x))
                .AddRule("codePassword.Password", codePassword?.Password, x => x != null && !string.IsNullOrWhiteSpace(x))
                .Validate();
            
            if (!validationResult.Success)
            {
                return new ServiceResult<Tuple<Instance, User, UserCredentials>>(validationResult.Error);
            }
            
            return await this.CreateInternalAsync(instanceName, creatorInfo, codePassword, oc);
        }

        public async Task<ServiceResult<Instance>> GetAsync(Guid instanceId, NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();

                using (var command = connWrapper.CreateCommand())
                {
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

        private async Task<ServiceResult<Tuple<Instance, User, UserCredentials>>> CreateInternalAsync(
            string instanceName,
            User creatorInfo,
            CodePassword codePassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();

                using (var transaction = connWrapper.BeginTransaction())
                {
                    var instanceId = await InsertNewInstance(instanceName, connWrapper);
                    creatorInfo.InstanceId = instanceId;
                    var code = await this.invitationManager.GenerateUniqueCodeAsync(oc);
                    var registrationResult = await this.accountService.RegisterAsync(
                        codePassword,
                        u =>
                        {
                            u.Code = code;
                            u.Role = DefaultRoles.CreatorRole;
                            u.Surname = creatorInfo.Surname;
                            u.Name = creatorInfo.Name;
                            u.Patronymic = creatorInfo.Patronymic;
                            u.PhoneNumber = creatorInfo.PhoneNumber;
                            u.InstanceId = instanceId;
                        },
                        connWrapper);
                    if (!registrationResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<Tuple<Instance, User, UserCredentials>>(registrationResult.Errors);
                    }

                    var user = registrationResult.Result.Item1;
                    var credentials = registrationResult.Result.Item2;
                    await SetInstanceCreator(instanceId, user.Id, connWrapper);
                    var createdInstance = new Instance
                    {
                        Id = instanceId,
                        Name = instanceName, 
                        CreatorId = user.Id
                    };
                    transaction.Commit();
                    return new ServiceResult<Tuple<Instance, User, UserCredentials>>(
                        new Tuple<Instance, User, UserCredentials>(createdInstance, user, credentials));
                }
            }
            
        }
        
        private static async Task<Guid> InsertNewInstance(
            string instanceName,
            NpgsqlConnectionWrapper oc)
        {
            using (var command = oc.CreateCommand())
            {
                var id = Guid.NewGuid();
                command.CommandText = SqlInsertInstance;
                command.Parameters.Add(new NpgsqlParameter("id", id));
                command.Parameters.Add(new NpgsqlParameter("name", instanceName));
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