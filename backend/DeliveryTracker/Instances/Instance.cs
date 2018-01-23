using System;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Common;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Instance : DictionaryObject
    {
        /// <summary>
        /// Id группы.
        /// </summary>
        public Guid Id 
        {
            get => this.Get<Guid>(nameof(this.Id));
            set => this.Set(nameof(this.Id), value);
        }
        
        /// <summary>
        /// Имя группы.
        /// </summary>
        public string Name
        {
            get => this.Get<string>(nameof(this.Name));
            set => this.Set(nameof(this.Name), value);
        }
        
        /// <summary>
        /// Имя группы.
        /// </summary>
        public Guid CreatorId 
        {
            get => this.Get<Guid>(nameof(this.CreatorId));
            set => this.Set(nameof(this.CreatorId), value);
        }
    }
}