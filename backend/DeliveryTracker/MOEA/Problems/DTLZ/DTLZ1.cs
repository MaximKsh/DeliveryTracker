using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Encoding.Variable;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.DTLZ
{
	/// <summary>
	/// Class representing problem DTLZ1
	/// </summary>
	public class DTLZ1 : Problem
	{
		#region Constructors
		/// <summary>
		/// Creates a default DTLZ1 problem (7 variables and 3 objectives)
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		public DTLZ1(string solutionType)
			: this(solutionType, 7, 3)
		{
		}

		/// <summary>
		/// Creates a DTLZ1 problem instance
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal"</param>
		/// <param name="numberOfVariables">Number of variables</param>
		/// <param name="numberOfObjectives">Number of objective functions</param>
		public DTLZ1(string solutionType, int numberOfVariables, int numberOfObjectives)
		{
			this.NumberOfVariables = numberOfVariables;
			this.NumberOfObjectives = numberOfObjectives;
			this.NumberOfConstraints = 0;
			this.ProblemName = "DTLZ1";

			this.LowerLimit = new double[this.NumberOfVariables];
			this.UpperLimit = new double[this.NumberOfVariables];
			for (int var = 0; var < numberOfVariables; var++)
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


			double[] f = new double[this.NumberOfObjectives];
			int k = this.NumberOfVariables - this.NumberOfObjectives + 1;

			double[] x = this.GetVariableValues(gen);

			double g = 0.0;
			for (int i = this.NumberOfVariables - k; i < this.NumberOfVariables; i++)
			{
				g += (x[i] - 0.5) * (x[i] - 0.5) - Math.Cos(20.0 * Math.PI * (x[i] - 0.5));
			}

			g = 100 * (k + g);
			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				f[i] = (1.0 + g) * 0.5;
			}

			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				for (int j = 0; j < this.NumberOfObjectives - (i + 1); j++)
				{
					f[i] *= x[j];
				}
				if (i != 0)
				{
					int aux = this.NumberOfObjectives - (i + 1);
					f[i] *= 1 - x[aux];
				}
			}

			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				solution.Objective[i] = f[i];
			}
		}

		#endregion

		#region Private Methods

		private double[] GetVariableValues(Variable[] gen)
		{
			double[] x = new double[this.NumberOfVariables];

			for (int i = 0; i < this.NumberOfVariables; i++)
			{
				x[i] = this.GetVariableValue(gen[i]);
			}
			return x;
		}

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
