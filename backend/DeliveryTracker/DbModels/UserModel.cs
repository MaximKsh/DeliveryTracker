using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace DeliveryTracker.DbModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UserModel : IdentityUser<Guid>
    {
        /// <summary>
        /// Фамилия.
        /// </summary>
        public string Surname { get; set; }
        
        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Долгота.
        /// </summary>
        public double? Longitude { get; set; }
        
        /// <summary>
        /// Широта.
        /// </summary>
        public double? Latitude { get; set; }
        
        /// <summary>
        /// Последняя дата обновления положения.
        /// </summary>
        public DateTime LastTimePositionUpdated { get; set; }
        
        /// <summary>
        /// Пользователь удален.
        /// </summary>
        public bool Deleted { get; set; }
        
        /// <summary>
        /// ID группы пользователя.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Группа пользователя.
        /// </summary>
        public virtual InstanceModel Instance { get; set;}

        /// <summary>
        /// Группа, созданная пользователем.
        /// </summary>
        public virtual InstanceModel CreatedInstance { get; set; }
        
        /// <summary>
        /// Задания, выданные пользователем.
        /// </summary>
        public virtual ICollection<TaskModel> SentTasks { get; set; } 

        /// <summary>
        /// Задания, выполняемые пользователем.
        /// </summary>
        public virtual ICollection<TaskModel> PerformingTasks { get; set; }
        
        /// <summary>
        /// Устройство пользователя.
        /// </summary>
        public virtual DeviceModel Device { get; set; }
    }
}