using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Utils.Comparators
{
	public interface IConstraintViolationComparator : IComparer<Solution>
	{
		bool NeedToCompare(Solution s1, Solution s2);
	}
}
