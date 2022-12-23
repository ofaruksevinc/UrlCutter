using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UrlCutter.Controllers
{
    public class UserController : Controller
    {
        public async Task<ActionResult> Index()
        {
            return View();
        }
    }
}
