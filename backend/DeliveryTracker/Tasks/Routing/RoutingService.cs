using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Tasks.Routing.GoogleApiModels;
using DeliveryTracker.Validation;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.Tasks.Routing
{
    public sealed class RoutingService: IRoutingService
    {
        #region sql

        private static readonly string GetDailyTasks = $@"
select
    t.id,
    performer_id,
    delivery_from,
    delivery_to,
    ST_X(geoposition::geometry),
    ST_Y(geoposition::geometry)
from tasks t
join client_addresses ca on t.client_address_id = ca.id
where t.instance_id = @instance_id
    and delivery_from::date = @date
    and (state_id = '{DefaultTaskStates.Queue.Id}'
        or state_id = '{DefaultTaskStates.Waiting.Id}'
        or state_id = '{DefaultTaskStates.IntoWork.Id}')
    and geoposition is not null
;
";

        private const string SqlGetPerformers = @"
select 
    id
from users
where instance_id = @instance_id
    and role = @role
;
";

        private const string SqlDeleteOldDailyRoutes = @"
delete from task_routes
where date = @date
;
";

        private const string SqlInsertDailyRouteItem = @"
insert into task_routes (id, instance_id, task_id, performer_id, eta_offset, date)
select
    uuid_generate_v4(),
    @instance_id,
    t.tid,
    t.pid,
    t.eta,
    @date
from unnest(
    @task_id,
    @performer_id,
    @eta) as t (tid, pid, eta)
;
";

        private const string SqlUpdateTask = @"
update tasks
set 
    performer_id = @perf_id,
    delivery_eta = @eta
where id = @task_id
;
";
        
        #endregion
        
        #region fields

        private const int SameTimeInterval = 1800; // +-30 минут

        private const string BaseDistanceApiUrl = "https://maps.googleapis.com/maps/api/distancematrix/json";
        
        private readonly IPostgresConnectionProvider cp;

        private readonly IDeliveryTrackerSerializer serializer;

        private readonly RoutingSettings routingSettings;
        
        private readonly HttpClient client = new HttpClient();
        
        #endregion

        #region constructor
        
        public RoutingService(
            IPostgresConnectionProvider cp,
            IDeliveryTrackerSerializer serializer,
            ISettingsStorage settingsStorage)
        {
            this.cp = cp;
            this.serializer = serializer;
            this.routingSettings = settingsStorage.GetSettings<RoutingSettings>(SettingsName.Routing);
        }

        #endregion

        #region public
        
        public async Task<ServiceResult> BuildDailyRoutesAsync(
            Guid instanceId,
            DateTime? date = null,
            bool tryKeepPerformers = false,
            NpgsqlConnectionWrapper oc = null)
        {
            if (string.IsNullOrWhiteSpace(this.routingSettings.DistanceMatrixApiKey)
                || string.IsNullOrWhiteSpace(this.routingSettings.RoutingServiceUrl))
            {
                return ServiceResult.Successful;
            }
            
            var currentDate = date ?? DateTime.Now;

            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var tasks = await GetTasks(instanceId, currentDate, conn);
                var performers = await GetPerformers(instanceId, conn);
                if (tasks.Count == 0
                    || performers.Count == 0)
                {
                    return ServiceResult.Successful;
                }

                var result = await this.BuildRoutesAsync(tasks, performers, tryKeepPerformers);
                if (!result.Success)
                {
                    return result;
                }

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        await DeleteOldRoutes(instanceId, currentDate, conn);
                        await InsertRoute(instanceId, currentDate, tasks, result.Result, conn);
                        foreach (var route in result.Result)
                        {
                            for (var i = 0; i < route.TaskRoute.Count; i++)
                            {
                                var task = tasks[route.TaskRoute[i]];
                                var eta = currentDate.Date.AddSeconds(route.Eta[i]);
                                await UpdateTask(task.TaskId, route.PerformerId, eta, conn);
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            
            return ServiceResult.Successful;
        }

        public async Task<ServiceResult<List<Route>>> BuildRoutesAsync(
            List<TaskRouteItem> tasks,
            List<Guid> performers,
            bool tryKeepPerformers = false)
        {
            var matrix = await this.BuildWeightMatrix(tasks);
            if (matrix is null)
            {
                return new ServiceResult<List<Route>>(ErrorFactory.BuildRouteError());
            }
            
            var request = new OptimizationRequest
            {
                TryKeepPerformers = tryKeepPerformers,
                Tasks = tasks,
                Performers = performers,
                Weights = matrix,
            };

            var payloadString = this.serializer.SerializeJson(request);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadString);
            var content = new ByteArrayContent(payloadBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentEncoding.Add("UTF-8");
            content.Headers.ContentLength = payloadBytes.Length;
            var response = await this.client.PostAsync(
                this.routingSettings.RoutingServiceUrl, 
                content);
            if (!response.IsSuccessStatusCode)
            {
                return new ServiceResult<List<Route>>(ErrorFactory.BuildRouteError());
            }
            
            var responseString = await response.Content.ReadAsStringAsync();
            var result = this.serializer.DeserializeJson<OptimizationResponse>(responseString);
            if (result.Routes is null)
            {
                return new ServiceResult<List<Route>>(ErrorFactory.BuildRouteError());
            }
            
            return new ServiceResult<List<Route>>(result.Routes);
        }

        #endregion

        #region private

        
        private static async Task<List<TaskRouteItem>> GetTasks(
            Guid instanceId,
            DateTime date,
            NpgsqlConnectionWrapper oc)
        {
            var taskGenes = new List<TaskRouteItem>();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = GetDailyTasks;
                command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                command.Parameters.Add(new NpgsqlParameter("date", date.Date).WithType(NpgsqlDbType.Date));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var task = new TaskRouteItem
                        {
                            TaskId = reader.GetGuid(0),
                            PerformerId = reader.GetValueOrDefault<Guid?>(1),
                            StartTimeOffset = Convert.ToInt32(reader.GetDateTime(2).TimeOfDay.TotalSeconds),
                            EndTimeOffset = Convert.ToInt32(reader.GetDateTime(3).TimeOfDay.TotalSeconds),
                            Longitude = reader.GetDouble(4),
                            Latitude = reader.GetDouble(5),
                        };
                        if (task.EndTimeOffset - task.StartTimeOffset < SameTimeInterval)
                        {
                            var avg = task.StartTimeOffset + (task.EndTimeOffset - task.StartTimeOffset) / 2;
                            
                            task.StartTimeOffset = avg - SameTimeInterval;
                            task.EndTimeOffset = avg + SameTimeInterval;
                        }
                        
                        
                        taskGenes.Add(task);
                    }
                }
            }

            return taskGenes;
        }
        
        private static async Task<List<Guid>> GetPerformers(
            Guid instanceId,
            NpgsqlConnectionWrapper oc)
        {
            var performers = new List<Guid>();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGetPerformers;
                command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                command.Parameters.Add(new NpgsqlParameter("role", DefaultRoles.PerformerRole));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        performers.Add(reader.GetGuid(0));
                    }
                }
            }

            return performers;
        }

        private async Task<List<List<int>>> BuildWeightMatrix(
            List<TaskRouteItem> tasks)
        {
            var allCoordinates = string.Join("|", tasks.Select(FormatCoordinates));
            var arrivalTime = new DateTimeOffset(DateTime.Now.Date.AddHours(13)).ToUnixTimeSeconds();
            var url =
                $"{BaseDistanceApiUrl}?origins={allCoordinates}&destinations={allCoordinates}&arrival_time={arrivalTime}&key={this.routingSettings.DistanceMatrixApiKey}";
            
            var response = await this.client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var responseString = await response.Content.ReadAsStringAsync();
            var result = this.serializer.DeserializeJson<GoogleDistanceMatrixResponse>(responseString);
            if (result.Status != "OK")
            {
                return null;
            }
            
            var matrix = new List<List<int>>(tasks.Count);

            var rowIdx = 0;
            foreach (var row in result.Rows)
            {
                var columnIdx = 0;
                var matrixRow = new List<int>(tasks.Count);
                foreach (var element in row.Elements)
                {
                    if (rowIdx == columnIdx
                        || element.Status != "OK")
                    {
                        matrixRow.Add(int.MaxValue);
                    }
                    else
                    {
                        matrixRow.Add(element.Duration.Value);
                    }
                    
                    columnIdx++;
                }
                matrix.Add(matrixRow);
                rowIdx++;
            }
            
            return matrix;
        }

        private static string FormatCoordinates(
            TaskRouteItem p) =>
            $"{p.Latitude.ToString(NumberFormatInfo.InvariantInfo)},{p.Longitude.ToString(NumberFormatInfo.InvariantInfo)}";


        private static async Task DeleteOldRoutes(Guid instanceId, DateTime date, NpgsqlConnectionWrapper oc)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlDeleteOldDailyRoutes;
                command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                command.Parameters.Add(new NpgsqlParameter("date", date.Date).WithType(NpgsqlDbType.Date));
                await command.ExecuteNonQueryAsync();
            }
        }

        
        private static async Task InsertRoute(
            Guid instanceId,
            DateTime date,
            List<TaskRouteItem> tasks,
            List<Route> routes,
            NpgsqlConnectionWrapper oc)
        {
            var taskIds = new List<Guid>(routes.Count * 10);
            var performerIds = new List<Guid>(routes.Count * 10);
            var eta = new List<int>(routes.Count * 10);

            foreach (var route in routes)
            {
                var vertexIndex = 0;
                foreach (var vertex in route.TaskRoute)
                {
                    taskIds.Add(tasks[vertex].TaskId);
                    performerIds.Add(route.PerformerId);
                    eta.Add(route.Eta[vertexIndex]);
                    vertexIndex++;
                }
            }
            
            using (var comm = oc.CreateCommand())
            {
                comm.CommandText = SqlInsertDailyRouteItem;
                comm.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                comm.Parameters.Add(new NpgsqlParameter("date", date.Date).WithType(NpgsqlDbType.Date));
                comm.Parameters.Add(new NpgsqlParameter("task_id", taskIds).WithArrayType(NpgsqlDbType.Uuid));
                comm.Parameters.Add(new NpgsqlParameter("performer_id", performerIds).WithArrayType(NpgsqlDbType.Uuid));
                comm.Parameters.Add(new NpgsqlParameter("eta", eta).WithArrayType(NpgsqlDbType.Integer));
                await comm.ExecuteNonQueryAsync();
            }
        }

        private static async Task UpdateTask(
            Guid taskId,
            Guid perfId,
            DateTime eta,
            NpgsqlConnectionWrapper oc)
        {
            using (var comm = oc.CreateCommand())
            {
                comm.CommandText = SqlUpdateTask;
                comm.Parameters.Add(new NpgsqlParameter("task_id", taskId));
                comm.Parameters.Add(new NpgsqlParameter("perf_id", perfId));
                comm.Parameters.Add(new NpgsqlParameter("eta", eta).WithType(NpgsqlDbType.Timestamp));
                await comm.ExecuteNonQueryAsync();
            }
        }
        
        #endregion
    }
}