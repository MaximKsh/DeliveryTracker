using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Metaheuristics.MOEAD
{
	/// <summary>
	/// Class implemeting the pMOEA/D algorithm
	/// </summary>
	public class PMOEAD : Algorithm
	{
		#region Private Attribute

		private readonly object syncLock = new object();
		/// <summary>
		/// Population size
		/// </summary>
		private int populationSize;

		/// <summary>
		/// Stores the population
		/// </summary>
		private SolutionSet population;

		/// <summary>
		/// Number of threads
		/// </summary>
		private int numberOfThreads;

		/// <summary>
		/// Z vector (ideal point)
		/// </summary>
		double[] z;

		/// <summary>
		/// Lambda vectors
		/// </summary>
		double[][] lambda;

		/// <summary>
		/// Neighbour size
		/// </summary>
		int t;

		/// <summary>
		/// Neighborhood
		/// </summary>
		int[][] neighborhood;

		/// <summary>
		/// Probability that parent solutions are selected from neighbourhood
		/// </summary>
		double delta;

		/// <summary>
		/// Maximal number of solutions replaced by each child solution
		/// </summary>
		int nr;

		Solution[] indArray;

		string functionType;

		int evaluations;

		int maxEvaluations;

		Operator crossover;

		Operator mutation;
		int id;

		public Dictionary<string, object> map;

		PMOEAD parentThread;
		Task[] tasks;

		string dataDirectory;

		Barrier barrier;

		long initTime;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public PMOEAD(Problem problem)
			: base(problem)
		{

			this.parentThread = null;

			this.functionType = "_TCHE1";

			this.id = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentThread"></param>
		/// <param name="problem">Problem to solve</param>
		/// <param name="id"></param>
		/// <param name="numberOfThreads"></param>
		public PMOEAD(PMOEAD parentThread, Problem problem, int id, int numberOfThreads)
			: base(problem)
		{

			this.parentThread = parentThread;

			this.numberOfThreads = numberOfThreads;
			this.tasks = new Task[numberOfThreads];

			this.functionType = "_TCHE1";

			this.id = id;
		}


		#endregion

		#region Public Override

		public override SolutionSet Execute()
		{
			this.parentThread = this;

			this.evaluations = 0;
			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "maxEvaluations", ref this.maxEvaluations);
			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "populationSize", ref this.populationSize);
			MOEA.Utils.Utils.GetStringValueFromParameter(this.InputParameters, "dataDirectory", ref this.dataDirectory);
			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "numberOfThreads", ref this.numberOfThreads);

			this.tasks = new Task[this.numberOfThreads];

			this.barrier = new Barrier(this.numberOfThreads);

			this.population = new SolutionSet(this.populationSize);
			this.indArray = new Solution[this.Problem.NumberOfObjectives];

			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "T", ref this.t);
			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "nr", ref this.nr);
			MOEA.Utils.Utils.GetDoubleValueFromParameter(this.InputParameters, "delta", ref this.delta);

			this.neighborhood = new int[this.populationSize][];
			for (int i = 0; i < this.populationSize; i++)
			{
				this.neighborhood[i] = new int[this.t];
			}

			this.z = new double[this.Problem.NumberOfObjectives];

			this.lambda = new double[this.populationSize][];
			for (int i = 0; i < this.populationSize; i++)
			{
				this.lambda[i] = new double[this.Problem.NumberOfObjectives];
			}

			this.crossover = this.Operators["crossover"]; // default: DE crossover
			this.mutation = this.Operators["mutation"];  // default: polynomial mutation

			// STEP 1. Initialization
			// STEP 1.1. Compute euclidean distances between weight vectors and find T
			this.InitUniformWeight();

			this.InitNeighborhood();

			// STEP 1.2. Initialize population
			this.InitPopulation();

			// STEP 1.3. Initialize z
			this.InitIdealPoint();

			this.initTime = Environment.TickCount;

			for (int j = 0; j < this.numberOfThreads; j++)
			{
				this.CreateTask(this.tasks, this, this.Problem, j, this.numberOfThreads);
				this.tasks[j].Start();
			}

			for (int i = 0; i < this.numberOfThreads; i++)
			{
				try
				{
					this.tasks[i].Wait();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error in " + this.GetType().FullName + ".Execute()");
					Logger.Log.Error(this.GetType().FullName + ".Execute()", ex);
				}
			}

			this.Result = this.population;

			return this.population;
		}

		private void CreateTask(Task[] tasks, PMOEAD parent, Core.Problem Problem, int j, int numberOfThreads)
		{
			tasks[j] = new Task(() => new PMOEAD(parent, Problem, j, numberOfThreads).Run());
		}

		#endregion

		#region Private Methods

		private void Run()
		{
			this.neighborhood = this.parentThread.neighborhood;
			this.Problem = this.parentThread.Problem;
			this.lambda = this.parentThread.lambda;
			this.population = this.parentThread.population;
			this.z = this.parentThread.z;
			this.indArray = this.parentThread.indArray;
			this.barrier = this.parentThread.barrier;


			int partitions = this.parentThread.populationSize / this.parentThread.numberOfThreads;

			this.evaluations = 0;
			this.maxEvaluations = this.parentThread.maxEvaluations / this.parentThread.numberOfThreads;

			this.barrier.SignalAndWait();

			int first;
			int last;

			first = partitions * this.id;
			if (this.id == (this.parentThread.numberOfThreads - 1))
			{
				last = this.parentThread.populationSize - 1;
			}
			else
			{
				last = first + partitions - 1;
			}

			Logger.Log.Info("Id: " + this.id + "  Partitions: " + partitions + " First: " + first + " Last: " + last);
			Console.WriteLine("Id: " + this.id + "  Partitions: " + partitions + " First: " + first + " Last: " + last);
			do
			{
				for (int i = first; i <= last; i++)
				{
					int n = i;
					int type;
					double rnd = JMetalRandom.NextDouble();

					// STEP 2.1. Mating selection based on probability
					if (rnd < this.parentThread.delta)
					{
						type = 1;   // neighborhood
					}
					else
					{
						type = 2;   // whole population
					}

					List<int> p = new List<int>();
					this.MatingSelection(p, n, 2, type);

					// STEP 2.2. Reproduction
					Solution child = null;
					Solution[] parents = new Solution[3];

					try
					{
						lock (this.parentThread)
						{
							parents[0] = this.parentThread.population.Get(p[0]);
							parents[1] = this.parentThread.population.Get(p[1]);
							parents[2] = this.parentThread.population.Get(n);
							// Apply DE crossover
							child = (Solution)this.parentThread.crossover.Execute(new object[] { this.parentThread.population.Get(n), parents });
						}
						// Apply mutation
						this.parentThread.mutation.Execute(child);

						// Evaluation
						this.parentThread.Problem.Evaluate(child);

					}
					catch (Exception ex)
					{
						Logger.Log.Error(this.GetType().FullName + ".Run()", ex);
						Console.WriteLine("Error in " + this.GetType().FullName + ".Run()");
					}

					this.evaluations++;

					// STEP 2.3. Repair. Not necessary

					// STEP 2.4. Update z
					this.UpdateReference(child);

					// STEP 2.5. Update of solutions
					this.UpdateOfSolutions(child, n, type);
				}
			} while (this.evaluations < this.maxEvaluations);

			long estimatedTime = Environment.TickCount - this.parentThread.initTime;
			Logger.Log.Info("Time thread " + this.id + ": " + estimatedTime);
			Console.WriteLine("Time thread " + this.id + ": " + estimatedTime);
		}

		private void InitUniformWeight()
		{
			if ((this.Problem.NumberOfObjectives == 2) && (this.populationSize < 300))
			{
				for (int n = 0; n < this.populationSize; n++)
				{
					double a = 1.0 * n / (this.populationSize - 1);
					this.lambda[n][0] = a;
					this.lambda[n][1] = 1 - a;
				}
			}
			else
			{
				string dataFileName;
				dataFileName = "W" + this.Problem.NumberOfObjectives + "D_" + this.populationSize + ".dat";

				try
				{
					// Open the file
					using (StreamReader reader = new StreamReader(this.dataDirectory + "/" + dataFileName))
					{

						int numberOfObjectives = 0;
						int i = 0;
						int j = 0;
						string aux = reader.ReadLine();
						while (aux != null)
						{
							string[] st = aux.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
							j = 0;
							numberOfObjectives = st.Length;

							foreach (string s in st)
							{
								double value = MOEA.Utils.Utils.ParseDoubleInvariant(s);
								this.lambda[i][j] = value;
								j++;
							}
							aux = reader.ReadLine();
							i++;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log.Error("InitUniformWeight: failed when reading for file: " + this.dataDirectory + "/" + dataFileName, ex);
					Console.WriteLine("InitUniformWeight: failed when reading for file: " + this.dataDirectory + "/" + dataFileName);
				}
			}
		}

		private void InitNeighborhood()
		{
			double[] x = new double[this.populationSize];
			int[] idx = new int[this.populationSize];

			for (int i = 0; i < this.populationSize; i++)
			{
				// calculate the distances based on weight vectors
				for (int j = 0; j < this.populationSize; j++)
				{
					x[j] = Utils.DistVector(this.lambda[i], this.lambda[j]);
					idx[j] = j;
				}

				// find 'niche' nearest neighboring subproblems
				Utils.MinFastSort(x, idx, this.populationSize, this.t);

				for (int k = 0; k < this.t; k++)
				{
					this.neighborhood[i][k] = idx[k];
				}
			}
		}

		private void InitPopulation()
		{
			for (int i = 0; i < this.populationSize; i++)
			{
				Solution newSolution = new Solution(this.Problem);

				this.Problem.Evaluate(newSolution);
				this.evaluations++;
				this.population.Add(newSolution);
			}
		}

		void InitIdealPoint()
		{
			for (int i = 0; i < this.Problem.NumberOfObjectives; i++)
			{
				this.z[i] = 1.0e+30;
				this.indArray[i] = new Solution(this.Problem);
				this.Problem.Evaluate(this.indArray[i]);
				this.evaluations++;
			}

			for (int i = 0; i < this.populationSize; i++)
			{
				this.UpdateReference(this.population.Get(i));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="list">The set of the indexes of selected mating parents</param>
		/// <param name="cid">The id of current subproblem</param>
		/// <param name="size">The number of selected mating parents</param>
		/// <param name="type">1 - neighborhood; otherwise - whole population</param>
		private void MatingSelection(List<int> list, int cid, int size, int type)
		{
			int ss;
			int r;
			int p;

			ss = this.parentThread.neighborhood[cid].Length;
			while (list.Count < size)
			{
				if (type == 1)
				{
					r = JMetalRandom.Next(0, ss - 1);
					p = this.parentThread.neighborhood[cid][r];
				}
				else
				{
					p = JMetalRandom.Next(0, this.parentThread.populationSize - 1);
				}
				bool flag = true;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == p) // p is in the list
					{
						flag = false;
						break;
					}
				}

				if (flag)
				{
					list.Add(p);
				}
			}
		}

		private void UpdateReference(Solution individual)
		{
			lock (this.syncLock)
			{
				for (int n = 0; n < this.parentThread.Problem.NumberOfObjectives; n++)
				{
					if (individual.Objective[n] < this.z[n])
					{
						this.parentThread.z[n] = individual.Objective[n];

						this.parentThread.indArray[n] = individual;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indiv">Child solution</param>
		/// <param name="id">The id of current subproblem</param>
		/// <param name="type">Update solutions in - neighborhood (1) or whole population (otherwise)</param>
		private void UpdateOfSolutions(Solution indiv, int id, int type)
		{
			int size;
			int time;

			time = 0;

			if (type == 1)
			{
				size = this.parentThread.neighborhood[id].Length;
			}
			else
			{
				size = this.parentThread.population.Size();
			}
			int[] perm = new int[size];

			Utils.RandomPermutation(perm, size);

			for (int i = 0; i < size; i++)
			{
				int k;
				if (type == 1)
				{
					k = this.parentThread.neighborhood[id][perm[i]];
				}
				else
				{
					k = perm[i];      // calculate the values of objective function regarding the current subproblem
				}
				double f1, f2;

				f2 = this.FitnessFunction(indiv, this.parentThread.lambda[k]);
				lock (this.parentThread)
				{
					f1 = this.FitnessFunction(this.parentThread.population.Get(k), this.parentThread.lambda[k]);

					if (f2 < f1)
					{
						this.parentThread.population.Replace(k, new Solution(indiv));
						time++;
					}
				}
				// the maximal number of solutions updated is not allowed to exceed 'limit'
				if (time >= this.parentThread.nr)
				{
					return;
				}
			}
		}

		double FitnessFunction(Solution individual, double[] lambda)
		{
			double fitness;
			fitness = 0.0;

			if (this.parentThread.functionType == "_TCHE1")
			{
				double maxFun = -1.0e+30;

				for (int n = 0; n < this.parentThread.Problem.NumberOfObjectives; n++)
				{
					double diff = Math.Abs(individual.Objective[n] - this.z[n]);

					double feval;
					if (lambda[n] == 0)
					{
						feval = 0.0001 * diff;
					}
					else
					{
						feval = diff * lambda[n];
					}
					if (feval > maxFun)
					{
						maxFun = feval;
					}
				}

				fitness = maxFun;
			}
			else
			{
				Logger.Log.Error("MOEAD.FitnessFunction: unknown type " + this.functionType);
				Console.WriteLine("MOEAD.FitnessFunction: unknown type " + this.functionType);
				Environment.Exit(-1);
				throw new Exception("MOEAD.FitnessFunction: unknown type " + this.functionType);
			}
			return fitness;
		}
		#endregion
	}
}
