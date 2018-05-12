using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;
using DeliveryTracker.Tasks.TaskObservers;

namespace DeliveryTracker.Validation
{
    public static class ErrorFactory
    {
        #region generic errors
        
        /// <summary>
        /// Доступ запрещен.
        /// </summary>
        /// <returns></returns>
        public static IError AccessDenied() =>
            new Error(
                ErrorCode.AccessDenied,
                LocalizationAlias.Error.AccessDenied);


        /// <summary>
        /// Неверный формат пароля.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectPassword(
            int? minLen = null,
            int? maxLen = null,
            bool atLeastOneUpperCase = false,
            bool atLeastOneLowerCase = false,
            bool atLeastOneDigit = false,
            string forbiddenCharacters = null,
            int? sameCharactersInARow = null)
        {
            var formatErrors = new Dictionary<string, string>();
            if (minLen.HasValue)
            {
                formatErrors.Add(nameof(minLen), minLen.Value.ToString());
            }
            if (maxLen.HasValue)
            {
                formatErrors.Add(nameof(maxLen), maxLen.Value.ToString());
            }
            if (atLeastOneUpperCase)
            {
                formatErrors.Add(nameof(atLeastOneUpperCase), "true");
            }
            if (atLeastOneLowerCase)
            {
                formatErrors.Add(nameof(atLeastOneLowerCase), "true");
            }
            if (atLeastOneDigit)
            {
                formatErrors.Add(nameof(atLeastOneDigit), "true");
            }
            if (forbiddenCharacters != null)
            {
                formatErrors.Add(nameof(forbiddenCharacters), forbiddenCharacters);
            }
            if (sameCharactersInARow.HasValue)
            {
                formatErrors.Add(nameof(sameCharactersInARow), sameCharactersInARow.Value.ToString());
            }
            return new Error(
                ErrorCode.IncorrectPassword,
                LocalizationAlias.Error.IncorrectPassword,
                formatErrors);
        }
        
        /// <summary>
        /// Ошибка сервера.
        /// </summary>
        /// <returns></returns>
        public static IError ServerError() =>
            new Error(
                ErrorCode.ServerError,
                LocalizationAlias.Error.ServerError);
        
