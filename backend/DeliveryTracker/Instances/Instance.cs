using System;
using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.DbModels;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Instance : DictionarySerializableBase
    {
        public Instance()
        {
        }

        public Instance(InstanceModel instanceModel)
        {
            if (instanceModel == null)
            {
                throw new ArgumentNullException();
            }

            this.Id = instanceModel.Id;
            this.Name = instanceModel.InstanceName;
        }

        /// <summary>
        /// Id группы.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя группы.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Id)] = this.Id,
                [nameof(this.Name)] = this.Name,
            };
        }

        /// <inheritdoc />
        public override void Deserialize(IDictionary<string, object> dict)
        {
            this.Id = GetPlain<Guid>(dict, nameof(this.Id));
            this.Name = GetPlain<string>(dict, nameof(this.Name));
        }
    }
}