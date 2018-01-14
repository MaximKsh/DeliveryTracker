using System;
using System.Collections.Generic;
using DeliveryTracker.Common;

namespace DeliveryTracker.Identification
{
    public class Role : IDictionarySerializable
    {
        public Role()
        {
        }
        
        public Role(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
        
        /// <summary>
        /// ID роли.
        /// </summary>
        public Guid Id { get; private set; }
        
        /// <summary>
        /// Локализуемое имя роли.
        /// </summary>
        public string Name { get; private set; }
        
        /// <inheritdoc />
        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Id)] = this.Id,
                [nameof(this.Name)] = this.Name,
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.Id = dict.GetPlain<Guid>(nameof(this.Id));
            this.Name = dict.GetPlain<string>(nameof(this.Name));
        }

    }
}
