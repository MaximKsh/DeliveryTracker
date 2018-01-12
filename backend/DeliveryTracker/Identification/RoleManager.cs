using System;
using System.Collections.Generic;

namespace DeliveryTracker.Identification
{
    public class RoleManager : IRoleManager
    {
        /// <inheritdoc />
        public void AddToRole(Guid userId, Guid roleId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IReadOnlyList<Role> GetUserRoles(Guid userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IReadOnlyList<User> GetUsersInRole(Guid roleId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RemoveFromRole(Guid userId, Guid roleId)
        {
            throw new NotImplementedException();
        }
    }
}