using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace HawkSync_SM.Api.v1
{
    public class OptionsController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetCurrentOptions()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.SystemInfo), Encoding.Default, "application/json");
            return Response;
        }
    }
}
