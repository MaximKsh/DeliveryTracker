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
    public class PaymentTypesView : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {ReferenceHelper.GetPaymentTypeColumns()}
from payment_types
where instance_id = @instance_id
    and deleted = false
{{0}}

order by lower(name)
limit {ViewHelper.DefaultViewLimit}
;
";

        private static readonly string SqlCount = $@"
select payment_types_count
from entries_statistics
where instance_id = @instance_id
;
";
        #endregion
        
        #region fields
        
        private readonly int order;

        private readonly IReferenceService<PaymentType> paymentTypeReferenceService;
        
        #endregion
        
        #region constuctor
        
        public PaymentTypesView(
            int order,
            IReferenceService<PaymentType> paymentTypeReferenceService)
        {
            this.order = order;
            this.paymentTypeReferenceService = paymentTypeReferenceService;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(PaymentTypesView);
        
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
                Caption = LocalizationAlias.Views.PaymentTypesView,
                Count = result.Result,
                EntityType = nameof(PaymentType),
                Order = this.order,
            });
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<PaymentType>();
            using (var command = oc.CreateCommand())
            {
                
                var sb = new StringBuilder(256);
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search", "name");
                ViewHelper.TryAddAfterParameter(parameters, command, sb, "payment_types", "name");

                command.CommandText = string.Format(SqlGet, sb);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetPaymentType());
                    }
                }
            }

            var package = await this.paymentTypeReferenceService.PackAsync(list, oc);
            
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