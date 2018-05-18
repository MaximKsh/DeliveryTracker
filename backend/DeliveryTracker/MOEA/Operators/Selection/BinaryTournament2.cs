using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;
using DeliveryTracker.MOEA.Utils.Comparators;

namespace DeliveryTracker.MOEA.Operators.Selection
{
	/// <summary>
	/// This class implements an operator for binary selections using the same code in Deb's NSGA-II implementation
	/// </summary>
	public class BinaryTournament2 : Selection
	{
		/// <summary>
		/// Store the <code>Comparator</code> for check dominance
		/// </summary>
		private IComparer<Solution> dominance;

		/// <summary>
		/// Stores a permutation of the solutions in the solutionSet used
		/// </summary>
		private int[] a;

		/// <summary>
		/// Stores the actual index for selection
		/// </summary>
		private int index = 0;

		/// <summary>
		/// Constructor Creates a new instance of the Binary tournament operator
		/// (Deb's NSGA-II implementation version)
		/// </summary>
		/// <param name="parameters"></param>
		public BinaryTournament2(Dictionary<string, object> parameters)
			: base(parameters)
		{
			this.dominance = new DominanceComparator();
		}

		/// <summary>
		/// Performs the operation
		/// </summary>
		/// <param name="obj">Object representing a SolutionSet</param>
		/// <returns>The selected solution</returns>
		public override object Execute(object obj)
		{
			SolutionSet population = (SolutionSet)obj;
			if (this.index == 0) //Create the permutation
			{
				this.a = (new PermutationUtility()).IntPermutation(population.Size());
			}

			Solution solution1, solution2;
			solution1 = population.Get(this.a[this.index]);
			solution2 = population.Get(this.a[this.index + 1]);

			this.index = (this.index + 2) % population.Size();

			int flag = this.dominance.Compare(solution1, solution2);
			if (flag == -1)
			{
				return solution1;
			}
			else if (flag == 1)
			{
				return solution2;
			}
			else if (solution1.CrowdingDistance > solution2.CrowdingDistance)
			{
				return solution1;
			}
			else if (solution2.CrowdingDistance > solution1.CrowdingDistance)
			{
				return solution2;
			}
			else if (JMetalRandom.NextDouble() < 0.5)
			{
				return solution1;
			}
			else
			{
				return solution2;
			}
		}
	}
}
