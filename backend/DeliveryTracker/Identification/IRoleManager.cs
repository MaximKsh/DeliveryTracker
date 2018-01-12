using System;
using System.Collections.Generic;

namespace DeliveryTracker.Identification
{
    public interface IRoleManager
    {
        void AddToRole(Guid userId, Guid roleId);

        IReadOnlyList<Role> GetUserRoles(Guid userId);

        IReadOnlyList<User> GetUsersInRole(Guid roleId);

        void RemoveFromRole(Guid userId, Guid roleId);
    }
}