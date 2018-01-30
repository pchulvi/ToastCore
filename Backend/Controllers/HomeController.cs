using Backend.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{
    public class HomeController : Controller
    {
        public IConfigurationRoot Configuration { get; }
        public IOptions<ApplicationConfigurations> OptionsApplicationConfiguration { get; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "The best toaster for the future testers: IToast";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Support IToast with your cheerings (money will be welcome too)";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
