using LinkShortener.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using UrlCutter.Models;

namespace UrlCutter.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IMongoDatabase mongoDatabase_database;
        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
            var connect = "mongodb+srv://faruksevinc:pasiflora45@yazlab.92ir6av.mongodb.net/?retryWrites=true&w=majority";
            var mongo = new MongoClient(connect);
            mongoDatabase_database = mongo.GetDatabase("Yazlab");
        }
        public async Task<ActionResult> Index()
        {

            var admin = HttpContext.Session.GetString("role");
            var user = HttpContext.Session.GetString("user");
            if (admin != null)
            {
                if (admin == "False")
                {
                    return RedirectToAction("LogOut", "Login");
                }
                var userList = mongoDatabase_database.GetCollection<User>("users");
                var shortlUrlList = mongoDatabase_database.GetCollection<ShortUrl>("cut-urls");
                var users = await userList.Find(Builders<User>.Filter.Eq(x => x.Role, false)).ToListAsync();
                var shortUrls = await shortlUrlList.Find(Builders<ShortUrl>.Filter.Empty).ToListAsync();
                var mostUrl = shortUrls.OrderByDescending(x => x.Click).Take(1);
                var mostUser = users.OrderByDescending(n => n.Kısaltma).Take(1);
                ViewBag.Users = users;
                ViewBag.shortUrls = shortUrls;
                dynamic infos = new ExpandoObject();
                infos.MostUrl = mostUrl;
                infos.MostUser = mostUser;
                return View(infos);
            }
            return RedirectToAction("LogOut", "Login");
        }
    }
}
