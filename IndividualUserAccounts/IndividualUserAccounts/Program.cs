using IndividualUserAccounts.Data;
using IndividualUserAccounts.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IndividualUserAccounts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args)
                .MigrateDbContext<ApplicationDbContext>((ctx, sp) =>
                 {
                     var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                     var logger = sp.GetRequiredService<ILogger<IdentitySeedData>>();
                     var seeder = new IdentitySeedData(userManager, logger);
                     seeder.EnsurePopulatedAsync().Wait();
                 })
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
