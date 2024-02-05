using HawkSync_SM.classes.logs;
using Newtonsoft.Json;
using System;
using System.Data.SQLite;

namespace HawkSync_SM.RCClasses
{
    public class RCLoginFunctions
    {
        AppState _state;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public RCLoginFunctions(AppState state)
        {
            _state = state;
        }
        public RCListenerClass.StatusCodes Login(string sessionid, string username, string password)
        {
            if (_state.rcClients.ContainsKey(sessionid) && (username == string.Empty || password == string.Empty))
            {
                return RCListenerClass.StatusCodes.INVALIDLOGIN;
            }
            else if (username == string.Empty || password == string.Empty)
            {
                return RCListenerClass.StatusCodes.INVALIDLOGIN;
            }
            else
            {
                bool userFound = false;
                if (_state.Users.ContainsKey(username))
                {
                    if (_state.Users[username].Password == password)
                    {
                        if (_state.Users[username].SuperAdmin == true)
                        {
                            userFound = true;
                        }
                        else if (_state.Users[username].Permissions.RemoteAdmin == true)
                        {
                            userFound = true;
                        }
                        else
                        {
                            userFound = false;
                        }
                    }
                }
                if (userFound == false)
                {
                    return RCListenerClass.StatusCodes.INVALIDLOGIN;
                }
                else
                {
                    if (!_state.rcClients.ContainsKey(sessionid))
                    {
                        return RCListenerClass.StatusCodes.INVALIDSESSION;
                    }
                    else
                    {
                        _state.rcClients[sessionid].SetSessionUsername(username);
                        _state.rcClients[sessionid].active = true;
                        _state.rcClients[sessionid].authenticated = true;
                        if (ProgramConfig.ApplicationDebug)
                        {
                            log.Info(">> Client Number: " + _state.rcClients[sessionid].clientNo + " has authenicated as: " + _state.rcClients[sessionid]._username);
                        }
                        SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                        db.Open();
                        SQLiteCommand firstNumChk = new SQLiteCommand("SELECT COUNT(`id`) FROM `rclogs`;", db);
                        int firstNumChkID = Convert.ToInt32(firstNumChk.ExecuteScalar());
                        int newID;
                        firstNumChk.Dispose();
                        if (firstNumChkID == 0)
                        {
                            newID = 1;
                        }
                        else
                        {
                            SQLiteCommand numCmd = new SQLiteCommand("SELECT `id` FROM `rclogs` ORDER BY `id` DESC;", db);
                            newID = Convert.ToInt32(numCmd.ExecuteScalar()) + 1;
                            numCmd.Dispose();
                        }

                        SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (@id, @sessionid, @username, @action, @address, @date);", db);
                        newEntryCmd.Parameters.AddWithValue("@id", newID);
                        newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
                        newEntryCmd.Parameters.AddWithValue("@username", username);
                        newEntryCmd.Parameters.AddWithValue("@action", "Logged In");
                        newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
                        newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        newEntryCmd.ExecuteNonQuery();
                        newEntryCmd.Dispose();
                        db.Close();
                        db.Dispose();

                        _state.RCLogs.Add(new RCLogs
                        {
                            Action = "Logged In",
                            Address = _state.rcClients[sessionid].RemoteAddress,
                            Date = DateTime.Now,
                            SessionID = sessionid,
                            Username = _state.rcClients[sessionid]._username
                        });
                        _state.rcClients[sessionid].expires = DateTime.Now.AddHours(1).ToString();

                        return RCListenerClass.StatusCodes.LOGINSUCCESS;
                    }
                }
            }
        }
        public UserCodes GetUserPermissions(string sessionid)
        {
            /*SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand firstNumChk = new SQLiteCommand("SELECT COUNT(`id`) FROM `rclogs`;", db);
            int firstNumChkID = Convert.ToInt32(firstNumChk.ExecuteScalar());
            int newID;
            firstNumChk.Dispose();
            if (firstNumChkID == 0)
            {
                newID = 1;
            }
            else
            {
                SQLiteCommand numCmd = new SQLiteCommand("SELECT `id` FROM `rclogs` ORDER BY `id` DESC;", db);
                newID = Convert.ToInt32(numCmd.ExecuteScalar()) + 1;
                numCmd.Dispose();
            }

            SQLiteCommand newEntryCmd = new SQLiteCommand("INSERT INTO `rclogs` (`id`, `sessionid`, `username`, `action`, `address`, `date`) VALUES (@id, @sessionid, @username, @action, @address, @date);", db);
            newEntryCmd.Parameters.AddWithValue("@id", newID);
            newEntryCmd.Parameters.AddWithValue("@sessionid", sessionid);
            newEntryCmd.Parameters.AddWithValue("@username", _state.rcClients[sessionid]._username);
            newEntryCmd.Parameters.AddWithValue("@action", "GetUserPermissions");
            newEntryCmd.Parameters.AddWithValue("@address", _state.rcClients[sessionid].RemoteAddress.ToString());
            newEntryCmd.Parameters.AddWithValue("@date", DateTime.Now);
            newEntryCmd.ExecuteNonQuery();
            newEntryCmd.Dispose();
            db.Close();
            db.Dispose();

            _state.RCLogs.Add(new RCLogs
            {
                Action = "GetUserPermissions",
                Address = _state.rcClients[sessionid].RemoteAddress,
                Date = DateTime.Now,
                SessionID = sessionid,
                Username = _state.rcClients[sessionid]._username
            });*/

            _state.rcClients[sessionid].expires = DateTime.Now.AddHours(1).ToString();
            return _state.Users[_state.rcClients[sessionid]._username];
        }

        public RCListenerClass.StatusCodes GetAutoRes(ref string autoRes)
        {
            autoRes = Crypt.Base64Encode(JsonConvert.SerializeObject(_state.autoRes));
            return RCListenerClass.StatusCodes.SUCCESS;
        }

        public RCListenerClass.StatusCodes GetFTPPort(ref int ftpPort)
        {
            ftpPort = ProgramConfig.RCFTPPort;
            return RCListenerClass.StatusCodes.SUCCESS;
        }
    }
}
