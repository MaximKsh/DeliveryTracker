using System;
using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Operators.Crossover;
using DeliveryTracker.MOEA.Operators.Selection;

namespace DeliveryTracker.MOEA.Utils.OffSpring
{
	public class SBXCrossoverOffspring : Offspring
	{
		#region Private Attributes

		private double crossoverProbability = 0.9;
		private double distributionIndexForCrossover = 20;
		private Operator crossover;
		private Operator selection;

		#endregion

		#region Constructors
		public SBXCrossoverOffspring(double crossoverProbability, double distributionIndexForCrossover)
		{
			Dictionary<string, object> parameters;
			this.crossoverProbability = crossoverProbability;
			this.distributionIndexForCrossover = distributionIndexForCrossover;

			// Crossover operator
			parameters = new Dictionary<string, object>();
			parameters.Add("probability", crossoverProbability);
			parameters.Add("distributionIndex", distributionIndexForCrossover);

			this.crossover = CrossoverFactory.GetCrossoverOperator("SBXCrossover", parameters);

			this.selection = SelectionFactory.GetSelectionOperator("BinaryTournament", null);

			this.Id = "SBXCrossover";
		}

		#endregion

		#region Public Overrides
		public override Solution GetOffspring(SolutionSet solutionSet)
		{
			Solution[] parents = new Solution[2];
			Solution offSpring = null;

			try
			{
				parents[0] = (Solution)this.selection.Execute(solutionSet);
				parents[1] = (Solution)this.selection.Execute(solutionSet);

				Solution[] children = (Solution[])this.crossover.Execute(parents);
				offSpring = children[0];
				//Create a new solution, using DE
			}
			catch (Exception ex)
			{
				Logger.Log.Error("Error in: " + this.GetType().FullName + ".GetOffspring()", ex);
				Console.Error.WriteLine("Error in: " + this.GetType().FullName + ".GetOffspring()");
			}
			return offSpring;
		}

		public override Solution GetOffspring(Solution[] parentSolutions)
		{
			Solution[] parents = new Solution[2];
			Solution offSpring = null;

			try
			{
				parents[0] = parentSolutions[0];
				parents[1] = parentSolutions[1];

				Solution[] children = (Solution[])this.crossover.Execute(parents);
				offSpring = children[0];
				//Create a new solution, using DE
			}
			catch (Exception ex)
			{
				Logger.Log.Error("Error in: " + this.GetType().FullName + ".GetOffspring()", ex);
				Console.Error.WriteLine("Error in: " + this.GetType().FullName);
			}
			return offSpring;
		}

		public override Solution GetOffspring(SolutionSet solutionSet, SolutionSet archive)
		{
			Solution[] parents = new Solution[2];
			Solution offSpring = null;

			try
			{
				parents[0] = (Solution)this.selection.Execute(solutionSet);

				if (archive.Size() > 0)
				{
					parents[1] = (Solution)this.selection.Execute(archive);
				}
				else
				{
					parents[1] = (Solution)this.selection.Execute(solutionSet);
				}

				Solution[] children = (Solution[])this.crossover.Execute(parents);
				offSpring = children[0];
				//Create a new solution, using DE
			}
			catch (Exception ex)
			{
				Logger.Log.Error("Error in: " + this.GetType().FullName + ".GetOffspring()", ex);
				Console.Error.WriteLine("Error in: " + this.GetType().FullName);
			}
			return offSpring;
		}

		public override string Configuration()
		{
			string result = "-----\n";
			result += "Operator: " + this.Id + "\n";
			result += "Probability: " + this.crossoverProbability + "\n";
			result += "DistributionIndex: " + this.distributionIndexForCrossover;

			return result;
		}

		#endregion
	}
}
