using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Encoding.Variable;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.WFG
{

	/// <summary>
	/// Implements a reference abstract class for all WFG test problems
	/// Reference: Simon Huband, Luigi Barone, Lyndon While, Phil Hingston
	///            A Scalable Multi-objective Test Problem Toolkit.
	///            Evolutionary Multi-Criterion Optimization: 
	///            Third International Conference, EMO 2005. 
	///            Proceedings, volume 3410 of Lecture Notes in Computer Science
	/// </summary>
	public abstract class WFG : Problem
	{

		#region Private Constant
		/// <summary>
		/// Stores a epsilon default value
		/// </summary>
		private readonly float epsilon = (float)1e-7;

		#endregion

		#region Protected Attributes

		protected int k; //Var for walking fish group
		protected int M;
		protected int l;
		protected int[] A;
		protected int[] S;
		protected int D = 1;
		protected Random random = new Random();

		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// Creates a WFG problem
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		/// <param name="k">position-related parameters</param>
		/// <param name="l">distance-related parameters</param>
		/// <param name="M">Number of objectives</param>
		public WFG(string solutionType, int k, int l, int M)
		{
			this.k = k;
			this.l = l;
			this.M = M;
			this.NumberOfVariables = this.k + this.l;
			this.NumberOfObjectives = this.M;
			this.NumberOfConstraints = 0;

			this.LowerLimit = new double[this.NumberOfVariables];
			this.UpperLimit = new double[this.NumberOfVariables];
			for (int var = 0; var < this.NumberOfVariables; var++)
			{
				this.LowerLimit[var] = 0;
				this.UpperLimit[var] = 2 * (var + 1);
			}

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

		#region Public Methods
		/// <summary>
		/// Gets the x vector (consulte WFG tooltik reference)
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public float[] Calculate_x(float[] t)
		{
			float[] x = new float[this.M];

			for (int i = 0; i < this.M - 1; i++)
			{
				x[i] = Math.Max(t[this.M - 1], this.A[i]) * (t[i] - (float)0.5) + (float)0.5;
			}

			x[this.M - 1] = t[this.M - 1];

			return x;
		}

		/// <summary>
		/// Normalizes a vector (consulte WFG toolkit reference)
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public float[] Normalise(float[] z)
		{
			float[] result = new float[z.Length];

			for (int i = 0; i < z.Length; i++)
			{
				float bound = (float)2.0 * (i + 1);
				result[i] = z[i] / bound;
				result[i] = this.Correct_to_01(result[i]);
			}

			return result;
		}

		public float Correct_to_01(float a)
		{
			float min = (float)0.0;
			float max = (float)1.0;

			float min_epsilon = min - this.epsilon;
			float max_epsilon = max + this.epsilon;

			if ((a <= min && a >= min_epsilon) || (a >= min && a <= min_epsilon))
			{
				return min;
			}
			else if ((a >= max && a <= max_epsilon) || (a <= max && a >= max_epsilon))
			{
				return max;
			}
			else
			{
				return a;
			}
		}

		/// <summary>
		/// Gets a subvector of a given vector (Head inclusive and tail inclusive)
		/// </summary>
		/// <param name="z">the vector</param>
		/// <param name="head"></param>
		/// <param name="tail"></param>
		/// <returns>the subvector</returns>
		public float[] SubVector(float[] z, int head, int tail)
		{
			int size = tail - head + 1;
			float[] result = new float[size];

			Array.Copy(z, head, result, head - head, tail + 1 - head);

			return result;
		}

		#endregion

		#region Protected Methods

		protected float[] GetVariableValues(Variable[] gen)
		{
			float[] x = new float[this.NumberOfVariables];

			for (int i = 0; i < this.NumberOfVariables; i++)
			{
				x[i] = this.GetVariableValue(gen[i]);
			}
			return x;
		}

		protected float GetVariableValue(Variable variable)
		{
			float result;

			if (this.SolutionType.GetType() == typeof(BinaryRealSolutionType))
			{
				result = (float)((BinaryReal)variable).Value;
			}
			else
			{
				result = (float)((Real)variable).Value;
			}
			return result;
		}

		#endregion

		#region Abstract Methods
		/// <summary>
		/// Evaluates a solution 
		/// </summary>
		/// <param name="variables">The solution to evaluate</param>
		/// <returns>a double [] with the evaluation results</returns>
		public abstract float[] Evaluate(float[] variables);
		#endregion
	}
}
