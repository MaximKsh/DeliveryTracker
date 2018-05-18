using System.Text;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Encoding.Variable
{
	public class ArrayInt : Core.Variable
	{
		/// <summary>
		/// Problem using the type
		/// </summary>
		private Problem _problem;

		/// <summary>
		/// Stores an array of integer values
		/// </summary>
		public int[] Array { get; set; }

		/// <summary>
		/// Stores the length of the array
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		/// Store the lower bound of each int value of the array in case of
		/// having each one different limits
		/// </summary>
		public new int[] LowerBound { get; set; }

		/// <summary>
		/// Store the upper bound of each int value of the array in case of
		/// having each one different limits
		/// </summary>
		public new int[] UpperBound { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ArrayInt()
		{
			this.LowerBound = null;
			this.UpperBound = null;
			this.Size = 0;
			this.Array = null;
			this._problem = null;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">Size of the array</param>
		public ArrayInt(int size)
		{
			this.Size = size;
			this.Array = new int[size];
			this.LowerBound = new int[size];
			this.UpperBound = new int[size];
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">Size of the array</param>
		public ArrayInt(int size, Problem problem)
		{
			this._problem = problem;
			this.Size = size;
			this.Array = new int[size];
			this.LowerBound = new int[size];
			this.UpperBound = new int[size];

			for (int i = 0; i < this.Size; i++)
			{
				this.LowerBound[i] = (int)this._problem.LowerLimit[i];
				this.UpperBound[i] = (int)this._problem.UpperLimit[i];
				this.Array[i] = JMetalRandom.Next(this.LowerBound[i], this.UpperBound[i]);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">The size of the array</param>
		/// <param name="lowerBounds">Lower bounds</param>
		/// <param name="upperBounds">Upper bounds</param>
		public ArrayInt(int size, double[] lowerBounds, double[] upperBounds)
		{
			this.Size = size;
			this.Array = new int[this.Size];
			this.LowerBound = new int[this.Size];
			this.UpperBound = new int[this.Size];

			for (int i = 0; i < this.Size; i++)
			{
				this.LowerBound[i] = (int)lowerBounds[i];
				this.UpperBound[i] = (int)upperBounds[i];
				this.Array[i] = JMetalRandom.Next(this.LowerBound[i], this.UpperBound[i]);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="arrayInt">The ArrayInt to copy</param>
		private ArrayInt(ArrayInt arrayInt)
		{
			this.Size = arrayInt.Size;
			this.Array = new int[this.Size];
			this.LowerBound = new int[this.Size];
			this.UpperBound = new int[this.Size];

			for (int i = 0; i < this.Size; i++)
			{
				this.Array[i] = arrayInt.Array[i];
				this.LowerBound[i] = arrayInt.LowerBound[i];
				this.UpperBound[i] = arrayInt.UpperBound[i];
			}
		}

		public override Core.Variable DeepCopy()
		{
			return new ArrayInt(this);
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
