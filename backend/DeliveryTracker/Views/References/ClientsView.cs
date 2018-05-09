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
    public class ClientsView : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {ReferenceHelper.GetClientColumns()}
from clients
where instance_id = @instance_id
    and deleted = false
    {{0}}
order by surname
limit {ViewHelper.DefaultViewLimit}
;
";

        private const string SqlCount = @"
select clients_count
from entries_statistics
where instance_id = @instance_id
;
";
        #endregion
        
        #region fields
        
        private readonly int order;

        private readonly IReferenceService<Client> clientReferenceService;
        
        #endregion
        
        #region constuctor
        
        public ClientsView(
            int order,
            IReferenceService<Client> clientReferenceService)
        {
            this.order = order;
            this.clientReferenceService = clientReferenceService;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(ClientsView);
        
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
                Caption = LocalizationAlias.Views.ClientsView,
                Count = result.Result,
                EntityType = nameof(Client),
                Order = this.order,
            });
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<Client>();
            using (var command = oc.CreateCommand())
            {
                var sb = new StringBuilder(256);
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search");
                ViewHelper.TryAddStartsWithParameter(parameters, command, sb, "phone_number");
                ViewHelper.TryAddAfterParameter(parameters, command, sb, "clients", "surname");

                command.CommandText = string.Format(SqlGet, sb);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetClient());
                    }
                }
            }

            var packed = await this.clientReferenceService.PackAsync(list, oc);
            
            return new ServiceResult<IList<IDictionaryObject>>(packed.Result.Cast<IDictionaryObject>().ToList());
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