using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;

namespace DeliveryTracker.Tasks
{
    public interface ITaskTransitionManager
    {
        Task<ServiceResult<bool>> CanTransit(
            Guid role,
            Guid initialState,
            Guid finalState);

        Task<ServiceResult<TaskStateTransition>> GetTransition(
            Guid role,
            Guid initialState,
            Guid finalState);

        Task<ServiceResult<IList<TaskStateTransition>>> GetTransitions(
            Guid role,
            Guid initialState);
    }
}