using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.References
{
    public class ReferenceFacade : IReferenceFacade
    {
        #region fields
        
        private readonly ImmutableDictionary<string, IReferenceService> services;

        private readonly IDictionary<string, ReferenceDescription> referencesList;
        
        #endregion
        
        #region constuctor
        
        public ReferenceFacade(IServiceProvider serviceProvider)
        {
            var referenceServices = serviceProvider.GetServices<IReferenceService>();
            var dict = new Dictionary<string, IReferenceService>();
            foreach (var rs in referenceServices)
            {
                dict[rs.Name] = rs;
            }

            this.services = dict.ToImmutableDictionary();
            this.referencesList = this.services.ToDictionary(k => k.Key, v => v.Value.ReferenceDescription);
        }
        
        #endregion

        #region public
        
        /// <inheritdoc />
        public IDictionary<string, ReferenceDescription> GetReferencesList()
        {
            return this.referencesList;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferenceEntityBase>> CreateAsync(
            string type,
            IDictionary<string, object> value,
            NpgsqlConnectionWrapper oc = null)
        {
            if (this.services.TryGetValue(type, out var service))
            {
                return await service.CreateAsync(value, oc);
            }
            return new ServiceResult<ReferenceEntityBase>(ErrorFactory.ReferenceTypeNotFound(type));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferenceEntityBase>> GetAsync(
            string type,
            Guid id,
            NpgsqlConnectionWrapper oc = null)
        {
            if (this.services.TryGetValue(type, out var service))
            {
                return await service.GetAsync(id, oc);
            }
            return new ServiceResult<ReferenceEntityBase>(ErrorFactory.ReferenceTypeNotFound(type));
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<ReferenceEntityBase>>> GetAsync(
            string type,
            IList<Guid> ids,
            NpgsqlConnectionWrapper oc = null)
        {
            if (this.services.TryGetValue(type, out var service))
            {
                return await service.GetAsync(ids, oc);
            }
            return new ServiceResult<IList<ReferenceEntityBase>>(ErrorFactory.ReferenceTypeNotFound(type));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferenceEntityBase>> EditAsync(
            string type,
            IDictionary<string, object> newData,
            NpgsqlConnectionWrapper oc = null)
        {
            if (this.services.TryGetValue(type, out var service))
            {
                return await service.EditAsync(newData, oc);
            }
            return new ServiceResult<ReferenceEntityBase>(ErrorFactory.ReferenceTypeNotFound(type));
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(
            string type,
            Guid id,
            NpgsqlConnectionWrapper oc = null)
        {
            if (this.services.TryGetValue(type, out var service))
            {
                return await service.DeleteAsync(id, oc);
            }
            return new ServiceResult<ReferenceEntityBase>(ErrorFactory.ReferenceTypeNotFound(type));
        }

        #endregion
    }
}