﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using Npgsql;

namespace DeliveryTracker.References
{
    public sealed class PaymentTypeReferenceService : EntryReferenceServiceBase<PaymentType>
    {
        #region sql
        
        private static readonly string SqlCreate = @"
insert into payment_types (" + ReferenceHelper.GetPaymentTypeColumns() + @")
values (" + ReferenceHelper.GetPaymentTypeColumns("@") + @")
returning " + ReferenceHelper.GetPaymentTypeColumns() + ";";

        private static readonly string SqlUpdate = $@"
update payment_types
set
{{0}}
where id = @id and instance_id = @instance_id and deleted = false
returning {ReferenceHelper.GetPaymentTypeColumns()};";

        
        private static readonly string SqlGetWithDeleted = @"
select " + ReferenceHelper.GetPaymentTypeColumns() + @"
from payment_types
where id = @id and instance_id = @instance_id;";
        
        private static readonly string SqlGet = @"
select " + ReferenceHelper.GetPaymentTypeColumns() + @"
from payment_types
where id = @id and instance_id = @instance_id and deleted = false;";
        
        private static readonly string SqlGetListWithDeleted = @"
select " + ReferenceHelper.GetPaymentTypeColumns() + @"
from payment_types
where id = ANY (@ids) and instance_id = @instance_id;";
        
        private static readonly string SqlGetList = @"
select " + ReferenceHelper.GetPaymentTypeColumns() + @"
from payment_types
where id = ANY (@ids) and instance_id = @instance_id and deleted = false;";
        
        private const string SqlDelete = @"
update payment_types
set deleted = true
where id = @id and instance_id = @instance_id and deleted = false
;
";
        
        #endregion
        
        #region constructor
        
        public PaymentTypeReferenceService(
            IPostgresConnectionProvider cp, 
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
        }
        
        #endregion

        #region public

        public override ReferenceDescription ReferenceDescription { get; } = new ReferenceDescription
        {
            Caption = LocalizationAlias.References.PaymentTypes
        };
        
        #endregion
        
        #region protected
        
        protected override ExecutionParameters SetCommandCreate(
            NpgsqlCommand command,
            PaymentType newData, 
            Guid id, 
            UserCredentials credentials)
        {
            command.CommandText = SqlCreate;
            command.Parameters.Add(new NpgsqlParameter("name", newData.Name).CanBeNull());
            return null;
        }

        protected override ExecutionParameters SetCommandEdit(
            NpgsqlCommand command, 
            PaymentType newData,
            UserCredentials credentials)
        {
            var sb = new StringBuilder();

            var cnt = command.AppendIfNotDefault(sb, "name", newData.Name);
            command.CommandText = cnt == 1
                ? string.Format(SqlUpdate, sb.ToString())
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

        protected override PaymentType Read(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx)
        {
            return reader.GetPaymentType();
        }
        
        
        protected override IList<PaymentType> ReadList(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx)
        {
            var list = new List<PaymentType>();
            while (reader.Read())
            {
                list.Add(reader.GetPaymentType());
            }

            return list;
        }
        
        #endregion
    }
}