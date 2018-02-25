using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    /// <summary>
    /// Сервис для работы с инстансом
    /// </summary>
    public interface IInstanceService
    {
        /// <summary>
        /// Создать новый инстанс и зарегистрировать его создателя.
        /// </summary>
        /// <param name="instance">Информация об инстансе</param>
        /// <param name="creatorInfo">Информация о создателе</param>
        /// <param name="codePassword">Пароль создателя</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<InstanceServiceResult>> CreateAsync(string instance,
            User creatorInfo,
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить информацию об инстансе текущего пользователя.
        /// </summary>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<InstanceServiceResult>> GetAsync(
            NpgsqlConnectionWrapper oc = null);
    }
}