using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <inheritdoc />
    public sealed class InstanceService : IInstanceService
    {
        #region sql

        private static readonly string SqlInsertInstance = $@"
insert into instances({InstanceHelper.GetInstanceColumns()})
values ({InstanceHelper.GetInstanceColumns("@")})
;
insert into entries_statistics(instance_id)
values (@id)
;
";

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

        private readonly ISecurityManager securityManager;

        private readonly IUserManager userManager;

        private readonly IInvitationService invitationService;
        
        private readonly IUserCredentialsAccessor accessor;

        private readonly IReferenceService<PaymentType> paymentTypeReferenceService;

        private readonly IDeviceManager deviceManager;
        
        #endregion
        
        #region constructor
        
        public InstanceService(
            IPostgresConnectionProvider cp,
            IUserManager userManager,
            ISecurityManager securityManager, 
            IInvitationService invitationService,
            IUserCredentialsAccessor accessor,
            IReferenceService<PaymentType> paymentTypeReferenceService,
            IDeviceManager deviceManager)
        {
            this.cp = cp;
            this.userManager = userManager;
            this.securityManager = securityManager;
            this.invitationService = invitationService;
            this.accessor = accessor;
            this.paymentTypeReferenceService = paymentTypeReferenceService;
            this.deviceManager = deviceManager;
        }
        
        #endregion

        #region public

        /// <inheritdoc />
        public async Task<ServiceResult<InstanceServiceResult>> CreateAsync(
            string instanceName, 
            User creatorInfo, 
            Device creatorDevice,
            CodePassword codePassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule("InstanceName", instanceName)
                .AddNotNullOrWhitespaceRule("creatorInfo.Surname", creatorInfo.Surname)
                .AddNotNullOrWhitespaceRule("creatorInfo.Name", creatorInfo.Name)
                .AddNotNullOrWhitespaceRule("creatorInfo.PhoneNumber", creatorInfo.PhoneNumber)
                .AddNotNullOrWhitespaceRule("codePassword.Password", codePassword.Password)
                .Validate();
            
            if (!validationResult.Success)
            {
                return new ServiceResult<InstanceServiceResult>(validationResult.Error);
            }

            return await this.CreateInternalAsync(instanceName, creatorInfo, creatorDevice, codePassword, oc);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<InstanceServiceResult>> GetAsync(NpgsqlConnectionWrapper oc = null)
        {
            var instanceId = this.accessor.GetUserCredentials().InstanceId;
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
                            return new ServiceResult<InstanceServiceResult>(new InstanceServiceResult
                            {
                                Instance = reader.GetInstance()
                            });
                        }
                        return new ServiceResult<InstanceServiceResult>(ErrorFactory.InstanceNotFound());
                    }
                }
            }
        }
        
        #endregion
        
        #region private

        private async Task<ServiceResult<InstanceServiceResult>> CreateInternalAsync(
            string instanceName,
            User creator,
            Device creatorDevice,
            CodePassword codePassword, 
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            using (var transaction = connWrapper.BeginTransaction())
            {
                var instanceId = await InsertNewInstance(instanceName, connWrapper);
                creator.Id = Guid.NewGuid();
                creator.Code = await this.invitationService.GenerateUniqueCodeAsync(connWrapper);
                creator.Role = DefaultRoles.CreatorRole;
                creator.InstanceId = instanceId;

                var createResult = await this.userManager.CreateAsync(creator, connWrapper);
                if (!createResult.Success)
                {
                    transaction.Rollback();
                    return new ServiceResult<InstanceServiceResult>(createResult.Errors);
                }

                var user = createResult.Result;
                var setPasswordResult =
                    await this.securityManager.SetPasswordAsync(user.Id, codePassword.Password, connWrapper);
                if (!setPasswordResult.Success)
                {
                    transaction.Rollback();
                    return new ServiceResult<InstanceServiceResult>(setPasswordResult.Errors);
                }

                var result = new InstanceServiceResult
                {
                    User = user,
                    Credentials = setPasswordResult.Result,
                    Instance = new Instance
                    {
                        Id = instanceId,
                        Name = instanceName, 
                        CreatorId = user.Id
                    }
                };

                await SetInstanceCreator(instanceId, user.Id, connWrapper);

                if (creatorDevice != null)
                {
                    creatorDevice.UserId = user.Id;
                    await this.deviceManager.UpdateUserDeviceAsync(creatorDevice, connWrapper);
                    await this.AddDefaultValues(creatorDevice.Language, instanceId, connWrapper);
                }
                transaction.Commit();
                return new ServiceResult<InstanceServiceResult>(result);
                
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

        private async Task<ServiceResult> AddDefaultValues(
            string language,
            Guid instanceId,
            NpgsqlConnectionWrapper oc)
        {
            var paymentTypes = new[]
            {
                new PaymentType
                {
                    Id = Guid.NewGuid(),
                    InstanceId = instanceId,
                },
                new PaymentType
                {
                    Id = Guid.NewGuid(),
                    InstanceId = instanceId,
                },
            };
            
            switch (language)
            {
                case "en":
                    paymentTypes[0].Name = LocalizationAlias.DefaultValues.CashEn;
                    paymentTypes[1].Name = LocalizationAlias.DefaultValues.CardEn;
                    break;
                case "ru":
                    paymentTypes[0].Name = LocalizationAlias.DefaultValues.CashRu;
                    paymentTypes[1].Name = LocalizationAlias.DefaultValues.CardRu;
                    break;
                default:
                    return ServiceResult.Successful;
            }

            foreach (var paymentType in paymentTypes)
            {
                var result = await this.paymentTypeReferenceService.CreateAsync(paymentType, false, oc);
                if (!result.Success)
                {
                    return result;
                }
            }
            return ServiceResult.Successful;
        }
        
        #endregion
    }
}