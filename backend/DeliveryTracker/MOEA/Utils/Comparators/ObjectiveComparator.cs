using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Utils.Comparators
{
	public class ObjectiveComparator : IComparer<Solution>
	{
		#region Private attibutes

		/// <summary>
		/// Stores the index of the objective to compare
		/// </summary>
		private int nObj;
		private bool ascendingOrder;

		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nObj">The index of the objective to compare</param>
		public ObjectiveComparator(int nObj)
		{
			this.nObj = nObj;
			this.ascendingOrder = true;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nObj">The index of the objective to compare</param>
		/// <param name="descendingOrder">The descending order</param>
		public ObjectiveComparator(int nObj, bool descendingOrder)
		{
			this.nObj = nObj;
			this.ascendingOrder = !descendingOrder;
		}

		#endregion

		#region Implement Interface

		public int Compare(Solution s1, Solution s2)
		{
			int result;

			if (s1 == null)
			{
				result = 1;
			}
			else if (s2 == null)
			{
				result = -1;
			}
			else
			{
				double objective1 = s1.Objective[this.nObj];
				double objective2 = s2.Objective[this.nObj];

				if (this.ascendingOrder)
				{
					if (objective1 < objective2)
					{
						result = -1;
					}
					else if (objective1 > objective2)
					{
						result = 1;
					}
					else
					{
						result = 0;
					}
				}
				else
				{
					if (objective1 < objective2)
					{
						result = 1;
					}
					else if (objective1 > objective2)
					{
						result = -1;
					}
					else
					{
						result = 0;
					}
				}
			}

			return result;
		}

		#endregion
	}
}
