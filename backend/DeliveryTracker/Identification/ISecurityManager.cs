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
        /// Выписать токен для пользователя по предоставленным Credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        string AcquireToken(UserCredentials credentials);
    }
}