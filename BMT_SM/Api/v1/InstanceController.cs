using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace HawkSync_SM.Api.v1
{
    public class InstanceController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetInstances()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.Instances), Encoding.Default, "application/json");
            return Response;
        }
        public HttpResponseMessage GetInstance()
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
        public HttpResponseMessage GetInstance(int id)
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
