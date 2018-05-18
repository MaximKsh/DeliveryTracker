using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Utils;
using DeliveryTracker.MOEA.Utils.Wrapper;

namespace DeliveryTracker.MOEA.Problems.Kursawe
{
	/// <summary>
	/// Class representing problem Kursawe
	/// </summary>
	public class Kursawe : Problem
	{
		#region Constructor


		/// <summary>
		/// Constructor.
		/// Creates a default instance of the Kursawe problem.
		/// </summary>
		/// <param name="solutionType">The solution type must "Real", "BinaryReal, and "ArrayReal".</param>
		public Kursawe(string solutionType)
			: this(solutionType, 3)
		{

		}

		/// <summary>
		/// Constructor.
		/// Creates a new instance of the Kursawe problem.
		/// </summary>
		/// <param name="solutionType">The solution type must "Real", "BinaryReal, and "ArrayReal".</param>
		/// <param name="numberOfVariables">Number of variables of the problem</param>
		public Kursawe(string solutionType, int numberOfVariables)
		{
			this.NumberOfVariables = numberOfVariables;
			this.NumberOfObjectives = 2;
			this.NumberOfConstraints = 0;
			this.ProblemName = "Kursawe";

			this.UpperLimit = new double[this.NumberOfVariables];
			this.LowerLimit = new double[this.NumberOfVariables];

			for (int i = 0; i < this.NumberOfVariables; i++)
			{
				this.LowerLimit[i] = -5.0;
				this.UpperLimit[i] = 5.0;
			}

			if (solutionType == "BinaryReal")
			{
				this.SolutionType = new BinaryRealSolutionType(this);
			}
			else if (solutionType == "Real")
			{
				this.SolutionType = new RealSolutionType(this);
			}
			else if (solutionType == "ArrayReal")
			{
				this.SolutionType = new ArrayRealSolutionType(this);
			}
			else
			{
				Console.WriteLine("Error: solution type " + solutionType + " is invalid");
				Logger.Log.Error("Error: solution type " + solutionType + " is invalid");
				Environment.Exit(-1);
			}
		}

		#endregion

		#region Public Override

		/// <summary>
		/// Evaluates a solution
		/// </summary>
		/// <param name="solution">The solution to evaluate</param>
		public override void Evaluate(Solution solution)
		{
			XReal vars = new XReal(solution);

			double aux, xi, xj; // auxiliary variables
			double[] fx = new double[2]; // function values
			double[] x = new double[this.NumberOfVariables];
			for (int i = 0; i < this.NumberOfVariables; i++)
			{
				x[i] = vars.GetValue(i);
			}

			fx[0] = 0.0;
			for (int var = 0; var < this.NumberOfVariables - 1; var++)
			{
				xi = x[var] * x[var];
				xj = x[var + 1] * x[var + 1];
				aux = (-0.2) * Math.Sqrt(xi + xj);
				fx[0] += (-10.0) * Math.Exp(aux);
			}

			fx[1] = 0.0;

			for (int var = 0; var < this.NumberOfVariables; var++)
			{
				fx[1] += Math.Pow(Math.Abs(x[var]), 0.8) + 5.0 * Math.Sin(Math.Pow(x[var], 3.0));
			}

			solution.Objective[0] = fx[0];
			solution.Objective[1] = fx[1];
		}

		#endregion
	}
}
