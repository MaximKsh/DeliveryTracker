using System;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskState : IEquatable<TaskState>
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

        /// <inheritdoc />
        public bool Equals(
            TaskState other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            
            return this.Id.Equals(other.Id);
        }

        /// <inheritdoc />
        public override bool Equals(
            object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is TaskState state && this.Equals(state);
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Id.GetHashCode();

        public static bool operator ==(
            TaskState left,
            TaskState right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            TaskState left,
            TaskState right)
        {
            return !Equals(left, right);
        }
    }
}