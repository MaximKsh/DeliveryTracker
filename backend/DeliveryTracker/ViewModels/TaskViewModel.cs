using System;

namespace DeliveryTracker.ViewModels
{
    public class TaskViewModel
    {
        /// <summary>
        /// ID задания.
        /// </summary>
        public Guid? Id { get; set; }

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
        /// Состояние задания.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Инстанс, в рамках которой существует таск. 
        /// </summary>
        public InstanceViewModel Instance { get; set; }
        
        /// <summary>
        /// Автор задания. 
        /// </summary>
        public UserViewModel Author { get; set; }

        /// <summary>
        /// Исполнитель задания. 
        /// </summary>
        public UserViewModel Performer { get; set; }
        
        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Временной промежуток, за который необходимо выполнить задание
        /// </summary>
        public DateTimeRangeViewModel TaskDateTimeRange { get; set; }
        
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