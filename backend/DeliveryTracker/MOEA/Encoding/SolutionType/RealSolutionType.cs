using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.Variable;

namespace DeliveryTracker.MOEA.Encoding.SolutionType
{
	/// <summary>
	/// Class representing a solution type composed of real variables
	/// </summary>
	public class RealSolutionType : Core.SolutionType
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public RealSolutionType(Problem problem)
			: base(problem)
		{

		}

		public override Core.Variable[] CreateVariables()
		{
			Core.Variable[] variables = new Core.Variable[this.Problem.NumberOfVariables];

			for (int i = 0, li = this.Problem.NumberOfVariables; i < li; i++)
			{
				variables[i] = new Real(this.Problem.LowerLimit[i], this.Problem.UpperLimit[i]);
			}

			return variables;
		}
	}
}
