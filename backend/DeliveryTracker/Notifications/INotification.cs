using System.Collections.Generic;

namespace DeliveryTracker.Notifications
{
    public interface INotification
    {
        IList<INotificationComponent> Components { get; }
    }
}