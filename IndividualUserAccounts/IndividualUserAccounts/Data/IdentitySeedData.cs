using IndividualUserAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndividualUserAccounts.Data
{
    public class IdentitySeedData
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private static readonly IEnumerable<string> _userNames = new[]
        {
            "joerg@joergjooss.de",
            "anne@joergjooss.de",
            "jonah@joergjooss.de",
            "paul@joergjooss.de"
        };

        private const string InitialPassword = "Pass@word1";

        public IdentitySeedData(UserManager<ApplicationUser> userManager, ILogger<IdentitySeedData> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task EnsurePopulatedAsync()
        {
            _logger.LogInformation("Seeding identity store...");
            foreach (string userName in _userNames)
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    user = new ApplicationUser(userName);
                    await _userManager.CreateAsync(user, InitialPassword);
                }
            }
            _logger.LogInformation("Identity store setup completed.");
        }
    }
}