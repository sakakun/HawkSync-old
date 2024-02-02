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
    public class DashboardAPIController : ApiController
    {
        public class CurrentPlayers
        {
            public string name { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string isp { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class ImportantData
        {
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string isp { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
        }

        private ImportantData IPData(string address)
        {
            foreach (var IPInfo in GlobalAppState.AppState.IPQualityCache)
            {
                var IPId = IPInfo.Key;
                var IPVa = IPInfo.Value;
                foreach (var IPInformation in IPVa.IPInformation)
                {
                    if (address == IPInformation.address)
                    {
                        return new ImportantData
                        {
                            city = IPInformation.city,
                            state = IPInformation.region,
                            country = IPInformation.country_code,
                            isp = IPInformation.ISP,
                            lat = Convert.ToDouble(IPInformation.latitude),
                            lon = Convert.ToDouble(IPInformation.longitude)
                        };
                    }
                }
            }
            return new ImportantData { };
        }
        public class PlayerCount
        {
            public int numPlayers { get; set; }
        }
        public class ProfilesOnline
        {
            public int profilesOnline { get; set; }
        }
        public class CaughtVPNs
        {
            public int numVPN { get; set; }
        }
        public class UniquePlayers
        {
            public int num { get; set; }
        }
        public class ChartStats
        {
            public Dictionary<int, monthstats> stat { get; set; }
        }
        public class monthstats
        {
            public Dictionary<int, int> month { get; set; }
        }

        public HttpResponseMessage Get()
        {
            var Response = Request.CreateResponse();
            Response.StatusCode = HttpStatusCode.BadRequest;
            Response.Content = new StringContent("Module not specified!", Encoding.Default, "text/html");
            return Response;
        }

        public HttpResponseMessage Get(string id)
        {
            var Response = Request.CreateResponse();
            switch (id)
            {
                case "MapPlayers":
                    Response.StatusCode = HttpStatusCode.OK;
                    Response.Content = new StringContent(JsonConvert.SerializeObject(MapPlayers()), Encoding.Default, "application/json");
                    return Response;
                case "CurrentNumberPlayers":
                    Response.StatusCode = HttpStatusCode.OK;
                    Response.Content = new StringContent(JsonConvert.SerializeObject(CurrentNumberPlayers()), Encoding.Default, "application/json");
                    return Response;
                case "OnlineProfiles":
                    Response.StatusCode = HttpStatusCode.OK;
                    Response.Content = new StringContent(JsonConvert.SerializeObject(OnlineProfiles()), Encoding.Default, "application/json");
                    return Response;
                case "VPNsCaught":
                    Response.StatusCode = HttpStatusCode.OK;
                    Response.Content = new StringContent(JsonConvert.SerializeObject(VPNsCaught()), Encoding.Default, "application/json");
                    return Response;
                case "UniquePlayers":
                    Response.StatusCode = HttpStatusCode.OK;
                    Response.Content = new StringContent(JsonConvert.SerializeObject(UniquePlayersAPI()), Encoding.Default, "application/json");
                    return Response;
                case "ChartStats":
                    Response.StatusCode = HttpStatusCode.OK;
                    Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.yearlystats), Encoding.Default, "application/json");
                    return Response;
                default: // leave the default, this ensures we don't get a blank module.
                    Response.StatusCode = HttpStatusCode.BadRequest;
                    Response.Content = new StringContent("Module not specified!", Encoding.Default, "text/html");
                    return Response;
            }
        }

        private UniquePlayers UniquePlayersAPI()
        {
            int num = 0;
            foreach (var item in GlobalAppState.AppState.IPQualityCache)
            {
                ipqualityscore ipqualityscore = item.Value;
                foreach (var IPInfo in ipqualityscore.IPInformation)
                {
                    if (IPInfo.active_tor == false || IPInfo.active_vpn == false)
                    {
                        num++;
                    }
                }
            }
            return new UniquePlayers { num = num };
        }

        private CaughtVPNs VPNsCaught()
        {
            int caught = 0;
            foreach (var item in GlobalAppState.AppState.IPQualityCache)
            {
                ipqualityscore ipqualityscore = item.Value;
                foreach (var IPInfo in ipqualityscore.IPInformation)
                {
                    if (IPInfo.active_tor == true || IPInfo.active_vpn == true)
                    {
                        caught++;
                    }
                }
            }
            return new CaughtVPNs { numVPN = caught };
        }

        private ProfilesOnline OnlineProfiles()
        {
            int profilesOnline = 0;
            foreach (var item in GlobalAppState.AppState.Instances)
            {
                Instance Instance = item.Value;
                if (Instance.Status != InstanceStatus.OFFLINE)
                {
                    profilesOnline++;
                }
            }
            return new ProfilesOnline { profilesOnline = profilesOnline };
        }

        private PlayerCount CurrentNumberPlayers()
        {
            int totalPlayers = 0;
            foreach (var item in GlobalAppState.AppState.Instances)
            {
                Instance Instance = item.Value;
                totalPlayers += Instance.NumPlayers;
            }
            return new PlayerCount { numPlayers = totalPlayers };
        }

        private List<CurrentPlayers> MapPlayers()
        {
            ImportantData IPInfo;
            List<CurrentPlayers> players = new List<CurrentPlayers>();
            Dictionary<int, playerlist> currentPlayers;
            foreach (var instance in GlobalAppState.AppState.Instances)
            {
                var key = instance.Key;
                var val = instance.Value;
                if (val.PlayerList == null)
                {
                    currentPlayers = new Dictionary<int, playerlist>();
                }
                else
                {
                    currentPlayers = val.PlayerList;
                }
                foreach (var player in currentPlayers)
                {
                    var name = player.Value.name;
                    var address = player.Value.address;
                    IPInfo = IPData(player.Value.address);
                    players.Add(new CurrentPlayers
                    {
                        address = address,
                        name = name,
                        city = IPInfo.city,
                        state = IPInfo.state,
                        country = IPInfo.country,
                        isp = IPInfo.isp,
                        lat = IPInfo.lat,
                        lon = IPInfo.lon
                    });
                }
            }
            return players;
        }
    }
}
