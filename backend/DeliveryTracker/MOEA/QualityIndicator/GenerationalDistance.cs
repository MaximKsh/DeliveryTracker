using System;
using DeliveryTracker.MOEA.QualityIndicator.Utils;

namespace DeliveryTracker.MOEA.QualityIndicator
{
	/// <summary>
	/// This class implements the generational distance indicator.
	/// Reference: Van Veldhuizen, D.A., Lamont, G.B.: Multiobjective Evolutionary 
	///            Algorithm Research: A History and Analysis. 
	///            Technical Report TR-98-03, Dept. Elec. Comput. Eng., Air Force 
	///            Inst. Technol. (1998)
	/// </summary>
	public class GenerationalDistance
	{
		#region Private Attributes

		/// <summary>
		/// Is used to access to the MetricsUtil funcionalities
		/// </summary>
		private MetricsUtil utils;

		/// <summary>
		/// This is the pow used for the distances
		/// </summary>
		static readonly double pow = 2.0;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// Creates a new instance of the generational distance metric. 
		/// </summary>
		public GenerationalDistance()
		{
			this.utils = new MetricsUtil();
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Returns the generational distance value for a given front
		/// </summary>
		/// <param name="front">The front </param>
		/// <param name="trueParetoFront">The true pareto front</param>
		/// <param name="numberOfObjectives"></param>
		/// <returns></returns>
		public double CalculateGenerationalDistance(double[][] front, double[][] trueParetoFront, int numberOfObjectives)
		{
			// Stores the maximum values of true pareto front.
			double[] maximumValue;

			// Stores the minimum values of the true pareto front.
			double[] minimumValue;

			// Stores the normalized front.
			double[][] normalizedFront;

			// Stores the normalized true Pareto front.
			double[][] normalizedParetoFront;

			// STEP 1. Obtain the maximum and minimum values of the Pareto front
			maximumValue = this.utils.GetMaximumValues(trueParetoFront, numberOfObjectives);
			minimumValue = this.utils.GetMinimumValues(trueParetoFront, numberOfObjectives);

			// STEP 2. Get the normalized front and true Pareto fronts
			normalizedFront = this.utils.GetNormalizedFront(front, maximumValue, minimumValue);

			normalizedParetoFront = this.utils.GetNormalizedFront(trueParetoFront, maximumValue, minimumValue);

			// STEP 3. Sum the distances between each point of the front and the 
			// nearest point in the true Pareto front
			double sum = 0.0;
			for (int i = 0; i < front.Length; i++)
				sum += Math.Pow(this.utils.DistanceToClosedPoint(normalizedFront[i], normalizedParetoFront), pow);


			// STEP 4. Obtain the sqrt of the sum
			sum = Math.Pow(sum, 1.0 / pow);

			// STEP 5. Divide the sum by the maximum number of points of the front
			double generationalDistance = sum / normalizedFront.Length;

			return generationalDistance;
		}

		#endregion
	}
}
