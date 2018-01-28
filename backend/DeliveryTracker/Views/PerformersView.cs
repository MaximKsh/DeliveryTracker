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
    public class PerformersView : IView
    {
        private static readonly string SqlGet = $@"
select
    {IdentificationHelper.GetUserColumns()}
from users
where instance_id = @instance_id and role = @role
;
";

        private const string SqlCount = @"
select count(1)
from users
where instance_id = @instance_id and role = @role
;
";
        
        
        /// <inheritdoc />
        public string Name { get; } = nameof(PerformersView);
        
        
        /// <inheritdoc />
        public string Caption { get; } = LocalizationAlias.Views.PerformersView;
        
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
                command.Parameters.Add(new NpgsqlParameter("@role", DefaultRoles.PerformerRole));

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
                command.Parameters.Add(new NpgsqlParameter("@role", DefaultRoles.PerformerRole));

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
    }
}