using Microsoft.AspNetCore.Mvc;

namespace WebListener.Controllers
{
    public class HomeController : Controller
    {
        public ViewResult Index() => View();

        public ViewResult About() => View();

        public ViewResult Error() => View();
    }
}
