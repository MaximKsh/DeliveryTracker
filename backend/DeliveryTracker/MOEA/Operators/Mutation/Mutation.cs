using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Operators.Mutation
{
	public abstract class Mutation : Operator
	{
		public Mutation(Dictionary<string, object> parameters)
			: base(parameters)
		{

		}
	}
}
