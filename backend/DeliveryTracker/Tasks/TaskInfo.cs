using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskInfo : DictionaryObject
    {
        /// <summary>
        /// Идентификатор задания.
        /// </summary>
        public Guid Id   
        {
            get => this.Get<Guid>(nameof(this.Id));
            set => this.Set(nameof(this.Id), value);
        }
        
        /// <summary>
        /// Идентификатор инстанса, в рамках которого существует задание.
        /// </summary>
        public Guid InstanceId
        {
            get => this.Get<Guid>(nameof(this.InstanceId));
            set => this.Set(nameof(this.InstanceId), value);
        }

        /// <summary>
        /// Id состояния задания.
        /// </summary>
        public Guid TaskStateId
        {
            get => this.Get<Guid>(nameof(this.TaskStateId));
            set => this.Set(nameof(this.TaskStateId), value);
        }
        
        /// <summary>
        /// Название состояния задания.
        /// </summary>
        public string TaskStateName 
        {
            get => this.Get<string>(nameof(this.TaskStateName));
            set => this.Set(nameof(this.TaskStateName), value);
        }
        
        /// <summary>
        /// Локализационная строка состояния задания.
        /// </summary>
        public string TaskStateCaption 
        {
            get => this.Get<string>(nameof(this.TaskStateCaption));
            set => this.Set(nameof(this.TaskStateCaption), value);
        }
        
        /// <summary>
        /// Идентификатор автора.
        /// </summary>
        public Guid AuthorId
        {
            get => this.Get<Guid>(nameof(this.AuthorId));
            set => this.Set(nameof(this.AuthorId), value);
        }
        
        /// <summary>
        /// Идентификатор исполнителя.
        /// </summary>
        public Guid? PerformerId
        {
            get => this.Get<Guid?>(nameof(this.PerformerId));
            set => this.Set(nameof(this.PerformerId), value);
        }
        
        /// <summary>
        /// Номер задания.
        /// </summary>
        public string TaskNumber
        {
            get => this.Get<string>(nameof(this.TaskNumber));
            set => this.Set(nameof(this.TaskNumber), value);
        }
        
        /// <summary>
        /// Дата создания задания.
        /// </summary>
        public DateTime? Created
        {
            get => this.Get<DateTime?>(nameof(this.Created));
            set => this.Set(nameof(this.Created), value);
        }
        
        /// <summary>
        /// Дата последней смены состояния.
        /// </summary>
        public DateTime? StateChangedLastTime
        {
            get => this.Get<DateTime?>(nameof(this.StateChangedLastTime));
            set => this.Set(nameof(this.StateChangedLastTime), value);
        }
        
        /// <summary>
        /// Дата получения товара, указанная в задании.
        /// </summary>
        public DateTime? Receipt
        {
            get => this.Get<DateTime?>(nameof(this.Receipt));
            set => this.Set(nameof(this.Receipt), value);
        }
        
        /// <summary>
        /// Актуальная дата получения товараю
        /// </summary>
        public DateTime? ReceiptActual
        {
            get => this.Get<DateTime?>(nameof(this.ReceiptActual));
            set => this.Set(nameof(this.ReceiptActual), value);
        }
        
        /// <summary>
        /// Дата доставки, начиная с.
        /// </summary>
        public DateTime? DeliveryFrom
        {
            get => this.Get<DateTime?>(nameof(this.DeliveryFrom));
            set => this.Set(nameof(this.DeliveryFrom), value);
        }
        
        /// <summary>
        /// Дата доставки, по
        /// </summary>
        public DateTime? DeliveryTo
        {
            get => this.Get<DateTime?>(nameof(this.DeliveryTo));
            set => this.Set(nameof(this.DeliveryTo), value);
        }
        
        /// <summary>
        /// Фактическая дата доставки.
        /// </summary>
        public DateTime? DeliveryActual
        {
            get => this.Get<DateTime?>(nameof(this.DeliveryActual));
            set => this.Set(nameof(this.DeliveryActual), value);
        }

        /// <summary>
        /// Комментарий к заданию.
        /// </summary>
        public string Comment
        {
            get => this.Get<string>(nameof(this.Comment));
            set => this.Set(nameof(this.Comment), value);
        }
        
        /// <summary>
        /// Идентификатор склада, откуда забирать заказ.
        /// </summary>
        public Guid? WarehouseId
        {
            get => this.Get<Guid?>(nameof(this.WarehouseId));
            set => this.Set(nameof(this.WarehouseId), value);
        }
        
        /// <summary>
        /// Идентификатор клиента.
        /// </summary>
        public Guid? ClientId
        {
            get => this.Get<Guid?>(nameof(this.ClientId));
            set => this.Set(nameof(this.ClientId), value);
        }
        
        /// <summary>
        /// Идентификатор адреса клиента.
        /// </summary>
        public Guid? ClientAddressId
        {
            get => this.Get<Guid?>(nameof(this.ClientAddressId));
            set => this.Set(nameof(this.ClientAddressId), value);
        }
    
        /// <summary>
        /// Идентификатор типа оплаты.
        /// </summary>
        public Guid? PaymentTypeId
        {
            get => this.Get<Guid?>(nameof(this.PaymentTypeId));
            set => this.Set(nameof(this.PaymentTypeId), value);
        }
        
        /// <summary>
        /// Стоимость товаров. 
        /// </summary>
        public decimal? Cost
        {
            get => this.Get<decimal?>(nameof(this.Cost));
            set => this.Set(nameof(this.Cost), value);
        }
    
        /// <summary>
        /// Стоимость дсоставки.
        /// </summary>
        public decimal? DeliveryCost
        {
            get => this.Get<decimal?>(nameof(this.DeliveryCost));
            set => this.Set(nameof(this.DeliveryCost), value);
        }
    }
}