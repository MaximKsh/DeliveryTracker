using System;
using System.Data;
using System.Text;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using Npgsql;

namespace DeliveryTracker.References
{
    public class WarehouseReferenceService : ReferenceServiceBase<Warehouse>
    {
        
        #region sql
        
        private static readonly string SqlCreate = @"
insert into warehouses (
    id,
    instance_id,
    name,
    raw_address
)
values (
    @id,
    @instance_id,
    @name,
    @raw_address
)
returning " + ReferenceHelper.GetWarehouseColumns() + ";";

        private static readonly string SqlUpdate = @"
update warehouses
set
{0}
where id = @id and instance_id = @instance_id
returning " + ReferenceHelper.GetWarehouseColumns() + ";";

        private static readonly string SqlGet = @"
select " + ReferenceHelper.GetWarehouseColumns() + @"
from warehouses
where id = @id and instance_id = @instance_id;";
        
        private const string SqlDelete = @"
delete from warehouses
where id = @id and instance_id = @instance_id
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

        public override string Name { get; } = nameof(Warehouse);

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
            command.CommandText = SqlCreate;
            command.Parameters.Add(new NpgsqlParameter("name", newData.Name).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("raw_address", newData.RawAddress).CanBeNull());
            
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

            command.CommandText = parametersCounter > 0
                ? string.Format(SqlUpdate, queryStringBuilder.ToString())
                : SqlGet;
            return null;
        }

        protected override ExecutionParameters SetCommandGet(
            NpgsqlCommand command,
            Guid id,
            UserCredentials credentials)
        {
            command.CommandText = SqlGet;
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

        #endregion
    }
}