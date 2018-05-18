using System;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Encoding.Variable
{
	public class BinaryReal : Binary
	{
		/// <summary>
		/// Defines the default number of bits used for binary coded variables.
		/// </summary>
		public static int DEFAULT_PRECISION = 30;

		/// <summary>
		/// Stores the real value of the encodings.variable
		/// </summary>
		public new double Value { get; private set; }

		/// <summary>
		/// Stores the real value of the encodings.variable
		/// </summary>
		public new double LowerBound { get; set; }

		/// <summary>
		/// Stores the upper limit for the encodings.variable
		/// </summary>
		public new double UpperBound { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public BinaryReal()
			: base()
		{

		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="numberOfBits">Length of the binary string</param>
		/// <param name="lowerBound">The lower limit for the encodings.variable</param>
		/// <param name="upperBound">The upper limit for the encodings.variable</param>
		public BinaryReal(int numberOfBits, double lowerBound, double upperBound)
			: base(numberOfBits)
		{
			this.LowerBound = lowerBound;
			this.UpperBound = upperBound;

			this.Decode();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="bits">BitSet</param>
		/// <param name="nbBits">Number of bits</param>
		/// <param name="lowerBound">Lower bound</param>
		/// <param name="upperBound">Upper bound</param>
		public BinaryReal(BitSet bits, int nbBits, double lowerBound, double upperBound)
			: base(nbBits)
		{
			this.Bits = bits;
			this.LowerBound = lowerBound;
			this.UpperBound = upperBound;
			this.Decode();
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="variable">The encoding.variable to copy</param>
		public BinaryReal(BinaryReal variable)
			: base(variable)
		{
			this.LowerBound = variable.LowerBound;
			this.UpperBound = variable.UpperBound;

			this.Value = variable.Value;
		}

		/// <summary>
		/// Decodes the real value encoded in the binary string represented 
		/// by the <code>BinaryReal</code> object. The decoded value is stores in the 
		/// </summary>
		public new void Decode()
		{
			double value = 0.0;
			for (int i = 0; i < this.NumberOfBits; i++)
			{
				if (this.Bits[i])
				{
					value += Math.Pow(2, i);
				}
			}

			this.Value = value * (this.UpperBound - this.LowerBound) / (Math.Pow(2, this.NumberOfBits) - 1);
			this.Value += this.LowerBound;
		}

		/// <summary>
		/// This implementation is efficient for binary string of length up to 24
		/// bits, and for positive intervals.
		/// </summary>
		/// <param name="value"></param>
		public void SetValue(double value)
		{
			if (this.NumberOfBits <= 24 && this.LowerBound >= 0)
			{
				BitSet bitSet = new BitSet(this.NumberOfBits);
				if (this.Value <= this.LowerBound)
				{
					bitSet.Clear();
				}
				else if (value >= this.UpperBound)
				{
					bitSet.Set(0, this.NumberOfBits);
				}
				else
				{
					bitSet.Clear();


					int integerToDecode = 0;
					double tmp = this.LowerBound;
					double path = (this.UpperBound - this.LowerBound) / (Math.Pow(2, this.NumberOfBits) - 1);

					while (tmp < value)
					{
						tmp += path;
						integerToDecode++;
					}

					int remain = integerToDecode;

					for (int i = this.NumberOfBits - 1; i >= 0; i--)
					{
						int ithPowerOf2 = (int)Math.Pow(2, i);
						if (ithPowerOf2 <= remain)
						{
							bitSet.Set(i);
							remain -= ithPowerOf2;
						}
						else
						{
							bitSet.Clear(i);
						}

					}
				}
				this.Bits = bitSet;
				this.Decode();
			}
			else
			{
				if (this.LowerBound < 0)
				{
					throw new Exception("Unsupported lowerbound: " + this.LowerBound);
				}
				if (this.NumberOfBits >= 24)
				{
					throw new Exception("Unsupported bit string length " + this.NumberOfBits + " is > 24 bits");
				}
			}
		}

		/// <summary>
		/// Creates an exact copy of a <code>BinaryReal</code> object
		/// </summary>
		/// <returns></returns>
		public override Core.Variable DeepCopy()
		{
			return new BinaryReal(this);
		}

		/// <summary>
		/// Returns a string representing the object.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Value.ToString();
		}
	}
}
