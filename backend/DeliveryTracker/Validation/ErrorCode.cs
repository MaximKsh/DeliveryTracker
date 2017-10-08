namespace DeliveryTracker.Validation
{
    public static class ErrorCode
    {
        /// <summary>
        /// Код внутренней серверной ошибки.
        /// </summary>
        public const string ServerError = "ServerError";
        
        /// <summary>
        /// Код ошибки неправильных входных параметров.
        /// </summary>
        public const string InvalidInputParameter = "InvalidQueryParameter";

        /// <summary>
        /// Код ошибки: пользователь не найден.
        /// </summary>
        public const string UserNotFound = "UserNotFound";
        
        /// <summary>
        /// Код ошибки: пользователь не входит ни в одну роль.
        /// </summary>
        public const string UserWithoutRole = "UserWithoutRole";

        /// <summary>
        /// Код ошибки: пользователь не входит в ожидаемую роль.
        /// </summary>
        public const string UserNotInRole = "UserNotInRole";
        
        /// <summary>
        /// Код ошибки, скрывающий ошибку модуля Identity.
        /// </summary>
        public const string IdentityError = "IdentityError";
        
        /// <summary>
        /// Код ошибки: приглашение не существует.
        /// </summary>
        public const string InvitationDoesnotExist = "InvitationDoesnotExist";

        /// <summary>
        /// Код ошибки: приглашение просрочено.
        /// </summary>
        public const string InvitationExpired = "InvitationExpired";
        
        /// <summary>
        /// Код ошибки: у группы уже есть создатель.
        /// </summary>
        public const string GroupAlreadyHasCreator = "GroupAlreadyHasCreator";
        
        /// <summary>
        /// Код ошибки: указанный исполнитель находится в другой группе.
        /// </summary>
        public const string PerformerInAnotherGroup = "PerformerInAnotherGroup";  
        
        /// <summary>
        /// Код ошибки: задание не найдено.
        /// </summary>
        public const string TaskNotFound = "TaskNotFound";
        
        /// <summary>
        /// Код ошибки: неправильное состояние задания.
        /// </summary>
        public const string IncorrectTaskState = "IncorrectTaskState";
        
        /// <summary>
        /// Код ошибки: неправильный переход между состояниями.
        /// </summary>
        public const string IncorrectTaskStateTransition = "IncorrectTaskStateTransition";
        
        /// <summary>
        /// Код ошибки: нет доступа к заданию.
        /// </summary>
        public const string TaskIsForbidden = "TaskIsForbidden";
    }
}