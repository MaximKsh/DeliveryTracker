﻿using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public class SmsNotificator : NotificatorBase<SmsNotificator.ISmsNotificationComponent>
    {
        public interface ISmsNotificationComponent : INotificationComponent
        {
            
        }


        public override ServiceResult Notify(
            ISmsNotificationComponent notification)
        {
            return new ServiceResult();
        }
    }
}