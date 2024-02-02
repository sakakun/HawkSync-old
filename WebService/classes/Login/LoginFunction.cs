using System;
using System.Net.Http;
using System.Text;
using System.Web;

namespace WebService.classes.Login
{
    public class LoginFunction
    {
        public HttpResponseMessage Login(string username, string password)
        {
            HttpClient client = new();
            UriBuilder uriBuilder = new("http://localhost:7001/api/v1/User/Login");
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["username"] = username;
            query["password"] = password;
            uriBuilder.Query = query.ToString();

            HttpContent c = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            HttpRequestMessage r = new()
            {
                Content = c,
                Method = HttpMethod.Post,
                RequestUri = uriBuilder.Uri,
            };
            try
            {
                HttpResponseMessage response = client.Send(r);
                return response;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
