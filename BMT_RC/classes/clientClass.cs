using HawkSync_RC.classes.RCClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using WatsonTcp;

namespace HawkSync_RC.classes
{
    public class clientClass
    {
        Dictionary<dynamic, dynamic> json_array = new Dictionary<dynamic, dynamic>();
        public WatsonTcpClient Connect(IPAddress Address, int Port)
        {
            WatsonTcpClient client = new WatsonTcpClient(Address.ToString(), Port);
            client.Events.MessageReceived += Events_MessageReceived;
            try
            {
                client.Connect();
            }
            catch
            {

            }
            return client;
        }

        private void Events_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            return;
        }

        public string Open(WatsonTcpClient client)
        {
            Assembly RCExeInfo = Assembly.GetExecutingAssembly();
            FileVersionInfo ExeInfo = FileVersionInfo.GetVersionInfo(RCExeInfo.Location);
            json_array.Add("action", "BMTRC.Open");
            json_array.Add("Version", ExeInfo.ProductVersion);
            var json = JsonConvert.SerializeObject(json_array);
            try
            {
                byte[] jsonBytes = Compression.Compress(Encoding.Default.GetBytes(json));
                SyncResponse reply = client.SendAndWait(ProgramConfig.timeOut, jsonBytes);
                byte[] bytes = Compression.Decompress(reply.Data);
                return Encoding.ASCII.GetString(bytes);
            }
            catch (TimeoutException)
            {
                Dictionary<string, dynamic> timeoutResponse = new Dictionary<string, dynamic>
                {
                    { "status", false },
                    { "LoginMessage", OpenClass.Status.TIMEOUT },
                    { "SessionID", string.Empty },
                };
                return JsonConvert.SerializeObject(timeoutResponse);
            }
        }
        public OpenClass.Status Login(WatsonTcpClient client, string sessionID = "", string username = "", string password = "")
        {
            json_array.Clear();
            json_array.Add("action", "BMTRC.Login");
            json_array.Add("SessionID", sessionID);
            json_array.Add("username", username);
            json_array.Add("password", password);
            var json = JsonConvert.SerializeObject(json_array);
            try
            {
                byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(json));
                SyncResponse reply = client.SendAndWait(ProgramConfig.timeOut, bytes);
                Dictionary<dynamic, dynamic> TVReply = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Encoding.ASCII.GetString(Compression.Decompress(reply.Data)));
                return (OpenClass.Status)TVReply["Status"];
            }
            catch (TimeoutException)
            {
                return OpenClass.Status.TIMEOUT;
            }
        }
        public string GetUserPermissions(WatsonTcpClient client, string SessionID = "")
        {
            json_array.Clear();
            json_array.Add("action", "BMTRC.GetUserPermissions");
            json_array.Add("SessionID", SessionID);

            var json = JsonConvert.SerializeObject(json_array);
            byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(json));
            SyncResponse reply = client.SendAndWait(ProgramConfig.timeOut, bytes);

            return Encoding.ASCII.GetString(Compression.Decompress(reply.Data));
        }
        public string GetInstances(WatsonTcpClient client, string SessionID = "")
        {
            json_array.Clear();
            json_array.Add("action", "BMTRC.GetInstances");
            json_array.Add("SessionID", SessionID);
            var json = JsonConvert.SerializeObject(json_array);
            try
            {
                byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(json));
                SyncResponse reply = client.SendAndWait(ProgramConfig.timeOut, bytes);
                if (reply == null)
                {
                    return string.Empty;
                }

                return Encoding.ASCII.GetString(Compression.Decompress(reply.Data));
            }
            catch (TimeoutException)
            {
                return String.Empty;
            }
        }
        public string GetFTPPort(WatsonTcpClient client, string SessionID = "")
        {
            json_array.Clear();
            json_array.Add("action", "BMTRC.GetFTPPort");
            json_array.Add("SessionID", SessionID);
            var json = JsonConvert.SerializeObject(json_array);
            try
            {
                byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(json));
                SyncResponse reply = client.SendAndWait(ProgramConfig.timeOut, bytes);
                if (reply == null)
                {
                    return string.Empty;
                }

                return Encoding.ASCII.GetString(Compression.Decompress(reply.Data));
            }
            catch (TimeoutException)
            {
                return String.Empty;
            }
        }
        public OpenClass.Status Logout(WatsonTcpClient client, string SessionID = "")
        {
            json_array.Clear();
            json_array.Add("action", "BMTRC.Logout");
            json_array.Add("SessionID", SessionID);
            var json = JsonConvert.SerializeObject(json_array);
            client.Send(Compression.Compress(Encoding.Default.GetBytes(json)));
            return OpenClass.Status.SUCCESS;
        }

        public string GetCountryCodes(WatsonTcpClient client, string SessionID)
        {
            json_array.Clear();
            json_array.Add("action", "BMTRC.GetCountryCodes");
            json_array.Add("SessionID", SessionID);
            var json = JsonConvert.SerializeObject(json_array);
            byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(json));
            SyncResponse reply = client.SendAndWait(ProgramConfig.timeOut, bytes);
            return Encoding.ASCII.GetString(Compression.Decompress(reply.Data));
        }
        public string GetAutoRes(WatsonTcpClient client, string SessionID)
        {
            json_array.Clear();
            json_array.Add("action", "BMTRC.GetAutoRes");
            json_array.Add("SessionID", SessionID);
            var json = JsonConvert.SerializeObject(json_array);
            byte[] bytes = Compression.Compress(Encoding.Default.GetBytes(json));
            SyncResponse reply = client.SendAndWait(ProgramConfig.timeOut, bytes);
            return Encoding.ASCII.GetString(Compression.Decompress(reply.Data));
        }
    }
}
