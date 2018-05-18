using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Operators.Selection
{
	/// <summary>
	/// This class represents the super class of all the selection operators
	/// </summary>
	public abstract class Selection : Operator
	{
		public Selection(Dictionary<string, object> parameters)
			: base(parameters)
		{

		}
	}
}
