using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using Npgsql;

namespace DeliveryTracker.References
{
    public sealed class WarehouseReferenceService : EntryReferenceServiceBase<Warehouse>
    {
        #region sql
        
        private static readonly string SqlCreate = $@"
insert into warehouses (
    id,
    instance_id,
    name,
    raw_address
    {{0}}
)
values (
    @id,
    @instance_id,
    @name,
    @raw_address
    {{1}}
)
returning {ReferenceHelper.GetWarehouseColumns()};";

        private static readonly string SqlUpdate = $@"
update warehouses
set
{{0}}
where id = @id and instance_id = @instance_id and deleted = false
returning {ReferenceHelper.GetWarehouseColumns()};";

        private static readonly string SqlGetWithDeleted = $@"
select {ReferenceHelper.GetWarehouseColumns()}
from warehouses
where id = @id and instance_id = @instance_id;";
        
        private static readonly string SqlGet = $@"
select {ReferenceHelper.GetWarehouseColumns()}
from warehouses
where id = @id and instance_id = @instance_id and deleted = false;";
        
        private static readonly string SqlGetListWithDeleted = $@"
select {ReferenceHelper.GetWarehouseColumns()}
from warehouses
where id = ANY (@ids) and instance_id = @instance_id;";
        
        private static readonly string SqlGetList = $@"
select {ReferenceHelper.GetWarehouseColumns()}
from warehouses
where id = ANY (@ids) and instance_id = @instance_id and deleted = false;";
        
        private const string SqlDelete = @"
update warehouses
set deleted = true
where id = @id and instance_id = @instance_id  and deleted = false
;
";
        
        #endregion
        
        #region constructor
        
        public WarehouseReferenceService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
        }

        #endregion
        
        #region public

        public override ReferenceDescription ReferenceDescription { get; } = new ReferenceDescription
        {
            Caption = LocalizationAlias.References.Warehouses
        };

        #endregion

        #region base overrides
        
        protected override ExecutionParameters SetCommandCreate(
            NpgsqlCommand command,
            Warehouse newData,
            Guid id,
            UserCredentials credentials)
        {
            command.Parameters.Add(new NpgsqlParameter("name", newData.Name).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("raw_address", newData.RawAddress).CanBeNull());
            var geopositionColumnName = string.Empty;
            var geopositionColumnValue = string.Empty;
            if (newData.Geoposition != null)
            {
                geopositionColumnName = ", geoposition";
                geopositionColumnValue = ", st_setsrid(ST_MakePoint(@lon, @lat), 4326)::geography";
                command.Parameters.Add(new NpgsqlParameter("lon", newData.Geoposition.Longitude));
                command.Parameters.Add(new NpgsqlParameter("lat", newData.Geoposition.Latitude));
            }            
            command.CommandText = string.Format(SqlCreate, geopositionColumnName, geopositionColumnValue);

            return null;
        }

        protected override ExecutionParameters SetCommandEdit(
            NpgsqlCommand command,
            Warehouse newData,
            UserCredentials credentials)
        {
            var queryStringBuilder = new StringBuilder();
            var parametersCounter = 0;
            parametersCounter += command.AppendIfNotDefault(queryStringBuilder, "name", newData.Name);
            parametersCounter += command.AppendIfNotDefault(queryStringBuilder, "raw_address", newData.RawAddress);

            if (newData.Geoposition != null)
            {
                if (parametersCounter != 0)
                {
                    queryStringBuilder.Append(",");
                }
                queryStringBuilder.AppendLine("geoposition = st_setsrid(ST_MakePoint(@lon, @lat), 4326)::geography");

                command.Parameters.Add(new NpgsqlParameter("lon", newData.Geoposition.Longitude));
                command.Parameters.Add(new NpgsqlParameter("lat", newData.Geoposition.Latitude));
            } 
            
            command.CommandText = parametersCounter > 0
                ? string.Format(SqlUpdate, queryStringBuilder.ToString())
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

        protected override Warehouse Read(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx) =>
            reader.GetWarehouse();

        protected override IList<Warehouse> ReadList(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx)
        {
            var list = new List<Warehouse>();
            while (reader.Read())
            {
                list.Add(reader.GetWarehouse());
            }

            return list;
        }
            

        #endregion
    }
}