using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.Variable;

namespace DeliveryTracker.MOEA.Encoding.SolutionType
{
	/// <summary>
	/// Class representing the solution type of solutions composed of an ArrayReal 
	/// encodings.variable
	/// </summary>
	public class ArrayRealSolutionType : Core.SolutionType
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public ArrayRealSolutionType(Problem problem)
			: base(problem)
		{

		}

		public override Core.Variable[] CreateVariables()
		{
			Core.Variable[] variables = new Core.Variable[1];

			variables[0] = new ArrayReal(this.Problem.NumberOfVariables, this.Problem);
			return variables;
		}
	}
}
