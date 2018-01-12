using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Services;
using DeliveryTracker.ViewModels;

namespace DeliveryTracker.Views
{
    public class TaskViewService : IViewService<TaskViewModel>
    {
        public string GroupName => ViewGroups.TaskViewGroup;
        
        public ServiceResult<IList<TaskViewModel>> GetViewResult(UserCredentials userCredentials,
            string viewName,
            IReadOnlyDictionary<string, string[]> parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}