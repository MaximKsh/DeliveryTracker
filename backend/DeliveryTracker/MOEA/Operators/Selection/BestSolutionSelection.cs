using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Operators.Selection
{
	/// <summary>
	/// This class implements a selection operator used for selecting the best 
	/// solution in a SolutionSet according to a given comparator
	/// </summary>
	public class BestSolutionSelection : Selection
	{
		/// <summary>
		/// Comparator
		/// </summary>
		private IComparer<Solution> comparator;

		public BestSolutionSelection(Dictionary<string, object> parameters)
			: base(parameters)
		{

			this.comparator = null;
			object value;

			if (parameters.TryGetValue("comparator", out value))
				this.comparator = (IComparer<Solution>)value;
		}

		/// <summary>
		/// Performs the operation
		/// </summary>
		/// <param name="obj">Object representing a SolutionSet.</param>
		/// <returns>the best solution found</returns>
		public override object Execute(object obj)
		{
			SolutionSet solutionSet = (SolutionSet)obj;

			if (solutionSet.Size() == 0)
			{
				return null;
			}
			int bestSolution;

			bestSolution = 0;

			for (int i = 1; i < solutionSet.Size(); i++)
			{
				if (this.comparator.Compare(solutionSet.Get(i), solutionSet.Get(bestSolution)) < 0)
					bestSolution = i;
			}

			return bestSolution;
		}
	}
}
