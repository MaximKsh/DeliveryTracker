﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public interface IView
    {        
        /// <summary>
        /// Название представления.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Список ролей, для которых доступно представление.
        /// </summary>
        IReadOnlyList<Guid> PermittedRoles { get; }

        /// <summary>
        /// Получить дайджест представления.
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="userCredentials"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ServiceResult<ViewDigest>> GetViewDigestAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters);

        /// <summary>
        /// Получить результат представления.
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="userCredentials"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters);


        /// <summary>
        /// Получить количество записей в представлении.
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="userCredentials"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters);
    }
}