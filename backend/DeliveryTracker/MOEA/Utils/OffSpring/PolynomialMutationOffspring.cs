using System;
using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Operators.Mutation;
using DeliveryTracker.MOEA.Operators.Selection;

namespace DeliveryTracker.MOEA.Utils.OffSpring
{
	public class PolynomialMutationOffspring : Offspring
	{
		private Operator mutation;
		private Operator selection;

		private double mutationProbability;
		private double distributionIndex;

		public PolynomialMutationOffspring(double mutationProbability, double distributionIndexForMutation)
		{
			Dictionary<string, object> parameters; // Operator parameters
			parameters = new Dictionary<string, object>();
			parameters.Add("probability", this.mutationProbability = mutationProbability);
			parameters.Add("distributionIndex", this.distributionIndex = distributionIndexForMutation);
			this.mutation = MutationFactory.GetMutationOperator("PolynomialMutation", parameters);

			this.selection = SelectionFactory.GetSelectionOperator("BinaryTournament", null);
			this.Id = "PolynomialMutation";
		}

		public Solution GetOffspring(Solution solution)
		{
			Solution res = new Solution(solution);
			try
			{
				this.mutation.Execute(res);
			}
			catch (Exception ex)
			{
				Logger.Log.Error("Exception in " + this.GetType().FullName + ".GetOffspring()", ex);
			}
			return res;
		}

		public override string Configuration()
		{
			string result = "-----\n";
			result += "Operator: " + this.Id + "\n";
			result += "Probability: " + this.mutationProbability + "\n";
			result += "DistributionIndex: " + this.distributionIndex;

			return result;
		}
	}
}
