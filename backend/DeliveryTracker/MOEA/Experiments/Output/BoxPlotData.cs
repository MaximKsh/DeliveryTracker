using System.Collections.Generic;

namespace DeliveryTracker.MOEA.Experiments.Output
{
	public class BoxPlotData
	{
		public double Q1 { get; set; }
		public double Median { get; set; }
		public double Q3 { get; set; }
		public double Low { get; set; }
		public double Top { get; set; }
		public IEnumerable<double> Outliers { get; set; }
	}
}
