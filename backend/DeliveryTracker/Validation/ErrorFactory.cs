using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.DbModels;
using DeliveryTracker.Localization;
using Microsoft.AspNetCore.Identity;

namespace DeliveryTracker.Validation
{
    public static class ErrorFactory
    {
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
        /// <param name="invalidParameters"></param>
        /// <returns></returns>
        public static IError InvalidInputParameters(IEnumerable<KeyValuePair<string, object>> invalidParameters) =>
            new Error(
                ErrorCode.InvalidInputParameter,
                LocalizationAlias.Error.InvalidInputParameter,
                invalidParameters.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? "null"));
      
        /// <summary>
        /// Пользователь не найден.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static IError UserNotFound(string username) =>
            new Error(
                ErrorCode.UserNotFound,
                LocalizationAlias.Error.UserNotFound,
                new Dictionary<string, string>
                {
                    ["username"] = username,
                });
        
        /// <summary>
        /// Для пользователя не найдена роль.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static IError UserWithoutRole(string username) =>
            new Error(
                ErrorCode.UserWithoutRole,
                LocalizationAlias.Error.UserWithoutRole,
                new Dictionary<string, string>
                {
                    ["username"] = username,
                });

        /// <summary>
        /// Пользователь не состоит в требуемой роли.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static IError UserNotInRole(string username, string expected) =>
            new Error(
                ErrorCode.UserNotInRole,
                LocalizationAlias.Error.UserNotInRole,
                new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["expected"] = expected,
                });

        /// <summary>
        /// Ошибка, возникшая в Identity.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IError IdentityError(IdentityError error) =>
            new Error(
                ErrorCode.IdentityError,
                LocalizationAlias.Error.IdentityError,
                new Dictionary<string, string>
                {
                    ["details"] = error.Description,
                });
        
        /// <summary>
        /// Указанное приглашение не существует.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static IError InvitationDoesNotExist(string invitationCode, string role) =>
            new Error(
                ErrorCode.InvitationDoesnotExist,
                LocalizationAlias.Error.InvitationDoesnotExist,
                new Dictionary<string, string>
                {
                    ["invitationCode"] = invitationCode,
                    ["role"] = role
                });

        /// <summary>
        /// Указанное приглашение просрочено.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <returns></returns>
        public static IError InvitaitonExpired(string invitationCode) =>
            new Error(
                ErrorCode.InvitationExpired,
                LocalizationAlias.Error.InvitationExpired,
                new Dictionary<string, string>
                {
                    ["invitationCode"] = invitationCode
                });
        
        /// <summary>
        /// У инстанса уже существует создатель. Повторно указать создателя нельзя.
        /// </summary>
        /// <param name="instanceDisplayableName"></param>
        /// <returns></returns>
        public static IError InstanceAlreadyHasCreator(string instanceDisplayableName) =>
            new Error(
                ErrorCode.InstanceAlreadyHasCreator,
                LocalizationAlias.Error.InstanceAlreadyHasCreator,
                new Dictionary<string, string>
                {
                    ["instanceDisplayableName"] = instanceDisplayableName,
                });
        
        /// <summary>
        /// Указанный исполнитель находится в другом инстансе.
        /// </summary>
        /// <returns></returns>
        public static IError PerformerInAnotherInstance() =>
            new Error(
                ErrorCode.PerformerInAnotherInstance,
                LocalizationAlias.Error.PerformerInAnotherInstance);
        
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
                    ["taskId"] = taskId.ToString()
                });
        
        /// <summary>
        /// Некорректное состояние задания.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectTaskState(TaskStateModel actual, params TaskStateModel[] expected) =>
            new Error(
                ErrorCode.IncorrectTaskState,
                LocalizationAlias.Error.IncorrectTaskState,
                new Dictionary<string, string>
                {
                    ["actual"] = actual?.Alias ?? "null",
                    ["expected"] = string.Join(',', expected.Select(p => p.Alias)),
                });
        
        /// <summary>
        /// Некорректное переход между состояниями задания.
        /// </summary>
        /// <returns></returns>
        public static IError IncorrectTaskStateTransition(
            TaskStateModel newState,
            TaskStateModel currentState) =>
            new Error(
                ErrorCode.IncorrectTaskStateTransition,
                LocalizationAlias.Error.IncorrectTaskStateTransition,
                new Dictionary<string, string>
                {
                    ["newState"] = newState?.Alias ?? "null",
                    ["currentState"] = currentState?.Alias ?? "null",
                });
        
        /// <summary>
        /// Нет доступа к заданию.
        /// </summary>
        /// <returns></returns>
        public static IError TaskIsForbidden() =>
            new Error(
                ErrorCode.TaskIsForbidden,
                LocalizationAlias.Error.TaskIsForbidden);
        
        /// <summary>
        /// Доступ запрещен.
        /// </summary>
        /// <returns></returns>
        public static IError AccessDenied() =>
            new Error(
                ErrorCode.TaskIsForbidden,
                LocalizationAlias.Error.AccessDenied);
        
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
    }
}