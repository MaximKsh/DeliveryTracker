using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public interface IInstanceService
    {
        Task<ServiceResult<Instance>> CreateAsync(Instance instance, User creator, UsernamePassword usernamePassword);
    }
}