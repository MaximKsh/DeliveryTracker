using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.Variable;

namespace DeliveryTracker.MOEA.Encoding.SolutionType
{
	/// <summary>
	/// Class representing the solution type of solutions composed of BinaryReal 
	/// variables
	/// </summary>
	public class BinaryRealSolutionType : Core.SolutionType
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public BinaryRealSolutionType(Problem problem)
			: base(problem)
		{

		}

		/// <summary>
		/// Creates the variables of the solution
		/// </summary>
		/// <returns></returns>
		public override Core.Variable[] CreateVariables()
		{
			Core.Variable[] variables = new Core.Variable[this.Problem.NumberOfVariables];

			for (int i = 0, li = this.Problem.NumberOfVariables; i < li; i++)
			{
				if (this.Problem.Precision == null)
				{
					int[] precision = new int[this.Problem.NumberOfVariables];
					for (int j = 0, lj = this.Problem.NumberOfVariables; j < lj; j++)
					{
						precision[j] = BinaryReal.DEFAULT_PRECISION;
					}
					this.Problem.Precision = precision;
				}
				variables[i] = new BinaryReal(this.Problem.Precision[i], this.Problem.LowerLimit[i], this.Problem.UpperLimit[i]);
			}
			return variables;
		}
	}
}
