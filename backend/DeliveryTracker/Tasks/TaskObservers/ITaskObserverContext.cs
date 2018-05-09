using System.Collections.Generic;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Tasks.TaskObservers
{
    public interface ITaskObserverContext
    {
        /// <summary>
        /// Изменения в задании, переданные на сохранение.
        /// </summary>
        TaskInfo TaskChanges { get; }
        
        /// <summary>
        /// Информация о задании.
        /// </summary>
        TaskInfo TaskInfo { get; }

        /// <summary>
        /// Информация о переходе.
        /// </summary>
        TaskStateTransition Transition { get; }
        
        /// <summary>
        /// Данные текущего пользователя.
        /// </summary>
        UserCredentials Credentials { get; }
        
        /// <summary>
        /// Подключение к бд.
        /// </summary>
        NpgsqlConnectionWrapper ConnectionWrapper { get; }
        
        bool Cancel { get; set; }
        
        IList<IError> Errors { get; }
    }
}