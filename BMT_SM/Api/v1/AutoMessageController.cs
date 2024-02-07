using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace HawkSync_SM.Api.v1
{
    public class AutoMessagesController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage MoveMsg(string direction, int instanceID, int id)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            if (!GlobalAppState.AppState.Instances.ContainsKey(instanceID))
            {
                Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                {
                    { "success", false },
                    { "error", "Either the Profile does not exist, or has been been setup yet." }
                };
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            else
            {
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                if (direction == "MoveUp")
                {
                    int IntDirection = -1;
                    if (GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.ElementAtOrDefault(id) != null)
                    {
                        int SelectedItem = id;
                        int newIndex = id + IntDirection;

                        // setup array bounds so we don't fuck up again...
                        if (newIndex < 0 || newIndex >= GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.Count)
                        {
                            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                            {
                                { "success", false },
                                { "error", "Message cannot be moved any higher than it's current position!" }
                            };
                            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                        }
                        else
                        {
                            // remove item then re-add at the correct position
                            string selected = GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages[id];
                            GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.Remove(selected);
                            GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.Insert(newIndex, selected);
                            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                            {
                                { "success", true },
                                { "error", "Message Moved Up." }
                            };
                            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @autoMessages WHERE `profile_id` = @profileid;", db); // update database with new changes
                            cmd.Parameters.AddWithValue("@autoMessages", JsonConvert.SerializeObject(GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages));
                            cmd.Parameters.AddWithValue("@profileid", instanceID);
                            cmd.ExecuteNonQuery(); // update database
                            cmd.Dispose();
                            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                        }
                    }
                    else
                    {
                        Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                        {
                            { "success", false },
                            { "error", "Invalid MessageID!" }
                        };
                        Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                    }
                }
                else if (direction == "MoveDown")
                {
                    int IntDirection = 1;
                    if (GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.ElementAtOrDefault(id) != null)
                    {
                        int SelectedItem = id;
                        int newIndex = id + IntDirection;

                        // setup array bounds so we don't fuck up again...
                        if (newIndex < 0 || newIndex >= GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.Count)
                        {
                            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                            {
                                { "success", false },
                                { "error", "Message cannot be moved any lower than it's current position!" }
                            };
                            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                        }
                        else
                        {
                            // remove item then re-add at the correct position
                            string selected = GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages[id];
                            GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.Remove(selected);
                            GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages.Insert(newIndex, selected);
                            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                            {
                                { "success", true },
                                { "error", "Message Moved Down." }
                            };
                            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @autoMessages WHERE `profile_id` = @profileid;", db); // update database with new changes
                            cmd.Parameters.AddWithValue("@autoMessages", JsonConvert.SerializeObject(GlobalAppState.AppState.Instances[instanceID].AutoMessages.messages));
                            cmd.Parameters.AddWithValue("@profileid", instanceID);
                            cmd.ExecuteNonQuery(); // update database
                            cmd.Dispose();
                            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                        }
                    }
                    else
                    {
                        Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                        {
                            { "success", false },
                            { "error", "Invalid MessageID!" }
                        };
                        Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                    }
                }
                else
                {
                    Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                    {
                        { "success", false },
                        { "error", "Invalid Direction!" }
                    };
                    Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                }
                db.Close(); // close DB to prevent locking
                db.Dispose(); // prevent memory locking
            }
            return Response;
        }
        [HttpGet]
        public HttpResponseMessage GetMessages(int id)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            if (!GlobalAppState.AppState.Instances.ContainsKey(id))
            {
                Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                {
                    { "success", false },
                    { "error", "Either the Profile does not exist, or has been been setup yet." }
                };
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            else
            {
                ob_AutoMessages messages = GlobalAppState.AppState.Instances[id].AutoMessages;
                Response.Content = new StringContent(JsonConvert.SerializeObject(messages), Encoding.Default, "application/json");
            }
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage AddMsg(int instanceid, string msg)
        {

            var Response = Request.CreateResponse(HttpStatusCode.OK);
            if (!GlobalAppState.AppState.Instances.ContainsKey(instanceid))
            {
                Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                {
                    { "success", false },
                    { "error", "Either the Profile does not exist, or has been been setup yet." }
                };
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            else
            {
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                GlobalAppState.AppState.Instances[instanceid].AutoMessages.messages.Add(msg);
                Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                {
                    { "success", true },
                    { "error", "Message added!" }
                };
                SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @autoMessages WHERE `profile_id` = @profileid;", db); // update database with new changes
                cmd.Parameters.AddWithValue("@autoMessages", JsonConvert.SerializeObject(GlobalAppState.AppState.Instances[instanceid].AutoMessages.messages));
                cmd.Parameters.AddWithValue("@profileid", instanceid);
                cmd.ExecuteNonQuery(); // update database
                cmd.Dispose();
                db.Close();
                db.Dispose();
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage RemoveMsg(int instanceid, int id)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            if (!GlobalAppState.AppState.Instances.ContainsKey(instanceid))
            {
                Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                {
                    { "success", false },
                    { "error", "Either the Profile does not exist, or has been been setup yet." }
                };
                Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            }
            else
            {
                if (GlobalAppState.AppState.Instances[instanceid].AutoMessages.messages.ElementAtOrDefault(id) == null)
                {
                    Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                    {
                        { "success", false },
                        { "error", "Invalid MessageID" }
                    };
                    Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                }
                else
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    GlobalAppState.AppState.Instances[instanceid].AutoMessages.messages.Remove(GlobalAppState.AppState.Instances[instanceid].AutoMessages.messages[id]);
                    SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_messages` = @automessages WHERE `profile_id` = @profileid;", db);
                    cmd.Parameters.AddWithValue("@automessages", JsonConvert.SerializeObject(GlobalAppState.AppState.Instances[instanceid].AutoMessages.messages));
                    cmd.Parameters.AddWithValue("@profileid", instanceid);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
                    {
                        { "success", true },
                        { "error", "Message Removed!" }
                    };

                    db.Close();
                    db.Dispose();
                    Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
                }
            }
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage UpdateInterval(int instanceid, int newInt)
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            GlobalAppState.AppState.Instances[instanceid].AutoMessages.interval = newInt;
            SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_config` SET `auto_msg_interval` = @newInt WHERE `profile_id` = @profileid;", db);
            cmd.Parameters.AddWithValue("@newInt", newInt);
            cmd.Parameters.AddWithValue("@profileid", instanceid);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
            {
                { "success", true },
                { "error", "Interval changes successfully!" }
            };
            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");


            db.Close();
            db.Dispose();
            return Response;
        }
    }
}
