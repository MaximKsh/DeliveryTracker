using System.Collections.Generic;
using DeliveryTracker.Tasks;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    public class TaskResponse : ResponseBase
    {
        public TaskResponse() : base()
        {
        }

        public TaskResponse(IError error) : base(error)
        {
        }

        public TaskResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
        
        public TaskPackage TaskPackage { get; set; }
    }
}