using System.Data;
using DeliveryTracker.Identification;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Instances
{
    public static class InstanceExtensions
    {
        public static IServiceCollection AddDeliveryTrackerInstances(this IServiceCollection services)
        {
            services
                .AddSingleton<IAccountService, AccountService>()
                .AddSingleton<IInstanceService, InstanceService>();
            
            return services;
        }
        
        
        public static Instance GetInstance(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetInstance(ref idx);
        }
        
        public static Instance GetInstance(this IDataReader reader, ref int idx)
        {
            return new Instance(
                reader.GetGuid(idx++),
                reader.GetString(idx++),
                reader.GetGuid(idx++));
        }
    }
}