using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Identification
{
    public interface IUserRepository
    {
        ServiceResult<User> Create(User user, NpgsqlConnectionWrapper outerConnection = null);
    }
}