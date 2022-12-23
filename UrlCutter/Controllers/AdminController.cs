using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UrlCutter.Controllers
{
    public class AdminController : Controller
    {
        public async Task<ActionResult> Index()
        {
            return View();
        }
    }
}
