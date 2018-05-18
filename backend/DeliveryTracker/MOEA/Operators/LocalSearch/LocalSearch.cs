using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Operators.LocalSearch
{
	/// <summary>
	/// Abstract class representing a generic local search operator
	/// </summary>
	public abstract class LocalSearch : Operator
	{
		public LocalSearch(Dictionary<string, object> parameters)
			: base(parameters)
		{
		}

		/// <summary>
		/// Returns the number of evaluations made by the local search operator
		/// </summary>
		/// <returns></returns>
		public abstract int GetEvaluations();
	}

}
