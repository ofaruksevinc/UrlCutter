using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using UrlCutter.Models;

namespace LinkShortener.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ILogger<SignUpController> _logger;
        private readonly IMongoDatabase mongoDatabase_database;
        private readonly IConfiguration _configuration;

        public SignUpController(ILogger<SignUpController> logger)
        {

            _logger = logger;
            var connect = "mongodb+srv://faruksevinc:pasiflora45@yazlab.92ir6av.mongodb.net/?retryWrites=true&w=majority";
            var mongo = new MongoClient(connect);
            mongoDatabase_database = mongo.GetDatabase("Yazlab");
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            var userCollection = mongoDatabase_database.GetCollection<User>("users");
            var info = await userCollection.AsQueryable().FirstOrDefaultAsync(z => z.Name == user.Name);
            if (info == null)
            {
                info = new User
                {
                    Name = user.Name,
                    pass = user.pass,
                    Role = false,
                    Kısaltma = 0
                };
                await userCollection.InsertOneAsync(info);
                return RedirectToAction("Index", "Login");
            }
            else
            {
                TempData["ErrorMes"] = "Kullanıcı adı kayıtlı.";
                return View();
            }
        }
    }
}