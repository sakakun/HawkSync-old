using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers
{
    public class ServerManagerController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("loggedin") == null || HttpContext.Session.GetString("loggedin") == "0")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public IActionResult AutoMessages()
        {
            if (HttpContext.Session.GetString("loggedin") == null || HttpContext.Session.GetString("loggedin") == "0")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public IActionResult ChatManager()
        {
            if (HttpContext.Session.GetString("loggedin") == null || HttpContext.Session.GetString("loggedin") == "0")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public IActionResult MapManager()
        {
            if (HttpContext.Session.GetString("loggedin") == null || HttpContext.Session.GetString("loggedin") == "0")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public IActionResult PlayerManager()
        {
            if (HttpContext.Session.GetString("loggedin") == null || HttpContext.Session.GetString("loggedin") == "0")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public IActionResult PluginSettings()
        {
            if (HttpContext.Session.GetString("loggedin") == null || HttpContext.Session.GetString("loggedin") == "0")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public IActionResult ServerSettings()
        {
            if (HttpContext.Session.GetString("loggedin") == null || HttpContext.Session.GetString("loggedin") == "0")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        //[HttpPost]
        // setup POST actions below
    }
}
