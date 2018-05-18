using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;
using DeliveryTracker.MOEA.Utils.Comparators;
using DeliveryTracker.MOEA.Utils.OffSpring;

namespace DeliveryTracker.MOEA.Metaheuristics.NSGAII
{
	public class NSGAIIRandom : Algorithm
	{
		public int populationSize;
		public SolutionSet population;
		public SolutionSet offspringPopulation;
		public SolutionSet union;

		int maxEvaluations;
		int evaluations;

		int[] contributionCounter; // contribution per crossover operator
		double[] contribution; // contribution per crossover operator

		Operator selectionOperator;

		public NSGAIIRandom(Problem problem)
			: base(problem)
		{
		}

		public override SolutionSet Execute()
		{
			double[] contrReal = new double[3];

			contrReal[0] = contrReal[1] = contrReal[2] = 0;

			Distance distance = new Distance();

			//Read parameter values
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "maxEvaluations", ref this.maxEvaluations);
			Utils.Utils.GetIntValueFromParameter(this.InputParameters, "populationSize", ref this.populationSize);

			//Init the variables
			this.population = new SolutionSet(this.populationSize);
			this.evaluations = 0;

			this.selectionOperator = this.Operators["selection"];

			Offspring[] getOffspring;
			int N_O; // number of offpring objects

			getOffspring = (Offspring[])this.GetInputParameter("offspringsCreators");
			N_O = getOffspring.Length;

			this.contribution = new double[N_O];
			this.contributionCounter = new int[N_O];

			this.contribution[0] = (double)(this.populationSize / (double)N_O) / (double)this.populationSize;
			for (int i = 1; i < N_O; i++)
			{
				this.contribution[i] = (double)(this.populationSize / (double)N_O) / (double)this.populationSize + (double)this.contribution[i - 1];
			}

			for (int i = 0; i < N_O; i++)
			{
				Console.WriteLine(getOffspring[i].Configuration());
				Console.WriteLine("Contribution: " + this.contribution[i]);
			}

			// Create the initial solutionSet
			Solution newSolution;
			for (int i = 0; i < this.populationSize; i++)
			{
				newSolution = new Solution(this.Problem);
				this.Problem.Evaluate(newSolution);
				this.Problem.EvaluateConstraints(newSolution);
				this.evaluations++;
				newSolution.Location = i;
				this.population.Add(newSolution);
			}

			while (this.evaluations < this.maxEvaluations)
			{

				// Create the offSpring solutionSet      
				this.offspringPopulation = new SolutionSet(this.populationSize);
				Solution[] parents = new Solution[2];
				for (int i = 0; i < (this.populationSize / 1); i++)
				{
					if (this.evaluations < this.maxEvaluations)
					{
						Solution individual = new Solution(this.population.Get(JMetalRandom.Next(0, this.populationSize - 1)));

						int selected = 0;
						bool found = false;
						Solution offSpring = null;
						double rnd = JMetalRandom.NextDouble();
						for (selected = 0; selected < N_O; selected++)
						{

							if (!found && (rnd <= this.contribution[selected]))
							{
								if ("DE" == getOffspring[selected].Id)
								{
									offSpring = getOffspring[selected].GetOffspring(this.population, i);
									//contrDE++;
								}
								else if ("SBXCrossover" == getOffspring[selected].Id)
								{
									offSpring = getOffspring[selected].GetOffspring(this.population);
									//contrSBX++;
								}
								else if ("PolynomialMutation" == getOffspring[selected].Id)
								{
									offSpring = ((PolynomialMutationOffspring)getOffspring[selected]).GetOffspring(individual);
									//contrPol++;
								}
								else
								{
									Logger.Log.Error("Error in NSGAIIRandom. Operator " + offSpring + " does not exist");
									Console.WriteLine("Error in NSGAIIRandom. Operator " + offSpring + " does not exist");
								}

								offSpring.Fitness = (int)selected;
								found = true;
							}
						}

						this.Problem.Evaluate(offSpring);
						this.offspringPopulation.Add(offSpring);
						this.evaluations += 1;
					}
				}

				// Create the solutionSet union of solutionSet and offSpring
				this.union = ((SolutionSet)this.population).Union(this.offspringPopulation);

				// Ranking the union
				Ranking ranking = new Ranking(this.union);

				int remain = this.populationSize;
				int index = 0;
				SolutionSet front = null;
				this.population.Clear();

				// Obtain the next front
				front = ranking.GetSubfront(index);

				while ((remain > 0) && (remain >= front.Size()))
				{
					//Assign crowding distance to individuals
					distance.CrowdingDistanceAssignment(front, this.Problem.NumberOfObjectives);
					//Add the individuals of this front
					for (int k = 0; k < front.Size(); k++)
					{
						this.population.Add(front.Get(k));
					}

					//Decrement remain
					remain = remain - front.Size();

					//Obtain the next front
					index++;
					if (remain > 0)
					{
						front = ranking.GetSubfront(index);
					}
				}

				// Remain is less than front(index).size, insert only the best one
				if (remain > 0)
				{  // front contains individuals to insert                        
					distance.CrowdingDistanceAssignment(front, this.Problem.NumberOfObjectives);
					front.Sort(new CrowdingComparator());
					for (int k = 0; k < remain; k++)
					{
						this.population.Add(front.Get(k));
					}
				}
			}

			// Return the first non-dominated front
			Ranking rank = new Ranking(this.population);

			this.Result = rank.GetSubfront(0);

			return this.Result;
		}
	}
}
