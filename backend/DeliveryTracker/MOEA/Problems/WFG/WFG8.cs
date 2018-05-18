using System;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Problems.WFG
{
	public class WFG8 : WFG
	{
		#region Constructors
		/// <summary>
		/// Creates a default WFG8 with 
		/// 2 position-related parameters, 
		/// 4 distance-related parameters,
		/// and 2 objectives
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		public WFG8(string solutionType)
			: this(solutionType, 2, 4, 2)
		{
		}

		/// <summary>
		/// Creates a WFG8 problem instance
		/// </summary>
		/// <param name="solutionType">The solution type must "Real" or "BinaryReal".</param>
		/// <param name="k">Number of position parameters</param>
		/// <param name="l">Number of distance parameters</param>
		/// <param name="M">Number of objective functions</param>
		public WFG8(string solutionType, int k, int l, int M)
			: base(solutionType, k, l, M)
		{
			this.ProblemName = "WFG8";

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
		/// WFG8 t1 transformation
		/// </summary>
		/// <param name="z"></param>
		/// <param name="k"></param>
		/// <returns></returns>
		public float[] T1(float[] z, int k)
		{
			float[] result = new float[z.Length];
			float[] w = new float[z.Length];

			for (int i = 0; i < w.Length; i++)
			{
				w[i] = (float)1.0;
			}

			Array.Copy(z, 0, result, 0, k);

			for (int i = k; i < z.Length; i++)
			{
				int head = 0;
				int tail = i - 1;
				float[] subZ = this.SubVector(z, head, tail);
				float[] subW = this.SubVector(w, head, tail);
				float aux = Transformations.R_sum(subZ, subW);

				result[i] = Transformations.B_param(z[i], aux, (float)0.98 / (float)49.98, (float)0.02, 50);
			}

			return result;
		}

		/// <summary>
		/// WFG8 t2 transformation
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
				result[i] = Transformations.S_linear(z[i], (float)0.35);
			}

			return result;
		}

		/// <summary>
		/// WFG8 t3 transformation
		/// </summary>
		/// <param name="z"></param>
		/// <param name="k"></param>
		/// <param name="M"></param>
		/// <returns></returns>
		public float[] T3(float[] z, int k, int M)
		{
			float[] result = new float[M];
			float[] w = new float[z.Length];

			for (int i = 0; i < z.Length; i++)
			{
				w[i] = (float)1.0;
			}

			for (int i = 1; i <= M - 1; i++)
			{
				int head = (i - 1) * k / (M - 1) + 1;
				int tail = i * k / (M - 1);
				float[] subZ = this.SubVector(z, head - 1, tail - 1);
				float[] subW = this.SubVector(w, head - 1, tail - 1);

				result[i - 1] = Transformations.R_sum(subZ, subW);
			}

			int h = k + 1;
			int t = z.Length;
			float[] sZ = this.SubVector(z, h - 1, t - 1);
			float[] sW = this.SubVector(w, h - 1, t - 1);
			result[M - 1] = Transformations.R_sum(sZ, sW);

			return result;
		}

		#endregion

		#region Public Overrides

		/// <summary>
		/// Evaluates a solution 
		/// </summary>
		/// <param name="z">The solution to evaluate</param>
		/// <returns>float[] with the evaluation results</returns>
		public override float[] Evaluate(float[] z)
		{
			float[] y;

			y = this.Normalise(z);
			y = this.T1(y, this.k);
			y = this.T2(y, this.k);
			y = this.T3(y, this.k, this.M);

			float[] result = new float[this.M];
			float[] x = this.Calculate_x(y);
			for (int m = 1; m <= this.M; m++)
			{
				result[m - 1] = this.D * x[this.M - 1] + this.S[m - 1] * Shapes.Concave(x, m);
			}

			return result;
		}

		/// <summary>
		/// Evaluates a solution 
		/// </summary>
		/// <param name="solution">The solution to evaluate</param>
		public override void Evaluate(Solution solution)
		{
			float[] variables = this.GetVariableValues(solution.Variable);

			float[] sol = this.Evaluate(variables);

			for (int i = 0; i < sol.Length; i++)
			{
				solution.Objective[i] = sol[i];
			}
		}
		#endregion
	}
}
