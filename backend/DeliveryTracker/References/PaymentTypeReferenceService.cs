using System;
using System.Data;
using System.Text;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using Npgsql;

namespace DeliveryTracker.References
{
    public class PaymentTypeReferenceService : ReferenceServiceBase<PaymentType>
    {
        #region sql
        
        private static readonly string SqlCreate = @"
insert into payment_types (" + ReferenceHelper.GetPaymentTypeColumns() + @")
values (" + ReferenceHelper.GetPaymentTypeColumns("@") + @")
returning " + ReferenceHelper.GetPaymentTypeColumns() + ";";

        private static readonly string SqlUpdate = @"
update payment_types
set
{0}
where id = @id and instance_id = @instance_id
returning " + ReferenceHelper.GetPaymentTypeColumns() + ";";

        private static readonly string SqlGet = @"
select " + ReferenceHelper.GetPaymentTypeColumns() + @"
from payment_types
where id = @id and instance_id = @instance_id;";
        
        private const string SqlDelete = @"
delete from payment_types
where id = @id and instance_id = @instance_id
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

        public override string Name { get; } = nameof(PaymentType);

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

        protected override PaymentType Read(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx)
        {
            return reader.GetPaymentType();
        }
        
        #endregion
    }
}