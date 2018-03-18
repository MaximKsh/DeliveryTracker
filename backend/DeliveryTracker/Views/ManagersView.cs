﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using Npgsql;

namespace DeliveryTracker.Views
{
    public class ManagersView : IView
    {
        #region sql
        
        private static readonly string SqlGet = $@"
select
    {IdentificationHelper.GetUserColumns()}
from users
where instance_id = @instance_id 
    and (role = @role or role = @role_creator)
    and deleted = false
    {{0}}

order by surname
limit {ViewHelper.DefaultViewLimit}
;
";

        private const string SqlCount = @"
select managers_count
from entries_statistics
where instance_id = @instance_id
;
";
        #endregion
        
        
        #region fields
        
        private readonly int order;
        
        #endregion
        
        #region constuctor
        
        public ManagersView(
            int order)
        {
            this.order = order;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(ManagersView);
        
        /// <inheritdoc />
        public IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole
        }.AsReadOnly();

        /// <inheritdoc />
        public async Task<ServiceResult<ViewDigest>> GetViewDigestAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var result = await this.GetCountAsync(oc, userCredentials, parameters);
            if (!result.Success)
            {
                return new ServiceResult<ViewDigest>(result.Errors);
            }
            return new ServiceResult<ViewDigest>(new ViewDigest
            {
                Caption = LocalizationAlias.Views.ManagersView,
                Count = result.Result,
                EntityType = nameof(User),
                Order = this.order,
                IconName = "1111"
            });
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var list = new List<IDictionaryObject>();
            using (var command = oc.CreateCommand())
            {
                var sb = new StringBuilder(256);
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                command.Parameters.Add(new NpgsqlParameter("role_creator", DefaultRoles.CreatorRole));
                command.Parameters.Add(new NpgsqlParameter("role", DefaultRoles.ManagerRole));
                ViewHelper.TryAddCaseInsensetiveContainsParameter(parameters, command, sb, "search");
                ViewHelper.TryAddAfterParameter(parameters, command, sb, "users", "surname");
                command.CommandText = string.Format(SqlGet, sb);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetUser());
                    }
                }
            }
            
            return new ServiceResult<IList<IDictionaryObject>>(list);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlCount;
                command.Parameters.Add(new NpgsqlParameter("instance_id", userCredentials.InstanceId));
                return new ServiceResult<long>((long)await command.ExecuteScalarAsync() + 1);
            }
        }
        
        #endregion
    }
}