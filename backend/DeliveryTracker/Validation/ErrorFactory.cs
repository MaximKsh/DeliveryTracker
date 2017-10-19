using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Localization;
using DeliveryTracker.Models;
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
                LocalizationString.Error.ServerError);


        /// <summary>
        /// Неправильный входные параметры.
        /// </summary>
        /// <param name="invalidParameters"></param>
        /// <returns></returns>
        public static IError InvalidInputParameters(IEnumerable<KeyValuePair<string, object>> invalidParameters) =>
            new Error(
                ErrorCode.InvalidInputParameter,
                LocalizationString.Error.InvalidInputParameter,
                invalidParameters.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? "null"));
      
        /// <summary>
        /// Пользователь не найден.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static IError UserNotFound(string username) =>
            new Error(
                ErrorCode.UserNotFound,
                LocalizationString.Error.UserNotFound,
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
                LocalizationString.Error.UserWithoutRole,
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
                LocalizationString.Error.UserNotInRole,
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
                LocalizationString.Error.IdentityError,
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
                LocalizationString.Error.InvitationDoesnotExist,
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
                LocalizationString.Error.InvitationExpired,
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
                LocalizationString.Error.InstanceAlreadyHasCreator,
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
                LocalizationString.Error.PerformerInAnotherInstance);
        
        /// <summary>
        /// Задание не найдено.
        /// </summary>
        /// <returns></returns>
        public static IError TaskNotFound(Guid taskId) =>
            new Error(
                ErrorCode.TaskNotFound,
                LocalizationString.Error.TaskNotFound,
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
                LocalizationString.Error.IncorrectTaskState,
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
                LocalizationString.Error.IncorrectTaskStateTransition,
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
                LocalizationString.Error.TaskIsForbidden);
        
        /// <summary>
        /// Доступ запрещен.
        /// </summary>
        /// <returns></returns>
        public static IError AccessDenied() =>
            new Error(
                ErrorCode.TaskIsForbidden,
                LocalizationString.Error.AccessDenied);
        
        /// <summary>
        /// Пользователь удален.
        /// </summary>
        /// <returns></returns>
        public static IError UserDeleted(string username) =>
            new Error(
                ErrorCode.UserDeleted,
                LocalizationString.Error.UserDeleted,
                new Dictionary<string, string>
                {
                    ["username"] = username,
                });
    }
}