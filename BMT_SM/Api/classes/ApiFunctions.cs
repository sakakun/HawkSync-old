using System;
using System.Collections.Generic;

namespace HawkSync_SM.Api.classes
{
    public static class ApiFunctions
    {
        public static ImportantData IPData(string address)
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
        public static UniquePlayersClass UniquePlayersAPI()
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
            return new UniquePlayersClass { num = num };
        }
        public static CaughtVPNs VPNsCaughtStats()
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
        public static ProfilesOnline OnlineProfilesStats()
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
        public static PlayerCount CurrentNumberPlayersStats()
        {
            int totalPlayers = 0;
            foreach (var item in GlobalAppState.AppState.Instances)
            {
                Instance Instance = item.Value;
                totalPlayers += Instance.NumPlayers;
            }
            return new PlayerCount { numPlayers = totalPlayers };
        }
        public static List<CurrentPlayers> MapPlayers()
        {
            ImportantData IPInfo;
            List<CurrentPlayers> players = new List<CurrentPlayers>();
            Dictionary<int, ob_playerList> currentPlayers;
            foreach (var instance in GlobalAppState.AppState.Instances)
            {
                var key = instance.Key;
                var val = instance.Value;
                if (val.PlayerList == null)
                {
                    currentPlayers = new Dictionary<int, ob_playerList>();
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
