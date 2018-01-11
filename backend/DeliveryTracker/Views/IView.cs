using System.Collections.Generic;
using DeliveryTracker.DbModels;
using DeliveryTracker.Instances;

namespace DeliveryTracker.Views
{
    public interface IView<T>
    {
        string Name { get; }
        
        IList<T> GetViewResult(
            DeliveryTrackerDbContext dbContext,
            UserCredentials userCredentials,
            Dictionary<string, string> parameters);
    }
}