using System;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskState 
    {
        public TaskState(
            Guid id,
            string name,
            string caption)
        {
            this.Id = id;
            this.Name = name;
            this.Caption = caption;
        }
        
        /// <summary>
        /// Идентификатор состояния задания.
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Название состояния задания.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Локализационная строка состояния задания.
        /// </summary>
        public string Caption { get; }
        
    }
}