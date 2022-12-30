using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        [HttpGet]
        public async Task<IActionResult> Index(string u)
        {
            u = HttpContext.Session.GetString("user");
            var userUrlList = mongoDatabase_database.GetCollection<ShortUrl>("cut-urls");
            var userUrl = userUrlList.AsQueryable().Where(x => x.user == HttpContext.Session.GetString("user"));
            var ShortUrlList = mongoDatabase_database.GetCollection<ShortUrl>("cut-urls");
            var ShortUrl = await ShortUrlList
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.UniqueChar == u);
            if (u != null)
            {
                    ViewBag.UserUrls = userUrl;
                    return View();
            }
            return RedirectToAction("Index","Login");
        }

        [HttpPost]
        public async Task<IActionResult> CutUrl(string OrgUrl, string uniqueChar, int EndTime, string user)
        {
            user = HttpContext.Session.GetString("user");
            var ShortUrlList = mongoDatabase_database.GetCollection<ShortUrl>("cut-urls");
            var originalUrlList = mongoDatabase_database.GetCollection<OriginalUrl>("org-urls");
            var userList = mongoDatabase_database.GetCollection<User>("users");

            var ShortUrl = await ShortUrlList
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.OrgUrl == OrgUrl);
            var originalUrl = await originalUrlList.AsQueryable().FirstOrDefaultAsync(x => x.Url == OrgUrl);
            var cr_user = await userList.AsQueryable().FirstOrDefaultAsync(x => x.Name == user);

            if (ShortUrl != null)
            {
                if (ShortUrl.user != user)
                {
                    string rndChar = GetRandomAlphanumericString();
                    if (uniqueChar == null)
                    {
                        ShortUrl = new ShortUrl
                        {
                            CreatedTime = DateTime.UtcNow,
                            OrgUrl = OrgUrl,
                            RandomChar = rndChar,
                            UniqueChar = "",
                            EndTime = DateTime.UtcNow.AddDays(EndTime),
                            CutUrl = $"{ServiceUrl}/{rndChar}",
                            user = user,
                            Click = 1
                        };
                        cr_user.Kısaltma += 1;
                        var filter = Builders<User>.Filter.Eq(s => s.Name, user);
                        var update = Builders<User>.Update.Set(s => s.Kısaltma, cr_user.Kısaltma);
                        userList.UpdateOneAsync(filter, update);
                        await ShortUrlList.InsertOneAsync(ShortUrl);


                    }
                    else
                    {
                        ShortUrl = new ShortUrl
                        {
                            CreatedTime = DateTime.UtcNow,
                            OrgUrl = OrgUrl,
                            RandomChar = rndChar,
                            UniqueChar = uniqueChar,
                            EndTime = DateTime.UtcNow.AddDays(EndTime),
                            CutUrl = $"{ServiceUrl}/{uniqueChar}",
                            user = user,
                            Click = 1
                        };
                        cr_user.Kısaltma += 1;
                        var filter = Builders<User>.Filter.Eq(s => s.Name, user);
                        var update = Builders<User>.Update.Set(s => s.Kısaltma, cr_user.Kısaltma);
                        userList.UpdateOneAsync(filter, update);
                    }
                    await ShortUrlList.InsertOneAsync(ShortUrl);
                }
                else
                {
                    cr_user.Kısaltma += 1;
                    var filter = Builders<User>.Filter.Eq(s => s.Name, user);
                    var update = Builders<User>.Update.Set(s => s.Kısaltma, cr_user.Kısaltma);
                    userList.UpdateOneAsync(filter, update);
                }
            }
            else
            {
                string rndChar = GetRandomAlphanumericString();
                if (uniqueChar == null)
                {
                    ShortUrl = new ShortUrl
                    {
                        CreatedTime = DateTime.UtcNow,
                        OrgUrl = OrgUrl,
                        RandomChar = rndChar,
                        UniqueChar = "",
                        EndTime = DateTime.UtcNow.AddDays(EndTime),
                        CutUrl = $"{ServiceUrl}/{rndChar}",
                        user = user,
                        Click = 1
                    };
                    originalUrl = new OriginalUrl
                    {
                        Url = OrgUrl
                    };
                    cr_user.Kısaltma += 1;
                    var filter = Builders<User>.Filter.Eq(s => s.Name, user);
                    var update = Builders<User>.Update.Set(s => s.Kısaltma, cr_user.Kısaltma);
                    userList.UpdateOneAsync(filter, update);

                }
                else
                {
                    ShortUrl = new ShortUrl
                    {
                        CreatedTime = DateTime.UtcNow,
                        OrgUrl = OrgUrl,
                        RandomChar = rndChar,
                        UniqueChar = uniqueChar,
                        EndTime = DateTime.UtcNow.AddDays(EndTime),
                        CutUrl = $"{ServiceUrl}/{uniqueChar}",
                        user = user,
                        Click = 1
                    };
                    originalUrl = new OriginalUrl
                    {
                        Url = OrgUrl
                    };
                    cr_user.Kısaltma += 1;
                    var filter = Builders<User>.Filter.Eq(s => s.Name, user);
                    var update = Builders<User>.Update.Set(s => s.Kısaltma, cr_user.Kısaltma);
                    userList.UpdateOneAsync(filter, update);
                }
                await originalUrlList.InsertOneAsync(originalUrl);
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
        //public async Task<IActionResult> ClickCounter(string orgUrl)
        //{
        //    string user = HttpContext.Session.GetString("user");
        //    var shortenedUrlCollection = mongoDatabase_database.GetCollection<ShortenedUrl>("shortened-urls");
        //    var shortenedUrl = await shortenedUrlCollection.AsQueryable().FirstOrDefaultAsync(x => x.OriginalUrl == orgUrl && x.User == user);
        //    shortenedUrl.Click += 1;
        //    var filter = Builders<ShortenedUrl>.Filter.And
        //        (
        //            Builders<ShortenedUrl>.Filter.Where(x => x.User == user),
        //            Builders<ShortenedUrl>.Filter.Where(x => x.OriginalUrl == orgUrl)
        //        );
        //    var update = Builders<ShortenedUrl>.Update.Set(s => s.Click, shortenedUrl.Click);
        //    shortenedUrlCollection.UpdateOneAsync(filter, update);
        //    return Redirect(shortenedUrl.OriginalUrl);
        //}
    }
}
