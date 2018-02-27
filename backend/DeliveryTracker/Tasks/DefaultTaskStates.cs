using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker.Tasks
{
    public static class DefaultTaskStates
    {
        public static readonly TaskState Unconfirmed = 
            new TaskState(
                new Guid(0x3f7d33e6, 0x5ecf, 0x41a3, 0x87, 0x6a, 0xc0, 0x34, 0x5f, 0x69, 0x0c, 0xa4),
                "Unconfirmed",
                "");
        
        public static readonly IReadOnlyDictionary<Guid, TaskState> AllTaskStates = 
            new ReadOnlyDictionary<Guid, TaskState>(new Dictionary<Guid, TaskState>
            {
                [Unconfirmed.Id] = Unconfirmed,
            });
        
        
    }
}