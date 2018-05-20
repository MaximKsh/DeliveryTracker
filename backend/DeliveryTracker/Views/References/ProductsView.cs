using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.References;
using Npgsql;

namespace DeliveryTracker.Views.References
{
    public class ProductsView : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {ReferenceHelper.GetProductColumns()}
from products 
where instance_id = @instance_id
    and deleted = false
{{0}}

order by name
limit {ViewHelper.DefaultViewLimit}
;
";

        private const string SqlCount = @"
select products_count
from entries_statistics
where instance_id = @instance_id
;
";
        #endregion
        
        #region fields
        
        private readonly int order;

        private readonly IReferenceService<Product> productsReferenceService;
        
        #endregion
        
        #region constuctor
        
        public ProductsView(
            int order,
            IReferenceService<Product> productsReferenceService)
        {
            this.order = order;
            this.productsReferenceService = productsReferenceService;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(ProductsView);
        
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
                Caption = LocalizationAlias.Views.ProductsView,
                Count = result.Result,
                EntityType = nameof(Product),
                Order = this.order,
            });
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<Product>();
            using (var command = oc.CreateCommand())
            {
                var sb = new StringBuilder(256);
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search", "name");
                ViewHelper.TryAddAfterParameter(parameters, command, sb, "products", "name");
                
                command.CommandText = string.Format(SqlGet, sb);
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetProduct());
                    }
                }
            }

            var package = await this.productsReferenceService.PackAsync(list, withDeleted:false,  oc:oc);
            
            return new ServiceResult<IList<IDictionaryObject>>(package.Result.Cast<IDictionaryObject>().ToList());
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlCount;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
        
        #endregion
    }
}