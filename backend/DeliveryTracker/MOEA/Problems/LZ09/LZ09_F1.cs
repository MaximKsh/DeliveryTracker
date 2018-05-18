using System;
using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.LZ09
{
	/// <summary>
	/// Class representing problem LZ09_F1
	/// </summary>
	public class LZ09_F1 : Problem
	{
		#region Private Attributes

		LZ09 LZ09;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a default LZ09_F1 problem (30 variables and 2 objectives)
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		public LZ09_F1(string solutionType)
			: this(solutionType, 21, 1, 21)
		{

		}

		/// <summary>
		/// Creates a LZ09_F1 problem instance
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal"</param>
		/// <param name="ptype"></param>
		/// <param name="dtype"></param>
		/// <param name="ltype"></param>
		public LZ09_F1(string solutionType, int ptype, int dtype, int ltype)
		{
			this.NumberOfVariables = 10;
			this.NumberOfObjectives = 2;
			this.NumberOfConstraints = 0;
			this.ProblemName = "LZ09_F1";

			this.LZ09 = new LZ09(this.NumberOfVariables, this.NumberOfObjectives, ptype, dtype, ltype);

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

			List<double> x = new List<double>(this.NumberOfVariables);
			List<double> y = new List<double>(this.NumberOfObjectives);

			for (int i = 0; i < this.NumberOfVariables; i++)
			{
				x.Add(this.LZ09.GetVariableValue(gen[i], this.SolutionType));
				y.Add(0.0);
			}

			this.LZ09.Objective(x, y);

			for (int i = 0; i < this.NumberOfObjectives; i++)
			{
				solution.Objective[i] = y[i];
			}
		}

		#endregion
	}
}
