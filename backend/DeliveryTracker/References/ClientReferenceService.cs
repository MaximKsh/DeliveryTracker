using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.References
{
    public class ClientReferenceService : ReferenceServiceBase<Client>
    {
        #region sql
        
        private static readonly string SqlCreate = @"
insert into clients (" + ReferenceHelper.GetClientColumns() + @")
values (" + ReferenceHelper.GetClientColumns("@") + @")
returning " + ReferenceHelper.GetClientColumns() + ";";

        private static readonly string SqlCreateAddresses = @"
insert into client_addresses (
    id,
    instance_id,
    parent_id,
    raw_address)
values {0}
returning " + ReferenceHelper.GetAddressColumns() + ";";

        private static readonly string SqlAddressValues = @"
(
    @n{0}_id,
    @n{0}_instance_id,
    @n{0}_parent_id,
    @n{0}_raw_address)";
        

        private static readonly string SqlGet = @"
select " + ReferenceHelper.GetClientColumns() + @"
from clients
where id = @id and instance_id = @instance_id
;";
        private static readonly string SqlGetAddresses = @"
select " + ReferenceHelper.GetAddressColumns() + @"
from client_addresses
where instance_id = @instance_id and parent_id = @id;";

        private static readonly string SqlGetFull = 
            SqlGet + SqlGetAddresses;
        
        
        
        
        private static readonly string SqlUpdate = @"
update clients
set
{0}
where id = @id and instance_id = @instance_id
returning " + ReferenceHelper.GetClientColumns() + @";";
        
        private static readonly string SqlCreateAddressesReturning1 = @"
insert into client_addresses (
    id,
    instance_id,
    parent_id,
    raw_address)
values {0}
returning 1;
";
        
        private static readonly string SqlUpdateAddress = @"
update client_addresses
set
{0}
where id = @n{1}_address_id and instance_id = @instance_id and parent_id = @id
returning 1;
";
        
        private static readonly string SqlDeleteAddresses = @"
delete from client_addresses
where ARRAY[id] <@ @address_ids and instance_id = @instance_id and parent_id = @id
returning 1
;";
        
        
        
        private const string SqlDelete = @"
delete from client_addresses
where instance_id = @instance_id and parent_id = @id
returning 1
;
delete from clients
where id = @id and instance_id = @instance_id
returning 1
;
";
        
        #endregion
        
        #region fields

        private const string QueriesCountKey = "QueriesCountKey";
        
        #endregion

        #region constructor
        
        public ClientReferenceService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
        }

        #endregion

        #region public

        public override string Name { get; } = nameof(Client);
        
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
            var builder = new StringBuilder();
            builder.AppendLine(SqlCreate);
            command.Parameters.Add(new NpgsqlParameter("surname", newData.Surname).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("name", newData.Name).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("patronymic", newData.Patronymic).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("phone_number", newData.PhoneNumber).CanBeNull());

            if (newData.Addresses != null
                && newData.Addresses.Count > 0)
            {
                var addressCount = 0;
                var valuesArray = new string[newData.Addresses.Count];
                foreach (var address in newData.Addresses)
                {
                    var addressCountStr = addressCount.ToString();
                    valuesArray[addressCount] = string.Format(SqlAddressValues, addressCountStr);
                    command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_instance_id", credentials.InstanceId));
                    command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_parent_id", id));
                    command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_raw_address", address.RawAddress).CanBeNull());
                    addressCount++;
                }

                var values = string.Join(", ", valuesArray);
                builder.AppendFormat(SqlCreateAddresses, values);
            }
            
            command.CommandText = builder.ToString();

            return null;
        }

        protected override ExecutionParameters SetCommandEdit(
            NpgsqlCommand command,
            Client newData, 
            UserCredentials credentials)
        {
            var queryStringBuilder = new StringBuilder();
            var updateFieldsBuilder = new StringBuilder();
            
            var appendedFields = 0;
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder, "surname", newData.Surname);
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder, "name", newData.Name);
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder,"patronymic", newData.Patronymic);
            appendedFields += command.AppendIfNotDefault(updateFieldsBuilder, "phone_number", newData.Patronymic);
            queryStringBuilder.AppendLine(appendedFields != 0 
                ? string.Format(SqlUpdate, updateFieldsBuilder) 
                : SqlGet);

            var addressQueriesCount = 0;
            if (newData.Addresses != null
                && newData.Addresses.Count > 0)
            {
                var addrToDelete = new List<Guid>(newData.Addresses.Count);
                var valuesArray = new List<string>(newData.Addresses.Count);
                var updateArray = new List<string>(newData.Addresses.Count);
                var cnt = 0;
                foreach (var address in newData.Addresses)
                {
                    var cntStr = cnt.ToString();
                    switch (address.Action)
                    {
                        case CollectionEntityAction.Create:
                            CreateAddressValues(command, cntStr, valuesArray, newData.Id,
                                credentials.InstanceId, address);
                            break;
                        case CollectionEntityAction.Edit:
                            UpdateAddressQuery(command, cntStr, updateArray, address);
                            break;
                        case CollectionEntityAction.Delete:
                            addrToDelete.Add(address.Id);
                            break;
                        case CollectionEntityAction.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(address.Action));
                    }

                    cnt++;
                }

                if (valuesArray.Count > 0)
                {
                    var values = string.Join(", ", valuesArray);
                    queryStringBuilder.AppendFormat(SqlCreateAddressesReturning1, values);
                    addressQueriesCount++;
                }
                
                if (addrToDelete.Count > 0)
                {
                    queryStringBuilder.AppendLine(SqlDeleteAddresses);
                    command.Parameters.Add(new NpgsqlParameter("address_ids", addrToDelete)
                    {
                        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                        NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid
                    });
                    addressQueriesCount++;
                }

                if (updateArray.Count > 0)
                {
                    addressQueriesCount += updateArray.Count;
                    foreach (var updQuery in updateArray)
                    {
                        queryStringBuilder.Append(updQuery);
                    }
                }
            }
            
            queryStringBuilder.AppendLine(SqlGetAddresses);
            command.CommandText = queryStringBuilder.ToString();
            return new ExecutionParameters
            {
                [QueriesCountKey] = addressQueriesCount,
            };
        }

        protected override ExecutionParameters SetCommandGet(
            NpgsqlCommand command,
            Guid id, 
            UserCredentials credentials)
        {
            command.CommandText = SqlGetFull;
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
            var client = reader.GetClient();
            reader.NextResult();
            
            if (ctx.Action == ReferenceExecutionAction.Edit
                && ctx.Parameters.TryGetValue(QueriesCountKey, out var cntObj)
                && cntObj is int cnt)
            {
                for (; 0 < cnt; cnt--)
                {
                    reader.Read();
                    reader.NextResult();
                }
            }
            
            client.Addresses = new List<Address>();
            while (reader.Read())
            {
                client.Addresses.Add(reader.GetAddress());
            }

            return client;
        }

        protected override async Task<bool> ExecDeleteAsync(NpgsqlCommand command, ReferenceServiceExecutionContext ctx)
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                reader.Read();
                reader.NextResult();
                return reader.Read() && reader.GetInt32(0) == 1;
            }
        }
        
        #endregion
        
        #region private

        private static void CreateAddressValues(
            NpgsqlCommand command,
            string addressCountStr,
            List<string> valuesArray,
            Guid parentId,
            Guid instanceId,
            Address address)
        {
            valuesArray.Add(string.Format(SqlAddressValues, addressCountStr));
            command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_id", Guid.NewGuid()));
            command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_parent_id", parentId));
            command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_instance_id", instanceId));
            command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_raw_address", address.RawAddress).CanBeNull());
        }

        private static void UpdateAddressQuery(
            NpgsqlCommand command,
            string addressCountStr,
            List<string> updateArray,
            Address address)
        {
            var sb = new StringBuilder();
            var appendedFields = 0;
            command.Parameters.Add(new NpgsqlParameter($"n{addressCountStr}_address_id", address.Id));
            appendedFields += command.AppendIfNotDefault(
                sb, $"n{addressCountStr}_raw_address", address.RawAddress, "raw_address");

            if (appendedFields > 0)
            {
                updateArray.Add(string.Format(SqlUpdateAddress, sb.ToString(), addressCountStr));
            }
            
        }
        
        #endregion
    }
}