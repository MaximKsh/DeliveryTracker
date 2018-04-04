using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Tasks;
using Npgsql;

namespace DeliveryTracker.Views
{
    public abstract class TaskViewBase : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {TaskHelper.GetTasksColumns()}
from tasks
where instance_id = @instance_id 
    and ({{0}})
    {{1}}


order by created desc
limit {ViewHelper.DefaultViewLimit}
;
";

        private const string SqlCount = @"
select count(1)
from tasks
where instance_id = @instance_id 
    and ({0})
;
";
        #endregion
        
        
        #region fields
        
        protected readonly int Order;

        private readonly Lazy<string> sqlGetLazy;
        private readonly Lazy<string> sqlCountLazy;
        
        #endregion
        
        #region constuctor
        
        protected TaskViewBase(
            int order)
        {
            this.Order = order;
            this.sqlGetLazy = new Lazy<string>(() => this.ExtendSqlGet(SqlGet), LazyThreadSafetyMode.PublicationOnly);
            this.sqlCountLazy = new Lazy<string>(() => this.ExtendSqlCount(SqlCount), LazyThreadSafetyMode.PublicationOnly);
        }
        
        #endregion
        
        #region implementation

        protected abstract ViewDigest ViewDigestFactory(long count);

        protected abstract string ExtendSqlGet(string sqlGet);
        
        protected abstract string ExtendSqlCount(string sqlCount);
        
        /// <inheritdoc />
        public abstract string Name { get; }
        
        /// <inheritdoc />
        public abstract IReadOnlyList<Guid> PermittedRoles { get; }
        
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
            return new ServiceResult<ViewDigest>(this.ViewDigestFactory(result.Result));
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<IDictionaryObject>();
            using (var command = oc.CreateCommand())
            {
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("user_id", userCredentials.Id));
                
                var sb = new StringBuilder(256);
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search", "task_number");
                ViewHelper.TryAddAfterParameter(parameters, command, sb, "warehouses", "name");
                command.CommandText = string.Format(this.sqlGetLazy.Value, sb.ToString());
                
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetTaskInfo());
                    }
                }
            }
            
            return new ServiceResult<IList<IDictionaryObject>>(list);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = this.sqlCountLazy.Value;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("user_id", userCredentials.Id));

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
        
        #endregion
        
    }
}