using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Encoding.Variable;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.Schaffer
{
	/// <summary>
	/// Class representing problem Schaffer
	/// </summary>
	public class Schaffer : Problem
	{
		#region Constructor
		/// <summary>
		/// Constructor.
		/// Creates a default instance of problem Schaffer
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		public Schaffer(string solutionType)
		{
			this.NumberOfVariables = 1;
			this.NumberOfObjectives = 2;
			this.NumberOfConstraints = 0;
			this.ProblemName = "Schaffer";

			this.LowerLimit = new double[this.NumberOfVariables];
			this.UpperLimit = new double[this.NumberOfVariables];
			this.LowerLimit[0] = -100000;
			this.UpperLimit[0] = 100000;

			if (solutionType == "BinaryReal")
				this.SolutionType = new BinaryRealSolutionType(this);
			else if (solutionType == "Real")
				this.SolutionType = new RealSolutionType(this);
			else
			{
				Console.WriteLine("Error: solution type " + solutionType + " is invalid");
				Logger.Log.Error("Error: solution type " + solutionType + " is invalid");
				Environment.Exit(-1);
			}
		}

		#endregion

		#region Public Overrides
		/// <summary>
		/// Evaluates a solution
		/// </summary>
		/// <param name="solution">The solution to evaluate</param>
		public override void Evaluate(Solution solution)
		{
			double variable = this.GetVariableValue(solution.Variable);

			double[] f = new double[this.NumberOfObjectives];
			f[0] = variable * variable;

			f[1] = (variable - 2.0) * (variable - 2.0);

			solution.Objective[0] = f[0];
			solution.Objective[1] = f[1];
		}

		#endregion

		#region Private Methods

		private double GetVariableValue(Variable variable)
		{
			double result;

			if (this.SolutionType.GetType() == typeof(BinaryRealSolutionType))
			{
				result = ((BinaryReal)variable).Value;
			}
			else
			{
				result = ((Real)variable).Value;
			}
			return result;
		}

		private double GetVariableValue(Variable[] gen)
		{
			double x = this.GetVariableValue(gen[0]);

			return x;
		}

		#endregion
	}

}
