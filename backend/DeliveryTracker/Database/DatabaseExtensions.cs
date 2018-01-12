using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Database
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDeliveryTrackerDatabase(this IServiceCollection services)
        {
            services
                .AddSingleton<IPostgresConnectionProvider, PostgresConnectionProvider>()
                ;

            return services;
        }
    }
}
