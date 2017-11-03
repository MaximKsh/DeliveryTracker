using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DeliveryTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var listenIndex = args.ToList().IndexOf("--listen");
            var urls = "http://localhost:5000";
            if (listenIndex != -1
                && listenIndex + 1 < args.Length)
            {
                urls = args[listenIndex + 1];
            }
            BuildWebHost(args, urls).Run();
        }

        public static IWebHost BuildWebHost(string[] args, string urls) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(urls)
                .Build();
    }
}
