using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using UrlCutter.Models;

namespace LinkShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoDatabase mongoDatabase_database;
        private const string ServiceUrl = "https://localhost:4000";
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            var connect = "mongodb+srv://faruksevinc:pasiflora45@yazlab.92ir6av.mongodb.net/?retryWrites=true&w=majority";
            var mongo = new MongoClient(connect);
            mongoDatabase_database = mongo.GetDatabase("Yazlab");
        }
       // [Authorize]

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string u)
        {
            var userUrlList = mongoDatabase_database.GetCollection<ShortUrl>("shortUrls");
            var userUrl = userUrlList.AsQueryable().Where(x => x.user == HttpContext.Session.GetString("user"));
            var ShortUrlList = mongoDatabase_database.GetCollection<ShortUrl>("shortUrls");
            var ShortUrl = await ShortUrlList
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.UniqueChar == u);

            if (ShortUrl == null)
            {
                ViewBag.UserUrls = userUrl;
                return View();
            }
            return Redirect(ShortUrl.OrgUrl);
        }

        [HttpPost]
        public async Task<IActionResult> CutUrl(string OrgUrl, string rndChar, int EndTime, string user)
        {
            //user = HttpContext.Session.GetString("user");
            var ShortUrlList = mongoDatabase_database.GetCollection<ShortUrl>("cut-urls");
            var originalUrlColleciton = mongoDatabase_database.GetCollection<OriginalUrl>("org-urls");

            var ShortUrl = await ShortUrlList
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.OrgUrl == OrgUrl);
            var originalUrl = await originalUrlColleciton.AsQueryable().FirstOrDefaultAsync(x => x.Url == OrgUrl);

            if (ShortUrl != null)
            {
                if (ShortUrl.user != user)
                {
                    string uniqueChar = GetRandomAlphanumericString();
                    if (rndChar == null)
                    {
                        ShortUrl = new ShortUrl
                        {
                            CreatedTime = DateTime.UtcNow,
                            OrgUrl = OrgUrl,
                            UniqueChar = uniqueChar,
                            RandomChar = "",
                            EndTime = DateTime.UtcNow.AddDays(EndTime),
                            CutUrl = $"{ServiceUrl}/{uniqueChar}",
                            user = user
                        };


                    }
                    else
                    {
                        ShortUrl = new ShortUrl
                        {
                            CreatedTime = DateTime.UtcNow,
                            OrgUrl = OrgUrl,
                            UniqueChar = uniqueChar,
                            RandomChar = rndChar,
                            EndTime = DateTime.UtcNow.AddDays(EndTime),
                            CutUrl = $"{ServiceUrl}/{uniqueChar}",
                            user = user
                        };
                    }

                    originalUrl.Click += 1;
                    var filter = Builders<OriginalUrl>.Filter.Eq(s => s.Url, ShortUrl.OrgUrl);
                    var update = Builders<OriginalUrl>.Update.Set(s => s.Click, originalUrl.Click);
                    originalUrlColleciton.UpdateOneAsync(filter, update);
                    await ShortUrlList.InsertOneAsync(ShortUrl);
                }
                else
                {
                    originalUrl.Click += 1;
                    var filter = Builders<OriginalUrl>.Filter.Eq(s => s.Url, ShortUrl.OrgUrl);
                    var update = Builders<OriginalUrl>.Update.Set(s => s.Click, originalUrl.Click);
                    originalUrlColleciton.UpdateOneAsync(filter, update);
                }
            }
            else
            {
                string uniqueChar = GetRandomAlphanumericString();
                if (rndChar == null)
                {
                    ShortUrl = new ShortUrl
                    {
                        CreatedTime = DateTime.UtcNow,
                        OrgUrl = OrgUrl,
                        UniqueChar = uniqueChar,
                        RandomChar = "",
                        EndTime = DateTime.UtcNow.AddDays(EndTime),
                        CutUrl = $"{ServiceUrl}/{uniqueChar}",
                        user = "atayaz"
                    };
                    originalUrl = new OriginalUrl
                    {
                        Url = OrgUrl,
                        Click = 1
                    };

                }
                else
                {
                    ShortUrl = new ShortUrl
                    {
                        CreatedTime = DateTime.UtcNow,
                        OrgUrl = OrgUrl,
                        UniqueChar = uniqueChar,
                        RandomChar = rndChar,
                        EndTime = DateTime.UtcNow.AddDays(EndTime),
                        CutUrl = $"{ServiceUrl}/{uniqueChar}",
                        user = "atayaz"
                    };
                    originalUrl = new OriginalUrl
                    {
                        Url = OrgUrl,
                        Click = 1
                    };
                }
                await originalUrlColleciton.InsertOneAsync(originalUrl);
                await ShortUrlList.InsertOneAsync(ShortUrl);
            }

            return View(ShortUrl);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public static string GetRandomAlphanumericString()
        {
            Random random = new Random();
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";
            return new string(alphanumericCharacters.Select(c => alphanumericCharacters[random.Next(alphanumericCharacters.Length)]).Take(6).ToArray());
        }
    }
}
