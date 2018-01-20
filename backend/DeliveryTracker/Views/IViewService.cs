using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public interface IViewService<T>
    {
        string GroupName { get; }

        ServiceResult<IList<T>> GetViewResult(
            UserCredentials userCredentials,
            string viewName,
            IReadOnlyDictionary<string, string[]> parameters);

    }
}