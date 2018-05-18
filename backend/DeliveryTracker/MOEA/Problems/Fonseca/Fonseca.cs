using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Utils;
using DeliveryTracker.MOEA.Utils.Wrapper;

namespace DeliveryTracker.MOEA.Problems.Fonseca
{

	/// <summary>
	/// Class representing problem Fonseca
	/// </summary>
	public class Fonseca : Problem
	{
		#region Constructor

		/// <summary>
		/// Constructor
		/// Creates a default instance of the Fonseca problem
		/// </summary>
		/// <param name="solutionType">The solution type must "Real", "BinaryReal, ArrayReal, or ArrayRealC".</param>
		public Fonseca(string solutionType)
		{
			this.NumberOfVariables = 3;
			this.NumberOfObjectives = 2;
			this.NumberOfConstraints = 0;
			this.ProblemName = "Fonseca";

			this.UpperLimit = new double[this.NumberOfVariables];
			this.LowerLimit = new double[this.NumberOfVariables];
			for (int var = 0; var < this.NumberOfVariables; var++)
			{
				this.LowerLimit[var] = -4.0;
				this.UpperLimit[var] = 4.0;
			}

			if (solutionType == "BinaryReal")
				this.SolutionType = new BinaryRealSolutionType(this);
			else if (solutionType == "Real")
				this.SolutionType = new RealSolutionType(this);
			else if (solutionType == "ArrayReal")
				this.SolutionType = new ArrayRealSolutionType(this);
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
			XReal x = new XReal(solution);

			double[] f = new double[this.NumberOfObjectives];
			double sum1 = 0.0;
			for (int var = 0; var < this.NumberOfVariables; var++)
			{
				sum1 += Math.Pow(x.GetValue(var) - (1.0 / Math.Sqrt((double)this.NumberOfVariables)), 2.0);
			}
			double exp1 = Math.Exp((-1.0) * sum1);
			f[0] = 1 - exp1;

			double sum2 = 0.0;
			for (int var = 0; var < this.NumberOfVariables; var++)
			{
				sum2 += Math.Pow(x.GetValue(var) + (1.0 / Math.Sqrt((double)this.NumberOfVariables)), 2.0);
			}
			double exp2 = Math.Exp((-1.0) * sum2);
			f[1] = 1 - exp2;

			solution.Objective[0] = f[0];
			solution.Objective[1] = f[1];
		}

		#endregion
	}
}
