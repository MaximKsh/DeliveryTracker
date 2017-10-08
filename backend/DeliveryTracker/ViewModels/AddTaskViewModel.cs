using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AddTaskViewModel
    {
        /// <summary>
        /// Заголовок задания.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.CaptionIsRequired)]
        public string Caption { get; set; }
        
        /// <summary>
        /// Описание(содержимое) задания.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.ContentIsRequired)]
        public string Content { get; set; }
        
        /// <summary>
        /// Дедлайн, после которого задание будет считаться просроченным
        /// </summary>
        public DateTime? DeadlineDate { get; set; }
        
        /// <summary>
        /// На кого назначить задание.
        /// </summary>
        public string PerformerUserName { get; set; }
    }
}