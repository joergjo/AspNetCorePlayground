using Microsoft.AspNetCore.Mvc;

namespace AzureCDN.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult About() => View();

        public IActionResult Error() => View();
    }
}
