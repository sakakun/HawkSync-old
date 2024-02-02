using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace BMT_TV.Api
{
    public class InstancesController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.Instances), Encoding.Default, "application/json");
            return Response;
        }
    }
}
