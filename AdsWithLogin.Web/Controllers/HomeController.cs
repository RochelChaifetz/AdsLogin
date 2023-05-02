using AdsWithLogin.Data;
using AdsWithLogin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace AdsWithLogin.Web.Controllers
{
    public class HomeController : Controller
    {
        String _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=Ads; Integrated Security=true;";

        public IActionResult Index()
        {
            var repo = new Repository(_connectionString);
            var vm = new AdViewModel
            {
                AllAds = repo.GetAllAds()
            };
            foreach (var a in vm.AllAds)
            {
                a.UserName = repo.GetUserName(a.UserId);
                a.Delete = a.UserId == repo.GetUserIdByEmail(User.Identity.Name);
            }

            return View(vm);
        }
        [Authorize]
        public IActionResult MyAccount()
        {
            var repo = new Repository(_connectionString);
            var id = repo.GetUserIdByEmail(User.Identity.Name);
            var vm = new AdViewModel
            {
                AllAds = repo.GetAdsByUser(id)
            };
            return View(vm);
        }
        [Authorize]
        public IActionResult NewAd()
        {
            return View();
        }
        [HttpPost]
        public IActionResult NewAd(Ad ad)
        {
            var repo = new Repository(_connectionString);
            ad.UserId = repo.GetUserIdByEmail(User.Identity.Name);
            repo.NewAd(ad);
            return Redirect("/");
        }
        [HttpPost]
        public IActionResult DeleteAd(int id)
        {
            var mgr = new Repository(_connectionString);
            mgr.DeleteAd(id);

            return Redirect("/home/index");
        }
    }
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonSerializer.Deserialize<T>(value);
        }
    }

}