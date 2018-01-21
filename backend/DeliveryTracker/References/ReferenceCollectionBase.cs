using System;

namespace DeliveryTracker.References
{
    public abstract class ReferenceCollectionBase : ReferenceEntityBase
    {
        /// <summary>
        /// Идентификатор родительской записи справочника.
        /// </summary>
        public Guid ParentId { get; set; }
        
        /// <summary>
        /// Действие, которое необходимо соверить с записью при редактировании
        /// родительской записи.
        /// Актуально только при выполнении редактирования родительской.
        /// В базе не сохраняетсяю
        /// </summary>
        public CollectionEntityAction Action { get; set; }
        
    }
}