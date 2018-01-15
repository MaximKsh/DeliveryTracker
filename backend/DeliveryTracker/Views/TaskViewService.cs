using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public class TaskViewService : IViewService<Task>
    {
        public string GroupName => ViewGroups.TaskViewGroup;
        
        public ServiceResult<IList<Task>> GetViewResult(UserCredentials userCredentials,
            string viewName,
            IReadOnlyDictionary<string, string[]> parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}