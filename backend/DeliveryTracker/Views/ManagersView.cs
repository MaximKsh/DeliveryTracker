using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using Npgsql;

namespace DeliveryTracker.Views
{
    public class ManagersView : IView
    {
        private static readonly string SqlGet = $@"
(
    select
        {IdentificationHelper.GetUserColumns()}
    from users
    where instance_id = @instance_id and role = @role_creator
    limit 1
)
union all
(
    select
        {IdentificationHelper.GetUserColumns()}
    from users
    where instance_id = @instance_id and role = @role
)
;
";

        private const string SqlCount = @"
select count(1)
from users
where instance_id = @instance_id and role = @role
;
";
        
        
        /// <inheritdoc />
        public string Name { get; } = nameof(ManagersView);
        
        
        /// <inheritdoc />
        public string Caption { get; } = LocalizationAlias.Views.ManagersView;
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            var list = new List<IDictionaryObject>();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGet;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("@role_creator", DefaultRoles.CreatorRole));
                command.Parameters.Add(new NpgsqlParameter("@role", DefaultRoles.ManagerRole));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetUser());
                    }
                }
            }
            
            return new ServiceResult<IList<IDictionaryObject>>(list);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlCount;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("role", DefaultRoles.ManagerRole));

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync() + 1);
            }
        }
    }
}