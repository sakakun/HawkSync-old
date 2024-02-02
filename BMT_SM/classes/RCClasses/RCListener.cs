using HawkSync_SM.classes.logs;
using HawkSync_SM.RCClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WatsonTcp;

namespace HawkSync_SM
{
    public class RCListener
    {
        AppState _state;
        string sessionID = string.Empty;
        RCLoginFunctions RCLoginFunctions;
        RCRotationManager RCRotationManager;
        RCFunctions RCFunctions;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public RCListener(AppState state)
        {
            _state = state;
            RCLoginFunctions = new RCLoginFunctions(_state);
            RCRotationManager = new RCRotationManager(_state);
            RCFunctions = new RCFunctions(_state);
        }
        public byte[] BMTRemoteFunctions(int clientNo, ref Dictionary<object, object> MetaData, MessageReceivedEventArgs msg = null, SyncRequest request = null)
        {
            byte[] msData = new byte[0];
            if (msg != null)
            {
                msData = Compression.Decompress(msg.Data);
            }
            else if (request != null)
            {
                msData = Compression.Decompress(request.Data);
            }
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            try
            {
                RCListenerClass json;
                try
                {
                    string cmd = Encoding.Default.GetString(msData);
                    json = JsonConvert.DeserializeObject<RCListenerClass>(cmd);
                }
                catch
                {
                    return new byte[0];
                }
                switch (json.action)
                {
                    case "BMTRC.Open":
                        Random rand = new Random();
                        string sessionIDData = string.Empty;
                        string IPAddressPort = string.Empty;
                        if (msg != null)
                        {
                            sessionIDData = msg.Client.IpPort + DateTime.Now + rand.Next();
                            IPAddressPort = msg.Client.IpPort;
                        }
                        else if (request != null)
                        {
                            sessionIDData = request.Client.IpPort + DateTime.Now + rand.Next();
                            IPAddressPort = request.Client.IpPort;
                        }
                        if (json.Version < ProgramConfig.RCVersion)
                        {
                            reply.Add("action", "BMTRC.Open");
                            reply.Add("LoginMessage", RCListenerClass.StatusCodes.UPDATEREQUIRED);
                            reply.Add("message", "RC Version is out of date. Please update to the latest version.");
                            reply.Add("sessionID", string.Empty);
                            return Encoding.Default.GetBytes(JsonConvert.SerializeObject(reply));
                        }
                        sessionID = Crypt.CreateMD5(sessionIDData);
                        string[] clientaddress = IPAddressPort.Split(':');

                        _state.rcClients.Add(sessionID, new RCListenerClass
                        {
                            SessionID = sessionID,
                            lastCMD = "Connected",
                            authenticated = false,
                            lastCMDTime = DateTime.Now,
                            active = true,
                            expires = DateTime.Now.AddMinutes(10).ToString(),
                            clientNo = clientNo,
                            RemoteAddress = clientaddress[0],
                            RemotePort = Convert.ToInt32(clientaddress[1])
                        });

                        Dictionary<dynamic, dynamic> welcomepackage = new Dictionary<dynamic, dynamic>
                        {
                            { "LoginMessage", RCListenerClass.StatusCodes.WELCOME },
                            { "SessionID", sessionID }
                        };
                        var jsonid = JsonConvert.SerializeObject(welcomepackage);
                        _state.rcClients[sessionID].expires = DateTime.Now.AddMinutes(10).ToString();
                        return Encoding.ASCII.GetBytes(jsonid);
                    case "BMTRC.Login":
                        reply.Add("Status", RCLoginFunctions.Login(json.SessionID, json.username, json.password));
                        var jsonready = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonready);
                    case "BMTRC.GetUserPermissions":
                        reply.Add("Status", RCListenerClass.StatusCodes.SUCCESS);
                        reply.Add("UserCodes", Crypt.Base64Encode(JsonConvert.SerializeObject(RCLoginFunctions.GetUserPermissions(json.SessionID))));
                        var jsonPermissions = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonPermissions);
                    case "BMTRC.GetVPNSettings":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.SUCCESS);
                            reply.Add("WarnLevel", RCFunctions.GetVPNSettings(json.serverID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonVPNSettings = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonVPNSettings);
                    case "BMTRC.DeleteWarning":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.DeleteWarning(json.serverID, json.warnID, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonDeleteWarning = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonDeleteWarning);
                    case "BMTRC.CreateWarning":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.CreateWarning(json.serverID, json.newWarning, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonCreateWarning = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonCreateWarning);
                    case "BMTRC.WarnPlayer":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.WarnPlayer(json.serverID, json.warning, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonWarnPlayer = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonWarnPlayer);
                    case "BMTRC.GetCountryCodes":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            List<string> countryCodes = new List<string>();
                            reply.Add("Status", RCFunctions.GetCountryCodes(ref countryCodes));
                            reply.Add("CountryCodeList", countryCodes);
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonGetCountryCodes = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonGetCountryCodes);
                    case "BMTRC.GetAutoRes":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            string autoResBase64 = string.Empty;
                            reply.Add("Status", RCLoginFunctions.GetAutoRes(ref autoResBase64));
                            reply.Add("AutoRes", autoResBase64);
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonAutoRes = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonAutoRes);
                    case "BMTRC.GetFTPPort":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            int ftpPort = 0;
                            reply.Add("Status", RCLoginFunctions.GetFTPPort(ref ftpPort));
                            reply.Add("Port", ftpPort);
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonftpPort = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonftpPort);
                    case "BMTRC.KickPlayer":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.KickPlayer(json.serverID, json.slot, json.banReason, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonKickPlayer = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonKickPlayer);
                    case "BMTRC.BanPlayer":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.BanPlayer(json.serverID, json.slot, json.banReason, json.expires.ToString(), json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonBanPlayer = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonBanPlayer);
                    case "BMTRC.AddBan":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.AddBan(json.serverID, json.PlayerName, json.banReason, json.PlayerIP, json.AddBanExpires, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonAddBan = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonAddBan);
                    case "BMTRC.DisarmPlayer":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.DisarmPlayer(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonDisarmPlayer = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonDisarmPlayer);
                    case "BMTRC.RearmPlayer":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.RearmPlayer(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonRearmPlayer = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonRearmPlayer);
                    case "BMTRC.KillPlayer":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.KillPlayer(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonKillPlayer = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonKillPlayer);
                    case "BMTRC.ChangePlayerTeam":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.ChangeTeam(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonChangeTeam = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonChangeTeam);
                    case "BMTRC.ScoreMap":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.ScoreMap(json.serverID, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonScoreMap = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonScoreMap);
                    case "BMTRC.SetNextMap":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.SetNextMap(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonSetNextMap = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonSetNextMap);
                    case "BMTRC.AddAutoMsg":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.AddAutoMsg(json.serverID, json.newMsg, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonAddAutoMsg = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonAddAutoMsg);
                    case "BMTRC.DeleteAutoMsg":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.DeleteAutoMsg(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonDeleteAutoMsg = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonDeleteAutoMsg);
                    case "BMTRC.ChangeAutoMsgInterval":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.ChangeAutoMsgInterval(json.serverID, json.interval, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonChangeAutoMsgInterval = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonChangeAutoMsgInterval);
                    case "BMTRC.EnableAutoMsg":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.EnableAutoMsg(json.serverID, json.active, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonEnableAutoMsg = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonEnableAutoMsg);
                    case "BMTRC.RemoveBan":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.RemoveBan(json.serverID, json.PlayerName, json.PlayerIP, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonRemoveBan = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonRemoveBan);
                    case "BMTRC.UpdateWarnLevel":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateWarnLevel(json.serverID, json.WarnLevel, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateWarnLevel = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateWarnLevel);
                    case "BMTRC.DisallowVPN":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.DisallowVPN(json.serverID, json.DisallowVPN, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonDisallowVPN = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonDisallowVPN);
                    case "BMTRC.EnableGodMode":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.EnableGodMode(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonEnableGodMode = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonEnableGodMode);
                    case "BMTRC.SendMsg":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.SendMsg(json.serverID, json.slot, json.newMsg, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonSendMsg = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonSendMsg);
                    case "BMTRC.UpdateMapCycle":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateMapCycle(json.serverID, json.MapCycle, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateMapCycle = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateMapCycle);
                    case "BMTRC.DisableGodMode":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.DisableGodMode(json.serverID, json.slot, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonDisableGodMode = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonDisableGodMode);
                    case "BMTRC.UpdateServerName":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateServerName(json.serverID, json.ServerName, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateServerName = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateServerName);
                    case "BMTRC.UpdateCountryCode":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateCountryCode(json.serverID, json.CountryCode, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateCountryCode = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateCountryCode);
                    case "BMTRC.UpdateServerPassword":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateServerPassword(json.serverID, json.password, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateServerPassword = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateServerPassword);
                    case "BMTRC.UpdateSessionType":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateSessionType(json.serverID, json.SessionType, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateSessionType = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateSessionType);
                    case "BMTRC.UpdateTimeLimit":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateTimeLimit(json.serverID, json.TimeLimit, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateTimeLimit = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateTimeLimit);
                    case "BMTRC.UpdateStartDelay":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateStartDelay(json.serverID, json.StartDelay, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateStartDelay = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateStartDelay);
                    case "BMTRC.UpdateLoopMaps":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateLoopMaps(json.serverID, json.LoopMaps, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateLoopMaps = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateLoopMaps);
                    case "BMTRC.UpdateRespawnTime":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateRespawnTime(json.serverID, json.RespawnTime, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateRespawnTime = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateRespawnTime);
                    case "BMTRC.UpdatePSPTime":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdatePSPTime(json.serverID, json.PSPTime, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdatePSPTime = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdatePSPTime);
                    case "BMTRC.UpdateFlagReturnTime":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateFlagReturnTime(json.serverID, json.FlagReturnTime, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateFlagReturnTime = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateFlagReturnTime);
                    case "BMTRC.UpdateMaxTeamLives":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateMaxTeamLives(json.serverID, json.MaxTeamLives, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateMaxTeamLives = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateMaxTeamLives);
                    case "BMTRC.UpdateFriendlyFireKills":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateFriendlyFireKills(json.serverID, json.FriendlyFireKills, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateFriendlyFireKills = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateFriendlyFireKills);
                    case "BMTRC.UpdateScoreboardDelay":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateScoreboardDelay(json.serverID, json.ScoreboardDelay, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateScoreboardDelay = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateScoreboardDelay);
                    case "BMTRC.UpdateRequireNovaLogin":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateRequireNovaLogin(json.serverID, json.RequireNovaLogin, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateRequireNovaLogin = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateRequireNovaLogin);
                    case "BMTRC.UpdateAllowCustomSkins":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateAllowCustomSkins(json.serverID, json.AllowCustomSkins, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateAllowCustomSkins = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateAllowCustomSkins);
                    case "BMTRC.UpdateMOTD":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateMOTD(json.serverID, json.MOTD, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateMOTD = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateMOTD);
                    case "BMTRC.UpdateBluePassword":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateBluePassword(json.serverID, json.BluePassword, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateBluePassword = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateBluePassword);
                    case "BMTRC.UpdateRedPassword":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateRedPassword(json.serverID, json.RedPassword, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateRedPassword = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateRedPassword);
                    case "BMTRC.UpdateFBScore":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateFBScore(json.serverID, json.FBScore, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateFBScore = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateFBScore);
                    case "BMTRC.UpdateKOTHScore":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateKOTHScore(json.serverID, json.KOTHScore, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateKOTHScore = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateKOTHScore);
                    case "BMTRC.UpdateGameScore":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateGameScore(json.serverID, json.GameScore, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateGameScore = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateGameScore);
                    case "BMTRC.UpdateGamePlayOptions":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateGamePlayOptions(json.serverID, json.FriendlyFire, json.FriendlyTags, json.ShowTeamClays, json.AutoBalance, json.FriendlyFireWarning, json.ShowTracers, json.AllowAutoRange, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateGamePlayOptions = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateGamePlayOptions);
                    case "BMTRC.UpdateMinPing":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateMinPing(json.serverID, json.EnablePing, json.PingValue, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateMinPing = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateMinPing);
                    case "BMTRC.UpdateMaxPing":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateMaxPing(json.serverID, json.EnablePing, json.PingValue, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateMaxPing = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateMaxPing);
                    case "BMTRC.UpdateWeapons":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateWeapons(json.serverID, json.weapons, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonUpdateWeapons = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonUpdateWeapons);
                    case "BMTRC.AddVPN":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.AddVPN(json.serverID, json.Description, json.PlayerIP, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonAddVPN = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonAddVPN);
                    case "BMTRC.StartInstance":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.StartInstance(json.serverID, json.ServerName, json.MOTD, json.CountryCode, json.password, json.SessionType, json.MaxSlots, json.StartDelay, json.StartLoopMaps, json.MaxKills, json.GameScore, json.ZoneTimer, json.RespawnTime, json.TimeLimit, json.RequireNovaLogin, json.WindowedMode, json.AllowCustomSkins, json.Dedicated, json.BluePassword, json.RedPassword, json.FriendlyFire, json.FriendlyFireWarning, json.FriendlyTags, json.AutoBalance, json.ShowTracers, json.ShowTeamClays, json.AllowAutoRange, json.MinPing, json.MinPingValue, json.MaxPing, json.MaxPingValue, 0, json.StartList, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonStartInstance = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonStartInstance);
                    case "BMTRC.StopInstance":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.StopInstance(json.serverID, json.SessionID));
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }

                        var jsonStopInstance = JsonConvert.SerializeObject(reply);
                        return Encoding.ASCII.GetBytes(jsonStopInstance);
                    case "BMTRC.Logout":
                        reply.Add("Status", RCListenerClass.StatusCodes.LOGOUTSUCCESS);
                        var jsonready3 = JsonConvert.SerializeObject(reply);
                        _state.rcClients[json.SessionID].authenticated = false;
                        _state.rcClients[json.SessionID].active = false;
                        if (ProgramConfig.Debug)
                        {
                            log.Info(">> Client Number: " + clientNo + " has disconnected.");
                        }
                        return Encoding.ASCII.GetBytes(jsonready3);
                    case "BMTRC.GetInstances":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.SUCCESS);
                            reply.Add("Instances", Crypt.Base64Encode(JsonConvert.SerializeObject(RCFunctions.GetInstances(json.SessionID))));
                            reply.Add("ChatLogs", Crypt.Base64Encode(JsonConvert.SerializeObject(_state.ChatLogs)));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonprofilelist = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonprofilelist);
                    case "BMTRC.GetPlayerIPInfo":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.GetPlayerIPInfo(json.serverID, json.slot, out ipqualityClass ipqualityInfo));
                            reply.Add("PlayerInfo", JsonConvert.SerializeObject(ipqualityInfo));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonGetPlayerInfo = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonGetPlayerInfo);
                    case "BMTRC.GetPlayerHistory":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.GetPlayerHistory(json.serverID, json.slot, out List<playerHistory> history));
                            reply.Add("history", JsonConvert.SerializeObject(history));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonGetPlayerHistory = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonGetPlayerHistory);
                    case "BMTRC.GetAdminNotes":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.GetAdminNotes(json.serverID, json.slot, out List<adminnotes> adminNotes));
                            reply.Add("notes", JsonConvert.SerializeObject(adminNotes));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonGetAdminNotes = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonGetAdminNotes);
                    case "BMTRC.AddAdminNote":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.AddAdminNote(json.PlayerIP, json.PlayerName, json.newMsg, json.SessionID));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonAddAdminNote = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonAddAdminNote);
                    case "BMTRC.RemoveAdminNote":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.RemoveAdminNote(json.PlayerName, json.newMsg, json.SessionID));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonRemoveAdminNote = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonRemoveAdminNote);
                    case "BMTRC.GetAvalMaps":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            Dictionary<int, MapList> avalMaps = new Dictionary<int, MapList>();
                            reply.Add("Status", RCFunctions.ScanInstanceDirectory(json.serverID, out avalMaps));
                            reply.Add("maps", JsonConvert.SerializeObject(avalMaps));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonAvalMaps = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonAvalMaps);
                    case "BMTRC.UserManager.GetUsers":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            Dictionary<string, UserCodes> subUsers = new Dictionary<string, UserCodes>();
                            reply.Add("Status", RCFunctions.GetUsers(json.SessionID, out subUsers));
                            reply.Add("users", JsonConvert.SerializeObject(subUsers));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonsubUsers = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonsubUsers);
                    case "BMTRC.UserManager.CurrentConnections":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            List<RCLogs> logs = new List<RCLogs>();
                            reply.Add("Status", RCFunctions.GetCurrentConnections(json.SessionID, out logs));
                            reply.Add("currentConnections", JsonConvert.SerializeObject(logs));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonCurrentConnections = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonCurrentConnections);
                    case "BMTRC.UserManager.AddUser":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.AddUser(json.SessionID, json.username, json.password, json.superadmin, json.subadmin, json.userPermissions));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonAddUser = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonAddUser);
                    case "BMTRC.UserManager.DeleteUser":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.DeleteUser(json.SessionID, json.username));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonDeleteUser = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonDeleteUser);
                    case "BMTRC.UserManager.GetLogs":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            List<RCLogs> logs = new List<RCLogs>();
                            reply.Add("Status", RCFunctions.GetLogs(json.SessionID, out logs));
                            reply.Add("logs", JsonConvert.SerializeObject(logs));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonGetLogs = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonGetLogs);
                    case "BMTRC.RotationManager.DeleteRotation":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.DeleteRotation(json.serverID, json.RotationID));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonDeleteRotation = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonDeleteRotation);
                    case "BMTRC.RotationManager.CreateRotation":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.CreateRotation(json.serverID, json.Rotation, json.description));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonCreateRotation = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonCreateRotation);
                    case "BMTRC.RotationManager.UpdateRotation":
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCFunctions.UpdateRotation(json.serverID, json.Rotation, json.description, json.RotationID));
                            _state.rcClients[json.SessionID].expires = DateTime.Now.AddHours(1).ToString();
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonUpdateRotation = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonUpdateRotation);
                    default:
                        if (_state.rcClients[json.SessionID].authenticated == true)
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.INVALIDCOMMAND);
                        }
                        else
                        {
                            reply.Add("Status", RCListenerClass.StatusCodes.NOTAUTHENTICATED);
                        }
                        var jsonDefault = JsonConvert.SerializeObject(reply);
                        return ProgramConfig.Encoder.GetBytes(jsonDefault);
                }
            }
            catch
            {
                return new byte[0];
            }
        }
    }
}
