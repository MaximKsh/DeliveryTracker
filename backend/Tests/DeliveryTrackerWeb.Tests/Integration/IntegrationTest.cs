using System.Net;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTrackerWeb.ViewModels;
using Xunit;

namespace DeliveryTrackerWeb.Tests.Integration
{
    public class IntegrationTest : FunctionalTestBase
    {


        [Fact]
        public async void WebServiceAlive()
        {
            var client = this.Server.CreateClient();
            var result = await RequestGet(client, ServiceUrl(""));
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
        
        
        [Fact]
        public async void CreateInstance()
        {
            var client = this.Server.CreateClient();
            var result = await RequestPost<InstanceResponse>(
                client, 
                InstanceUrl("create"),
                new InstanceRequest
                {
                    Creator = new User
                    {
                        Surname = "Petrov",
                        Name = "Ivan",
                        PhoneNumber = "+89123456789"
                    },
                    CodePassword = new CodePassword
                    {
                        Password = CorrectPassword
                    },
                    Instance = new Instance
                    {
                        Name = CorrectInstanceName,
                    }
                });
            
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}