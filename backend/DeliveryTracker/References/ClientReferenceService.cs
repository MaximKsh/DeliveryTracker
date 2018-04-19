using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using Npgsql;

namespace DeliveryTracker.References
{
    public sealed class ClientReferenceService : EntryReferenceServiceBase<Client>
    {
        #region sql
        
        private static readonly string SqlCreate = $@"
insert into clients ({ReferenceHelper.GetClientColumns()})
values ({ReferenceHelper.GetClientColumns("@")})
returning {ReferenceHelper.GetClientColumns()};";

      

        private static readonly string SqlGet = $@"
select {ReferenceHelper.GetClientColumns()}
from clients
where id = @id and instance_id = @instance_id and deleted = false
;";
        private static readonly string SqlGetWithDeleted = $@"
select {ReferenceHelper.GetClientColumns()}
from clients
where id = @id and instance_id = @instance_id
;";
        
     
        private static readonly string SqlGetList = $@"
select {ReferenceHelper.GetClientColumns()}
from clients
where id = ANY (@ids) and instance_id = @instance_id and deleted = false
;";

        private static readonly string SqlGetListWithDeleted = $@"
select {ReferenceHelper.GetClientColumns()}
from clients
where id = ANY (@ids) and instance_id = @instance_id";
        
        private static readonly string SqlUpdate = $@"
update clients
set
{{0}}
where id = @id and instance_id = @instance_id and deleted = false
returning {ReferenceHelper.GetClientColumns()};";
        
       
        private const string SqlDelete = @"
update clients
set deleted = true
where id = @id and instance_id = @instance_id and deleted = false
;
";
        
        #endregion

        #region fields

        private readonly ICollectionReferenceService<ClientAddress> clientAddressService;

        #endregion
        
        #region constructor
        
        public ClientReferenceService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor,
            ICollectionReferenceService<ClientAddress> clientAddressService) : base(cp, accessor)
        {
            this.clientAddressService = clientAddressService;
        }

        #endregion

        #region public
        
        public override ReferenceDescription ReferenceDescription { get; } = new ReferenceDescription
        {
            Caption = LocalizationAlias.References.Clients
        };
        
        #endregion
        
        #region protected

        protected override ExecutionParameters SetCommandCreate(
            NpgsqlCommand command,
            Client newData, 
            Guid id, 
            UserCredentials credentials)
        {
            command.Parameters.Add(new NpgsqlParameter("surname", newData.Surname).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("name", newData.Name).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("patronymic", newData.Patronymic).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("phone_number", newData.PhoneNumber).CanBeNull());

            command.CommandText = SqlCreate;

            return null;
        }

        protected override ExecutionParameters SetCommandEdit(
            NpgsqlCommand command,
            Client newData, 
            UserCredentials credentials)
        {
            var updateFieldsBuilder = new StringBuilder();
            
            var appendedFields = 0;
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder, "surname", newData.Surname);
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder, "name", newData.Name);
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder,"patronymic", newData.Patronymic);
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder, "phone_number", newData.PhoneNumber);
            command.CommandText = appendedFields != 0 
                ? string.Format(SqlUpdate, updateFieldsBuilder) 
                : SqlGet;
            return null;
        }

        protected override ExecutionParameters SetCommandGet(
            NpgsqlCommand command,
            Guid id, 
            bool withDeleted,
            UserCredentials credentials)
        {
            command.CommandText = withDeleted 
                ? SqlGetWithDeleted
                : SqlGet;
            return null;
        }
        
        
        protected override ExecutionParameters SetCommandGetList(
            NpgsqlCommand command,
            ICollection<Guid> ids,
            bool withDeleted,
            UserCredentials credentials)
        {
            command.CommandText = withDeleted
                ? SqlGetListWithDeleted
                : SqlGetList;
            return null;
        }

        protected override ExecutionParameters SetCommandDelete(
            NpgsqlCommand command,
            Guid id, 
            UserCredentials credentials)
        {
            command.CommandText = SqlDelete;
            return null;
        }

        protected override Client Read(IDataReader reader, ReferenceServiceExecutionContext ctx)
        {
            return reader.GetClient();
        }
        
        protected override IList<Client> ReadList(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx)
        {
            var list = new List<Client>();
            while (reader.Read())
            {
                list.Add(reader.GetClient());
            }
            return list;
        }
        
        /// <inheritdoc />
        public override async Task<ServiceResult<ReferencePackage>> PackAsync(
            Client entry,
            NpgsqlConnectionWrapper oc = null)
        {
            return await this.PackAsync((ReferenceEntryBase) entry, oc);
        }

        /// <inheritdoc />
        public override async Task<ServiceResult<IList<ReferencePackage>>> PackAsync(
            ICollection<Client> entries,
            NpgsqlConnectionWrapper oc = null)
        {
            return await this.PackAsyncInternal(entries, entries.Count, oc);
        }
        
        /// <inheritdoc />
        public override async Task<ServiceResult<ReferencePackage>> PackAsync(
            ReferenceEntryBase entry,
            NpgsqlConnectionWrapper oc = null)
        {
            var package = new ReferencePackage
            {
                Entry = entry,
                Collections = new List<ReferenceCollectionBase>(),
            };

            var addressesResult = await this.clientAddressService.GetAsync(entry.Id, oc: oc);
            if (!addressesResult.Success)
            {
                return new ServiceResult<ReferencePackage>(addressesResult.Errors);
            }

            foreach (var address in addressesResult.Result)
            {
                package.Collections.Add(address);
            }
            
            return new ServiceResult<ReferencePackage>(package);
        }

        /// <inheritdoc />
        public override async Task<ServiceResult<IList<ReferencePackage>>> PackAsync(
            ICollection<ReferenceEntryBase> entries,
            NpgsqlConnectionWrapper oc = null)
        {
            return await this.PackAsyncInternal(entries, entries.Count, oc);
        }

        #endregion
        
        #region private

        private async Task<ServiceResult<IList<ReferencePackage>>> PackAsyncInternal(
            IEnumerable<ReferenceEntryBase> entries,
            int count,
            NpgsqlConnectionWrapper oc = null)
        {
            var packages = entries
                .Select(entry => new ReferencePackage
                {
                    Entry = entry, 
                    Collections = new List<ReferenceCollectionBase>(2 * count)
                })
                .ToList();

            using (var conn = oc?.Connect() ?? this.Cp.Create().Connect())
            {
                foreach (var package in packages)
                {
                    var entry = package.Entry;

                    // TODO: оптимизировать загрузку для нескольких клиентов сразу.
                    // TODO: добавить в CollectionService загрузку по нескольким парентам.
                    var addressesResult = await this.clientAddressService.GetAsync(entry.Id, oc: conn);
                    if (!addressesResult.Success)
                    {
                        return new ServiceResult<IList<ReferencePackage>>(addressesResult.Errors);
                    }

                    foreach (var address in addressesResult.Result)
                    {
                        package.Collections.Add(address);
                    }
                }
            }

            return new ServiceResult<IList<ReferencePackage>>(packages);
        }
        
        #endregion
    }
}