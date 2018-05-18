using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Encoding.Variable;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.DTLZ
{
	/// <summary>
	/// Class representing problem DTLZ7
	/// </summary>
	public class DTLZ7 : Problem
	{
		#region Constructors

		/// <summary>
		/// Creates a default DTLZ7 problem instance (22 variables and 3 objectives)
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		public DTLZ7(string solutionType)
			: this(solutionType, 22, 3)
		{

		}

		/// <summary>
		/// Creates a new DTLZ7 problem instance
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		/// <param name="numberOfVariables">Number of variables</param>
		/// <param name="numberOfObjectives">Number of objective functions</param>
		public DTLZ7(string solutionType, int numberOfVariables, int numberOfObjectives)
		{
			this.NumberOfVariables = numberOfVariables;
			this.NumberOfObjectives = numberOfObjectives;
			this.NumberOfConstraints = 0;
			this.ProblemName = "DTLZ7";

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
			int k = this.NumberOfVariables - this.NumberOfObjectives + 1;

			for (int i = 0; i < this.NumberOfVariables; i++)
			{
				x[i] = this.GetVariableValue(gen[i]);
			}

			//Calculate g
			double g = 0.0;
			for (int i = this.NumberOfVariables - k; i < this.NumberOfVariables; i++)
			{
				g += x[i];
			}

			g = 1 + (9.0 * g) / k;
			//<-

			//Calculate the value of f1,f2,f3,...,fM-1 (take acount of vectors start at 0)
			Array.Copy(x, 0, f, 0, this.NumberOfObjectives - 1);
			//<-

			//->Calculate fM
			double h = 0.0;
			for (int i = 0; i < this.NumberOfObjectives - 1; i++)
			{
				h += (f[i] / (1.0 + g)) * (1 + Math.Sin(3.0 * Math.PI * f[i]));
			}

			h = this.NumberOfObjectives - h;

			f[this.NumberOfObjectives - 1] = (1 + g) * h;
			//<-

			//-> Setting up the value of the objetives
			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				solution.Objective[i] = f[i];
			}
			//<-
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
