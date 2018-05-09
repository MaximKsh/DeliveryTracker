using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;
using Npgsql;

namespace DeliveryTracker.Views.AuxTasks
{
    public class UserTasksView : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {TaskHelper.GetTasksColumns()}
from tasks
where instance_id = @instance_id 
    {{0}}
order by created desc
limit {ViewHelper.DefaultViewLimit}
;
";

        private const string SqlCount = @"
select count(1)
from tasks
where instance_id = @instance_id 
    {0}
;
";
        #endregion
        
        #region fields
        
        protected readonly int Order;
        protected readonly ITaskService TaskService;
        
        #endregion
        
        #region constuctor
        
        public UserTasksView(
            int order,
            ITaskService taskService)
        {
            this.Order = order;
            this.TaskService = taskService;
        }
        
        #endregion
        
        #region implementation

        public virtual string Name { get; } = nameof(UserTasksView);
        
        public virtual IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole,
            DefaultRoles.PerformerRole,
        }.AsReadOnly();

        protected virtual ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.UserTasksView,
                Count = count,
                EntityType = nameof(TaskInfo),
                Order = this.Order,
            };
        }
        
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
        public virtual async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<TaskInfo>();
            using (var command = oc.CreateCommand())
            {
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                var sb = new StringBuilder(256);
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search", "task_number");
                ViewHelper.TryAddEqualsParameter<Guid>(parameters, command, sb, "author_id");
                ViewHelper.TryAddEqualsParameter<Guid>(parameters, command, sb, "performer_id");

                ViewHelper.TryAddAfterParameter(parameters, command, sb, "created", "tasks", true);
                command.CommandText = string.Format(SqlGet, sb.ToString());
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetTaskInfo());
                    }
                }
            }

            var package = await this.TaskService.PackTasksAsync(list, oc);
            
            return new ServiceResult<IList<IDictionaryObject>>(new List<IDictionaryObject> { package.Result });
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            using (var command = oc.CreateCommand())
            {
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                var sb = new StringBuilder(256);
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search", "task_number");
                ViewHelper.TryAddEqualsParameter<Guid>(parameters, command, sb, "author_id");
                ViewHelper.TryAddEqualsParameter<Guid>(parameters, command, sb, "performer_id");

                command.CommandText = string.Format(SqlCount, sb.ToString());

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
        
        #endregion
    }
}