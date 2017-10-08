using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace DeliveryTracker.Models
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UserModel : IdentityUser<Guid>
    {
        /// <summary>
        /// Отображаемое имя пользователя.
        /// </summary>
        public string DisplayableName { get; set; }
        
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
        /// ID группы пользователя.
        /// </summary>
        public Guid GroupId { get; set; }

        /// <summary>
        /// Группа пользователя.
        /// </summary>
        public GroupModel Group { get; set;}

        /// <summary>
        /// Группа, созданная пользователем.
        /// </summary>
        public virtual GroupModel CreatedGroup { get; set; }
        
        /// <summary>
        /// Задания, выданные пользователем.
        /// </summary>
        public virtual ICollection<TaskModel> SentTasks { get; set; } 

        /// <summary>
        /// Задания, выполняемые пользователем.
        /// </summary>
        public virtual ICollection<TaskModel> PerformingTasks { get; set; }
    }
}