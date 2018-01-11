using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.References
{
    public class ProductReferenceService : IReferenceService<Product>
    {
        public ServiceResult<Product> Create(Product newData)
        {
            throw new NotImplementedException();
        }

        public ServiceResult<Product> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public ServiceResult<Product> Edit(Guid id, Product newData)
        {
            throw new NotImplementedException();
        }

        public ServiceResult Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}