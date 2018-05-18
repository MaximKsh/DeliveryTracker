using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Operators.LocalSearch;
using DeliveryTracker.MOEA.Utils;
using DeliveryTracker.MOEA.Utils.Archive;
using DeliveryTracker.MOEA.Utils.Comparators;
using DeliveryTracker.MOEA.Utils.Wrapper;

namespace DeliveryTracker.MOEA.Metaheuristics.AbYSS
{
	/// <summary>
	/// This class implements the AbYSS algorithm. This algorithm is an adaptation
	/// of the single-objective scatter search template defined by F. Glover in:
	/// F. Glover. "A template for scatter search and path relinking", Lecture Notes 
	/// in Computer Science, Springer Verlag, 1997. AbYSS is described in: 
	/// A.J. Nebro, F. Luna, E. Alba, B. Dorronsoro, J.J. Durillo, A. Beham 
	/// "AbYSS: Adapting Scatter Search to Multiobjective Optimization." 
	/// IEEE Transactions on Evolutionary Computation. Vol. 12, 
	/// No. 4 (August 2008), pp. 439-457
	/// </summary>
	public class AbYSS : Algorithm
	{
		#region Private Attributes

		/// <summary>
		/// Stores the number of subranges in which each encodings.variable is divided. Used in
		/// the diversification method. By default it takes the value 4 (see the method
		/// <code>initParams</code>).
		/// </summary>
		private int numberOfSubranges;

		/// <summary>
		/// These variables are used in the diversification method.
		/// </summary>
		private int[] sumOfFrequencyValues;
		private int[] sumOfReverseFrequencyValues;
		private int[][] frequency;
		private int[][] reverseFrequency;

		/// <summary>
		/// Stores the initial solution set
		/// </summary>
		private SolutionSet solutionSet;

		/// <summary>
		/// Stores the external solution archive
		/// </summary>
		private CrowdingArchive archive;

		/// <summary>
		/// Stores the reference set one
		/// </summary>
		private SolutionSet refSet1;

		/// <summary>
		/// Stores the reference set two
		/// </summary>
		private SolutionSet refSet2;

		/// <summary>
		/// Stores the solutions provided by the subset generation method of the
		/// scatter search template
		/// </summary>
		private SolutionSet subSet;

		/// <summary>
		/// Maximum number of solution allowed for the initial solution set
		/// </summary>
		private int solutionSetSize;

		/// <summary>
		/// Maximum size of the external archive
		/// </summary>
		private int archiveSize;

		/// <summary>
		/// Maximum size of the reference set one
		/// </summary>
		private int refSet1Size;

		/// <summary>
		/// Maximum size of the reference set two
		/// </summary>
		private int refSet2Size;

		/// <summary>
		/// Maximum number of getEvaluations to carry out
		/// </summary>
		private int maxEvaluations;

		/// <summary>
		/// Stores the current number of performed getEvaluations
		/// </summary>
		private int evaluations;

		/// <summary>
		/// Stores the comparators for dominance and equality, respectively
		/// </summary>
		private IComparer<Solution> dominance;
		private IComparer<Solution> equal;
		private IComparer<Solution> fitness;
		private IComparer<Solution> crowdingDistance;

		/// <summary>
		/// Stores the crossover operator
		/// </summary>
		private Operator crossoverOperator;

		/// <summary>
		/// Stores the improvement operator
		/// </summary>
		private LocalSearch improvementOperator;

		/// <summary>
		/// Stores a <code>Distance</code> object
		/// </summary>
		private Distance distance;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public AbYSS(Problem problem)
			: base(problem)
		{
			//Initialize the fields 

			this.solutionSet = null;
			this.archive = null;
			this.refSet1 = null;
			this.refSet2 = null;
			this.subSet = null;
		}

		#endregion

