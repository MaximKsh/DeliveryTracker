using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.Variable;

namespace DeliveryTracker.MOEA.Encoding.SolutionType
{
	/// <summary>
	/// Class representing  a solution type including two variables: an integer 
	/// and a real.
	/// </summary>
	public class IntRealSolutionType : Core.SolutionType
	{
		private readonly int intVariables;
		private readonly int realVariables;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">Problem to solve</param>
		/// <param name="intVariables">Number of integer variables</param>
		/// <param name="realVariables">Number of real variables</param>
		public IntRealSolutionType(Problem problem, int intVariables, int realVariables)
			: base(problem)
		{
			this.intVariables = intVariables;
			this.realVariables = realVariables;
		}

		/// <summary>
		/// Create the variables of the solution
		/// </summary>
		/// <returns></returns>
		public override Core.Variable[] CreateVariables()
		{
			Core.Variable[] variables = new Core.Variable[this.Problem.NumberOfVariables];

			for (int i = 0; i < this.intVariables; i++)
			{
				variables[i] = new Int((int)this.Problem.LowerLimit[i], (int)this.Problem.UpperLimit[i]);
			}

			for (int i = this.intVariables; i < (this.intVariables + this.realVariables); i++)
			{
				variables[i] = new Real(this.Problem.LowerLimit[i], this.Problem.UpperLimit[i]);
			}

			return variables;
		}
	}
}
