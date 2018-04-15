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
        
        #region constructor
        
        public ClientReferenceService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
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

        #endregion
    }
}