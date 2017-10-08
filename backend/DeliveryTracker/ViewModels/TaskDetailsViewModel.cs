using System;

namespace DeliveryTracker.ViewModels
{
    public class TaskDetailsViewModel
    {
        /// <summary>
        /// ID задания.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Состояние задания.
        /// </summary>
        public string TaskState { get; set; }
        
        /// <summary>
        /// Заголовок задания.
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// Содержимое задания.
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Имя(уникальное) отправителя задания.
        /// </summary>
        public string SenderUserName { get; set; }
        
        /// <summary>
        /// Отображаемое имя отправителя.
        /// </summary>
        public string SenderDisplayableName { get; set; }
        
        /// <summary>
        /// Имя(уникальное) отправителя исполнителя.
        /// </summary>
        public string PerformerUserName { get; set; }
        
        /// <summary>
        /// Отображаемое имя отправителя.
        /// </summary>
        public string PerformerDisplayableName { get; set; } 
        
        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Дедлайн задания.
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Дата взятия в работу.
        /// </summary>
        public DateTime? InWorkDate { get; set; }
        
        /// <summary>
        /// Дата завершения.
        /// </summary>
        public DateTime? CompletionDate { get; set; }
    }
}