using System;
using System.Collections.Generic;
using System.IO;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Metaheuristics.MOEAD
{
	/// <summary>
	/// Reference: Q. Zhang, W. Liu, and H Li, The Performance of a New Version of
	/// MOEA/D on CEC09 Unconstrained MOP Test Instances, Working Report CES-491,
	/// School of CS & EE, University of Essex, 02/2009
	/// </summary>
	public class MOEAD_DRA : Algorithm
	{
		#region Private Attributes

		private int populationSize;

		/// <summary>
		/// Stores the population
		/// </summary>
		private SolutionSet population;

		/// <summary>
		/// Stores the values of the individuals
		/// </summary>
		private Solution[] savedValues;

		private double[] utility;
		private int[] frequency;

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

		/// <summary>
		/// Operators
		/// </summary>
		private Operator crossover;
		private Operator mutation;

		private string dataDirectory;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public MOEAD_DRA(Problem problem)
			: base(problem)
		{

			this.functionType = "_TCHE1";

		}

		#endregion

		#region Public Overrides
		public override SolutionSet Execute()
		{
			int maxEvaluations = -1;

			this.evaluations = 0;
			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "maxEvaluations", ref maxEvaluations);
			MOEA.Utils.Utils.GetIntValueFromParameter(this.InputParameters, "populationSize", ref this.populationSize);
			MOEA.Utils.Utils.GetStringValueFromParameter(this.InputParameters, "dataDirectory", ref this.dataDirectory);

			this.population = new SolutionSet(this.populationSize);
			this.savedValues = new Solution[this.populationSize];
			this.utility = new double[this.populationSize];
			this.frequency = new int[this.populationSize];
			for (int i = 0; i < this.utility.Length; i++)
			{
				this.utility[i] = 1.0;
				this.frequency[i] = 0;
			}
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

			int gen = 0;
			// STEP 2. Update
			do
			{
				int[] permutation = new int[this.populationSize];
				Utils.RandomPermutation(permutation, this.populationSize);
				List<int> order = this.TourSelection(10);

				for (int i = 0; i < order.Count; i++)
				{
					int n = order[i];
					this.frequency[n]++;

					int type;
					double rnd = JMetalRandom.NextDouble();

					// STEP 2.1. Mating selection based on probability
					if (rnd < this.delta)
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
					// STEP 2.4. Update z
					this.UpdateReference(child);

					// STEP 2.5. Update of solutions
					this.UpdateProblem(child, n, type);
				}

				gen++;
				if (gen % 30 == 0)
				{
					this.CompUtility();
				}

			} while (this.evaluations < maxEvaluations);

			int finalSize = this.populationSize;
			try
			{
				finalSize = (int)this.GetInputParameter("finalSize");

				Logger.Log.Info("FINAL SIZE: " + finalSize);
				Console.WriteLine("FINAL SIZE: " + finalSize);
			}
			catch (Exception ex)
			{ // if there is an exception indicate it!
				Logger.Log.Warn("The final size paramater has been ignored", ex);
				Logger.Log.Warn("The number of solutions is " + this.population.Size());
				Console.WriteLine("The final size paramater has been ignored");
				Console.WriteLine("The number of solutions is " + this.population.Size());
				return this.population;

			}

			this.Result = this.FinalSelection(finalSize);
			return this.Result;
		}

		#endregion

		#region Private Methods

		private void InitUniformWeight()
		{
			if ((this.Problem.NumberOfObjectives == 2) && (this.populationSize <= 100))
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
				dataFileName = "W" + this.Problem.NumberOfObjectives + "D_"
						+ this.populationSize + ".dat";

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

				Array.Copy(idx, 0, this.neighborhood[i], 0, this.t);
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
				this.savedValues[i] = new Solution(newSolution);
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
			}

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

		private List<int> TourSelection(int depth)
		{
			// selection based on utility
			List<int> selected = new List<int>();
			List<int> candidate = new List<int>();

			for (int k = 0; k < this.Problem.NumberOfObjectives; k++)
			{
				selected.Add(k);   // WARNING! HERE YOU HAVE TO USE THE WEIGHT PROVIDED BY QINGFU (NOT SORTED!!!!)
			}

			for (int n = this.Problem.NumberOfObjectives; n < this.populationSize; n++)
			{
				candidate.Add(n);  // set of unselected weights
			}
			while (selected.Count < (int)(this.populationSize / 5.0))
			{
				int bestIdd = (int)(JMetalRandom.NextDouble() * candidate.Count);

				int i2;
				int bestSub = candidate[bestIdd];
				int s2;
				for (int i = 1; i < depth; i++)
				{
					i2 = (int)(JMetalRandom.NextDouble() * candidate.Count);
					s2 = candidate[i2];

					if (this.utility[s2] > this.utility[bestSub])
					{
						bestIdd = i2;
						bestSub = s2;
					}
				}
				selected.Add(bestSub);
				candidate.Remove(bestIdd);
			}
			return selected;
		}

		private void CompUtility()
		{
			double f1, f2, uti, delta;
			for (int n = 0; n < this.populationSize; n++)
			{
				f1 = this.FitnessFunction(this.population.Get(n), this.lambda[n]);
				f2 = this.FitnessFunction(this.savedValues[n], this.lambda[n]);
				delta = f2 - f1;
				if (delta > 0.001)
				{
					this.utility[n] = 1.0;
				}
				else
				{
					uti = (0.95 + (0.05 * delta / 0.001)) * this.utility[n];
					this.utility[n] = uti < 1.0 ? uti : 1.0;
				}
				this.savedValues[n] = new Solution(this.population.Get(n));
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
					k = perm[i]; // calculate the values of objective function regarding the current subproblem
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

		/// <summary>
		/// @author Juanjo This method selects N solutions from a set M, where N <= M
		/// using the same method proposed by Qingfu Zhang, W. Liu, and Hui Li in the
		/// paper describing MOEA/D-DRA (CEC 09 COMPTETITION) An example is giving in
		/// that paper for two objectives. If N = 100, then the best solutions
		/// attenting to the weights (0,1), (1/99,98/99), ...,(98/99,1/99), (1,0) are
		/// selected.
		///
		/// Using this method result in 101 solutions instead of 100. We will just
		/// compute 100 even distributed weights and used them. The result is the
		/// same
		///
		/// In case of more than two objectives the procedure is: 1- Select a
		/// solution at random 2- Select the solution from the population which have
		/// maximum distance to it (whithout considering the already included)
		/// </summary>
		/// <param name="n">The number of solutions to return</param>
		/// <returns>A solution set containing those elements</returns>
		private SolutionSet FinalSelection(int n)
		{
			SolutionSet res = new SolutionSet(n);
			if (this.Problem.NumberOfObjectives == 2)
			{ // subcase 1                     
				double[][] internLambda = new double[n][];
				for (int i = 0; i < n; i++)
				{
					internLambda[i] = new double[2];
				}

				for (int i = 0; i < n; i++)
				{
					double a = 1.0 * i / (n - 1);
					internLambda[i][0] = a;
					internLambda[i][1] = 1 - a;
				}

				// we have now the weights, now select the best solution for each of them
				for (int i = 0; i < n; i++)
				{
					Solution currentBest = this.population.Get(0);
					double value = this.FitnessFunction(currentBest, internLambda[i]);

					for (int j = 1; j < n; j++)
					{
						double aux = this.FitnessFunction(this.population.Get(j), internLambda[i]); // we are looking the best for the weight i
						if (aux < value)
						{ // solution in position j is better!               
							value = aux;
							currentBest = this.population.Get(j);
						}
					}
					res.Add(new Solution(currentBest));
				}
			}
			else
			{ // general case (more than two objectives)
				Distance distanceUtility = new Distance();
				int randomIndex = JMetalRandom.Next(0, this.population.Size() - 1);

				// create a list containing all the solutions but the selected one (only references to them)
				List<Solution> candidate = new List<Solution>();
				candidate.Add(this.population.Get(randomIndex));

				for (int i = 0; i < this.population.Size(); i++)
				{
					if (i != randomIndex)
					{
						candidate.Add(this.population.Get(i));
					}
				}

				while (res.Size() < n)
				{
					int index = 0;
					Solution selected = candidate[0]; // it should be a next! (n <= population size!)
					double distanceValue = distanceUtility.DistanceToSolutionSetInObjectiveSpace(selected, res);
					int i = 1;
					while (i < candidate.Count)
					{
						Solution nextCandidate = candidate[i];
						double aux = distanceValue = distanceUtility.DistanceToSolutionSetInObjectiveSpace(nextCandidate, res);
						if (aux > distanceValue)
						{
							distanceValue = aux;
							index = i;
						}
						i++;
					}

					// add the selected to res and remove from candidate list
					res.Add(new Solution(candidate[index]));
					candidate.RemoveAt(index);
				}
			}
			return res;
		}

		#endregion
	}
}
