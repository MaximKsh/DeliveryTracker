using System.Collections.Generic;

namespace DeliveryTracker.Notifications
{
    public sealed class Notification : INotification
    {
        public IList<INotificationComponent> Components { get; } = new List<INotificationComponent>();
    }
}