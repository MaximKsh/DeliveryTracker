using System.Collections.Generic;
using DeliveryTracker.Common;

namespace DeliveryTracker.Views
{
    /// <summary>
    /// Сервис для обращения к группам представлений.
    /// </summary>
    public interface IViewService
    {
        /// <summary>
        /// Получить список групп.
        /// </summary>
        /// <returns></returns>
        ServiceResult<IList<string>> GetViewGroupsList();
        
        /// <summary>
        /// Получить группу представлений.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        ServiceResult<IViewGroup> GetViewGroup(string groupName);
    }
}