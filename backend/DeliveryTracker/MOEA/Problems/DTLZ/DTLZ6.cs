using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Encoding.Variable;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.DTLZ
{
	/// <summary>
	/// Class representing problem DTLZ6
	/// </summary>
	public class DTLZ6 : Problem
	{
		#region Constructors

		/// <summary>
		/// Creates a default DTLZ6 problem instance (12 variables and 3 objectives)
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		public DTLZ6(string solutionType)
			: this(solutionType, 12, 3)
		{
		}

		/// <summary>
		/// Creates a new DTLZ6 problem instance
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal"</param>
		/// <param name="numberOfVariables">Number of variables</param>
		/// <param name="numberOfObjectives">Number of objective functions</param>
		public DTLZ6(string solutionType, int numberOfVariables, int numberOfObjectives)
		{
			this.NumberOfVariables = numberOfVariables;
			this.NumberOfObjectives = numberOfObjectives;
			this.NumberOfConstraints = 0;
			this.ProblemName = "DTLZ6";

			this.LowerLimit = new double[this.NumberOfVariables];
			this.UpperLimit = new double[this.NumberOfVariables];
			for (int var = 0; var < this.NumberOfVariables; var++)
			{
				this.LowerLimit[var] = 0.0;
				this.UpperLimit[var] = 1.0;
			}

			if (solutionType == "BinaryReal")
			{
				this.SolutionType = new BinaryRealSolutionType(this);
			}
			else if (solutionType == "Real")
			{
				this.SolutionType = new RealSolutionType(this);
			}
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
			Variable[] gen = solution.Variable;

			double[] x = new double[this.NumberOfVariables];
			double[] f = new double[this.NumberOfObjectives];
			double[] theta = new double[this.NumberOfObjectives - 1];
			int k = this.NumberOfVariables - this.NumberOfObjectives + 1;

			for (int i = 0; i < this.NumberOfVariables; i++)
			{
				x[i] = this.GetVariableValue(gen[i]);
			}

			double g = 0.0;
			for (int i = this.NumberOfVariables - k; i < this.NumberOfVariables; i++)
			{
				g += Math.Pow(x[i], 0.1);
			}

			double t = Math.PI / (4.0 * (1.0 + g));
			theta[0] = x[0] * Math.PI / 2;
			for (int i = 1; i < (this.NumberOfObjectives - 1); i++)
			{
				theta[i] = t * (1.0 + 2.0 * g * x[i]);
			}

			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				f[i] = 1.0 + g;
			}

			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				for (int j = 0; j < this.NumberOfObjectives - (i + 1); j++)
				{
					f[i] *= Math.Cos(theta[j]);
				}
				if (i != 0)
				{
					int aux = this.NumberOfObjectives - (i + 1);
					f[i] *= Math.Sin(theta[aux]);
				}
			}

			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				solution.Objective[i] = f[i];
			}
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

		#endregion
	}
}
