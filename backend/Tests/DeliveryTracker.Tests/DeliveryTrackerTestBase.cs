using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Tests
{
    public abstract class DeliveryTrackerTestBase
    {
        protected readonly IConfiguration Configuration;

        protected readonly string DefaultConnectionString;

        protected readonly ISettingsStorage SettingsStorage;

        protected readonly DatabaseSettings DefaultDatabaseSettings;
        
        protected readonly TokenSettings DefaultTokenSettings;
        
        protected readonly TokenSettings DefaultRefreshTokenSettings;

        protected readonly PasswordSettings DefaultPasswordSettings;

        protected readonly InvitationSettings DefaultInvitationSettings;
        
        protected DeliveryTrackerTestBase()
        {
            
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            this.DefaultConnectionString = this.Configuration.GetConnectionString("DefaultConnection");
            
            this.SettingsStorage = new SettingsStorage();

            this.DefaultDatabaseSettings = DatabaseHelper.ReadDatabaseSettingsFromConf(this.Configuration);
            this.SettingsStorage.RegisterSettings(this.DefaultDatabaseSettings);
            
            this.DefaultRefreshTokenSettings = IdentificationHelper.ReadRefreshTokenSettingsFromConf(this.Configuration);
            this.SettingsStorage.RegisterSettings(this.DefaultRefreshTokenSettings);
            
            this.DefaultTokenSettings = IdentificationHelper.ReadTokenSettingsFromConf(this.Configuration);
            this.SettingsStorage.RegisterSettings(this.DefaultTokenSettings);
            
            this.DefaultInvitationSettings = InstanceHelper.ReadInvitationSettingsFromConf(this.Configuration);
            this.SettingsStorage.RegisterSettings(this.DefaultInvitationSettings);
            
            this.DefaultPasswordSettings = IdentificationHelper.ReadPasswordSettingsFromConf(this.Configuration);
            this.SettingsStorage.RegisterSettings(this.DefaultPasswordSettings);
        }
    }
}