        /// <summary>
        /// Неправильный входные параметры.
        /// </summary>
        /// <param name="invalidValues"></param>
        /// <returns></returns>
        public static IError ValidationError(IEnumerable<KeyValuePair<string, object>> invalidValues) =>
            new Error(
                ErrorCode.ValidationError,
                LocalizationAlias.Error.ValidationError,
                invalidValues.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? "null"));
        
        
        #endregion
        
        #region user errors
        
        /// <summary>
        /// Невозможно удалить создателя.
        /// </summary>
        /// <returns></returns>
        public static IError CantDeleteCreator() =>
            new Error(
                ErrorCode.CantDeleteCreator,
                LocalizationAlias.Error.CantDeleteCreator);
        
        /// <summary>
        /// Невозможно удалить пользователя.
        /// </summary>
        /// <returns></returns>
        public static IError CantDeleteUser() =>
            new Error(
                ErrorCode.CantDeleteUser,
                LocalizationAlias.Error.CantDeleteUser);
        
        /// <summary>
        /// Роль не найдена.
        /// </summary>
        /// <returns></returns>
        public static IError RoleNotFound() =>
            new Error(
                ErrorCode.RoleNotFound,
                LocalizationAlias.Error.RoleNotFound);
        
        /// <summary>
        /// Возникла ошибка при создании пользователя.
        /// </summary>
        /// <returns></returns>
        public static IError UserCreationError() =>
            new Error(
                ErrorCode.UserCreationError,
                LocalizationAlias.Error.UserCreationError);
        
        /// <summary>
        /// Пользователь удален.
        /// </summary>
        /// <returns></returns>
        public static IError UserDeleted(string username) =>
            new Error(
                ErrorCode.UserDeleted,
                LocalizationAlias.Error.UserDeleted,
                new Dictionary<string, string>
                {
                    ["username"] = username,
                });
        
        /// <summary>
        /// Возникла ошибка при редактировании пользователя.
        /// </summary>
        /// <returns></returns>
        public static IError UserEditError() =>
            new Error(
                ErrorCode.UserEditError,
                LocalizationAlias.Error.UserEditError);
        
        /// <summary>
        /// Пользователь не найден.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IError UserNotFound(Guid userId) =>
            new Error(
                ErrorCode.UserNotFound,
                LocalizationAlias.Error.UserNotFound,
                new Dictionary<string, string>
                {
                    ["userId"] = userId.ToString(),
                });
        
        /// <summary>
        /// Пользователь не найден.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static IError UserNotFound(string code) =>
            new Error(
                ErrorCode.UserNotFound,
                LocalizationAlias.Error.UserNotFound,
                new Dictionary<string, string>
                {
                    ["code"] = code,
                });
        
        
        #endregion
        
        #region instance errors
        
        /// <summary>
        /// Инстанс не найден.
        /// </summary>
        /// <returns></returns>
        public static IError InstanceNotFound() =>
            new Error(
                ErrorCode.InstanceNotFound,
                LocalizationAlias.Error.InstanceNotFound);
        
        #endregion
        
        #region invitations errors

        /// <summary>
        /// Ошибка при создании приглашения.
        /// </summary>
        /// <returns></returns>
        public static IError InvitationCreationError() =>
            new Error(
                ErrorCode.InvitationCreationError,
                LocalizationAlias.Error.InvitationCreationError);
        
        /// <summary>
        /// Указанное приглашение просрочено.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="expired"></param>
        /// <returns></returns>
        public static IError InvitaitonExpired(string invitationCode, DateTime expired) =>
            new Error(
                ErrorCode.InvitationExpired,
                LocalizationAlias.Error.InvitationExpired,
                new Dictionary<string, string>
                {
                    ["invitationCode"] = invitationCode,
                    ["expired"] = expired.ToString(CultureInfo.InvariantCulture),
                });
        
        /// <summary>
        /// Указанное приглашение не существует.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IError InvitationNotFound(Guid id) =>
            new Error(
                ErrorCode.InvitationNotFound,
                LocalizationAlias.Error.InvitationNotFound,
                new Dictionary<string, string>
                {
                    ["id"] = id.ToString(),
                });
        
        /// <summary>
        /// Указанное приглашение не существует.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <returns></returns>
        public static IError InvitationNotFound(string invitationCode) =>
            new Error(
                ErrorCode.InvitationNotFound,
                LocalizationAlias.Error.InvitationNotFound,
                new Dictionary<string, string>
                {
                    ["invitationCode"] = invitationCode,
                });
        
        #endregion
        
        #region references error
            
       
        /// <summary>
        /// Ошибка при создании новой записи в справочнике.
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        public static IError ReferenceCreationError(string referenceType) =>
            new Error(
                ErrorCode.ReferenceCreationError,
                LocalizationAlias.Error.ReferenceCreationError,
                new Dictionary<string, string>
                {
                    ["referenceType"] = referenceType,
                });
        
        /// <summary>
        /// Ошибка при редактировании записи в справочнике.
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        public static IError ReferenceEditError(string referenceType) =>
            new Error(
                ErrorCode.ReferenceEditError,
                LocalizationAlias.Error.ReferenceEditError,
                new Dictionary<string, string>
                {
                    ["referenceType"] = referenceType,
                });

        /// <summary>
        /// Не найдена запись в справочнике.
        /// </summary>
        /// <param name="referenceType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IError ReferenceEntryNotFound(string referenceType, Guid id) =>
            new Error(
                ErrorCode.ReferenceEntryNotFound,
                LocalizationAlias.Error.ReferenceEntryNotFound,
                new Dictionary<string, string>
                {
                    ["referenceType"] = referenceType,
                    ["id"] = id.ToString(),
                });
        
        /// <summary>
        /// Не найдена тип справочника.
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        public static IError ReferenceTypeNotFound(string referenceType) =>
            new Error(
                ErrorCode.ReferenceTypeNotFound,
                LocalizationAlias.Error.ReferenceTypeNotFound,
                new Dictionary<string, string>
                {
                    ["referenceType"] = referenceType,
                });
        
        #endregion
        
        #region views
        
        /// <summary>
        /// Группа представлений не найдена.
        /// </summary>
        /// <param name="viewGroup"></param>
        /// <returns></returns>
        public static IError ViewGroupNotFound(string viewGroup) =>
            new Error(
                ErrorCode.ViewGroupNotFound,
                LocalizationAlias.Error.ViewGroupNotFound,
                new Dictionary<string, string>
                {
                    ["viewGroup"] = viewGroup,
                });
        
        /// <summary>
        /// Представление не найдено в группе.
        /// </summary>
        /// <param name="viewGroup"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static IError ViewNotFound(string viewGroup, string viewName) =>
            new Error(
                ErrorCode.ViewNotFound,
                LocalizationAlias.Error.ViewNotFound,
                new Dictionary<string, string>
                {
                    ["viewGroup"] = viewGroup,
                    ["view"] = viewName,
                });
        
        /// <summary>
        /// Ошибка при преобразовании типов результата выполнения представления.
        /// </summary>
        /// <param name="viewGroup"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static IError ViewResultTypeError(string viewGroup, string viewName) =>
            new Error(
                ErrorCode.ViewResultTypeError,
                LocalizationAlias.Error.ViewResultTypeError,
                new Dictionary<string, string>
                {
                    ["viewGroup"] = viewGroup,
                    ["view"] = viewName,
                });
        
        #endregion

        #region tasks
        
        /// <summary>
        /// Некорректное состояние задания.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectTaskState(Guid taskStateId) =>
            new Error(
                ErrorCode.IncorrectTaskState,
                LocalizationAlias.Error.IncorrectTaskState,
                new Dictionary<string, string>
                {
                    [nameof(taskStateId)] = taskStateId.ToString()
                });
        
        /// <summary>
        /// Некорректное состояние задания.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectTaskState(TaskState taskState) =>
            new Error(
                ErrorCode.IncorrectTaskState,
                LocalizationAlias.Error.IncorrectTaskState,
                new Dictionary<string, string>
                {
                    [nameof(taskState.Id)] = taskState.Id.ToString(),
                    [nameof(taskState.Name)] = taskState.Name,
                    [nameof(taskState.Caption)] = taskState.Caption,
                });
        
        /// <summary>
        /// Некорректное переход между состояниями задания.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectTaskStateTransition(Guid id) =>
            new Error(
                ErrorCode.IncorrectTaskStateTransition,
                LocalizationAlias.Error.IncorrectTaskStateTransition,
                new Dictionary<string, string>
                {
                    [nameof(id)] = id.ToString(),
                });
        
        /// <summary>
        /// Некорректное переход между состояниями задания.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectTaskStateTransition(Guid role, Guid initialState, Guid finalState) =>
            new Error(
                ErrorCode.IncorrectTaskStateTransition,
                LocalizationAlias.Error.IncorrectTaskStateTransition,
                new Dictionary<string, string>
                {
                    [nameof(role)] = role.ToString(),
                    [nameof(initialState)] = initialState.ToString(),
                    [nameof(finalState)] = finalState.ToString(),
                });
        
        /// <summary>
        /// Некорректное переход между состояниями задания.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectTaskStateTransition(
                Guid taskId, 
                Guid taskStateId,
                Guid taskStateTransitionId, 
                Guid role) =>
            new Error(
                ErrorCode.IncorrectTaskStateTransition,
                LocalizationAlias.Error.IncorrectTaskStateTransition,
                new Dictionary<string, string>
                {
                    [nameof(taskId)] = taskId.ToString(),
                    [nameof(taskStateId)] = taskStateId.ToString(),
                    [nameof(taskStateTransitionId)] = taskStateTransitionId.ToString(),
                    [nameof(role)] = role.ToString(),
                });
        
        /// <summary>
        /// Наблюдатель отменил выполнение действия с заданием.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public static IError ObserverCancelExecution(ITaskObserver observer) =>
            new Error(
                ErrorCode.ObserverCancelExection,
                LocalizationAlias.Error.ObserverCancelExecution,
                new Dictionary<string, string>
                {
                    [nameof(observer)] = observer.GetType().Name,
                });
        
        /// <summary>
        /// Возникла ошибка при создании задания.
        /// </summary>
        /// <returns></returns>
        public static IError TaskCreationError() =>
            new Error(
                ErrorCode.TaskCreationError,
                LocalizationAlias.Error.TaskCreationError);
        
        /// <summary>
        /// Возникла ошибка при редактировании задания.
        /// </summary>
        /// <returns></returns>
        public static IError TaskEditError(Guid taskId, string comment = null) =>
            new Error(
                ErrorCode.TaskEditError,
                LocalizationAlias.Error.TaskEditError,
                new Dictionary<string, string>
                {
                    [nameof(taskId)] = taskId.ToString(),
                    [nameof(comment)] = comment ?? string.Empty
                });
        
        
        /// <summary>
        /// Задание не найдено.
        /// </summary>
        /// <returns></returns>
        public static IError TaskNotFound(Guid taskId) =>
            new Error(
                ErrorCode.TaskNotFound,
                LocalizationAlias.Error.TaskNotFound,
                new Dictionary<string, string>
                {
                    [nameof(taskId)] = taskId.ToString()
                });
        

        #endregion
    }
}