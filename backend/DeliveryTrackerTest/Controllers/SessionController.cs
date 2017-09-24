using System.Net.Http;
using System.Threading.Tasks;
using DeliveryTracker;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace DeliveryTrackerTest.Controllers
{
    public class SessionController
    {
        private readonly TestServer server;
        private readonly HttpClient client;
        public SessionController()
        {
            // Arrange
            this.server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            this.client = this.server.CreateClient();
        }

        [Fact]
        public async Task IsServerAvailable()
        {
            var response = await this.client.GetAsync("/");
            response.EnsureSuccessStatusCode();
        }
        
//        
//        [Fact]
//        public async Task ReturnHelloWorld()
//        {
//            // Act
//            var response = await this.client.GetAsync("/");
//            response.EnsureSuccessStatusCode();
//
//            var responseString = await response.Content.ReadAsStringAsync();
//
//            // Assert
//            Assert.Equal("Hello World!",
//                responseString);
//        }
    }
}