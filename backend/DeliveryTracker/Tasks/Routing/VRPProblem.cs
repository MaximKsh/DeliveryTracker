using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Encoding.Variable;

namespace DeliveryTracker.Tasks.Routing
{
    public class VRPProblem : Problem
    {
        public class Route
        {
            public List<int> RouteSequence;
            public int Distance;
            public List<int> ETA;

        }
        
        private const int StartWeight = 0;

        private readonly Guid[] performers;
        private readonly int[,] weight;
        private readonly TaskGene[] tasks;

        private readonly int numberOfPerformers;

        
        public VRPProblem(
            Guid[] performers,    
            int[,] weight,
            TaskGene[] tasks)
        {
            this.weight = weight;
            this.tasks = tasks;
            this.performers = performers;
            this.numberOfPerformers = performers.Length;
            this.NumberOfVariables = tasks.Length;
            
            // N(X) = k - приближенность к минимальному
            // D(X) - время в пути
            // B(X) - условие балансировки
            this.NumberOfObjectives = 3;

            // Время прибытия попадает в тайм виндоу
            // Все маршруты покрывают всех клиентов
            // К каждому заехали один раз
            this.NumberOfConstraints = 3;

            this.ProblemName = "VRP";

            this.LowerLimit = new double[this.NumberOfVariables];
            this.UpperLimit = new double[this.NumberOfVariables];
            for (var i = 0; i < tasks.Length; i++)
            {
                this.LowerLimit[i] = 0;
                this.UpperLimit[i] = tasks.Length - 1;
            }
            this.SolutionType = new RealSolutionType(this);

        }
        
        public override void Evaluate(
            Solution solution)
        {
            var chromosome = solution.Variable.Select(p => (int)(((Real)p).Value)).ToList();
            var routes = this.DivideIntoRoutes(chromosome);
            
            if (!this.CheckConstaints(chromosome, routes))
            {
                for (var i = 0; i < this.NumberOfObjectives; i++)
                {
                    solution.Objective[0] = int.MaxValue;
                }

                return;
            }

            var n = routes.Count;
            var d = routes.Sum(p => p.Distance);
            var b = routes.Max(p => p.Distance) - d / routes.Count;

            solution.Objective[0] = n;
            solution.Objective[1] = d;
            solution.Objective[2] = b;

        }

        private bool CheckConstaints(
            List<int> chromosome,
            List<Route> routes)
        {
            if (routes.Count == 0
                || routes.Count > this.numberOfPerformers)
            {
                return false;
            }
            
            // Прошлись по всем и один раз
            var fullSet = new HashSet<int>(chromosome);
            if (fullSet.Count != this.NumberOfVariables)
            {
                return false;
            }

            return true;
        }

        public List<Route> DivideIntoRoutes(List<int> chromosome)
        {
            var routes = new List<Route>();

            foreach (var gene in chromosome)
            {
                if (!(0 <= gene && gene < this.NumberOfVariables))
                {
                    return new List<Route>();
                }
                
                var added = false;
                foreach (var route in routes)
                {
                    var totalDistance = route.Distance + this.weight[route.RouteSequence[route.RouteSequence.Count - 1], gene];
                    if (this.tasks[gene].TimeWindowStart <= totalDistance && totalDistance <= this.tasks[gene].TimeWindowEnd)
                    {
                        route.RouteSequence.Add(gene);
                        route.Distance = totalDistance;
                        route.ETA.Add(totalDistance);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    var newRoute = new List<int> {gene};
                    var eta = new List<int> {this.tasks[gene].TimeWindowStart };
                    routes.Add(new Route
                    {
                        Distance = this.tasks[gene].TimeWindowStart ,
                        ETA = eta,
                        RouteSequence = newRoute,
                    });
                }
            }

            return routes;
        }
    }
}