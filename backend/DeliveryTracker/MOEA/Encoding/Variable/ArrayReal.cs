using System;
using System.Text;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Encoding.Variable
{
	public class ArrayReal : Core.Variable
	{
		/// <summary>
		/// Problem using the type
		/// </summary>
		private Problem problem;

		/// <summary>
		/// Stores an array of real values
		/// </summary>
		public double[] Array { get; set; }

		/// <summary>
		/// Stores the length of the array
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ArrayReal()
		{
			this.problem = null;
			this.Size = 0;
			this.Array = null;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">Size of the array</param>
		/// <param name="problem"></param>
		public ArrayReal(int size, Problem problem)
		{
			this.problem = problem;
			this.Size = size;
			this.Array = new double[this.Size];

			for (int i = 0; i < this.Size; i++)
			{
				double val = JMetalRandom.NextDouble() * (this.problem.UpperLimit[i] - this.problem.LowerLimit[i]) + this.problem.LowerLimit[i];
				this.Array[i] = val;
			}
		}

		public ArrayReal(ArrayReal arrayReal)
		{
			this.problem = arrayReal.problem;
			this.Size = arrayReal.Size;
			this.Array = new double[this.Size];

			arrayReal.Array.CopyTo(this.Array, 0);
		}

		public override Core.Variable DeepCopy()
		{
			return new ArrayReal(this);
		}

		public double GetLowerBound(int index)
		{
			if ((index >= 0) && (index < this.Size))
			{
				return this.problem.LowerLimit[index];
			}
			else
			{
				IndexOutOfRangeException ex = new IndexOutOfRangeException();
				Logger.Log.Error("ArrayReal.GetLowerBound", ex);
				throw ex;
			}
		}

		public double GetUpperBound(int index)
		{
			if ((index >= 0) && (index < this.Size))
			{
				return this.problem.UpperLimit[index];
			}
			else
			{
				IndexOutOfRangeException ex = new IndexOutOfRangeException();
				Logger.Log.Error("ArrayReal.GetUpperBound", ex);
				throw ex;
			}
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			for (int i = 0; i < this.Size; i += 1)
			{
				result.Append(this.Array[i] + (i < this.Size - 1 ? " " : ""));
			}

			return result.ToString();
		}
	}
}
