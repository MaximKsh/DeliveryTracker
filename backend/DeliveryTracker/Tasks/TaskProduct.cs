using System;
using DeliveryTracker.Common;
using DeliveryTracker.References;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskProduct : ReferenceCollectionBase, IEquatable<TaskProduct>
    {
        /// <summary>
        /// Идентификатор товара.
        /// </summary>
        public Guid ProductId
        {
            get => this.Get<Guid>(nameof(this.ProductId));
            set => this.Set(nameof(this.ProductId), value);
        }
     
        /// <summary>
        /// Количество товара.
        /// </summary>
        public int Quantity
        {
            get => this.Get<int>(nameof(this.Quantity));
            set => this.Set(nameof(this.Quantity), value);
        }

        public bool Equals(
            TaskProduct other)
        {
            return this.ProductId == other.ProductId;
        }

        public override bool Equals(
            object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is TaskProduct product && this.Equals(product);
        }

        public override int GetHashCode()
        {
            return this.ProductId.GetHashCode();
        }

        public static bool operator ==(
            TaskProduct left,
            TaskProduct right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            TaskProduct left,
            TaskProduct right)
        {
            return !Equals(left, right);
        }
    }
}