		/// <summary>
		/// Reads the parameter from the parameter list using the
		/// <code>getInputParameter</code> method.
		/// </summary>
		public void InitParam()
		{
			//Read the parameters
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "populationSize", ref this.solutionSetSize);
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "refSet1Size", ref this.refSet1Size);
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "refSet2Size", ref this.refSet2Size);
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "archiveSize", ref this.archiveSize);
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "maxEvaluations", ref this.maxEvaluations);

			//Initialize the variables
			this.solutionSet = new SolutionSet(this.solutionSetSize);
			this.archive = new CrowdingArchive(this.archiveSize, this.Problem.NumberOfObjectives);
			this.refSet1 = new SolutionSet(this.refSet1Size);
			this.refSet2 = new SolutionSet(this.refSet2Size);
			this.subSet = new SolutionSet(this.solutionSetSize * 1000);
			this.evaluations = 0;

			this.numberOfSubranges = 4;

			this.dominance = new DominanceComparator();
			this.equal = new EqualSolutions();
			this.fitness = new FitnessComparator();
			this.crowdingDistance = new CrowdingDistanceComparator();
			this.distance = new Distance();
			this.sumOfFrequencyValues = new int[this.Problem.NumberOfVariables];
			this.sumOfReverseFrequencyValues = new int[this.Problem.NumberOfVariables];
			this.frequency = new int[this.numberOfSubranges][];
			this.reverseFrequency = new int[this.numberOfSubranges][];

			for (int i = 0; i < this.numberOfSubranges; i++)
			{
				this.frequency[i] = new int[this.Problem.NumberOfVariables];
				this.reverseFrequency[i] = new int[this.Problem.NumberOfVariables];
			}

			// Read the operators of crossover and improvement
			this.crossoverOperator = this.Operators["crossover"];
			this.improvementOperator = (LocalSearch)this.Operators["improvement"];
			this.improvementOperator.SetParameter("archive", this.archive);
		}

		/// <summary>
		/// Returns a <code>Solution</code> using the diversification generation method 
		/// described in the scatter search template.
		/// </summary>
		/// <returns></returns>
		public Solution DiversificationGeneration()
		{
			Solution solution;
			solution = new Solution(this.Problem);
			XReal wrapperSolution = new XReal(solution);

			double value;
			int range;

			for (int i = 0; i < this.Problem.NumberOfVariables; i++)
			{
				this.sumOfReverseFrequencyValues[i] = 0;
				for (int j = 0; j < this.numberOfSubranges; j++)
				{
					this.reverseFrequency[j][i] = this.sumOfFrequencyValues[i] - this.frequency[j][i];
					this.sumOfReverseFrequencyValues[i] += this.reverseFrequency[j][i];
				}

				if (this.sumOfReverseFrequencyValues[i] == 0)
				{
					range = JMetalRandom.Next(0, this.numberOfSubranges - 1);
				}
				else
				{
					value = JMetalRandom.Next(0, this.sumOfReverseFrequencyValues[i] - 1);
					range = 0;
					while (value > this.reverseFrequency[range][i])
					{
						value -= this.reverseFrequency[range][i];
						range++;
					}
				}

				this.frequency[range][i]++;
				this.sumOfFrequencyValues[i]++;

				double low = this.Problem.LowerLimit[i] + range * (this.Problem.UpperLimit[i] -
							 this.Problem.LowerLimit[i]) / this.numberOfSubranges;
				double high = low + (this.Problem.UpperLimit[i] -
							 this.Problem.LowerLimit[i]) / this.numberOfSubranges;
				value = JMetalRandom.NextDouble(low, high);

				wrapperSolution.SetValue(i, value);
			}
			return solution;
		}


		/// <summary>
		/// Implements the referenceSetUpdate method.
		/// </summary>
		/// <param name="build">build if true, indicates that the reference has to be build for the
		/// first time; if false, indicates that the reference set has to be
		/// updated with new solutions</param>
		public void ReferenceSetUpdate(bool build)
		{
			if (build)
			{ // Build a new reference set
				// STEP 1. Select the p best individuals of P, where p is refSet1Size. 
				//         Selection Criterium: Spea2Fitness
				Solution individual;
				(new Spea2Fitness(this.solutionSet)).FitnessAssign();
				this.solutionSet.Sort(this.fitness);

				// STEP 2. Build the RefSet1 with these p individuals            
				for (int i = 0; i < this.refSet1Size; i++)
				{
					individual = this.solutionSet.Get(0);
					this.solutionSet.Remove(0);
					individual.UnMarked();
					this.refSet1.Add(individual);
				}

				// STEP 3. Compute Euclidean distances in SolutionSet to obtain q 
				//         individuals, where q is refSet2Size_
				for (int i = 0; i < this.solutionSet.Size(); i++)
				{
					individual = this.solutionSet.Get(i);
					individual.DistanceToSolutionSet = this.distance.DistanceToSolutionSetInSolutionSpace(individual, this.refSet1);
				}

				int size = this.refSet2Size;
				if (this.solutionSet.Size() < this.refSet2Size)
				{
					size = this.solutionSet.Size();
				}

				// STEP 4. Build the RefSet2 with these q individuals
				for (int i = 0; i < size; i++)
				{
					// Find the maximumMinimunDistanceToPopulation
					double maxMinimum = 0.0;
					int index = 0;
					for (int j = 0; j < this.solutionSet.Size(); j++)
					{
						if (this.solutionSet.Get(j).DistanceToSolutionSet > maxMinimum)
						{
							maxMinimum = this.solutionSet.Get(j).DistanceToSolutionSet;
							index = j;
						}
					}
					individual = this.solutionSet.Get(index);
					this.solutionSet.Remove(index);

					// Update distances to REFSET in population
					for (int j = 0; j < this.solutionSet.Size(); j++)
					{
						double aux = this.distance.DistanceBetweenSolutions(this.solutionSet.Get(j), individual);
						if (aux < individual.DistanceToSolutionSet)
						{
							this.solutionSet.Get(j).DistanceToSolutionSet = aux;
						}
					}

					// Insert the individual into REFSET2
					this.refSet2.Add(individual);

					// Update distances in REFSET2
					for (int j = 0; j < this.refSet2.Size(); j++)
					{
						for (int k = 0; k < this.refSet2.Size(); k++)
						{
							if (i != j)
							{
								double aux = this.distance.DistanceBetweenSolutions(this.refSet2.Get(j), this.refSet2.Get(k));
								if (aux < this.refSet2.Get(j).DistanceToSolutionSet)
								{
									this.refSet2.Get(j).DistanceToSolutionSet = aux;
								}
							}
						}
					}
				}

			}
			else
			{ // Update the reference set from the subset generation result
				Solution individual;
				for (int i = 0; i < this.subSet.Size(); i++)
				{
					individual = (Solution)this.improvementOperator.Execute(this.subSet.Get(i));
					this.evaluations += this.improvementOperator.GetEvaluations();

					if (this.RefSet1Test(individual))
					{ //Update distance of RefSet2
						for (int indSet2 = 0; indSet2 < this.refSet2.Size(); indSet2++)
						{
							double aux = this.distance.DistanceBetweenSolutions(individual, this.refSet2.Get(indSet2));
							if (aux < this.refSet2.Get(indSet2).DistanceToSolutionSet)
							{
								this.refSet2.Get(indSet2).DistanceToSolutionSet = aux;
							}
						}
					}
					else
					{
						this.RefSet2Test(individual);
					}
				}
				this.subSet.Clear();
			}
		}

		/// <summary>
		/// Tries to update the reference set 2 with a <code>Solution</code>
		/// </summary>
		/// <param name="solution">The <code>Solution</code></param>
		/// <returns>true if the <code>Solution</code> has been inserted, false 
		/// otherwise.</returns>
		public bool RefSet2Test(Solution solution)
		{
			double aux;
			if (this.refSet2.Size() < this.refSet2Size)
			{
				solution.DistanceToSolutionSet = this.distance.DistanceToSolutionSetInSolutionSpace(solution, this.refSet1);
				aux = this.distance.DistanceToSolutionSetInSolutionSpace(solution, this.refSet2);
				if (aux < solution.DistanceToSolutionSet)
				{
					solution.DistanceToSolutionSet = aux;
				}
				this.refSet2.Add(solution);
				return true;
			}

			solution.DistanceToSolutionSet = this.distance.DistanceToSolutionSetInSolutionSpace(solution, this.refSet1);
			aux = this.distance.DistanceToSolutionSetInSolutionSpace(solution, this.refSet2);
			if (aux < solution.DistanceToSolutionSet)
			{
				solution.DistanceToSolutionSet = aux;
			}

			double peor = 0.0;
			int index = 0;
			for (int i = 0; i < this.refSet2.Size(); i++)
			{
				aux = this.refSet2.Get(i).DistanceToSolutionSet;
				if (aux > peor)
				{
					peor = aux;
					index = i;
				}
			}

			if (solution.DistanceToSolutionSet < peor)
			{
				this.refSet2.Remove(index);
				//Update distances in REFSET2
				for (int j = 0; j < this.refSet2.Size(); j++)
				{
					aux = this.distance.DistanceBetweenSolutions(this.refSet2.Get(j), solution);
					if (aux < this.refSet2.Get(j).DistanceToSolutionSet)
					{
						this.refSet2.Get(j).DistanceToSolutionSet = aux;
					}
				}
				solution.UnMarked();
				this.refSet2.Add(solution);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Tries to update the reference set one with a <code>Solution</code>.
		/// </summary>
		/// <param name="solution">The <code>Solution</code></param>
		/// <returns>true if the <code>Solution</code> has been inserted, false
		/// otherwise.</returns>
		public bool RefSet1Test(Solution solution)
		{
			bool dominated = false;
			int flag;
			int i = 0;
			while (i < this.refSet1.Size())
			{
				flag = this.dominance.Compare(solution, this.refSet1.Get(i));
				if (flag == -1)
				{ //This is: solution dominates 
					this.refSet1.Remove(i);
				}
				else if (flag == 1)
				{
					dominated = true;
					i++;
				}
				else
				{
					flag = this.equal.Compare(solution, this.refSet1.Get(i));
					if (flag == 0)
					{
						return true;
					}
					i++;
				}
			}

			if (!dominated)
			{
				solution.UnMarked();
				if (this.refSet1.Size() < this.refSet1Size)
				{ //refSet1 isn't full
					this.refSet1.Add(solution);
				}
				else
				{
					this.archive.Add(solution);
				}
			}
			else
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Implements the subset generation method described in the scatter search
		/// template
		/// </summary>
		/// <returns>Number of solutions created by the method</returns>
		public int SubSetGeneration()
		{
			Solution[] parents = new Solution[2];
			Solution[] offSpring;

			this.subSet.Clear();

			//All pairs from refSet1
			for (int i = 0; i < this.refSet1.Size(); i++)
			{
				parents[0] = this.refSet1.Get(i);
				for (int j = i + 1; j < this.refSet1.Size(); j++)
				{
					parents[1] = this.refSet1.Get(j);
					if (!parents[0].IsMarked() || !parents[1].IsMarked())
					{
						offSpring = (Solution[])this.crossoverOperator.Execute(parents);
						this.Problem.Evaluate(offSpring[0]);
						this.Problem.Evaluate(offSpring[1]);
						this.Problem.EvaluateConstraints(offSpring[0]);
						this.Problem.EvaluateConstraints(offSpring[1]);
						this.evaluations += 2;
						if (this.evaluations < this.maxEvaluations)
						{
							this.subSet.Add(offSpring[0]);
							this.subSet.Add(offSpring[1]);
						}
						parents[0].Marked();
						parents[1].Marked();
					}
				}
			}

			// All pairs from refSet2
			for (int i = 0; i < this.refSet2.Size(); i++)
			{
				parents[0] = this.refSet2.Get(i);
				for (int j = i + 1; j < this.refSet2.Size(); j++)
				{
					parents[1] = this.refSet2.Get(j);
					if (!parents[0].IsMarked() || !parents[1].IsMarked())
					{
						offSpring = (Solution[])this.crossoverOperator.Execute(parents);
						this.Problem.EvaluateConstraints(offSpring[0]);
						this.Problem.EvaluateConstraints(offSpring[1]);
						this.Problem.Evaluate(offSpring[0]);
						this.Problem.Evaluate(offSpring[1]);
						this.evaluations += 2;
						if (this.evaluations < this.maxEvaluations)
						{
							this.subSet.Add(offSpring[0]);
							this.subSet.Add(offSpring[1]);
						}
						parents[0].Marked();
						parents[1].Marked();
					}
				}
			}

			return this.subSet.Size();
		}

		/// <summary>
		/// Runs of the AbYSS algorithm.
		/// </summary>
		/// <returns>a <code>SolutionSet</code> that is a set of non dominated solutions
		/// as a result of the algorithm execution  </returns>
		public override SolutionSet Execute()
		{
			// STEP 1. Initialize parameters
			this.InitParam();

			// STEP 2. Build the initial solutionSet
			Solution solution;
			for (int i = 0; i < this.solutionSetSize; i++)
			{
				solution = this.DiversificationGeneration();
				this.Problem.Evaluate(solution);
				this.Problem.EvaluateConstraints(solution);
				this.evaluations++;
				solution = (Solution)this.improvementOperator.Execute(solution);
				this.evaluations += this.improvementOperator.GetEvaluations();
				this.solutionSet.Add(solution);
			}

			// STEP 3. Main loop
			int newSolutions = 0;
			while (this.evaluations < this.maxEvaluations)
			{
				this.ReferenceSetUpdate(true);
				newSolutions = this.SubSetGeneration();
				while (newSolutions > 0)
				{ // New solutions are created
					this.ReferenceSetUpdate(false);
					if (this.evaluations >= this.maxEvaluations)
					{
						this.Result = this.archive;
						return this.archive;
					}
					newSolutions = this.SubSetGeneration();
				} // while

				// RE-START
				if (this.evaluations < this.maxEvaluations)
				{
					this.solutionSet.Clear();
					// Add refSet1 to SolutionSet
					for (int i = 0; i < this.refSet1.Size(); i++)
					{
						solution = this.refSet1.Get(i);
						solution.UnMarked();
						solution = (Solution)this.improvementOperator.Execute(solution);
						this.evaluations += this.improvementOperator.GetEvaluations();
						this.solutionSet.Add(solution);
					}
					// Remove refSet1 and refSet2
					this.refSet1.Clear();
					this.refSet2.Clear();

					// Sort the archive and insert the best solutions
					this.distance.CrowdingDistanceAssignment(this.archive, this.Problem.NumberOfObjectives);
					this.archive.Sort(this.crowdingDistance);

					int insert = this.solutionSetSize / 2;
					if (insert > this.archive.Size())
						insert = this.archive.Size();

					if (insert > (this.solutionSetSize - this.solutionSet.Size()))
						insert = this.solutionSetSize - this.solutionSet.Size();

					// Insert solutions 
					for (int i = 0; i < insert; i++)
					{
						solution = new Solution(this.archive.Get(i));
						solution.UnMarked();
						this.solutionSet.Add(solution);
					}

					// Create the rest of solutions randomly
					while (this.solutionSet.Size() < this.solutionSetSize)
					{
						solution = this.DiversificationGeneration();
						this.Problem.EvaluateConstraints(solution);
						this.Problem.Evaluate(solution);
						this.evaluations++;
						solution = (Solution)this.improvementOperator.Execute(solution);
						this.evaluations += this.improvementOperator.GetEvaluations();
						solution.UnMarked();
						this.solutionSet.Add(solution);
					}
				}
			}

			// STEP 4. Return the archive
			this.Result = this.archive;
			return this.archive;
		}
	}
}
