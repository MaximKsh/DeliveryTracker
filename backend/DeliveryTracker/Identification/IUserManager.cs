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

        Task<ServiceResult<User>> GetAsync(Guid userId, NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult<User>> GetAsync(string code, NpgsqlConnectionWrapper oc = null);
        
        Task<Guid> GetIdAsync(string code, NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null);
    }
}