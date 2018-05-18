using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Utils.Archive
{
	/// <summary>
	/// This class represents the super class for archive objects.
	/// </summary>
	public class Archive : SolutionSet
	{
		public Archive(int size)
			: base(size)
		{

		}
	}
}
