using DeliveryTracker.Common;
using DeliveryTracker.Services;

namespace DeliveryTracker.Instances
{
    public interface IInstanceService
    {
        ServiceResult<Instance> Create(Instance instance, User creator, LoginPassword loginPassword);
    }
}