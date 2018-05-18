using System.Collections;

namespace DeliveryTracker.MOEA.Utils
{
	public class BitSet
	{
		#region Properties

		private BitArray bitArray { get; set; }

		public bool this[int index]
		{
			get
			{
				return this.bitArray[index];
			}
			set
			{
				this.bitArray[index] = value;
			}
		}

		public int Length
		{
			get
			{
				return this.bitArray.Length;
			}
		}

		#endregion

		#region Constructors

		public BitSet(int length)
		{
			this.bitArray = new BitArray(length);
		}

		#endregion

		#region Public Methods

		public void Clear()
		{
			for (int i = 0; i < this.bitArray.Length; i++)
			{
				this.bitArray[i] = false;
			}
		}

		public void Clear(int bitIndex)
		{
			this.bitArray[bitIndex] = false;
		}


		public void Flip(int bitIndex)
		{
			this.bitArray[bitIndex] = !this.bitArray[bitIndex];
		}

		public void Set(int bitIndex)
		{
			this.bitArray[bitIndex] = true;
		}

		public void Set(int startIndex, int endIndex)
		{
			for (int i = 0; i < this.bitArray.Length; i++)
			{
				this.bitArray[i] = true;
			}
		}

		public int Cardinality()
		{
			int res = 0;
			for (int i = 0; i < this.bitArray.Length; i++)
			{
				if (this.bitArray[i])
				{
					res += 1;
				}
			}

			return res;
		}

		#endregion
	}
}
