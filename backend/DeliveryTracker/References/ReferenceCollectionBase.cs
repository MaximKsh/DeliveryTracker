using System;

namespace DeliveryTracker.References
{
    public class ReferenceCollectionBase : ReferenceEntryBase
    {
        /// <summary>
        /// Идентификатор родительской записи справочника.
        /// </summary>
        public Guid ParentId 
        {
            get => this.Get<Guid>(nameof(this.ParentId));
            set => this.Set(nameof(this.ParentId), value);
        }
        
        
        /// <summary>
        /// Действие, которое необходимо соверить с записью при редактировании
        /// родительской записи.
        /// Актуально только при выполнении редактирования родительской.
        /// В базе не сохраняется.
        /// </summary>
        public ReferenceAction Action
        {
            get => this.Get<ReferenceAction>(nameof(this.Action));
            set => this.Set(nameof(this.Action), value);
        }
        
    }
}