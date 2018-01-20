using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.References
{
    public class ClientReferenceService : IReferenceService<ClientReferenceService>
    {
        public Task<ServiceResult<ClientReferenceService>> CreateAsync(ClientReferenceService newData, NpgsqlConnectionWrapper oc = null)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<ClientReferenceService>> GetAsync(Guid id, NpgsqlConnectionWrapper oc = null)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<ClientReferenceService>> EditAsync(ClientReferenceService newData,
            NpgsqlConnectionWrapper oc = null)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> DeleteAsync(Guid id, NpgsqlConnectionWrapper oc = null)
        {
            throw new NotImplementedException();
        }
    }
}