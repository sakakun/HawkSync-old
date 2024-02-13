using System;
using System.Collections.Generic;
using System.Text;
using HawkSync_RC.classes;
using HawkSync_RC.classes.RCClasses;
using Newtonsoft.Json;

namespace HawkSync_RC.TVFunctions
{
    public class TVUserManager
    {
        private readonly RCSetup _setup;
        private AppState _state;

        public TVUserManager(AppState state, RCSetup setup)
        {
            _state = state;
            _setup = setup;
        }

        public OpenClass.Status GetUsers(out Dictionary<string, UserCodes> users)
        {
            users = new Dictionary<string, UserCodes>();
            var sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.UserManager.GetUsers" },
                { "SessionID", _setup.SessionID }
            };
            var sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            var bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            var reply = _setup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            var responseArray =
                JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                    Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            users = JsonConvert.DeserializeObject<Dictionary<string, UserCodes>>(responseArray["users"]);
            return (OpenClass.Status)responseArray["Status"];
        }

        public OpenClass.Status AddUser(string username, string password, bool superadmin, int subadmin,
            Permissions permissions)
        {
            var request = new Dictionary<string, dynamic>
            {
                { "SessionID", _setup.SessionID },
                { "action", "BMTRC.UserManager.AddUser" },
                { "username", username },
                { "password", password },
                { "superadmin", superadmin },
                { "subadmin", subadmin },
                { "userPermissions", permissions }
            };
            var responseBytes = _setup.SendCMD(request);
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.Default.GetString(responseBytes));
            return (OpenClass.Status)response["Status"];
        }

        public OpenClass.Status DeleteUser(string username)
        {
            var request = new Dictionary<string, dynamic>
            {
                { "SessionID", _setup.SessionID },
                { "action", "BMTRC.UserManager.DeleteUser" },
                { "username", username }
            };
            var responseBytes = _setup.SendCMD(request);
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.Default.GetString(responseBytes));
            return (OpenClass.Status)response["Status"];
        }

        public OpenClass.Status GetLogs(out List<RCLogs> logs)
        {
            logs = new List<RCLogs>();
            var sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.UserManager.GetLogs" },
                { "SessionID", _setup.SessionID }
            };
            var sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            var bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            var reply = _setup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            var responseArray =
                JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                    Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            logs = JsonConvert.DeserializeObject<List<RCLogs>>(responseArray["logs"]);
            return (OpenClass.Status)responseArray["Status"];
        }

        public OpenClass.Status GetCurrentConnections(out List<RCLogs> currentConnections)
        {
            currentConnections = new List<RCLogs>();
            var sendCmdArray = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.UserManager.CurrentConnections" },
                { "SessionID", _setup.SessionID }
            };
            var sendCmdString = JsonConvert.SerializeObject(sendCmdArray);
            var bytes = Compression.Compress(Encoding.Default.GetBytes(sendCmdString));

            var reply = _setup.client.SendAndWait(ProgramConfig.timeOut, bytes);
            var responseArray =
                JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                    Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
            currentConnections = JsonConvert.DeserializeObject<List<RCLogs>>(responseArray["currentConnections"]);
            return (OpenClass.Status)responseArray["Status"];
        }

        internal OpenClass.Status DisconnectUser(string username, string ipPort)
        {
            throw new NotImplementedException();
        }
    }
}