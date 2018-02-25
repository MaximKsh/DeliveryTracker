using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.References;
using Npgsql;

namespace DeliveryTracker.Views
{
    public class ClientsView : IView
    {
        private static readonly string SqlGet = $@"
select
    {ReferenceHelper.GetClientColumns()}
from clients
where instance_id = @instance_id
;
";

        private const string SqlCount = @"
select count(1)
from clients
where instance_id = @instance_id
;
";
        
        /// <inheritdoc />
        public string Name { get; } = nameof(ClientsView);

        /// <inheritdoc />
        public async Task<ServiceResult<ViewDigest>> GetViewDigestAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            var result = await this.GetCountAsync(oc, userCredentials, parameters);
            if (!result.Success)
            {
                return new ServiceResult<ViewDigest>(result.Errors);
            }
            return new ServiceResult<ViewDigest>(new ViewDigest
            {
                Caption = LocalizationAlias.Views.ClientsView,
                Count = result.Result,
                EntityType = nameof(Client),
            });
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            var list = new List<IDictionaryObject>();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGet;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetClient());
                    }
                }
            }
            
            return new ServiceResult<IList<IDictionaryObject>>(list);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlCount;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
    }
}