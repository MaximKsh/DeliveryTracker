using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Localization;

namespace DeliveryTracker.Tasks
{
    public static class DefaultTaskStates
    {
        public static readonly TaskState Preparing = 
            new TaskState(
                new Guid("8c9c1011-f7c1-4cef-902f-4925f5e83f4a"),
                nameof(Preparing),
                LocalizationAlias.TaskStates.Preparing);
        
        public static readonly TaskState Queue = 
            new TaskState(
                new Guid("d4595da3-6a5f-4455-b975-7637ea429cb5"),
                nameof(Queue),
                LocalizationAlias.TaskStates.Queue);
        
        public static readonly TaskState Waiting = 
            new TaskState(
                new Guid("0a79703f-4570-4a58-8509-9e598b1eefaf"),
                nameof(Waiting),
                LocalizationAlias.TaskStates.Waiting);
        
        public static readonly TaskState IntoWork = 
            new TaskState(
                new Guid("8912d18f-192a-4327-bd47-5c9963b5f2b0"),
                nameof(IntoWork),
                LocalizationAlias.TaskStates.IntoWork);
        
        public static readonly TaskState Delivered = 
            new TaskState(
                new Guid("020d7c7e-bb4e-4add-8b11-62a91471b7c8"),
                nameof(Delivered),
                LocalizationAlias.TaskStates.Delivered);
        
        public static readonly TaskState Complete = 
            new TaskState(
                new Guid("d91856f9-d1bf-4fad-a46e-c3baafabf762"),
                nameof(Complete),
                LocalizationAlias.TaskStates.Complete);
        
        public static readonly TaskState Revoked = 
            new TaskState(
                new Guid("d2e70369-3d37-420f-b176-5fa0b2c1d4a9"),
                nameof(Revoked),
                LocalizationAlias.TaskStates.Revoked);
        
        public static readonly IReadOnlyDictionary<Guid, TaskState> AllTaskStates = 
            new ReadOnlyDictionary<Guid, TaskState>(new Dictionary<Guid, TaskState>
            {
                [Preparing.Id] = Preparing,
                [Queue.Id] = Queue,
                [Waiting.Id] = Waiting,
                [IntoWork.Id] = IntoWork,
                [Delivered.Id] = Delivered,
                [Complete.Id] = Complete,
                [Revoked.Id] = Revoked,
            });
    }
}