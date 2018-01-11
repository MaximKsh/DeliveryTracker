using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Instances;
using DeliveryTracker.Services;
using DeliveryTracker.ViewModels;

namespace DeliveryTracker.Views
{
    public class UserViewService : IViewService<UserViewModel>
    {
        public string GroupName => ViewGroups.UserViewGroup;
        
        public ServiceResult<IList<UserViewModel>> GetViewResult(UserCredentials userCredentials,
            string viewName,
            IReadOnlyDictionary<string, string[]> parameters)
        {
            return null;
        }
    }
}