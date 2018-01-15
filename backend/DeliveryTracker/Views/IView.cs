using System.Collections.Generic;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public interface IView<T>
    {
        string Name { get; }
        
        IList<T> GetViewResult(
            UserCredentials userCredentials,
            Dictionary<string, string> parameters);
    }
}