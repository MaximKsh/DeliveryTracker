using System.Collections.Generic;
using DeliveryTracker.Common;

namespace DeliveryTracker.References
{
    /// <summary>
    /// Запакованный элемент справочника.
    /// Запакованный - подготовленный к сериализации с приложенными коллекциями.
    /// </summary>
    public sealed class ReferencePackage : DictionaryObject
    {
        public ReferenceEntryBase Entry
        {
            get => this.GetObject<ReferenceEntryBase>(nameof(this.Entry));
            set => this.Set(nameof(this.Entry), value);
        }

        public IList<ReferenceCollectionBase> Collections
        {
            get => this.GetList<ReferenceCollectionBase>(nameof(this.Collections));
            set => this.Set(nameof(this.Collections), value);
        }
    }
}