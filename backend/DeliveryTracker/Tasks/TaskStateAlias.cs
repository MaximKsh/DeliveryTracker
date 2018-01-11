namespace DeliveryTracker.Tasks
{
    public static class TaskStateAlias
    {
        public const string NewUndistributedState = "TaskState_NewUndistributed";
        
        public const string NewState = "TaskState_New";
        
        public const string InWorkState = "TaskState_InWork";

        public const string PerformedState = "TaskState_Performed";

        public const string CancelledState = "TaskState_Cancelled";

        public const string CancelledByManagerState = "TaskState_CancelledByManager";
    }
}