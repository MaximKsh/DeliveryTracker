﻿using System;
using System.Data;
using System.Text;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using Npgsql;

namespace DeliveryTracker.References
{
    public class ProductReferenceService : ReferenceServiceBase<Product>
    {
        #region sql
        
        private static readonly string SqlCreate = @"
insert into products (" + ReferenceHelper.GetProductColumns() + @")
values (" + ReferenceHelper.GetProductColumns("@") + @")
returning " + ReferenceHelper.GetProductColumns() + ";";

        private static readonly string SqlUpdate = @"
update products
set
{0}
where id = @id and instance_id = @instance_id
returning " + ReferenceHelper.GetProductColumns() + ";";

        private static readonly string SqlGet = @"
select " + ReferenceHelper.GetProductColumns() + @"
from products
where id = @id and instance_id = @instance_id;";
        
        private const string SqlDelete = @"
delete from products
where id = @id and instance_id = @instance_id
;
";
        
        #endregion
        
        #region fields

        #endregion

        #region constructor
        
        public ProductReferenceService(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
        }

        #endregion

        #region public

        public override string Name { get; } = nameof(Product);

        #endregion
        
        #region protected

        protected override ExecutionParameters SetCommandCreate(
            NpgsqlCommand command, 
            Product newData, 
            Guid id, 
            UserCredentials credentials)
        {
            command.CommandText = SqlCreate;
            command.Parameters.Add(new NpgsqlParameter("vendor_code", newData.VendorCode).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("name", newData.Name).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("description", newData.Description).CanBeNull());
            command.Parameters.Add(new NpgsqlParameter("cost", newData.Cost).CanBeNull());
            
            return null;
        }

        protected override ExecutionParameters SetCommandEdit(
            NpgsqlCommand command, 
            Product newData, 
            UserCredentials credentials)
        {
            var queryStringBuilder = new StringBuilder();
            var parametersCounter = 0;
            parametersCounter += command.AppendIfNotDefault(queryStringBuilder, "vendor_code", newData.VendorCode);
            parametersCounter += command.AppendIfNotDefault(queryStringBuilder, "name", newData.Name);
            parametersCounter += command.AppendIfNotDefault(queryStringBuilder,"description", newData.Description);
            parametersCounter += command.AppendIfNotDefault(queryStringBuilder, "cost", newData.Cost);

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

        protected override Product Read(IDataReader reader, ReferenceServiceExecutionContext ctx)
        {
            return reader.GetProduct();
        }

        #endregion
    }
}