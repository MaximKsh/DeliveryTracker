using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using Npgsql;

namespace DeliveryTracker.References
{
    public abstract class ReferenceServiceBase <T>
        where T : ReferenceEntryBase, new()
    {
        protected abstract ExecutionParameters SetCommandCreate(
            NpgsqlCommand command, 
            T newData, 
            Guid id, 
            UserCredentials credentials);

        protected abstract ExecutionParameters SetCommandEdit(
            NpgsqlCommand command, 
            T newData, 
            UserCredentials credentials);

        protected abstract ExecutionParameters SetCommandGet(
            NpgsqlCommand command, 
            Guid id, 
            bool withDeleted,
            UserCredentials credentials);

        protected abstract ExecutionParameters SetCommandGetList(
            NpgsqlCommand command, 
            ICollection<Guid> ids, 
            bool withDeleted,
            UserCredentials credentials);
        
        protected abstract ExecutionParameters SetCommandDelete(
            NpgsqlCommand command, 
            Guid id, 
            UserCredentials credentials);

        protected abstract T Read(
            IDataReader reader,  
            ReferenceServiceExecutionContext ctx);
        
        protected abstract IList<T> ReadList(
            IDataReader reader,  
            ReferenceServiceExecutionContext ctx);

        protected virtual async Task<bool> ExecDeleteAsync(
            NpgsqlCommand command, 
            ReferenceServiceExecutionContext ctx)
        {
            return await command.ExecuteNonQueryAsync() == 1;
        }
        
        protected virtual bool CanCreate(T newData, UserCredentials credentials)
        {
            return credentials.Valid
                   && (credentials.Role == DefaultRoles.CreatorRole
                       || credentials.Role == DefaultRoles.ManagerRole);
        }
        
        protected virtual bool CanEdit(T newData, UserCredentials credentials)
        {
            return credentials.Valid
                   && (credentials.Role == DefaultRoles.CreatorRole
                       || credentials.Role == DefaultRoles.ManagerRole);
        }
        
        protected virtual bool CanGet(Guid id, UserCredentials credentials)
        {
            return credentials.Valid;
        }
        
        protected virtual bool CanGetList(ICollection<Guid> ids, UserCredentials credentials)
        {
            return credentials.Valid;
        }
        
        protected virtual bool CanDelete(Guid id, UserCredentials credentials)
        {
            return credentials.Valid
                   && (credentials.Role == DefaultRoles.CreatorRole
                       || credentials.Role == DefaultRoles.ManagerRole);
        }
    }
}