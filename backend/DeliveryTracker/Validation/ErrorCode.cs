namespace DeliveryTracker.Validation
{
    public static class ErrorCode
    {
        
        #region general errors
        
        /// <summary>
        /// Код ошибки: доступ запрещен.
        /// </summary>
        public const string AccessDenied = "AccessDenied";
        
        /// <summary>
        /// Код ошибки: неверный формат пароля.
        /// </summary>
        public const string IncorrectPassword = "IncorrectPassword";
        
        /// <summary>
        /// Код внутренней серверной ошибки.
        /// </summary>
        public const string ServerError = "ServerError";
        
        /// <summary>
        /// Код ошибки неправильных входных параметров.
        /// </summary>
        public const string ValidationError = "ValidationError";
        
        #endregion
        
        #region user errors
        
        /// <summary>
        /// Код ошибки: роль не найдена.
        /// </summary>
        public const string RoleNotFound = "RoleNotFound";
        
        /// <summary>
        /// Код ошибки: ошибка создания пользователя.
        /// </summary>
        public const string UserCreationError = "UserCreationError";
        
        /// <summary>
        /// Код ошибки: пользователь удален.
        /// </summary>
        public const string UserDeleted = "UserDeleted";
        
        /// <summary>
        /// Код ошибки: ошибка редактирования пользователя.
        /// </summary>
        public const string UserEditError = "UserEditError";
        
        /// <summary>
        /// Код ошибки: пользователь не найден.
        /// </summary>
        public const string UserNotFound = "UserNotFound";
        
        #endregion

        #region instance errors
        
        /// <summary>
        /// Код ошибки: инстанс не найден.
        /// </summary>
        public const string InstanceNotFound = "InstanceNotFound";

        
        
        #endregion

        #region invitation errors

        /// <summary>
        /// Код ошибки: ошибка при добавлении приглашения.
        /// </summary>
        public const string InvitationCreationError = "InvitationCreationError";
        
        /// <summary>
        /// Код ошибки: приглашение просрочено.
        /// </summary>
        public const string InvitationExpired = "InvitationExpired";
        
        /// <summary>
        /// Код ошибки: приглашение не существует.
        /// </summary>
        public const string InvitationNotFound = "InvitationNotFound";

        #endregion

        #region reference errors
        
        /// <summary>
        /// Код ошибки: ошибка при добавлении записи в справочник.
        /// </summary>
        public const string ReferenceCreationError = "ReferenceCreationError";
        
        
        /// <summary>
        /// Код ошибки: ошибка при изменении записи в справочник.
        /// </summary>
        public const string ReferenceEditError = "ReferenceEditError";
        
        /// <summary>
        /// Код ошибки: запись в справочнике не найдена.
        /// </summary>
        public const string ReferenceEntryNotFound = "ReferenceEntryNotFound";
        
        /// <summary>
        /// Код ошибки: тип справочника не найден.
        /// </summary>
        public const string ReferenceTypeNotFound = "ReferenceTypeNotFound";
        
        #endregion

        #region task errors

        /// <summary>
        /// Код ошибки: неправильное состояние задания.
        /// </summary>
        public const string IncorrectTaskState = "IncorrectTaskState";

        /// <summary>
        /// Код ошибки: неправильный переход между состояниями.
        /// </summary>
        public const string IncorrectTaskStateTransition = "IncorrectTaskStateTransition";
        
        /// <summary>
        /// Код ошибки: задание не найдено.
        /// </summary>
        public const string TaskNotFound = "TaskNotFound";

        #endregion
        
        #region view errors
        
        /// <summary>
        /// Код ошибки: группа представлений не найдена.
        /// </summary>   
        public const string ViewGroupNotFound = "ViewGroupNotFound";
        
        /// <summary>
        /// Код ошибки: представление не найдена.
        /// </summary>   
        public const string ViewNotFound = "ViewNotFound";
        
        /// <summary>
        /// Код ошибки: ошибка при преобразовании типов.
        /// </summary>
        public const string ViewResultTypeError = "ViewResultTypeError";
        
        #endregion
    }
}