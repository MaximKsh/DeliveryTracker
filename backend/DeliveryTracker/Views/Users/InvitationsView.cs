using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Localization;
using Npgsql;

namespace DeliveryTracker.Views.Users
{
    public class InvitationsView : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {InstanceHelper.GetInvitationColumns()}
from invitations
where instance_id = @instance_id
    and deleted = false
    {{0}}
order by expires desc
limit {ViewHelper.DefaultViewLimit}
;
";

        private const string SqlCount = @"
select managers_invitations_count + performers_invitations_count
from entries_statistics
where instance_id = @instance_id
;
";

        private const string SqlCountPerformers = @"
select performers_invitations_count
from entries_statistics
where instance_id = @instance_id
;
";
        

        #endregion
        
        #region fields
        
        private readonly int order;
        
        #endregion
        
        #region constuctor
        
        public InvitationsView(
            int order)
        {
            this.order = order;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(InvitationsView);
        
        /// <inheritdoc />
        public IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole
        }.AsReadOnly();
        
        /// <inheritdoc />
        public async Task<ServiceResult<ViewDigest>> GetViewDigestAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var result = await this.GetCountAsync(oc, userCredentials, parameters);
            if (!result.Success)
            {
                return new ServiceResult<ViewDigest>(result.Errors);
            }
            return new ServiceResult<ViewDigest>(new ViewDigest
            {
                Caption = LocalizationAlias.Views.InvitationsView,
                Count = result.Result,
                EntityType = nameof(Invitation),
                Order = this.order,
            });
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<IDictionaryObject>();
            using (var command = oc.CreateCommand())
            {
                var sb = new StringBuilder(256);
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                if (userCredentials.Role == DefaultRoles.ManagerRole)
                {
                    ViewHelper.AddEqualsParameter(command, sb, "role", "role", DefaultRoles.PerformerRole);
                }
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search");
                ViewHelper.TryAddAfterParameter(parameters, command, sb, "invitations", "expires", true);

                command.CommandText = string.Format(SqlGet, sb);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetInvitation());
                    }
                }
            }
            
            return new ServiceResult<IList<IDictionaryObject>>(list);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            
            using (var command = oc.CreateCommand())
            {
                command.CommandText = userCredentials.Role == DefaultRoles.CreatorRole
                    ? SqlCount
                    : SqlCountPerformers;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
        
        #endregion
    }
}