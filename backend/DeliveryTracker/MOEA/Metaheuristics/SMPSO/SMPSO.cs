using System;
using System.Collections.Generic;
using System.IO;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.QualityIndicator;
using DeliveryTracker.MOEA.QualityIndicator.Utils;
using DeliveryTracker.MOEA.Utils;
using DeliveryTracker.MOEA.Utils.Archive;
using DeliveryTracker.MOEA.Utils.Comparators;
using DeliveryTracker.MOEA.Utils.Wrapper;

namespace DeliveryTracker.MOEA.Metaheuristics.SMPSO
{
	/// <summary>
	/// This class implements the SMPSO algorithm described in:
	/// A.J. Nebro, J.J. Durillo, J. Garcia-Nieto, C.A. Coello Coello, F. Luna and E. Alba
	/// "SMPSO: A New PSO-based Metaheuristic for Multi-objective Optimization".
	/// IEEE Symposium on Computational Intelligence in Multicriteria Decision-Making
	/// (MCDM 2009), pp: 66-73. March 2009
	/// </summary>
	public class SMPSO : Algorithm
	{
		#region Private Attributes
		/// <summary>
		/// Stores the number of particles used
		/// </summary>
		private int swarmSize;

		/// <summary>
		/// Stores the maximum size for the archive
		/// </summary>
		private int archiveSize;

		/// <summary>
		/// Stores the maximum number of iteration
		/// </summary>
		private int maxIterations;

		/// <summary>
		/// Stores the current number of iteration
		/// </summary>
		private int iteration;

		/// <summary>
		/// Stores the particles
		/// </summary>
		private SolutionSet particles;

		/// <summary>
		/// Stores the best solutions founds so far for each particles
		/// </summary>
		private Solution[] best;

		/// <summary>
		/// Stores the leaders
		/// </summary>
		public CrowdingArchive Leaders
		{
			get;
			private set;
		}

		/// <summary>
		/// Stores the speed of each particle
		/// </summary>
		private double[][] speed;

		/// <summary>
		/// Stores a comparator for checking dominance
		/// </summary>
		private IComparer<Solution> dominance;

		/// <summary>
		/// Stores a comparator for crowding checking
		/// </summary>
		private IComparer<Solution> crowdingDistanceComparator;

		/// <summary>
		/// Stores a <code>Distance</code> object
		/// </summary>
		private Distance distance;

		/// <summary>
		/// Stores a operator for non uniform mutations
		/// </summary>
		private Operator polynomialMutation;

		private QualityIndicator.QualityIndicator indicators; // QualityIndicator object

		private double r1Max;
		private double r1Min;
		private double r2Max;
		private double r2Min;
		private double C1Max;
		private double C1Min;
		private double C2Max;
		private double C2Min;
		private double WMax;
		private double WMin;
		private double ChVel1;
		private double ChVel2;

		private double trueHypervolume;
		private HyperVolume hy;
		private SolutionSet trueFront;
		private double[] deltaMax;
		private double[] deltaMin;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public SMPSO(Problem problem)
			: base(problem)
		{

			this.r1Max = 1.0;
			this.r1Min = 0.0;
			this.r2Max = 1.0;
			this.r2Min = 0.0;
			this.C1Max = 2.5;
			this.C1Min = 1.5;
			this.C2Max = 2.5;
			this.C2Min = 1.5;
			this.WMax = 0.1;
			this.WMin = 0.1;
			this.ChVel1 = -1;
			this.ChVel2 = -1;
		}

		public SMPSO(Problem problem,
		  List<double> variables,
		  string trueParetoFront)
			: base(problem)
		{

			this.r1Max = variables[0];
			this.r1Min = variables[1];
			this.r2Max = variables[2];
			this.r2Min = variables[3];
			this.C1Max = variables[4];
			this.C1Min = variables[5];
			this.C2Max = variables[6];
			this.C2Min = variables[7];
			this.WMax = variables[8];
			this.WMin = variables[9];
			this.ChVel1 = variables[10];
			this.ChVel2 = variables[11];

			this.hy = new HyperVolume();
			MetricsUtil mu = new MetricsUtil();
			this.trueFront = mu.ReadNonDominatedSolutionSet(trueParetoFront);
			this.trueHypervolume = this.hy.Hypervolume(this.trueFront.WriteObjectivesToMatrix(),
			  this.trueFront.WriteObjectivesToMatrix(),
			  problem.NumberOfObjectives);

		}

