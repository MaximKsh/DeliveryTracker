using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.Variable;

namespace DeliveryTracker.MOEA.Encoding.SolutionType
{
	/// <summary>
	/// Class representing the solution type of solutions composed of Permutation 
	/// variables
	/// </summary>
	public class PermutationSolutionType : Core.SolutionType
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		public PermutationSolutionType(Problem problem)
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
				variables[i] = new Permutation(this.Problem.GetLength(i));
			}

			return variables;
		}
	}
}
