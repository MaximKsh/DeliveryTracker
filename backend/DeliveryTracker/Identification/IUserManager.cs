using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Identification
{
    public interface IUserManager
    {
        /// <summary>
        /// Создать нового пользователя с указанными данными.
        /// </summary>
        /// <param name="user">Данные о добавляемом пользователе</param>
        /// <param name="oc">Открытое соединение с базой. Может быть null</param>
        /// <returns>Созданный объект пользователя</returns>
        Task<ServiceResult<User>> CreateAsync(User user, NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Обновить данные о пользователе с указанным id.
        /// </summary>
        /// <param name="user">
        /// Данные для изменения.
        /// Учитываются только Surname, Name, Patronymic, PhoneNumber
        /// </param>
        /// <param name="oc">Открытое соединение с базой. Может быть null.</param>
        /// <returns></returns>
        Task<ServiceResult<User>> EditAsync(User user, NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить пользователя из инстанса по id.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="instanceId">Идентификатор инстанса.</param>
        /// <param name="oc">Открытое соединение с базой</param>
        /// <returns>Пользователь или список ошибок.</returns>
        Task<ServiceResult<User>> GetAsync(Guid userId, Guid instanceId, NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Получить пользователя из инстанса по коду.
        /// </summary>
        /// <param name="code">Код пользователя.</param>
        /// <param name="instanceId">Идентификатор инстанса.</param>
        /// <param name="oc">Открытое соединение с базой</param>
        /// <returns>Пользователь или список ошибок.</returns>
        Task<ServiceResult<User>> GetAsync(string code, Guid instanceId, NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Получить ID пользователя по коду.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="instanceId"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<Guid?> GetIdAsync(string code, Guid instanceId, NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Удаление пользователя.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="instanceId"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAsync(Guid userId, Guid instanceId, NpgsqlConnectionWrapper oc = null);
    }
}