		public SMPSO(Problem problem, string trueParetoFront)
			: base(problem)
		{
			this.hy = new HyperVolume();
			MetricsUtil mu = new MetricsUtil();
			this.trueFront = mu.ReadNonDominatedSolutionSet(trueParetoFront);
			this.trueHypervolume = this.hy.Hypervolume(this.trueFront.WriteObjectivesToMatrix(),
			  this.trueFront.WriteObjectivesToMatrix(),
			  problem.NumberOfObjectives);

			// Default configuration
			this.r1Max = 1.0;
			this.r1Min = 0.0;
			this.r2Max = 1.0;
			this.r2Min = 0.0;
			this.C1Max = 2.5;
			this.C1Min = 1.5;
			this.C2Max = 2.5;
			this.C2Min = 1.5;
			this.WMax = 0.1;
			this.WMin = 0.1;
			this.ChVel1 = -1;
			this.ChVel2 = -1;
		}

		#endregion

		#region Private Methods
		/// <summary>
		///  Initialize all parameter of the algorithm
		/// </summary>
		private void InitParams()
		{
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "swarmSize", ref this.swarmSize);
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "archiveSize", ref this.archiveSize);
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "maxIterations", ref this.maxIterations);
			Utils.Utils.GetIndicatorsFromParameters(this.InputParameters, "indicators", ref this.indicators);

			this.polynomialMutation = this.Operators["mutation"];

			this.iteration = 0;

			this.particles = new SolutionSet(this.swarmSize);
			this.best = new Solution[this.swarmSize];
			this.Leaders = new CrowdingArchive(this.archiveSize, this.Problem.NumberOfObjectives);

			// Create comparators for dominance and crowding distance
			this.dominance = new DominanceComparator();
			this.crowdingDistanceComparator = new CrowdingDistanceComparator();
			this.distance = new Distance();

			// Create the speed vector
			this.speed = new double[this.swarmSize][];

			for (int i = 0; i < this.swarmSize; i++)
			{
				this.speed[i] = new double[this.Problem.NumberOfVariables];
			}

			this.deltaMax = new double[this.Problem.NumberOfVariables];
			this.deltaMin = new double[this.Problem.NumberOfVariables];
			for (int i = 0; i < this.Problem.NumberOfVariables; i++)
			{
				this.deltaMax[i] = (this.Problem.UpperLimit[i] - this.Problem.LowerLimit[i]) / 2.0;
				this.deltaMin[i] = -this.deltaMax[i];
			}
		}

		// Adaptive inertia 
		private double InertiaWeight(int iter, int miter, double wma, double wmin)
		{
			return wma;
		}

		// constriction coefficient (M. Clerc)
		private double ConstrictionCoefficient(double c1, double c2)
		{
			double rho = c1 + c2;
			if (rho <= 4)
			{
				return 1.0;
			}
			else
			{
				return 2 / (2 - rho - Math.Sqrt(Math.Pow(rho, 2.0) - 4.0 * rho));
			}
		}

		// velocity bounds
		private double VelocityConstriction(double v, double[] deltaMax,
											double[] deltaMin, int variableIndex,
											int particleIndex)
		{


			double result;

			double dmax = deltaMax[variableIndex];
			double dmin = deltaMin[variableIndex];

			result = v;

			if (v > dmax)
			{
				result = dmax;
			}

			if (v < dmin)
			{
				result = dmin;
			}

			return result;
		}

		/// <summary>
		/// Update the speed of each particle
		/// </summary>
		/// <param name="iter"></param>
		/// <param name="miter"></param>
		private void ComputeSpeed(int iter, int miter)
		{
			double r1, r2, W, C1, C2;
			double wmax, wmin;
			XReal bestGlobal;

			for (int i = 0; i < this.swarmSize; i++)
			{
				XReal particle = new XReal(this.particles.Get(i));
				XReal bestParticle = new XReal(this.best[i]);

				//Select a global best for calculate the speed of particle i, bestGlobal
				Solution one, two;
				int pos1 = JMetalRandom.Next(0, this.Leaders.Size() - 1);
				int pos2 = JMetalRandom.Next(0, this.Leaders.Size() - 1);
				one = this.Leaders.Get(pos1);
				two = this.Leaders.Get(pos2);

				if (this.crowdingDistanceComparator.Compare(one, two) < 1)
				{
					bestGlobal = new XReal(one);
				}
				else
				{
					bestGlobal = new XReal(two);
					//Params for velocity equation
				}
				r1 = JMetalRandom.NextDouble(this.r1Min, this.r1Max);
				r2 = JMetalRandom.NextDouble(this.r2Min, this.r2Max);
				C1 = JMetalRandom.NextDouble(this.C1Min, this.C1Max);
				C2 = JMetalRandom.NextDouble(this.C2Min, this.C2Max);
				W = JMetalRandom.NextDouble(this.WMin, this.WMax);

				wmax = this.WMax;
				wmin = this.WMin;

				for (int var = 0; var < particle.GetNumberOfDecisionVariables(); var++)
				{
					//Computing the velocity of this particle 
					this.speed[i][var] = this.VelocityConstriction(this.ConstrictionCoefficient(C1, C2) * (this.InertiaWeight(iter, miter, wmax, wmin) *
						this.speed[i][var] + C1 * r1 * (bestParticle.GetValue(var) - particle.GetValue(var)) + C2 * r2 * (bestGlobal.GetValue(var) - particle.GetValue(var))),
						this.deltaMax,
						this.deltaMin,
						var,
						i);
				}
			}
		}

		/// <summary>
		///  Update the position of each particle
		/// </summary>
		private void ComputeNewPositions()
		{
			for (int i = 0; i < this.swarmSize; i++)
			{
				XReal particle = new XReal(this.particles.Get(i));
				for (int var = 0; var < particle.GetNumberOfDecisionVariables(); var++)
				{
					particle.SetValue(var, particle.GetValue(var) + this.speed[i][var]);

					if (particle.GetValue(var) < this.Problem.LowerLimit[var])
					{
						particle.SetValue(var, this.Problem.LowerLimit[var]);
						this.speed[i][var] = this.speed[i][var] * this.ChVel1;
					}
					if (particle.GetValue(var) > this.Problem.UpperLimit[var])
					{
						particle.SetValue(var, this.Problem.UpperLimit[var]);
						this.speed[i][var] = this.speed[i][var] * this.ChVel2;
					}
				}
			}
		}

		/// <summary>
		/// Apply a mutation operator to some particles in the swarm
		/// </summary>
		/// <param name="actualIteration"></param>
		/// <param name="totalIterations"></param>
		private void MopsoMutation(int actualIteration, int totalIterations)
		{
			for (int i = 0; i < this.particles.Size(); i++)
			{
				if ((i % 6) == 0)
				{
					this.polynomialMutation.Execute(this.particles.Get(i));
				}
			}
		}

		#endregion

		#region Public Overrides
		/// <summary>
		/// Runs of the SMPSO algorithm.
		/// </summary>
		/// <returns>a <code>SolutionSet</code> that is a set of non dominated solutions
		/// as a result of the algorithm execution</returns>
		public override SolutionSet Execute()
		{
			this.InitParams();

			//->Step 1 (and 3) Create the initial population and evaluate
			for (int i = 0; i < this.swarmSize; i++)
			{
				Solution particle = new Solution(this.Problem);
				this.Problem.Evaluate(particle);
				this.Problem.EvaluateConstraints(particle);
				this.particles.Add(particle);
			}

			//-> Step2. Initialize the speed of each particle to 0
			for (int i = 0; i < this.swarmSize; i++)
			{
				for (int j = 0; j < this.Problem.NumberOfVariables; j++)
				{
					this.speed[i][j] = 0.0;
				}
			}

			// Step4 and 5   
			for (int i = 0; i < this.particles.Size(); i++)
			{
				Solution particle = new Solution(this.particles.Get(i));
				this.Leaders.Add(particle);
			}

			//-> Step 6. Initialize the memory of each particle
			for (int i = 0; i < this.particles.Size(); i++)
			{
				Solution particle = new Solution(this.particles.Get(i));
				this.best[i] = particle;
			}

			//Crowding the leaders
			this.distance.CrowdingDistanceAssignment(this.Leaders, this.Problem.NumberOfObjectives);

			//-> Step 7. Iterations ..        
			while (this.iteration < this.maxIterations)
			{
				try
				{
					//Compute the speed
					this.ComputeSpeed(this.iteration, this.maxIterations);
				}
				catch (IOException ex)
				{
					Logger.Log.Error(this.GetType().FullName + ".Execute()", ex);
				}

				//Compute the new positions for the particles
				this.ComputeNewPositions();

				//Mutate the particles
				this.MopsoMutation(this.iteration, this.maxIterations);

				//Evaluate the new particles in new positions
				for (int i = 0; i < this.particles.Size(); i++)
				{
					Solution particle = this.particles.Get(i);
					this.Problem.Evaluate(particle);
					this.Problem.EvaluateConstraints(particle);
				}

				//Actualize the archive
				for (int i = 0; i < this.particles.Size(); i++)
				{
					Solution particle = new Solution(this.particles.Get(i));
					this.Leaders.Add(particle);
				}

				//Actualize the memory of this particle
				for (int i = 0; i < this.particles.Size(); i++)
				{
					int flag = this.dominance.Compare(this.particles.Get(i), this.best[i]);
					if (flag != 1)
					{ // the new particle is best than the older remeber
						Solution particle = new Solution(this.particles.Get(i));
						this.best[i] = particle;
					}
				}

				//Assign crowding distance to the leaders
				this.distance.CrowdingDistanceAssignment(this.Leaders, this.Problem.NumberOfObjectives);
				this.iteration++;
			}

			this.Result = this.Leaders;
			return this.Leaders;
		}

		#endregion
	}
}
