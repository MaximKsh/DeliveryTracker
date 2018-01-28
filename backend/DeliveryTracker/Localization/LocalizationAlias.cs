namespace DeliveryTracker.Localization
{
    public static class LocalizationAlias
    {
        #region const prefix
        
        private const string LocalizationStringPrefix = "ServerMessage";
        
        #endregion
        
        #region Error Group

        private const string ErrorGroup = "Error";
        
        public static class Error
        {
            private const string Pref = LocalizationStringPrefix + "_" + ErrorGroup + "_";
            
            
            public const string AccessDenied = Pref + "AccessDenied";
            public const string IncorrectPassword = Pref + "IncorrectPassword";
            public const string InstanceNotFound = Pref + "InstanceNotFound";
            public const string InvitationExpired = Pref + "InvitationExpired"; 
            public const string InvitationNotFound = Pref + "InvitationNotFound";
            public const string RoleNotFound = Pref + "RoleNotFound";
            public const string ServerError = Pref + "ServerError";
            public const string UserCreationError = Pref + "UserCreationError";
            public const string UserDeleted = Pref + "UserDeleted";
            public const string UserEditError = Pref + "UserEditError";
            public const string UserNotFound = Pref + "UserNotFound";
            public const string ValidationError = Pref + "ValidationError";
            public const string InvitationCreationError = Pref + "InvitationCreationError";
            public const string ReferenceCreationError = Pref + "ReferenceCreationError";
            public const string ReferenceEditError = Pref + "ReferenceEditError";
            public const string ReferenceEntryNotFound = Pref + "ReferenceEntryNotFound";
            public const string ViewGroupNotFound = Pref + "ViewGroupNotFound";
            public const string ViewNotFound = Pref + "ViewNotFound";
            public const string ViewResultTypeError = Pref + "ViewResultTypeError";
            public const string ReferenceTypeNotFound = Pref + "ReferenceTypeNotFound";



            
            
            public const string UserWithoutRole = Pref + "UserWithoutRole";
            
            public const string UserNotInRole = Pref + "UserNotInRole";
        
            public const string IdentityError = Pref + "IdentityError";
            
            
            
            
            public const string PerformerInAnotherInstance = Pref + "PerformerInAnotherInstance";
            
            public const string TaskNotFound = Pref + "TaskNotFound";
            
            public const string IncorrectTaskState = Pref + "IncorrectTaskState";
            
            public const string IncorrectTaskStateTransition = Pref + "IncorrectTaskStateTransition";
            
            public const string TaskIsForbidden = Pref + "TaskIsForbidden";
            
            
            public const string RoleRange= Pref + "RoleRange";


            public const string InvitationCodeIsRequired = Pref + "InvitationCodeIsRequired";

            public const string InstanceNameIsRequired = Pref + "InstanceNameIsRequired";
            
            public const string LongitudeIsRequired = Pref + "LongitudeIsRequired";
            
            public const string LatitudeIsRequired = Pref + "LatitudeIsRequired";
            
            public const string ExpirationDateIsRequired = Pref + "ExpirationDateIsRequired";

        }
        
        #endregion
            
        #region Push Message Group
        
        private const string PushMessageGroup = "PushMessage";
        
        public static class PushMessage
        {
            private const string Pref = LocalizationStringPrefix + "_" + PushMessageGroup + "_";
                    
            public const string AddTask = Pref + "AddTask";
            public const string CompleteTask = Pref + "CompleteTask";
            public const string CancelTask = Pref + "CancelTask";
            public const string TaskCancelledByManager = Pref + "TaskCancelledByManager";
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
        }
        
        #endregion
        
    }
}