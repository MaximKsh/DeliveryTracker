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
        /// Номер задания
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Описание задания.
        /// </summary>
        public string Details { get; set; }
        
        /// <summary>
        /// Предмет доставки.
        /// </summary>
        public string ShippingDesc { get; set; }
        
        /// <summary>
        /// Адрес доставки.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Идентификатор состояния задания.
        /// </summary>
        public Guid StateId { get; set; }

        /// <summary>
        /// Состояние задания.
        /// </summary>
        public virtual TaskStateModel State { get; set; }

        /// <summary>
        /// ID инстанса, в рамках которой существует таск.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Инстанс, в рамках которой существует таск. 
        /// </summary>
        public virtual InstanceModel Instance { get; set; }

        /// <summary>
        /// ID отправителя задания.
        /// </summary>
        public Guid AuthorId { get; set; }

        /// <summary>
        /// Отправитель. 
        /// </summary>
        public virtual UserModel Author { get; set; }

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
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Нижняя граница времени, когда нужно выполнить задание.
        /// </summary>
        public DateTime? DatetimeFrom { get; set; }
        
        /// <summary>
        /// Верхняя граница задания, когда нужно выполнить задание.
        /// </summary>
        public DateTime? DatetimeTo { get; set; }
        
        /// <summary>
        /// Дата указания исполнителя.
        /// </summary>
        public DateTime? SetPerformerDate { get; set; }
        
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