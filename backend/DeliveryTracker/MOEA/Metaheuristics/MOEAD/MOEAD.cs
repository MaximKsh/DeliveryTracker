using System;
using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Metaheuristics.MOEAD
{
	public class MOEAD : Algorithm
	{
		#region Private attributes

		private int populationSize;

		/// <summary>
		/// Store the population
		/// </summary>
		private SolutionSet population;

		/// <summary>
		/// Z vector (ideal point)
		/// </summary>
		private double[] z;

		/// <summary>
		/// Lambda vectors
		/// </summary>
		private double[][] lambda;

		/// <summary>
		/// neighbour size
		/// </summary>
		private int t;

		/// <summary>
		/// Neighborhood
		/// </summary>
		private int[][] neighborhood;

		/// <summary>
		/// probability that parent solutions are selected from neighbourhood
		/// </summary>
		private double delta;

		/// <summary>
		/// maximal number of solutions replaced by each child solution
		/// </summary>
		private int nr;

		private Solution[] indArray;

		private string functionType;

		private int evaluations;

		private Operator crossover;
		private Operator mutation;

		private string dataDirectory;

		#endregion

		#region Constructors

		public MOEAD(Problem problem)
			: base(problem)
		{
			this.functionType = "_TCHE1";
		}

		#endregion

		#region Override Functions

		public override SolutionSet Execute()
		{
			int maxEvaluations = -1;

			this.evaluations = 0;

			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "maxEvaluations", ref maxEvaluations);
			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "populationSize", ref this.populationSize);
			MOEA.Utils.Utils.GetStringValueFromParameter(this.InputParameters, "dataDirectory", ref this.dataDirectory);

			Logger.Log.Info("POPSIZE: " + this.populationSize);
			Console.WriteLine("POPSIZE: " + this.populationSize);

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
			this.crossover = this.Operators["crossover"];
			this.mutation = this.Operators["mutation"];

			//Step 1. Initialization
			//Step 1.1 Compute euclidean distances between weight vectors and find T
			this.InitUniformWeight();

			this.InitNeighborhood();

			//Step 1.2 Initialize population
			this.InitPoputalion();

			//Step 1.3 Initizlize z
			this.InitIdealPoint();

			//Step 2 Update
			do
			{
				int[] permutation = new int[this.populationSize];
				Utils.RandomPermutation(permutation, this.populationSize);

				for (int i = 0; i < this.populationSize; i++)
				{
					int n = permutation[i]; // or int n = i;

					int type;
					double rnd = JMetalRandom.NextDouble();

					// STEP 2.1. Mating selection based on probability
					if (rnd < this.delta) // if (rnd < realb)    
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
					Solution child;
					Solution[] parents = new Solution[3];

					parents[0] = this.population.Get(p[0]);
					parents[1] = this.population.Get(p[1]);
					parents[2] = this.population.Get(n);

					// Apply DE crossover 
					child = (Solution)this.crossover.Execute(new object[] { this.population.Get(n), parents });

					// Apply mutation
					this.mutation.Execute(child);

					// Evaluation
					this.Problem.Evaluate(child);

					this.evaluations++;

					// STEP 2.3. Repair. Not necessary

					// STEP 2.4. Update z_
					this.UpdateReference(child);

					// STEP 2.5. Update of solutions
					this.UpdateProblem(child, n, type);
				}
			} while (this.evaluations < maxEvaluations);

			this.Result = this.population;

			return this.population;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// InitUniformWeight
		/// </summary>
		private void InitUniformWeight()
		{
			for (int n = 0; n < this.populationSize; n++)
			{
				double a = 1.0 * n / (this.populationSize - 1);
				this.lambda[n][0] = a;
				this.lambda[n][2] = (1 - a)/2;
				this.lambda[n][1] = (1 - a)/2;
			}
			
			/*if ((Problem.NumberOfObjectives == 2) && (populationSize <= 300))
			{
				for (int n = 0; n < populationSize; n++)
				{
					double a = 1.0 * n / (populationSize - 1);
					lambda[n][0] = a;
					lambda[n][1] = 1 - a;
				}
			}
			else
			{
				string dataFileName;
				dataFileName = "W" + Problem.NumberOfObjectives + "D_" +
				  populationSize + ".dat";

				try
				{
					// Open the file
					using (StreamReader reader = new StreamReader(dataDirectory + "/" + dataFileName))
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
								double value = JMetalCSharp.Utils.Utils.ParseDoubleInvariant(s);
								lambda[i][j] = value;
								j++;
							}
							aux = reader.ReadLine();
							i++;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log.Error("InitUniformWeight: failed when reading for file: " + dataDirectory + "/" + dataFileName, ex);
					Console.WriteLine("InitUniformWeight: failed when reading for file: " + dataDirectory + "/" + dataFileName);
				}
			}*/
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
				} // for

				// find 'niche' nearest neighboring subproblems
				Utils.MinFastSort(x, idx, this.populationSize, this.t);

				Array.Copy(idx, 0, this.neighborhood[i], 0, this.t);
			} // for
		}

		private void InitPoputalion()
		{
			for (int i = 0; i < this.populationSize; i++)
			{
				Solution newSolution = new Solution(this.Problem);

				this.Problem.Evaluate(newSolution);
				this.evaluations++;
				this.population.Add(newSolution);
			}
		}

		private void InitIdealPoint()
		{
			for (int i = 0; i < this.Problem.NumberOfObjectives; i++)
			{
				this.z[i] = 1.0e+30;
				this.indArray[i] = new Solution(this.Problem);
				this.Problem.Evaluate(this.indArray[i]);
				this.evaluations++;
			} // for

			for (int i = 0; i < this.populationSize; i++)
			{
				this.UpdateReference(this.population.Get(i));
			}
		}

		private void UpdateReference(Solution individual)
		{
			for (int n = 0; n < this.Problem.NumberOfObjectives; n++)
			{
				if (individual.Objective[n] < this.z[n])
				{
					this.z[n] = individual.Objective[n];

					this.indArray[n] = individual;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="list">the set of the indexes of selected mating parents</param>
		/// <param name="cid">the id of current subproblem</param>
		/// <param name="size">the number of selected mating parents</param>
		/// <param name="type">1 - neighborhood; otherwise - whole population</param>
		private void MatingSelection(List<int> list, int cid, int size, int type)
		{
			int ss;
			int r;
			int p;

			ss = this.neighborhood[cid].Length;
			while (list.Count < size)
			{
				if (type == 1)
				{
					r = JMetalRandom.Next(0, ss - 1);
					p = this.neighborhood[cid][r];
				}
				else
				{
					p = JMetalRandom.Next(0, this.populationSize - 1);
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indiv">child solution</param>
		/// <param name="id">the id of current subproblem</param>
		/// <param name="type">update solutions in - neighborhood (1) or whole population (otherwise)</param>
		private void UpdateProblem(Solution indiv, int id, int type)
		{
			int size;
			int time;

			time = 0;

			if (type == 1)
			{
				size = this.neighborhood[id].Length;
			}
			else
			{
				size = this.population.Size();
			}
			int[] perm = new int[size];

			Utils.RandomPermutation(perm, size);

			for (int i = 0; i < size; i++)
			{
				int k;
				if (type == 1)
				{
					k = this.neighborhood[id][perm[i]];
				}
				else
				{
					k = perm[i];      // calculate the values of objective function regarding the current subproblem
				}
				double f1, f2;

				f1 = this.FitnessFunction(this.population.Get(k), this.lambda[k]);
				f2 = this.FitnessFunction(indiv, this.lambda[k]);

				if (f2 < f1)
				{
					this.population.Replace(k, new Solution(indiv));
					time++;
				}
				// the maximal number of solutions updated is not allowed to exceed 'limit'
				if (time >= this.nr)
				{
					return;
				}
			}
		}

		private double FitnessFunction(Solution individual, double[] lambda)
		{
			double fitness;
			fitness = 0.0;

			if (this.functionType == "_TCHE1")
			{
				double maxFun = -1.0e+30;

				for (int n = 0; n < this.Problem.NumberOfObjectives; n++)
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
