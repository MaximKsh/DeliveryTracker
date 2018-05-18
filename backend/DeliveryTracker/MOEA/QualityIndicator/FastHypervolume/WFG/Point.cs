using System;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.QualityIndicator.FastHypervolume.WFG
{
	public class Point
	{
		public double[] Objectives { get; private set; }

		public Point(int dimension)
		{
			this.Objectives = new double[dimension];

			for (int i = 0; i < dimension; i++)
			{
				this.Objectives[i] = 0.0;
			}
		}

		public Point(Solution solution)
		{
			int dimension = solution.NumberOfObjectives;
			this.Objectives = new double[dimension];

			for (int i = 0; i < dimension; i++)
			{
				this.Objectives[i] = solution.Objective[i];
			}
		}

		public Point(double[] points)
		{
			this.Objectives = new double[points.Length];
			Array.Copy(points, this.Objectives, points.Length);
		}

		public int GetNumberOfObjectives()
		{
			return this.Objectives.Length;
		}

		public override string ToString()
		{
			string result = "";
			for (int i = 0; i < this.Objectives.Length; i++)
				result += this.Objectives[i] + " ";

			return result;
		}
	}

}
