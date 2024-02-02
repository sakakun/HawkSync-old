using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers
{
    public class LogoutController : Controller
    {
        public IActionResult Index()
        {
            /*var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1),
                Path = "/"
            };
            Response.Cookies.Append(".AspNetCore.Session", "false", cookieOptions);*/
            Response.Cookies.Delete(".AspNetCore.Session");
            return RedirectToAction("Index", "Login", new { success = 1 });
        }
    }
}
