using HawkSync_SM.Api.classes;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace HawkSync_SM.Api.v1
{
    public class DashboardStatsController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage MapPlayers()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(ApiFunctions.MapPlayers()), Encoding.Default, "application/json");
            return Response;
        }
        [HttpGet]
        public HttpResponseMessage CurrentNumberPlayers()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(ApiFunctions.CurrentNumberPlayersStats()), Encoding.Default, "application/json");
            return Response;
        }
        [HttpGet]
        public HttpResponseMessage OnlineProfiles()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(ApiFunctions.OnlineProfilesStats()), Encoding.Default, "application/json");
            return Response;
        }
        [HttpGet]
        public HttpResponseMessage VPNsCaught()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(ApiFunctions.VPNsCaughtStats()), Encoding.Default, "application/json");
            return Response;
        }
        [HttpGet]
        public HttpResponseMessage UniquePlayers()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(ApiFunctions.UniquePlayersAPI()), Encoding.Default, "application/json");
            return Response;
        }
        [HttpGet]
        public HttpResponseMessage ChartStats()
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.yearlystats), Encoding.Default, "application/json");
            return Response;
        }
    }
}
