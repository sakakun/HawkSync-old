using HawkSync_SM;
using HawkSync_SM.classes.ChatManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM
{
    public class IPManagement
    {
        public static string IPpattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        public static void public_ip()
        {
            try {
                string url = "http://checkip.dyndns.org";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string a4 = a3[0];
                ProgramConfig.PublicIP = a4;
            } catch (Exception)
            {
                ProgramConfig.PublicIP = "0.0.0.0";
            }
            return;

        }

        public void initIPManagement_ProgramConfig(SQLiteConnection hawkSyncDB)
        {
            SQLiteCommand checkVPNQuery = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = @key;", hawkSyncDB);
            checkVPNQuery.Parameters.AddWithValue("@key", "enable_vpnChecking");
            int checkVPN = Convert.ToInt32(checkVPNQuery.ExecuteScalar());
            ProgramConfig.EnableVPNCheck = Convert.ToBoolean(checkVPN);

            checkVPNQuery = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = @key;", hawkSyncDB);
            checkVPNQuery.Parameters.AddWithValue("@key", "enable_wfb");
            int EnableWFB = Convert.ToInt32(checkVPNQuery.ExecuteScalar());
            ProgramConfig.EnableWFB = Convert.ToBoolean(EnableWFB);

            checkVPNQuery.Parameters.AddWithValue("@key", "ip_quality_score_apikey");
            string apikey = checkVPNQuery.ExecuteScalar().ToString();
            ProgramConfig.ip_quality_score_apikey = apikey;

            checkVPNQuery.Dispose();
        }

        public List<ipqualityClass> cache_loadIPQuality(int profileid, SQLiteConnection db)
        {
            List<ipqualityClass> loadCache = new List<ipqualityClass>();
            SQLiteCommand query = new SQLiteCommand("SELECT * FROM `ipqualitycache` WHERE `profile_id` = @profileid;", db);
            query.Parameters.AddWithValue("@profileid", profileid);
            SQLiteDataReader reader = query.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    loadCache.Add(new ipqualityClass
                    {
                        address = reader.GetString(reader.GetOrdinal("address")),
                        fraud_score = reader.GetInt32(reader.GetOrdinal("fraud_score")),
                        country_code = reader.GetString(reader.GetOrdinal("country_code")),
                        region = reader.GetString(reader.GetOrdinal("region")),
                        city = reader.GetString(reader.GetOrdinal("city")),
                        ISP = reader.GetString(reader.GetOrdinal("ISP")),
                        is_crawler = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("is_crawler")))),
                        mobile = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("mobile")))),
                        host = reader.GetString(reader.GetOrdinal("host")),
                        proxy = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("proxy")))),
                        vpn = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("vpn")))),
                        tor = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("tor")))),
                        active_vpn = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("active_vpn")))),
                        active_tor = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("active_tor")))),
                        recent_abuse = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("recent_abuse")))),
                        bot_status = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("bot_status")))),
                        request_id = reader.GetString(reader.GetOrdinal("request_id")),
                        latitude = reader.GetString(reader.GetOrdinal("lat")),
                        longitude = reader.GetString(reader.GetOrdinal("long")),
                        NextCheck = DateTime.Parse(reader.GetString(reader.GetOrdinal("NextCheck")))
                    });
                }
                reader.Close();
                query.Dispose();
                return loadCache;
            }
            else
            {
                return new List<ipqualityClass>();
            }
        }

        public Dictionary<int, ob_ipWhitelist> cache_loadWhitelist(int ArrayID, int InstanceID, SQLiteConnection db)
        {
            Dictionary<int, ob_ipWhitelist> WhiteList = new Dictionary<int, ob_ipWhitelist>();
            
            // Build this regardless, incase it has to be turned off but don't want to loose the settings.
            SQLiteCommand query = new SQLiteCommand("SELECT `description`, `address` FROM `vpnwhitelist` WHERE `profile_id` = @profileid;", db);
            query.Parameters.AddWithValue("@profileid", InstanceID);
            SQLiteDataReader list = query.ExecuteReader();
            bool hasRows = list.HasRows;
            if (hasRows == true)
            {
                int Index = 0;
                while (list.Read())
                {
                    string description = list.GetString(list.GetOrdinal("description"));
                    IPAddress address = IPAddress.Parse(list.GetString(list.GetOrdinal("address")));
                    WhiteList.Add(Index, new ob_ipWhitelist
                    {
                        Description = description,
                        IPAddress = address.ToString()
                    });
                    Index++;
                }
            }
            list.Close();
            query.Dispose();
            return WhiteList;
        }

        public string IPQualityCheck(string ipaddress)
        {
            string URL = $"https://ipqualityscore.com/api/json/ip/{ProgramConfig.ip_quality_score_apikey}/{ipaddress}?strictness=0&allow_public_access_points=true&fast=true&lighter_penalties=true&mobile=true";
            WebRequest request = WebRequest.Create(URL);
            request.Timeout = 7200;
            WebResponse reply = request.GetResponse();
            StreamReader replyTxt = new StreamReader(reply.GetResponseStream());
            string Response = replyTxt.ReadToEnd();
            reply.Close();
            reply.Dispose();
            replyTxt.Dispose();
            return Response;
        }

        public void Check4VPN(ref AppState _state, int ArrayID)
        {
            // This will confirm if the Global VPN checking is allowed and if the instance has it enabled.
            if (!_state.Instances[ArrayID].enableVPNCheck || !ProgramConfig.EnableVPNCheck)
                return;

            foreach (var playerData in _state.Instances[ArrayID].PlayerList)
            {
                try
                {
                    var ipInfo = _state.IPQualityCache[ArrayID].IPInformation.FirstOrDefault(info => info.address == playerData.Value.address);
                    bool isNewAddress = ipInfo == null;
                    
                    if (isNewAddress)
                    {
                        var jsonData = IPQualityCheck(playerData.Value.address);
                        ipInfo = JsonConvert.DeserializeObject<ipqualityClass>(jsonData);
                        ipInfo.address = playerData.Value.address;
                        ipInfo.NextCheck = DateTime.Now.AddDays(25);
                        _state.IPQualityCache[ArrayID].IPInformation.Add(ipInfo);
                        InsertOrUpdateIPQualityCache(_state.Instances[ArrayID].Id, ipInfo);
                    }
                    else if (DateTime.Compare(ipInfo.NextCheck, DateTime.Now) < 0)
                    {
                        var jsonData = IPQualityCheck(playerData.Value.address);
                        ipInfo = JsonConvert.DeserializeObject<ipqualityClass>(jsonData);
                        ipInfo.address = playerData.Value.address;
                        ipInfo.NextCheck = DateTime.Now.AddDays(25);
                        UpdateIPQualityCache(_state, ArrayID, ipInfo);
                    }

                    if (ipInfo.fraud_score >= _state.IPQualityCache[ArrayID].WarnLevel && ipInfo.active_vpn)
                    {
                        bool isBanned = _state.Instances[ArrayID].BanList.Any(ban => ban.ipaddress == ipInfo.address);
                        if (!isBanned)
                        {
                            _state.Instances[ArrayID].BanList.Add(new ob_playerBanList
                            {
                                ipaddress = playerData.Value.address,
                                player = playerData.Value.name,
                                reason = "[BMT] Failed to Pass IP Checks",
                                retry = DateTime.Now,
                                newBan = true,
                                onlykick = false,
                                expires = "-1",
                                VPNBan = true,
                                addedDate = DateTime.Now,
                                bannedBy = "BMT Automated Systems",
                                lastseen = DateTime.Now
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    _state.eventLog.WriteEntry("BMTv4 TV has detected an error!\n\n" + e.ToString(), EventLogEntryType.Error);
                    return;
                }
            }
        }

        private void InsertOrUpdateIPQualityCache(int profileID, ipqualityClass ipInfo)
        {
            using (var conn = new SQLiteConnection(ProgramConfig.DBConfig))
            {
                conn.Open();
                var cmd = new SQLiteCommand(conn);
                cmd.CommandText = @"INSERT INTO `ipqualitycache` (`id`, `profile_id`, `address`, `fraud_score`, `country_code`, `region`, `city`, `ISP`, `is_crawler`, `mobile`, `host`, `proxy`, `vpn`, `tor`, `active_vpn`, `active_tor`, `recent_abuse`, `bot_status`, `lat`, `long`, `request_id`, `NextCheck`) 
                            VALUES (NULL, @profileid, @address, @fraud_score, @country_code, @region, @city, @ISP, @is_crawler, @mobile, @host, @proxy, @vpn, @tor, @active_vpn, @active_tor, @recent_abuse, @bot_status, @lat, @long, @request_id, @NextCheck);";
                cmd.Parameters.AddWithValue("@profileid", profileID);
                cmd.Parameters.AddWithValue("@address", ipInfo.address);
                cmd.Parameters.AddWithValue("@fraud_score", ipInfo.fraud_score);
                cmd.Parameters.AddWithValue("@country_code", ipInfo.country_code);
                cmd.Parameters.AddWithValue("@region", ipInfo.region);
                cmd.Parameters.AddWithValue("@city", ipInfo.city);
                cmd.Parameters.AddWithValue("@ISP", ipInfo.ISP);
                cmd.Parameters.AddWithValue("@is_crawler", ipInfo.is_crawler);
                cmd.Parameters.AddWithValue("@mobile", ipInfo.mobile);
                cmd.Parameters.AddWithValue("@host", ipInfo.host);
                cmd.Parameters.AddWithValue("@proxy", ipInfo.proxy);
                cmd.Parameters.AddWithValue("@vpn", ipInfo.vpn);
                cmd.Parameters.AddWithValue("@tor", ipInfo.tor);
                cmd.Parameters.AddWithValue("@active_vpn", ipInfo.active_vpn);
                cmd.Parameters.AddWithValue("@active_tor", ipInfo.active_tor);
                cmd.Parameters.AddWithValue("@recent_abuse", ipInfo.recent_abuse);
                cmd.Parameters.AddWithValue("@bot_status", ipInfo.bot_status);
                cmd.Parameters.AddWithValue("@lat", ipInfo.latitude);
                cmd.Parameters.AddWithValue("@long", ipInfo.longitude);
                cmd.Parameters.AddWithValue("@request_id", ipInfo.request_id);
                cmd.Parameters.AddWithValue("@NextCheck", ipInfo.NextCheck);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateIPQualityCache(AppState state, int arrayID, ipqualityClass ipInfo)
        {
            using (var conn = new SQLiteConnection(ProgramConfig.DBConfig))
            {
                conn.Open();
                var cmd = new SQLiteCommand(conn);
                cmd.CommandText = @"UPDATE `ipqualitycache` 
                            SET `address` = @address, `fraud_score` = @fraud_score, `country_code` = @country_code, `region` = @region, `city` = @city, `ISP` = @ISP, `is_crawler` = @is_crawler, 
                                `mobile` = @mobile, `host` = @host, `proxy` = @proxy, `vpn` = @vpn, `tor` = @tor, `active_vpn` = @active_vpn, `active_tor` = @active_tor, `recent_abuse` = @recent_abuse, 
                                `bot_status` = @bot_status, `request_id` = @request_id, `lat` = @lat, `long` = @long, `NextCheck` = @NextCheck 
                            WHERE `id` = @id AND `profile_id` = @profileid;";
                cmd.Parameters.AddWithValue("@address", ipInfo.address);
                cmd.Parameters.AddWithValue("@fraud_score", ipInfo.fraud_score);
                cmd.Parameters.AddWithValue("@country_code", ipInfo.country_code);
                cmd.Parameters.AddWithValue("@region", ipInfo.region);
                cmd.Parameters.AddWithValue("@city", ipInfo.city);
                cmd.Parameters.AddWithValue("@ISP", ipInfo.ISP);
                cmd.Parameters.AddWithValue("@is_crawler", ipInfo.is_crawler);
                cmd.Parameters.AddWithValue("@mobile", ipInfo.mobile);
                cmd.Parameters.AddWithValue("@host", ipInfo.host);
                cmd.Parameters.AddWithValue("@proxy", ipInfo.proxy);
                cmd.Parameters.AddWithValue("@vpn", ipInfo.vpn);
                cmd.Parameters.AddWithValue("@tor", ipInfo.tor);
                cmd.Parameters.AddWithValue("@active_vpn", ipInfo.active_vpn);
                cmd.Parameters.AddWithValue("@active_tor", ipInfo.active_tor);
                cmd.Parameters.AddWithValue("@recent_abuse", ipInfo.recent_abuse);
                cmd.Parameters.AddWithValue("@bot_status", ipInfo.bot_status);
                cmd.Parameters.AddWithValue("@lat", ipInfo.latitude);
                cmd.Parameters.AddWithValue("@long", ipInfo.longitude);
                cmd.Parameters.AddWithValue("@request_id", ipInfo.request_id);
                cmd.Parameters.AddWithValue("@NextCheck", ipInfo.NextCheck);
                cmd.Parameters.AddWithValue("@id", ipInfo.id);
                cmd.Parameters.AddWithValue("@profileid", state.Instances[arrayID].Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void CheckBans(ref AppState _state, int profileid, int pid)
        {
            var currentPlayers = _state.Instances[profileid].PlayerList;
            var currentBans = _state.Instances[profileid].BanList;
            var numPlayers = _state.Instances[profileid].NumPlayers;

            if (numPlayers <= 0) return;

            foreach (var ban in currentBans)
            {
                var bannedIP = IPAddress.Parse(ban.ipaddress);
                var subnetMaskString = ban.ipaddress.Contains('/') ? ban.ipaddress.Split('/')[1] : "32"; // default to /32 if no subnet mask provided
                var subnetMask = LeftShift(IPAddress.Parse("255.255.255.255"), 32 - int.Parse(subnetMaskString)); // call LeftShift here
                var bannedSubnet = ban.ipaddress.Contains('/') ? IPAddress.Parse(ban.ipaddress.Split('/')[0]) : bannedIP;

                foreach (var player in currentPlayers)
                {
                    try
                    {
                        var playerName = player.Value.name;
                        var playerIP = IPAddress.Parse(player.Value.address);

                        // Check if the player's IP matches the banned IP or falls within the banned subnet
                        if (IPAddress.Equals(playerIP, bannedIP) ||
                            (GetNetworkAddress(playerIP, subnetMask).Equals(bannedSubnet) &&
                            IsInSubnet(playerIP, bannedSubnet, subnetMask)))
                        {
                            var reason = ban.reason;
                            var onlyKick = ban.onlykick;
                            var slot = player.Value.slot;

                            if ((onlyKick && DateTime.Compare(ban.retry, DateTime.Now) < 0) ||
                                (!onlyKick && DateTime.Compare(ban.retry, DateTime.Now) < 0))
                            {
                                var action = onlyKick ? "KICKING" : (ban.newBan ? "BANNING" : "KICKING");
                                (new ServerManagement()).SendChatMessage(ref _state, profileid, ChatManagement.ChatChannels[2], $"{action}!!! {playerName} - {reason}");
                                (new ServerManagement()).SendConsoleCommand(ref _state, profileid, $"punt {slot}");

                                ban.retry = DateTime.Now.AddMinutes(1.0);
                                ban.lastseen = DateTime.Now;
                                Thread.Sleep(100);

                                if (onlyKick)
                                    _state.Instances[profileid].BanList.Remove(ban);

                                if (!onlyKick && ban.newBan)
                                    ban.newBan = false;
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }

        // Perform left shift operation on an IPAddress
        public static IPAddress LeftShift(IPAddress ipAddress, int bits)
        {
            byte[] addressBytes = ipAddress.GetAddressBytes();
            int byteShift = bits / 8;
            int bitShift = bits % 8;
            byte carry = 0;

            for (int i = 0; i < addressBytes.Length; i++)
            {
                byte newByte = (byte)(addressBytes[i] << bitShift | carry);
                carry = (byte)(addressBytes[i] >> (8 - bitShift));
                addressBytes[i] = newByte;
            }

            return new IPAddress(addressBytes);
        }

        // Get the network address of an IP address using a subnet mask
        public static IPAddress GetNetworkAddress(IPAddress ipAddress, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = ipAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();
            byte[] networkAddressBytes = new byte[ipAddressBytes.Length];

            for (int i = 0; i < ipAddressBytes.Length; i++)
            {
                networkAddressBytes[i] = (byte)(ipAddressBytes[i] & subnetMaskBytes[i]);
            }

            return new IPAddress(networkAddressBytes);
        }

        // Check if an IP address is in a subnet
        public static bool IsInSubnet(IPAddress ipAddress, IPAddress subnetAddress, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = ipAddress.GetAddressBytes();
            byte[] subnetAddressBytes = subnetAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            for (int i = 0; i < ipAddressBytes.Length; i++)
            {
                byte invertedSubnetMaskByte = (byte)~subnetMaskBytes[i];
                if ((ipAddressBytes[i] & subnetMaskBytes[i]) != subnetAddressBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

    }
}
