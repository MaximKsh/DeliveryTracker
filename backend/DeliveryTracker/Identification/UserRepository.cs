using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace DeliveryTracker.Identification
{
    public class UserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> users = new Dictionary<Guid, User>();

        public ServiceResult<User> Create(User user, NpgsqlConnectionWrapper outerConnection = null)
        {
            this.users[user.Id] = user;
            return new ServiceResult<User>(user);
        }


        /*
        /// <inheritdoc />
        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
        */
    }
}