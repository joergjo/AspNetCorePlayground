using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace WebListener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseWebListener()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseUrls("http://localhost:5001")
                .Build();

            host.Run();
        }
    }
}
