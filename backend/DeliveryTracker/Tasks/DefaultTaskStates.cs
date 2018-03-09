using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Localization;

namespace DeliveryTracker.Tasks
{
    public static class DefaultTaskStates
    {
        public static readonly TaskState Unconfirmed = 
            new TaskState(
                new Guid(0x3f7d33e6, 0x5ecf, 0x41a3, 0x87, 0x6a, 0xc0, 0x34, 0x5f, 0x69, 0x0c, 0xa4),
                "Unconfirmed",
                LocalizationAlias.TaskStates.Unconfirmed);
        
        public static readonly TaskState New = 
            new TaskState(
                new Guid(0x135374ad, 0xba12, 0x4ada, 0x9d, 0xc7, 0x8f, 0x8e, 0x3b, 0x11, 0xd9, 0xe5),
                "New",
                LocalizationAlias.TaskStates.New);
        
        public static readonly TaskState InProgress = 
            new TaskState(
                new Guid(0xbeaff26f, 0x2193, 0x41ed, 0xb2, 0xa6, 0x98, 0x3b, 0x70, 0x7a, 0x21, 0x6d),
                "InProgress",
                LocalizationAlias.TaskStates.InProgress);
        
        public static readonly TaskState Complete = 
            new TaskState(
                new Guid(0xd91856f9, 0xd1bf, 0x4fad, 0xa4, 0x6e, 0xc3, 0xba, 0xaf, 0xab, 0xf7, 0x62),
                "Complete",
                LocalizationAlias.TaskStates.Complete);
        
        public static readonly TaskState Cancelled = 
            new TaskState(
                new Guid(0x1483e2f3, 0x5bcf, 0x48ca, 0xbc, 0xaa, 0x87, 0x05, 0x73, 0x99, 0x74, 0x65),
                "Cancelled",
                LocalizationAlias.TaskStates.Cancelled);
        
        public static readonly IReadOnlyDictionary<Guid, TaskState> AllTaskStates = 
            new ReadOnlyDictionary<Guid, TaskState>(new Dictionary<Guid, TaskState>
            {
                [Unconfirmed.Id] = Unconfirmed,
                [New.Id] = New,
                [InProgress.Id] = InProgress,
                [Complete.Id] = Complete,
                [Cancelled.Id] = Cancelled,
            });
        
        
    }
}