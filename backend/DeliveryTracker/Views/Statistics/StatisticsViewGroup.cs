using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Views.Tasks;

namespace DeliveryTracker.Views.Statistics
{
    public sealed class StatisticsViewGroup : ViewGroupBase
    {
        public StatisticsViewGroup(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
            var dict = new Dictionary<string, IView>();
            
            var performanceStatisticsView = new PerformanceStatisticsView();
            dict[performanceStatisticsView.Name] = performanceStatisticsView;
            var actualTasksStateDistributionView = new ActualTasksStateDistributionView();
            dict[actualTasksStateDistributionView.Name] = actualTasksStateDistributionView;

            this.Views = new ReadOnlyDictionary<string, IView>(dict);
        }

        public override string Name { get; } = nameof(StatisticsViewGroup);
    }
}