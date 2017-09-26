using System;

namespace DeliveryTracker.ViewModels
{
    public class AddTaskViewModel
    {
        /// <summary>
        /// Заголовок задания.
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// Описание(содержимое) задания.
        /// </summary>
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