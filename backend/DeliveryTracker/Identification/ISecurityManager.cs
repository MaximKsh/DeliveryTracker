using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Identification
{
    public interface ISecurityManager
    {
        /// <summary>
        /// Проверить пару Код-Пароль.
        /// Если пользователь с такой парой есть, то вернуть Credentials для него.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="password"></param>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult<UserCredentials>> ValidatePasswordAsync(
            string code,
            string password, 
            NpgsqlConnectionWrapper outerConnection = null);

        /// <summary>
        /// Проверка пароля для пользователя по Id.
        /// Если пользователь с такой парой есть, то вернуть Credentials для него.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult<UserCredentials>> ValidatePasswordAsync(
            Guid userId,
            string password, 
            NpgsqlConnectionWrapper outerConnection = null);
        
        /// <summary>
        /// Сменить пароль пользователя.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newPassword"></param>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult<UserCredentials>> SetPasswordAsync(
            Guid userId, 
            string newPassword,
            NpgsqlConnectionWrapper outerConnection = null);

        /// <summary>
        /// Создать новую сессию.
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult<Session>> NewSessionAsync(
            UserCredentials credentials,
            NpgsqlConnectionWrapper outerConnection = null);

        /// <summary>
        /// Обновить сессию по долгоживущему токену.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult<Session>> RefreshSessionAsync(
            string refreshToken,
            NpgsqlConnectionWrapper outerConnection = null);

        /// <summary>
        /// Завершить сессию.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult> LogoutAsync(
            Guid userId,
            NpgsqlConnectionWrapper outerConnection = null);

        /// <summary>
        /// Проверить, существует ли указанная сессия
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionTokenId"></param>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult> HasSession(
            Guid userId,
            Guid sessionTokenId,
            NpgsqlConnectionWrapper outerConnection = null);

        /// <summary>
        /// Удалить все просроченные сессии.
        /// </summary>
        /// <param name="outerConnection"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAllExpiredAsync(
            NpgsqlConnectionWrapper outerConnection = null);
    }
}