using Microsoft.AspNetCore.Mvc;

namespace WebService.Views
{
    public class UsersController : Controller
    {
        public IActionResult List()
        {
            return View();
        }
    }
}
