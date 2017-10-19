namespace DeliveryTracker.Localization
{
    public static class LocalizationString
    {
        #region const prefix
        
        private const string LocalizationStringPrefix = "ServerMessage";
        
        #endregion
        
        #region Error Group

        private const string ErrorGroup = "Error";
        
        public static class Error
        {
            private const string Pref = LocalizationStringPrefix + "_" + ErrorGroup + "_";
        
            public const string ServerError = Pref + "ServerError";
        
            public const string InvalidInputParameter = Pref + "InvalidInputParameter";
        
            public const string UserNotFound = Pref + "UserNotFound";
        
            public const string UserWithoutRole = Pref + "UserWithoutRole";
            
            public const string UserNotInRole = Pref + "UserNotInRole";
        
            public const string IdentityError = Pref + "IdentityError";
            
            public const string InvitationDoesnotExist = Pref + "InvitationDoesnotExist";
            
            public const string InvitationExpired = Pref + "InvitationExpired";
            
            public const string InstanceAlreadyHasCreator = Pref + "InstanceAlreadyHasCreator";
            
            public const string PerformerInAnotherInstance = Pref + "PerformerInAnotherInstance";
            
            public const string TaskNotFound = Pref + "TaskNotFound";
            
            public const string IncorrectTaskState = Pref + "IncorrectTaskState";
            
            public const string IncorrectTaskStateTransition = Pref + "IncorrectTaskStateTransition";
            
            public const string TaskIsForbidden = Pref + "TaskIsForbidden";
            
            public const string AccessDenied = Pref + "AccessDenied";

            public const string UserDeleted = Pref + "UserDeleted";
            

            public const string IdIsRequired = Pref + "IdIsRequired";
            
            public const string UserNameIsRequired = Pref + "UserNameIsRequired";
            
            public const string RoleIsRequired = Pref + "RoleIsRequired";
            
            public const string RoleRange= Pref + "RoleRange";

            public const string DisplayableNameIsRequired = Pref + "DisplayableNameIsRequired";

            public const string InvitationCodeIsRequired = Pref + "InvitationCodeIsRequired";

            public const string PasswordIsRequired = Pref + "PasswordIsRequired";
            
            public const string CaptionIsRequired = Pref + "CaptionIsRequired";
            
            public const string ContentIsRequired = Pref + "ContentIsRequired";
            
            public const string InstanceNameIsRequired = Pref + "InstanceNameIsRequired";
            
            public const string CreatorDisplayableNameIsRequired = Pref + "CreatorDisplayableNameIsRequired";
            
            public const string CreatorPasswordIsRequired = Pref + "CreatorPasswordIsRequired";
            
            public const string LongitudeIsRequired = Pref + "LongitudeIsRequired";
            
            public const string LatitudeIsRequired = Pref + "LatitudeIsRequired";
            
            public const string ExpirationDateIsRequired = Pref + "ExpirationDateIsRequired";

        }
        
        #endregion
            
        
        
    }
}