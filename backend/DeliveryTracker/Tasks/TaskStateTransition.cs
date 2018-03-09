using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskStateTransition : DictionaryObject
    {
        /// <summary>
        /// Идентификатор перехода.
        /// </summary>
        public Guid Id
        {
            get => this.Get<Guid>(nameof(this.Id));
            set => this.Set(nameof(this.Id), value);
        }
        
        /// <summary>
        /// Роль, которой доступен переход.
        /// </summary>
        public Guid Role 
        {
            get => this.Get<Guid>(nameof(this.Role));
            set => this.Set(nameof(this.Role), value);
        }
        
        /// <summary>
        /// Начальное состояние задания.
        /// </summary>
        public Guid InitialState
        {
            get => this.Get<Guid>(nameof(this.InitialState));
            set => this.Set(nameof(this.InitialState), value);
        }

        /// <summary>
        /// Конечное состояние задания.
        /// </summary>
        public Guid FinalState 
        {
            get => this.Get<Guid>(nameof(this.FinalState));
            set => this.Set(nameof(this.FinalState), value);
        }
        
        /// <summary>
        /// Надпись на кнопке.
        /// </summary>
        public string ButtonCaption 
        {
            get => this.Get<string>(nameof(this.ButtonCaption));
            set => this.Set(nameof(this.ButtonCaption), value);
        }
        
    }
}