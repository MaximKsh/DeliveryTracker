using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public class UserViewService : IViewService<User>
    {
        public string GroupName => ViewGroups.UserViewGroup;
        
        public ServiceResult<IList<User>> GetViewResult(UserCredentials userCredentials,
            string viewName,
            IReadOnlyDictionary<string, string[]> parameters)
        {
            return null;
        }
    }
}