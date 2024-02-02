using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace BMT_TV.Api
{
    public class UserController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage RequestUser(string userid)
        {
            var Response = Request.CreateResponse();
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            if (userid == null || userid == "")
            {
                reply.Add("success", false);
                reply.Add("error", "UserID was not specified!");
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            else
            {
                // return specific user instead of every user in memory
                string Username = "";
                Permissions userPermissions = new Permissions();
                foreach (var item in GlobalAppState.AppState.Users)
                {
                    if (item.Value.UserID == Convert.ToInt32(userid))
                    {
                        Username = item.Key;
                        userPermissions = item.Value.Permissions;
                        break;
                    }
                }
                reply.Add("Username", Username);
                reply.Add("UserPermissions", userPermissions);
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage Login(string username, string password)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                reply.Add("success", false);
                reply.Add("error", "Please provide credentials.");
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            else
            {
                // check if username exists first before assigning userData for lookups.
                if (!GlobalAppState.AppState.Users.ContainsKey(username))
                {
                    reply.Add("success", false);
                    reply.Add("error", "Invalid username or password");
                    Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                }
                else
                {
                    UserCodes userData = GlobalAppState.AppState.Users[username];

                    // since username exists, check password
                    if (userData.Password != password || userData.WebAdmin != true)
                    {
                        reply.Add("success", false);
                        reply.Add("error", "Invalid username or password");
                        Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                    }
                    else
                    {
                        reply.Clear();
                        reply.Add("success", true);
                        reply.Add("userid", userData.UserID);
                        reply.Add("permissions", userData.Permissions);
                        Console.WriteLine("User has logged into the Web Interface: " + username);
                        Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                    }
                }
            }
            return Response;
        }
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage Fuck()
        {
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
            {
                { "success", true },
                { "msg", "yeah, it fucking works." }
            };
            var Response = Request.CreateResponse();
            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            return Response;
        }
    }
}
