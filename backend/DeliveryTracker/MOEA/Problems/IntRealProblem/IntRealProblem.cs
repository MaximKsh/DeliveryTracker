using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.IntRealProblem
{
	/// <summary>
	/// Class representing problem OneMax. The problem consist of maximizing the
	/// number of '1's in a binary string.
	/// </summary>
	public class IntRealProblem : Problem
	{
		int intVariables;
		int realVariables;

		/// <summary>
		/// Constructor.
		/// Creates a default instance of the IntRealProblem problem.
		/// /// </summary>
		/// <param name="solutionType"></param>
		public IntRealProblem(string solutionType)
			: this(solutionType, 3, 3)
		{
		}

		/// <summary>
		/// Constructor.
		/// Creates a new instance of the IntRealProblem problem.
		/// </summary>
		/// <param name="solutionType"></param>
		/// <param name="intVariables">Number of integer variables of the problem </param>
		/// <param name="realVariables">Number of real variables of the problem </param>
		public IntRealProblem(string solutionType, int intVariables, int realVariables)
		{
			this.intVariables = intVariables;
			this.realVariables = realVariables;

			this.NumberOfVariables = intVariables + realVariables;
			this.NumberOfObjectives = 2;
			this.NumberOfConstraints = 0;
			this.ProblemName = "IntRealProblem";

			this.UpperLimit = new double[this.NumberOfVariables];
			this.LowerLimit = new double[this.NumberOfVariables];

			for (int i = 0; i < intVariables; i++)
			{
				this.LowerLimit[i] = -5;
				this.UpperLimit[i] = 5;
			}

			for (int i = intVariables; i < this.NumberOfVariables; i++)
			{
				this.LowerLimit[i] = -5.0;
				this.UpperLimit[i] = 5.0;
			}

			if (solutionType == "IntReal")
				this.SolutionType = new IntRealSolutionType(this, intVariables, realVariables);
			else
			{
				Console.WriteLine("Error: solution type " + solutionType + " is invalid");
				Logger.Log.Error("Solution type " + solutionType + " is invalid");
				return;
			}
		}

		/// <summary>
		/// Evaluates a solution 
		/// </summary>
		/// <param name="solution">The solution to evaluate</param>
		public override void Evaluate(Solution solution)
		{
			Variable[] variable = solution.Variable;

			double[] fx = new double[2]; // function values     

			fx[0] = 0.0;
			for (int var = 0; var < this.intVariables; var++)
			{
				fx[0] += (int)variable[var].Value;
			}

			fx[1] = 0.0;
			for (int var = this.intVariables; var < this.NumberOfVariables; var++)
			{
				fx[1] += (int)variable[var].Value;
			}

			solution.Objective[0] = fx[0];
			solution.Objective[1] = fx[1];
		}
	}
}
