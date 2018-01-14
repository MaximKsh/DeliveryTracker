﻿using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public interface IInvitationManager
    {
        /// <summary>
        /// Сформировать код без проверки на повторения.
        /// </summary>
        /// <returns></returns>
        string GenerateCode();

        /// <summary>
        /// Сформировать новый код приглашения с проверкой с уже существующими в базе
        /// </summary>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<string> GenerateUniqueCode(NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Создать новое приглашение
        /// </summary>
        /// <param name="roleId">ID роли, на которую создается приглашение.</param>
        /// <param name="preliminaryUserData">Предварительные данные о пользователе</param>
        /// <param name="oc">Открытое подключение к бд</param>
        /// <returns></returns>
        Task<ServiceResult<Invitation>> Create(
            Guid roleId,
            User preliminaryUserData, 
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Загрузить приглашение по коду.
        /// </summary>
        /// <param name="invitationCode"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<Invitation>> Get(
            string invitationCode,
            NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Удалить приглашение с указанным кодом.
        /// </summary>
        /// <param name="invitationCode">Код приглашения, которое необходимо удалить</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> Delete(
            string invitationCode, 
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Очистить все устаревшие приглашение.
        /// Может быть долгой операцией.
        /// </summary>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAllExpired(NpgsqlConnectionWrapper oc = null);
    }
}