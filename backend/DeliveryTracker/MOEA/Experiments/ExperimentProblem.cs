using System.Collections.Generic;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.Experiments
{
	public class ExperimentProblem
	{
		public string Alias { get; set; }
		public string ProblemName { get; set; }
		public string ParetoFront { get; set; }
		public IDictionary<string, IEnumerable<Algorithm>> AlgorithmDictionary { get; set; }

		public ExperimentProblem()
		{
			this.AlgorithmDictionary = new Dictionary<string, IEnumerable<Algorithm>>();
		}
	}
}
