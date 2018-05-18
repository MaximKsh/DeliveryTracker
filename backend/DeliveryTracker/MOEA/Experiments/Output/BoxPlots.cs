using System.Collections.Generic;

namespace DeliveryTracker.MOEA.Experiments.Output
{
	public class BoxPlots
	{
		private Experiment experiment;
		private IDictionary<string, IDictionary<string, IDictionary<string, List<double>>>> indicators;

		public BoxPlots(Experiment experiment, IDictionary<string, IDictionary<string, IDictionary<string, List<double>>>> indicators)
		{
			this.experiment = experiment;
			this.indicators = indicators;
		}

		public IDictionary<string, IDictionary<string, IDictionary<string, BoxPlotData>>> Generate()
		{
			var result = new Dictionary<string, IDictionary<string, IDictionary<string, BoxPlotData>>>();
			foreach (var indicator in this.indicators)
			{
				result.Add(indicator.Key, this.CalculateBoxPlot(indicator));
			}

			return result;
		}

		private IDictionary<string, IDictionary<string, BoxPlotData>> CalculateBoxPlot(KeyValuePair<string, IDictionary<string, IDictionary<string, List<double>>>> indicator)
		{
			var result = new Dictionary<string, IDictionary<string, BoxPlotData>>();

			foreach (var problem in indicator.Value)
			{
				var algorithmDictionary = new Dictionary<string, BoxPlotData>();

				result.Add(problem.Key, algorithmDictionary);

				foreach (var algorithm in problem.Value)
				{
					List<double> values = new List<double>(algorithm.Value);
					var boxPlotData = new BoxPlotData();

					values.Sort();

					boxPlotData.Median = Utils.Statistics.CalculateMedian(values);

					boxPlotData.Q1 = Utils.Statistics.CalculateQ1(values);
					boxPlotData.Q3 = Utils.Statistics.CalculateQ3(values);

					boxPlotData.Low = Utils.Statistics.GetBoxPlotLowValue(boxPlotData.Q1, boxPlotData.Q3, values);
					boxPlotData.Top = Utils.Statistics.GetBoxPlotTopValue(boxPlotData.Q1, boxPlotData.Q3, values);

					boxPlotData.Outliers = Utils.Statistics.GetOutlidersValues(boxPlotData.Q1, boxPlotData.Q3, values);

					algorithmDictionary.Add(algorithm.Key, boxPlotData);
				}
			}

			return result;
		}
	}
}
