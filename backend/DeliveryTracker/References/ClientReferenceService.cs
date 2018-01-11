using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.References
{
    public class ClientReferenceService : IReferenceService<ClientReferenceService>
    {
        public ServiceResult<ClientReferenceService> Create(ClientReferenceService newData)
        {
            throw new NotImplementedException();
        }

        public ServiceResult<ClientReferenceService> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public ServiceResult<ClientReferenceService> Edit(Guid id, ClientReferenceService newData)
        {
            throw new NotImplementedException();
        }

        public ServiceResult Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}