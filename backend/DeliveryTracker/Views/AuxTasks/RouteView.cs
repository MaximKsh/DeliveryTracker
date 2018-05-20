using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.Views.AuxTasks
{
    public sealed class RouteView: IView
    {
        #region sql

        private static readonly string SqlGet = $@"
select {TaskHelper.GetTasksColumns("t.")}
from tasks t
join task_routes tr on t.id = tr.task_id
where tr.instance_id = @instance_id
    and tr.performer_id = @performer_id
    and tr.date = @date
order by tr.eta_offset
;
";

        private const string SqlCount = @"
select count(1)
from task_routes
where instance_id = @instance_id
    and performer_id = @performer_id
    and date = @date;
";

        #endregion
        
        #region fields

        private readonly int order;

        private readonly ITaskService taskService;

        #endregion
        
        #region constuctor

        public RouteView(
            int order,
            ITaskService taskService)
        {
            this.order = order;
            this.taskService = taskService;
        }
        
        #endregion
            
        #region implementation

        public string Name { get; } = nameof(RouteView);
        
        public IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole,
            DefaultRoles.PerformerRole,
        }.AsReadOnly();

        private ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.RouteView,
                Count = count,
                EntityType = nameof(TaskInfo),
                Order = this.order,
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
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<TaskInfo>();
            var performerId = parameters.TryGetOneValue("performer_id", out var idStr)
                              && Guid.TryParse(idStr, out var id)
                ? id
                : userCredentials.Id;
            
            using (var command = oc.CreateCommand())
            {
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("date", DateTime.Now.Date).WithType(NpgsqlDbType.Date));
                command.Parameters.Add(new NpgsqlParameter("performer_id", performerId));
                command.CommandText = SqlGet;
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetTaskInfo());
                    }
                }
            }

            var package = await this.taskService.PackTasksAsync(list, oc);
            
            return new ServiceResult<IList<IDictionaryObject>>(new List<IDictionaryObject> { package.Result });
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            
            var performerId = parameters.TryGetOneValue("performer_id", out var idStr)
                              && Guid.TryParse(idStr, out var id)
                ? id
                : userCredentials.Id;
            
            using (var command = oc.CreateCommand())
            {
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("date", DateTime.Now.Date).WithType(NpgsqlDbType.Date));
                command.Parameters.Add(new NpgsqlParameter("performer_id", performerId));
                command.CommandText = SqlCount;

                return new ServiceResult<long>((long)await command.ExecuteScalarAsync());
            }
        }
        
        #endregion
    }
}