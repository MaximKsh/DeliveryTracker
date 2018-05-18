using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.QualityIndicator.Utils;

namespace DeliveryTracker.MOEA.QualityIndicator
{
	public class QualityIndicator
	{
		#region Private Attributes

		private SolutionSet trueParetoFront;

		private Problem problem;
		private MetricsUtil Utils { get; set; }
		#endregion

		#region Properties

		/// <summary>
		/// Hypervolume of the true Pareto front
		/// </summary>
		public double TrueParetoFrontHypervolume { get; private set; }

		#endregion

		#region Constructos

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="problem">The problem</param>
		/// <param name="paretoFrontFile">Pareto front file</param>
		public QualityIndicator(Problem problem, string paretoFrontFile)
		{
			this.problem = problem;
			this.Utils = new MetricsUtil();
			this.trueParetoFront = this.Utils.ReadNonDominatedSolutionSet(paretoFrontFile);
			this.TrueParetoFrontHypervolume = new HyperVolume().Hypervolume(
					this.trueParetoFront.WriteObjectivesToMatrix(),
					this.trueParetoFront.WriteObjectivesToMatrix(),
					problem.NumberOfObjectives);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns the hypervolume of solution set
		/// </summary>
		/// <param name="solutionSet">Solution set</param>
		/// <returns>The value of the hypervolume indicator</returns>
		public double GetHypervolume(SolutionSet solutionSet)
		{
			return new HyperVolume().Hypervolume(solutionSet.WriteObjectivesToMatrix(),
					this.trueParetoFront.WriteObjectivesToMatrix(),
					this.problem.NumberOfObjectives);
		}

		/// <summary>
		/// Returns the inverted generational distance of solution set
		/// </summary>
		/// <param name="solutionSet">Solution set</param>
		/// <returns>The value of the hypervolume indicator</returns>
		public double GetIGD(SolutionSet solutionSet)
		{
			return new InvertedGenerationalDistance().CalculateInvertedGenerationalDistance(
				solutionSet.WriteObjectivesToMatrix(),
				this.trueParetoFront.WriteObjectivesToMatrix(),
				this.problem.NumberOfObjectives);
		}

		/// <summary>
		/// Returns the generational distance of solution set
		/// </summary>
		/// <param name="solutionSet">Solution set</param>
		/// <returns>The value of the hypervolume indicator</returns>
		public double GetGD(SolutionSet solutionSet)
		{
			return new GenerationalDistance().CalculateGenerationalDistance(
				solutionSet.WriteObjectivesToMatrix(),
				this.trueParetoFront.WriteObjectivesToMatrix(),
				this.problem.NumberOfObjectives);
		}

		/// <summary>
		/// Returns the spread of solution set
		/// </summary>
		/// <param name="solutionSet">Solution set</param>
		/// <returns>The value of the hypervolume indicator</returns>
		public double GetSpread(SolutionSet solutionSet)
		{
			return new Spread().CalculateSpread(solutionSet.WriteObjectivesToMatrix(),
				this.trueParetoFront.WriteObjectivesToMatrix(),
				this.problem.NumberOfObjectives);
		}


		/// <summary>
		/// Returns the epsilon indicator of solution set
		/// </summary>
		/// <param name="solutionSet">Solution set</param>
		/// <returns>The value of the hypervolume indicator</returns>
		public double GetEpsilon(SolutionSet solutionSet)
		{
			return new Epsilon().CalcualteEpsilon(solutionSet.WriteObjectivesToMatrix(),
					this.trueParetoFront.WriteObjectivesToMatrix(),
					this.problem.NumberOfObjectives);
		}

		#endregion
	}
}
