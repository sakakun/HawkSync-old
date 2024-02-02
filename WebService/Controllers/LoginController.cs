using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using WebService.classes;
using WebService.classes.Login;

namespace WebService.Controllers
{
    public class LoginController : Controller
    {
        [HttpPost]
        public IActionResult Index(IFormCollection form)
        {
            string password = Crypt.CreateMD5(form["password"]);
            string username = form["username"];
            string remember = form["rememberme"];
            LoginFunction loginFunctions = new();
            HttpResponseMessage response;
            try
            {
                response = loginFunctions.Login(username, password);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Login", new { error = 3 });
            }
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Login", new { error = 2 });
            }

            LoginAPI loginAPI = JsonConvert.DeserializeObject<LoginAPI>(response.Content.ReadAsStringAsync().Result);
            if (loginAPI.success == true)
            {
                // setup static vars after login
                HttpContext.Session.SetString("loggedin", "true");
                HttpContext.Session.SetString("username", username);
                HttpContext.Session.SetInt32("userid", loginAPI.userid);


                // .AspNetCore.Session
                // if RememberMe is selected, modify the cookie, and set the expiry date for 7 days.
                if (remember == "true")
                {
                    string currentSessionID = Request.Cookies[".AspNetCore.Session"];
                    if (currentSessionID == null || currentSessionID == "false")
                    {
                        Guid guid = new Guid();
                        Response.Cookies.Append(".AspNetCore.Session", guid.ToString());
                        currentSessionID = guid.ToString();
                    }
                    Response.Cookies.Append(".AspNetCore.Session", currentSessionID, new CookieOptions()
                    {
                        Expires = DateTime.Now.AddDays(7),
                        Path = "/"
                    });
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Login", new { error = 1 });
            }
        }
        [HttpGet]
        public IActionResult Index(int? error, int? success)
        {
            if (error != null)
            {
                ViewBag.LoginError = error;
            }
            else
            {
                ViewBag.LoginError = null;
            }
            if (success != null)
            {
                ViewBag.LoginSuccess = success;
            }
            else
            {
                ViewBag.LoginSuccess = null;
            }
            return View();
        }
    }
}
