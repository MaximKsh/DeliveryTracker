using DeliveryTracker.Database;

namespace DeliveryTracker.Tests
{
    public abstract class DeliveryTrackerConnectionTestBase : DeliveryTrackerTestBase
    {
        protected readonly IPostgresConnectionProvider Cp;
        
        protected DeliveryTrackerConnectionTestBase() : base()
        {
            this.Cp = new PostgresConnectionProvider(this.Configuration);
        }
    }
}