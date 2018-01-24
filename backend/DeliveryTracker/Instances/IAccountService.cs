using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    /// <summary>
    /// Сервис для работы с текущим пользователем.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Регистрация нового пользователя по Коду-Паролю
        /// </summary>
        /// <param name="codePassword"></param>
        /// <param name="userModificationAction">Действие по модификации данных пользователя перед добавлением.</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<AccountServiceResult>> RegisterAsync(
            CodePassword codePassword,
            Action<User> userModificationAction = null,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Логин пользователя.
        /// </summary>
        /// <param name="codePassword"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<AccountServiceResult>> LoginAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Логин.
        /// Если логин неудачен, то поиск приглашения с таким кодом.
        /// Если приглашение есть, то оно подтверждается и пользователь регистрируется.
        /// </summary>
        /// <param name="codePassword"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<AccountServiceResult>> LoginWithRegistrationAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить данные о текущем пользователе.
        /// </summary>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<AccountServiceResult>> GetAsync(NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Редактирование данных текущего пользователя.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<AccountServiceResult>> EditAsync(
            User newData,
            NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Смена пароля текущего пользователя.
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> ChangePasswordAsync(
            string oldPassword,
            string newPassword,
            NpgsqlConnectionWrapper oc = null);
    }
}