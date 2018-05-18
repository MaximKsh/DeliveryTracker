using System;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.WFG
{
	/// <summary>
	/// This class implements the WFG1 problem
	/// Reference: Simon Huband, Luigi Barone, Lyndon While, Phil Hingston
	///            A Scalable Multi-objective Test Problem Toolkit.
	///            Evolutionary Multi-Criterion Optimization: 
	///            Third International Conference, EMO 2005. 
	///            Proceedings, volume 3410 of Lecture Notes in Computer Science
	/// </summary>
	public class WFG1 : WFG
	{
		#region Constructors
		/// <summary>
		/// Constructor
		/// Creates a default WFG1 instance with 
		/// 2 position-related parameters
		/// 4 distance-related parameters 
		/// and 2 objectives
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		public WFG1(string solutionType) :
			this(solutionType, 2, 4, 2)
		{

		}

		/// <summary>
		/// Creates a WFG1 problem instance
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		/// <param name="k">Number of position parameters</param>
		/// <param name="l">Number of distance parameters</param>
		/// <param name="M">Number of objective functions</param>
		public WFG1(string solutionType, int k, int l, int M)
			: base(solutionType, k, l, M)
		{

			this.ProblemName = "WFG1";

			this.S = new int[M];
			for (int i = 0; i < M; i++)
				this.S[i] = 2 * (i + 1);

			this.A = new int[M - 1];
			for (int i = 0; i < M - 1; i++)
				this.A[i] = 1;

		}

		#endregion

		#region Public Methods

		/// <summary>
		/// WFG1 t1 transformation
		/// </summary>
		/// <param name="z"></param>
		/// <param name="k"></param>
		/// <returns></returns>
		public float[] T1(float[] z, int k)
		{
			float[] result = new float[z.Length];

			Array.Copy(z, 0, result, 0, k);

			for (int i = k; i < z.Length; i++)
			{
				result[i] = Transformations.S_linear(z[i], (float)0.35);
			}

			return result;
		}

		/// <summary>
		/// WFG1 t2 transformation
		/// </summary>
		/// <param name="z"></param>
		/// <param name="k"></param>
		/// <returns></returns>
		public float[] T2(float[] z, int k)
		{
			float[] result = new float[z.Length];

			Array.Copy(z, 0, result, 0, k);

			for (int i = k; i < z.Length; i++)
			{
				result[i] = Transformations.B_flat(z[i], (float)0.8, (float)0.75, (float)0.85);
			}

			return result;
		}

		/// <summary>
		/// WFG1 t3 transformation
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public float[] T3(float[] z)
		{
			float[] result = new float[z.Length];

			for (int i = 0; i < z.Length; i++)
			{
				result[i] = Transformations.B_poly(z[i], (float)0.02);
			}

			return result;
		}

		/// <summary>
		/// WFG1 t4 transformation
		/// </summary>
		/// <param name="z"></param>
		/// <param name="k"></param>
		/// <param name="M"></param>
		/// <returns></returns>
		public float[] T4(float[] z, int k, int M)
		{
			float[] result = new float[M];
			float[] w = new float[z.Length];

			for (int i = 0; i < z.Length; i++)
			{
				w[i] = (float)2.0 * (i + 1);
			}

			for (int i = 1; i <= M - 1; i++)
			{
				int head = (i - 1) * k / (M - 1) + 1;
				int tail = i * k / (M - 1);
				float[] subZ = this.SubVector(z, head - 1, tail - 1);
				float[] subW = this.SubVector(w, head - 1, tail - 1);

				result[i - 1] = Transformations.R_sum(subZ, subW);
			}

			int h = k + 1 - 1;
			int t = z.Length - 1;
			float[] sZ = this.SubVector(z, h, t);
			float[] sW = this.SubVector(w, h, t);
			result[M - 1] = Transformations.R_sum(sZ, sW);

			return result;
		}

		#endregion

		#region Public Override

		/// <summary>
		/// Evaluates a solution
		/// </summary>
		/// <param name="z">The solution to evaluate</param>
		/// <returns>A float [] with the evaluation results</returns>
		public override float[] Evaluate(float[] z)
		{
			float[] y;

			y = this.Normalise(z);
			y = this.T1(y, this.k);
			y = this.T2(y, this.k);
			try
			{
				y = this.T3(y);
			}
			catch (Exception ex)
			{
				Logger.Log.Error(ex.Message, ex);
			}
			y = this.T4(y, this.k, this.M);


			float[] result = new float[this.M];
			float[] x = this.Calculate_x(y);
			for (int m = 1; m <= this.M - 1; m++)
			{
				result[m - 1] = this.D * x[this.M - 1] + this.S[m - 1] * Shapes.Convex(x, m);
			}

			result[this.M - 1] = this.D * x[this.M - 1] + this.S[this.M - 1] * Shapes.Mixed(x, 5, (float)1.0);

			return result;
		}

		/// <summary>
		/// Evaluates a solution
		/// </summary>
		/// <param name="solution">The solution to evaluate</param>
		public override void Evaluate(Solution solution)
		{
			float[] variables = this.GetVariableValues(solution.Variable);

			float[] f = this.Evaluate(variables);

			for (int i = 0; i < f.Length; i++)
			{
				solution.Objective[i] = f[i];
			}
		}
		#endregion
	}
}
