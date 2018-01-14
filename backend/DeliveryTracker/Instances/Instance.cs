using System;
using System.Collections.Generic;
using DeliveryTracker.Common;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Instance : IDictionarySerializable
    {
        public Instance(Guid id, string name, Guid creatorId)
        {
            this.Id = id;
            this.Name = name;
            this.CreatorId = creatorId;
        }
        
        /// <summary>
        /// Id группы.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Имя группы.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Имя группы.
        /// </summary>
        public Guid CreatorId { get; private set; }

        /// <inheritdoc />
        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Id)] = this.Id,
                [nameof(this.Name)] = this.Name,
                [nameof(this.CreatorId)] = this.CreatorId,
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.Id = dict.GetPlain<Guid>(nameof(this.Id));
            this.Name = dict.GetPlain<string>(nameof(this.Name));
            this.CreatorId = dict.GetPlain<Guid>(nameof(this.CreatorId));
        }
    }
}