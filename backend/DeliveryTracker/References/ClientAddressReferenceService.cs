using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using Npgsql;

namespace DeliveryTracker.References
{
    public sealed class ClientAddressReferenceService : CollectionReferenceServiceBase<ClientAddress>
    {
        private const string EmptySelect = "select 1";
        
        private const string SqlCreateAddresses = @"
insert into client_addresses (
    id,
    instance_id,
    parent_id,
    raw_address
    {0})
values (
    @id, 
    @instance_id,
    @parent_id, 
    @raw_address
    {1});";

        private static readonly string SqlGetAddresses = $@"
select {ReferenceHelper.GetAddressColumns()}
from client_addresses
where instance_id = @instance_id and parent_id = @parent_id and deleted = false
;";
        
        private static readonly string SqlGetAddressesWithDeleted = $@"
select {ReferenceHelper.GetAddressColumns()}
from client_addresses
where instance_id = @instance_id and parent_id = @parent_id
;";
        
        private const string SqlUpdateAddress = @"
update client_addresses
set
{0}
where id = @id and instance_id = @instance_id and parent_id = @parent_id and deleted = false
;
";
        
        private const string SqlDeleteAddresses = @"
update client_addresses
set deleted = true
where id = @id and instance_id = @instance_id and parent_id = @parent_id and deleted = false
;";
        
        
        public ClientAddressReferenceService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
        }

        protected override ExecutionParameters SetCommandCreate(
            NpgsqlCommand command,
            ClientAddress newData,
            Guid id,
            UserCredentials credentials)
        {
            command.Parameters.Add(new NpgsqlParameter("raw_address", newData.RawAddress));
            var geopositionColumnName = string.Empty;
            var geopositionColumnValue = string.Empty;

            if (newData.Geoposition != null)
            {
                geopositionColumnName = ", geoposition";
                geopositionColumnValue = ", st_setsrid(ST_MakePoint(@lon, @lat), 4326)::geography";
                command.Parameters.Add(new NpgsqlParameter("lon", newData.Geoposition.Longitude));
                command.Parameters.Add(new NpgsqlParameter("lat", newData.Geoposition.Latitude));
            } 

            command.CommandText = string.Format(SqlCreateAddresses, geopositionColumnName, geopositionColumnValue);

            return null;
        }

        protected override ExecutionParameters SetCommandEdit(
            NpgsqlCommand command,
            ClientAddress newData,
            UserCredentials credentials)
        {
            var sb = new StringBuilder();
            var appendedFields = 0;
            appendedFields += command.AppendIfNotDefault(sb, "raw_address", newData.RawAddress);
           /* if (newData.Geoposition != null)
            {
                if (appendedFields != 0)
                {
                    sb.Append(",");
                }
                sb.AppendLine("geoposition = st_setsrid(ST_MakePoint(@lon, @lat), 4326)::geography");

                command.Parameters.Add(new NpgsqlParameter("lon", newData.Geoposition.Longitude));
                command.Parameters.Add(new NpgsqlParameter("lat", newData.Geoposition.Latitude));
            } */

            command.CommandText = appendedFields > 0
                ? string.Format(SqlUpdateAddress, sb.ToString())
                : EmptySelect;

            return null;
        }

        protected override ExecutionParameters SetCommandGet(
            NpgsqlCommand command,
            Guid id,
            bool withDeleted,
            UserCredentials credentials)
        {
            command.CommandText = withDeleted 
                ? SqlGetAddressesWithDeleted
                : SqlGetAddresses;
            return null;
        }

        protected override ExecutionParameters SetCommandDelete(
            NpgsqlCommand command,
            Guid id,
            UserCredentials credentials)
        {
            command.CommandText = SqlDeleteAddresses;
            return null;
        }

        protected override IList<ClientAddress> ReadList(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx)
        {
            var addresses = new List<ClientAddress>();
            while (reader.Read())
            {
                addresses.Add(reader.GetAddress());
            }

            return addresses;
        }
    }
}