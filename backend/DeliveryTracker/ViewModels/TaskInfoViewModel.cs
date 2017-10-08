using System;
using System.ComponentModel.DataAnnotations;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    public class TaskInfoViewModel
    {
        /// <summary>
        /// Id задания, которое пользователь берет в работу.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.IdIsRequired)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Guid Id { get; set; }
        
        /// <summary>
        /// Заголовок задания.
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// Состояние задания.
        /// </summary>
        public string TaskState { get; set; }
    }
}