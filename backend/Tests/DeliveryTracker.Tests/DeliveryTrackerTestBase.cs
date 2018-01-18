using System;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Tests
{
    public abstract class DeliveryTrackerTestBase
    {
        protected readonly IConfiguration Configuration;

        protected readonly string DefaultConnectionString;

        protected readonly TokenSettings DefaultTokenSettings;

        protected readonly PasswordSettings DefaultPasswordSettings;

        protected readonly InvitationSettings DefaultInvitationSettings;
        
        protected DeliveryTrackerTestBase()
        {
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            this.DefaultConnectionString = this.Configuration.GetConnectionString("DefaultConnection");
            
            this.DefaultTokenSettings = IdentificationHelper.ReadTokenSettingsFromConf(this.Configuration);
            this.DefaultInvitationSettings = InstanceHelper.ReadInvitationSettingsFromConf(this.Configuration);
            this.DefaultPasswordSettings = IdentificationHelper.ReadPasswordSettingsFromConf(this.Configuration);
        }
    }
}