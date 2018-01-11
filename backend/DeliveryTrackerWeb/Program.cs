using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DeliveryTrackerWeb
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json")
                .AddCommandLine(args)
                .Build();
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(config)
                .Build();
            
        }
    }
}