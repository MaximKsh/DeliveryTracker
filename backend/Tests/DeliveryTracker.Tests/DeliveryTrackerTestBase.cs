using System;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Tests
{
    public abstract class DeliveryTrackerTestBase
    {
        protected readonly IConfiguration Configuration;

        protected readonly string DefaultConnectionString;
        
        protected DeliveryTrackerTestBase()
        {
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            this.DefaultConnectionString = this.Configuration.GetConnectionString("DefaultConnection");
        }
    }
}