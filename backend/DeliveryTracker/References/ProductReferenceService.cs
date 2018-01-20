using System;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.References
{
    public class ProductReferenceService : IReferenceService<Product>
    {
        #region sql
        
        private static readonly string SqlCreate = @"
insert into products (" + ReferenceHelper.GetProductColumns() + @")
values (" + ReferenceHelper.GetProductColumns("@") + @")
returning " + ReferenceHelper.GetProductColumns() + ";";

        private static readonly string SqlUpdate = @"
update products
set
{0}
where id = @id and instance_id = @instance_id
returning " + ReferenceHelper.GetProductColumns() + ";";

        private static readonly string SqlGet = @"
select " + ReferenceHelper.GetProductColumns() + @"
from products
where id = @id and instance_id = @instance_id;";
        
        private const string SqlDelete = @"
delete from products
where id = @id and instance_id = @instance_id
;
";
        
        #endregion
        
        #region fields

        private readonly IPostgresConnectionProvider cp;

        private readonly IUserCredentialsAccessor accessor;

        #endregion

        #region constructor
        
        public ProductReferenceService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor)
        {
            this.cp = cp;
            this.accessor = accessor;
        }

        #endregion

        #region public
        
        /// <inheritdoc />
        public async Task<ServiceResult<Product>> CreateAsync(Product newData, NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule("Product", newData)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<Product>(validationResult.Error);
            }

            var credentials = this.accessor.UserCredentials;
            if (!credentials.Valid)
            {
                return new ServiceResult<Product>(ErrorFactory.AccessDenied());
            }

            Product newProduct = null;
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlCreate;
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));
                    command.Parameters.Add(new NpgsqlParameter("vendor_code", newData.VendorCode).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("name", newData.Name).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("description", newData.Description).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("cost", newData.Cost).CanBeNull());
                    
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                newProduct = reader.GetProduct();
                            }
                        }
                    }
                    catch (NpgsqlException)
                    {
                        return new ServiceResult<Product>(ErrorFactory.ReferenceCreationError(nameof(Product)));
                    }
                }
            }
            return new ServiceResult<Product>(newProduct);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Product>> GetAsync(Guid id, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.accessor.UserCredentials;
            if (!credentials.Valid)
            {
                return new ServiceResult<Product>(ErrorFactory.AccessDenied());
            }

            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlGet;
                    
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<Product>(reader.GetProduct());
                        }
                        return new ServiceResult<Product>(ErrorFactory.ReferenceEntryNotFound(nameof(Product), id));
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Product>> EditAsync(Product newData, NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule("Product", newData)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<Product>(validationResult.Error);
            }

            var credentials = this.accessor.UserCredentials;
            if (!credentials.Valid)
            {
                return new ServiceResult<Product>(ErrorFactory.AccessDenied());
            }

            Product updatedProduct = null;
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    var queryStringBuilder = new StringBuilder();
                    
                    command.Parameters.Add(new NpgsqlParameter("id", newData.Id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));
                    command.AppendIfNotNull(queryStringBuilder, "vendor_code", newData.VendorCode);
                    command.AppendIfNotNull(queryStringBuilder, "name", newData.Name);
                    command.AppendIfNotNull(queryStringBuilder,"description", newData.Description);
                    command.AppendIfNotNull(queryStringBuilder, "cost", newData.Cost);
                    
                    command.CommandText = string.Format(SqlUpdate, queryStringBuilder.ToString());
                    
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                updatedProduct = reader.GetProduct();
                            }
                        }
                    }
                    catch (NpgsqlException)
                    {
                        return new ServiceResult<Product>(ErrorFactory.ReferenceEditError(nameof(Product)));
                    }
                }
            }
            return new ServiceResult<Product>(updatedProduct);
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(Guid id, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.accessor.UserCredentials;
            if (!credentials.Valid)
            {
                return new ServiceResult<Product>(ErrorFactory.AccessDenied());
            }

            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlDelete;
                    
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));

                    var affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows == 1 
                        ? new ServiceResult()
                        : new ServiceResult(ErrorFactory.ReferenceEntryNotFound(nameof(Product), id));
                }
            }
        }
        
        #endregion
    }
}