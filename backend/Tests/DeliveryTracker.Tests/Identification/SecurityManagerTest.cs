using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Tests.Identification
{
    public class SecurityManagerTest : DeliveryTrackerTestBase
    {
        private readonly ISecurityManager defaultSecurityManager;
        
        public SecurityManagerTest()
        {
            var provider = new PostgresConnectionProvider(this.Configuration);
            var tokenSettings = IdentificationHelper.ReadTokenSettingsFromConf(this.Configuration);
            var passwordSettings = IdentificationHelper.ReadPasswordSettingsFromConf(this.Configuration);
            this.defaultSecurityManager = new SecurityManager(
                provider, tokenSettings, passwordSettings);
        }


        public void CodeGeneratorUniqueEnough()
        {
            var codes = new HashSet<string>();
            
            
        }
        
        
    }
}