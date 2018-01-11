using System;

namespace DeliveryTracker.DbModels
{
    /// <summary>
    /// Модель устройства пользователя.
    /// </summary>
    public class DeviceModel
    {
        /// <summary>
        /// ID устройства.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id устройства в Google Firebase.
        /// </summary>
        public string FirebaseId { get; set; }
        
        /// <summary>
        /// Идентификатор пользователя устройства.
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Пользователь устройства.
        /// </summary>
        public UserModel User { get; set; }
    }
}