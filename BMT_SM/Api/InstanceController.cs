using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace BMT_TV.Api
{
    public class InstanceController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
            {
                { "success", false },
                { "error", "Invalid Instance ID" }
            };
            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            return Response;
        }

        public HttpResponseMessage Get(int id)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            if (!GlobalAppState.AppState.Instances.ContainsKey(id))
            {
                Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                {
                    { "success", false },
                    { "error", "Invalid Instance ID" }
                };
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                return Response;
            }
            else
            {
                Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.Instances[id]), Encoding.Default, "application/json");
                return Response;
            }
        }
    }
}
