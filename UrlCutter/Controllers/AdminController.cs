using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UrlCutter.Controllers
{
    public class AdminController : Controller
    {
        public async Task<ActionResult> Index(string admin, string role)
        {
            admin = HttpContext.Session.GetString("user");
            role = HttpContext.Session.GetString("role");
            if (admin != null)
            {
                if (role == "True")
                {

                    return View();

                }
                return RedirectToAction("Index","Login");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}
