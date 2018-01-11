using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.References
{
    public interface IReferenceService<T>
        where T : class
    {
        ServiceResult<T> Create(T newData);
        ServiceResult<T> Get(Guid id);
        ServiceResult<T> Edit(Guid id, T newData);
        ServiceResult Delete(Guid id);
    }
}