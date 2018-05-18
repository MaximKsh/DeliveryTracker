using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.Variable;
using DeliveryTracker.MOEA.Metaheuristics.MOEAD;
using DeliveryTracker.MOEA.Operators.Crossover;
using DeliveryTracker.MOEA.Operators.Mutation;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.Tasks.Routing
{
    public sealed class RoutingService: IRoutingService
    {
        #region sql

        private const string GetDailyTasks = @"
select
    t.id,
    performer_id,
    delivery_from,
    delivery_to,
    ST_X(geoposition::geometry) as xpos,
    ST_Y(geoposition::geometry) as ypos
from tasks t
join client_addresses ca on t.client_address_id = ca.id
where t.instance_id = @instance_id
    and delivery_from::date = @date
    and state_id = @state_id
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
values (uuid_generate_v4(), @instance_id, @task_id, @performer_id, @eta_offset, @date)
;
";
        
        #endregion
        
        #region fields
        
        private readonly IPostgresConnectionProvider cp;

        #endregion

        #region constructor
        
        public RoutingService(
            IPostgresConnectionProvider cp)
        {
            this.cp = cp;
        }

        #endregion

        #region public
        
        public async Task<ServiceResult> BuildDailyRoutesAsync(
            Guid instanceId,
            DateTime? date = null,
            NpgsqlConnectionWrapper oc = null)
        {
            var currentDate = date ?? DateTime.Now;

            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var tasks = await GetTasks(instanceId, currentDate, DefaultTaskStates.Preparing.Id, conn);
                var matrix = BuildWeightMatrix(tasks);
                var performers = await GetPerformers(instanceId, conn);
                if (tasks.Length == 0
                    || performers.Length == 0)
                {
                    return ServiceResult.Successful;
                }
                
                var route = RunGA(performers, matrix, tasks);
                if (route is null)
                {
                    return ServiceResult.Successful;
                }
                await DeleteOldRoutes(instanceId, currentDate, conn);
                await InsertRoute(instanceId, currentDate, tasks, performers, route, conn);
            }
            
            return ServiceResult.Successful;
        }

        #endregion

        #region private

        private static List<VRPProblem.Route> RunGA(
            Guid[] performers,
            int[,] matrix,
            TaskGene[] tasks)
        {
            var problem = new VRPProblem(performers, matrix, tasks);
	        
            Algorithm algorithm = new MOEAD(problem);
            algorithm.SetInputParameter("populationSize", 300);
            algorithm.SetInputParameter("maxEvaluations", 100000);
            algorithm.SetInputParameter("T", 20);
            algorithm.SetInputParameter("delta", 0.9);
            algorithm.SetInputParameter("nr", 2);

            // Crossover operator 
            var parameters = new Dictionary<string, object> {{"CR", 1.0}, {"F", 0.5}};
            Operator crossover = CrossoverFactory.GetCrossoverOperator("DifferentialEvolutionCrossover", parameters);

            // Mutation operator
            parameters = new Dictionary<string, object>
            {
                {"probability", 1.0 / problem.NumberOfVariables},
                {"distributionIndex", 20.0}
            };
            Operator mutation = MutationFactory.GetMutationOperator("PolynomialMutation", parameters);

            algorithm.AddOperator("crossover", crossover);
            algorithm.AddOperator("mutation", mutation);

            Solution bestBalancedSolution = null;
            var count = 0;
            while (count < 3)
            {
                var population = algorithm.Execute();

                bestBalancedSolution = population.SolutionsList
                    .OrderBy(p => p.Objective[2])
                    .First();

                if (bestBalancedSolution.Objective.Any(p => p >= int.MaxValue - 1))
                {
                    count++;
                }
                else
                {
                    count = 3;
                }
            }

            if (bestBalancedSolution?.Objective.Any(p => p >= int.MaxValue - 1) == true)
            {
                return null;
            }
            
            // ReSharper disable once PossibleNullReferenceException
            var chromosome = bestBalancedSolution    
                .Variable
                .Select(p => (int)(((Real)p).Value))
                .ToList();
            return problem.DivideIntoRoutes(chromosome);
        }
        
        private static async Task<TaskGene[]> GetTasks(
            Guid instanceId,
            DateTime date,
            Guid stateId,
            NpgsqlConnectionWrapper oc)
        {
            var taskGenes = new List<TaskGene>();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = GetDailyTasks;
                command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                command.Parameters.Add(new NpgsqlParameter("date", date.Date).WithType(NpgsqlDbType.Date));
                command.Parameters.Add(new NpgsqlParameter("state_id", stateId));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        taskGenes.Add(new TaskGene
                        {
                            TaskId = reader.GetGuid(0),
                            PerformerId = reader.GetValueOrDefault<Guid?>(1),
                            TimeWindowStart = Convert.ToInt32(reader.GetDateTime(2).TimeOfDay.TotalSeconds),
                            TimeWindowEnd = Convert.ToInt32(reader.GetDateTime(3).TimeOfDay.TotalSeconds),
                            XPosition = reader.GetDouble(4),
                            YPosition = reader.GetDouble(5),
                        });
                    }
                }
            }

            return taskGenes.ToArray();
        }
        
        private static async Task<Guid[]> GetPerformers(
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

            return performers.ToArray();
        }

        private static int[,] BuildWeightMatrix(
            TaskGene[] tasks)
        {
            var matrix = new int[tasks.Length, tasks.Length];

            for (var i = 0; i < tasks.Length; i++)
            {
                for (var j = 0; j < tasks.Length; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = int.MaxValue;
                        continue;
                    }

                    matrix[i, j] = matrix[j, i] = (int) GetDistance(tasks[i], tasks[j]);
                }
            }

            return matrix;
        }

        private static double GetDistance(TaskGene first, TaskGene second)
        {
            var lat1 = first.XPosition;
            var lat2 = second.XPosition;
            var lon1 = first.YPosition;
            var lon2 = second.YPosition;
            
            var rlat1 = Math.PI*lat1/180;
            var rlat2 = Math.PI*lat2/180;
            var theta = lon1 - lon2;
            var rtheta = Math.PI*theta/180;
            var dist =
                Math.Sin(rlat1)*Math.Sin(rlat2) + Math.Cos(rlat1)*
                Math.Cos(rlat2)*Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist*180/Math.PI;
            dist = dist*60*1.1515;


            return dist*1.609344;
        }

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
            TaskGene[] tasks,
            Guid[] performers,
            List<VRPProblem.Route> routes,
            NpgsqlConnectionWrapper oc)
        {
            var routeNumber = 0;
            foreach (var route in routes)
            {
                foreach (var vertex in route.RouteSequence)
                {
                    using (var comm = oc.CreateCommand())
                    {
                        comm.CommandText = SqlInsertDailyRouteItem;
                        comm.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                        comm.Parameters.Add(new NpgsqlParameter("date", date.Date).WithType(NpgsqlDbType.Date));

                        var taskId = new NpgsqlParameter("task_id", NpgsqlDbType.Uuid);
                        var performerId = new NpgsqlParameter("performer_id", NpgsqlDbType.Uuid);
                        var etaOffset = new NpgsqlParameter("eta_offset", NpgsqlDbType.Integer);
                        comm.Parameters.AddRange(new[] {taskId, performerId, etaOffset});
                        taskId.Value = tasks[vertex].TaskId;
                        performerId.Value = performers[routeNumber];
                        etaOffset.Value = route.ETA[vertex];
                        await comm.ExecuteNonQueryAsync();
                    }

                }
                routeNumber++;
            }
        }
        
        #endregion
    }
}