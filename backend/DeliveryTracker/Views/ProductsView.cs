using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.References;
using Npgsql;

namespace DeliveryTracker.Views
{
    public class ProductsView : IView
    {
        private static readonly string SqlGet = $@"
select
    {ReferenceHelper.GetProductColumns()}
from products
where instance_id = @instance_id
;
";

        private const string SqlCount = @"
select count(1)
from products
where instance_id = @instance_id
;
";
        
        
        /// <inheritdoc />
        public string Name { get; } = nameof(ProductsView);
        
        /// <inheritdoc />
        public async Task<ServiceResult<object[]>> GetViewResultAsync(NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            var list = new List<Product>();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGet;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetProduct());
                    }
                }
            }
            
            return new ServiceResult<object[]>(list.Cast<object>().ToArray());
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

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
    }
}