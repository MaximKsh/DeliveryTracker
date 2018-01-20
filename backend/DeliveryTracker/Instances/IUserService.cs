using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public interface IUserService
    {
        /// <summary>
        /// Получить пользователя текущего инстанса.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<User>> GetAsync(Guid userId, NpgsqlConnectionWrapper oc = null);
               
        /// <summary>
        /// Получить пользователя текущего инстанса.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<User>> GetAsync(string code, NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Редактировать пользователя текущего инстанса.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<User>> EditAsync(User newData, NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Удалить пользователя текущего инстанса.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null);
    }
}