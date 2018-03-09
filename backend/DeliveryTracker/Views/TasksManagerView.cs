using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.References;
using DeliveryTracker.Tasks;
using Npgsql;

namespace DeliveryTracker.Views
{
    public class TasksManagerView : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {TaskHelper.GetTasksColumns()}
from tasks
where instance_id = @instance_id and author_id = @author_id
;
";

        private const string SqlCount = @"
select count(1)
from tasks
where instance_id = @instance_id and author_id = @author_id
;
";
        #endregion
        
        #region fields
        
        private readonly int order;
        
        #endregion
        
        #region constuctor
        
        public TasksManagerView(
            int order)
        {
            this.order = order;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(TasksManagerView);
        
        /// <inheritdoc />
        public IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole
        }.AsReadOnly();
        
        /// <inheritdoc />
        public async Task<ServiceResult<ViewDigest>> GetViewDigestAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            var result = await this.GetCountAsync(oc, userCredentials, parameters);
            if (!result.Success)
            {
                return new ServiceResult<ViewDigest>(result.Errors);
            }
            return new ServiceResult<ViewDigest>(new ViewDigest
            {
                Caption = LocalizationAlias.Views.TasksManagersView,
                Count = result.Result,
                EntityType = nameof(TaskInfo),
                Order = this.order,
                IconName = "1"
            });
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            var list = new List<IDictionaryObject>();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGet;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("author_id", userCredentials.Id));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetProduct());
                    }
                }
            }
            
            return new ServiceResult<IList<IDictionaryObject>>(list);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlCount;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("author_id", userCredentials.Id));

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
        
        #endregion
    }
}