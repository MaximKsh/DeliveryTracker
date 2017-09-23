using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.Models
{
    /// <summary>
    /// Модель состояния задания
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TaskStateModel
    {
        /// <summary>
        /// ID состояния.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Уникальный псевдоним состояния.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Отображаемое значение.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Список заданий в состоянии.
        /// </summary>
        public virtual ICollection<TaskModel> Tasks { get; set; }

    }
}