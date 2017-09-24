using System;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.Models
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TaskModel
    {
        /// <summary>
        /// ID задания.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Заголовок задания.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Описание задания.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Идентификатор состояния задания.
        /// </summary>
        public Guid StateId { get; set; }

        /// <summary>
        /// Состояние задания.
        /// </summary>
        public virtual TaskStateModel State { get; set; }

        /// <summary>
        /// ID отправителя задания.
        /// </summary>
        public Guid SenderId { get; set; }

        /// <summary>
        /// Отправитель. 
        /// </summary>
        public virtual UserModel Sender { get; set; }

        /// <summary>
        /// ID исполнителя. 
        /// </summary>
        public Guid? PerformerId { get; set; }
        
        /// <summary>
        /// Исполнитель.
        /// </summary>
        public virtual UserModel Performer { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        /// <returns></returns>
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