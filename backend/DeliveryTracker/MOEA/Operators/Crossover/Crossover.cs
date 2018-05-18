using System.Collections.Generic;

namespace DeliveryTracker.MOEA.Operators.Crossover
{
	/// <summary>
	/// This class represents the super class of all the crossover operators
	/// </summary>
	public abstract class Crossover : Core.Operator
	{
		public Crossover(Dictionary<string, object> parameters)
			: base(parameters)
		{

		}
	}
}
