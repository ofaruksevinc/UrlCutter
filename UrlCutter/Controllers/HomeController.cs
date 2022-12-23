using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UrlCutter.Models;

namespace UrlCutter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoDatabase mongoDatabase_database;
        private const string Link = "https://localhost:4000";

        public HomeController(ILogger<HomeController> logger)
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
        [HttpGet]
        public async Task<IActionResult> Index(string rndch)
        {
            var cutUrlList = mongoDatabase_database.GetCollection<ShortUrl>("cut-urls");
            var cutUrl = await cutUrlList.AsQueryable().FirstOrDefaultAsync(f => f.RandomChar == rndch);

            if (cutUrl == null)
            {
                return View();
            }
            return Redirect(cutUrl.OrgUrl);
        }
        [HttpPost]
        public async Task<IActionResult> CutUrl(string orgUrl, string unqCh, int endTime)
        {
            var cutUrlList = mongoDatabase_database.GetCollection<ShortUrl>("cut-urls");
            var orgUrlList = mongoDatabase_database.GetCollection<OriginalUrl>("org-urls");

            var cutUrl = await cutUrlList
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.OrgUrl == orgUrl);
            var originalUrl = await orgUrlList.AsQueryable().FirstOrDefaultAsync(x => x.Url == orgUrl);
            if (cutUrl == null)
            {
                string shortCode = GetRandomAlphanumericString();
                if (unqCh == null)
                {
                    cutUrl = new ShortUrl
                    {
                        CreatedTime = DateTime.UtcNow,
                        OrgUrl = orgUrl,
                        RandomChar = shortCode,
                        UniqueChar = null,
                        EndTime = DateTime.UtcNow.AddDays(endTime),
                        CutUrl = $"{Link}/{shortCode}"
                        //UserId = cutUrl.UserId
                    };
                    originalUrl = new OriginalUrl
                    {
                        Url = orgUrl,
                        Click = 1
                    };
                }
                else
                {
                    cutUrl = new ShortUrl
                    {
                        CreatedTime = DateTime.UtcNow,
                        OrgUrl = orgUrl,
                        RandomChar = shortCode,
                        UniqueChar = unqCh,
                        EndTime = DateTime.UtcNow.AddDays(endTime),
                        CutUrl = $"{Link}/{unqCh}"
                    };
                    originalUrl = new OriginalUrl
                    {
                        Url = orgUrl,
                        Click = 1
                    };
                }

                await cutUrlList.InsertOneAsync(cutUrl);
                await orgUrlList.InsertOneAsync(originalUrl);
            }
            else
            {
                originalUrl.Click += 1;
                var filter = Builders<OriginalUrl>.Filter.Eq(s => s.Url, cutUrl.OrgUrl);
                var update = Builders<OriginalUrl>.Update.Set(s => s.Click, originalUrl.Click);
                orgUrlList.UpdateOneAsync(filter, update);
            }

            return View(cutUrl);
        }
        public static string GetRandomAlphanumericString()
        {
            Random random = new Random();
            const string uniqCode =
                "ABCDEFGHIJKLMNOPRSTUVYZ" +
                "abcdefghijklmnoprstuvyz" +
                "0123456789";
            return new string(uniqCode.Select(c => uniqCode[random.Next(uniqCode.Length)]).Take(6).ToArray());
        }
    }
}
