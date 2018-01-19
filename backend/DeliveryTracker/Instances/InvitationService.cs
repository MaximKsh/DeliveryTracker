using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.Instances
{
    public class InvitationService : IInvitationService
    {
        #region sql

        private const string SqlCodeExists = @"
select 1 from invitations where invitation_code = @code;
select 1 from users where code = @code;
";

        private static readonly string SqlCreate = $@"
insert into invitations ({InstanceHelper.GetInvitationColumns()})
values ({InstanceHelper.GetInvitationColumns("@")})
returning {InstanceHelper.GetInvitationColumns()};
";

        private static readonly string SqlGet = $@"
select
{InstanceHelper.GetInvitationColumns()}
from invitations
where invitation_code = @code;
";
        
        private static readonly string SqlDelete = $@"
delete from invitations
where invitation_code = @code;
";
        
        private static readonly string SqlDeleteExpired = $@"
delete from invitations
where expires < (now() AT TIME ZONE 'UTC');
";
        
        #endregion
        
        #region fields

        private readonly Random random = new Random();
        
        private readonly IPostgresConnectionProvider cp;

        private readonly InvitationSettings invitationSettings;

        private readonly IUserCredentialsAccessor userCredentialsAccessor;
        
        private readonly ILogger<IInvitationService> logger;
        
        #endregion
        
        #region constructor
        
        public InvitationService(
            IPostgresConnectionProvider cp,
            InvitationSettings invitationSettings,
            IUserCredentialsAccessor userCredentialsAccessor,
            ILogger<IInvitationService> logger)
        {
            this.cp = cp;
            this.invitationSettings = invitationSettings;
            this.userCredentialsAccessor = userCredentialsAccessor;
            this.logger = logger;
        }
        
        #endregion
        
        #region public
        
        /// <inheritdoc />
        public string GenerateCode()
        {
            var invitationCode = new char[this.invitationSettings.CodeLength];
            var alphabetSize = this.invitationSettings.Alphabet.Length;
            for (var i = 0; i < invitationCode.Length; i++)
            {
                invitationCode[i] = this.invitationSettings.Alphabet[this.random.Next(alphabetSize - 1)];
            }
            return new string(invitationCode);
        }

        /// <inheritdoc />
        public async Task<string> GenerateUniqueCodeAsync(NpgsqlConnectionWrapper oc = null)
        {
            var code = string.Empty;
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlCodeExists;
                    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                    command.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Varchar));
                    var codeExists = true;
                    while (codeExists)
                    {
                        code = this.GenerateCode();
                        command.Parameters[0].Value = code;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var cnt = 0;
                            if (await reader.ReadAsync())
                            {
                                cnt++;
                            }
                            await reader.NextResultAsync();
                            if (await reader.ReadAsync())
                            {
                                cnt++;
                            }

                            if (cnt == 0)
                            {
                                codeExists = false;
                            }
                            else
                            {
                                this.logger.LogWarning($"Invitation code {code} repeated.");
                            }
                        }

                    }
                }
            }

            return code;
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult<Invitation>> CreateAsync(
            User preliminaryUserData,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule("User", preliminaryUserData)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<Invitation>(validationResult.Error);
            }
            
            if (!DefaultRoles.AllRoles.Contains(preliminaryUserData.Role))
            {
                return new ServiceResult<Invitation>(ErrorFactory.RoleNotFound());
            }

            if (!this.CanCreateInvitation(preliminaryUserData, out var credentials))
            {
                return new ServiceResult<Invitation>(ErrorFactory.AccessDenied());
            }

            return await this.CreateAsyncInternal(credentials.Id, credentials.InstanceId, preliminaryUserData, oc);
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult<Invitation>> GetAsync(
            string invitationCode, 
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule("InvitationCode", invitationCode)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<Invitation>(validationResult.Error);
            }
            
            Invitation invitation = null;
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlGet;
                    command.Parameters.Add(new NpgsqlParameter("code", invitationCode));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {
                            invitation = reader.GetInvitation();
                        }
                    }
                }
            }

            return invitation != null
                ? new ServiceResult<Invitation>(invitation)
                : new ServiceResult<Invitation>(ErrorFactory.InvitationNotFound(invitationCode));
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(string invitationCode, NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlDelete;
                    command.Parameters.Add(new NpgsqlParameter("code", invitationCode));
                    var success = await command.ExecuteNonQueryAsync() == 1;
                    return success
                        ? new ServiceResult()
                        : new ServiceResult(ErrorFactory.InvitationNotFound(invitationCode));
                }
            }
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAllExpiredAsync(NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlDeleteExpired;
                    await command.ExecuteNonQueryAsync();
                    return new ServiceResult();
                }
            }
        }
        
        #endregion
        
        #region private

        private bool CanCreateInvitation(User preliminaryUserData, out UserCredentials userCredentials)
        {
            userCredentials = this.userCredentialsAccessor.UserCredentials;
            if (preliminaryUserData.Role == DefaultRoles.ManagerRole)
            {
                return userCredentials.Role == DefaultRoles.CreatorRole;
            }

            if (preliminaryUserData.Role == DefaultRoles.PerformerRole)
            {
                return userCredentials.Role == DefaultRoles.CreatorRole
                       || userCredentials.Role == DefaultRoles.ManagerRole;
            }

            return false;
        }
        
        private async Task<ServiceResult<Invitation>> CreateAsyncInternal(
            Guid creatorId,
            Guid instanceId,
            User preliminaryUserData,
            NpgsqlConnectionWrapper oc = null)
        {
            Invitation invitation = null;
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                var invitationCode = await this.GenerateUniqueCodeAsync(conn);
                
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlCreate;
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("invitation_code", invitationCode));
                    command.Parameters.Add(new NpgsqlParameter("creator_id", creatorId));
                    command.Parameters.Add(new NpgsqlParameter("created", DateTime.UtcNow));
                    command.Parameters.Add(new NpgsqlParameter(
                        "expires", DateTime.UtcNow.AddDays(this.invitationSettings.ExpiresInDays)));
                    command.Parameters.Add(new NpgsqlParameter("role", preliminaryUserData.Role));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    command.Parameters.Add(new NpgsqlParameter("surname", preliminaryUserData.Surname).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("name", preliminaryUserData.Name).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("patronymic", preliminaryUserData.Patronymic).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("phone_number", preliminaryUserData.PhoneNumber).CanBeNull());

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
                            {
                                invitation = reader.GetInvitation();
                            }
                        }
                    }
                    catch (NpgsqlException)
                    {
                        return new ServiceResult<Invitation>(ErrorFactory.InvitationCreationError());   
                    }
                }
            }
            return new ServiceResult<Invitation>(invitation);
        }
        
        #endregion
    }
}