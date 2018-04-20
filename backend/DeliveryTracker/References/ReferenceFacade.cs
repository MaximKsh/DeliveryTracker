using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.References
{
    public sealed class ReferenceFacade : IReferenceFacade
    {
        #region fields
        
        private readonly IReadOnlyDictionary<string, IReferenceService> services;
        
        private readonly IReadOnlyDictionary<string, ICollectionReferenceService> collectionServices;

        private readonly IReadOnlyDictionary<string, ReferenceDescription> referencesList;

        private readonly IPostgresConnectionProvider cp;
        
        #endregion
        
        #region constuctor
        
        public ReferenceFacade(IServiceProvider serviceProvider,
            IPostgresConnectionProvider cp)
        {
            this.cp = cp;
            var referenceServices = serviceProvider.GetServices<IReferenceService>();
            var dict = new Dictionary<string, IReferenceService>();
            foreach (var rs in referenceServices)
            {
                dict[rs.Name] = rs;
            }
            this.services = new ReadOnlyDictionary<string, IReferenceService>(dict);
            this.referencesList = new ReadOnlyDictionary<string, ReferenceDescription>(
                this.services.ToDictionary(k => k.Key, v => v.Value.ReferenceDescription));
            
            var collectionReferenceServices = serviceProvider.GetServices<ICollectionReferenceService>();
            var collectionDict = new Dictionary<string, ICollectionReferenceService>();
            foreach (var rs in collectionReferenceServices)
            {
                collectionDict[rs.Name] = rs;
            }
            this.collectionServices = new ReadOnlyDictionary<string, ICollectionReferenceService>(collectionDict);
        }
        
        #endregion

        #region public
        
        /// <inheritdoc />
        public IReadOnlyDictionary<string, ReferenceDescription> GetReferencesList()
        {
            return this.referencesList;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferencePackage>> CreateAsync(
            string type,
            IDictionary<string, object> value,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!this.services.TryGetValue(type, out var service))
            {
                return new ServiceResult<ReferencePackage>(ErrorFactory.ReferenceTypeNotFound(type));
            }

            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            using (var transact = conn.BeginTransaction())
            {
                if (!JsonHelper.TryGetSubdictionary(value, nameof(ReferencePackage.Entry), out var entryDict))
                {
                    return new ServiceResult<ReferencePackage>(ErrorFactory.ReferenceCreationError(type));
                }

                var result = await service.CreateAsync(entryDict , conn);
                if (!result.Success)
                {
                    transact.Rollback();
                    return new ServiceResult<ReferencePackage>(result.Errors);
                }

                if (JsonHelper.TryGetSubdictionaryList(value, nameof(ReferencePackage.Collections), out var collectionsList))
                {
                    foreach (var collection in collectionsList)
                    {
                        if (!collection.TryGetValue(nameof(ReferenceEntryBase.Type), out var typeObj)
                            || !(typeObj is string collectionType)
                            || !this.collectionServices.TryGetValue(collectionType, out var collectionService))
                        {
                            continue;
                        }
                        
                        var collectionResult = await collectionService.CreateAsync(collection, conn);
                        if (!collectionResult.Success)
                        {
                            transact.Rollback();
                            return new ServiceResult<ReferencePackage>(collectionResult.Errors);
                        }
                    }
                }
                
                var entry = result.Result;
                var packResult = await service.PackAsync(entry, conn);
                if (packResult.Success)
                {
                    transact.Commit();
                }
                else
                {
                    transact.Rollback();
                }
                return packResult;
            }            
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferencePackage>> GetAsync(
            string type,
            Guid id,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!this.services.TryGetValue(type, out var service))
            {
                return new ServiceResult<ReferencePackage>(ErrorFactory.ReferenceTypeNotFound(type));
            }

            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var result = await service.GetAsync(id, withDeleted, conn);
                if (!result.Success)
                {
                    return new ServiceResult<ReferencePackage>(result.Errors);
                }

                var entry = result.Result;
                return await service.PackAsync(entry, conn);
            }
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<ReferencePackage>>> GetAsync(
            string type,
            ICollection<Guid> ids,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!this.services.TryGetValue(type, out var service))
            {
                return new ServiceResult<IList<ReferencePackage>>(ErrorFactory.ReferenceTypeNotFound(type));
            }

            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var result = await service.GetAsync(ids, withDeleted, conn);
                if (!result.Success)
                {
                    return new ServiceResult<IList<ReferencePackage>>(result.Errors);
                }

                var entries = result.Result;
                return await service.PackAsync(entries, conn);
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferencePackage>> EditAsync(
            string type,
            IDictionary<string, object> newData,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!this.services.TryGetValue(type, out var service))
            {
                return new ServiceResult<ReferencePackage>(ErrorFactory.ReferenceTypeNotFound(type));
            }
            
            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            using (var transact = conn.BeginTransaction())
            {
                if (!JsonHelper.TryGetSubdictionary(newData, nameof(ReferencePackage.Entry), out var entryDict))
                {
                    return new ServiceResult<ReferencePackage>(ErrorFactory.ReferenceCreationError(type));
                }
                
                var editResult = await service.EditAsync(entryDict, oc);
                if (!editResult.Success)
                {
                    transact.Rollback();
                    return new ServiceResult<ReferencePackage>(editResult.Errors);
                }

                if (JsonHelper.TryGetSubdictionaryList(
                    newData,
                    nameof(ReferencePackage.Collections),
                    out var collectionsList))
                {
                    foreach (var collection in collectionsList)
                    {
                        if (!collection.TryGetValue(nameof(ReferenceEntryBase.Type), out var typeObj)
                            || !(typeObj is string collectionType)
                            || !this.collectionServices.TryGetValue(collectionType, out var collectionService)
                            || !collection.TryGetValue(nameof(ReferenceCollectionBase.Action), out var actionObj)
                            || !Enum.TryParse(typeof(ReferenceAction), actionObj.ToString(), out var action))
                        {
                            continue;
                        }
                        
                        var collectionResult = ServiceResult.Successful; 
                        
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (action)
                        {
                            case ReferenceAction.Create:
                                collectionResult = await collectionService.CreateAsync(collection, conn);
                                break;
                            case ReferenceAction.Edit:
                                collectionResult = await collectionService.EditAsync(collection, conn);
                                break;
                            case ReferenceAction.Delete:
                                if (collection.TryGetValue(nameof(ReferenceEntryBase.Id), out var idObj)
                                    && (idObj is Guid id || Guid.TryParse(idObj.ToString(), out id))
                                    && collection.TryGetValue(nameof(ReferenceCollectionBase.ParentId), out var pidObj)
                                    && (pidObj is Guid pid || Guid.TryParse(pidObj.ToString(), out pid)))
                                {
                                    collectionResult = await collectionService.DeleteAsync(id, pid, conn);
                                }
                                break;
                        } 
                        if (!collectionResult.Success)
                        {
                            transact.Rollback();
                            return new ServiceResult<ReferencePackage>(collectionResult.Errors);
                        }
                    }
                    
                }
                
                /*
                foreach (var collection in package.Collections)
                {
                    

                    
                }*/
                
                var packageResult = await service.PackAsync(editResult.Result, conn);
                if (!packageResult.Success)
                {
                    transact.Rollback();
                    return new ServiceResult<ReferencePackage>(packageResult.Errors);
                }
                
                return new ServiceResult<ReferencePackage>(packageResult.Result);
            }
            
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
            return new ServiceResult(ErrorFactory.ReferenceTypeNotFound(type));
        }

        #endregion
    }
}