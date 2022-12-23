using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlCutter.Models;

namespace UrlCutter.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IMongoDatabase mongoDatabase_database;
        private readonly IMongoCollection<User> _user;

        public LoginController(ILogger<LoginController> logger, IMongoDatabase mongoDatabase_database, IMongoCollection<User> user)
        {
            _logger = logger;
            var connect = "mongodb+srv://faruksevinc:pasiflora45@yazlab.92ir6av.mongodb.net/?retryWrites=true&w=majority";
            var mongo = new MongoClient(connect);
            mongoDatabase_database = mongo.GetDatabase("Yazlab");
        }
        public async Task<ActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            var userList = mongoDatabase_database.GetCollection<User>("users");
            var user_info = await userList.AsQueryable().FirstOrDefaultAsync(a => a.Name == user.Name && a.pass == user.pass);
            if (user_info != null)
            {
                HttpContext.Session.SetString("role", user.Role.ToString());
                HttpContext.Session.SetString("user", user.Name.ToString());
                HttpContext.Session.SetString("password", user.pass.ToString());
                var userClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user_info.Name)
                };
                var identity = new ClaimsIdentity(userClaim,"login");
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}
