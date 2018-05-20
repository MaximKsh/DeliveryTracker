namespace DeliveryTracker.Localization
{
    public static class LocalizationAlias
    {
        #region const prefix
        
        private const string LocalizationStringPrefix = "ServerMessage";
        
        #endregion

        #region Default values

        public static class DefaultValues
        {
            public const string CashEn = "Cash";
            public const string CashRu = "Наличные";
            public const string CardEn = "Credit card";
            public const string CardRu = "Кредитная карта";
        }

        #endregion
        
        #region Error Group

        private const string ErrorGroup = "Error";
        
        public static class Error
        {
            private const string Pref = LocalizationStringPrefix + "_" + ErrorGroup + "_";
            
            
            public const string AccessDenied = Pref + "AccessDenied";
            public const string BuildRouteError = Pref + "BuildRouteError";
            public const string CantDeleteCreator = Pref + "CantDeleteCreator";
            public const string CantDeleteUser = Pref + "CantDeleteUser";
            public const string IncorrectPassword = Pref + "IncorrectPassword";
            public const string IncorrectTaskState = Pref + "IncorrectTaskState";
            public const string IncorrectTaskStateTransition = Pref + "IncorrectTaskStateTransition";
            public const string InstanceNotFound = Pref + "InstanceNotFound";
            public const string InvitationCreationError = Pref + "InvitationCreationError";
            public const string InvitationExpired = Pref + "InvitationExpired"; 
            public const string InvitationNotFound = Pref + "InvitationNotFound";
            public const string ObserverCancelExecution = Pref + "ObserverCancelExecution";
            public const string ReferenceCreationError = Pref + "ReferenceCreationError";
            public const string ReferenceEditError = Pref + "ReferenceEditError";
            public const string ReferenceEntryNotFound = Pref + "ReferenceEntryNotFound";
            public const string ReferenceTypeNotFound = Pref + "ReferenceTypeNotFound";
            public const string RoleNotFound = Pref + "RoleNotFound";
            public const string ServerError = Pref + "ServerError";
            public const string TaskCreationError = Pref + "TaskCreationError";
            public const string TaskEditError = Pref + "TaskEditError";
            public const string TaskNotFound = Pref + "TaskNotFound";
            public const string UserCreationError = Pref + "UserCreationError";
            public const string UserDeleted = Pref + "UserDeleted";
            public const string UserEditError = Pref + "UserEditError";
            public const string UserNotFound = Pref + "UserNotFound";
            public const string ValidationError = Pref + "ValidationError";
            public const string ViewGroupNotFound = Pref + "ViewGroupNotFound";
            public const string ViewNotFound = Pref + "ViewNotFound";
            public const string ViewResultTypeError = Pref + "ViewResultTypeError";
        }
        
        #endregion
            
        #region Push Message Group
        
        private const string PushMessageGroup = "PushMessage";
        
        public static class PushMessage
        {
            private const string Pref = LocalizationStringPrefix + "_" + PushMessageGroup + "_";

            public const string TaskHeader = Pref + "TaskHeader";
            public const string TaskPlannedToYou = Pref + "TaskPlannedToYou";
            public const string TaskWaits = Pref + "TaskWaits";
            public const string TaskRevoked = Pref + "TaskRevoked";
            public const string TaskReturnToPreparing = Pref + "TaskReturnToPreparing";
            public const string TaskDelivered = Pref + "TaskDelivered";
        }
        
        #endregion
        
        #region Roles
        
        private const string RolesGroup = "Roles";
        
        public static class Roles
        {
            private const string Pref = LocalizationStringPrefix + "_" + RolesGroup + "_";
                    
            public const string CreatorRole = Pref + "CreatorRole";
            public const string ManagerRole = Pref + "ManagerRole";
            public const string PerformerRole = Pref + "PerformerRole";
            public const string SchedulerRole = Pref + "SchedulerRole";
        }
        
        #endregion
        
        #region References
        
        private const string ReferencesGroup = "References";
        
        public static class References
        {
            private const string Pref = LocalizationStringPrefix + "_" + ReferencesGroup + "_";
                    
            public const string Clients = Pref + "Clients";
            public const string Products = Pref + "Products";
            public const string PaymentTypes = Pref + "PaymentTypes";
            public const string Warehouses = Pref + "Warehouses";
        }
        
        #endregion
        
        #region Views
        
        private const string ViewsGroup = "Views";
        
        public static class Views
        {
            private const string Pref = LocalizationStringPrefix + "_" + ViewsGroup + "_";
                    
            public const string ClientsView = Pref + "ClientsView";
            public const string InvitationsView = Pref + "InvitationsView";
            public const string ManagersView = Pref + "ManagersView";
            public const string PaymentTypesView = Pref + "PaymentTypesView";
            public const string PerformersView = Pref + "PerformersView";
            public const string ProductsView = Pref + "ProductsView";
            public const string ActualTasksPerformerView = Pref + "ActualTasksPerformerView";
            public const string ActualTasksManagerView = Pref + "ActualTasksManagerView";
            public const string CompletedTasksManagerView = Pref + "CompletedTasksManagerView";
            public const string UserTasksView = Pref + "UserTasksView";
            public const string MyTasksManagerView = Pref + "MyTasksManagerView";
            public const string PreparingTasksManagerView = Pref + "PreparingTasksManagerView";
            public const string QueueTasksManagerView = Pref + "QueueTasksManagerView"; 
            public const string RevokedTasksManagerView = Pref + "RevokedTasksManagerView";
            public const string RouteView = Pref + "RouteView";
            public const string QueueTasksPerformerView = Pref + "QueueTasksPerformerView";
            public const string DeliveredTasksPerformerView = Pref + "DeliveredTasksPerformerView";
            public const string CompletedTasksPerformerView = Pref + "CompletedTasksPerformerView";
            public const string RevokedTasksPerformerView = Pref + "RevokedTasksPerformerView";
            public const string WarehousesView = Pref + "WarehousesView";
        }
        
        #endregion
        
        #region TaskStates
        
        private const string TaskStatesGroup = "TaskStates";
        
        public static class TaskStates
        {
            private const string Pref = LocalizationStringPrefix + "_" + TaskStatesGroup + "_";
                    
            public const string Preparing = Pref + "Preparing";
            public const string Queue = Pref + "Queue";
            public const string Waiting = Pref + "Waiting";
            public const string IntoWork = Pref + "IntoWork";
            public const string Delivered = Pref + "Delivered";
            public const string Complete = Pref + "Complete";
            public const string Revoked = Pref + "Revoked";
        }
        
        #endregion
        
    }
}