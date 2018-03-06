using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Tasks
{
    public interface ITaskStateTransitionManager
    {
        Task<ServiceResult<bool>> CanTransit(
            Guid taskId,
            Guid userId,
            Guid transitionId,
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult<bool>> HasTransition(
            Guid role,
            Guid initialState,
            Guid finalState, 
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<TaskStateTransition>> GetTransition(
            Guid role,
            Guid initialState,
            Guid finalState, 
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<IList<TaskStateTransition>>> GetTransitions(
            Guid role,
            Guid initialState, 
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult<IList<TaskStateTransition>>> GetTransitions(
            Guid role,
            ICollection<Guid> initialState, 
            NpgsqlConnectionWrapper oc = null);
    }
}